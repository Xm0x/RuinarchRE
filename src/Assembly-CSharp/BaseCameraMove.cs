using DG.Tweening;
using Inner_Maps;
using Ruinarch;
using Settings;
using UnityEngine;

public abstract class BaseCameraMove : BaseMonoBehaviour
{
	public Camera camera;

	[Header("Bounds")]
	private const float MIN_Z = -10f;

	private const float MAX_Z = -10f;

	[SerializeField]
	protected float MIN_X;

	[SerializeField]
	protected float MAX_X;

	[SerializeField]
	protected float MIN_Y;

	[SerializeField]
	protected float MAX_Y;

	[SerializeField]
	protected float _minFov;

	[SerializeField]
	protected float _maxFov;

	[Header("Panning")]
	[SerializeField]
	private float baseCameraPanSpeed = 26f;

	[Header("Dragging")]
	private float dragThreshold = 0.05f;

	private float currDragTime;

	private Vector3 dragOrigin;

	public bool isDragging;

	private bool startedOnUI;

	private bool hasReachedThreshold;

	private Vector3 originMousePos;

	[Header("Edging")]
	[SerializeField]
	private int edgeBoundary = 30;

	[SerializeField]
	private float edgingSpeed;

	[SerializeField]
	private bool allowEdgePanning;

	[Header("Targeting")]
	[SerializeField]
	private float dampTime = 0.2f;

	[SerializeField]
	private Vector3 velocity = Vector3.zero;

	[SerializeField]
	private Transform _target;

	[SerializeField]
	private Vector3 _targetPos;

	[SerializeField]
	private bool isUsingVectorTarget;

	[Header("Threat")]
	[SerializeField]
	protected ThreatParticleEffect threatEffect;

	[Header("Zooming")]
	[SerializeField]
	private float zoomSensitivity;

	[SerializeField]
	private bool allowZoom = true;

	private TweenCallback _zoomTweenCallback;

	private Camera _zoomCamera;

	private float _zoomAdjustment;

	private Rect _screenRect;

	protected bool isMovementDisabled;

	public bool allowSmoothCameraFollow;

	public float smoothFollowSpeed;

	public float cameraPanSpeed => baseCameraPanSpeed + (float)SettingsManager.Instance.settings.cameraPanSpeed;

	public Transform target
	{
		get
		{
			return _target;
		}
		protected set
		{
			_target = value;
			if (_target == null)
			{
				Messenger.RemoveListener<GameObject>(ObjectPoolSignals.POOLED_OBJECT_DESTROYED, OnPooledObjectDestroyed);
			}
			else
			{
				Messenger.AddListener<GameObject>(ObjectPoolSignals.POOLED_OBJECT_DESTROYED, OnPooledObjectDestroyed);
			}
		}
	}

	public virtual void Initialize()
	{
		_screenRect = new Rect(0f, 0f, Screen.width, Screen.height);
		AllowEdgePanning(SettingsManager.Instance.settings.useEdgePanning);
		Messenger.AddListener<bool>(SettingsSignals.EDGE_PANNING_TOGGLED, AllowEdgePanning);
		Messenger.AddListener<SHORTCUT_ACTION>(ControlsSignals.PLAYER_INPUT_ACTION, OnReceivePlayerInputAction);
	}

	protected virtual void Awake()
	{
		_zoomTweenCallback = OnZoom;
	}

	private void LateUpdate()
	{
		Dragging(camera);
		Edging();
		Targeting(camera);
		ConstrainCameraBounds();
	}

	private void AllowEdgePanning(bool state)
	{
		allowEdgePanning = state;
		edgingSpeed = cameraPanSpeed;
	}

	private void OnReceivePlayerInputAction(SHORTCUT_ACTION p_action)
	{
		switch (p_action)
		{
		case SHORTCUT_ACTION.Pan_Left:
		case SHORTCUT_ACTION.Pan_Right:
		case SHORTCUT_ACTION.Pan_Up:
		case SHORTCUT_ACTION.Pan_Down:
			ClearOutCameraTargets();
			PanMovement(p_action);
			break;
		case SHORTCUT_ACTION.Zoom_In:
		case SHORTCUT_ACTION.Zoom_Out:
			ShortcutKeyZooming(p_action);
			break;
		}
	}

	private bool CanMoveCamera()
	{
		if (SaveManager.Instance.saveCurrentProgressManager.isSaving)
		{
			return false;
		}
		if (LevelLoaderManager.Instance.isLoadingNewScene || LevelLoaderManager.Instance.IsLoadingScreenActive())
		{
			return false;
		}
		if (PlayerUI.Instance != null && PlayerUI.Instance.IsMajorUIShowing())
		{
			return false;
		}
		if (UIManager.Instance != null && UIManager.Instance.IsObjectPickerOpen())
		{
			return false;
		}
		if (UIManager.Instance.IsConsoleShowing())
		{
			return false;
		}
		return !isMovementDisabled;
	}

	private void PanMovement(SHORTCUT_ACTION p_action)
	{
		if (!CanMoveCamera() || InputManager.Instance.HasSelectedUIObject())
		{
			return;
		}
		if (p_action == SHORTCUT_ACTION.Pan_Up || p_action == SHORTCUT_ACTION.Pan_Down)
		{
			float num = ((p_action == SHORTCUT_ACTION.Pan_Up) ? 1f : (-1f));
			Vector3 vector = new Vector3(0f, num * Time.deltaTime * cameraPanSpeed, 0f);
			Vector3 p_pos = base.transform.position + vector;
			if (IsInsideCameraBounds(p_pos))
			{
				_ = Vector3.zero;
				if (allowSmoothCameraFollow)
				{
					base.transform.DOBlendableMoveBy(vector, smoothFollowSpeed);
				}
				else
				{
					base.transform.Translate(vector);
				}
			}
		}
		if (p_action != SHORTCUT_ACTION.Pan_Left && p_action != SHORTCUT_ACTION.Pan_Right)
		{
			return;
		}
		float num2 = ((p_action == SHORTCUT_ACTION.Pan_Right) ? 1f : (-1f));
		Vector3 vector2 = new Vector3(num2 * Time.deltaTime * cameraPanSpeed, 0f, 0f);
		Vector3 p_pos2 = base.transform.position + vector2;
		if (IsInsideCameraBounds(p_pos2))
		{
			_ = Vector3.zero;
			if (allowSmoothCameraFollow)
			{
				base.transform.DOBlendableMoveBy(vector2, smoothFollowSpeed);
			}
			else
			{
				base.transform.Translate(vector2);
			}
		}
	}

	private void Dragging(Camera targetCamera)
	{
		if (!CanMoveCamera())
		{
			return;
		}
		if (startedOnUI)
		{
			if (!InputManager.Instance.GetMouseButton(2))
			{
				ResetDragValues();
			}
			return;
		}
		if (!isDragging)
		{
			if (InputManager.Instance.GetMouseButtonDown(2))
			{
				if (UIManager.Instance.IsMouseOnUI())
				{
					startedOnUI = true;
					return;
				}
			}
			else if (InputManager.Instance.GetMouseButton(2))
			{
				currDragTime += Time.deltaTime;
				if (currDragTime >= dragThreshold)
				{
					if (!hasReachedThreshold)
					{
						dragOrigin = targetCamera.ScreenToWorldPoint(InputManager.Instance.mousePosition);
						originMousePos = InputManager.Instance.mousePosition;
						hasReachedThreshold = true;
					}
					if (originMousePos != InputManager.Instance.mousePosition)
					{
						if (PlayerManager.Instance.player == null || !PlayerManager.Instance.player.seizeComponent.hasSeizedPOI)
						{
							InputManager.Instance.SetCursorTo(Cursor_Type.Drag_Clicked);
						}
						isDragging = true;
					}
				}
			}
		}
		if (isDragging)
		{
			Vector3 vector = targetCamera.ScreenToWorldPoint(InputManager.Instance.mousePosition) - targetCamera.transform.position;
			targetCamera.transform.position = dragOrigin - vector;
			if (InputManager.Instance.GetMouseButtonUp(2))
			{
				ResetDragValues();
				if (InputManager.Instance.currentCursorType == Cursor_Type.Drag_Clicked)
				{
					InputManager.Instance.SetCursorTo(Cursor_Type.Default);
				}
			}
			else if (InputManager.Instance.currentCursorType == Cursor_Type.Default)
			{
				InputManager.Instance.SetCursorTo(Cursor_Type.Drag_Clicked);
			}
		}
		else if (!InputManager.Instance.GetMouseButton(2))
		{
			currDragTime = 0f;
			hasReachedThreshold = false;
		}
	}

	private void ResetDragValues()
	{
		currDragTime = 0f;
		isDragging = false;
		startedOnUI = false;
		hasReachedThreshold = false;
	}

	public void DisableMovement()
	{
		isMovementDisabled = true;
	}

	public void EnableMovement()
	{
		isMovementDisabled = false;
	}

	private void Edging()
	{
		if (allowEdgePanning && !isDragging)
		{
			bool flag = false;
			Vector3 position = base.transform.position;
			if (InputManager.Instance.mousePosition.x >= (float)Screen.width - ((float)edgeBoundary + 5f))
			{
				position.x += edgingSpeed * Time.deltaTime;
				flag = true;
			}
			if (InputManager.Instance.mousePosition.x <= (float)edgeBoundary)
			{
				position.x -= edgingSpeed * Time.deltaTime;
				flag = true;
			}
			if (InputManager.Instance.mousePosition.y >= (float)(Screen.height - edgeBoundary))
			{
				position.y += edgingSpeed * Time.deltaTime;
				flag = true;
			}
			if (InputManager.Instance.mousePosition.y <= (float)edgeBoundary)
			{
				position.y -= edgingSpeed * Time.deltaTime;
				flag = true;
			}
			if (flag)
			{
				ClearOutCameraTargets();
			}
			if (allowSmoothCameraFollow)
			{
				Vector3 byValue = position - base.transform.position;
				base.transform.DOBlendableMoveBy(byValue, smoothFollowSpeed);
			}
			else
			{
				base.transform.position = position;
			}
		}
	}

	public void CenterCameraOn(GameObject GO, bool instantCenter = false)
	{
		isUsingVectorTarget = false;
		if ((object)GO == null)
		{
			target = null;
			return;
		}
		if (instantCenter)
		{
			MoveCamera(GO.transform.position);
		}
		target = GO.transform;
	}

	public void CenterCameraOn(Vector3 pos)
	{
		_targetPos = pos;
		isUsingVectorTarget = true;
		target = null;
	}

	public void ClearOutCameraTargets()
	{
		target = null;
		isUsingVectorTarget = false;
	}

	private void Targeting(Camera camera)
	{
		if (isDragging)
		{
			ClearOutCameraTargets();
		}
		if ((bool)target)
		{
			Vector3 position = target.position;
			Vector3 position2 = base.transform.position;
			Vector3 vector = position - camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, camera.WorldToViewportPoint(position).z));
			Vector3 vector2 = position2 + vector;
			base.transform.position = Vector3.SmoothDamp(position2, vector2, ref velocity, dampTime);
			if (HasReachedBounds() || (Mathf.Approximately(base.transform.position.x, vector2.x) && Mathf.Approximately(base.transform.position.y, vector2.y)))
			{
				target = null;
			}
		}
		else if (isUsingVectorTarget)
		{
			Vector3 targetPos = _targetPos;
			Vector3 position3 = base.transform.position;
			Vector3 vector3 = targetPos - camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, camera.WorldToViewportPoint(targetPos).z));
			Vector3 vector4 = position3 + vector3;
			base.transform.position = Vector3.SmoothDamp(position3, vector4, ref velocity, dampTime);
			if (HasReachedBounds() || (Mathf.Approximately(base.transform.position.x, vector4.x) && Mathf.Approximately(base.transform.position.y, vector4.y)))
			{
				isUsingVectorTarget = false;
			}
		}
	}

	private void OnPooledObjectDestroyed(GameObject obj)
	{
		if (target == obj.transform)
		{
			target = null;
		}
	}

	protected void ConstrainCameraBounds()
	{
		base.transform.position = GetConstrainedCameraBounds(base.transform.position);
	}

	protected Vector3 GetConstrainedCameraBounds(Vector3 p_pos)
	{
		float min = MIN_X;
		float max = MAX_X;
		float min2 = MIN_Y;
		float max2 = MAX_Y;
		if (MAX_X < MIN_X)
		{
			min = MAX_X;
			max = MIN_X;
		}
		if (MAX_Y < MIN_Y)
		{
			min2 = MAX_Y;
			max2 = MIN_Y;
		}
		float x = Mathf.Clamp(p_pos.x, min, max);
		float y = Mathf.Clamp(p_pos.y, min2, max2);
		float z = Mathf.Clamp(p_pos.z, -10f, -10f);
		return new Vector3(x, y, z);
	}

	protected bool IsInsideCameraBounds(Vector3 p_pos)
	{
		float num = MIN_X;
		float num2 = MAX_X;
		float num3 = MIN_Y;
		float num4 = MAX_Y;
		if (MAX_X < MIN_X)
		{
			num = MAX_X;
			num2 = MIN_X;
		}
		if (MAX_Y < MIN_Y)
		{
			num3 = MAX_Y;
			num4 = MIN_Y;
		}
		if (p_pos.x >= num && p_pos.x <= num2 && p_pos.y >= num3 && p_pos.y <= num4)
		{
			return true;
		}
		return false;
	}

	private bool HasReachedBounds()
	{
		if ((Mathf.Approximately(base.transform.position.x, MAX_X) || Mathf.Approximately(base.transform.position.x, MIN_X)) && (Mathf.Approximately(base.transform.position.y, MAX_Y) || Mathf.Approximately(base.transform.position.y, MIN_Y)))
		{
			return true;
		}
		return false;
	}

	public void ScrollWheelZooming(float p_axis)
	{
		if (allowZoom && CanMoveCamera() && !InputManager.Instance.HasSelectedUIObject() && _screenRect.Contains(InputManager.Instance.mousePosition))
		{
			float orthographicSize = camera.orthographicSize;
			float num = p_axis * zoomSensitivity;
			if (num != 0f && !UIManager.Instance.IsMouseOnUI())
			{
				orthographicSize -= num;
				orthographicSize = Mathf.Clamp(orthographicSize, _minFov, _maxFov);
				_zoomCamera = camera;
				_zoomAdjustment = num;
				camera.DOOrthoSize(orthographicSize, 0.5f).OnUpdate(_zoomTweenCallback);
			}
		}
	}

	private void ShortcutKeyZooming(SHORTCUT_ACTION p_action)
	{
		if (allowZoom && CanMoveCamera() && !InputManager.Instance.HasSelectedUIObject())
		{
			float num = -0.1f;
			if (p_action == SHORTCUT_ACTION.Zoom_In)
			{
				num = 0.1f;
			}
			float orthographicSize = camera.orthographicSize;
			float num2 = num * zoomSensitivity;
			if (num2 != 0f)
			{
				orthographicSize -= num2;
				orthographicSize = Mathf.Clamp(orthographicSize, _minFov, _maxFov);
				_zoomCamera = camera;
				_zoomAdjustment = num2;
				camera.DOOrthoSize(orthographicSize, 0.5f).OnUpdate(_zoomTweenCallback);
			}
		}
	}

	public void SetZoom(float p_zoom)
	{
		float value = p_zoom;
		value = Mathf.Clamp(value, _minFov, _maxFov);
		_zoomCamera = camera;
		_zoomAdjustment = p_zoom;
		camera.DOOrthoSize(value, 0.5f).OnUpdate(_zoomTweenCallback);
	}

	private void OnZoom()
	{
		if (threatEffect != null)
		{
			threatEffect.OnZoomCamera(camera);
		}
		Messenger.Broadcast(ControlsSignals.CAMERA_ZOOM_CHANGED, _zoomCamera, _zoomAdjustment);
	}

	public void MoveCamera(Vector3 newPos)
	{
		base.transform.position = newPos;
	}

	public void MoveCameraForMinimap(Vector3 newPos)
	{
		base.transform.position = newPos;
		ConstrainCameraBounds();
	}

	public void JustCenterCamera(bool instantCenter)
	{
		if (instantCenter)
		{
			Vector3 newPos = new Vector3((MIN_X + MAX_X) * 0.5f, (MIN_Y + MAX_Y) * 0.5f);
			MoveCamera(newPos);
		}
		else
		{
			InnerMapManager.Instance.currentlyShowingMap.centerGo.transform.position = new Vector3((MIN_X + MAX_X) * 0.5f, (MIN_Y + MAX_Y) * 0.5f);
			target = InnerMapManager.Instance.currentlyShowingMap.centerGo.transform;
		}
	}

	public void CenterCameraOnTile(Area area, bool instantCenter = true)
	{
		if (instantCenter)
		{
			MoveCamera(area.worldPosition);
		}
		else
		{
			target = area.areaItem.transform;
		}
	}
}

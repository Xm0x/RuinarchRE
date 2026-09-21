using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChaosOrbPointer : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private RectTransform _arrowPointerTransform;

	[SerializeField]
	private Image _imgArrowPointer;

	private RectTransform _boundsRectTransform;

	private float _yOffsetArrowPosition;

	private Vector3 _targetPosition;

	private Vector3 _arrowWorldPosition;

	private Vector2 _pivotSize;

	private int _durationInSeconds;

	private float _timeElapsed;

	private Vector3 _lastArrowWorldPosition;

	private Sequence _idleAnimation;

	private float _xMinPosition => _boundsRectTransform.offsetMin.x;

	private float _yMinPosition => _boundsRectTransform.offsetMin.y;

	private float _xMaxPosition => UIManager.Instance.canvasScaler.referenceResolution.x + _boundsRectTransform.offsetMax.x;

	private float _yMaxPosition => UIManager.Instance.canvasScaler.referenceResolution.y + _boundsRectTransform.offsetMax.y;

	private float _xMinPositionWithPivot => _pivotSize.x + _xMinPosition;

	private float _yMinPositionWithPivot => _pivotSize.y + _yMinPosition;

	private float _xMaxPositionWithPivot => _xMaxPosition - _pivotSize.x;

	private float _yMaxPositionWithPivot => _yMaxPosition - _pivotSize.y;

	private Vector2 _maxPositionWithPivotAndScaling => new Vector2(_xMaxPositionWithPivot * scaleFactorX, _yMaxPositionWithPivot * scaleFactorY);

	private Vector2 _minPositionWithPivotAndScaling => new Vector2(_xMinPositionWithPivot * scaleFactorX, _yMinPositionWithPivot * scaleFactorY);

	private Vector2 _maxPositionWithScalingOnly => new Vector2(_xMaxPosition * scaleFactorX, _yMaxPosition * scaleFactorY);

	private Vector2 _minPositionWithScalingOnly => new Vector2(_xMinPosition * scaleFactorX, _yMinPosition * scaleFactorY);

	private float scaleFactorX => (float)Screen.width / UIManager.Instance.canvasScaler.referenceResolution.x;

	private float scaleFactorY => (float)Screen.height / UIManager.Instance.canvasScaler.referenceResolution.y;

	private void Start()
	{
		_pivotSize = new Vector2(_arrowPointerTransform.sizeDelta.x * _arrowPointerTransform.pivot.x, _arrowPointerTransform.sizeDelta.y * _arrowPointerTransform.pivot.y);
	}

	private void OnDisable()
	{
		if (_idleAnimation != null)
		{
			_idleAnimation.Kill();
			_idleAnimation = null;
		}
	}

	private void Update()
	{
		if (_targetPosition == Vector3.positiveInfinity)
		{
			return;
		}
		if (_durationInSeconds >= 0)
		{
			_timeElapsed += Time.deltaTime;
			if (_timeElapsed >= (float)_durationInSeconds)
			{
				_timeElapsed = 0f;
				HideChaosOrbPointer();
				return;
			}
		}
		_arrowWorldPosition = GridMap.Instance.mainRegion.innerMap.GetWorldPositionFromScreenPosition(_arrowPointerTransform.position);
		Vector3 screenPositionFromWorldPosition = GridMap.Instance.mainRegion.innerMap.GetScreenPositionFromWorldPosition(_targetPosition);
		screenPositionFromWorldPosition.z = 0f;
		if (IsPositionInsideCoordinates(screenPositionFromWorldPosition, _minPositionWithScalingOnly, _maxPositionWithScalingOnly))
		{
			PositionArrow(screenPositionFromWorldPosition);
		}
		else
		{
			MoveTo(screenPositionFromWorldPosition);
			LookAt(_targetPosition);
			if (_lastArrowWorldPosition != _arrowWorldPosition)
			{
				PlayIdleAnimation();
			}
		}
		_lastArrowWorldPosition = _arrowWorldPosition;
	}

	public void Initialize(RectTransform p_boundsTransform, float p_yOffsetArrowPosition)
	{
		_boundsRectTransform = p_boundsTransform;
		_yOffsetArrowPosition = p_yOffsetArrowPosition;
	}

	public void ShowChaosOrbPointerOn(Vector3 p_targetPosition, int p_durationInSeconds)
	{
		_durationInSeconds = p_durationInSeconds;
		_targetPosition = p_targetPosition;
		_timeElapsed = 0f;
		UpdatePointerGO();
		if (base.gameObject.activeSelf)
		{
			PlayShowAnimation();
		}
	}

	public void HideChaosOrbPointer()
	{
		_targetPosition = Vector3.positiveInfinity;
		UpdatePointerGO();
		ChaosOrbParentPointerUI.Instance.ReturnChaosOrbPointerToPool(this);
	}

	private void UpdatePointerGO()
	{
		base.gameObject.SetActive(_targetPosition != Vector3.positiveInfinity);
	}

	private void LookAt(Vector3 p_targetWorldPosition)
	{
		Vector3 vector = p_targetWorldPosition - _arrowWorldPosition;
		vector.Normalize();
		float num = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
		_arrowPointerTransform.localRotation = Quaternion.Euler(0f, 0f, num - 90f);
	}

	private void MoveTo(Vector3 p_targetScreenPosition)
	{
		KeepArrowPositionFullyInRect(p_targetScreenPosition);
	}

	private void PositionArrow(Vector3 p_targetPosition)
	{
		p_targetPosition.y += _yOffsetArrowPosition * UIManager.Instance.canvas.scaleFactor;
		_arrowPointerTransform.position = p_targetPosition;
		_arrowPointerTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);
	}

	private void KeepArrowPositionFullyInRect(Vector3 p_targetScreenPosition)
	{
		Vector3 position = p_targetScreenPosition;
		if (position.x > _maxPositionWithPivotAndScaling.x)
		{
			position.x = _maxPositionWithPivotAndScaling.x;
		}
		if (position.y > _maxPositionWithPivotAndScaling.y)
		{
			position.y = _maxPositionWithPivotAndScaling.y;
		}
		if (position.x < _minPositionWithPivotAndScaling.x)
		{
			position.x = _minPositionWithPivotAndScaling.x;
		}
		if (position.y < _minPositionWithPivotAndScaling.y)
		{
			position.y = _minPositionWithPivotAndScaling.y;
		}
		_arrowPointerTransform.position = position;
	}

	private Vector3 GetArrowPositionOutsideOtherUI(Vector3 p_supposedPosition, DIRECTION p_direction)
	{
		Vector3 result = p_supposedPosition;
		Vector2 vector = new Vector2(_pivotSize.x * scaleFactorX, _pivotSize.y * scaleFactorY);
		for (int i = 0; i < UIManager.Instance.disallowOverlapsWithCP.Count; i++)
		{
			DisallowOverlapWithCharacterPointer disallowOverlapWithCharacterPointer = UIManager.Instance.disallowOverlapsWithCP[i];
			if (!disallowOverlapWithCharacterPointer.gameObject.activeInHierarchy)
			{
				continue;
			}
			Vector3 vector2 = disallowOverlapWithCharacterPointer.anchoredOffsetMin;
			Vector3 vector3 = disallowOverlapWithCharacterPointer.anchoredOffsetMax;
			if (RectTransformUtility.RectangleContainsScreenPoint(disallowOverlapWithCharacterPointer.rectTransform, p_supposedPosition))
			{
				if (p_direction == DIRECTION.RIGHT)
				{
					result.x = vector3.x + vector.x;
				}
				if (p_direction == DIRECTION.LEFT)
				{
					result.x = vector2.x - vector.x;
				}
				if (p_direction == DIRECTION.UP)
				{
					result.y = vector3.y + vector.y;
				}
				if (p_direction == DIRECTION.DOWN)
				{
					result.y = vector2.y - vector.y;
				}
			}
		}
		return result;
	}

	private bool IsPositionInsideCoordinates(Vector3 p_supposedPosition, Vector3 p_minPosition, Vector3 p_maxPosition)
	{
		Vector3 vector = p_minPosition;
		Vector3 vector2 = p_maxPosition;
		if (p_supposedPosition.x >= vector.x && p_supposedPosition.x <= vector2.x && p_supposedPosition.y >= vector.y && p_supposedPosition.y <= vector2.y)
		{
			return true;
		}
		return false;
	}

	private void PlayShowAnimation()
	{
		Color color = _imgArrowPointer.color;
		color.a = 0f;
		_imgArrowPointer.color = color;
		Sequence sequence = DOTween.Sequence();
		sequence.Append(_imgArrowPointer.DOFade(1f, 0.5f));
		sequence.Join(_imgArrowPointer.rectTransform.DOPunchScale(Vector3.one, 0.5f));
		sequence.Play();
	}

	private void PlayIdleAnimation()
	{
		if (_idleAnimation != null)
		{
			_idleAnimation.Kill();
			_idleAnimation = null;
		}
		_idleAnimation = DOTween.Sequence();
		_idleAnimation.Append(_imgArrowPointer.rectTransform.DOAnchorPosY(-10f, 0.4f));
		_idleAnimation.Append(_imgArrowPointer.rectTransform.DOAnchorPosY(0f, 0.4f));
		_idleAnimation.SetLoops(-1, LoopType.Yoyo);
		_idleAnimation.Play();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		InnerMapCameraMove.Instance.CenterCameraOn(_targetPosition);
	}

	public void ReturnGameObjectToPool()
	{
		if (_idleAnimation != null)
		{
			_idleAnimation.Kill();
			_idleAnimation = null;
		}
	}
}

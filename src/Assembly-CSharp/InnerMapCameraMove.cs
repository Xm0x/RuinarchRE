using DG.Tweening;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Settings;
using UnityEngine;

public class InnerMapCameraMove : BaseCameraMove
{
	public static InnerMapCameraMove Instance;

	private float previousCameraFOV;

	[SerializeField]
	private float xSeeLimit;

	public Tweener innerMapCameraShakeMeteorTween { get; private set; }

	public float currentFOV => camera.orthographicSize;

	public float maxFOV => _maxFov;

	public float minFOV => _minFov;

	protected override void Awake()
	{
		base.Awake();
		Instance = this;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Instance = null;
	}

	public override void Initialize()
	{
		base.Initialize();
		Messenger.AddListener<Region>(RegionSignals.REGION_MAP_OPENED, OnInnerMapOpened);
		Messenger.AddListener<Region>(RegionSignals.REGION_MAP_CLOSED, OnInnerMapClosed);
	}

	private void OnInnerMapOpened(Region location)
	{
		base.gameObject.SetActive(value: true);
		SetCameraBordersForMap(location.innerMap);
		ConstrainCameraBounds();
		camera.depth = 2f;
	}

	private void OnInnerMapClosed(Region location)
	{
		base.gameObject.SetActive(value: false);
		camera.depth = 0f;
	}

	public void SetCameraBordersForMap(InnerTileMap map)
	{
		float y = map.transform.localPosition.y;
		MIN_X = map.cameraBounds.x;
		MIN_Y = y + map.cameraBounds.y;
		MAX_X = map.cameraBounds.z;
		MAX_Y = y + map.cameraBounds.w;
	}

	public bool IsVisible(Vector3 p_worldPosition)
	{
		Vector3 vector = camera.WorldToViewportPoint(p_worldPosition);
		if (vector.x >= 0f && vector.x <= xSeeLimit && vector.y >= 0f && vector.y <= 1f)
		{
			return vector.z >= 0f;
		}
		return false;
	}

	public bool CanSee(DemonicStructure demonicStructure)
	{
		if (demonicStructure.structureObj == null)
		{
			return false;
		}
		return IsVisible(demonicStructure.structureObj.transform.position);
	}

	public void MeteorShake()
	{
		if (!SettingsManager.Instance.settings.disableCameraShake && !DOTween.IsTweening(camera))
		{
			innerMapCameraShakeMeteorTween = camera.DOShakeRotation(0.8f, new Vector3(8f, 8f, 0f), 35, 90f, fadeOut: false);
			innerMapCameraShakeMeteorTween.OnComplete(OnCompleteMeteorShakeTween);
		}
	}

	private void OnCompleteMeteorShakeTween()
	{
		camera.transform.DORotate(new Vector3(0f, 0f, 0f), 0.2f);
		innerMapCameraShakeMeteorTween = null;
	}

	public void EarthquakeShake()
	{
		if (!SettingsManager.Instance.settings.disableCameraShake)
		{
			camera.DOShakeRotation(1f, new Vector3(2f, 2f, 2f), 15, 90f, fadeOut: false);
		}
	}

	public void DoGenericCameraShake()
	{
		if (!SettingsManager.Instance.settings.disableCameraShake)
		{
			camera.DOShakeRotation(0.1f, new Vector3(2f, 2f, 2f), 15, 90f, fadeOut: false);
		}
	}
}

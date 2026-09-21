using EZObjectPools;
using Inner_Maps;
using UnityEngine;
using UnityEngine.UI;

public class BedMarkerNameplate : PooledObject
{
	[SerializeField]
	private RectTransform thisRect;

	[SerializeField]
	private GameObject visualsParent;

	[SerializeField]
	private Image actionIcon;

	private BedObjectGameObject bedGO;

	private const float DefaultSize = 80f;

	public void Initialize(BedObjectGameObject bedGO)
	{
		base.name = bedGO.name + " Marker Nameplate";
		this.bedGO = bedGO;
		UpdateSizeBasedOnZoom();
		Messenger.AddListener<Camera, float>(ControlsSignals.CAMERA_ZOOM_CHANGED, OnCameraZoomChanged);
		Messenger.AddListener<Region>(RegionSignals.REGION_MAP_OPENED, OnLocationMapOpened);
		Messenger.AddListener<Region>(RegionSignals.REGION_MAP_CLOSED, OnLocationMapClosed);
	}

	private void OnCameraZoomChanged(Camera camera, float amount)
	{
		if (camera == InnerMapCameraMove.Instance.camera)
		{
			UpdateSizeBasedOnZoom();
		}
	}

	private void OnLocationMapClosed(Region location)
	{
		if (location == bedGO.bedTileObject.currentRegion)
		{
			HideMarkerNameplate();
		}
	}

	private void OnLocationMapOpened(Region location)
	{
		if (location == bedGO.bedTileObject.currentRegion)
		{
			UpdateMarkerNameplate(bedGO.bedTileObject);
		}
	}

	private void LateUpdate()
	{
		Vector3 position = InnerMapCameraMove.Instance.camera.WorldToScreenPoint(bedGO.transform.position);
		position.z = 0f;
		base.transform.position = position;
	}

	public override void Reset()
	{
		base.Reset();
		bedGO = null;
		Messenger.RemoveListener<Camera, float>(ControlsSignals.CAMERA_ZOOM_CHANGED, OnCameraZoomChanged);
		Messenger.RemoveListener<Region>(RegionSignals.REGION_MAP_OPENED, OnLocationMapOpened);
		Messenger.RemoveListener<Region>(RegionSignals.REGION_MAP_CLOSED, OnLocationMapClosed);
	}

	public void UpdateMarkerNameplate(TileObject bedTileObject)
	{
		int userCount = bedTileObject.GetUserCount();
		bool flag = false;
		if (bedGO.bedTileObject.currentRegion == InnerMapManager.Instance.currentlyShowingLocation)
		{
			if (bedTileObject.isBeingSeized)
			{
				flag = false;
			}
			else
			{
				switch (userCount)
				{
				case 1:
					flag = true;
					break;
				case 2:
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			bool flag2 = true;
			Character firstUser = bedTileObject.GetFirstUser();
			if (firstUser == null)
			{
				HideMarkerNameplate();
				return;
			}
			ActualGoapNode currentActionNode = firstUser.currentActionNode;
			if (currentActionNode != null && (currentActionNode.actionStatus == ACTION_STATUS.PERFORMING || currentActionNode.actionStatus == ACTION_STATUS.STARTED))
			{
				string actionIconString = currentActionNode.action.GetActionIconString(currentActionNode);
				if (actionIconString != GoapActionStateDB.No_Icon)
				{
					UpdateActionIcon(InteractionManager.Instance.actionIconDictionary[actionIconString]);
				}
				else
				{
					flag2 = false;
				}
			}
			else if (firstUser.traitContainer.HasTrait("Quarantined"))
			{
				UpdateActionIcon(InteractionManager.Instance.actionIconDictionary[GoapActionStateDB.Sick_Icon]);
			}
			else
			{
				UpdateActionIcon(InteractionManager.Instance.actionIconDictionary[GoapActionStateDB.Sleep_Icon]);
			}
			if (flag2)
			{
				ShowMarkerNameplate();
			}
			else
			{
				HideMarkerNameplate();
			}
		}
		else
		{
			HideMarkerNameplate();
		}
	}

	public void ShowMarkerNameplate()
	{
		base.gameObject.SetActive(value: true);
	}

	public void HideMarkerNameplate()
	{
		base.gameObject.SetActive(value: false);
	}

	private void UpdateActionIcon(Sprite sprite)
	{
		actionIcon.sprite = sprite;
	}

	private void UpdateSizeBasedOnZoom()
	{
		float num = InnerMapCameraMove.Instance.currentFOV - InnerMapCameraMove.Instance.minFOV;
		float num2 = bedGO.usedBedRect.width + 4f * num;
		thisRect.sizeDelta = new Vector2(num2, num2);
	}
}

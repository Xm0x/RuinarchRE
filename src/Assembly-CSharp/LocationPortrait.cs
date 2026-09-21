using EZObjectPools;
using Locations.Settlements;
using Ruinarch;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LocationPortrait : PooledObject, IPointerClickHandler, IEventSystemHandler
{
	private BaseSettlement _settlement;

	[SerializeField]
	private Image portrait;

	[SerializeField]
	private GameObject hoverObj;

	public bool disableInteraction;

	public Region region { get; private set; }

	public void OnPointerClick(PointerEventData eventData)
	{
		if (!disableInteraction && eventData.button == PointerEventData.InputButton.Right && _settlement != null)
		{
			UIManager.Instance.ShowPlayerActionContextMenu(_settlement, InputManager.Instance.mousePosition, p_isScreenPosition: true);
		}
	}

	public void SetLocation(Region region)
	{
		this.region = region;
	}

	public void SetLocation(BaseSettlement p_settlement)
	{
		region = null;
		_settlement = p_settlement;
	}

	public void ClearLocations()
	{
		region = null;
		_settlement = null;
	}

	public void SetPortrait(STRUCTURE_TYPE landmarkType)
	{
		portrait.sprite = LandmarkManager.Instance.GetStructureData(landmarkType)?.structureSprite;
	}

	public void SetHoverHighlightState(bool state)
	{
		if (!disableInteraction)
		{
			hoverObj.SetActive(state);
		}
	}

	public void ShowLocationInfo()
	{
		if (region != null)
		{
			UIManager.Instance.ShowSmallInfo(region.name);
		}
	}

	public void HideLocationInfo()
	{
		UIManager.Instance.HideSmallInfo();
	}

	public override void Reset()
	{
		base.Reset();
		region = null;
		_settlement = null;
	}
}

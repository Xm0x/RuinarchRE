using System.Linq;
using Locations.Settlements;
using Ruinarch;
using UnityEngine;

public class SettlementNameplateItem : NameplateItem<BaseSettlement>
{
	[Header("Settlement Attributes")]
	[SerializeField]
	private LocationPortrait portrait;

	private BaseSettlement _settlement;

	public BaseSettlement settlement => _settlement;

	private void OnEnable()
	{
		AddOnRightClickAction(OnRightClickItem);
	}

	private void OnDisable()
	{
		RemoveOnRightClickAction(OnRightClickItem);
	}

	public override void SetObject(BaseSettlement o)
	{
		base.SetObject(o);
		_settlement = o;
		UpdateVisuals();
	}

	private void UpdateVisuals()
	{
		if (_settlement.areas.Count > 0)
		{
			portrait.SetPortrait(STRUCTURE_TYPE.CITY_CENTER);
		}
		mainLbl.text = _settlement.name;
		if (_settlement is NPCSettlement nPCSettlement)
		{
			if (nPCSettlement.locationType == LOCATION_TYPE.VILLAGE)
			{
				string text = string.Empty;
				if (nPCSettlement.owner != null)
				{
					text = LocalizationManager.Instance.GetLocalizedValue("Faction_Table", nPCSettlement.owner.factionType.type.ToStringEnum());
				}
				subLbl.text = text;
			}
			else if (nPCSettlement.settlementType != null)
			{
				subLbl.text = nPCSettlement.settlementType.settlementType.ToStringEnumWithSpace();
			}
			else if (nPCSettlement.structures.Count > 0)
			{
				STRUCTURE_TYPE key = nPCSettlement.structures.First().Key;
				subLbl.text = key.LocalizedStructureName();
			}
			else
			{
				subLbl.text = _settlement.locationType.ToStringEnumWithSpaceNormalized();
			}
		}
		else
		{
			subLbl.text = _settlement.locationType.ToStringEnumWithSpaceNormalized();
		}
	}

	private void OnRightClickItem(BaseSettlement p_settlement)
	{
		UIManager.Instance.ShowPlayerActionContextMenu(p_settlement, InputManager.Instance.mousePosition, p_isScreenPosition: true);
	}

	public override void Reset()
	{
		base.Reset();
		_settlement = null;
	}
}

using EZObjectPools;
using Locations.Settlements;
using Ruinarch.Custom_UI;
using TMPro;

public class FactionVillageFilterItem : PooledObject
{
	public TextMeshProUGUI nameLbl;

	public RuinarchToggle toggle;

	public BaseSettlement village { get; private set; }

	public void SetVillage(BaseSettlement village)
	{
		this.village = village;
		nameLbl.text = village.iconRichText + " " + village.name;
	}

	public void OnToggle(bool state)
	{
		if (state)
		{
			FactionInfoHubUI.Instance.FilterVillage(village);
		}
		else
		{
			FactionInfoHubUI.Instance.UnFilterVillage(village);
		}
	}

	public override void Reset()
	{
		base.Reset();
		village = null;
	}
}

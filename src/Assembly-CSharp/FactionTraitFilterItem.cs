using EZObjectPools;
using Ruinarch.Custom_UI;
using TMPro;

public class FactionTraitFilterItem : PooledObject
{
	public TextMeshProUGUI nameLbl;

	public RuinarchToggle toggle;

	public string traitName { get; private set; }

	public void SetTraitName(string traitName)
	{
		this.traitName = traitName;
		nameLbl.text = TraitManager.Instance.GetLocalizedNameOfTrait(traitName);
	}

	public void OnToggle(bool state)
	{
		if (state)
		{
			FactionInfoHubUI.Instance.FilterTrait(traitName);
		}
		else
		{
			FactionInfoHubUI.Instance.UnFilterTrait(traitName);
		}
	}

	public override void Reset()
	{
		base.Reset();
		traitName = null;
	}
}

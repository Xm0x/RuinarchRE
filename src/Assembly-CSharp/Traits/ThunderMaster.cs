namespace Traits;

public class ThunderMaster : Trait
{
	public ThunderMaster()
	{
		name = "Thunder Master";
		description = "Obsessed with Electricity.";
		type = TRAIT_TYPE.BUFF;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		AddTraitOverrideFunctionIdentifier("Change_Element");
	}

	public override void OnChangeElement(Character p_owner, ELEMENTAL_TYPE p_newElement, ELEMENTAL_TYPE p_oldElement)
	{
		if (p_newElement == ELEMENTAL_TYPE.Electric)
		{
			if (p_oldElement != ELEMENTAL_TYPE.Electric)
			{
				p_owner.piercingAndResistancesComponent.AdjustPiercingMultiplier(50f);
			}
		}
		else if (p_oldElement == ELEMENTAL_TYPE.Electric)
		{
			p_owner.piercingAndResistancesComponent.AdjustPiercingMultiplier(-50f);
		}
	}
}

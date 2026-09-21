namespace Traits;

public class WindMaster : Trait
{
	public WindMaster()
	{
		name = "Wind Master";
		description = "Can snuff out Tornadoes. Bonus Piercing for Wind attacks.";
		type = TRAIT_TYPE.BUFF;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		AddTraitOverrideFunctionIdentifier("Change_Element");
	}

	public override void OnChangeElement(Character p_owner, ELEMENTAL_TYPE p_newElement, ELEMENTAL_TYPE p_oldElement)
	{
		if (p_newElement == ELEMENTAL_TYPE.Wind)
		{
			if (p_oldElement != ELEMENTAL_TYPE.Wind)
			{
				p_owner.piercingAndResistancesComponent.AdjustPiercingMultiplier(50f);
			}
		}
		else if (p_oldElement == ELEMENTAL_TYPE.Wind)
		{
			p_owner.piercingAndResistancesComponent.AdjustPiercingMultiplier(-50f);
		}
	}
}

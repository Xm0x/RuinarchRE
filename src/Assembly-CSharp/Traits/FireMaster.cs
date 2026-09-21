namespace Traits;

public class FireMaster : Trait
{
	public FireMaster()
	{
		name = "Fire Master";
		description = "Can extinguish all sorts of Fire. Bonus Piercing for Fire attacks.";
		type = TRAIT_TYPE.BUFF;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		AddTraitOverrideFunctionIdentifier("Change_Element");
	}

	public override void OnChangeElement(Character p_owner, ELEMENTAL_TYPE p_newElement, ELEMENTAL_TYPE p_oldElement)
	{
		if (p_newElement == ELEMENTAL_TYPE.Fire)
		{
			if (p_oldElement != ELEMENTAL_TYPE.Fire)
			{
				p_owner.piercingAndResistancesComponent.AdjustPiercingMultiplier(50f);
			}
		}
		else if (p_oldElement == ELEMENTAL_TYPE.Fire)
		{
			p_owner.piercingAndResistancesComponent.AdjustPiercingMultiplier(-50f);
		}
	}
}

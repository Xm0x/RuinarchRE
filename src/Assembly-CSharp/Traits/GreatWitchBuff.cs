namespace Traits;

public class GreatWitchBuff : Status
{
	public GreatWitchBuff()
	{
		name = "Great Witch Buff";
		description = "Very high Elemental and Mental Resistances";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.POSITIVE;
		ticksDuration = 0;
		isHidden = true;
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character character)
		{
			character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Fire, 50f);
			character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Water, 50f);
			character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Earth, 50f);
			character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Wind, 50f);
			character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Mental, 50f);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character character)
		{
			character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Fire, -50f);
			character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Water, -50f);
			character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Earth, -50f);
			character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Wind, -50f);
			character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Mental, -50f);
		}
	}
}

namespace Traits;

public class ElementalProtection : Status
{
	public ElementalProtection()
	{
		name = "Elemental Protection";
		description = "Temporarily increases resistance to Earth, Wind, Fire and Water";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.POSITIVE;
		isStacking = true;
		stackLimit = 1;
		ticksDuration = 20;
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character character)
		{
			character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Fire, 30f);
			character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Water, 30f);
			character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Earth, 30f);
			character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Wind, 30f);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character character)
		{
			character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Fire, -30f);
			character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Water, -30f);
			character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Earth, -30f);
			character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Wind, -30f);
		}
	}
}

namespace Traits;

public class Stoneskin : Status
{
	public Stoneskin()
	{
		name = "Stoneskin";
		description = "Temporarily increases Physical Resistance";
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
			character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Physical, 30f);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character character)
		{
			character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Physical, -30f);
		}
	}
}

namespace Traits;

public class FishFed : Status
{
	public FishFed()
	{
		name = "Fish Fed";
		description = "Recently ate: Fish. Doubles all secondary resistances";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.POSITIVE;
		isStacking = true;
		stackLimit = 1;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(24);
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character character)
		{
			character.piercingAndResistancesComponent.AdjustResistanceMultiplier(RESISTANCE.Poison, 2f);
			character.piercingAndResistancesComponent.AdjustResistanceMultiplier(RESISTANCE.Electric, 2f);
			character.piercingAndResistancesComponent.AdjustResistanceMultiplier(RESISTANCE.Ice, 2f);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character character)
		{
			character.piercingAndResistancesComponent.AdjustResistanceMultiplier(RESISTANCE.Poison, -2f);
			character.piercingAndResistancesComponent.AdjustResistanceMultiplier(RESISTANCE.Electric, -2f);
			character.piercingAndResistancesComponent.AdjustResistanceMultiplier(RESISTANCE.Ice, -2f);
		}
	}
}

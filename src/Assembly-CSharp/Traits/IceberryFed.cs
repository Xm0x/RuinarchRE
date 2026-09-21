namespace Traits;

public class IceberryFed : Status
{
	public IceberryFed()
	{
		name = "Iceberry Fed";
		description = "Recently ate: Iceberry. Doubles all elemental resistances";
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
			character.piercingAndResistancesComponent.AdjustResistanceMultiplier(RESISTANCE.Fire, 2f);
			character.piercingAndResistancesComponent.AdjustResistanceMultiplier(RESISTANCE.Water, 2f);
			character.piercingAndResistancesComponent.AdjustResistanceMultiplier(RESISTANCE.Earth, 2f);
			character.piercingAndResistancesComponent.AdjustResistanceMultiplier(RESISTANCE.Wind, 2f);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character character)
		{
			character.piercingAndResistancesComponent.AdjustResistanceMultiplier(RESISTANCE.Fire, -2f);
			character.piercingAndResistancesComponent.AdjustResistanceMultiplier(RESISTANCE.Water, -2f);
			character.piercingAndResistancesComponent.AdjustResistanceMultiplier(RESISTANCE.Earth, -2f);
			character.piercingAndResistancesComponent.AdjustResistanceMultiplier(RESISTANCE.Wind, -2f);
		}
	}
}

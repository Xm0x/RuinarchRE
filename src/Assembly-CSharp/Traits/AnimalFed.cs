namespace Traits;

public class AnimalFed : Status
{
	public AnimalFed()
	{
		name = "Animal Fed";
		description = "Recently ate: Animal Meat. Increases piercing by 50%";
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
			character.piercingAndResistancesComponent.AdjustPiercingMultiplier(50f);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character character)
		{
			character.piercingAndResistancesComponent.AdjustPiercingMultiplier(-50f);
		}
	}
}

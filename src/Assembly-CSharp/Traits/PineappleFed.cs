namespace Traits;

public class PineappleFed : Status
{
	public PineappleFed()
	{
		name = "Pineapple Fed";
		description = "Recently ate: Pineapple. Doubles mental and normal resistances";
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
			character.piercingAndResistancesComponent.AdjustResistanceMultiplier(RESISTANCE.Physical, 2f);
			character.piercingAndResistancesComponent.AdjustResistanceMultiplier(RESISTANCE.Mental, 2f);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character character)
		{
			character.piercingAndResistancesComponent.AdjustResistanceMultiplier(RESISTANCE.Physical, -2f);
			character.piercingAndResistancesComponent.AdjustResistanceMultiplier(RESISTANCE.Mental, -2f);
		}
	}
}

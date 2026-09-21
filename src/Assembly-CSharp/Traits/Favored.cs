namespace Traits;

public class Favored : Status
{
	public Favored()
	{
		name = "Favored";
		description = "Favored by gods.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.POSITIVE;
		isStacking = false;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(12);
		isHidden = true;
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character character)
		{
			character.piercingAndResistancesComponent.AdjustAllResistances(25f);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character character)
		{
			character.piercingAndResistancesComponent.AdjustAllResistances(-25f);
		}
	}
}

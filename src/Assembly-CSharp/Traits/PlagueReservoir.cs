namespace Traits;

public class PlagueReservoir : Trait
{
	public override bool isSingleton => true;

	public PlagueReservoir()
	{
		name = "Plague Reservoir";
		description = "Immune to a Plague's effect but can spread it.";
		type = TRAIT_TYPE.BUFF;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		addedTo.traitContainer.RemoveTrait(addedTo, "Plagued");
		addedTo.traitContainer.AddTrait(addedTo, "Plagued");
		if (addedTo is Character character)
		{
			float num = EditableValuesManager.Instance.baseFullnessDecreaseRate / 2f;
			float num2 = EditableValuesManager.Instance.baseHappinessDecreaseRate / 2f;
			character.needsComponent.AdjustFullnessDecreaseRate(0f - num);
			character.needsComponent.AdjustHappinessDecreaseRate(0f - num2);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character character)
		{
			float amount = EditableValuesManager.Instance.baseFullnessDecreaseRate / 2f;
			float amount2 = EditableValuesManager.Instance.baseHappinessDecreaseRate / 2f;
			character.needsComponent.AdjustFullnessDecreaseRate(amount);
			character.needsComponent.AdjustHappinessDecreaseRate(amount2);
		}
	}
}

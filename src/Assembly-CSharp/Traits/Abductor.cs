namespace Traits;

public class Abductor : Trait
{
	public override bool isSingleton => true;

	public Abductor()
	{
		name = "Abductor";
		description = "Abducts one villager at a time and drops them where you summoned it. Eats them.";
		type = TRAIT_TYPE.NEUTRAL;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		isHidden = true;
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character character)
		{
			character.behaviourComponent.UpdateDefaultBehaviourSet();
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character character)
		{
			character.behaviourComponent.UpdateDefaultBehaviourSet();
		}
	}
}

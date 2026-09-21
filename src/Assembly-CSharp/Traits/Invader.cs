namespace Traits;

public class Invader : Trait
{
	public override bool isSingleton => true;

	public Invader()
	{
		name = "Invader";
		description = "Attacks nearest Village or occupied Special Structure.";
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
			character.movementComponent.SetEnableDigging(state: true);
			character.behaviourComponent.UpdateDefaultBehaviourSet();
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character character)
		{
			character.movementComponent.SetEnableDigging(state: false);
			character.behaviourComponent.UpdateDefaultBehaviourSet();
		}
	}
}

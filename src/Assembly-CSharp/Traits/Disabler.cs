namespace Traits;

public class Disabler : Trait
{
	public override bool isSingleton => true;

	public Disabler()
	{
		name = "Disabler";
		description = "Disables all hostiles within its range. It will immediately perish afterwards.";
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
			character.SetDestroyMarkerOnDeath(state: true);
			if (character is Summon summon)
			{
				summon.SetShowNotificationOnDeath(showNotificationOnDeath: false);
			}
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

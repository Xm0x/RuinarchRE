namespace Traits;

public class Tower : Trait
{
	public override bool isSingleton => true;

	public Tower()
	{
		name = "Tower";
		description = "Stationary. This will not move.";
		type = TRAIT_TYPE.NEUTRAL;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
	}

	public override void OnAddTrait(ITraitable sourcePOI)
	{
		base.OnAddTrait(sourcePOI);
		if (sourcePOI is Character)
		{
			Character obj = sourcePOI as Character;
			obj.reactionComponent.SetIsHidden(state: false);
			obj.behaviourComponent.UpdateDefaultBehaviourSet();
		}
	}

	public override void OnRemoveTrait(ITraitable sourcePOI, Character removedBy)
	{
		base.OnRemoveTrait(sourcePOI, removedBy);
		if (sourcePOI is Character)
		{
			(sourcePOI as Character).behaviourComponent.UpdateDefaultBehaviourSet();
		}
	}
}

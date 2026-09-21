using System.Collections.Generic;

namespace Traits;

public class NoxiousWanderer : Trait
{
	public override bool isSingleton => true;

	public NoxiousWanderer()
	{
		name = "Noxious Wanderer";
		description = "Spews out poison clouds from time to time.";
		type = TRAIT_TYPE.NEUTRAL;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		advertisedInteractions = new List<INTERACTION_TYPE> { INTERACTION_TYPE.SPAWN_POISON_CLOUD };
	}

	public override void OnAddTrait(ITraitable sourcePOI)
	{
		base.OnAddTrait(sourcePOI);
		if (sourcePOI is Character)
		{
			(sourcePOI as Character).behaviourComponent.UpdateDefaultBehaviourSet();
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

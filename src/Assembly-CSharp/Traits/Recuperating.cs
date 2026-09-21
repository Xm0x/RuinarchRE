using System.Collections.Generic;

namespace Traits;

public class Recuperating : Status
{
	public override bool isSingleton => true;

	public Recuperating()
	{
		name = "Recuperating";
		description = "Recovering from an illness.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.POSITIVE;
		ticksDuration = 0;
		advertisedInteractions = new List<INTERACTION_TYPE>
		{
			INTERACTION_TYPE.FEED,
			INTERACTION_TYPE.KICK_OUT_OF_HOSPICE
		};
		isHidden = true;
		hindersWitness = true;
		AddTraitOverrideFunctionIdentifier("Tick_Started_Trait");
	}

	public override void OnTickStarted(ITraitable traitable)
	{
		base.OnTickStarted(traitable);
		if (traitable is Character character && character.tileObjectComponent.isUsingBed)
		{
			RecoverHP(character);
		}
	}

	private void RecoverHP(Character character)
	{
		character.PassiveHPRecovery(0.01f);
	}
}

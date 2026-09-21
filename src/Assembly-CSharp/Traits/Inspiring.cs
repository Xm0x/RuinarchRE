using UnityEngine;

namespace Traits;

public class Inspiring : Trait
{
	public override bool isSingleton => true;

	public Inspiring()
	{
		name = "Inspiring";
		description = "Randomly blurts out inspirational quotes. Somehow, others feel inspired by it.";
		type = TRAIT_TYPE.BUFF;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		AddTraitOverrideFunctionIdentifier("See_Poi_Trait");
	}

	public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob)
	{
		if (targetPOI is Character)
		{
			Character character = targetPOI as Character;
			if (characterThatWillDoJob.isNormalCharacter && character.isNormalCharacter && !characterThatWillDoJob.isDead && !character.isDead && character.limiterComponent.canWitness && (characterThatWillDoJob.faction == character.faction || characterThatWillDoJob.homeSettlement == character.homeSettlement) && Random.Range(0, 100) < 8)
			{
				character.interruptComponent.TriggerInterrupt(INTERRUPT.Inspired, characterThatWillDoJob);
				return true;
			}
		}
		return base.OnSeePOI(targetPOI, characterThatWillDoJob);
	}
}

using UtilityScripts;

namespace Traits;

public class Rabid : Status
{
	public Rabid()
	{
		name = "Rabid";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(12);
		moodEffect = -3;
		isStacking = false;
		AddTraitOverrideFunctionIdentifier("See_Poi_Trait");
	}

	public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob)
	{
		if (targetPOI is Character character && !character.IsHostileWith(characterThatWillDoJob) && GameUtilities.RollChance(50))
		{
			bool isLethal = characterThatWillDoJob.moodComponent.moodState == MOOD_STATE.Critical;
			characterThatWillDoJob.combatComponent.Fight(character, "Rabid", null, isLethal);
		}
		return base.OnSeePOI(targetPOI, characterThatWillDoJob);
	}
}

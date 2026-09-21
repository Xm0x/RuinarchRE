namespace Traits;

public class MusicHater : Trait
{
	public override bool isSingleton => true;

	public MusicHater()
	{
		name = "Music Hater";
		description = "Has an irrational hate for music.";
		type = TRAIT_TYPE.NEUTRAL;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = 0;
		mutuallyExclusive = new string[1] { "Music Lover" };
		AddTraitOverrideFunctionIdentifier("See_Poi_Trait");
	}

	public override void ExecuteCostModification(INTERACTION_TYPE action, Character actor, IPointOfInterest poiTarget, OtherData[] otherData, ref int cost)
	{
		switch (action)
		{
		case INTERACTION_TYPE.SING:
			cost += 2000;
			break;
		case INTERACTION_TYPE.PLAY_GUITAR:
			cost += 2000;
			break;
		}
	}

	public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob)
	{
		if (targetPOI is Guitar guitar && (guitar.IsOwnedBy(characterThatWillDoJob) || PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.MUSIC_HATER).HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Destroys_Guitars_On_Sight)))
		{
			return characterThatWillDoJob.jobComponent.TriggerDestroy(targetPOI, "Destroy_Music_Hater");
		}
		return base.OnSeePOI(targetPOI, characterThatWillDoJob);
	}

	public void ReactToMusicPerformer(Character p_witness, Character p_actor)
	{
		AfflictData afflictionData = PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.MUSIC_HATER);
		if (afflictionData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Murder_Singers_Guitar_Players))
		{
			p_witness.combatComponent.Fight(p_actor, "Music_Hater_Murder");
		}
		else if (afflictionData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Knockout_Singers_Guitar_Players))
		{
			p_witness.combatComponent.Fight(p_actor, "Music_Hater_Knockout", null, isLethal: false);
		}
	}
}

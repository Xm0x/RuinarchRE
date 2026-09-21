namespace Traits;

public class Betrayed : Status
{
	public Betrayed()
	{
		name = "Betrayed";
		description = "Someone backstabbed it.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(48);
		moodEffect = -10;
		isStacking = true;
		stackLimit = 5;
		stackModifier = 0.25f;
		hindersSocials = true;
		AddTraitOverrideFunctionIdentifier("Death_Trait");
	}

	public override bool OnDeath(Character character)
	{
		SchedulingManager.Instance.AddEntry(GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnMinutes(30)), delegate
		{
			SpawnGhostOf(character);
		}, character);
		return base.OnDeath(character);
	}

	private void SpawnGhostOf(Character character)
	{
		if (character.gridTileLocation != null)
		{
			Summon summon = CharacterManager.Instance.SpawnNewMonsterInstanceFrom(SUMMON_TYPE.Ghost, character, null, null, character.gridTileLocation);
			(summon as Ghost).SetBetrayedBy(base.responsibleCharacter);
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Trait", "Traits_Table", name + " spawn_ghost", LOG_TAG.Social, LOG_TAG.Life_Changes);
			log.AddToFillers(summon, summon.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(character, character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			log.AddLogToDatabase(releaseLogAfter: true);
			Messenger.Broadcast(CharacterSignals.GHOST_SPAWNED_THAT_COUNTS_FOR_TASK, summon);
		}
	}
}

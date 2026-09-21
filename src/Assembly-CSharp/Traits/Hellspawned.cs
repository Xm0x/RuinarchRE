using Characters.Components;
using Inner_Maps;
using UtilityScripts;

namespace Traits;

public class Hellspawned : Trait, CharacterEventDispatcher.IMoodListener
{
	public Hellspawned()
	{
		name = "Hellspawned";
		description = "Vessel for demons.";
		type = TRAIT_TYPE.FLAW;
		effect = TRAIT_EFFECT.NEGATIVE;
		AddTraitOverrideFunctionIdentifier("Death_Trait");
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character { isDead: false } character)
		{
			character.eventDispatcher.SubscribeToMoodEvents(this);
		}
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character { isDead: false } character)
		{
			character.eventDispatcher.SubscribeToMoodEvents(this);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character character)
		{
			character.eventDispatcher.UnsubscribeToMoodEvents(this);
		}
	}

	public override bool OnDeath(Character character)
	{
		if (character.hasMarker)
		{
			AkSoundEngine.PostEvent("Play_Hellspawn", character.marker.gameObject);
		}
		character.eventDispatcher.UnsubscribeToMoodEvents(this);
		LocationGridTile locationGridTile = character.gridTileLocation;
		if (locationGridTile == null)
		{
			locationGridTile = character.deathTilePosition;
		}
		if (locationGridTile == null)
		{
			return character.traitContainer.RemoveTrait(character, this);
		}
		PlayerAction playerActionData = PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.HELLSPAWN);
		SUMMON_TYPE sUMMON_TYPE = SUMMON_TYPE.None;
		int overrideDuration = 0;
		int num = 0;
		string text = "death_multiple";
		if (playerActionData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Spawn_Ifrit))
		{
			text = "death_single";
			sUMMON_TYPE = SUMMON_TYPE.Ifrit;
			num = 1;
			overrideDuration = GameManager.Instance.GetTicksBasedOnHour(3);
		}
		else if (playerActionData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Spawn_More_Imps))
		{
			sUMMON_TYPE = SUMMON_TYPE.Imp;
			num = 2;
			overrideDuration = GameManager.Instance.GetTicksBasedOnHour(2);
		}
		else if (playerActionData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Spawn_Imps))
		{
			sUMMON_TYPE = SUMMON_TYPE.Imp;
			num = 1;
			overrideDuration = GameManager.Instance.GetTicksBasedOnHour(2);
		}
		if (sUMMON_TYPE != SUMMON_TYPE.None)
		{
			for (int i = 0; i < num; i++)
			{
				Summon summon = CharacterManager.Instance.CreateNewSummon(sUMMON_TYPE, PlayerManager.Instance.player.playerFaction, null, locationGridTile.parentMap.region, null, "", bypassIdeologyChecking: true);
				summon.traitContainer.AddTrait(summon, "Ephemeral", null, bypassElementalChance: false, overrideDuration);
				CharacterManager.Instance.PlaceSummonInitially(summon, locationGridTile);
				GameManager.Instance.CreateParticleEffectAt(summon, PARTICLE_EFFECT.Spawn_Effect);
			}
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Trait", "Traits_Table", name + " " + text, LOG_TAG.Player);
			log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
		}
		return character.traitContainer.RemoveTrait(character, this);
	}

	public void OnMoodChanged(Character p_character, MOOD_STATE p_previousMood, MOOD_STATE p_newMood)
	{
		if (p_newMood == MOOD_STATE.Critical)
		{
			TryHellspawnDeath(p_character);
		}
	}

	public void TryHellspawnDeath(Character p_character)
	{
		if (GameUtilities.RollChance(30) && PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.HELLSPAWN).HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Erupt_On_Crit_Mood))
		{
			p_character.Death(name, null, null, null, null, null, null, isPlayerSource: true);
		}
	}
}

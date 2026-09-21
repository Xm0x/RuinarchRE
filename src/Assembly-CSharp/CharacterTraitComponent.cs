using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;
using UtilityScripts;

public class CharacterTraitComponent : CharacterComponent
{
	public bool hasAgoraphobicReactedThisTick { get; private set; }

	public bool willProcessPlayerSourceChaosOrb { get; private set; }

	public bool isOtherTick { get; private set; }

	public List<string> obsessedCharacterIDs { get; private set; }

	public CharacterTraitComponent()
	{
		obsessedCharacterIDs = new List<string>();
	}

	public CharacterTraitComponent(SaveDataCharacterTraitComponent data)
	{
		hasAgoraphobicReactedThisTick = data.hasAgoraphobicReactedThisTick;
		willProcessPlayerSourceChaosOrb = data.willProcessPlayerSourceChaosOrb;
		isOtherTick = data.isOtherTick;
		if (data.obsessedCharacterIDs != null && data.obsessedCharacterIDs.Count > 0)
		{
			obsessedCharacterIDs = new List<string>(data.obsessedCharacterIDs);
		}
		else
		{
			obsessedCharacterIDs = new List<string>();
		}
	}

	public void OnCharacterFinishedJob(JobQueueItem p_job)
	{
		if (p_job is GoapPlanJob { targetInteractionType: INTERACTION_TYPE.EAT_CORPSE } && base.owner.traitContainer.HasTrait("Hunting"))
		{
			base.owner.traitContainer.RemoveTrait(base.owner, "Hunting");
		}
	}

	public void SubscribeToAgoraphobiaLevelUpSignal()
	{
		Messenger.AddListener<SkillData>("AgoraphobiaLevelUp", OnAgoraphobiaLevelUp);
	}

	public void UnsubscribeToAgoraphobiaLevelUpSignal()
	{
		Messenger.RemoveListener<SkillData>("AgoraphobiaLevelUp", OnAgoraphobiaLevelUp);
	}

	public void OnAgoraphobiaLevelUp(SkillData p_skill)
	{
		if (p_skill.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.No_Longer_Join_Parties) && base.owner.HasAfflictedByPlayerWith(PLAYER_SKILL_TYPE.AGORAPHOBIA) && base.owner.partyComponent.hasParty)
		{
			base.owner.interruptComponent.TriggerInterrupt(INTERRUPT.Left_Party, base.owner, "", null, "Agoraphobic_Reason");
		}
	}

	public void SetHasAgoraphobicReactedThisTick(bool p_state)
	{
		if (hasAgoraphobicReactedThisTick == p_state)
		{
			return;
		}
		hasAgoraphobicReactedThisTick = p_state;
		if (hasAgoraphobicReactedThisTick)
		{
			GameDate gameDate = GameManager.Instance.Today();
			gameDate.AddTicks(1);
			SchedulingManager.Instance.AddEntry(gameDate, delegate
			{
				SetHasAgoraphobicReactedThisTick(p_state: false);
			}, base.owner);
		}
	}

	public void SubscribeToGluttonLevelUpSignal()
	{
		Messenger.AddListener<SkillData>("GluttonyLevelUp", OnGluttonLeveledUp);
	}

	public void UnsubscribeToGluttonLevelUpSignal()
	{
		Messenger.RemoveListener<SkillData>("GluttonyLevelUp", OnGluttonLeveledUp);
	}

	private void OnGluttonLeveledUp(SkillData p_skillData)
	{
		base.owner.traitContainer.GetTraitOrStatus<Glutton>("Glutton")?.OnGluttonLeveledUp();
	}

	public void SetWillProcessPlayerSourceChaosOrb(bool p_state)
	{
		if (willProcessPlayerSourceChaosOrb != p_state)
		{
			willProcessPlayerSourceChaosOrb = p_state;
			if (willProcessPlayerSourceChaosOrb)
			{
				Messenger.AddListener(Signals.TICK_STARTED, TickStartedProcessPlayerSourceChaosOrb);
			}
			else
			{
				Messenger.RemoveListener(Signals.TICK_STARTED, TickStartedProcessPlayerSourceChaosOrb);
			}
		}
	}

	private void TickStartedProcessPlayerSourceChaosOrb()
	{
		if (!isOtherTick)
		{
			isOtherTick = true;
			return;
		}
		isOtherTick = false;
		ProcessPlayerSourceChaosOrb();
	}

	private void ProcessPlayerSourceChaosOrb()
	{
		if (!base.owner.isDead && base.owner.isNormalAndNotAlliedWithPlayer && GameUtilities.RollChance(10))
		{
			LocationGridTile gridTileLocation = base.owner.gridTileLocation;
			if (gridTileLocation != null)
			{
				Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, gridTileLocation.centeredWorldLocation, 1, gridTileLocation.parentMap);
			}
		}
	}

	public void ApplyKnightBonuses(int p_talentLevel)
	{
		RemoveKnightBonuses();
		base.owner.traitContainer.AddTrait(base.owner, "Knight Party Bonus", out var trait);
		if (trait is KnightPartyBonus knightPartyBonus)
		{
			knightPartyBonus.SetLevel(p_talentLevel, base.owner);
		}
	}

	public void ApplyKnightBonuses()
	{
		int level = base.owner.TryGetTalentLevel(CHARACTER_TALENT.Martial_Arts);
		RemoveKnightBonuses();
		base.owner.traitContainer.AddTrait(base.owner, "Knight Party Bonus", out var trait);
		if (trait is KnightPartyBonus knightPartyBonus)
		{
			knightPartyBonus.SetLevel(level, base.owner);
		}
	}

	public void RemoveKnightBonuses()
	{
		base.owner.traitContainer.RemoveTrait(base.owner, "Knight Party Bonus");
	}

	public void MummifiedReleaseIncantationByCharacter(Character p_responsibleCharacter)
	{
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "cursed_mummy", LOG_TAG.Life_Changes, LOG_TAG.Major);
		log.AddToFillers(p_responsibleCharacter, p_responsibleCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddToFillers(base.owner, base.owner.name, LOG_IDENTIFIER.TARGET_CHARACTER);
		log.AddLogToDatabase();
		PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
		p_responsibleCharacter.traitContainer.AddTrait(p_responsibleCharacter, "Cursed");
		MummifiedReleaseIncantation();
	}

	public void MummifiedReleaseIncantationByPlayer()
	{
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "PlayerAlerts_Table", "cursed_mummy", LOG_TAG.Player, LOG_TAG.Major);
		log.AddToFillers(base.owner, base.owner.name, LOG_IDENTIFIER.TARGET_CHARACTER);
		log.AddLogToDatabase();
		PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
		Messenger.Broadcast(PlayerSignals.MUMMIFIED_RELEASED_BY_PLAYER, base.owner);
		PlayerManager.Instance.player.playerSkillComponent.SetAllPowersCooldownMultiplier(2);
		MummifiedReleaseIncantation();
		AkSoundEngine.PostEvent("Play_Mummified_Release", InnerMapCameraMove.Instance.gameObject);
	}

	private void MummifiedReleaseIncantation()
	{
		LocationGridTile gridTileLocation = base.owner.gridTileLocation;
		if (gridTileLocation != null)
		{
			GameManager.Instance.CreateParticleEffectAt(gridTileLocation, PARTICLE_EFFECT.Minion_Dissipate);
		}
		if (base.owner.isBeingCarriedBy != null)
		{
			base.owner.isBeingCarriedBy.UncarryPOI();
		}
		if (base.owner.grave != null && base.owner.grave.gridTileLocation != null)
		{
			base.owner.grave.gridTileLocation.structure.RemovePOI(base.owner.grave);
		}
		if (base.owner.hasMarker)
		{
			base.owner.DestroyMarker();
		}
	}

	public bool IsCharacterTargetOfObsession(Character p_target)
	{
		if (base.owner.traitContainer.HasTrait("Obsessed") && base.owner.traitContainer.GetTraitOrStatus<Obsessed>("Obsessed").IsCharacterTargetOfObsession(p_target))
		{
			return true;
		}
		return false;
	}

	public void AddObsessedCharacter(Character p_character)
	{
		obsessedCharacterIDs.Add(p_character.persistentID);
	}

	public bool RemoveObsessedCharacter(Character p_character)
	{
		return obsessedCharacterIDs.Remove(p_character.persistentID);
	}

	public bool IsCharacterSourceOfObsession(Character p_source)
	{
		return obsessedCharacterIDs.Contains(p_source.persistentID);
	}

	public void LoadReferences(SaveDataCharacterTraitComponent data)
	{
		if (willProcessPlayerSourceChaosOrb)
		{
			Messenger.AddListener(Signals.TICK_STARTED, TickStartedProcessPlayerSourceChaosOrb);
		}
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void DisconnectFromCharacter(Character p_character)
	{
		RemoveObsessedCharacter(p_character);
	}
}

using System;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Object_Pools;
using Traits;
using UnityEngine;

public class Minion
{
	public const int MAX_INTERVENTION_ABILITY_SLOT = 5;

	public Character character { get; private set; }

	public bool isSummoned { get; private set; }

	public PLAYER_SKILL_TYPE minionPlayerSkillType { get; private set; }

	public MINION_TYPE minionType { get; private set; }

	public DeadlySin deadlySin => CharacterManager.Instance.GetDeadlySin(character.characterClass.className);

	public Minion(Character character, bool keepData)
	{
		this.character = character;
		character.SetMinion(this);
		if (!keepData)
		{
			CharacterClass characterClass = character.characterClass;
			character.SetFirstName(characterClass.displayName);
		}
		if (character.behaviourComponent.defaultBehaviourSetName == "Default Resident Behaviour")
		{
			character.behaviourComponent.ChangeDefaultBehaviourSet("Default Minion Behaviour");
		}
		character.visuals.UpdateAllVisuals(character);
		character.isInfoUnlocked = true;
	}

	public Minion(Character character, SaveDataMinion data)
	{
		this.character = character;
		isSummoned = data.isSummoned;
		minionPlayerSkillType = data.minionPlayerSkillType;
		minionType = data.minionType;
		if (isSummoned)
		{
			SubscribeListeners();
		}
	}

	public void Death(string cause = "normal", ActualGoapNode deathFromAction = null, Character responsibleCharacter = null, Log _deathLog = null, LogFiller[] multipleDeathLogFillers = null, LogFiller p_singleDeathLogFiller = null)
	{
		if (character.isDead)
		{
			return;
		}
		_ = character.currentRegion;
		_ = character.currentStructure;
		LocationGridTile gridTileLocation = character.gridTileLocation;
		if (character.isBeingSeized)
		{
			PlayerManager.Instance.player.seizeComponent.UnseizePOIOnCharacterDeath();
		}
		character.SetDeathLocation(character.gridTileLocation);
		if (deathFromAction != null && deathFromAction.action != null)
		{
			character.causeOfDeath = deathFromAction.action.goapType;
		}
		character.SetIsDead(isDead: true);
		character.SetPOIState(POI_STATE.INACTIVE);
		character.reactionComponent.SetDisguisedCharacter(null);
		if (character.currentRegion == null)
		{
			throw new Exception("Specific location of " + character.name + " is null! Please use command /l_character_location_history [Character Name/ID] in console menu to log character's location history. (Use '~' to show console menu)");
		}
		if (character.stateComponent.currentState != null)
		{
			character.stateComponent.ExitCurrentState();
		}
		character.behaviourComponent.OnOwnerDied();
		character.DropAllItems(gridTileLocation);
		character.UnownOrTransferOwnershipOfAllItems();
		character.reactionComponent.SetIsHidden(state: false);
		character.UncarryPOI();
		character.isBeingCarriedBy?.UncarryPOI(character);
		if (character.partyComponent.hasParty)
		{
			character.partyComponent.currentParty.RemoveMember(character);
		}
		character.traitContainer.RemoveAllTraitsAndStatusesByName(character, "Criminal");
		List<Trait> traitOverrideFunctions = character.traitContainer.GetTraitOverrideFunctions("Death_Trait");
		if (traitOverrideFunctions != null)
		{
			for (int i = 0; i < traitOverrideFunctions.Count; i++)
			{
				if (traitOverrideFunctions[i].OnDeath(character))
				{
					i--;
				}
			}
		}
		character.traitContainer.RemoveAllNonPersistentTraitAndStatuses(character);
		character.marker?.OnDeath(gridTileLocation);
		Messenger.Broadcast(CharacterSignals.CHARACTER_DEATH, character);
		character.eventDispatcher.ExecuteCharacterDied(character);
		Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, (IPointOfInterest)character, GoapPlanJob.Target_Already_Dead_Reason);
		Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_ACTIONS_TARGETING_POI, (IPointOfInterest)character, GoapPlanJob.Target_Already_Dead_Reason);
		character.jobQueue.CancelAllJobs();
		if (_deathLog == null)
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterDeath_Table", "death_" + cause, LOG_TAG.Life_Changes);
			log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			if (responsibleCharacter != null)
			{
				log.AddToFillers(responsibleCharacter, responsibleCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			}
			if (p_singleDeathLogFiller != null)
			{
				log.AddToFillers(p_singleDeathLogFiller);
			}
			if (multipleDeathLogFillers != null)
			{
				for (int j = 0; j < multipleDeathLogFillers.Length; j++)
				{
					log.AddToFillers(multipleDeathLogFillers[j]);
				}
			}
			log.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFrom(character, log);
			character.SetDeathLog(log);
			LogPool.Release(log);
		}
		else
		{
			character.SetDeathLog(_deathLog);
		}
		List<Trait> traitOverrideFunctions2 = character.traitContainer.GetTraitOverrideFunctions("After_Death");
		if (traitOverrideFunctions2 != null)
		{
			for (int k = 0; k < traitOverrideFunctions2.Count; k++)
			{
				traitOverrideFunctions2[k].AfterDeath(character);
			}
		}
		Unsummon();
		GameManager.Instance.CreateParticleEffectAt(gridTileLocation, PARTICLE_EFFECT.Minion_Dissipate);
		Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, (IPlayerActionTarget)character);
	}

	private void OnTickEnded()
	{
		if (!character.isDead)
		{
			character.interruptComponent.OnTickEnded();
			character.stateComponent.OnTickEnded();
			character.ProcessTraitsOnTickEnded();
			character.TryProcessTraitsOnTickEndedWhileStationaryOrUnoccupied();
			character.EndTickPerformJobs();
		}
	}

	private void OnTickStarted()
	{
		if (character.isDead)
		{
			return;
		}
		character.ProcessTraitsOnTickStarted();
		if (character.CanPlanGoap())
		{
			character.PerStartTickActionPlanning();
			return;
		}
		ActualGoapNode currentActionNode = character.currentActionNode;
		if (currentActionNode != null && currentActionNode.hasStartedPerTickEffect)
		{
			currentActionNode.PerTickEffect();
		}
	}

	public void SetMinionPlayerSkillType(PLAYER_SKILL_TYPE skillType)
	{
		minionPlayerSkillType = skillType;
	}

	public void SetMinionType(MINION_TYPE p_type)
	{
		minionType = p_type;
	}

	public string GetMinionClassName(PLAYER_SKILL_TYPE skillType)
	{
		return skillType switch
		{
			PLAYER_SKILL_TYPE.DEMON_WRATH => "Wrath", 
			PLAYER_SKILL_TYPE.DEMON_ENVY => "Envy", 
			PLAYER_SKILL_TYPE.DEMON_GLUTTONY => "Gluttony", 
			PLAYER_SKILL_TYPE.DEMON_GREED => "Greed", 
			PLAYER_SKILL_TYPE.DEMON_LUST => "Lust", 
			PLAYER_SKILL_TYPE.DEMON_PRIDE => "Pride", 
			PLAYER_SKILL_TYPE.DEMON_SLOTH => "Sloth", 
			_ => "Wrath", 
		};
	}

	public void Summon(LocationGridTile tile)
	{
		character.CreateMarker();
		character.marker.visionColliderComponent.VoteToUnFilterVision();
		character.ConstructInitialGoapAdvertisementActions();
		character.marker.InitialPlaceMarkerAt(tile);
		character.SetIsDead(isDead: false);
		character.behaviourComponent.OnSummon(tile);
		character.SubscribeToPermanentSignals();
		SubscribeListeners();
		SetIsSummoned(state: true);
		Messenger.Broadcast(PlayerSkillSignals.SUMMON_MINION, this);
	}

	private void Unsummon()
	{
		character.UnsubscribeFromPermanentSignals();
		UnSubscribeListeners();
		SetIsSummoned(state: false);
		character.MigrateHomeStructureTo(null);
		character.currentRegion?.RemoveCharacterFromLocation(character);
		character.homeRegion?.RemoveResident(character);
		character.jobQueue.CancelAllJobs();
		character.interruptComponent.ForceEndNonSimultaneousInterrupt();
		character.combatComponent.ClearAvoidInRange(processCombatBehavior: false);
		character.combatComponent.ClearHostilesInRange(processCombatBehavior: false);
		Messenger.Broadcast(PlayerSkillSignals.UNSUMMON_MINION, this);
		SkillData minionPlayerSkillData = PlayerSkillManager.Instance.GetMinionPlayerSkillData(minionPlayerSkillType);
		float cooldownSpeedModification = WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCooldownSpeedModification();
		int num = Mathf.CeilToInt((float)GameManager.Instance.GetTicksBasedOnHour(6) * cooldownSpeedModification);
		if (num > 0)
		{
			minionPlayerSkillData.SetCooldown(num);
			minionPlayerSkillData.StartCooldown();
		}
		else if (WorldSettings.Instance.worldSettingsData.playerSkillSettings.chargeAmount == SKILL_CHARGE_AMOUNT.Unlimited)
		{
			PlayerManager.Instance.player.underlingsComponent.TrySpawnMissingDemons(minionType);
		}
		else if (minionPlayerSkillData.hasCharges && minionPlayerSkillData.charges < minionPlayerSkillData.maxCharges)
		{
			minionPlayerSkillData.AdjustCharges(1);
		}
	}

	public void OnSeize()
	{
		Messenger.RemoveListener(Signals.TICK_ENDED, OnTickEnded);
		Messenger.RemoveListener(Signals.TICK_STARTED, OnTickStarted);
	}

	public void OnUnseize()
	{
		Messenger.AddListener(Signals.TICK_ENDED, OnTickEnded);
		Messenger.AddListener(Signals.TICK_STARTED, OnTickStarted);
	}

	public void SetIsSummoned(bool state)
	{
		isSummoned = state;
	}

	private void SubscribeListeners()
	{
		Messenger.AddListener(Signals.TICK_ENDED, OnTickEnded);
		Messenger.AddListener(Signals.TICK_STARTED, OnTickStarted);
		Messenger.AddListener<IPointOfInterest, string>(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, character.ForceCancelAllJobsTargetingPOI);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_PERFORM_AGAIN, OnCharacterCanPerformAgain);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
		Messenger.AddListener<CHARACTER_CATEGORY, PRIMORDIAL_STATS_BONUS>(PlayerSkillSignals.ON_PRIMORDIAL_POOL_UPGRADE, OnUpgradeFromPrimordialActivated);
		Messenger.AddListener<IPointOfInterest>(CharacterSignals.ON_SEIZE_POI, OnSeizePOI);
		Messenger.AddListener<NPCSettlement>(SettlementSignals.DISCONNECT_FROM_SETTLEMENT, character.DisconnectFromSettlement);
		Messenger.AddListener<LocationStructure>(StructureSignals.DISCONNECT_FROM_STRUCTURE, character.DisconnectFromStructure);
		Messenger.AddListener<LocationStructure>(StructureSignals.BEFORE_STRUCTURE_DESTROYED, OnBeforeStructureDestroyed);
		character.religionComponent.SubscribeListeners();
		character.previousCharacterDataComponent.SubscribeToListeners();
	}

	private void UnSubscribeListeners()
	{
		Messenger.RemoveListener(Signals.TICK_ENDED, OnTickEnded);
		Messenger.RemoveListener(Signals.TICK_STARTED, OnTickStarted);
		Messenger.RemoveListener<IPointOfInterest, string>(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, character.ForceCancelAllJobsTargetingPOI);
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_PERFORM_AGAIN, OnCharacterCanPerformAgain);
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
		Messenger.RemoveListener<CHARACTER_CATEGORY, PRIMORDIAL_STATS_BONUS>(PlayerSkillSignals.ON_PRIMORDIAL_POOL_UPGRADE, OnUpgradeFromPrimordialActivated);
		Messenger.RemoveListener<IPointOfInterest>(CharacterSignals.ON_SEIZE_POI, OnSeizePOI);
		Messenger.RemoveListener<NPCSettlement>(SettlementSignals.DISCONNECT_FROM_SETTLEMENT, character.DisconnectFromSettlement);
		Messenger.RemoveListener<LocationStructure>(StructureSignals.DISCONNECT_FROM_STRUCTURE, character.DisconnectFromStructure);
		Messenger.RemoveListener<LocationStructure>(StructureSignals.BEFORE_STRUCTURE_DESTROYED, OnBeforeStructureDestroyed);
		character.religionComponent.UnsubscribeListeners();
		character.previousCharacterDataComponent.UnsubscribeToListeners();
	}

	private void OnCharacterCanPerformAgain(Character character)
	{
		if (character == this.character)
		{
			for (int i = 0; i < character.marker.inVisionPOIs.Count; i++)
			{
				IPointOfInterest poi = character.marker.inVisionPOIs[i];
				character.marker.AddUnprocessedPOI(poi);
			}
		}
	}

	private void OnCharacterCanNoLongerPerform(Character character)
	{
		if (character != this.character || character.isDead)
		{
			return;
		}
		if (!character.interruptComponent.isInterrupted || (character.interruptComponent.currentInterrupt.interrupt.type != INTERRUPT.Narcoleptic_Nap && character.interruptComponent.currentInterrupt.interrupt.type != INTERRUPT.Narcoleptic_Nap_Short && character.interruptComponent.currentInterrupt.interrupt.type != INTERRUPT.Narcoleptic_Nap_Medium && character.interruptComponent.currentInterrupt.interrupt.type != INTERRUPT.Narcoleptic_Nap_Long))
		{
			if (character.currentActionNode != null && character.currentActionNode.actionStatus == ACTION_STATUS.PERFORMING && character.currentActionNode.action.goapType.IsRestingAction())
			{
				character.CancelAllJobsExceptForCurrent();
			}
			else
			{
				character.jobQueue.CancelAllJobs();
			}
		}
		if ((bool)character.marker)
		{
			character.marker.StopMovement();
			character.marker.pathfindingAI.ClearAllCurrentPathData();
		}
		character.reactionComponent.SetIsHidden(state: false);
		character.UncarryPOI();
		if (character.traitContainer.HasTrait("Unconscious"))
		{
			character.ForceCancelAllJobsTargetingThisCharacter(JOB_TYPE.KNOCKOUT);
		}
		character.traitContainer.RemoveTrait(character, "Polymorphed");
		Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, (IPlayerActionTarget)this.character);
	}

	private void OnSeizePOI(IPointOfInterest poi)
	{
		character?.OnSeizePOI(poi);
	}

	private void OnBeforeStructureDestroyed(LocationStructure structure)
	{
		if (character != null)
		{
			character.OnBeforeStructureDestroyed(structure);
		}
	}

	private void OnUpgradeFromPrimordialActivated(CHARACTER_CATEGORY p_category, PRIMORDIAL_STATS_BONUS p_bonusType)
	{
		if (RaceManager.Instance.GetRaceData(character.race).category == p_category)
		{
			character.combatComponent.LevelUpBaseOnPrimordialBonusUpgrade(p_category, p_bonusType);
		}
	}

	public void CleanUp()
	{
		character = null;
	}
}

using System;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Interrupts;
using Object_Pools;
using Traits;
using UnityEngine.Localization;
using UtilityScripts;

public class Summon : Character
{
	protected string agitatedMessageForDisplay = string.Empty;

	public SUMMON_TYPE summonType { get; }

	private bool showNotificationOnDeath { get; set; }

	public virtual TILE_OBJECT_TYPE produceableMaterial => TILE_OBJECT_TYPE.NONE;

	public virtual SUMMON_TYPE adultSummonType => SUMMON_TYPE.None;

	public virtual COMBAT_MODE defaultCombatMode => COMBAT_MODE.Aggressive;

	public virtual bool defaultDigMode => false;

	public override Type serializedData => typeof(SaveDataSummon);

	public bool isUsingDefaultName => name == base.raceClassName;

	public bool isTamed
	{
		get
		{
			if (base.faction != null)
			{
				if (!base.faction.isMajorNonPlayer)
				{
					return base.faction.factionType.type == FACTION_TYPE.Ratmen;
				}
				return true;
			}
			return false;
		}
	}

	public virtual Faction defaultFaction => FactionManager.Instance.wildMonsterFaction;

	public virtual int gainedKennelSummonCapacity => 3;

	public virtual SUMMON_TYPE gainedKennelSummonType => summonType;

	protected Summon(SUMMON_TYPE summonType, string className, RACE race, GENDER gender)
		: base(className, race, gender, HAIR_COLOR.Brunette)
	{
		this.summonType = summonType;
		showNotificationOnDeath = true;
		base.isInfoUnlocked = true;
		base.isWildMonster = true;
	}

	protected Summon(SaveDataSummon data)
		: base(data)
	{
		summonType = data.summonType;
		showNotificationOnDeath = true;
		base.isInfoUnlocked = true;
		base.isWildMonster = true;
	}

	public override void Initialize()
	{
		base.visuals.Initialize();
		base.combatComponent.SetCombatMode(defaultCombatMode);
		ConstructDefaultPlayerActions();
		OnUpdateRace();
		base.classComponent.OnUpdateCharacterClass();
		base.moodComponent.OnCharacterBecomeMinionOrSummon();
		base.moodComponent.SetMoodValue(50);
		base.needsComponent.Initialize();
		base.moneyComponent.Initialize();
		base.advertisedActions.Clear();
		ConstructInitialGoapAdvertisementActions();
		base.behaviourComponent.UpdateDefaultBehaviourSet();
	}

	public override void OnActionPerformed(ActualGoapNode node)
	{
	}

	public override void Death(string cause = "normal", ActualGoapNode deathFromAction = null, Character responsibleCharacter = null, Log _deathLog = null, LogFiller[] multipleDeathLogFillers = null, LogFiller p_singleDeathLogFiller = null, Interrupt interrupt = null, bool isPlayerSource = false, object deathSource = null, ELEMENTAL_TYPE elementType = ELEMENTAL_TYPE.Normal)
	{
		if (_isDead)
		{
			return;
		}
		if (base.mountComponent.IsBeingMounted())
		{
			base.mountComponent.GetRider()?.mountComponent.Dismount();
		}
		else if (base.mountComponent.IsMounting())
		{
			base.mountComponent.Dismount();
		}
		if (base.gridTileLocation == null || !base.gridTileLocation.IsPassable())
		{
			SetDestroyMarkerOnDeath(state: true);
		}
		base.deathTilePosition = base.gridTileLocation;
		if (base.isBeingSeized)
		{
			PlayerManager.Instance.player.seizeComponent.UnseizePOIOnCharacterDeath();
		}
		SetDeathLocation(base.gridTileLocation);
		if (deathFromAction != null && deathFromAction.action != null)
		{
			base.causeOfDeath = deathFromAction.action.goapType;
		}
		List<Trait> traitOverrideFunctions = base.traitContainer.GetTraitOverrideFunctions("Death_Trait");
		if (traitOverrideFunctions != null)
		{
			for (int i = 0; i < traitOverrideFunctions.Count; i++)
			{
				if (traitOverrideFunctions[i].OnDeath(this))
				{
					i--;
				}
			}
		}
		if (base.isLycanthrope)
		{
			Character originalForm = base.lycanData.originalForm;
			base.lycanData.LycanDies(this, cause, deathFromAction, responsibleCharacter, _deathLog, multipleDeathLogFillers, p_singleDeathLogFiller, deathSource);
			_deathLog = originalForm.deathLog;
			_deathLog.AddInvolvedObjectManual(base.persistentID);
		}
		SetIsDead(isDead: true);
		base.petComponent.OnComponentOwnerDied();
		if (base.isLimboCharacter && base.isInLimbo)
		{
			CharacterManager.Instance.RemoveLimboCharacter(this);
			return;
		}
		base.reactionComponent.SetDisguisedCharacter(null);
		if (responsibleCharacter != null)
		{
			base.reactionComponent.AddCharacterThatSawThisDead(responsibleCharacter);
		}
		UnsubscribeSignals();
		if (base.stateComponent.currentState != null)
		{
			base.stateComponent.ExitCurrentState();
		}
		Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, (IPointOfInterest)this, GoapPlanJob.Target_Already_Dead_Reason);
		Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_ACTIONS_TARGETING_POI, (IPointOfInterest)this, GoapPlanJob.Target_Already_Dead_Reason);
		base.jobQueue.CancelAllJobs();
		DropAllItems(base.deathTilePosition);
		UnownOrTransferOwnershipOfAllItems();
		base.reactionComponent.SetIsHidden(state: false);
		UncarryPOI();
		base.isBeingCarriedBy?.UncarryPOI(this);
		base.previousCharacterDataComponent.SetHomeSettlementOnDeath(base.homeSettlement);
		if (base.homeRegion != null)
		{
			Region region = base.homeRegion;
			base.homeRegion.RemoveResident(this);
			MigrateHomeStructureTo(null, broadcast: true, addToRegionResidents: false);
			SetHomeRegion(region);
		}
		if (base.partyComponent.hasParty)
		{
			base.partyComponent.currentParty.RemoveMember(this);
		}
		base.traitContainer.RemoveAllTraitsAndStatusesByName(this, "Criminal");
		if (base.interruptComponent.isInterrupted && base.interruptComponent.currentInterrupt.interrupt != interrupt)
		{
			base.interruptComponent.ForceEndNonSimultaneousInterrupt();
		}
		base.traitContainer.AddTrait(this, "Dead", responsibleCharacter);
		if (deathFromAction != null)
		{
			base.traitContainer.GetTraitOrStatus<Trait>("Dead")?.SetGainedFromDoingAction(deathFromAction.action.goapType, deathFromAction.isStealth);
		}
		if (cause == "attacked" && responsibleCharacter != null && responsibleCharacter.isInWerewolfForm)
		{
			base.traitContainer.AddTrait(this, "Mangled", responsibleCharacter);
			if (deathFromAction != null)
			{
				base.traitContainer.GetTraitOrStatus<Trait>("Mangled")?.SetGainedFromDoingAction(deathFromAction.action.goapType, deathFromAction.isStealth);
			}
		}
		Messenger.Broadcast(CharacterSignals.CHARACTER_DEATH, (Character)this);
		base.eventDispatcher.ExecuteCharacterDied(this);
		base.jobQueue.CancelAllJobs();
		if (_deathLog == null)
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterDeath_Table", "death_" + cause, LOG_TAG.Life_Changes);
			log.AddToFillers(this, name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
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
			if (showNotificationOnDeath)
			{
				PlayerManager.Instance.player.ShowNotificationFrom(this, log);
			}
			SetDeathLog(log);
			LogPool.Release(log);
		}
		else
		{
			SetDeathLog(_deathLog);
		}
		base.wasKilledByPlayerSource = isPlayerSource;
		AfterDeath(base.deathTilePosition);
		if (base.hasMarker)
		{
			base.marker.OnDeath(base.deathTilePosition);
		}
		Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, (IPlayerActionTarget)this);
		if (base.faction != null)
		{
			base.faction.ideologyComponent.OnFactionMemberDied(this);
			if (base.faction.isPlayerFaction)
			{
				PlayerManager.Instance.player.underlingsComponent.DecreaseMonsterUnderlingCharge(summonType);
			}
		}
	}

	protected override void OnTickStarted()
	{
		if (!base.isDead)
		{
			base.needsComponent.PerTickSummon();
			ProcessTraitsOnTickStarted();
			StartTickGoapPlanGeneration();
		}
	}

	public override void OnUnseizePOI(LocationGridTile tileLocation)
	{
		base.OnUnseizePOI(tileLocation);
	}

	public override void LoadReferences(SaveDataCharacter data)
	{
		base.LoadReferences(data);
		base.movementComponent.UpdateSpeed();
	}

	private void OnUpgradeFromPrimordialActivated(CHARACTER_CATEGORY p_category, PRIMORDIAL_STATS_BONUS p_bonusType)
	{
		if (RaceManager.Instance.GetRaceData(base.race).category == p_category)
		{
			base.combatComponent.LevelUpBaseOnPrimordialBonusUpgrade(p_category, p_bonusType);
		}
	}

	public override void SubscribeToSignals()
	{
		if (!base.hasSubscribedToSignals)
		{
			base.SubscribeToSignals();
			Messenger.AddListener<CHARACTER_CATEGORY, PRIMORDIAL_STATS_BONUS>(PlayerSkillSignals.ON_PRIMORDIAL_POOL_UPGRADE, OnUpgradeFromPrimordialActivated);
		}
	}

	public override void UnsubscribeSignals()
	{
		if (base.hasSubscribedToSignals)
		{
			base.UnsubscribeSignals();
			Messenger.RemoveListener<CHARACTER_CATEGORY, PRIMORDIAL_STATS_BONUS>(PlayerSkillSignals.ON_PRIMORDIAL_POOL_UPGRADE, OnUpgradeFromPrimordialActivated);
		}
	}

	public virtual void OnPlaceSummon(LocationGridTile tile)
	{
		SubscribeToSignals();
		base.movementComponent.UpdateSpeed();
		base.behaviourComponent.OnSummon(tile);
	}

	protected virtual void AfterDeath(LocationGridTile deathTileLocation)
	{
		if (base.marker == null && base.destroyMarkerOnDeath && deathTileLocation != null)
		{
			if (base.race == RACE.TRITON)
			{
				GameManager.Instance.CreateParticleEffectAt(deathTileLocation, PARTICLE_EFFECT.Water_Bomb);
			}
			else
			{
				GameManager.Instance.CreateParticleEffectAt(deathTileLocation, PARTICLE_EFFECT.Minion_Dissipate);
			}
		}
		List<Trait> traitOverrideFunctions = base.traitContainer.GetTraitOverrideFunctions("After_Death");
		if (traitOverrideFunctions != null)
		{
			for (int i = 0; i < traitOverrideFunctions.Count; i++)
			{
				traitOverrideFunctions[i].AfterDeath(this);
			}
		}
	}

	public virtual void OnSummonAsPlayerMonster()
	{
		base.combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
	}

	public override void ConstructDefaultPlayerActions(bool broadcastSignal = true)
	{
		if (base.actions == null)
		{
			base.actions = new List<PLAYER_SKILL_TYPE>();
		}
		else
		{
			base.actions.Clear();
		}
		AddPlayerAction(PLAYER_SKILL_TYPE.SEIZE_MONSTER, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.AGITATE, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.SACRIFICE, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.RELEASE, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.HEAL, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.EXPEL, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.LET_GO, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.FULL_HEAL, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.SNATCH_MONSTER, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.FINGER_OF_DEATH, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.HELLSPAWN, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.TRIGGER_AROUSAL, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.TRIGGER_GRUDGE, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.UNDEPLOY_PARTY, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.SNATCH_VILLAGER, broadcastSignal);
	}

	public override bool IsCurrentlySelected()
	{
		Character character = this;
		if (base.isLycanthrope)
		{
			character = base.lycanData.activeForm;
		}
		if (UIManager.Instance.monsterInfoUI.isShowing)
		{
			return UIManager.Instance.monsterInfoUI.activeMonster == character;
		}
		return false;
	}

	public void SetShowNotificationOnDeath(bool showNotificationOnDeath)
	{
		this.showNotificationOnDeath = showNotificationOnDeath;
	}

	public virtual void OnMonsterCreatedForInitialWorldGeneration()
	{
	}

	public virtual bool Agitate(ref JobQueueItem p_agitateJob)
	{
		return AgitateAttackNearbyVillager(ref p_agitateJob);
	}

	protected virtual string GetAgitateTooltipKey()
	{
		return base.classComponent.characterClass.className + " " + AGITATE_MESSAGE_TYPE.Tooltip.ToStringEnum();
	}

	public virtual void OnAgitatedSuccessfully()
	{
	}

	protected bool AgitateAttackNearbyVillager(ref JobQueueItem p_agitateJob)
	{
		if (base.limiterComponent.IsIncapacitated())
		{
			CreateAgitateLog(AGITATE_MESSAGE_TYPE.Incapacitated);
			return false;
		}
		Area area = base.areaLocation;
		if (area != null)
		{
			Character character = null;
			List<Area> list = RuinarchListPool<Area>.Claim();
			List<Character> list2 = RuinarchListPool<Character>.Claim();
			area.PopulateAreasInRange(list, 6, includeCenterTile: true);
			for (int i = 0; i < list.Count; i++)
			{
				Character randomCharacterInsideAreaThatIsAliveVillagerAndNotInPrisonOrKennel = list[i].locationCharacterTracker.GetRandomCharacterInsideAreaThatIsAliveVillagerAndNotInPrisonOrKennel();
				if (randomCharacterInsideAreaThatIsAliveVillagerAndNotInPrisonOrKennel != null)
				{
					list2.Add(randomCharacterInsideAreaThatIsAliveVillagerAndNotInPrisonOrKennel);
				}
			}
			if (list2.Count > 0)
			{
				character = list2[GameUtilities.RandomBetweenTwoNumbers(0, list2.Count - 1)];
			}
			RuinarchListPool<Area>.Release(list);
			RuinarchListPool<Character>.Release(list2);
			if (character != null)
			{
				base.jobComponent.TriggerAttackVillager(JOB_TYPE.AGITATED, character, out p_agitateJob);
				if (p_agitateJob is GoapPlanJob goapPlanJob)
				{
					goapPlanJob.SetIsAgitateJob(p_state: true);
					CreateAgitateLog(AGITATE_MESSAGE_TYPE.Agitate_Success);
					return true;
				}
			}
			else
			{
				CreateAgitateLog(AGITATE_MESSAGE_TYPE.Attack_Villager_No_Target);
			}
		}
		return false;
	}

	public void CreateAgitateLog(AGITATE_MESSAGE_TYPE p_messageType)
	{
		LocationStructure locationStructure = null;
		NPCSettlement nPCSettlement = null;
		string text = p_messageType.ToStringEnum();
		switch (p_messageType)
		{
		case AGITATE_MESSAGE_TYPE.Special_1:
			text = base.classComponent.characterClass.className + " " + p_messageType.ToStringEnum();
			if (summonType == SUMMON_TYPE.Revenant)
			{
				if (base.homeStructure != null)
				{
					locationStructure = base.homeStructure;
				}
				if (locationStructure == null && nPCSettlement == null)
				{
					nPCSettlement = base.homeSettlement;
				}
				if (locationStructure == null && nPCSettlement == null)
				{
					text = base.classComponent.characterClass.className + " " + AGITATE_MESSAGE_TYPE.Special_2.ToStringEnum();
				}
			}
			break;
		case AGITATE_MESSAGE_TYPE.Special_2:
		case AGITATE_MESSAGE_TYPE.Tooltip:
			text = base.classComponent.characterClass.className + " " + p_messageType.ToStringEnum();
			break;
		}
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "MonsterAgitate_Table", "MonsterAgitate_Table", text, LOG_TAG.Major);
		log.AddToFillers(this, name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		if (locationStructure != null)
		{
			log.AddToFillers(locationStructure, locationStructure.GetNameRelativeTo(this), LOG_IDENTIFIER.LANDMARK_1, replaceExisting: true, overrideStringValue: true);
		}
		else if (nPCSettlement != null)
		{
			log.AddToFillers(locationStructure, nPCSettlement.name, LOG_IDENTIFIER.LANDMARK_1);
		}
		log.AddLogToDatabase();
		if (text == "Agitate_Success")
		{
			PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
			return;
		}
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Agitate_Failed");
		PlayerUI.Instance.ShowGeneralConfirmation(localizedValue, log.logText);
	}

	public string GetAgitateToolTipDescription(ref bool canBeAgitated)
	{
		_ = string.Empty;
		if (base.traitContainer.HasTrait("Agitated"))
		{
			return LocalizationManager.Instance.GetLocalizedValue("MonsterAgitate_Table", "Cant_Agitate_Agitated");
		}
		if (base.characterClass.cantBeAgitated)
		{
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("MonsterAgitate_Table", "No Tooltip");
			canBeAgitated = false;
			return localizedValue;
		}
		return LocalizationManager.Instance.GetLocalizedValue("MonsterAgitate_Table", GetAgitateTooltipKey());
	}

	public string GetAgitateIncapacitatedMessage()
	{
		return Utilities.LogReplacer(LocalizationManager.Instance.GetLocalizedValue("MonsterAgitate_Table", AGITATE_MESSAGE_TYPE.Incapacitated.ToStringEnum()), ObjectPoolManager.Instance.CreateNewLogFiller(this, LOG_IDENTIFIER.ACTIVE_CHARACTER));
	}

	public virtual bool ReactionToAnotherCharacter(Character actor, Character targetCharacter, Character disguisedActor, Character disguisedTarget, bool isHostile, ref string debugLog)
	{
		return false;
	}

	public override void OnJobAddedToCharacterJobQueue(JobQueueItem job, Character character)
	{
		base.OnJobAddedToCharacterJobQueue(job, character);
		if (character == this && (job.jobType == JOB_TYPE.IDLE_RETURN_HOME || job.jobType == JOB_TYPE.IDLE_RETURN_HOME_HIGHER))
		{
			base.combatComponent.SetCombatMode(COMBAT_MODE.Defend);
		}
	}

	public override void OnJobRemovedFromCharacterJobQueue(JobQueueItem job, Character character, bool shouldBlacklist = false)
	{
		if (character == this && (job.jobType == JOB_TYPE.IDLE_RETURN_HOME || job.jobType == JOB_TYPE.IDLE_RETURN_HOME_HIGHER))
		{
			base.combatComponent.SetCombatMode(base.combatComponent.previousCombatMode);
		}
		base.OnJobRemovedFromCharacterJobQueue(job, character, shouldBlacklist);
	}

	public override void OnLocaleChanged(Locale locale)
	{
		RenameCharacter(base.raceClassName);
	}
}

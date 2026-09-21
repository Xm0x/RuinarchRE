using System;
using System.Collections.Generic;
using Character_Talents;
using Characters.Behaviour;
using Characters.Components;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Traits;
using UtilityScripts;

public class BehaviourComponent : CharacterComponent, CharacterEventDispatcher.ILocationListener
{
	private readonly int _deMoodCooldownPeriod;

	private readonly int _disableCooldownPeriod;

	private readonly int _arsonCooldownPeriod;

	public bool isCurrentlySnatching;

	private string _visitVillagetEndDateKey;

	public int criticalBreakFiresCreated;

	public List<CharacterBehaviour> currentBehaviourComponents { get; private set; }

	public NPCSettlement attackVillageTarget { get; private set; }

	public Area attackAreaTarget { get; private set; }

	public STRUCTURE_TYPE wildernessMonsterSpawnerStructureType { get; private set; }

	public bool hasBuiltWildernessMonsterSpawnerStructure { get; private set; }

	public string monsterSpawnerID { get; private set; }

	public bool isAttackingDemonicStructure { get; private set; }

	public bool hasLayedAnEgg { get; private set; }

	public bool subterraneanJustExitedCombat { get; private set; }

	public string defaultBehaviourSetName { get; private set; }

	public COMBAT_MODE combatModeBeforeAttackVillageBehaviour { get; private set; }

	public COMBAT_MODE combatModeBeforePatrolling { get; private set; }

	public NPCSettlement dousingFireForSettlement { get; private set; }

	public NPCSettlement cleansingTilesForSettlement { get; private set; }

	public string settlementTargetForPurifyGround { get; private set; }

	public int numberOfPurifiedGrounds { get; private set; }

	public Character currentAbductTarget { get; private set; }

	public int currentDeMoodCooldown { get; private set; }

	public List<Area> deMoodVillageTarget { get; private set; }

	public List<Area> invadeVillageTarget { get; private set; }

	public int followerCount { get; private set; }

	public int currentDisableCooldown { get; private set; }

	public LocationGridTile nest { get; private set; }

	public bool hasEatenInTheMorning { get; private set; }

	public bool hasEatenInTheNight { get; private set; }

	public bool canArson => currentArsonCooldown >= _arsonCooldownPeriod;

	public int currentArsonCooldown { get; private set; }

	public List<Area> arsonVillageTarget { get; private set; }

	public Area abominationTarget { get; private set; }

	public BaseSettlement pestSettlementTarget { get; private set; }

	public bool pestHasFailedEat { get; private set; }

	public DemonicStructure attackDemonicStructureTarget { get; private set; }

	public COMBAT_MODE combatModeBeforeAttackingDemonicStructure { get; private set; }

	public LocationStructure targetSocializeStructure { get; private set; }

	public GameDate socializingEndTime { get; private set; }

	public NPCSettlement targetVisitVillage { get; private set; }

	public LocationStructure targetVisitVillageStructure { get; private set; }

	public GameDate visitVillageEndTime { get; private set; }

	public VISIT_VILLAGE_INTENT visitVillageIntent { get; private set; }

	public bool shouldTryToBuildNewVillage { get; private set; }

	public VillageSpot chosenVillageSpotForNewVillage { get; private set; }

	public int tendedCropsForToday { get; private set; }

	public bool canFlirtOnActivePartyQuest { get; private set; }

	public LocationStructure criticalBreakDestroyStructureTarget { get; private set; }

	public Character criticalBreakKillTarget { get; private set; }

	public BehaviourComponent()
	{
		deMoodVillageTarget = new List<Area>();
		invadeVillageTarget = new List<Area>();
		arsonVillageTarget = new List<Area>();
		defaultBehaviourSetName = string.Empty;
		currentBehaviourComponents = new List<CharacterBehaviour>();
		wildernessMonsterSpawnerStructureType = STRUCTURE_TYPE.NONE;
		_deMoodCooldownPeriod = 40;
		currentDeMoodCooldown = _deMoodCooldownPeriod;
		_disableCooldownPeriod = 40;
		currentDisableCooldown = _disableCooldownPeriod;
		_arsonCooldownPeriod = 40;
		currentArsonCooldown = _arsonCooldownPeriod;
		PopulateInitialBehaviourComponents();
	}

	public BehaviourComponent(SaveDataBehaviourComponent data)
	{
		deMoodVillageTarget = new List<Area>();
		invadeVillageTarget = new List<Area>();
		arsonVillageTarget = new List<Area>();
		_deMoodCooldownPeriod = 40;
		_disableCooldownPeriod = 40;
		_arsonCooldownPeriod = 40;
		currentBehaviourComponents = new List<CharacterBehaviour>();
		for (int i = 0; i < data.currentBehaviourComponents.Count; i++)
		{
			Type type = Type.GetType(data.currentBehaviourComponents[i]);
			CharacterBehaviour characterBehaviourComponent = CharacterManager.Instance.GetCharacterBehaviourComponent(type);
			AddBehaviourComponentFromSave(characterBehaviourComponent);
		}
		isAttackingDemonicStructure = data.isAttackingDemonicStructure;
		hasLayedAnEgg = data.hasLayedAnEgg;
		subterraneanJustExitedCombat = data.subterraneanJustExitedCombat;
		defaultBehaviourSetName = data.defaultBehaviourSetName;
		currentDeMoodCooldown = data.currentDeMoodCooldown;
		followerCount = data.followerCount;
		currentDisableCooldown = data.currentDisableCooldown;
		currentArsonCooldown = data.currentArsonCooldown;
		hasEatenInTheMorning = data.hasEatenInTheMorning;
		hasEatenInTheNight = data.hasEatenInTheNight;
		isCurrentlySnatching = data.isCurrentlySnatching;
		combatModeBeforeAttackingDemonicStructure = data.combatModeBeforeAttackingDemonicStructure;
		pestHasFailedEat = data.pestHasFailedEat;
		shouldTryToBuildNewVillage = data.shouldTryToBuildNewVillage;
		combatModeBeforeAttackVillageBehaviour = data.combatModeBeforeAttackVillageBehaviour;
		wildernessMonsterSpawnerStructureType = data.wildernessMonsterSpawnerStructureType;
		hasBuiltWildernessMonsterSpawnerStructure = data.hasBuiltWildernessMonsterSpawnerStructure;
		monsterSpawnerID = data.monsterSpawnerID;
		combatModeBeforePatrolling = data.combatModeBeforePatrolling;
		settlementTargetForPurifyGround = data.settlementTargetForPurifyGround;
		numberOfPurifiedGrounds = data.numberOfPurifiedGrounds;
		tendedCropsForToday = data.tendedCropsForToday;
		canFlirtOnActivePartyQuest = data.canFlirtOnActivePartyQuest;
		criticalBreakFiresCreated = data.criticalBreakFiresCreated;
	}

	public void PopulateInitialBehaviourComponents()
	{
		ChangeDefaultBehaviourSet("Default Resident Behaviour");
	}

	private bool AddBehaviourComponent(CharacterBehaviour component)
	{
		if (component == null)
		{
			throw new Exception(GameManager.Instance.TodayLogString() + base.owner.name + " is trying to add a new behaviour component but it is null!");
		}
		if (HasBehaviour(component.GetType()))
		{
			return false;
		}
		bool num = AddBehaviourComponentInOrder(component);
		if (num)
		{
			component.OnAddBehaviourToCharacter(base.owner);
		}
		return num;
	}

	private bool AddBehaviourComponentFromSave(CharacterBehaviour component)
	{
		if (component == null)
		{
			throw new Exception(GameManager.Instance.TodayLogString() + base.owner.name + " is trying to add a new behaviour component but it is null!");
		}
		if (HasBehaviour(component.GetType()))
		{
			return false;
		}
		return AddBehaviourComponentInOrder(component);
	}

	public bool AddBehaviourComponent(Type componentType)
	{
		return AddBehaviourComponent(CharacterManager.Instance.GetCharacterBehaviourComponent(componentType));
	}

	private bool RemoveBehaviourComponent(CharacterBehaviour component)
	{
		bool num = currentBehaviourComponents.Remove(component);
		if (num)
		{
			component.OnRemoveBehaviourFromCharacter(base.owner);
			Messenger.Broadcast(CharacterSignals.CHARACTER_REMOVED_BEHAVIOUR, base.owner, component);
		}
		return num;
	}

	public bool RemoveBehaviourComponent(Type componentType)
	{
		return RemoveBehaviourComponent(CharacterManager.Instance.GetCharacterBehaviourComponent(componentType));
	}

	public bool ReplaceBehaviourComponent(CharacterBehaviour behaviourToBeReplaced, CharacterBehaviour behaviourToReplace)
	{
		if (RemoveBehaviourComponent(behaviourToBeReplaced))
		{
			return AddBehaviourComponent(behaviourToReplace);
		}
		return false;
	}

	public bool ReplaceBehaviourComponent(Type componentToBeReplaced, Type componentToReplace)
	{
		if (RemoveBehaviourComponent(componentToBeReplaced))
		{
			return AddBehaviourComponent(componentToReplace);
		}
		return false;
	}

	private bool AddBehaviourComponentInOrder(CharacterBehaviour component)
	{
		if (currentBehaviourComponents.Count > 0)
		{
			for (int i = 0; i < currentBehaviourComponents.Count; i++)
			{
				if (component.priority > currentBehaviourComponents[i].priority)
				{
					currentBehaviourComponents.Insert(i, component);
					return true;
				}
			}
		}
		currentBehaviourComponents.Add(component);
		return true;
	}

	public void ChangeDefaultBehaviourSet(string setName)
	{
		if (defaultBehaviourSetName != setName)
		{
			RemoveDefaultBehaviourSet(defaultBehaviourSetName);
			AddDefaultBehaviourSet(setName);
			defaultBehaviourSetName = setName;
		}
	}

	private void AddDefaultBehaviourSet(string setName)
	{
		Type[] defaultBehaviourSet = CharacterManager.Instance.GetDefaultBehaviourSet(setName);
		if (defaultBehaviourSet != null)
		{
			for (int i = 0; i < defaultBehaviourSet.Length; i++)
			{
				AddBehaviourComponent(defaultBehaviourSet[i]);
			}
		}
	}

	private void RemoveDefaultBehaviourSet(string setName)
	{
		Type[] defaultBehaviourSet = CharacterManager.Instance.GetDefaultBehaviourSet(setName);
		if (defaultBehaviourSet != null)
		{
			for (int i = 0; i < defaultBehaviourSet.Length; i++)
			{
				RemoveBehaviourComponent(defaultBehaviourSet[i]);
			}
		}
	}

	public void UpdateDefaultBehaviourSet()
	{
		base.owner.SetIsWanderer(state: false);
		if (base.owner.traitContainer.HasTrait("Enslaved"))
		{
			ChangeDefaultBehaviourSet("Enslaved Behaviour");
			return;
		}
		if (base.owner.petComponent.petOwner != null)
		{
			ChangeDefaultBehaviourSet("Pet Behaviour");
			return;
		}
		Faction faction = base.owner.faction;
		if (faction != null && faction.factionType.type == FACTION_TYPE.Bandits)
		{
			ChangeDefaultBehaviourSet("Bandit Behaviour");
			return;
		}
		if (HasBehaviour(typeof(SettlementProtectorBehaviour)))
		{
			ChangeDefaultBehaviourSet("Settlement Protector Behaviour");
			return;
		}
		string traitBehaviourSetOf = CharacterManager.Instance.GetTraitBehaviourSetOf(base.owner);
		if (!string.IsNullOrEmpty(traitBehaviourSetOf) && CharacterManager.Instance.HasDefaultBehaviourSet(traitBehaviourSetOf))
		{
			ChangeDefaultBehaviourSet(traitBehaviourSetOf);
			return;
		}
		if ((base.owner.isNormalCharacter && !base.owner.isConsideredRatman) || base.owner.characterClass.IsZombie())
		{
			if (base.owner.homeSettlement != null)
			{
				base.owner.SetIsWanderer(state: false);
			}
			else
			{
				base.owner.SetIsWanderer(state: true);
			}
			return;
		}
		if (base.owner.minion != null)
		{
			if (base.owner.faction != null && base.owner.faction.isMajorNonPlayer)
			{
				ChangeDefaultBehaviourSet("Default Monster Behaviour");
			}
			else
			{
				ChangeDefaultBehaviourSet("Default Minion Behaviour");
			}
			return;
		}
		if (base.owner.race == RACE.ANGEL)
		{
			ChangeDefaultBehaviourSet("Default Angel Behaviour");
			return;
		}
		if (base.owner.race == RACE.ENT)
		{
			ChangeDefaultBehaviourSet("Ent Behaviour");
			return;
		}
		string setName = base.owner.characterClass.className + " Behaviour";
		if (CharacterManager.Instance.HasDefaultBehaviourSet(setName))
		{
			ChangeDefaultBehaviourSet(setName);
		}
		else
		{
			ChangeDefaultBehaviourSet("Default Monster Behaviour");
		}
	}

	public void SetCombatModeBeforeAttackVillageBehaviour(COMBAT_MODE p_combatMode)
	{
		combatModeBeforeAttackVillageBehaviour = p_combatMode;
	}

	public void SetWildernessMonsterSpawnerStructureType(STRUCTURE_TYPE p_structureType)
	{
		wildernessMonsterSpawnerStructureType = p_structureType;
	}

	public void SetHasBuiltWildernessMonsterSpawnerStructure(bool p_state)
	{
		hasBuiltWildernessMonsterSpawnerStructure = p_state;
	}

	public void SetMonsterSpawnerID(string p_id)
	{
		monsterSpawnerID = p_id;
	}

	public void SetCombatModeBeforePatrolling(COMBAT_MODE p_combatMode)
	{
		combatModeBeforePatrolling = p_combatMode;
	}

	public void OnOwnerDied()
	{
		ClearOutSocializingBehaviour();
		ClearOutVisitVillageBehaviour();
	}

	public void DisconnectFromStructure(LocationStructure p_structure)
	{
		if (targetSocializeStructure == p_structure)
		{
			ClearOutSocializingBehaviour();
		}
		if (targetVisitVillageStructure == p_structure)
		{
			ClearOutVisitVillageBehaviour();
		}
		if (attackDemonicStructureTarget == p_structure)
		{
			SetIsAttackingDemonicStructure(state: false, null);
		}
		if (criticalBreakDestroyStructureTarget == p_structure)
		{
			criticalBreakDestroyStructureTarget = null;
		}
	}

	public void DisconnectFromSettlement(NPCSettlement p_settlement)
	{
		if (targetVisitVillage == p_settlement)
		{
			ClearOutVisitVillageBehaviour();
		}
		if (attackVillageTarget == p_settlement)
		{
			ClearAttackVillageData();
		}
	}

	public void DisconnectFromCharacter(Character p_character)
	{
		if (currentAbductTarget == p_character)
		{
			SetAbductionTarget(null);
		}
		if (criticalBreakKillTarget == p_character)
		{
			criticalBreakKillTarget = null;
		}
	}

	public void DailyGoapProcesses()
	{
		ClearTendedAmount();
	}

	public void PopulateVillageTargetsByPriority(List<Area> areas)
	{
		List<BaseSettlement> list = RuinarchListPool<BaseSettlement>.Claim();
		base.owner.currentRegion?.PopulateSettlementsInRegionForGettingGeneralVillageTargets(list);
		if (list.Count > 0)
		{
			List<BaseSettlement> list2 = RuinarchListPool<BaseSettlement>.Claim();
			list.PopulateSettlementsThatAreUnownedOrHostileWithFaction(list2, LOCATION_TYPE.VILLAGE, PlayerManager.Instance.player.playerFaction);
			if (list2 != null)
			{
				BaseSettlement randomElement = CollectionUtilities.GetRandomElement(list2);
				for (int i = 0; i < randomElement.areas.Count; i++)
				{
					areas.Add(randomElement.areas[i]);
				}
			}
			else
			{
				List<BaseSettlement> list3 = RuinarchListPool<BaseSettlement>.Claim();
				list.PopulateSettlementsThatAreUnownedOrHostileWithFaction(list3, LOCATION_TYPE.DUNGEON, PlayerManager.Instance.player.playerFaction);
				if (list3 != null)
				{
					BaseSettlement randomElement2 = CollectionUtilities.GetRandomElement(list3);
					for (int j = 0; j < randomElement2.areas.Count; j++)
					{
						areas.Add(randomElement2.areas[j]);
					}
				}
				RuinarchListPool<BaseSettlement>.Release(list3);
			}
			RuinarchListPool<BaseSettlement>.Release(list2);
		}
		RuinarchListPool<BaseSettlement>.Release(list);
		if (areas.Count <= 0)
		{
			List<Area> list4 = RuinarchListPool<Area>.Claim();
			base.owner.currentRegion?.PopulateAreasOccupiedByVillagers(list4);
			if (list4 != null)
			{
				Area randomElement3 = CollectionUtilities.GetRandomElement(list4);
				areas.Add(randomElement3);
			}
			RuinarchListPool<Area>.Release(list4);
		}
	}

	public string RunBehaviour()
	{
		string log = string.Empty;
		List<CharacterBehaviour> list = RuinarchListPool<CharacterBehaviour>.Claim();
		list.AddRange(currentBehaviourComponents);
		for (int i = 0; i < list.Count; i++)
		{
			CharacterBehaviour characterBehaviour = list[i];
			if (!currentBehaviourComponents.Contains(characterBehaviour) || characterBehaviour.IsDisabledFor(base.owner) || !characterBehaviour.CanDoBehaviour(base.owner))
			{
				continue;
			}
			if (characterBehaviour.TryDoBehaviour(base.owner, ref log, out var producedJob))
			{
				bool flag = IsProducedJobValid(producedJob, base.owner);
				if (producedJob == null || flag)
				{
					if (producedJob != null)
					{
						base.owner.jobQueue.AddJobInQueue(producedJob);
					}
					characterBehaviour.PostProcessAfterSuccessfulDoBehaviour(base.owner);
					if (producedJob != null || !characterBehaviour.WillContinueProcess())
					{
						break;
					}
				}
				else if (producedJob.originalOwner != null)
				{
					if (producedJob.originalOwner.ownerType == JOB_OWNER.CHARACTER)
					{
						if (producedJob.originalOwner == base.owner)
						{
							JobManager.Instance.ReleaseJob(producedJob);
						}
					}
					else
					{
						producedJob.AddBlacklistedCharacter(base.owner);
					}
				}
			}
			if (characterBehaviour.StopsBehaviourLoop())
			{
				break;
			}
		}
		RuinarchListPool<CharacterBehaviour>.Release(list);
		return log;
	}

	private bool IsProducedJobValid(JobQueueItem job, Character character)
	{
		if (job is CharacterStateJob)
		{
			return true;
		}
		if (job is GoapPlanJob goapPlanJob)
		{
			if (character is Dragon)
			{
				return true;
			}
			if (goapPlanJob.jobType == JOB_TYPE.IDLE_RETURN_HOME)
			{
				if (character.homeStructure != null)
				{
					return character.movementComponent.HasPathToEvenIfDiffRegion(character.homeStructure.GetRandomUnoccupiedTile());
				}
				if (character.HasTerritory())
				{
					Area territory = character.territory;
					return character.movementComponent.HasPathToEvenIfDiffRegion(CollectionUtilities.GetRandomElement(territory.gridTileComponent.gridTiles));
				}
			}
			else
			{
				if (goapPlanJob.jobType == JOB_TYPE.RESCUE || goapPlanJob.jobType == JOB_TYPE.EXTERMINATE || goapPlanJob.jobType == JOB_TYPE.EXPLORE || goapPlanJob.jobType == JOB_TYPE.COUNTERATTACK || goapPlanJob.jobType == JOB_TYPE.MONSTER_INVADE || goapPlanJob.jobType == JOB_TYPE.CAPTURE_CHARACTER)
				{
					return true;
				}
				if (goapPlanJob.jobType == JOB_TYPE.PRODUCE_FOOD || goapPlanJob.jobType == JOB_TYPE.PRODUCE_FOOD_FOR_CAMP)
				{
					return true;
				}
				if (goapPlanJob.jobType == JOB_TYPE.DROP_ITEM || goapPlanJob.jobType == JOB_TYPE.DEMON_STEAL)
				{
					return true;
				}
				if (job.jobType == JOB_TYPE.FIND_FISH)
				{
					return true;
				}
			}
			if (character == goapPlanJob.targetPOI || goapPlanJob.targetPOI == null || goapPlanJob.targetPOI is TileObject { mapObjectState: MAP_OBJECT_STATE.UNBUILT })
			{
				return true;
			}
			if (goapPlanJob.targetPOI.gridTileLocation != null)
			{
				return character.movementComponent.HasPathToEvenIfDiffRegion(goapPlanJob.targetPOI.gridTileLocation, GridMap.Instance.mainRegion.innerMap.onlyUnwalkableGraph);
			}
			return false;
		}
		return false;
	}

	public void OnCharacterFinishedJob(JobQueueItem job)
	{
		if (job.jobType == JOB_TYPE.DECREASE_MOOD)
		{
			if (HasBehaviour(typeof(DeMooderBehaviour)))
			{
				StartDeMoodCooldown();
				ResetDeMoodVillageTarget();
			}
		}
		else if (job.jobType == JOB_TYPE.DISABLE)
		{
			if (HasBehaviour(typeof(DisablerBehaviour)))
			{
				StartDisablerCooldown();
			}
		}
		else if (job.jobType == JOB_TYPE.MONSTER_EAT)
		{
			if (HasBehaviour(typeof(AbductorBehaviour)))
			{
				switch (GameManager.Instance.GetCurrentTimeInWordsOfTick())
				{
				case TIME_IN_WORDS.MORNING:
				case TIME_IN_WORDS.AFTERNOON:
				case TIME_IN_WORDS.LUNCH_TIME:
					SetHasEatenInTheMorning(state: true);
					break;
				case TIME_IN_WORDS.EARLY_NIGHT:
				case TIME_IN_WORDS.LATE_NIGHT:
					SetHasEatenInTheNight(state: true);
					break;
				}
			}
		}
		else if (job.jobType == JOB_TYPE.MONSTER_ABDUCT)
		{
			if (job is GoapPlanJob { targetPOI: Character targetPOI })
			{
				if (HasBehaviour(typeof(AbductorBehaviour)))
				{
					targetPOI.defaultCharacterTrait.SetHasBeenAbductedByPlayerMonster(state: true);
				}
				if ((base.owner is GiantSpider || base.owner is Tarantula) && (targetPOI.traitContainer.HasTrait("Restrained") || targetPOI.traitContainer.HasTrait("Unconscious")))
				{
					targetPOI.traitContainer.AddTrait(targetPOI, "Webbed", base.owner);
					targetPOI.defaultCharacterTrait.SetHasBeenAbductedByWildMonster(state: true);
				}
			}
		}
		else if (job.jobType == JOB_TYPE.FACTION_KIDNAP)
		{
			if (job is GoapPlanJob { targetPOI: Character targetPOI2 } && base.owner.race == RACE.SPIDER && (targetPOI2.traitContainer.HasTrait("Restrained") || targetPOI2.traitContainer.HasTrait("Unconscious")))
			{
				targetPOI2.traitContainer.AddTrait(targetPOI2, "Webbed", base.owner);
				targetPOI2.defaultCharacterTrait.SetHasBeenAbductedByWildMonster(state: true);
			}
		}
		else if (job.jobType == JOB_TYPE.AGITATED)
		{
			if (job is GoapPlanJob { targetPOI: Character targetPOI3 } && (base.owner is GiantSpider || base.owner is Tarantula) && (targetPOI3.traitContainer.HasTrait("Restrained") || targetPOI3.traitContainer.HasTrait("Unconscious")))
			{
				targetPOI3.traitContainer.AddTrait(targetPOI3, "Webbed", base.owner);
				targetPOI3.defaultCharacterTrait.SetHasBeenAbductedByWildMonster(state: true);
			}
		}
		else if (job.jobType == JOB_TYPE.ARSON && HasBehaviour(typeof(ArsonistBehaviour)))
		{
			GameDate gameDate = GameManager.Instance.Today();
			gameDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(3));
			SchedulingManager.Instance.AddEntry(gameDate, ResetArsonistVillageTarget, base.owner);
			SchedulingManager.Instance.AddEntry(gameDate, StartArsonistCooldown, base.owner);
		}
	}

	public bool HasBehaviour(Type type)
	{
		for (int i = 0; i < currentBehaviourComponents.Count; i++)
		{
			if (currentBehaviourComponents[i].GetType() == type)
			{
				return true;
			}
		}
		return false;
	}

	public void SetIsAttackingDemonicStructure(bool state, DemonicStructure target)
	{
		if (isAttackingDemonicStructure != state)
		{
			isAttackingDemonicStructure = state;
			SetDemonicStructureTarget(target);
			base.owner.jobQueue.CancelAllJobs();
			if (isAttackingDemonicStructure)
			{
				combatModeBeforeAttackingDemonicStructure = base.owner.combatComponent.combatMode;
				base.owner.combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
				base.owner.movementComponent.SetEnableDigging(state: true);
				AddBehaviourComponent(typeof(AttackDemonicStructureBehaviour));
				base.owner.traitContainer.AddTrait(base.owner, "Fervor");
				StartCheckingIfShouldStopAttackingDemonicStructure();
			}
			else
			{
				base.owner.combatComponent.SetCombatMode(combatModeBeforeAttackingDemonicStructure);
				RemoveBehaviourComponent(typeof(AttackDemonicStructureBehaviour));
				base.owner.traitContainer.RemoveTrait(base.owner, "Fervor");
				base.owner.movementComponent.SetEnableDigging(state: false);
				StopCheckingIfShouldStopAttackingDemonicStructure();
			}
		}
	}

	private void SetDemonicStructureTarget(DemonicStructure target)
	{
		attackDemonicStructureTarget = target;
	}

	private void StartCheckingIfShouldStopAttackingDemonicStructure()
	{
	}

	private void StopCheckingIfShouldStopAttackingDemonicStructure()
	{
	}

	private void OnJobRemovedFromCharacter(JobQueueItem job, Character character)
	{
		_ = base.owner;
	}

	public int GetHighestBehaviourPriority()
	{
		if (currentBehaviourComponents.Count > 0)
		{
			return currentBehaviourComponents[0].priority;
		}
		return -1;
	}

	public void SetDouseFireSettlement(NPCSettlement settlement)
	{
		if (settlement == null)
		{
			dousingFireForSettlement?.settlementJobTriggerComponent.RemoveDouser(base.owner);
		}
		else
		{
			settlement.settlementJobTriggerComponent.AddDouser(base.owner);
		}
		dousingFireForSettlement = settlement;
	}

	public void SetCleansingTilesForSettlement(NPCSettlement settlement)
	{
		if (settlement == null)
		{
			cleansingTilesForSettlement?.settlementJobTriggerComponent.RemovePoisonCleanser(base.owner);
		}
		else
		{
			settlement.settlementJobTriggerComponent.AddPoisonCleanser(base.owner);
		}
		cleansingTilesForSettlement = settlement;
	}

	public void SetSettlementTargetForPurifyingGround(NPCSettlement settlement)
	{
		if (settlement != null)
		{
			settlementTargetForPurifyGround = settlement.persistentID;
		}
		else
		{
			settlementTargetForPurifyGround = string.Empty;
		}
	}

	public void ResetNumberOfPurifiedGrounds()
	{
		numberOfPurifiedGrounds = 0;
	}

	public void AddNumberOfPurifiedGrounds()
	{
		numberOfPurifiedGrounds++;
	}

	public void SetAttackVillageTarget(NPCSettlement npcSettlement)
	{
		attackVillageTarget = npcSettlement;
	}

	private void SetAttackAreaTarget(Area p_area)
	{
		attackAreaTarget = p_area;
	}

	public void ClearAttackVillageData()
	{
		SetAttackAreaTarget(null);
		SetAttackVillageTarget(null);
		if (HasBehaviour(typeof(AttackVillageBehaviour)))
		{
			RemoveBehaviourComponent(typeof(AttackVillageBehaviour));
		}
		if (base.owner.currentJob != null && base.owner.currentJob.jobType == JOB_TYPE.GO_TO)
		{
			base.owner.currentJob.ForceCancelJob();
		}
	}

	public void SetAbductionTarget(Character character)
	{
		currentAbductTarget = character;
	}

	public void OnSummon(LocationGridTile tile)
	{
		if (HasBehaviour(typeof(AbductorBehaviour)))
		{
			SetNest(tile);
		}
	}

	private void StartDeMoodCooldown()
	{
		currentDeMoodCooldown = 0;
		Messenger.AddListener(Signals.TICK_ENDED, PerTickDeMoodCooldown);
	}

	private void PerTickDeMoodCooldown()
	{
		if (currentDeMoodCooldown >= _deMoodCooldownPeriod)
		{
			Messenger.RemoveListener(Signals.TICK_ENDED, PerTickDeMoodCooldown);
		}
		currentDeMoodCooldown++;
	}

	public void ResetDeMoodVillageTarget()
	{
		deMoodVillageTarget.Clear();
	}

	public void ResetInvadeVillageTarget()
	{
		invadeVillageTarget.Clear();
	}

	public void AddFollower()
	{
		followerCount++;
	}

	public void RemoveFollower()
	{
		followerCount--;
	}

	private void StartDisablerCooldown()
	{
		currentDisableCooldown = 0;
		Messenger.AddListener(Signals.TICK_ENDED, PerTickDisablerCooldown);
	}

	private void PerTickDisablerCooldown()
	{
		if (currentDisableCooldown >= _disableCooldownPeriod)
		{
			Messenger.RemoveListener(Signals.TICK_ENDED, PerTickDisablerCooldown);
		}
		currentDisableCooldown++;
	}

	public void SetHasLayedAnEgg(bool state)
	{
		if (hasLayedAnEgg == state)
		{
			return;
		}
		hasLayedAnEgg = state;
		if (hasLayedAnEgg)
		{
			GameDate gameDate = GameManager.Instance.Today();
			gameDate.AddDays(2);
			SchedulingManager.Instance.AddEntry(gameDate, delegate
			{
				SetHasLayedAnEgg(state: false);
			}, base.owner);
		}
	}

	private void SetNest(LocationGridTile tile)
	{
		nest = tile;
	}

	public void OnBecomeAbductor()
	{
		Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_ADDED_TO_QUEUE, OnAbductorAddedJobToQueue);
		Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnAbductorRemovedJobFromQueue);
	}

	public void OnNoLongerAbductor()
	{
		Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_ADDED_TO_QUEUE, OnAbductorAddedJobToQueue);
		Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnAbductorRemovedJobFromQueue);
	}

	private void OnAbductorRemovedJobFromQueue(JobQueueItem job, Character character)
	{
		if (character == base.owner && job.jobType == JOB_TYPE.MONSTER_ABDUCT)
		{
			if (character is Summon summon)
			{
				character.combatComponent.SetCombatMode(summon.defaultCombatMode);
			}
			else
			{
				character.combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
			}
		}
	}

	private void OnAbductorAddedJobToQueue(JobQueueItem job, Character character)
	{
		if (character == base.owner && job.jobType == JOB_TYPE.MONSTER_ABDUCT && character.combatComponent.combatMode != COMBAT_MODE.Defend)
		{
			character.combatComponent.SetCombatMode(COMBAT_MODE.Defend);
		}
	}

	public bool AlreadyHasAbductedVictimAtNest(out Character target)
	{
		for (int i = 0; i < nest.charactersHere.Count; i++)
		{
			Character character = nest.charactersHere[i];
			if (!character.limiterComponent.canMove)
			{
				target = character;
				return true;
			}
		}
		target = null;
		return false;
	}

	public bool IsNestBlocked(out IPointOfInterest blocker)
	{
		if (nest.tileObjectComponent.objHere != null)
		{
			blocker = nest.tileObjectComponent.objHere;
			return true;
		}
		blocker = null;
		return false;
	}

	private void SetHasEatenInTheMorning(bool state)
	{
		hasEatenInTheMorning = state;
		if (state)
		{
			GameDate gameDate = GameManager.Instance.Today();
			gameDate.AddDays(1);
			gameDate.SetTicks(1);
			SchedulingManager.Instance.AddEntry(gameDate, delegate
			{
				SetHasEatenInTheMorning(state: false);
			}, base.owner);
		}
	}

	private void SetHasEatenInTheNight(bool state)
	{
		hasEatenInTheNight = state;
		if (state)
		{
			GameDate gameDate = GameManager.Instance.Today();
			gameDate.AddDays(1);
			gameDate.SetTicks(1);
			SchedulingManager.Instance.AddEntry(gameDate, delegate
			{
				SetHasEatenInTheNight(state: false);
			}, base.owner);
		}
	}

	public void ResetArsonistVillageTarget()
	{
		arsonVillageTarget.Clear();
	}

	private void StartArsonistCooldown()
	{
		currentArsonCooldown = 0;
		Messenger.AddListener(Signals.TICK_ENDED, PerTickArsonistCooldown);
	}

	private void PerTickArsonistCooldown()
	{
		if (currentArsonCooldown >= _arsonCooldownPeriod)
		{
			Messenger.RemoveListener(Signals.TICK_ENDED, PerTickArsonistCooldown);
		}
		currentArsonCooldown++;
	}

	public void OnBecomeArsonist()
	{
		Messenger.AddListener<Character>(CharacterSignals.START_FLEE, OnArsonistStartedFleeing);
	}

	public void OnNoLongerArsonist()
	{
		Messenger.RemoveListener<Character>(CharacterSignals.START_FLEE, OnArsonistStartedFleeing);
	}

	private void OnArsonistStartedFleeing(Character character)
	{
		if (character == base.owner)
		{
			ResetArsonistVillageTarget();
			StartArsonistCooldown();
		}
	}

	public void SetAbominationTarget(Area p_area)
	{
		abominationTarget = p_area;
		if (abominationTarget != null)
		{
			GameDate gameDate = GameManager.Instance.Today();
			gameDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(5));
			SchedulingManager.Instance.AddEntry(gameDate, delegate
			{
				SetAbominationTarget(null);
			}, base.owner);
		}
	}

	public void SetIsAgitated(bool state, int agitateDuration = -1)
	{
		if (base.owner.traitContainer.HasTrait("Agitated") != state)
		{
			if (state)
			{
				base.owner.traitContainer.AddTrait(base.owner, "Agitated", null, bypassElementalChance: false, agitateDuration);
			}
			else
			{
				base.owner.traitContainer.RemoveTrait(base.owner, "Agitated");
			}
		}
	}

	public void SetSubterraneanJustExitedCombat(bool state)
	{
		subterraneanJustExitedCombat = state;
	}

	public void SetIsSnatching(bool state)
	{
		isCurrentlySnatching = state;
	}

	public void OnBecomeSnatcher()
	{
	}

	public void OnNoLongerSnatcher()
	{
	}

	private void OnSnatcherAddedJobToQueue(JobQueueItem job, Character character)
	{
		if (character == base.owner && job.jobType == JOB_TYPE.SNATCH && character.combatComponent.combatMode != COMBAT_MODE.Defend)
		{
			character.combatComponent.SetCombatMode(COMBAT_MODE.Defend);
		}
	}

	private void OnSnatchJobRemoved(JobQueueItem job, Character character)
	{
		if (character == base.owner && job.jobType == JOB_TYPE.SNATCH)
		{
			SetIsSnatching(state: false);
			if (character is Summon summon)
			{
				character.combatComponent.SetCombatMode(summon.defaultCombatMode);
			}
			else
			{
				character.combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
			}
		}
	}

	public void OnBecomeCultist()
	{
		Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_ADDED_TO_QUEUE, OnCultistSnatchAddedJobToQueue);
		Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnCultistSnatchJobRemoved);
	}

	public void OnNoLongerCultist()
	{
		Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_ADDED_TO_QUEUE, OnCultistSnatchAddedJobToQueue);
		Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnCultistSnatchJobRemoved);
	}

	private void OnCultistSnatchAddedJobToQueue(JobQueueItem job, Character character)
	{
		if (character == base.owner && job.jobType == JOB_TYPE.SNATCH && character.combatComponent.combatMode != COMBAT_MODE.Defend)
		{
			character.combatComponent.SetCombatMode(COMBAT_MODE.Defend);
		}
	}

	private void OnCultistSnatchJobRemoved(JobQueueItem job, Character character)
	{
		if (character == base.owner && job.jobType == JOB_TYPE.SNATCH)
		{
			SetIsSnatching(state: false);
			character.combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
		}
	}

	public void OnBecomeDazed()
	{
		Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnDazedCharacterArrivedAtStructure);
		Messenger.AddListener<Character, CharacterState>(CharacterSignals.CHARACTER_STARTED_STATE, OnDazedCharacterStartedState);
	}

	public void OnCharacterEnteredArea(Area p_area)
	{
		if (base.owner.traitContainer.HasTrait("Dazed"))
		{
			if (base.owner.homeSettlement != null && base.owner.homeSettlement.areas.Contains(p_area))
			{
				base.owner.traitContainer.RemoveTrait(base.owner, "Dazed");
			}
			else if (base.owner.IsTerritory(p_area))
			{
				base.owner.traitContainer.RemoveTrait(base.owner, "Dazed");
			}
		}
	}

	private void OnDazedCharacterArrivedAtStructure(Character character, LocationStructure structure)
	{
		if (character == base.owner && character.homeStructure != null && character.homeStructure == structure)
		{
			character.traitContainer.RemoveTrait(character, "Dazed");
		}
	}

	private void OnDazedCharacterCanNoLongerPerform(Character character)
	{
		if (character == base.owner)
		{
			character.traitContainer.RemoveTrait(character, "Dazed");
		}
	}

	private void OnDazedCharacterStartedState(Character character, CharacterState state)
	{
		if (character == base.owner && state is CombatState)
		{
			character.traitContainer.RemoveTrait(character, "Dazed");
		}
	}

	public void OnNoLongerDazed()
	{
		Messenger.RemoveListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnDazedCharacterArrivedAtStructure);
		Messenger.RemoveListener<Character, CharacterState>(CharacterSignals.CHARACTER_STARTED_STATE, OnDazedCharacterStartedState);
	}

	public void SetPestSettlementTarget(BaseSettlement p_settlement)
	{
		pestSettlementTarget = p_settlement;
	}

	public void SetPestHasFailedEat(bool p_state)
	{
		pestHasFailedEat = p_state;
	}

	private bool CheckForUnderSiegeJob(out JobQueueItem p_producedJob)
	{
		if (base.owner.homeSettlement.isUnderSiege && base.owner.characterClass.IsCombatant())
		{
			List<Character> p_list = RuinarchListPool<Character>.Claim(30);
			base.owner.homeSettlement.PopulateHostileCharactersInSettlementForUnderSiege(p_list);
			Character nearestValidHostileFromList = base.owner.combatComponent.GetNearestValidHostileFromList(p_list);
			if (nearestValidHostileFromList != null)
			{
				JobQueueItem jobQueueItem = base.owner.homeSettlement.settlementJobTriggerComponent.CreateRestrainJob(nearestValidHostileFromList);
				p_producedJob = jobQueueItem;
				return true;
			}
		}
		p_producedJob = null;
		return false;
	}

	public bool PlanSettlementOrFactionWorkActions(out JobQueueItem producedJob)
	{
		if (base.owner.limiterComponent.canTakeJobs && !base.owner.movementComponent.isStationary)
		{
			if (base.owner.isAtHomeRegion && base.owner.homeSettlement != null && base.owner.homeSettlement.owner == base.owner.faction)
			{
				if (CheckForUnderSiegeJob(out producedJob))
				{
					return true;
				}
				JobQueueItem jobQueueItem = base.owner.homeSettlement.GetFirstJobBasedOnVision(base.owner);
				if (jobQueueItem != null)
				{
					producedJob = jobQueueItem;
					return true;
				}
				if (base.owner.homeSettlement.HasPathTowardsTileInSettlement(base.owner, 2))
				{
					if (base.owner.faction != null)
					{
						jobQueueItem = base.owner.faction.GetFirstUnassignedJobToCharacterJob(base.owner);
					}
					if (jobQueueItem == null && base.owner.currentSettlement == base.owner.homeSettlement)
					{
						jobQueueItem = base.owner.homeSettlement.GetFirstUnassignedJobToCharacterJob(base.owner);
					}
				}
				if (jobQueueItem != null)
				{
					producedJob = jobQueueItem;
					return true;
				}
			}
			if (base.owner.faction != null)
			{
				JobQueueItem firstUnassignedJobToCharacterJob = base.owner.faction.GetFirstUnassignedJobToCharacterJob(base.owner);
				if (firstUnassignedJobToCharacterJob != null)
				{
					producedJob = firstUnassignedJobToCharacterJob;
					return true;
				}
			}
		}
		producedJob = null;
		return false;
	}

	public bool TryTakeSettlementOrFactionWorkActionsOutsideOfWorkSchedule(out JobQueueItem producedJob)
	{
		if (base.owner.limiterComponent.canTakeJobs && !base.owner.movementComponent.isStationary)
		{
			if (base.owner.isAtHomeRegion && base.owner.homeSettlement != null && base.owner.homeSettlement.owner == base.owner.faction)
			{
				if (CheckForUnderSiegeJob(out producedJob))
				{
					return true;
				}
				JobQueueItem jobQueueItem = base.owner.homeSettlement.GetFirstJobBasedOnVisionGivenAllowedTypes(base.owner, WorkBehaviour.Allowed_Job_Types_Outside_Work);
				if (jobQueueItem != null)
				{
					producedJob = jobQueueItem;
					return true;
				}
				if (base.owner.homeSettlement.HasPathTowardsTileInSettlement(base.owner, 2))
				{
					if (base.owner.faction != null)
					{
						jobQueueItem = base.owner.faction.GetFirstUnassignedJobToCharacterJobGivenAllowedTypes(base.owner, WorkBehaviour.Allowed_Job_Types_Outside_Work);
					}
					if (jobQueueItem == null && base.owner.currentSettlement == base.owner.homeSettlement)
					{
						jobQueueItem = base.owner.homeSettlement.GetFirstUnassignedJobToCharacterJobGivenAllowedTypes(base.owner, WorkBehaviour.Allowed_Job_Types_Outside_Work);
					}
				}
				if (jobQueueItem != null)
				{
					producedJob = jobQueueItem;
					return true;
				}
			}
			if (base.owner.faction != null)
			{
				JobQueueItem firstUnassignedJobToCharacterJobGivenAllowedTypes = base.owner.faction.GetFirstUnassignedJobToCharacterJobGivenAllowedTypes(base.owner, WorkBehaviour.Allowed_Job_Types_Outside_Work);
				if (firstUnassignedJobToCharacterJobGivenAllowedTypes != null)
				{
					producedJob = firstUnassignedJobToCharacterJobGivenAllowedTypes;
					return true;
				}
			}
		}
		producedJob = null;
		return false;
	}

	public bool CanCharacterBeRecruitedBy(Character recruiter)
	{
		if (recruiter.faction == null || base.owner.faction == recruiter.faction || base.owner.race == RACE.TRITON)
		{
			return false;
		}
		if (base.owner is Summon summon && (summon.summonType == SUMMON_TYPE.Fallen_Angel || summon.summonType == SUMMON_TYPE.Nature_Spirit))
		{
			return false;
		}
		if (!base.owner.traitContainer.HasTrait("Restrained"))
		{
			return false;
		}
		if (base.owner.HasJobTargetingThis(JOB_TYPE.RECRUIT))
		{
			return false;
		}
		if (base.owner.isNormalCharacter && base.owner.race != RACE.RATMAN && !recruiter.faction.ideologyComponent.DoesCharacterFitCurrentIdeologiesIgnoreReligion(base.owner))
		{
			return false;
		}
		Prisoner traitOrStatus = base.owner.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
		if (traitOrStatus == null || !traitOrStatus.IsFactionPrisonerOf(recruiter.faction))
		{
			return false;
		}
		return true;
	}

	public void LoadReferences(SaveDataBehaviourComponent data)
	{
		if (!string.IsNullOrEmpty(data.attackVillageTarget))
		{
			attackVillageTarget = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentIDSafe(data.attackVillageTarget) as NPCSettlement;
		}
		if (!string.IsNullOrEmpty(data.attackAreaTarget))
		{
			attackAreaTarget = DatabaseManager.Instance.areaDatabase.GetAreaByPersistentID(data.attackAreaTarget);
		}
		if (!string.IsNullOrEmpty(data.attackDemonicStructureTarget))
		{
			attackDemonicStructureTarget = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentIDSafe(data.attackDemonicStructureTarget) as DemonicStructure;
		}
		if (!string.IsNullOrEmpty(data.dousingFireForSettlement))
		{
			dousingFireForSettlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentIDSafe(data.dousingFireForSettlement) as NPCSettlement;
		}
		if (!string.IsNullOrEmpty(data.cleansingTilesForSettlement))
		{
			cleansingTilesForSettlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentIDSafe(data.cleansingTilesForSettlement) as NPCSettlement;
		}
		if (!string.IsNullOrEmpty(data.currentAbductTarget))
		{
			currentAbductTarget = CharacterManager.Instance.GetCharacterByPersistentID(data.currentAbductTarget);
		}
		if (data.nest.hasValue)
		{
			nest = DatabaseManager.Instance.locationGridTileDatabase.GetTileBySavedData(data.nest);
		}
		if (!string.IsNullOrEmpty(data.abominationTarget))
		{
			abominationTarget = DatabaseManager.Instance.areaDatabase.GetAreaByPersistentID(data.abominationTarget);
		}
		if (data.deMoodVillageTarget != null)
		{
			for (int i = 0; i < data.deMoodVillageTarget.Count; i++)
			{
				Area areaByPersistentID = DatabaseManager.Instance.areaDatabase.GetAreaByPersistentID(data.deMoodVillageTarget[i]);
				deMoodVillageTarget.Add(areaByPersistentID);
			}
		}
		if (data.invadeVillageTarget != null)
		{
			for (int j = 0; j < data.invadeVillageTarget.Count; j++)
			{
				Area areaByPersistentID2 = DatabaseManager.Instance.areaDatabase.GetAreaByPersistentID(data.invadeVillageTarget[j]);
				invadeVillageTarget.Add(areaByPersistentID2);
			}
		}
		if (data.arsonVillageTarget != null)
		{
			for (int k = 0; k < data.arsonVillageTarget.Count; k++)
			{
				Area areaByPersistentID3 = DatabaseManager.Instance.areaDatabase.GetAreaByPersistentID(data.arsonVillageTarget[k]);
				arsonVillageTarget.Add(areaByPersistentID3);
			}
		}
		if (data.pestSettlementTarget != null && !string.IsNullOrEmpty(data.pestSettlementTarget))
		{
			pestSettlementTarget = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentIDSafe(data.pestSettlementTarget);
		}
		if (!string.IsNullOrEmpty(data.targetSocializeStructure))
		{
			targetSocializeStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentIDSafe(data.targetSocializeStructure);
			if (targetSocializeStructure != null)
			{
				base.owner.eventDispatcher.SubscribeToCharacterArrivedAtStructure(this);
			}
		}
		if (data.socializingEndTime.hasValue)
		{
			ScheduleSocializeEnd(data.socializingEndTime);
			base.owner.eventDispatcher.UnsubscribeToCharacterArrivedAtStructure(this);
		}
		if (!string.IsNullOrEmpty(data.targetVisitVillage))
		{
			targetVisitVillage = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(data.targetVisitVillage) as NPCSettlement;
			base.owner.eventDispatcher.SubscribeToCharacterArrivedAtSettlement(this);
		}
		if (!string.IsNullOrEmpty(data.targetVisitVillageStructure))
		{
			targetVisitVillageStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(data.targetVisitVillageStructure);
			base.owner.eventDispatcher.UnsubscribeToCharacterArrivedAtSettlement(this);
		}
		if (data.visitVillageEndTime.hasValue)
		{
			ScheduleVisitVillageEnd(data.visitVillageEndTime);
		}
		visitVillageIntent = data.visitVillageIntent;
		if (data.currentBehaviourComponents != null)
		{
			for (int l = 0; l < data.currentBehaviourComponents.Count; l++)
			{
				string typeName = data.currentBehaviourComponents[l];
				CharacterManager.Instance.GetCharacterBehaviourComponent(Type.GetType(typeName)).OnLoadBehaviourToCharacter(base.owner);
			}
		}
		if (data.shouldTryToBuildNewVillage)
		{
			Area hexTileGivenCoordinates = GameUtilities.GetHexTileGivenCoordinates(data.chosenVillageSpotForNewVillage, GridMap.Instance.map);
			chosenVillageSpotForNewVillage = hexTileGivenCoordinates.GetOccupyingVillageSpot();
		}
		if (!string.IsNullOrEmpty(data.criticalBreakDestroyStructureTarget))
		{
			criticalBreakDestroyStructureTarget = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(data.criticalBreakDestroyStructureTarget);
		}
		if (!string.IsNullOrEmpty(data.criticalBreakKillTarget))
		{
			criticalBreakKillTarget = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(data.criticalBreakKillTarget);
		}
	}

	public void OnBecomeDemonicDefender()
	{
		Messenger.AddListener<Character, DemonicStructure>(CharacterSignals.CHARACTER_HIT_DEMONIC_STRUCTURE, OnCharacterHitDemonicStructure);
	}

	public void OnNoLongerDemonicDefender()
	{
		Messenger.RemoveListener<Character, DemonicStructure>(CharacterSignals.CHARACTER_HIT_DEMONIC_STRUCTURE, OnCharacterHitDemonicStructure);
	}

	private void OnCharacterHitDemonicStructure(Character p_attacker, DemonicStructure p_demonicStructure)
	{
		if (!base.owner.combatComponent.isInCombat && base.owner.limiterComponent.canMove && base.owner.limiterComponent.canPerform && (base.owner.currentActionNode == null || base.owner.currentActionNode.action.goapType != INTERACTION_TYPE.ASSAULT))
		{
			base.owner.combatComponent.Fight(p_attacker, "Defending_Home");
		}
	}

	public void GoSocializing(Character p_character, LocationStructure p_targetStructure)
	{
		targetSocializeStructure = p_targetStructure;
		p_character.behaviourComponent.AddBehaviourComponent(typeof(SocializingBehaviour));
		if (p_character.gridTileLocation != null && p_character.gridTileLocation.structure == p_targetStructure)
		{
			GameDate p_date = GameManager.Instance.Today();
			p_date.AddTicks(GameManager.Instance.GetTicksBasedOnHour(2));
			ScheduleSocializeEnd(p_date);
		}
		else
		{
			p_character.eventDispatcher.SubscribeToCharacterArrivedAtStructure(this);
		}
	}

	public void OnCharacterLeftStructure(Character p_character, LocationStructure p_leftStructure)
	{
	}

	public void OnCharacterArrivedAtStructure(Character p_character, LocationStructure p_arrivedStructure)
	{
		if (p_arrivedStructure == targetSocializeStructure)
		{
			OnCharacterArrivedAtTargetSocializingStructure(p_character, p_arrivedStructure);
		}
	}

	private void OnCharacterArrivedAtTargetSocializingStructure(Character p_character, LocationStructure p_structure)
	{
		p_character.eventDispatcher.UnsubscribeToCharacterArrivedAtStructure(this);
		GameDate p_date = GameManager.Instance.Today();
		p_date.AddTicks(GameManager.Instance.GetTicksBasedOnHour(2));
		ScheduleSocializeEnd(p_date);
	}

	private void ScheduleSocializeEnd(GameDate p_date)
	{
		socializingEndTime = p_date;
		SchedulingManager.Instance.AddEntry(p_date, EndSocializeOnSchedule, this);
	}

	private void EndSocializeOnSchedule()
	{
		ClearOutSocializingBehaviour();
	}

	public void ClearOutSocializingBehaviour()
	{
		if (base.owner != null)
		{
			base.owner.behaviourComponent.RemoveBehaviourComponent(typeof(SocializingBehaviour));
			socializingEndTime = default(GameDate);
			targetSocializeStructure = null;
		}
	}

	public void VisitVillage(Character p_character, NPCSettlement p_settlement)
	{
		targetVisitVillage = p_settlement;
		p_character.behaviourComponent.AddBehaviourComponent(typeof(VisitVillageBehaviour));
		p_character.eventDispatcher.SubscribeToCharacterArrivedAtSettlement(this);
	}

	public void OnCharacterArrivedAtSettlement(Character p_character, NPCSettlement p_settlement)
	{
		if (p_settlement == targetVisitVillage)
		{
			OnCharacterArrivedAtTargetVillageSettlement(p_character, p_settlement);
		}
	}

	public void OnCharacterArrivedAtTargetVillageSettlement(Character p_character, NPCSettlement p_settlement)
	{
		p_character.eventDispatcher.UnsubscribeToCharacterArrivedAtSettlement(this);
		GameDate p_date = GameManager.Instance.Today();
		p_date.AddTicks(GameManager.Instance.GetTicksBasedOnHour(3));
		ScheduleVisitVillageEnd(p_date);
		SetVisitVillageIntent(VISIT_VILLAGE_INTENT.Socialize);
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Actions", "GoapActionsStrings_Table", "Visit Village arrived", LOG_TAG.Social);
		log.AddToFillers(p_character, p_character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddToFillers(p_settlement, p_settlement.name, LOG_IDENTIFIER.LANDMARK_1);
		log.AddLogToDatabase();
		if (p_settlement.HasStructure(STRUCTURE_TYPE.TAVERN))
		{
			SetTargetVisitVillageStructure(GameUtilities.RollChance(50) ? p_settlement.GetRandomStructureOfType(STRUCTURE_TYPE.TAVERN) : p_settlement.cityCenter);
		}
		else if (p_settlement.cityCenter != null)
		{
			SetTargetVisitVillageStructure(p_settlement.cityCenter);
		}
		else
		{
			SetTargetVisitVillageStructure(p_settlement.mainStorage);
		}
	}

	public void SetVisitVillageIntent(VISIT_VILLAGE_INTENT p_intent)
	{
		visitVillageIntent = p_intent;
	}

	public void SetTargetVisitVillageStructure(LocationStructure p_structure)
	{
		targetVisitVillageStructure = p_structure;
	}

	private void ScheduleVisitVillageEnd(GameDate p_date)
	{
		visitVillageEndTime = p_date;
		_visitVillagetEndDateKey = SchedulingManager.Instance.AddEntry(p_date, EndVisitVillageOnSchedule, base.owner);
	}

	private void EndVisitVillageOnSchedule()
	{
		ClearOutVisitVillageBehaviour();
	}

	public void ClearOutVisitVillageBehaviour()
	{
		if (!string.IsNullOrEmpty(_visitVillagetEndDateKey))
		{
			SchedulingManager.Instance.RemoveSpecificEntry(_visitVillagetEndDateKey);
		}
		base.owner.behaviourComponent.RemoveBehaviourComponent(typeof(VisitVillageBehaviour));
		visitVillageEndTime = default(GameDate);
		targetVisitVillage = null;
		targetVisitVillageStructure = null;
		visitVillageIntent = VISIT_VILLAGE_INTENT.Socialize;
	}

	public void OnCharacterBecameAPet()
	{
		Messenger.AddListener<Character, Character>(CharacterSignals.CHARACTER_REMOVED_FROM_VISION, OnCharacterExitedVision);
	}

	public void OnCharacterNoLongerAPet()
	{
		Messenger.RemoveListener<Character, Character>(CharacterSignals.CHARACTER_REMOVED_FROM_VISION, OnCharacterExitedVision);
	}

	private void OnCharacterExitedVision(Character p_character, Character p_characterThatExitedVision)
	{
		if (p_character == base.owner && p_characterThatExitedVision == p_character.petComponent.petOwner && !p_characterThatExitedVision.IsAtHome() && p_character.currentActionNode != null && (p_character.currentActionNode.associatedJobType == JOB_TYPE.IDLE || p_character.currentActionNode.associatedJobType == JOB_TYPE.IDLE_SIT || p_character.currentActionNode.associatedJobType == JOB_TYPE.IDLE_STAND || p_character.currentActionNode.associatedJobType == JOB_TYPE.STAND))
		{
			p_character.StopCurrentActionNode();
		}
	}

	public void MakeBuildVillagePriority(VillageSpot p_spot)
	{
		shouldTryToBuildNewVillage = true;
		chosenVillageSpotForNewVillage = p_spot;
	}

	public void DropBuildVillagePriority()
	{
		shouldTryToBuildNewVillage = false;
		chosenVillageSpotForNewVillage = null;
	}

	public bool HasTendedMaximumAmountOfCrops(Character p_character)
	{
		int num;
		if (p_character.HasTalents())
		{
			CharacterTalent talent = p_character.talentComponent.GetTalent(CHARACTER_TALENT.Food);
			num = ((talent.level >= 5) ? 3 : ((talent.level < 2) ? 1 : 2));
		}
		else
		{
			num = 1;
		}
		return tendedCropsForToday >= num;
	}

	public void AdjustTendedAmount(int p_amount)
	{
		tendedCropsForToday += p_amount;
	}

	private void ClearTendedAmount()
	{
		tendedCropsForToday = 0;
	}

	public void SetCanFlirtOnActivePartyQuest(bool p_state)
	{
		canFlirtOnActivePartyQuest = p_state;
	}

	public void StartNonInstantCriticalBreak()
	{
		criticalBreakDestroyStructureTarget = null;
		criticalBreakDestroyStructureTarget = null;
		AddBehaviourComponent(typeof(CriticalBreakBehaviour));
	}

	public void StartNonInstantCriticalBreak(LocationStructure p_structure)
	{
		criticalBreakDestroyStructureTarget = p_structure;
		criticalBreakKillTarget = null;
		AddBehaviourComponent(typeof(CriticalBreakBehaviour));
	}

	public void StartNonInstantCriticalBreak(Character p_character)
	{
		criticalBreakKillTarget = p_character;
		criticalBreakDestroyStructureTarget = null;
		AddBehaviourComponent(typeof(CriticalBreakBehaviour));
	}

	public void StopNonInstantCriticalBreak()
	{
		criticalBreakDestroyStructureTarget = null;
		criticalBreakKillTarget = null;
		criticalBreakFiresCreated = 0;
		RemoveBehaviourComponent(typeof(CriticalBreakBehaviour));
	}

	public void StopArsonCriticalBreak()
	{
		StopNonInstantCriticalBreak();
		base.owner.moodComponent.DoneDestroyStructureCriticalBreak();
	}

	public void EndCriticalBreakAbruptly()
	{
		StopNonInstantCriticalBreak();
		base.owner.moodComponent.EndCriticalBreakAbruptly();
	}

	public void AddCriticalBreakFire()
	{
		criticalBreakFiresCreated++;
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		_ = attackDemonicStructureTarget;
		_ = targetSocializeStructure;
		_ = targetVisitVillageStructure;
		_ = criticalBreakDestroyStructureTarget;
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = currentAbductTarget;
		_ = criticalBreakKillTarget;
	}
}

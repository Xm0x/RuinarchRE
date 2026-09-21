using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Locations.Settlements.Components;
using Locations.Settlements.Settlement_Types;
using Object_Pools;
using Traits;
using UnityEngine;
using UtilityScripts;

public class NPCSettlement : BaseSettlement, IJobOwner
{
	private Region _region;

	private readonly WeightedDictionary<Character> newRulerDesignationWeights;

	private int newRulerDesignationChance;

	private string _plaguedExpiryKey;

	private readonly List<TILE_OBJECT_TYPE> _neededObjects;

	private SettlementResources m_settlementResources;

	public LocationStructure prison { get; private set; }

	public LocationStructure mainStorage { get; private set; }

	public CityCenter cityCenter { get; private set; }

	public Character ruler { get; private set; }

	public List<JobQueueItem> forcedCancelJobsOnTickEnded { get; }

	public bool isUnderSiege { get; private set; }

	public bool isPlagued { get; private set; }

	public bool hasTriedToStealCorpse { get; private set; }

	public override SettlementResources SettlementResources => m_settlementResources ?? (m_settlementResources = new SettlementResources());

	public List<JobQueueItem> availableJobs { get; }

	public LocationEventManager eventManager { get; private set; }

	public SettlementType settlementType { get; private set; }

	public GameDate plaguedExpiryDate { get; private set; }

	public NPCSettlementEventDispatcher npcSettlementEventDispatcher { get; }

	public bool hasPeasants { get; private set; }

	public bool hasWorkers { get; private set; }

	public VillageSpot occupiedVillageSpot { get; private set; }

	public GameDate clearBlacklistScheduleDate { get; private set; }

	public bool hasClearBlacklistSchedule { get; private set; }

	public GameDate dateVillageWasLastClaimed { get; private set; }

	public SettlementJobTriggerComponent settlementJobTriggerComponent { get; }

	public SettlementVillageMigrationComponent migrationComponent { get; }

	public SettlementResourcesComponent resourcesComponent { get; }

	public SettlementClassComponent classComponent { get; }

	public SettlementPartyComponent partyComponent { get; }

	public SettlementStructureComponent structureComponent { get; }

	public SettlementExpirationComponent expirationComponent { get; }

	public SettlementTileObjectComponent tileObjectComponent { get; }

	public SettlementFactionIdeologyComponent factionIdeologyComponent { get; }

	public override Type serializedData => typeof(SaveDataNPCSettlement);

	public override Region region => _region;

	public JobTriggerComponent jobTriggerComponent => settlementJobTriggerComponent;

	public List<TILE_OBJECT_TYPE> neededObjects => _neededObjects;

	public JOB_OWNER ownerType => JOB_OWNER.SETTLEMENT;

	public bool isDestroyed => occupiedVillageSpot == null;

	public NPCSettlement(Region region, LOCATION_TYPE locationType)
		: base(locationType)
	{
		_region = region;
		newRulerDesignationWeights = new WeightedDictionary<Character>();
		forcedCancelJobsOnTickEnded = new List<JobQueueItem>();
		ResetNewRulerDesignationChance();
		availableJobs = new List<JobQueueItem>();
		eventManager = new LocationEventManager(this);
		settlementJobTriggerComponent = new SettlementJobTriggerComponent(this);
		npcSettlementEventDispatcher = new NPCSettlementEventDispatcher();
		_plaguedExpiryKey = string.Empty;
		_neededObjects = new List<TILE_OBJECT_TYPE>();
		migrationComponent = new SettlementVillageMigrationComponent();
		migrationComponent.SetOwner(this);
		resourcesComponent = new SettlementResourcesComponent();
		resourcesComponent.SetOwner(this);
		classComponent = new SettlementClassComponent();
		classComponent.SetOwner(this);
		classComponent.InitialMorningScheduleProcessingOfNeededClasses();
		classComponent.InitialAfternoonScheduleProcessingOfNeededClasses();
		partyComponent = new SettlementPartyComponent();
		partyComponent.SetOwner(this);
		partyComponent.InitialScheduleProcessingOfPartyQuests();
		structureComponent = new SettlementStructureComponent();
		structureComponent.SetOwner(this);
		expirationComponent = new SettlementExpirationComponent();
		expirationComponent.SetOwner(this);
		tileObjectComponent = new SettlementTileObjectComponent();
		tileObjectComponent.SetOwner(this);
		factionIdeologyComponent = new SettlementFactionIdeologyComponent();
		factionIdeologyComponent.SetOwner(this);
		factionIdeologyComponent.InitialScheduleProcessingOfEvents();
	}

	public NPCSettlement(SaveDataBaseSettlement saveDataBaseSettlement)
		: base(saveDataBaseSettlement)
	{
		SaveDataNPCSettlement saveDataNPCSettlement = saveDataBaseSettlement as SaveDataNPCSettlement;
		hasTriedToStealCorpse = saveDataNPCSettlement.hasTriedToStealCorpse;
		newRulerDesignationWeights = new WeightedDictionary<Character>();
		forcedCancelJobsOnTickEnded = new List<JobQueueItem>();
		ResetNewRulerDesignationChance();
		availableJobs = new List<JobQueueItem>();
		settlementJobTriggerComponent = new SettlementJobTriggerComponent(this);
		npcSettlementEventDispatcher = new NPCSettlementEventDispatcher();
		_plaguedExpiryKey = string.Empty;
		_neededObjects = new List<TILE_OBJECT_TYPE>(saveDataNPCSettlement.neededObjects);
		migrationComponent = saveDataNPCSettlement.migrationComponent.Load();
		migrationComponent.SetOwner(this);
		resourcesComponent = saveDataNPCSettlement.resourcesComponent.Load();
		resourcesComponent.SetOwner(this);
		classComponent = saveDataNPCSettlement.classComponent.Load();
		classComponent.SetOwner(this);
		partyComponent = saveDataNPCSettlement.partyComponent.Load();
		partyComponent.SetOwner(this);
		structureComponent = saveDataNPCSettlement.structureComponent.Load();
		structureComponent.SetOwner(this);
		if (saveDataNPCSettlement.expirationComponent != null)
		{
			expirationComponent = saveDataNPCSettlement.expirationComponent.Load();
			expirationComponent.SetOwner(this);
		}
		else
		{
			expirationComponent = new SettlementExpirationComponent();
			expirationComponent.SetOwner(this);
		}
		tileObjectComponent = saveDataNPCSettlement.tileObjectComponent.Load();
		tileObjectComponent.SetOwner(this);
		factionIdeologyComponent = saveDataNPCSettlement.factionIdeologyComponent.Load();
		factionIdeologyComponent.SetOwner(this);
		hasClearBlacklistSchedule = saveDataNPCSettlement.hasClearBlacklistSchedule;
		clearBlacklistScheduleDate = saveDataNPCSettlement.clearBlacklistScheduleDate;
		dateVillageWasLastClaimed = saveDataNPCSettlement.dateOfVillageCreation;
		isUnderSiege = saveDataNPCSettlement.isUnderSiege;
		isPlagued = saveDataNPCSettlement.isPlagued;
		hasPeasants = saveDataNPCSettlement.hasPeasants;
		hasWorkers = saveDataNPCSettlement.hasWorkers;
	}

	public override void LoadReferences(SaveDataBaseSettlement data)
	{
		base.LoadReferences(data);
		_region = GridMap.Instance.mainRegion;
		if (data is SaveDataNPCSettlement saveDataNPCSettlement)
		{
			List<Area> list = RuinarchListPool<Area>.Claim();
			GameUtilities.PopulateAreasGivenCoordinates(list, saveDataNPCSettlement.tileCoordinates, GridMap.Instance.map);
			for (int i = 0; i < list.Count; i++)
			{
				Area p_area = list[i];
				AddAreaToSettlement(p_area);
			}
			RuinarchListPool<Area>.Release(list);
			if (!string.IsNullOrEmpty(saveDataNPCSettlement.prisonID))
			{
				LocationStructure structureByPersistentID = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(saveDataNPCSettlement.prisonID);
				LoadPrison(structureByPersistentID);
			}
			if (!string.IsNullOrEmpty(saveDataNPCSettlement.mainStorageID))
			{
				LocationStructure structureByPersistentID2 = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(saveDataNPCSettlement.mainStorageID);
				LoadMainStorage(structureByPersistentID2);
			}
			eventManager = ((saveDataNPCSettlement.eventManager != null) ? new LocationEventManager(this, saveDataNPCSettlement.eventManager) : new LocationEventManager(this));
			LoadRuler(saveDataNPCSettlement.rulerID);
			LoadResidents(saveDataNPCSettlement);
			LoadParties(saveDataNPCSettlement);
			if (saveDataNPCSettlement.settlementType != null)
			{
				settlementType = saveDataNPCSettlement.settlementType.Load();
			}
			structureComponent.LoadReferences(saveDataNPCSettlement.structureComponent);
			if (saveDataNPCSettlement.hasOccupiedVillageSpot)
			{
				Area hexTileGivenCoordinates = GameUtilities.GetHexTileGivenCoordinates(saveDataNPCSettlement.occupiedVillageSpot, GridMap.Instance.map);
				occupiedVillageSpot = region.GetCoreVillageSpotOnArea(hexTileGivenCoordinates);
			}
			expirationComponent.LoadReferences(saveDataNPCSettlement.expirationComponent);
			tileObjectComponent.LoadReferences(saveDataNPCSettlement.tileObjectComponent);
		}
	}

	public override void LoadReferencesMainThread(SaveDataBaseSettlement data)
	{
		base.LoadReferencesMainThread(data);
		if (!(data is SaveDataNPCSettlement saveDataNPCSettlement))
		{
			return;
		}
		LoadJobs(saveDataNPCSettlement);
		if (isPlagued)
		{
			GameDate plaguedExpiry = saveDataNPCSettlement.plaguedExpiry;
			_plaguedExpiryKey = SchedulingManager.Instance.AddEntry(plaguedExpiry, delegate
			{
				SetIsPlagued(state: false);
			}, this);
			plaguedExpiryDate = plaguedExpiry;
		}
		bool shouldSubscribeToSignals = base.areas.Count > 0;
		Initialize(shouldSubscribeToSignals);
		migrationComponent.LoadReferences(saveDataNPCSettlement);
		resourcesComponent.LoadReferences(saveDataNPCSettlement.resourcesComponent);
		classComponent.LoadReferences(saveDataNPCSettlement.classComponent);
		partyComponent.LoadReferences(saveDataNPCSettlement.partyComponent);
		factionIdeologyComponent.LoadReferences(saveDataNPCSettlement.factionIdeologyComponent);
		if (hasClearBlacklistSchedule)
		{
			SchedulingManager.Instance.AddEntry(clearBlacklistScheduleDate, ClearAllBlacklistToAllExistingJobs, null);
		}
		else
		{
			ScheduleClearBlacklistJobs();
		}
	}

	private void LoadJobs(SaveDataNPCSettlement data)
	{
		int num;
		for (num = 0; num < availableJobs.Count; num++)
		{
			availableJobs[num].ForceCancelJob();
			num--;
		}
		for (int i = 0; i < data.jobIDs.Count; i++)
		{
			string text = data.jobIDs[i];
			JobQueueItem jobWithPersistentID = DatabaseManager.Instance.jobDatabase.GetJobWithPersistentID(text);
			availableJobs.Add(jobWithPersistentID);
		}
		for (int j = 0; j < data.forceCancelJobIDs.Count; j++)
		{
			string text2 = data.forceCancelJobIDs[j];
			JobQueueItem jobWithPersistentID2 = DatabaseManager.Instance.jobDatabase.GetJobWithPersistentID(text2);
			forcedCancelJobsOnTickEnded.Add(jobWithPersistentID2);
		}
	}

	private void LoadRuler(string rulerID)
	{
		if (base.locationType == LOCATION_TYPE.VILLAGE)
		{
			if (!string.IsNullOrEmpty(rulerID))
			{
				ruler = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(rulerID);
			}
			else
			{
				ruler = null;
			}
		}
	}

	private void LoadResidents(SaveDataBaseSettlement data)
	{
		if (data.residents == null)
		{
			return;
		}
		for (int i = 0; i < data.residents.Count; i++)
		{
			string text = data.residents[i];
			Character characterByPersistentID = CharacterManager.Instance.GetCharacterByPersistentID(text);
			if (characterByPersistentID != null)
			{
				base.residents.Add(characterByPersistentID);
			}
			else
			{
				Debug.LogWarning(base.name + " is trying to load a null resident with id " + text + "!");
			}
		}
	}

	private void LoadParties(SaveDataBaseSettlement data)
	{
		if (data.parties != null)
		{
			for (int i = 0; i < data.parties.Count; i++)
			{
				Party partyByPersistentID = DatabaseManager.Instance.partyDatabase.GetPartyByPersistentID(data.parties[i]);
				base.parties.Add(partyByPersistentID);
			}
		}
	}

	private void SubscribeToSignals()
	{
		Messenger.AddListener<Character, CharacterClass, CharacterClass>(CharacterSignals.CHARACTER_CLASS_CHANGE, OnCharacterClassChange);
		Messenger.AddListener<IPointOfInterest, string>(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, ForceCancelAllJobsTargetingCharacter);
		Messenger.AddListener<IPointOfInterest, string, JOB_TYPE>(CharacterSignals.FORCE_CANCEL_ALL_JOB_TYPES_TARGETING_POI, ForceCancelJobTypesTargetingPOI);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_PRESUMED_DEAD, OnCharacterPresumedDead);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
		Messenger.AddListener<Character, IPointOfInterest>(CharacterSignals.CHARACTER_SAW, OnCharacterSaw);
		Messenger.AddListener(Signals.TICK_ENDED, OnTickEnded);
		Messenger.AddListener(Signals.DAY_STARTED, OnDayStarted);
		Messenger.AddListener(Signals.HOUR_STARTED, OnHourStarted);
		Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_ADDED_TO_FACTION, OnCharacterAddedToFaction);
		Messenger.AddListener<Faction, Character>(FactionSignals.CREATE_FACTION_INTERRUPT, OnFactionCreated);
		Messenger.AddListener<LocationStructure>(StructureSignals.DISCONNECT_FROM_STRUCTURE, DisconnectFromStructure);
		Messenger.AddListener<Character>(CharacterSignals.DISCONNECT_FROM_CHARACTER, DisconnectFromCharacter);
		if (base.locationType == LOCATION_TYPE.VILLAGE)
		{
			settlementJobTriggerComponent.SubscribeToVillageListeners();
		}
		else if (base.locationType == LOCATION_TYPE.DUNGEON)
		{
			settlementJobTriggerComponent.SubscribeToDungeonListeners();
		}
	}

	private void UnsubscribeToSignals()
	{
		Messenger.RemoveListener<Character, CharacterClass, CharacterClass>(CharacterSignals.CHARACTER_CLASS_CHANGE, OnCharacterClassChange);
		Messenger.RemoveListener<IPointOfInterest, string>(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, ForceCancelAllJobsTargetingCharacter);
		Messenger.RemoveListener<IPointOfInterest, string, JOB_TYPE>(CharacterSignals.FORCE_CANCEL_ALL_JOB_TYPES_TARGETING_POI, ForceCancelJobTypesTargetingPOI);
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_PRESUMED_DEAD, OnCharacterPresumedDead);
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
		Messenger.RemoveListener<Character, IPointOfInterest>(CharacterSignals.CHARACTER_SAW, OnCharacterSaw);
		Messenger.RemoveListener(Signals.TICK_ENDED, OnTickEnded);
		Messenger.RemoveListener(Signals.DAY_STARTED, OnDayStarted);
		Messenger.RemoveListener(Signals.HOUR_STARTED, OnHourStarted);
		Messenger.RemoveListener<Character, Faction>(FactionSignals.CHARACTER_ADDED_TO_FACTION, OnCharacterAddedToFaction);
		Messenger.RemoveListener<Faction, Character>(FactionSignals.CREATE_FACTION_INTERRUPT, OnFactionCreated);
		Messenger.RemoveListener<LocationStructure>(StructureSignals.DISCONNECT_FROM_STRUCTURE, DisconnectFromStructure);
		Messenger.RemoveListener<Character>(CharacterSignals.DISCONNECT_FROM_CHARACTER, DisconnectFromCharacter);
		if (base.locationType == LOCATION_TYPE.VILLAGE)
		{
			settlementJobTriggerComponent.UnsubscribeFromVillageListeners();
		}
		else if (base.locationType == LOCATION_TYPE.DUNGEON)
		{
			settlementJobTriggerComponent.UnsubscribeFromDungeonListeners();
		}
	}

	private void OnFactionCreated(Faction p_faction, Character p_character)
	{
	}

	private void DisconnectFromStructure(LocationStructure p_structure)
	{
		List<JobQueueItem> list = RuinarchListPool<JobQueueItem>.Claim();
		list.AddRange(availableJobs);
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Jobs", "CancelReasons_Table", "Location_Destroyed", LOG_TAG.Life_Changes);
		log.AddToFillers(p_structure, p_structure.name, LOG_IDENTIFIER.LANDMARK_1);
		string logText = log.logText;
		LogPool.Release(log);
		for (int i = 0; i < list.Count; i++)
		{
			JobQueueItem jobQueueItem = list[i];
			if (jobQueueItem is GoapPlanJob goapPlanJob && goapPlanJob.HasOtherDataRelatedTo(p_structure))
			{
				jobQueueItem.ForceCancelJob(logText);
			}
		}
		RuinarchListPool<JobQueueItem>.Release(list);
		structureComponent.DisconnectFromStructure(p_structure);
	}

	private void DisconnectFromCharacter(Character p_character)
	{
		RemoveResident(p_character);
		settlementJobTriggerComponent?.DisconnectFromCharacter(p_character);
	}

	private void OnCharacterAddedToFaction(Character character, Faction faction)
	{
		if (base.locationType == LOCATION_TYPE.DUNGEON || base.locationType == LOCATION_TYPE.PSEUDO_VILLAGE)
		{
			if (!base.residents.Contains(character) || !faction.isMajorNonPlayerOrBandits)
			{
				return;
			}
			bool flag = true;
			for (int i = 0; i < base.residents.Count; i++)
			{
				if (base.residents[i].faction != faction)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				LandmarkManager.Instance.OwnSettlement(faction, this);
			}
		}
		else
		{
			if (base.locationType != LOCATION_TYPE.VILLAGE || character.homeSettlement != this || character.homeSettlement.owner != null)
			{
				return;
			}
			bool flag2 = false;
			for (int j = 0; j < base.residents.Count; j++)
			{
				Character character2 = base.residents[j];
				if (character2 != character && character2.faction != faction && character2.faction != null && (character2.faction.isMajorNonPlayer || character2.faction.factionType.type == FACTION_TYPE.Undead))
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				LandmarkManager.Instance.OwnSettlement(faction, this);
			}
		}
	}

	public void Initialize(bool shouldSubscribeToSignals)
	{
		if (shouldSubscribeToSignals)
		{
			SubscribeToSignals();
		}
		ScheduleClearBlacklistJobs();
	}

	private void ScheduleClearBlacklistJobs()
	{
		hasClearBlacklistSchedule = true;
		clearBlacklistScheduleDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(4));
		SchedulingManager.Instance.AddEntry(clearBlacklistScheduleDate, ClearAllBlacklistToAllExistingJobs, this);
	}

	protected override void SettlementWipedOut()
	{
		base.SettlementWipedOut();
		UnsubscribeToSignals();
		eventManager.OnSettlementDestroyed();
		settlementJobTriggerComponent.OnSettlementDestroyed();
	}

	public bool SetIsUnderSiege(bool state)
	{
		if (isUnderSiege != state)
		{
			isUnderSiege = state;
			return true;
		}
		return false;
	}

	public void SetIsPlagued(bool state)
	{
		if (state)
		{
			if (!string.IsNullOrEmpty(_plaguedExpiryKey))
			{
				SchedulingManager.Instance.RemoveSpecificEntry(_plaguedExpiryKey);
			}
			GameDate gameDate = GameManager.Instance.Today();
			gameDate.AddDays(2);
			_plaguedExpiryKey = SchedulingManager.Instance.AddEntry(gameDate, delegate
			{
				SetIsPlagued(state: false);
			}, this);
			plaguedExpiryDate = gameDate;
		}
		isPlagued = state;
	}

	private void OnTickEnded()
	{
		ProcessForcedCancelJobsOnTickEnded();
	}

	private void OnDayStarted()
	{
		hasTriedToStealCorpse = false;
	}

	private void OnHourStarted()
	{
		CheckSlaveResidents();
		CheckForJudgePrisoners();
		if ((base.locationType == LOCATION_TYPE.VILLAGE || base.locationType == LOCATION_TYPE.PSEUDO_VILLAGE) && ruler == null && base.owner != null && base.owner.isMajorNonPlayerOrBandits)
		{
			CheckForNewRulerDesignation();
		}
		if (isUnderSiege)
		{
			CheckIfStillUnderSiege();
		}
		if (base.owner != null && settlementType != null && settlementType.settlementType == SETTLEMENT_TYPE.Cult_Town && base.owner.factionType.type == FACTION_TYPE.Demon_Cult)
		{
			LocationStructure firstStructureOfType = GetFirstStructureOfType(STRUCTURE_TYPE.CULT_TEMPLE);
			if (firstStructureOfType != null)
			{
				if (!hasTriedToStealCorpse && GameManager.Instance.GetCurrentTimeInWordsOfTick() == TIME_IN_WORDS.MORNING)
				{
					hasTriedToStealCorpse = true;
					if (GameUtilities.RollChance(45))
					{
						settlementJobTriggerComponent.CreateStealCorpseJob(firstStructureOfType);
					}
				}
				if (GameUtilities.RollChance(10))
				{
					settlementJobTriggerComponent.CreateSummonBoneGolemJob(firstStructureOfType);
				}
			}
		}
		npcSettlementEventDispatcher.ExecuteHourStartedEvent(this);
		migrationComponent.OnHourStarted();
		structureComponent.PerHour();
	}

	private void OnSettlementAbandoned()
	{
		if (GameManager.Instance.gameHasStarted)
		{
			structureComponent.RelinkAllLinkedStructures();
			migrationComponent.OnSettlementAbandoned();
			expirationComponent.OnSettlementAbandoned();
			settlementJobTriggerComponent.OnSettlementAbandoned();
			if (base.locationType == LOCATION_TYPE.VILLAGE)
			{
				PlayerManager.Instance.player.playerSkillComponent.GetPrismEvent<RatmenEvent>().AdjustNumberOfAbandonedVillages(1);
			}
			Messenger.Broadcast(SettlementSignals.SETTLEMENT_ABANDONED, this);
		}
	}

	private void OnSettlementUnabandoned()
	{
		if (GameManager.Instance.gameHasStarted)
		{
			region.LinkAllUnlinkedSpecialStructures();
			migrationComponent.OnVillageGainFirstResident();
			expirationComponent.OnSettlementUnabandoned();
			if (base.locationType == LOCATION_TYPE.VILLAGE)
			{
				PlayerManager.Instance.player.playerSkillComponent.GetPrismEvent<RatmenEvent>().AdjustNumberOfAbandonedVillages(-1);
			}
		}
	}

	public override bool RemoveAreaFromSettlement(Area area)
	{
		if (base.RemoveAreaFromSettlement(area))
		{
			npcSettlementEventDispatcher.ExecuteTileRemovedEvent(area, this);
			return true;
		}
		return false;
	}

	public int GetNumberOfUnoccupiedStructure(STRUCTURE_TYPE structureType)
	{
		if (PlayerManager.Instance.player != null && PlayerManager.Instance.player.playerSettlement.id == base.id)
		{
			return 0;
		}
		int num = 0;
		if (base.structures.ContainsKey(structureType))
		{
			List<LocationStructure> list = base.structures[structureType];
			for (int i = 0; i < list.Count; i++)
			{
				if (!list[i].IsOccupied())
				{
					num++;
				}
			}
		}
		return num;
	}

	private void OnCharacterClassChange(Character character, CharacterClass previousClass, CharacterClass currentClass)
	{
		if (character.homeSettlement == this)
		{
			classComponent.OnResidentChangedClass(previousClass.className, character);
		}
	}

	public override bool AddResident(Character character, LocationStructure chosenHome = null, bool ignoreCapacity = true)
	{
		if (base.AddResident(character, chosenHome, ignoreCapacity))
		{
			OnAddResident(character);
			character.SetHomeSettlement(this);
			if (base.residents.Count == 1)
			{
				OnSettlementUnabandoned();
			}
			if (GameManager.Instance.gameHasStarted && character.isNormalCharacter && character.race != RACE.RATMAN)
			{
				bool flag = character.faction == null || (character.faction.factionType.type != FACTION_TYPE.Undead && character.faction.factionType.type != FACTION_TYPE.Wild_Monsters && character.faction.factionType.type != FACTION_TYPE.Ratmen);
				if (base.residents.Count > 1 && flag && (!dateVillageWasLastClaimed.hasValue || GameManager.Instance.Today().ConvertToContinuousDays() != dateVillageWasLastClaimed.ConvertToContinuousDays()))
				{
					for (int i = 0; i < 4; i++)
					{
						character.traitContainer.AddTrait(character, "Newcomer");
					}
				}
				GenerateCompatibilitiesWithNewResident(character);
			}
			return true;
		}
		return false;
	}

	public override bool RemoveResident(Character character)
	{
		if (base.RemoveResident(character))
		{
			character.SetHomeSettlement(null);
			OnRemoveResident(character);
			UnassignJobsTakenBy(character);
			if (!HasResidents())
			{
				OnSettlementAbandoned();
			}
			if (character.traitContainer.HasTrait("Newcomer"))
			{
				character.traitContainer.RemoveStatusAndStacks(character, "Newcomer");
			}
			return true;
		}
		return false;
	}

	private void OnCharacterPresumedDead(Character missingCharacter)
	{
		if (ruler != null && missingCharacter == ruler)
		{
			SetRuler(null);
		}
	}

	private void OnCharacterDied(Character deadCharacter)
	{
		if (ruler != null && deadCharacter == ruler)
		{
			SetRuler(null);
		}
	}

	public void SetRuler(Character newRuler)
	{
		Character character = ruler;
		ruler = newRuler;
		if (character != null)
		{
			character.behaviourComponent.RemoveBehaviourComponent(typeof(SettlementRulerBehaviour));
			if (!character.isFactionLeader)
			{
				character.jobComponent.RemoveAbleJob(JOB_TYPE.JUDGE_PRISONER);
				character.ForceCancelAllJobsTargetingThisCharacter(JOB_TYPE.REPORT_CRIME);
			}
		}
		if (ruler != null)
		{
			ruler.behaviourComponent.AddBehaviourComponent(typeof(SettlementRulerBehaviour));
			ruler.jobComponent.AddAbleJob(JOB_TYPE.JUDGE_PRISONER);
			Messenger.Broadcast(CharacterSignals.ON_SET_AS_SETTLEMENT_RULER, ruler, character);
		}
		else
		{
			Messenger.Broadcast(CharacterSignals.ON_SETTLEMENT_RULER_REMOVED, this, character);
		}
		npcSettlementEventDispatcher.ExecuteSettlementRulerChangedEvent(newRuler, this);
	}

	private void CheckForNewRulerDesignation()
	{
		if (UnityEngine.Random.Range(0, 100) < newRulerDesignationChance)
		{
			DesignateNewRuler();
		}
		else
		{
			newRulerDesignationChance += 5;
		}
	}

	public void DesignateNewRuler(bool willLog = true)
	{
		if (base.owner == null)
		{
			return;
		}
		newRulerDesignationWeights.Clear();
		for (int i = 0; i < base.residents.Count; i++)
		{
			Character character = base.residents[i];
			if (character.faction != base.owner || character.isDead || character.isBeingSeized || (base.owner != null && character.crimeComponent.IsWantedBy(base.owner)))
			{
				continue;
			}
			bool num = character.gridTileLocation != null && character.gridTileLocation.IsPartOfSettlement(this);
			bool isMemberThatJoinedQuest = character.partyComponent.isMemberThatJoinedQuest;
			if (!num && !isMemberThatJoinedQuest)
			{
				continue;
			}
			int num2 = 50;
			if (base.owner != null && base.owner.factionType.HasIdeology(FACTION_IDEOLOGY.Reveres_Vampires))
			{
				Vampire traitOrStatus = character.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
				if (traitOrStatus != null && traitOrStatus.DoesFactionKnowThisVampire(base.owner))
				{
					num2 += 100;
				}
			}
			if (base.owner != null && base.owner.factionType.HasIdeology(FACTION_IDEOLOGY.Reveres_Werewolves) && character.isLycanthrope && character.lycanData.DoesFactionKnowThisLycan(base.owner))
			{
				num2 += 100;
			}
			if (character.isFactionLeader)
			{
				num2 += 100;
			}
			if (character.characterClass.className == "Noble")
			{
				num2 += 40;
			}
			int num3 = 0;
			int num4 = 0;
			for (int j = 0; j < character.relationshipContainer.charactersWithOpinion.Count; j++)
			{
				Character character2 = character.relationshipContainer.charactersWithOpinion[j];
				if (character2.homeSettlement == this && !character2.isDead)
				{
					if (character2.relationshipContainer.IsFriendsWith(character))
					{
						num3++;
					}
					else if (character2.relationshipContainer.IsEnemiesWith(character))
					{
						num4++;
					}
				}
			}
			if (num3 > 0)
			{
				int num5 = 0;
				num5 = ((!character.traitContainer.HasTrait("Worker")) ? (num3 * 20) : Mathf.FloorToInt((float)(num3 * 20) * 0.2f));
				num2 += num5;
			}
			if (character.traitContainer.HasTrait("Inspiring"))
			{
				num2 += 25;
			}
			if (character.traitContainer.HasTrait("Authoritative"))
			{
				num2 += 50;
			}
			if (num4 > 0)
			{
				num2 += num4 * -10;
			}
			if (character.traitContainer.HasTrait("Unattractive"))
			{
				num2 += -20;
			}
			if (character.hasUnresolvedCrime)
			{
				num2 += -50;
			}
			if (character.traitContainer.HasTrait("Worker"))
			{
				num2 += -40;
			}
			if (num2 < 1)
			{
				num2 = 1;
			}
			if (character.traitContainer.HasTrait("Ambitious"))
			{
				num2 = Mathf.RoundToInt((float)num2 * 1.5f);
			}
			if ((character is Summon || character.characterClass.IsZombie()) && HasResidentThatIsSapientAndInsideSettlementOrHasJoinedQuest())
			{
				num2 = 0;
			}
			if (character.traitContainer.HasTrait("Enslaved"))
			{
				num2 = 0;
			}
			if (num2 > 0)
			{
				newRulerDesignationWeights.AddElement(character, num2);
			}
		}
		if (newRulerDesignationWeights.Count > 0)
		{
			Character character3 = newRulerDesignationWeights.PickRandomElementGivenWeights();
			if (character3 != null)
			{
				if (willLog)
				{
					character3.interruptComponent.TriggerInterrupt(INTERRUPT.Become_Settlement_Ruler, character3);
				}
				else
				{
					SetRuler(character3);
				}
			}
		}
		ResetNewRulerDesignationChance();
	}

	private void ResetNewRulerDesignationChance()
	{
		newRulerDesignationChance = 10;
	}

	public void PopulateHostileCharactersInSettlementForUnderSiege(List<Character> p_list)
	{
		for (int i = 0; i < base.areas.Count; i++)
		{
			Area area = base.areas[i];
			for (int j = 0; j < area.locationCharacterTracker.charactersAtLocation.Count; j++)
			{
				Character character = area.locationCharacterTracker.charactersAtLocation[j];
				if (character.reactionComponent.disguisedCharacter != null)
				{
					character = character.reactionComponent.disguisedCharacter;
				}
				if (!character.isDead && character.gridTileLocation != null && base.owner.IsHostileWith(character.faction) && character.combatComponent.combatMode != COMBAT_MODE.Passive && !character.traitContainer.HasTrait("Enslaved", "Restrained", "Transitioning", "Hidden") && (character.race != RACE.WOLF || !base.owner.factionType.HasIdeology(FACTION_IDEOLOGY.Reveres_Werewolves)) && (!(character.characterClass.className == "Noble") || character.partyComponent.isActiveMember))
				{
					p_list.Add(character);
				}
			}
		}
	}

	public void GenerateInitialOpinionBetweenResidents()
	{
		for (int i = 0; i < base.residents.Count; i++)
		{
			Character character = base.residents[i];
			for (int j = 0; j < base.residents.Count; j++)
			{
				Character character2 = base.residents[j];
				if (character != character2)
				{
					IRelationshipData orCreateRelationshipDataWith = character.relationshipContainer.GetOrCreateRelationshipDataWith(character, character2);
					IRelationshipData orCreateRelationshipDataWith2 = character2.relationshipContainer.GetOrCreateRelationshipDataWith(character2, character);
					int compatibilityValue = ((orCreateRelationshipDataWith.opinions.compatibilityValue != -1) ? orCreateRelationshipDataWith.opinions.compatibilityValue : ((orCreateRelationshipDataWith2.opinions.compatibilityValue == -1) ? UnityEngine.Random.Range(0, 5) : orCreateRelationshipDataWith2.opinions.compatibilityValue));
					orCreateRelationshipDataWith.opinions.SetCompatibilityValue(compatibilityValue);
					orCreateRelationshipDataWith2.opinions.SetCompatibilityValue(compatibilityValue);
					orCreateRelationshipDataWith.opinions.RandomizeBaseOpinionBasedOnCompatibility();
					orCreateRelationshipDataWith2.opinions.RandomizeBaseOpinionBasedOnCompatibility();
				}
			}
		}
	}

	private void GenerateCompatibilitiesWithNewResident(Character p_character)
	{
		for (int i = 0; i < base.residents.Count; i++)
		{
			Character character = base.residents[i];
			if (!character.isNormalCharacter)
			{
				continue;
			}
			if (character != p_character)
			{
				IRelationshipData orCreateRelationshipDataWith = character.relationshipContainer.GetOrCreateRelationshipDataWith(character, p_character);
				IRelationshipData orCreateRelationshipDataWith2 = p_character.relationshipContainer.GetOrCreateRelationshipDataWith(p_character, character);
				if (orCreateRelationshipDataWith.opinions.compatibilityValue == -1 || orCreateRelationshipDataWith2.opinions.compatibilityValue == -1)
				{
					int compatibilityValue = UnityEngine.Random.Range(0, 5);
					orCreateRelationshipDataWith.opinions.SetCompatibilityValue(compatibilityValue);
					orCreateRelationshipDataWith2.opinions.SetCompatibilityValue(compatibilityValue);
				}
			}
		}
	}

	public bool HasCanPerformOrAliveResidentInsideSettlement()
	{
		for (int i = 0; i < base.residents.Count; i++)
		{
			Character character = base.residents[i];
			if (character.limiterComponent.canPerform && !character.isDead && !character.isBeingSeized && character.gridTileLocation != null && character.gridTileLocation.IsPartOfSettlement(this))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAliveResident()
	{
		for (int i = 0; i < base.residents.Count; i++)
		{
			if (!base.residents[i].isDead)
			{
				return true;
			}
		}
		return false;
	}

	protected override bool IsResidentsFull()
	{
		if (base.structures.ContainsKey(STRUCTURE_TYPE.DWELLING))
		{
			List<LocationStructure> list = base.structures[STRUCTURE_TYPE.DWELLING];
			for (int i = 0; i < list.Count; i++)
			{
				if (!list[i].IsOccupied())
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool HasHomelessResident()
	{
		for (int i = 0; i < base.residents.Count; i++)
		{
			Character character = base.residents[i];
			if (character.homeStructure == null || character.homeStructure.structureType != STRUCTURE_TYPE.DWELLING)
			{
				return true;
			}
		}
		return false;
	}

	private void CheckForJudgePrisoners()
	{
		if (prison != null)
		{
			for (int i = 0; i < prison.charactersHere.Count; i++)
			{
				Character target = prison.charactersHere[i];
				settlementJobTriggerComponent.TryCreateJudgePrisoner(target);
			}
		}
	}

	private void OnAddResident(Character character)
	{
		eventManager.OnResidentAdded(character);
		classComponent.OnResidentAdded(character);
		if (base.locationType == LOCATION_TYPE.VILLAGE && GameManager.Instance.gameHasStarted && base.residents.Count == 1)
		{
			ChangeSettlementTypeAccordingTo(character);
		}
		if (base.locationType == LOCATION_TYPE.VILLAGE && availableJobs.Count > 0)
		{
			availableJobs.GetFirstJobWithOtherData(JOB_TYPE.CHANGE_CLASS, INTERACTION_TYPE.CHANGE_CLASS, character.characterClass.className)?.ForceCancelJob();
		}
	}

	private void OnRemoveResident(Character character)
	{
		eventManager.OnResidentRemoved(character);
		classComponent.OnResidentRemoved(character);
	}

	private void CheckSlaveResidents()
	{
		if (AreAllResidentsSlaves())
		{
			List<Character> list = RuinarchListPool<Character>.Claim();
			for (int i = 0; i < base.residents.Count; i++)
			{
				list.Add(base.residents[i]);
			}
			for (int j = 0; j < list.Count; j++)
			{
				Character character = list[j];
				character.traitContainer.RemoveTrait(character, "Enslaved");
			}
			RuinarchListPool<Character>.Release(list);
		}
	}

	private bool AreAllResidentsSlaves()
	{
		for (int i = 0; i < base.residents.Count; i++)
		{
			if (!base.residents[i].traitContainer.HasTrait("Enslaved"))
			{
				return false;
			}
		}
		return true;
	}

	public bool HasResidentThatIsOrCanBecomeClass(string p_className)
	{
		for (int i = 0; i < base.residents.Count; i++)
		{
			Character character = base.residents[i];
			if (character.characterClass.className == p_className || character.classComponent.HasAbleClass(p_className))
			{
				return true;
			}
		}
		return false;
	}

	public void PopulateResidentsCurrentlyInsideVillage(List<Character> p_characters)
	{
		for (int i = 0; i < base.residents.Count; i++)
		{
			Character character = base.residents[i];
			if (character.currentSettlement == this)
			{
				p_characters.Add(character);
			}
		}
	}

	public void PopulateAbleClassesOfAllResidents(List<string> p_ableClasses)
	{
		for (int i = 0; i < base.residents.Count; i++)
		{
			Character character = base.residents[i];
			for (int j = 0; j < character.classComponent.ableClasses.Count; j++)
			{
				string item = character.classComponent.ableClasses[j];
				if (!p_ableClasses.Contains(item))
				{
					p_ableClasses.Add(item);
				}
			}
		}
	}

	public bool HasResidentThatCanWorkAt(STRUCTURE_TYPE p_structureType)
	{
		for (int i = 0; i < base.residents.Count; i++)
		{
			Character character = base.residents[i];
			if (!character.isDead && !character.structureComponent.HasWorkPlaceStructure() && CharacterManager.Instance.CanCharacterWorkAt(character, p_structureType))
			{
				return true;
			}
		}
		return false;
	}

	public void OnItemAddedToLocation(TileObject item, LocationStructure structure)
	{
		CheckIfInventoryJobsAreStillValid(item, structure);
		settlementJobTriggerComponent.OnItemAddedToStructure(item, structure);
	}

	public void OnItemRemovedFromLocation(TileObject item, LocationStructure structure, LocationGridTile tile)
	{
		CheckAreaInventoryJobs(structure, item);
		settlementJobTriggerComponent.OnItemRemovedFromStructure(item, structure, tile);
	}

	private void CheckIfInventoryJobsAreStillValid(TileObject item, LocationStructure structure)
	{
		if (structure != mainStorage || !neededObjects.Contains(item.tileObjectType) || mainStorage.GetNumberOfBuiltTileObjects(item.tileObjectType) < 2)
		{
			return;
		}
		List<JobQueueItem> list = RuinarchListPool<JobQueueItem>.Claim();
		PopulateJobsOfType(list, JOB_TYPE.CRAFT_OBJECT);
		for (int i = 0; i < list.Count; i++)
		{
			JobQueueItem jobQueueItem = list[i];
			if (jobQueueItem is GoapPlanJob { targetPOI: TileObject targetPOI } && targetPOI.tileObjectType == item.tileObjectType)
			{
				jobQueueItem.ForceCancelJob("Settlement_Has_Enough");
			}
		}
		RuinarchListPool<JobQueueItem>.Release(list);
	}

	protected override void OnStructureAdded(LocationStructure structure)
	{
		base.OnStructureAdded(structure);
		if (cityCenter == null && structure.structureType == STRUCTURE_TYPE.CITY_CENTER)
		{
			cityCenter = structure as CityCenter;
		}
		UpdatePrison();
		UpdateMainStorage();
	}

	protected override void OnStructureRemoved(LocationStructure structure)
	{
		base.OnStructureRemoved(structure);
		UpdatePrison();
		UpdateMainStorage();
	}

	public void OnStructureBuilt(LocationStructure structure)
	{
		migrationComponent.OnStructureBuilt(structure);
	}

	private void OnCharacterSaw(Character character, IPointOfInterest seenPOI)
	{
		if (character.homeSettlement != this || character.currentSettlement != this)
		{
			return;
		}
		Character character2 = seenPOI as Character;
		if (character2 != null)
		{
			if (character2.reactionComponent.disguisedCharacter != null)
			{
				character2 = character2.reactionComponent.disguisedCharacter;
			}
			if (base.owner != null && character2.gridTileLocation != null && character2.gridTileLocation.IsNextToSettlementAreaOrPartOfSettlement(this) && (character2.gridTileLocation.structure.structureType == STRUCTURE_TYPE.WILDERNESS || character2.gridTileLocation.structure.settlementLocation == this) && (base.owner.isMajorFaction || base.owner.factionType.type == FACTION_TYPE.Bandits) && ShouldBeUnderSiegeIfCharacterEntersSettlement(character2) && SetIsUnderSiege(state: true))
			{
				PlayerManager.Instance.player?.retaliationComponent.UnderSiegeRetaliation(this, character2);
			}
		}
	}

	public bool ShouldBeUnderSiegeIfCharacterEntersSettlement(Character p_character)
	{
		if (p_character.limiterComponent.canPerform && p_character.limiterComponent.canMove && !p_character.isDead && p_character.combatComponent.combatMode != COMBAT_MODE.Passive && base.owner != null && p_character.faction != null && base.owner.IsHostileWith(p_character.faction) && (p_character.characterClass.className != "Noble" || p_character.partyComponent.isActiveMember))
		{
			if (p_character.race == RACE.WOLF && base.owner.factionType.HasIdeology(FACTION_IDEOLOGY.Reveres_Werewolves))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	private void CheckIfStillUnderSiege()
	{
		bool flag = false;
		for (int i = 0; i < region.charactersAtLocation.Count; i++)
		{
			Character character = region.charactersAtLocation[i];
			if (character.homeSettlement != this && character.gridTileLocation != null && character.faction != null && character.gridTileLocation.IsPartOfSettlement(this) && !character.isDead && !character.traitContainer.HasTrait("Restrained", "Paralyzed", "Hidden") && character.combatComponent.combatMode != COMBAT_MODE.Passive && base.owner.IsHostileWith(character.faction))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			SetIsUnderSiege(state: false);
		}
	}

	public StructureSetting GetMissingFacilityToBuildBasedOnWeights()
	{
		WeightedDictionary<StructureSetting> weightedDictionary = new WeightedDictionary<StructureSetting>(settlementType.facilityWeights.dictionary);
		foreach (KeyValuePair<StructureSetting, int> item in settlementType.facilityWeights.dictionary)
		{
			int facilityCap = settlementType.GetFacilityCap(item.Key);
			int numberOfStructures = GetNumberOfStructures(item.Key.structureType);
			SettlementResources.StructureRequirement requiredObjectForBuilding = item.Key.structureType.GetRequiredObjectForBuilding();
			if (numberOfStructures >= facilityCap || !m_settlementResources.IsRequirementAvailable(requiredObjectForBuilding, this))
			{
				weightedDictionary.SetElementWeight(item.Key, 0);
			}
		}
		if (weightedDictionary.GetTotalOfWeights() > 0)
		{
			return weightedDictionary.PickRandomElementGivenWeights();
		}
		return default(StructureSetting);
	}

	public int GetUnoccupiedDwellingCount()
	{
		int num = 0;
		List<LocationStructure> structuresOfType = GetStructuresOfType(STRUCTURE_TYPE.DWELLING);
		if (structuresOfType != null && structuresOfType.Count > 0)
		{
			for (int i = 0; i < structuresOfType.Count; i++)
			{
				if (!structuresOfType[i].IsOccupied())
				{
					num++;
				}
			}
		}
		return num;
	}

	public bool HasFoodProducingStructure()
	{
		return HasStructure(STRUCTURE_TYPE.BUTCHERS_SHOP, STRUCTURE_TYPE.FARM, STRUCTURE_TYPE.FISHERY);
	}

	public bool HasBasicResourceProducingStructure()
	{
		if (base.owner != null)
		{
			if (base.owner.factionType.type == FACTION_TYPE.Human_Empire)
			{
				return HasStructure(STRUCTURE_TYPE.MINE);
			}
			if (base.owner.factionType.type == FACTION_TYPE.Elven_Kingdom)
			{
				return HasStructure(STRUCTURE_TYPE.LUMBERYARD);
			}
			if (base.owner.factionType.type == FACTION_TYPE.Demon_Cult || base.owner.factionType.type == FACTION_TYPE.Lycan_Clan || base.owner.factionType.type == FACTION_TYPE.Vampire_Clan || base.owner.factionType.type == FACTION_TYPE.Divine_Church || base.owner.factionType.type == FACTION_TYPE.Wiccans)
			{
				if (!HasStructure(STRUCTURE_TYPE.MINE))
				{
					return HasStructure(STRUCTURE_TYPE.LUMBERYARD);
				}
				return true;
			}
		}
		return false;
	}

	public void PopulateStructureConnectorsForStructureType(List<StructureConnector> p_connectors, STRUCTURE_TYPE p_structureType)
	{
		switch (p_structureType)
		{
		case STRUCTURE_TYPE.FISHERY:
			PopulateAvailableFishingSpotConnectors(p_connectors);
			break;
		case STRUCTURE_TYPE.LUMBERYARD:
			PopulateAvailableTreeConnectors(p_connectors);
			break;
		case STRUCTURE_TYPE.MINE:
			PopulateAvailableMineShackConnectors(p_connectors);
			break;
		default:
			PopulateAvailableStructureConnectors(p_connectors);
			break;
		}
	}

	private void PopulateAvailableStructureConnectors(List<StructureConnector> connectors)
	{
		for (int i = 0; i < base.allStructures.Count; i++)
		{
			if (!(base.allStructures[i] is ManMadeStructure manMadeStructure) || !(manMadeStructure.structureObj != null))
			{
				continue;
			}
			for (int j = 0; j < manMadeStructure.structureObj.connectors.Length; j++)
			{
				StructureConnector structureConnector = manMadeStructure.structureObj.connectors[j];
				if (structureConnector.isOpen)
				{
					bool flag;
					if (structureConnector.tileLocation != null)
					{
						flag = occupiedVillageSpot.reservedAreas.Contains(structureConnector.tileLocation.area);
					}
					else
					{
						LocationGridTile tileFromWorldPosition = region.innerMap.GetTileFromWorldPosition(structureConnector.transform.position);
						flag = tileFromWorldPosition != null && occupiedVillageSpot.reservedAreas.Contains(tileFromWorldPosition.area);
					}
					if (flag)
					{
						connectors.Add(structureConnector);
					}
				}
			}
		}
	}

	private void PopulateAvailableTreeConnectors(List<StructureConnector> connectors)
	{
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		SettlementResources.PopulateAllTrees(list, this);
		for (int i = 0; i < list.Count; i++)
		{
			TreeObject treeObject = list[i] as TreeObject;
			if (treeObject.structureConnector != null && treeObject.structureConnector.isOpen && treeObject.gridTileLocation != null && treeObject.gridTileLocation.area.tileObjectComponent.GetTreeCount() >= 8)
			{
				connectors.Add(treeObject.structureConnector);
			}
		}
		for (int j = 0; j < occupiedVillageSpot.reservedAreas.Count; j++)
		{
			Area area = occupiedVillageSpot.reservedAreas[j];
			for (int k = 0; k < area.tileObjectComponent.trees.Count; k++)
			{
				TreeObject treeObject2 = area.tileObjectComponent.trees[k];
				if (treeObject2.structureConnector != null && treeObject2.structureConnector.isOpen && treeObject2.gridTileLocation != null && treeObject2.gridTileLocation.area.tileObjectComponent.GetTreeCount() >= 8 && !connectors.Contains(treeObject2.structureConnector))
				{
					connectors.Add(treeObject2.structureConnector);
				}
			}
		}
		RuinarchListPool<TileObject>.Release(list);
	}

	private void PopulateAvailableFishingSpotConnectors(List<StructureConnector> connectors)
	{
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		SettlementResources.PopulateAllFishingSpots(list, this);
		for (int i = 0; i < list.Count; i++)
		{
			FishingSpot fishingSpot = list[i] as FishingSpot;
			if (fishingSpot.structureConnector != null && fishingSpot.structureConnector.isOpen)
			{
				connectors.Add(fishingSpot.structureConnector);
			}
		}
		for (int j = 0; j < occupiedVillageSpot.reservedAreas.Count; j++)
		{
			Area area = occupiedVillageSpot.reservedAreas[j];
			for (int k = 0; k < area.tileObjectComponent.fishingSpots.Count; k++)
			{
				FishingSpot fishingSpot2 = area.tileObjectComponent.fishingSpots[k];
				if (fishingSpot2.structureConnector != null && fishingSpot2.structureConnector.isOpen && !connectors.Contains(fishingSpot2.structureConnector))
				{
					connectors.Add(fishingSpot2.structureConnector);
				}
			}
		}
		RuinarchListPool<TileObject>.Release(list);
	}

	private void PopulateAvailableMineShackConnectors(List<StructureConnector> connectors)
	{
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		SettlementResources.PopulateAllMineShackSpots(list, this);
		for (int i = 0; i < list.Count; i++)
		{
			LocationGridTile locationGridTile = list[i];
			if (locationGridTile.structure is Cave cave && locationGridTile.tileObjectComponent.genericTileObject.structureConnector != null && locationGridTile.tileObjectComponent.genericTileObject.structureConnector.isOpen && !cave.IsConnectedToSettlement(this))
			{
				connectors.Add(locationGridTile.tileObjectComponent.genericTileObject.structureConnector);
			}
		}
		for (int j = 0; j < occupiedVillageSpot.reservedAreas.Count; j++)
		{
			Area area = occupiedVillageSpot.reservedAreas[j];
			for (int k = 0; k < area.structureComponent.structureConnectors.Count; k++)
			{
				StructureConnector structureConnector = area.structureComponent.structureConnectors[k];
				if (structureConnector.tileLocation != null && structureConnector.isOpen && !connectors.Contains(structureConnector) && structureConnector.tileLocation.structure is Cave cave2 && !cave2.IsConnectedToSettlement(this))
				{
					connectors.Add(structureConnector);
				}
			}
		}
		RuinarchListPool<LocationGridTile>.Release(list);
	}

	public bool HasReservedSpotWithFeature(string p_feature)
	{
		for (int i = 0; i < occupiedVillageSpot.reservedAreas.Count; i++)
		{
			if (occupiedVillageSpot.reservedAreas[i].featureComponent.HasFeature(p_feature))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasStructureOfTypeThatIsAssigned(STRUCTURE_TYPE p_type)
	{
		List<LocationStructure> structuresOfType = GetStructuresOfType(p_type);
		if (structuresOfType != null)
		{
			for (int i = 0; i < structuresOfType.Count; i++)
			{
				if (structuresOfType[i] is ManMadeStructure manMadeStructure && manMadeStructure.HasAssignedWorker())
				{
					return true;
				}
			}
		}
		return false;
	}

	public LocationStructure GetFirstFoodProducingStructureThatCanAcceptWorkerAndIsNotReserved()
	{
		for (int i = 0; i < base.allStructures.Count; i++)
		{
			LocationStructure locationStructure = base.allStructures[i];
			if (locationStructure.structureType.IsFoodProducingStructure() && locationStructure is ManMadeStructure manMadeStructure && manMadeStructure.CanHireAWorker() && !availableJobs.HasJobWithOtherData(JOB_TYPE.CHANGE_CLASS, INTERACTION_TYPE.CHANGE_CLASS, locationStructure))
			{
				return locationStructure;
			}
		}
		return null;
	}

	public LocationStructure GetFirstResourceProducingStructureThatCanAcceptWorkerAndIsNotReserved()
	{
		for (int i = 0; i < base.allStructures.Count; i++)
		{
			LocationStructure locationStructure = base.allStructures[i];
			if (locationStructure.structureType.IsResourceProducingStructure() && locationStructure is ManMadeStructure manMadeStructure && manMadeStructure.CanHireAWorker() && !availableJobs.HasJobWithOtherData(JOB_TYPE.CHANGE_CLASS, INTERACTION_TYPE.CHANGE_CLASS, locationStructure))
			{
				return locationStructure;
			}
		}
		return null;
	}

	public LocationStructure GetFirstStructureOfTypeThatHasNoWorkerAndIsNotReserved(STRUCTURE_TYPE type)
	{
		if (HasStructure(type))
		{
			List<LocationStructure> list = base.structures[type];
			if (list != null && list.Count > 0)
			{
				for (int i = 0; i < list.Count; i++)
				{
					ManMadeStructure manMadeStructure = list[i] as ManMadeStructure;
					if (!manMadeStructure.HasAssignedWorker() && !availableJobs.HasJobWithOtherData(JOB_TYPE.CHANGE_CLASS, INTERACTION_TYPE.CHANGE_CLASS, manMadeStructure))
					{
						return manMadeStructure;
					}
				}
			}
		}
		return null;
	}

	public LocationStructure GetFirstStructureOfTypeThatHasWorkerOrIsReserved(STRUCTURE_TYPE type)
	{
		if (HasStructure(type))
		{
			List<LocationStructure> list = base.structures[type];
			if (list != null && list.Count > 0)
			{
				for (int i = 0; i < list.Count; i++)
				{
					ManMadeStructure manMadeStructure = list[i] as ManMadeStructure;
					if (manMadeStructure.HasAssignedWorker() || availableJobs.HasJobWithOtherData(JOB_TYPE.CHANGE_CLASS, INTERACTION_TYPE.CHANGE_CLASS, manMadeStructure))
					{
						return manMadeStructure;
					}
				}
			}
		}
		return null;
	}

	public LocationStructure GetRandomStructureOfTypeThatCanAcceptWorker(STRUCTURE_TYPE type)
	{
		LocationStructure result = null;
		if (HasStructure(type))
		{
			List<LocationStructure> list = base.structures[type];
			if (list != null && list.Count > 0)
			{
				List<LocationStructure> list2 = RuinarchListPool<LocationStructure>.Claim();
				List<LocationStructure> list3 = RuinarchListPool<LocationStructure>.Claim();
				for (int i = 0; i < list.Count; i++)
				{
					ManMadeStructure manMadeStructure = list[i] as ManMadeStructure;
					if (manMadeStructure.CanHireAWorker())
					{
						if (manMadeStructure.HasAssignedWorker())
						{
							list2.Add(manMadeStructure);
						}
						else
						{
							list3.Add(manMadeStructure);
						}
					}
				}
				if (list3.Count > 0)
				{
					result = CollectionUtilities.GetRandomElement(list3);
				}
				else if (list2.Count > 0)
				{
					result = CollectionUtilities.GetRandomElement(list2);
				}
				RuinarchListPool<LocationStructure>.Release(list2);
				RuinarchListPool<LocationStructure>.Release(list3);
			}
		}
		return result;
	}

	public bool HasStructureOfTypeThatCanAcceptWorkerAndIsNotReserved(STRUCTURE_TYPE type)
	{
		if (HasStructure(type))
		{
			List<LocationStructure> list = base.structures[type];
			if (list != null && list.Count > 0)
			{
				RuinarchListPool<LocationStructure>.Claim();
				RuinarchListPool<LocationStructure>.Claim();
				for (int i = 0; i < list.Count; i++)
				{
					ManMadeStructure manMadeStructure = list[i] as ManMadeStructure;
					if (manMadeStructure.CanHireAWorker() && !availableJobs.HasJobWithOtherData(JOB_TYPE.CHANGE_CLASS, INTERACTION_TYPE.CHANGE_CLASS, manMadeStructure))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public bool HasBlueprintOnTileForStructure(STRUCTURE_TYPE p_type)
	{
		for (int i = 0; i < availableJobs.Count; i++)
		{
			if (availableJobs[i] is GoapPlanJob { jobType: JOB_TYPE.BUILD_BLUEPRINT, poiTarget: GenericTileObject poiTarget } && poiTarget.blueprintOnTile != null && poiTarget.blueprintOnTile.structureType == p_type)
			{
				return true;
			}
		}
		return false;
	}

	public int GetNumberOfBlueprintOnTileForStructure(STRUCTURE_TYPE p_type)
	{
		int num = 0;
		for (int i = 0; i < availableJobs.Count; i++)
		{
			if (availableJobs[i] is GoapPlanJob { jobType: JOB_TYPE.BUILD_BLUEPRINT, poiTarget: GenericTileObject poiTarget } && poiTarget.blueprintOnTile != null && poiTarget.blueprintOnTile.structureType == p_type)
			{
				num++;
			}
		}
		return num;
	}

	public IEnumerator PlaceInitialObjectsForWorldGenCoroutine()
	{
		if (HasStructure(STRUCTURE_TYPE.LUMBERYARD))
		{
			List<LocationStructure> structuresOfType = GetStructuresOfType(STRUCTURE_TYPE.LUMBERYARD);
			for (int i = 0; i < structuresOfType.Count; i++)
			{
				LocationStructure locationStructure = structuresOfType[i];
				WoodPile woodPile = InnerMapManager.Instance.CreateNewTileObject<WoodPile>(TILE_OBJECT_TYPE.WOOD_PILE);
				woodPile.SetResourceInPile(GameUtilities.RandomBetweenTwoNumbers(50, 100));
				locationStructure.AddPOI(woodPile);
			}
		}
		if (HasStructure(STRUCTURE_TYPE.MINE))
		{
			List<LocationStructure> structuresOfType2 = GetStructuresOfType(STRUCTURE_TYPE.MINE);
			for (int j = 0; j < structuresOfType2.Count; j++)
			{
				LocationStructure locationStructure2 = structuresOfType2[j];
				StonePile stonePile = InnerMapManager.Instance.CreateNewTileObject<StonePile>(TILE_OBJECT_TYPE.STONE_PILE);
				stonePile.SetResourceInPile(GameUtilities.RandomBetweenTwoNumbers(50, 100));
				locationStructure2.AddPOI(stonePile);
			}
		}
		List<TILE_OBJECT_TYPE> list = RuinarchListPool<TILE_OBJECT_TYPE>.Claim();
		if (HasStructure(STRUCTURE_TYPE.FARM))
		{
			List<LocationStructure> structuresOfType3 = GetStructuresOfType(STRUCTURE_TYPE.FARM);
			for (int k = 0; k < structuresOfType3.Count; k++)
			{
				Farm farm = structuresOfType3[k] as Farm;
				List<TILE_OBJECT_TYPE> list2 = RuinarchListPool<TILE_OBJECT_TYPE>.Claim();
				for (int l = 0; l < farm.farmTiles.Count; l++)
				{
					if (farm.farmTiles[l].tileObjectComponent.objHere is Crops crops && !list2.Contains(crops.producedObjectOnHarvest))
					{
						list2.Add(crops.producedObjectOnHarvest);
					}
				}
				TILE_OBJECT_TYPE randomElement = CollectionUtilities.GetRandomElement(list2);
				if (!list.Contains(randomElement))
				{
					list.Add(randomElement);
				}
				RuinarchListPool<TILE_OBJECT_TYPE>.Release(list2);
				FoodPile foodPile = InnerMapManager.Instance.CreateNewTileObject<FoodPile>(randomElement);
				foodPile.SetResourceInPile(GameUtilities.RandomBetweenTwoNumbers(50, 100));
				farm.AddPOI(foodPile);
			}
		}
		if (HasStructure(STRUCTURE_TYPE.FISHERY))
		{
			list.Add(TILE_OBJECT_TYPE.FISH_PILE);
			List<LocationStructure> structuresOfType4 = GetStructuresOfType(STRUCTURE_TYPE.FISHERY);
			for (int m = 0; m < structuresOfType4.Count; m++)
			{
				LocationStructure locationStructure3 = structuresOfType4[m];
				FishPile fishPile = InnerMapManager.Instance.CreateNewTileObject<FishPile>(TILE_OBJECT_TYPE.FISH_PILE);
				fishPile.SetResourceInPile(GameUtilities.RandomBetweenTwoNumbers(50, 100));
				locationStructure3.AddPOI(fishPile);
			}
		}
		if (HasStructure(STRUCTURE_TYPE.BUTCHERS_SHOP))
		{
			list.Add(TILE_OBJECT_TYPE.ANIMAL_MEAT);
			List<LocationStructure> structuresOfType5 = GetStructuresOfType(STRUCTURE_TYPE.BUTCHERS_SHOP);
			for (int n = 0; n < structuresOfType5.Count; n++)
			{
				LocationStructure locationStructure4 = structuresOfType5[n];
				AnimalMeat animalMeat = InnerMapManager.Instance.CreateNewTileObject<AnimalMeat>(TILE_OBJECT_TYPE.ANIMAL_MEAT);
				animalMeat.SetResourceInPile(GameUtilities.RandomBetweenTwoNumbers(50, 100));
				locationStructure4.AddPOI(animalMeat);
			}
		}
		if (HasStructure(STRUCTURE_TYPE.DWELLING) && list.Count > 0)
		{
			List<LocationStructure> structuresOfType6 = GetStructuresOfType(STRUCTURE_TYPE.DWELLING);
			for (int num = 0; num < structuresOfType6.Count; num++)
			{
				LocationStructure locationStructure5 = structuresOfType6[num];
				TILE_OBJECT_TYPE randomElement2 = CollectionUtilities.GetRandomElement(list);
				if (GameUtilities.RollChance(50))
				{
					FoodPile foodPile2 = InnerMapManager.Instance.CreateNewTileObject<FoodPile>(randomElement2);
					foodPile2.SetResourceInPile(GameUtilities.RandomBetweenTwoNumbers(20, 60));
					locationStructure5.AddPOI(foodPile2);
					foodPile2.UpdateOwners();
				}
				Table tileObjectOfType = locationStructure5.GetTileObjectOfType<Table>();
				if (tileObjectOfType != null)
				{
					tileObjectOfType.resourceStorageComponent.SetResource(CONCRETE_RESOURCES.Animal_Meat, 0);
					CONCRETE_RESOURCES p_foodType = CONCRETE_RESOURCES.Animal_Meat;
					switch (randomElement2)
					{
					case TILE_OBJECT_TYPE.CORN:
						p_foodType = CONCRETE_RESOURCES.Corn;
						break;
					case TILE_OBJECT_TYPE.FISH_PILE:
						p_foodType = CONCRETE_RESOURCES.Fish;
						break;
					case TILE_OBJECT_TYPE.HYPNO_HERB:
						p_foodType = CONCRETE_RESOURCES.Hypno_Herb;
						break;
					case TILE_OBJECT_TYPE.ICEBERRY:
						p_foodType = CONCRETE_RESOURCES.Iceberry;
						break;
					case TILE_OBJECT_TYPE.PINEAPPLE:
						p_foodType = CONCRETE_RESOURCES.Pineapple;
						break;
					case TILE_OBJECT_TYPE.POTATO:
						p_foodType = CONCRETE_RESOURCES.Potato;
						break;
					case TILE_OBJECT_TYPE.VEGETABLES:
						p_foodType = CONCRETE_RESOURCES.Vegetables;
						break;
					}
					tileObjectOfType.SetFood(p_foodType, UnityEngine.Random.Range(20, 81));
				}
			}
		}
		RuinarchListPool<TILE_OBJECT_TYPE>.Release(list);
		yield return null;
	}

	public void PlaceInitialObjects()
	{
		PlaceResourcePiles();
	}

	private void PlaceResourcePiles()
	{
		WoodPile woodPile = InnerMapManager.Instance.CreateNewTileObject<WoodPile>(TILE_OBJECT_TYPE.WOOD_PILE);
		mainStorage.AddPOI(woodPile);
		woodPile.SetResourceInPile(180);
		StonePile stonePile = InnerMapManager.Instance.CreateNewTileObject<StonePile>(TILE_OBJECT_TYPE.STONE_PILE);
		mainStorage.AddPOI(stonePile);
		stonePile.SetResourceInPile(180);
		FoodPile poi = InnerMapManager.Instance.CreateNewTileObject<FoodPile>(TILE_OBJECT_TYPE.ANIMAL_MEAT);
		mainStorage.AddPOI(poi);
	}

	private void UpdatePrison()
	{
		LocationStructure locationStructure = GetRandomStructureOfType(STRUCTURE_TYPE.PRISON);
		if (locationStructure == null)
		{
			locationStructure = GetRandomStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
			if (locationStructure == null)
			{
				for (int i = 0; i < base.allStructures.Count; i++)
				{
					LocationStructure locationStructure2 = base.allStructures[i];
					if (locationStructure2.structureType != STRUCTURE_TYPE.WILDERNESS)
					{
						locationStructure = locationStructure2;
						break;
					}
				}
			}
		}
		SetPrison(locationStructure);
	}

	private void SetPrison(LocationStructure locationStructure)
	{
		prison = locationStructure;
	}

	private void UpdateMainStorage()
	{
		LocationStructure locationStructure = null;
		if (HasStructure(STRUCTURE_TYPE.WAREHOUSE))
		{
			locationStructure = GetRandomStructureOfType(STRUCTURE_TYPE.WAREHOUSE);
		}
		else if (HasStructure(STRUCTURE_TYPE.CITY_CENTER))
		{
			locationStructure = GetRandomStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
		}
		else
		{
			for (int i = 0; i < base.allStructures.Count; i++)
			{
				LocationStructure locationStructure2 = base.allStructures[i];
				if (locationStructure2.structureType != STRUCTURE_TYPE.WILDERNESS)
				{
					locationStructure = locationStructure2;
					break;
				}
			}
		}
		if (mainStorage == null || locationStructure == null || (locationStructure != null && locationStructure.structureType != mainStorage.structureType))
		{
			SetMainStorage(locationStructure);
		}
	}

	private void SetMainStorage(LocationStructure structure)
	{
		bool num = mainStorage != null && structure != null && mainStorage != structure;
		mainStorage = structure;
		if (num)
		{
			Messenger.Broadcast(SettlementSignals.SETTLEMENT_CHANGE_STORAGE, this);
		}
	}

	public void LoadPrison(LocationStructure prison)
	{
		SetPrison(prison);
	}

	public void LoadMainStorage(LocationStructure mainStorage)
	{
		SetMainStorage(mainStorage);
	}

	public void PopulateTileObjectsFromStructures<T>(List<TileObject> objs, STRUCTURE_TYPE structureType) where T : TileObject
	{
		if (HasStructure(structureType))
		{
			List<LocationStructure> list = base.structures[structureType];
			for (int i = 0; i < list.Count; i++)
			{
				list[i].PopulateTileObjectsOfType<T>(objs);
			}
		}
	}

	public void AddToAvailableJobs(JobQueueItem job, int position = -1)
	{
		if (position == -1)
		{
			availableJobs.Add(job);
		}
		else
		{
			availableJobs.Insert(position, job);
		}
	}

	public bool RemoveFromAvailableJobs(JobQueueItem job)
	{
		if (availableJobs.Remove(job))
		{
			_ = job is GoapPlanJob;
			OnJobRemovedFromAvailableJobs(job);
			return true;
		}
		return false;
	}

	public int GetNumberOfJobsWith(JOB_TYPE type)
	{
		int num = 0;
		for (int i = 0; i < availableJobs.Count; i++)
		{
			if (availableJobs[i].jobType == type)
			{
				num++;
			}
		}
		return num;
	}

	public int GetNumberOfJobsThatTargetsTileObjectOfType(TILE_OBJECT_TYPE p_type)
	{
		int num = 0;
		for (int i = 0; i < availableJobs.Count; i++)
		{
			if (availableJobs[i] is GoapPlanJob { poiTarget: TileObject poiTarget } && poiTarget.tileObjectType == p_type)
			{
				num++;
			}
		}
		return num;
	}

	public bool HasJob(JOB_TYPE job, IPointOfInterest target)
	{
		for (int i = 0; i < availableJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = availableJobs[i];
			if (jobQueueItem is GoapPlanJob)
			{
				GoapPlanJob goapPlanJob = jobQueueItem as GoapPlanJob;
				if (job == goapPlanJob.jobType && target == goapPlanJob.targetPOI)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasJob(JOB_TYPE job, IPointOfInterest target, out JobQueueItem p_job)
	{
		for (int i = 0; i < availableJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = availableJobs[i];
			if (jobQueueItem is GoapPlanJob)
			{
				GoapPlanJob goapPlanJob = jobQueueItem as GoapPlanJob;
				if (job == goapPlanJob.jobType && target == goapPlanJob.targetPOI)
				{
					p_job = goapPlanJob;
					return true;
				}
			}
		}
		p_job = null;
		return false;
	}

	public bool HasJob(JOB_TYPE jobType1)
	{
		for (int i = 0; i < availableJobs.Count; i++)
		{
			if (availableJobs[i].jobType == jobType1)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasJob(JOB_TYPE jobType1, JOB_TYPE jobType2)
	{
		for (int i = 0; i < availableJobs.Count; i++)
		{
			JOB_TYPE jobType3 = availableJobs[i].jobType;
			if (jobType3 == jobType1 || jobType3 == jobType2)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasJob(GoapEffect effect, IPointOfInterest target)
	{
		for (int i = 0; i < availableJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = availableJobs[i];
			if (jobQueueItem is GoapPlanJob)
			{
				GoapPlanJob goapPlanJob = jobQueueItem as GoapPlanJob;
				if (goapPlanJob.goal != null && effect.conditionType == goapPlanJob.goal.conditionType && effect.conditionKey == goapPlanJob.goal.conditionKey && effect.target == goapPlanJob.goal.target && target == goapPlanJob.targetPOI)
				{
					return true;
				}
			}
		}
		return false;
	}

	public JobQueueItem GetJob(params JOB_TYPE[] jobTypes)
	{
		for (int i = 0; i < availableJobs.Count; i++)
		{
			for (int j = 0; j < jobTypes.Length; j++)
			{
				JobQueueItem jobQueueItem = availableJobs[i];
				if (jobQueueItem.jobType == jobTypes[j])
				{
					return jobQueueItem;
				}
			}
		}
		return null;
	}

	public void PopulateJobsOfType(List<JobQueueItem> jobs, JOB_TYPE jobType)
	{
		for (int i = 0; i < availableJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = availableJobs[i];
			if (jobQueueItem.jobType == jobType)
			{
				jobs.Add(jobQueueItem);
			}
		}
	}

	public void PopulateJobsOfType(List<JobQueueItem> jobs, JOB_TYPE jobType1, JOB_TYPE jobType2)
	{
		for (int i = 0; i < availableJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = availableJobs[i];
			if (jobQueueItem.jobType == jobType1 || jobQueueItem.jobType == jobType2)
			{
				jobs.Add(jobQueueItem);
			}
		}
	}

	public JobQueueItem GetFirstJobOfTypeThatCanBeAssignedTo(JOB_TYPE jobType, Character p_character)
	{
		for (int i = 0; i < availableJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = availableJobs[i];
			if (jobQueueItem.jobType == jobType && jobQueueItem.assignedCharacter == null && p_character.jobQueue.CanJobBeAddedToQueue(jobQueueItem))
			{
				return jobQueueItem;
			}
		}
		return null;
	}

	public JobQueueItem GetJob(JOB_TYPE job, IPointOfInterest target)
	{
		for (int i = 0; i < availableJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = availableJobs[i];
			if (jobQueueItem is GoapPlanJob)
			{
				GoapPlanJob goapPlanJob = jobQueueItem as GoapPlanJob;
				if (job == goapPlanJob.jobType && target == goapPlanJob.targetPOI)
				{
					return goapPlanJob;
				}
			}
		}
		return null;
	}

	public bool AddFirstUnassignedJobToCharacterJob(Character character)
	{
		for (int i = 0; i < availableJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = availableJobs[i];
			if (jobQueueItem.assignedCharacter == null && character.jobQueue.AddJobInQueue(jobQueueItem))
			{
				return true;
			}
		}
		return false;
	}

	public JobQueueItem GetFirstUnassignedJobToCharacterJob(Character character)
	{
		for (int i = 0; i < availableJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = availableJobs[i];
			if (jobQueueItem.assignedCharacter == null && character.jobQueue.CanJobBeAddedToQueue(jobQueueItem))
			{
				return jobQueueItem;
			}
		}
		return null;
	}

	public JobQueueItem GetFirstUnassignedJobToCharacterJobGivenAllowedTypes(Character character, List<JOB_TYPE> p_jobTypes)
	{
		for (int i = 0; i < availableJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = availableJobs[i];
			if (jobQueueItem.assignedCharacter == null && p_jobTypes.Contains(jobQueueItem.jobType) && character.jobQueue.CanJobBeAddedToQueue(jobQueueItem))
			{
				return jobQueueItem;
			}
		}
		return null;
	}

	public bool AssignCharacterToJobBasedOnVision(Character character)
	{
		List<JobQueueItem> list = RuinarchListPool<JobQueueItem>.Claim();
		for (int i = 0; i < availableJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = availableJobs[i];
			if (jobQueueItem.assignedCharacter == null && jobQueueItem is GoapPlanJob)
			{
				GoapPlanJob goapPlanJob = jobQueueItem as GoapPlanJob;
				if (goapPlanJob.targetPOI != null && character.marker.IsPOIInVision(goapPlanJob.targetPOI) && character.jobQueue.CanJobBeAddedToQueue(jobQueueItem))
				{
					list.Add(jobQueueItem);
				}
			}
		}
		JobQueueItem jobQueueItem2 = null;
		if (list.Count > 0)
		{
			jobQueueItem2 = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<JobQueueItem>.Release(list);
		if (jobQueueItem2 != null)
		{
			return character.jobQueue.AddJobInQueue(jobQueueItem2);
		}
		return false;
	}

	public JobQueueItem GetFirstJobBasedOnVision(Character character)
	{
		for (int i = 0; i < availableJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = availableJobs[i];
			if (jobQueueItem.assignedCharacter == null && jobQueueItem is GoapPlanJob)
			{
				GoapPlanJob goapPlanJob = jobQueueItem as GoapPlanJob;
				if (goapPlanJob.targetPOI != null && goapPlanJob.jobType != JOB_TYPE.BLOOD_SACRIFICE && character.marker.IsPOIInVision(goapPlanJob.targetPOI) && character.jobQueue.CanJobBeAddedToQueue(jobQueueItem))
				{
					return jobQueueItem;
				}
			}
		}
		return null;
	}

	public JobQueueItem GetFirstJobBasedOnVisionGivenAllowedTypes(Character character, List<JOB_TYPE> p_jobTypes)
	{
		for (int i = 0; i < availableJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = availableJobs[i];
			if (jobQueueItem.assignedCharacter == null && jobQueueItem is GoapPlanJob && p_jobTypes.Contains(jobQueueItem.jobType))
			{
				GoapPlanJob goapPlanJob = jobQueueItem as GoapPlanJob;
				if (goapPlanJob.targetPOI != null && character.marker.IsPOIInVision(goapPlanJob.targetPOI) && character.jobQueue.CanJobBeAddedToQueue(jobQueueItem))
				{
					return jobQueueItem;
				}
			}
		}
		return null;
	}

	public JobQueueItem GetFirstJobBasedOnVisionExcept(Character character, params JOB_TYPE[] jobTypes)
	{
		for (int i = 0; i < availableJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = availableJobs[i];
			if (jobQueueItem.assignedCharacter == null && jobQueueItem is GoapPlanJob goapPlanJob && !jobTypes.Contains(goapPlanJob.jobType) && goapPlanJob.targetPOI != null && character.marker.IsPOIInVision(goapPlanJob.targetPOI) && character.jobQueue.CanJobBeAddedToQueue(goapPlanJob))
			{
				return goapPlanJob;
			}
		}
		return null;
	}

	public JobQueueItem GetFirstJobBasedOnVisionExcept(Character character, JOB_TYPE except)
	{
		for (int i = 0; i < availableJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = availableJobs[i];
			if (jobQueueItem.assignedCharacter == null && jobQueueItem is GoapPlanJob goapPlanJob && except != goapPlanJob.jobType && goapPlanJob.targetPOI != null && character.marker.IsPOIInVision(goapPlanJob.targetPOI) && character.jobQueue.CanJobBeAddedToQueue(goapPlanJob))
			{
				return goapPlanJob;
			}
		}
		return null;
	}

	private void CheckAreaInventoryJobs(LocationStructure affectedStructure, TileObject objectThatTriggeredChange)
	{
		if (affectedStructure != mainStorage || (objectThatTriggeredChange != null && !neededObjects.Contains(objectThatTriggeredChange.tileObjectType)))
		{
			return;
		}
		for (int i = 0; i < neededObjects.Count; i++)
		{
			TILE_OBJECT_TYPE tILE_OBJECT_TYPE = neededObjects[i];
			int numberOfTileObjects = affectedStructure.GetNumberOfTileObjects(tILE_OBJECT_TYPE);
			int num = 2;
			if (numberOfTileObjects >= num)
			{
				continue;
			}
			int num2 = num - numberOfTileObjects;
			for (int j = 0; j < num2; j++)
			{
				TileObject tileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(tILE_OBJECT_TYPE);
				affectedStructure.AddPOI(tileObject);
				tileObject.SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);
				GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CRAFT_OBJECT, INTERACTION_TYPE.CRAFT_TILE_OBJECT, tileObject, this);
				JobUtilities.PopulatePriorityLocationsForTakingNonEdibleResources(this, goapPlanJob, INTERACTION_TYPE.TAKE_RESOURCE);
				switch (tILE_OBJECT_TYPE)
				{
				case TILE_OBJECT_TYPE.HEALING_POTION:
					goapPlanJob.SetCanTakeThisJobChecker("CanBrewPotion");
					break;
				case TILE_OBJECT_TYPE.TOOL:
					goapPlanJob.SetCanTakeThisJobChecker("CanCraftTool");
					break;
				case TILE_OBJECT_TYPE.ANTIDOTE:
					goapPlanJob.SetCanTakeThisJobChecker("CanBrewAntidote");
					break;
				case TILE_OBJECT_TYPE.PHYLACTERY:
				{
					goapPlanJob.SetCanTakeThisJobChecker("CanCraftPhylactery");
					TileObjectDB.GetTileObjectData(TILE_OBJECT_TYPE.PHYLACTERY).TryGetPossibleRecipe(this, out var possibleRecipe);
					goapPlanJob.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[1] { possibleRecipe });
					goapPlanJob.AddOtherData(INTERACTION_TYPE.CRAFT_TILE_OBJECT, new object[1] { possibleRecipe });
					break;
				}
				}
				AddToAvailableJobs(goapPlanJob);
			}
		}
	}

	private void OnJobRemovedFromAvailableJobs(JobQueueItem job)
	{
		JobManager.Instance.ReleaseJob(job);
		if (job.jobType == JOB_TYPE.CRAFT_OBJECT)
		{
			CheckAreaInventoryJobs(mainStorage, null);
		}
	}

	private void ForceCancelAllJobsTargetingCharacter(IPointOfInterest target, string reason)
	{
		for (int i = 0; i < availableJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = availableJobs[i];
			if (jobQueueItem is GoapPlanJob)
			{
				GoapPlanJob goapPlanJob = jobQueueItem as GoapPlanJob;
				if (goapPlanJob.targetPOI == target && goapPlanJob.ForceCancelJob(reason))
				{
					i--;
				}
			}
		}
	}

	private void ForceCancelJobTypesTargetingPOI(IPointOfInterest target, string reason, JOB_TYPE jobType)
	{
		for (int i = 0; i < availableJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = availableJobs[i];
			if (!jobQueueItem.hasBeenReset && jobQueueItem.jobType == jobType && jobQueueItem is GoapPlanJob goapPlanJob && goapPlanJob.targetPOI == target && goapPlanJob.ForceCancelJob(reason))
			{
				i--;
			}
		}
	}

	public void ForceCancelJobTypesTargetingPOI(JOB_TYPE jobType, IPointOfInterest target)
	{
		for (int i = 0; i < availableJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = availableJobs[i];
			if (jobQueueItem.jobType == jobType && jobQueueItem is GoapPlanJob)
			{
				GoapPlanJob goapPlanJob = jobQueueItem as GoapPlanJob;
				if (goapPlanJob.targetPOI == target)
				{
					AddForcedCancelJobsOnTickEnded(goapPlanJob);
				}
			}
		}
	}

	public void ForceCancelJobTypes(JOB_TYPE jobType)
	{
		for (int i = 0; i < availableJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = availableJobs[i];
			if (jobQueueItem.jobType == jobType && jobQueueItem is GoapPlanJob)
			{
				GoapPlanJob job = jobQueueItem as GoapPlanJob;
				AddForcedCancelJobsOnTickEnded(job);
			}
		}
	}

	public void ForceCancelJobTypesImmediately(JOB_TYPE jobType)
	{
		for (int i = 0; i < availableJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = availableJobs[i];
			if (jobQueueItem.jobType == jobType && jobQueueItem is GoapPlanJob && (jobQueueItem as GoapPlanJob).ForceCancelJob())
			{
				i--;
			}
		}
	}

	private void ClearAllBlacklistToAllExistingJobs()
	{
		for (int i = 0; i < availableJobs.Count; i++)
		{
			availableJobs[i].ClearBlacklist();
		}
		ScheduleClearBlacklistJobs();
	}

	private void UnassignJobsTakenBy(Character character)
	{
		for (int i = 0; i < availableJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = availableJobs[i];
			if (jobQueueItem.assignedCharacter == character && jobQueueItem is GoapPlanJob goapPlanJob)
			{
				goapPlanJob.CancelJob();
			}
		}
	}

	public void OnJobAddedToCharacterJobQueue(JobQueueItem job, Character character)
	{
	}

	public void OnJobRemovedFromCharacterJobQueue(JobQueueItem job, Character character, bool shouldBlacklist = false)
	{
		if (!job.IsJobStillApplicable() || job.shouldBeRemovedFromSettlementWhenUnassigned)
		{
			RemoveFromAvailableJobs(job);
		}
		else if (shouldBlacklist && character != null)
		{
			job.AddBlacklistedCharacter(character);
		}
	}

	public bool ForceCancelJob(JobQueueItem job)
	{
		return RemoveFromAvailableJobs(job);
	}

	public void AddForcedCancelJobsOnTickEnded(JobQueueItem job)
	{
		if (!forcedCancelJobsOnTickEnded.Contains(job))
		{
			forcedCancelJobsOnTickEnded.Add(job);
		}
	}

	public void ProcessForcedCancelJobsOnTickEnded()
	{
		if (forcedCancelJobsOnTickEnded.Count > 0)
		{
			List<JobQueueItem> list = RuinarchListPool<JobQueueItem>.Claim(forcedCancelJobsOnTickEnded.Count);
			list.AddRange(forcedCancelJobsOnTickEnded);
			for (int i = 0; i < list.Count; i++)
			{
				list[i].ForceCancelJob();
			}
			RuinarchListPool<JobQueueItem>.Release(list);
			forcedCancelJobsOnTickEnded.Clear();
		}
	}

	public override void SetOwner(Faction p_newOwner)
	{
		Faction faction = base.owner;
		base.SetOwner(p_newOwner);
		if (p_newOwner != faction)
		{
			migrationComponent.ResetLongTermModifier();
		}
		if (p_newOwner == null)
		{
			SetIsUnderSiege(state: false);
			dateVillageWasLastClaimed = default(GameDate);
		}
		else
		{
			dateVillageWasLastClaimed = GameManager.Instance.Today();
		}
		migrationComponent.ForceRandomizePerHourIncrement();
		npcSettlementEventDispatcher.ExecuteFactionOwnerChangedEvent(faction, p_newOwner, this);
		factionIdeologyComponent.OnChangeFactionOwner(faction, p_newOwner);
	}

	public void SetSettlementType(SETTLEMENT_TYPE type)
	{
		if (base.locationType == LOCATION_TYPE.VILLAGE && (settlementType == null || settlementType.settlementType != type))
		{
			settlementType = LandmarkManager.Instance.CreateSettlementType(type);
			settlementType.ApplyDefaultSettings();
			migrationComponent.OnSettlementTypeChanged();
		}
	}

	private void ChangeSettlementTypeAccordingTo(Character character)
	{
		SETTLEMENT_TYPE settlementTypeForCharacter = LandmarkManager.Instance.GetSettlementTypeForCharacter(character);
		SetSettlementType(settlementTypeForCharacter);
	}

	public void AddNeededItems(TILE_OBJECT_TYPE tileObjectType)
	{
		neededObjects.Add(tileObjectType);
		CheckAreaInventoryJobs(mainStorage, null);
	}

	public void RemoveNeededItems(TILE_OBJECT_TYPE tileObjectType)
	{
		neededObjects.Remove(tileObjectType);
	}

	public void OnFinishedQuest(PartyQuest quest)
	{
		migrationComponent.OnFinishedQuest(quest);
	}

	public override void ConstructDefaultPlayerActions(bool broadcastSignal = true)
	{
		base.ConstructDefaultPlayerActions(broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.INDUCE_MIGRATION, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.STIFLE_MIGRATION, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.RAID, broadcastSignal);
		AddPlayerAction(PLAYER_SKILL_TYPE.CLEAR_VILLAGE, broadcastSignal);
	}

	public void SetOccupiedVillageSpot(VillageSpot p_spot)
	{
		occupiedVillageSpot = p_spot;
		occupiedVillageSpot.OccupyVillageSpot();
	}

	public void DestroySettlement()
	{
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Settlement", "WorldEvents_Table", "village_expiry", LOG_TAG.Major, LOG_TAG.Life_Changes);
		log.AddToFillers(this, base.name, LOG_IDENTIFIER.LANDMARK_1);
		log.AddLogToDatabase();
		PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
		Messenger.Broadcast(SettlementSignals.DISCONNECT_FROM_SETTLEMENT, this);
		migrationComponent.OnSettlementDestroyed();
		List<LocationStructure> list = RuinarchListPool<LocationStructure>.Claim();
		list.AddRange(base.allStructures);
		for (int i = 0; i < list.Count; i++)
		{
			list[i].DestroyStructureFromVillageExpiry();
		}
		RuinarchListPool<LocationStructure>.Release(list);
		List<JobQueueItem> list2 = RuinarchListPool<JobQueueItem>.Claim();
		list2.AddRange(availableJobs);
		Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Jobs", "CancelReasons_Table", "Location_Destroyed", LOG_TAG.Life_Changes);
		log2.AddToFillers(this, base.name, LOG_IDENTIFIER.LANDMARK_1);
		string logText = log2.logText;
		LogPool.Release(log2);
		for (int j = 0; j < list2.Count; j++)
		{
			list2[j].ForceCancelJob(logText);
		}
		RuinarchListPool<JobQueueItem>.Release(list2);
		for (int k = 0; k < base.areas.Count; k++)
		{
			Area p_area = base.areas[k];
			if (RemoveAreaFromSettlement(p_area))
			{
				k--;
			}
		}
		UnsubscribeToSignals();
		if (occupiedVillageSpot != null)
		{
			occupiedVillageSpot.VacateVillageSpot();
			occupiedVillageSpot = null;
		}
		DatabaseManager.Instance.settlementDatabase.UnRegisterSettlement(this);
	}

	protected override bool RemoveObjectOnFire(ITraitable traitable)
	{
		if (base.RemoveObjectOnFire(traitable))
		{
			settlementJobTriggerComponent.CheckDouseFireJobsValidity();
			return true;
		}
		return false;
	}

	public override string ToString()
	{
		return base.name;
	}

	public override void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		_ = prison;
		_ = mainStorage;
		_ = cityCenter;
		settlementJobTriggerComponent?.CheckIfStructureIsStillReferenced(p_structure);
		migrationComponent?.CheckIfStructureIsStillReferenced(p_structure);
		resourcesComponent?.CheckIfStructureIsStillReferenced(p_structure);
		classComponent?.CheckIfStructureIsStillReferenced(p_structure);
		partyComponent?.CheckIfStructureIsStillReferenced(p_structure);
		structureComponent?.CheckIfStructureIsStillReferenced(p_structure);
		expirationComponent?.CheckIfStructureIsStillReferenced(p_structure);
		tileObjectComponent?.CheckIfStructureIsStillReferenced(p_structure);
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = ruler;
		settlementJobTriggerComponent?.CheckIfCharacterIsStillReferenced(p_character);
		migrationComponent?.CheckIfCharacterIsStillReferenced(p_character);
		resourcesComponent?.CheckIfCharacterIsStillReferenced(p_character);
		classComponent?.CheckIfCharacterIsStillReferenced(p_character);
		partyComponent?.CheckIfCharacterIsStillReferenced(p_character);
		structureComponent?.CheckIfCharacterIsStillReferenced(p_character);
		expirationComponent?.CheckIfCharacterIsStillReferenced(p_character);
		tileObjectComponent?.CheckIfCharacterIsStillReferenced(p_character);
	}
}

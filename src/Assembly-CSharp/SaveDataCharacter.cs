using System;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UtilityScripts;

[Serializable]
public class SaveDataCharacter : SaveData<Character>, ISavableCounterpart
{
	public int id;

	public string firstName;

	public bool isDead;

	public GENDER gender;

	public SEXUALITY sexuality;

	public HAIR_COLOR hairColorType;

	public RACE race;

	public int currentHP;

	public int doNotRecoverHP;

	public Vector3 worldPos;

	public TileLocationSave deathTileLocation;

	public Quaternion rotation;

	public bool hasMarker;

	public bool hasExpiry;

	public GameDate markerExpiryDate;

	public PortraitSettings portraitSettings;

	public Color baseMarkerTint;

	public SaveDataMinion minion;

	public bool hasMinion;

	public List<INTERACTION_TYPE> advertisedActions;

	public bool canCombat;

	public bool hasUnresolvedCrime;

	public bool isInLimbo;

	public bool isLimboCharacter;

	public bool destroyMarkerOnDeath;

	public bool isWanderer;

	public bool hasBeenRaisedFromDead;

	public bool hasBlood;

	public bool isStoredAsTarget;

	public bool isDeadReference;

	public bool isRecruitedByAMajorFaction;

	public POI_STATE state;

	public INTERACTION_TYPE causeOfDeath;

	public List<PLAYER_SKILL_TYPE> afflictionsSkillsInflictedByPlayer;

	public SaveDataLycanthropeData lycanData;

	public bool hasLycan;

	public string grave;

	public Log deathLog;

	public string homeSettlement;

	public string homeStructure;

	public string currentStructure;

	public string faction;

	public string currentJob;

	public string currentActionNode;

	public string territory;

	public List<string> items;

	public List<string> ownedItems;

	public List<string> jobs;

	public List<string> pendingTopPriorityJobs;

	public List<string> forceCancelJobsOnTickEnded;

	public bool isInfoUnlocked;

	public string deployedAtStructure;

	public PLAYER_SKILL_TYPE resonancePower;

	public bool wasKilledByPlayerSource;

	public SaveDataTraitContainer saveDataTraitContainer;

	public SaveDataBaseRelationshipContainer saveDataBaseRelationshipContainer;

	public SaveDataTrapStructure trapStructure;

	public SaveDataCharacterClassComponent classComponent;

	public SaveDataCharacterNeedsComponent needsComponent;

	public SaveDataCharacterStructureComponent structureComponent;

	public SaveDataCharacterStateComponent stateComponent;

	public SaveDataNonActionEventsComponent nonActionEventsComponent;

	public SaveDataInterruptComponent interruptComponent;

	public SaveDataBehaviourComponent behaviourComponent;

	public SaveDataMoodComponent moodComponent;

	public SaveDataCharacterJobTriggerComponent jobComponent;

	public SaveDataReactionComponent reactionComponent;

	public SaveDataLogComponent logComponent;

	public SaveDataCombatComponent combatComponent;

	public SaveDataRumorComponent rumorComponent;

	public SaveDataAssumptionComponent assumptionComponent;

	public SaveDataMovementComponent movementComponent;

	public SaveDataStateAwarenessComponent stateAwarenessComponent;

	public SaveDataCarryComponent carryComponent;

	public SaveDataCharacterPartyComponent partyComponent;

	public SaveDataGatheringComponent gatheringComponent;

	public SaveDataCharacterTileObjectComponent tileObjectComponent;

	public SaveDataCrimeComponent crimeComponent;

	public SaveDataReligionComponent religionComponent;

	public SaveDataLimiterComponent limiterComponent;

	public SaveDataPiercingAndResistancesComponent piercingAndResistancesComponent;

	public SaveDataPreviousCharacterDataComponent previousCharacterDataComponent;

	public SaveDataCharacterTraitComponent traitComponent;

	public SaveDataCharacterMoneyComponent moneyComponent;

	public SaveDataEquipmentComponent equipmentComponent;

	public SaveDataCharacterTalentComponent talentComponent;

	public SaveDataResourceStorageComponent resourceStorageComponent;

	public SaveDataDailyScheduleComponent dailyScheduleComponent;

	public SaveDataPetComponent petComponent;

	public SaveDataCharacterMountComponent mountComponent;

	public string persistentID { get; set; }

	public OBJECT_TYPE objectType => OBJECT_TYPE.Character;

	public SaveDataVillagerWantsComponent villagerWantsComponent { get; set; }

	public override void Save(Character data)
	{
		persistentID = data.persistentID;
		id = data.id;
		firstName = data.name;
		isDead = data.isDead;
		gender = data.gender;
		sexuality = data.sexuality;
		hairColorType = data.hairColorType;
		race = data.race;
		currentHP = data.currentHP;
		doNotRecoverHP = data.doNotRecoverPassiveHP;
		portraitSettings = data.visuals.portraitSettings;
		baseMarkerTint = data.visuals.baseMarkerTint;
		advertisedActions = RuinarchListPool<INTERACTION_TYPE>.Claim();
		if (data.advertisedActions.Count > 0)
		{
			advertisedActions.AddRange(data.advertisedActions);
		}
		canCombat = data.canPersonalPatrol;
		hasUnresolvedCrime = data.hasUnresolvedCrime;
		isInLimbo = data.isInLimbo;
		isLimboCharacter = data.isLimboCharacter;
		destroyMarkerOnDeath = data.destroyMarkerOnDeath;
		isWanderer = data.isWanderer;
		hasBeenRaisedFromDead = data.hasBeenRaisedFromDead;
		hasBlood = data.visuals.HasBlood();
		state = data.state;
		causeOfDeath = data.causeOfDeath;
		afflictionsSkillsInflictedByPlayer = RuinarchListPool<PLAYER_SKILL_TYPE>.Claim();
		if (data.afflictionsSkillsInflictedByPlayer.Count > 0)
		{
			afflictionsSkillsInflictedByPlayer.AddRange(data.afflictionsSkillsInflictedByPlayer);
		}
		isStoredAsTarget = data.isStoredAsTarget;
		isDeadReference = data.isDeadReference;
		isRecruitedByAMajorFaction = data.isRecruitedByAMajorFaction;
		wasKilledByPlayerSource = data.wasKilledByPlayerSource;
		if (data.hasMarker)
		{
			hasMarker = true;
			worldPos = data.gridTileWorldPosition;
			if (data.marker.hasExpiry)
			{
				hasExpiry = true;
				markerExpiryDate = data.marker.destroyDate;
			}
		}
		deathTileLocation = ((data.deathTilePosition != null) ? new TileLocationSave(data.deathTilePosition) : default(TileLocationSave));
		trapStructure = new SaveDataTrapStructure();
		trapStructure.Save(data.trapStructure);
		classComponent = new SaveDataCharacterClassComponent();
		classComponent.Save(data.classComponent);
		needsComponent = new SaveDataCharacterNeedsComponent();
		needsComponent.Save(data.needsComponent);
		structureComponent = new SaveDataCharacterStructureComponent();
		structureComponent.Save(data.structureComponent);
		stateComponent = new SaveDataCharacterStateComponent();
		stateComponent.Save(data.stateComponent);
		nonActionEventsComponent = new SaveDataNonActionEventsComponent();
		nonActionEventsComponent.Save(data.nonActionEventsComponent);
		interruptComponent = new SaveDataInterruptComponent();
		interruptComponent.Save(data.interruptComponent);
		behaviourComponent = new SaveDataBehaviourComponent();
		behaviourComponent.Save(data.behaviourComponent);
		moodComponent = new SaveDataMoodComponent();
		moodComponent.Save(data.moodComponent);
		jobComponent = new SaveDataCharacterJobTriggerComponent();
		jobComponent.Save(data.jobComponent);
		reactionComponent = new SaveDataReactionComponent();
		reactionComponent.Save(data.reactionComponent);
		logComponent = new SaveDataLogComponent();
		logComponent.Save(data.logComponent);
		combatComponent = new SaveDataCombatComponent();
		combatComponent.Save(data.combatComponent);
		rumorComponent = new SaveDataRumorComponent();
		rumorComponent.Save(data.rumorComponent);
		assumptionComponent = new SaveDataAssumptionComponent();
		assumptionComponent.Save(data.assumptionComponent);
		movementComponent = new SaveDataMovementComponent();
		movementComponent.Save(data.movementComponent);
		stateAwarenessComponent = new SaveDataStateAwarenessComponent();
		stateAwarenessComponent.Save(data.stateAwarenessComponent);
		carryComponent = new SaveDataCarryComponent();
		carryComponent.Save(data.carryComponent);
		partyComponent = new SaveDataCharacterPartyComponent();
		partyComponent.Save(data.partyComponent);
		gatheringComponent = new SaveDataGatheringComponent();
		gatheringComponent.Save(data.gatheringComponent);
		tileObjectComponent = new SaveDataCharacterTileObjectComponent();
		tileObjectComponent.Save(data.tileObjectComponent);
		crimeComponent = new SaveDataCrimeComponent();
		crimeComponent.Save(data.crimeComponent);
		religionComponent = new SaveDataReligionComponent();
		religionComponent.Save(data.religionComponent);
		limiterComponent = new SaveDataLimiterComponent();
		limiterComponent.Save(data.limiterComponent);
		piercingAndResistancesComponent = new SaveDataPiercingAndResistancesComponent();
		piercingAndResistancesComponent.Save(data.piercingAndResistancesComponent);
		previousCharacterDataComponent = new SaveDataPreviousCharacterDataComponent();
		previousCharacterDataComponent.Save(data.previousCharacterDataComponent);
		traitComponent = new SaveDataCharacterTraitComponent();
		traitComponent.Save(data.traitComponent);
		moneyComponent = new SaveDataCharacterMoneyComponent();
		moneyComponent.Save(data.moneyComponent);
		equipmentComponent = new SaveDataEquipmentComponent();
		equipmentComponent.Save(data.equipmentComponent);
		resourceStorageComponent = new SaveDataResourceStorageComponent();
		resourceStorageComponent.Save(data.resourceStorageComponent);
		dailyScheduleComponent = new SaveDataDailyScheduleComponent();
		dailyScheduleComponent.Save(data.dailyScheduleComponent);
		mountComponent = new SaveDataCharacterMountComponent();
		mountComponent.Save(data.mountComponent);
		if (data.talentComponent != null)
		{
			talentComponent = new SaveDataCharacterTalentComponent();
			talentComponent.Save(data.talentComponent);
		}
		if (data.villagerWantsComponent != null)
		{
			villagerWantsComponent = new SaveDataVillagerWantsComponent();
			villagerWantsComponent.Save(data.villagerWantsComponent);
		}
		isInfoUnlocked = data.isInfoUnlocked;
		if (data.currentJob != null && data.currentJob.jobType != JOB_TYPE.NONE)
		{
			currentJob = data.currentJob.persistentID;
			SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data.currentJob);
		}
		if (data.currentActionNode != null)
		{
			currentActionNode = data.currentActionNode.persistentID;
			SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data.currentActionNode);
		}
		if (data.minion != null)
		{
			hasMinion = true;
			minion = new SaveDataMinion();
			minion.Save(data.minion);
		}
		if (data.isLycanthrope)
		{
			hasLycan = true;
			lycanData = new SaveDataLycanthropeData();
			lycanData.Save(data.lycanData);
		}
		if (data.grave != null)
		{
			grave = data.grave.persistentID;
		}
		if (data.deathLog != null)
		{
			deathLog = data.deathLog;
		}
		if (data.homeSettlement != null)
		{
			homeSettlement = data.homeSettlement.persistentID;
		}
		if (data.homeStructure != null)
		{
			homeStructure = data.homeStructure.persistentID;
		}
		if (data.currentStructure != null)
		{
			currentStructure = data.currentStructure.persistentID;
		}
		if (data.faction != null)
		{
			faction = data.faction.persistentID;
		}
		territory = string.Empty;
		if (data.HasTerritory())
		{
			territory = data.territory.persistentID;
		}
		items = RuinarchListPool<string>.Claim(data.items.Count);
		for (int i = 0; i < data.items.Count; i++)
		{
			items.Add(data.items[i].persistentID);
		}
		ownedItems = RuinarchListPool<string>.Claim(data.ownedItems.Count);
		for (int j = 0; j < data.ownedItems.Count; j++)
		{
			ownedItems.Add(data.ownedItems[j].persistentID);
		}
		jobs = RuinarchListPool<string>.Claim(data.jobQueue.jobsInQueue.Count);
		for (int k = 0; k < data.jobQueue.jobsInQueue.Count; k++)
		{
			JobQueueItem jobQueueItem = data.jobQueue.jobsInQueue[k];
			jobs.Add(jobQueueItem.persistentID);
		}
		pendingTopPriorityJobs = RuinarchListPool<string>.Claim(data.jobQueue.pendingTopPriorityJobs.Count);
		for (int l = 0; l < data.jobQueue.pendingTopPriorityJobs.Count; l++)
		{
			JobQueueItem jobQueueItem2 = data.jobQueue.pendingTopPriorityJobs[l];
			pendingTopPriorityJobs.Add(jobQueueItem2.persistentID);
		}
		forceCancelJobsOnTickEnded = RuinarchListPool<string>.Claim(data.forcedCancelJobsOnTickEnded.Count);
		for (int m = 0; m < data.forcedCancelJobsOnTickEnded.Count; m++)
		{
			JobQueueItem jobQueueItem3 = data.forcedCancelJobsOnTickEnded[m];
			if (jobQueueItem3.jobType != JOB_TYPE.NONE)
			{
				forceCancelJobsOnTickEnded.Add(jobQueueItem3.persistentID);
				SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(jobQueueItem3);
			}
		}
		if (data.deployedAtStructure != null)
		{
			deployedAtStructure = data.deployedAtStructure.persistentID;
		}
		resonancePower = data.resonancePower;
		saveDataTraitContainer = new SaveDataTraitContainer();
		saveDataTraitContainer.Save(data.traitContainer);
		saveDataBaseRelationshipContainer = new SaveDataBaseRelationshipContainer();
		saveDataBaseRelationshipContainer.Save(data.relationshipContainer as BaseRelationshipContainer);
		petComponent = new SaveDataPetComponent();
		petComponent.Save(data.petComponent);
	}

	public override Character Load()
	{
		return CharacterManager.Instance.CreateNewCharacter(this);
	}

	public override void CleanUp()
	{
		if (items != null)
		{
			RuinarchListPool<string>.Release(items);
			items = null;
		}
		if (ownedItems != null)
		{
			RuinarchListPool<string>.Release(ownedItems);
			ownedItems = null;
		}
		if (jobs != null)
		{
			RuinarchListPool<string>.Release(jobs);
			jobs = null;
		}
		if (forceCancelJobsOnTickEnded != null)
		{
			RuinarchListPool<string>.Release(forceCancelJobsOnTickEnded);
			forceCancelJobsOnTickEnded = null;
		}
		if (advertisedActions != null)
		{
			RuinarchListPool<INTERACTION_TYPE>.Release(advertisedActions);
			advertisedActions = null;
		}
		if (afflictionsSkillsInflictedByPlayer != null)
		{
			RuinarchListPool<PLAYER_SKILL_TYPE>.Release(afflictionsSkillsInflictedByPlayer);
			afflictionsSkillsInflictedByPlayer = null;
		}
		saveDataTraitContainer?.CleanUp();
		saveDataTraitContainer = null;
		saveDataBaseRelationshipContainer?.CleanUp();
		saveDataBaseRelationshipContainer = null;
		trapStructure?.CleanUp();
		trapStructure = null;
		classComponent?.CleanUp();
		classComponent = null;
		needsComponent?.CleanUp();
		needsComponent = null;
		structureComponent?.CleanUp();
		structureComponent = null;
		stateComponent?.CleanUp();
		stateComponent = null;
		nonActionEventsComponent?.CleanUp();
		nonActionEventsComponent = null;
		interruptComponent?.CleanUp();
		interruptComponent = null;
		behaviourComponent?.CleanUp();
		behaviourComponent = null;
		moodComponent?.CleanUp();
		moodComponent = null;
		jobComponent?.CleanUp();
		jobComponent = null;
		reactionComponent?.CleanUp();
		reactionComponent = null;
		logComponent?.CleanUp();
		logComponent = null;
		combatComponent?.CleanUp();
		combatComponent = null;
		rumorComponent?.CleanUp();
		rumorComponent = null;
		assumptionComponent?.CleanUp();
		assumptionComponent = null;
		movementComponent?.CleanUp();
		movementComponent = null;
		stateAwarenessComponent?.CleanUp();
		stateAwarenessComponent = null;
		carryComponent?.CleanUp();
		carryComponent = null;
		partyComponent?.CleanUp();
		partyComponent = null;
		gatheringComponent?.CleanUp();
		gatheringComponent = null;
		tileObjectComponent?.CleanUp();
		tileObjectComponent = null;
		crimeComponent?.CleanUp();
		crimeComponent = null;
		religionComponent?.CleanUp();
		religionComponent = null;
		limiterComponent?.CleanUp();
		limiterComponent = null;
		piercingAndResistancesComponent?.CleanUp();
		piercingAndResistancesComponent = null;
		previousCharacterDataComponent?.CleanUp();
		previousCharacterDataComponent = null;
		traitComponent?.CleanUp();
		traitComponent = null;
		moneyComponent?.CleanUp();
		moneyComponent = null;
		equipmentComponent?.CleanUp();
		equipmentComponent = null;
		talentComponent?.CleanUp();
		talentComponent = null;
		resourceStorageComponent?.CleanUp();
		resourceStorageComponent = null;
		dailyScheduleComponent?.CleanUp();
		dailyScheduleComponent = null;
		petComponent?.CleanUp();
		petComponent = null;
		mountComponent?.CleanUp();
		mountComponent = null;
	}
}

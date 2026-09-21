using System;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Maccima_Games.Util;
using Traits;
using UtilityScripts;

public class GoapPlanJob : JobQueueItem, IObjectPoolTester
{
	public static string Target_Already_Dead_Reason = "Target_Already_Dead";

	public GoapEffect goal { get; protected set; }

	public INTERACTION_TYPE targetInteractionType { get; protected set; }

	public GoapPlan assignedPlan { get; protected set; }

	public IPointOfInterest targetPOI { get; protected set; }

	public Dictionary<INTERACTION_TYPE, OtherData[]> otherData { get; protected set; }

	public bool shouldBeCancelledOnDeath { get; private set; }

	public Dictionary<INTERACTION_TYPE, List<ILocation>> priorityLocations { get; private set; }

	public bool isAssigned { get; set; }

	public bool isAgitateJob { get; private set; }

	public bool isTriggeredByPlayer { get; private set; }

	public override IPointOfInterest poiTarget => targetPOI;

	public override OBJECT_TYPE objectType => OBJECT_TYPE.Job;

	public override Type serializedData => typeof(SaveDataGoapPlanJob);

	public GoapPlanJob()
	{
		otherData = new Dictionary<INTERACTION_TYPE, OtherData[]>();
		priorityLocations = new Dictionary<INTERACTION_TYPE, List<ILocation>>();
	}

	public void Initialize(JOB_TYPE jobType, GoapEffect goal, IPointOfInterest targetPOI, IJobOwner owner)
	{
		Initialize(jobType, owner);
		this.goal = goal;
		this.targetPOI = targetPOI;
		shouldBeCancelledOnDeath = true;
		if (targetPOI is TileObject tileObject)
		{
			tileObject.AddExistingJobTargetingThis(this);
		}
	}

	public void Initialize(JOB_TYPE jobType, INTERACTION_TYPE targetInteractionType, IPointOfInterest targetPOI, IJobOwner owner)
	{
		Initialize(jobType, owner);
		this.targetPOI = targetPOI;
		this.targetInteractionType = targetInteractionType;
		shouldBeCancelledOnDeath = true;
		if (targetPOI is TileObject tileObject)
		{
			tileObject.AddExistingJobTargetingThis(this);
		}
	}

	public void Initialize(SaveDataGoapPlanJob data)
	{
		Initialize((SaveDataJobQueueItem)data);
		targetInteractionType = data.targetInteractionType;
		shouldBeCancelledOnDeath = data.shouldBeCancelledOnDeath;
		isAssigned = data.isAssigned;
		isAgitateJob = data.isAgitateJob;
		isTriggeredByPlayer = data.isTriggeredByPlayer;
	}

	public override bool LoadSecondWave(SaveDataJobQueueItem saveData)
	{
		bool result = base.LoadSecondWave(saveData);
		SaveDataGoapPlanJob saveDataGoapPlanJob = saveData as SaveDataGoapPlanJob;
		goal = InteractionManager.Instance.GetGoapEffectData(saveDataGoapPlanJob.goal);
		if (!string.IsNullOrEmpty(saveDataGoapPlanJob.targetPOIID))
		{
			if (saveDataGoapPlanJob.targetPOIObjectType == OBJECT_TYPE.Tile_Object)
			{
				targetPOI = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(saveDataGoapPlanJob.targetPOIID);
			}
			else if (saveDataGoapPlanJob.targetPOIObjectType == OBJECT_TYPE.Character)
			{
				targetPOI = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(saveDataGoapPlanJob.targetPOIID);
			}
			if (targetPOI == null)
			{
				result = false;
			}
			if (targetPOI is TileObject tileObject)
			{
				tileObject.AddExistingJobTargetingThis(this);
			}
			if (targetPOI != null && base.assignedCharacter != null)
			{
				targetPOI.AddJobTargetingThis(this);
			}
		}
		foreach (KeyValuePair<INTERACTION_TYPE, SaveDataOtherData[]> otherDatum in saveDataGoapPlanJob.otherData)
		{
			OtherData[] array = new OtherData[otherDatum.Value.Length];
			for (int i = 0; i < array.Length; i++)
			{
				SaveDataOtherData saveDataOtherData = otherDatum.Value[i];
				if (saveDataOtherData != null)
				{
					OtherData otherData = saveDataOtherData.Load();
					otherData.LoadAdditionalData(saveDataOtherData);
					array[i] = otherData;
				}
			}
			this.otherData.Add(otherDatum.Key, array);
		}
		if (saveDataGoapPlanJob.priorityLocations != null)
		{
			foreach (KeyValuePair<INTERACTION_TYPE, List<ILocationSaveData>> priorityLocation in saveDataGoapPlanJob.priorityLocations)
			{
				if (priorityLocation.Value == null)
				{
					continue;
				}
				priorityLocations.Add(priorityLocation.Key, RuinarchListPool<ILocation>.Claim(priorityLocation.Value.Capacity));
				for (int j = 0; j < priorityLocation.Value.Count; j++)
				{
					ILocationSaveData locationSaveData = priorityLocation.Value[j];
					if (locationSaveData.objectType == OBJECT_TYPE.Settlement)
					{
						BaseSettlement settlementByPersistentIDSafe = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentIDSafe(locationSaveData.persistentID);
						if (settlementByPersistentIDSafe != null)
						{
							priorityLocations[priorityLocation.Key].Add(settlementByPersistentIDSafe);
						}
					}
					else if (locationSaveData.objectType == OBJECT_TYPE.Structure)
					{
						LocationStructure structureByPersistentIDSafe = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentIDSafe(locationSaveData.persistentID);
						if (structureByPersistentIDSafe != null)
						{
							priorityLocations[priorityLocation.Key].Add(structureByPersistentIDSafe);
						}
					}
					else if (locationSaveData.objectType == OBJECT_TYPE.Area)
					{
						Area areaByPersistentID = DatabaseManager.Instance.areaDatabase.GetAreaByPersistentID(locationSaveData.persistentID);
						if (areaByPersistentID != null)
						{
							priorityLocations[priorityLocation.Key].Add(areaByPersistentID);
						}
					}
				}
			}
		}
		if (saveDataGoapPlanJob.saveDataGoapPlan != null)
		{
			GoapPlan goapPlan = saveDataGoapPlanJob.saveDataGoapPlan.Load();
			if (goapPlan.allNodes.Count <= 0)
			{
				assignedPlan = null;
				result = false;
			}
			else
			{
				assignedPlan = goapPlan;
			}
		}
		return result;
	}

	public override bool ProcessJob()
	{
		if (base.hasBeenReset)
		{
			return false;
		}
		if (assignedPlan == null && base.originalOwner != null && base.assignedCharacter != null && base.assignedCharacter.carryComponent.IsNotBeingCarried() && !base.assignedCharacter.isBeingSeized)
		{
			Character character = base.assignedCharacter;
			bool isPersonalPlan = base.originalOwner.ownerType == JOB_OWNER.CHARACTER;
			IPointOfInterest target = targetPOI ?? base.assignedCharacter;
			if (targetInteractionType != INTERACTION_TYPE.NONE)
			{
				character.planner.StartGOAP(targetInteractionType, target, this, isPersonalPlan);
			}
			else
			{
				character.planner.StartGOAP(goal, target, this, isPersonalPlan);
			}
			return true;
		}
		return base.ProcessJob();
	}

	public override bool CancelJob(string reason = "", bool shouldBlacklist = false)
	{
		if (base.assignedCharacter == null)
		{
			return false;
		}
		return base.assignedCharacter.jobQueue.RemoveJobInQueue(this, reason, shouldBlacklist);
	}

	public override bool ForceCancelJob(string reason = "", bool shouldBlacklist = false)
	{
		if (base.assignedCharacter != null)
		{
			Character character = base.assignedCharacter;
			JOB_OWNER ownerType = base.originalOwner.ownerType;
			bool result = character.jobQueue.RemoveJobInQueue(this, reason, shouldBlacklist);
			if (ownerType == JOB_OWNER.CHARACTER)
			{
				return result;
			}
		}
		if (base.originalOwner != null)
		{
			return base.originalOwner.ForceCancelJob(this);
		}
		return true;
	}

	public override void UnassignJob(string reason)
	{
		base.UnassignJob(reason);
		if (base.assignedCharacter == null)
		{
			return;
		}
		if (assignedPlan != null)
		{
			if (base.assignedCharacter.currentActionNode != null && assignedPlan.currentNode != null && base.assignedCharacter.currentActionNode == assignedPlan.currentActualNode)
			{
				base.assignedCharacter.StopCurrentActionNode(reason);
			}
			SetAssignedPlan(null);
		}
		SetAssignedCharacter(null);
	}

	public override void OnAddJobToQueue()
	{
		if (targetPOI != null)
		{
			targetPOI.AddJobTargetingThis(this);
		}
	}

	public override bool OnRemoveJobFromQueue()
	{
		if (targetPOI != null)
		{
			return targetPOI.RemoveJobTargetingThis(this);
		}
		return false;
	}

	protected override bool CanTakeJob(Character character)
	{
		if (targetPOI == null)
		{
			return true;
		}
		if (targetPOI.poiType == POINT_OF_INTEREST_TYPE.CHARACTER)
		{
			return (targetPOI as Character).carryComponent.IsNotBeingCarried();
		}
		if (base.jobType == JOB_TYPE.REMOVE_STATUS && (goal == null || (!string.IsNullOrEmpty(goal.conditionKey) && targetPOI.traitContainer.GetTraitOrStatus<Trait>(goal.conditionKey).IsResponsibleForTrait(character))))
		{
			return false;
		}
		return base.CanTakeJob(character);
	}

	public override string ToString()
	{
		return GetJobDetailString();
	}

	public override void AddOtherData(INTERACTION_TYPE actionType, object[] data)
	{
		OtherData[] array = new OtherData[data.Length];
		for (int i = 0; i < data.Length; i++)
		{
			object obj = data[i];
			OtherData otherData = null;
			if (obj is LocationGridTile tile)
			{
				otherData = new LocationGridTileOtherData(tile);
			}
			else if (obj is LocationStructure locationStructure)
			{
				otherData = new LocationStructureOtherData(locationStructure);
			}
			else if (obj is Area p_area)
			{
				otherData = new AreaOtherData(p_area);
			}
			else if (obj is int integer)
			{
				otherData = new IntOtherData(integer);
			}
			else if (obj is TileObject tileObject)
			{
				otherData = new TileObjectOtherData(tileObject);
			}
			else if (obj is Region region)
			{
				otherData = new RegionOtherData(region);
			}
			else if (obj is BaseSettlement settlement)
			{
				otherData = new SettlementOtherData(settlement);
			}
			else if (obj is Faction faction)
			{
				otherData = new FactionOtherData(faction);
			}
			else if (obj is CrimeData crimeData)
			{
				otherData = new CrimeDataOtherData(crimeData);
			}
			else if (obj is ICrimeable crimeable)
			{
				otherData = new CrimeableOtherData(crimeable);
			}
			else if (obj is ActualGoapNode action)
			{
				otherData = new ActualGoapNodeOtherData(action);
			}
			else if (obj is Rumor rumor)
			{
				otherData = new RumorOtherData(rumor);
			}
			else if (obj is Character character)
			{
				otherData = new CharacterOtherData(character);
			}
			else if (obj is string str)
			{
				otherData = new StringOtherData(str);
			}
			else if (obj is TileObjectRecipe recipe)
			{
				otherData = new TileObjectRecipeOtherData(recipe);
			}
			else if (obj is StructureSetting structureSetting)
			{
				otherData = new StructureSettingOtherData(structureSetting);
			}
			if (otherData != null)
			{
				array[i] = otherData;
				otherData?.IncreaseReferenceCount();
			}
		}
		this.otherData[actionType] = array;
	}

	public override void AddOtherData(INTERACTION_TYPE actionType, OtherData[] data)
	{
		otherData[actionType] = data;
		if (data != null)
		{
			for (int i = 0; i < data.Length; i++)
			{
				data[i]?.IncreaseReferenceCount();
			}
		}
	}

	public bool HasOtherDataRelatedTo(object p_object)
	{
		foreach (KeyValuePair<INTERACTION_TYPE, OtherData[]> otherDatum in this.otherData)
		{
			for (int i = 0; i < otherDatum.Value.Length; i++)
			{
				OtherData otherData = otherDatum.Value[i];
				if (otherData != null && otherData.obj == p_object)
				{
					return true;
				}
			}
		}
		return false;
	}

	public override bool CanBeInterruptedBy(JOB_TYPE jobType)
	{
		if (assignedPlan != null && assignedPlan.currentActualNode.actionStatus == ACTION_STATUS.PERFORMING)
		{
			if (jobType == JOB_TYPE.COMBAT)
			{
				return true;
			}
			return false;
		}
		return base.CanBeInterruptedBy(jobType);
	}

	protected override void CheckJobApplicability(JOB_TYPE p_jobType, IPointOfInterest p_targetPOI)
	{
		if (base.jobType == p_jobType && targetPOI == p_targetPOI && !IsJobStillApplicable())
		{
			base.originalOwner.AddForcedCancelJobsOnTickEnded(this);
		}
	}

	protected override void CheckJobApplicability(IPointOfInterest p_targetPOI)
	{
		if (targetPOI == p_targetPOI && !IsJobStillApplicable())
		{
			base.originalOwner.AddForcedCancelJobsOnTickEnded(this);
		}
	}

	protected override void CheckJobApplicability(JOB_TYPE p_jobType)
	{
		if (base.jobType == p_jobType && !IsJobStillApplicable())
		{
			base.originalOwner.AddForcedCancelJobsOnTickEnded(this);
		}
	}

	protected override void DisconnectFromStructure(LocationStructure p_structure)
	{
		base.DisconnectFromStructure(p_structure);
		_ = string.Empty;
		Dictionary<INTERACTION_TYPE, List<ILocation>> dictionary = new Dictionary<INTERACTION_TYPE, List<ILocation>>(priorityLocations);
		foreach (KeyValuePair<INTERACTION_TYPE, List<ILocation>> item in dictionary)
		{
			if (item.Value.Contains(p_structure))
			{
				priorityLocations[item.Key].Remove(p_structure);
			}
		}
		MaccimaDictionaryPool<INTERACTION_TYPE, List<ILocation>>.Release(dictionary);
		bool shouldCancelPlan = false;
		if (assignedPlan != null)
		{
			assignedPlan.DisconnectFromStructure(p_structure, out shouldCancelPlan);
		}
		bool flag = false;
		foreach (KeyValuePair<INTERACTION_TYPE, OtherData[]> otherDatum in this.otherData)
		{
			for (int i = 0; i < otherDatum.Value.Length; i++)
			{
				OtherData otherData = otherDatum.Value[i];
				if (otherData.IsStructureReferenced(p_structure) || otherData.IsOtherDataInvalid())
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Jobs", "CancelReasons_Table", "Location_Destroyed", LOG_TAG.Life_Changes);
			log.AddToFillers(p_structure, p_structure.name, LOG_IDENTIFIER.LANDMARK_1);
			string logText = log.logText;
			if (base.assignedCharacter != null && base.assignedCharacter.currentJob == this)
			{
				base.assignedCharacter.StopCurrentActionNode(logText);
			}
			if (!base.hasBeenReset)
			{
				Reset();
			}
		}
		else if (shouldCancelPlan)
		{
			Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Jobs", "CancelReasons_Table", "Location_Destroyed", LOG_TAG.Life_Changes);
			log2.AddToFillers(p_structure, p_structure.name, LOG_IDENTIFIER.LANDMARK_1);
			string logText2 = log2.logText;
			if (base.assignedCharacter != null && base.assignedCharacter.currentJob == this)
			{
				base.assignedCharacter.StopCurrentActionNode(logText2);
			}
			SetAssignedPlan(null);
		}
	}

	protected override void DisconnectFromCharacter(Character p_character)
	{
		base.DisconnectFromCharacter(p_character);
		_ = string.Empty;
		bool shouldCancelPlan = false;
		if (assignedPlan != null)
		{
			assignedPlan.DisconnectFromCharacter(p_character, out shouldCancelPlan);
		}
		bool flag = targetPOI == p_character || p_character == base.originalOwner;
		if (!flag)
		{
			foreach (KeyValuePair<INTERACTION_TYPE, OtherData[]> otherDatum in this.otherData)
			{
				for (int i = 0; i < otherDatum.Value.Length; i++)
				{
					OtherData otherData = otherDatum.Value[i];
					if (otherData.IsCharacterReferenced(p_character) || otherData.IsOtherDataInvalid())
					{
						flag = true;
						break;
					}
				}
			}
		}
		if (flag)
		{
			if (base.assignedCharacter != null && base.assignedCharacter.currentJob == this)
			{
				base.assignedCharacter.StopCurrentActionNode();
			}
			ForceCancelJob();
			if (!base.hasBeenReset)
			{
				Reset();
			}
		}
		else if (shouldCancelPlan)
		{
			if (base.assignedCharacter != null && base.assignedCharacter.currentJob == this)
			{
				base.assignedCharacter.StopCurrentActionNode();
			}
			SetAssignedPlan(null);
		}
	}

	protected override void OnCrimeRemovedFromDatabase(CrimeData p_crime)
	{
		base.OnCrimeRemovedFromDatabase(p_crime);
		bool flag = false;
		foreach (KeyValuePair<INTERACTION_TYPE, OtherData[]> otherDatum in this.otherData)
		{
			for (int i = 0; i < otherDatum.Value.Length; i++)
			{
				OtherData otherData = otherDatum.Value[i];
				if (otherData.obj == p_crime || otherData.IsOtherDataInvalid())
				{
					flag = true;
					break;
				}
			}
		}
		if (flag && !ForceCancelJob())
		{
			Reset();
		}
	}

	public void SetAssignedPlan(GoapPlan plan)
	{
		if (assignedPlan == plan)
		{
			return;
		}
		GoapPlan goapPlan = assignedPlan;
		assignedPlan = plan;
		if (goapPlan != null)
		{
			goapPlan.OnUnattachPlanToJob(this);
			if (goapPlan.isBeingRecalculated)
			{
				goapPlan.SetResetPlanOnFinishRecalculation(p_state: true);
			}
			else
			{
				ObjectPoolManager.Instance.ReturnGoapPlanToPool(goapPlan);
			}
		}
		plan?.OnAttachPlanToJob(this);
	}

	public string GetJobDetailString()
	{
		switch (base.jobType)
		{
		case JOB_TYPE.REMOVE_STATUS:
		{
			if (goal == null)
			{
				return LocalizationManager.Instance.GetLocalizedValue("Jobs_Table", "REMOVE_STATUS");
			}
			Dictionary<string, string> dictionary2 = MaccimaDictionaryPool<string, string>.Claim();
			string value2 = TraitManager.Instance.GetLocalizedNameOfTrait(goal.conditionKey);
			if (string.IsNullOrEmpty(value2))
			{
				value2 = goal.conditionKey;
			}
			dictionary2.Add("text1", value2);
			string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("GoapActionsStrings_Table", "Remove_Status", dictionary2);
			MaccimaDictionaryPool<string, string>.Release(dictionary2);
			return localizedValue2;
		}
		case JOB_TYPE.OBTAIN_PERSONAL_ITEM:
		{
			Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
			string value = LocalizationManager.Instance.GetLocalizedValue("TileObjects_Table", goal.conditionKey);
			if (string.IsNullOrEmpty(value))
			{
				value = goal.conditionKey;
			}
			dictionary.Add("text1", value);
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("GoapActionsStrings_Table", "Obtain_Personal_Item", dictionary);
			MaccimaDictionaryPool<string, string>.Release(dictionary);
			return localizedValue;
		}
		case JOB_TYPE.FULLNESS_RECOVERY_URGENT:
		case JOB_TYPE.FULLNESS_RECOVERY_NORMAL:
			return LocalizationManager.Instance.GetLocalizedValue("GoapActionsStrings_Table", "Hunger_Recovery");
		case JOB_TYPE.HAPPINESS_RECOVERY:
			return LocalizationManager.Instance.GetLocalizedValue("GoapActionsStrings_Table", "Happiness_Recovery");
		case JOB_TYPE.ENERGY_RECOVERY_URGENT:
		case JOB_TYPE.ENERGY_RECOVERY_NORMAL:
			return LocalizationManager.Instance.GetLocalizedValue("GoapActionsStrings_Table", "Tiredness_Recovery");
		default:
			if (targetInteractionType != INTERACTION_TYPE.NONE)
			{
				return LocalizationManager.Instance.GetLocalizedValue("GoapActionsStrings_Table", targetInteractionType.ToStringEnum());
			}
			return base.name;
		}
	}

	public void SetCancelOnDeath(bool state)
	{
		shouldBeCancelledOnDeath = state;
	}

	public bool HasOtherData(INTERACTION_TYPE p_actionType)
	{
		return otherData.ContainsKey(p_actionType);
	}

	public bool HasOtherData(INTERACTION_TYPE p_actionType, object p_obj)
	{
		if (otherData.ContainsKey(p_actionType))
		{
			OtherData[] array = otherData[p_actionType];
			if (array != null)
			{
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i].obj.Equals(p_obj))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public bool HasFoodProducerOtherData(INTERACTION_TYPE p_actionType)
	{
		if (otherData.ContainsKey(p_actionType))
		{
			OtherData[] array = otherData[p_actionType];
			if (array != null)
			{
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i].obj is string text && text.IsFoodProducerClassName())
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public OtherData[] GetOtherDataSpecific(INTERACTION_TYPE actionType)
	{
		if (HasOtherData(actionType))
		{
			return otherData[actionType];
		}
		return null;
	}

	public OtherData[] GetOtherDataFor(INTERACTION_TYPE actionType)
	{
		OtherData[] result = null;
		if (otherData.ContainsKey(actionType))
		{
			result = otherData[actionType];
		}
		else if (otherData.ContainsKey(INTERACTION_TYPE.NONE))
		{
			result = otherData[INTERACTION_TYPE.NONE];
		}
		return result;
	}

	public void AddPriorityLocation(INTERACTION_TYPE actionType, ILocation location)
	{
		if (!priorityLocations.ContainsKey(actionType))
		{
			priorityLocations.Add(actionType, RuinarchListPool<ILocation>.Claim(5));
		}
		if (!priorityLocations[actionType].Contains(location))
		{
			priorityLocations[actionType].Add(location);
		}
	}

	public List<ILocation> GetPriorityLocationsFor(INTERACTION_TYPE actionType)
	{
		List<ILocation> result = null;
		if (priorityLocations.ContainsKey(actionType))
		{
			result = priorityLocations[actionType];
		}
		else if (priorityLocations.ContainsKey(INTERACTION_TYPE.NONE))
		{
			result = priorityLocations[INTERACTION_TYPE.NONE];
		}
		return result;
	}

	public void SetIsAgitateJob(bool p_state)
	{
		isAgitateJob = p_state;
	}

	public void SetIsTriggeredByPlayer(bool p_state)
	{
		isTriggeredByPlayer = p_state;
	}

	public bool HasGoalConditionKey(string key)
	{
		if (goal != null)
		{
			return goal.conditionKey == key;
		}
		return false;
	}

	public bool HasGoalConditionType(GOAP_EFFECT_CONDITION conditionType)
	{
		if (goal != null)
		{
			return goal.conditionType == conditionType;
		}
		return false;
	}

	public override void Reset()
	{
		if (targetPOI != null)
		{
			targetPOI.RemoveJobTargetingThis(this);
			if (targetPOI is TileObject tileObject)
			{
				tileObject.RemoveExistingJobTargetingThis(this);
			}
		}
		base.Reset();
		isAgitateJob = false;
		isTriggeredByPlayer = false;
		goal = null;
		targetPOI = null;
		targetInteractionType = INTERACTION_TYPE.NONE;
		SetAssignedPlan(null);
		shouldBeCancelledOnDeath = true;
		foreach (List<ILocation> value in priorityLocations.Values)
		{
			RuinarchListPool<ILocation>.Release(value);
		}
		priorityLocations.Clear();
		CleanUpAfterActionIsDone();
	}

	public override void CleanUpAfterActionIsDone()
	{
		base.CleanUpAfterActionIsDone();
		if (this.otherData == null)
		{
			return;
		}
		foreach (KeyValuePair<INTERACTION_TYPE, OtherData[]> otherDatum in this.otherData)
		{
			for (int i = 0; i < otherDatum.Value.Length; i++)
			{
				OtherData otherData = otherDatum.Value[i];
				if (otherData != null)
				{
					otherData.DecreaseReferenceCount();
					if (otherData.actionReferenceCount <= 0)
					{
						otherData.CleanUp();
					}
				}
			}
		}
		this.otherData.Clear();
	}

	public override void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		base.CheckIfStructureIsStillReferenced(p_structure);
		foreach (KeyValuePair<INTERACTION_TYPE, OtherData[]> otherDatum in otherData)
		{
			for (int i = 0; i < otherDatum.Value.Length; i++)
			{
				otherDatum.Value[i].CheckIfStructureIsStillReferenced(p_structure);
			}
		}
		foreach (KeyValuePair<INTERACTION_TYPE, List<ILocation>> priorityLocation in priorityLocations)
		{
			priorityLocation.Value.Contains(p_structure);
		}
		assignedPlan?.CheckIfStructureIsStillReferenced(p_structure);
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		foreach (KeyValuePair<INTERACTION_TYPE, OtherData[]> otherDatum in otherData)
		{
			for (int i = 0; i < otherDatum.Value.Length; i++)
			{
				otherDatum.Value[i].CheckIfCharacterIsStillReferenced(p_character);
			}
		}
		_ = targetPOI;
		assignedPlan?.CheckIfCharacterIsStillReferenced(p_character);
	}
}

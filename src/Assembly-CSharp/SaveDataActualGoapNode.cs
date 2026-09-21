using System;
using System.Collections.Generic;
using Goap.Unique_Action_Data;
using Object_Pools;
using UtilityScripts;

[Serializable]
public class SaveDataActualGoapNode : SaveData<ActualGoapNode>, ISavableCounterpart
{
	public string instanceID;

	public string actor;

	public string poiTarget;

	public POINT_OF_INTEREST_TYPE poiTargetType;

	public string disguisedActor;

	public string disguisedTarget;

	public bool isStealth;

	public bool isAvoidWitnesses;

	public bool avoidCombat;

	public SaveDataOtherData[] otherData;

	public int cost;

	public bool isIntel;

	public bool isNegativeInfo;

	public bool isSupposedToBeInPool;

	public bool isUsedAsCrime;

	public int stillProcessingCounter;

	public bool hasBeenReset;

	public bool isAssigned;

	public bool hasStartedPerTickEffect;

	public INTERACTION_TYPE action;

	public ACTION_STATUS actionStatus;

	public Log thoughtBubbleLog;

	public Log thoughtBubbleMovingLog;

	public Log descriptionLog;

	public string targetStructure;

	public string targetTile;

	public string targetPOIToGoTo;

	public POINT_OF_INTEREST_TYPE targetPOIToGoToType;

	public JOB_TYPE associatedJobType;

	public string associatedJobID;

	public string currentStateName;

	public int ticksPerformingCurrentState;

	public int expectedActionStateDuration;

	public SaveDataRumor rumor;

	public SaveDataAssumption assumption;

	public bool hasRumor;

	public bool hasAssumption;

	public bool isIllusion;

	public bool isFabricated;

	public List<string> awareCharacters;

	public List<LOG_TAG> logTags;

	public CRIME_TYPE crimeType;

	public SaveDataUniqueActionData uniqueActionData;

	public string persistentID { get; set; }

	public OBJECT_TYPE objectType => OBJECT_TYPE.Action;

	public override void Save(ActualGoapNode data)
	{
		instanceID = data.instanceID;
		persistentID = data.persistentID;
		isStealth = data.isStealth;
		isAvoidWitnesses = data.isAvoidWitnesses;
		avoidCombat = data.avoidCombat;
		cost = data.cost;
		action = data.action.goapType;
		actionStatus = data.actionStatus;
		associatedJobType = data.associatedJobType;
		currentStateName = data.currentStateName;
		ticksPerformingCurrentState = data.ticksPerformingCurrentState;
		expectedActionStateDuration = data.expectedActionStateDuration;
		crimeType = data.crimeType;
		actor = data.actor.persistentID;
		poiTarget = data.poiTarget.persistentID;
		poiTargetType = data.poiTarget.poiType;
		isIntel = data.isIntel;
		isNegativeInfo = data.isNegativeInfo;
		isUsedAsCrime = data.isUsedAsCrime;
		isSupposedToBeInPool = data.isSupposedToBeInPool;
		stillProcessingCounter = data.stillProcessingCounter;
		hasBeenReset = data.hasBeenReset;
		logTags = RuinarchListPool<LOG_TAG>.Claim();
		logTags.AddRange(data.logTags);
		hasStartedPerTickEffect = data.hasStartedPerTickEffect;
		disguisedActor = string.Empty;
		disguisedTarget = string.Empty;
		if (data.disguisedActor != null)
		{
			disguisedActor = data.disguisedActor.persistentID;
		}
		if (data.disguisedTarget != null)
		{
			disguisedTarget = data.disguisedTarget.persistentID;
		}
		if (data.thoughtBubbleLog != null)
		{
			thoughtBubbleLog = LogPool.Claim();
			thoughtBubbleLog.Copy(data.thoughtBubbleLog);
		}
		if (data.thoughtBubbleMovingLog != null)
		{
			thoughtBubbleMovingLog = LogPool.Claim();
			thoughtBubbleMovingLog.Copy(data.thoughtBubbleMovingLog);
		}
		if (data.descriptionLog != null)
		{
			descriptionLog = LogPool.Claim();
			descriptionLog.Copy(data.descriptionLog);
		}
		if (data.targetStructure != null)
		{
			targetStructure = data.targetStructure.persistentID;
		}
		if (data.targetPOIToGoTo != null)
		{
			targetPOIToGoTo = data.targetPOIToGoTo.persistentID;
			targetPOIToGoToType = data.targetPOIToGoTo.poiType;
		}
		awareCharacters = RuinarchListPool<string>.Claim();
		if (data.awareCharacters != null && data.awareCharacters.Count > 0)
		{
			for (int i = 0; i < data.awareCharacters.Count; i++)
			{
				Character character = data.awareCharacters[i];
				if (character == null)
				{
					data.awareCharacters.RemoveAt(i);
					i--;
				}
				else
				{
					awareCharacters.Add(character.persistentID);
				}
			}
		}
		if (data.rumor != null)
		{
			hasRumor = true;
			rumor = new SaveDataRumor();
			rumor.Save(data.rumor);
		}
		if (data.assumption != null)
		{
			hasAssumption = true;
			assumption = new SaveDataAssumption();
			assumption.Save(data.assumption);
		}
		if (data.otherData != null)
		{
			this.otherData = new SaveDataOtherData[data.otherData.Length];
			for (int j = 0; j < this.otherData.Length; j++)
			{
				OtherData otherData = data.otherData[j];
				if (otherData != null)
				{
					this.otherData[j] = otherData.Save();
				}
			}
		}
		if (data.associatedJob != null && data.associatedJob.jobType != JOB_TYPE.NONE)
		{
			associatedJobID = data.associatedJob.persistentID;
		}
		if (data.uniqueActionData != null)
		{
			uniqueActionData = data.uniqueActionData.Save();
		}
		isAssigned = data.isAssigned;
		isIllusion = data.isIllusion;
		isFabricated = data.isFabricated;
	}

	public override ActualGoapNode Load()
	{
		return new ActualGoapNode(this);
	}

	public override void CleanUp()
	{
		if (thoughtBubbleLog != null)
		{
			LogPool.Release(thoughtBubbleLog);
		}
		if (thoughtBubbleMovingLog != null)
		{
			LogPool.Release(thoughtBubbleMovingLog);
		}
		if (descriptionLog != null)
		{
			LogPool.Release(descriptionLog);
		}
		if (otherData != null)
		{
			for (int i = 0; i < otherData.Length; i++)
			{
				otherData[i].CleanUp();
			}
			otherData = null;
		}
		if (awareCharacters != null)
		{
			RuinarchListPool<string>.Release(awareCharacters);
			awareCharacters = null;
		}
		if (logTags != null)
		{
			RuinarchListPool<LOG_TAG>.Release(logTags);
			logTags = null;
		}
	}
}

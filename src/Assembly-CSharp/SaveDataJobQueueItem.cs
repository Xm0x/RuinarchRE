using System;
using System.Collections.Generic;
using UtilityScripts;

[Serializable]
public abstract class SaveDataJobQueueItem : SaveData<JobQueueItem>, ISavableCounterpart
{
	public string _persistentID;

	public OBJECT_TYPE _objectType;

	public int id;

	public string originalOwnerID;

	public OBJECT_TYPE originalOwnerType;

	public string assignedCharacterID;

	public string name;

	public JOB_TYPE jobType;

	public List<string> blacklistedCharacterIDs;

	public bool doNotRecalculate;

	public int invalidCounter;

	public bool isThisAPartyJob;

	public bool isThisAGatheringJob;

	public bool cannotBePushedBack;

	public bool shouldBeRemovedFromSettlementWhenUnassigned;

	public bool forceCancelOnInvalid;

	public FACTION_IDEOLOGY connectedFactionIdeology;

	public string canTakeJobKey;

	public string applicabilityCheckerKey;

	public string persistentID => _persistentID;

	public OBJECT_TYPE objectType => _objectType;

	public override void Save(JobQueueItem job)
	{
		_persistentID = job.persistentID;
		_objectType = job.objectType;
		id = job.id;
		if (job.originalOwner != null)
		{
			originalOwnerID = job.originalOwner.persistentID;
			originalOwnerType = job.originalOwner.objectType;
		}
		else
		{
			originalOwnerID = string.Empty;
			originalOwnerType = OBJECT_TYPE.Character;
		}
		assignedCharacterID = ((job.assignedCharacter == null) ? string.Empty : job.assignedCharacter.persistentID);
		name = job.name;
		jobType = job.jobType;
		blacklistedCharacterIDs = RuinarchListPool<string>.Claim();
		for (int i = 0; i < job.blacklistedCharacters.Count; i++)
		{
			Character character = job.blacklistedCharacters[i];
			blacklistedCharacterIDs.Add(character.persistentID);
		}
		doNotRecalculate = job.doNotRecalculate;
		invalidCounter = job.invalidCounter;
		isThisAPartyJob = job.isThisAPartyJob;
		isThisAGatheringJob = job.isThisAGatheringJob;
		cannotBePushedBack = job.cannotBePushedBack;
		shouldBeRemovedFromSettlementWhenUnassigned = job.shouldBeRemovedFromSettlementWhenUnassigned;
		forceCancelOnInvalid = job.forceCancelOnInvalid;
		connectedFactionIdeology = job.connectedFactionIdeology;
		canTakeJobKey = ((job.canTakeJobChecker == null) ? string.Empty : job.canTakeJobChecker.key);
		applicabilityCheckerKey = ((job.stillApplicable == null) ? string.Empty : job.stillApplicable.key);
	}

	public abstract override JobQueueItem Load();

	public override void CleanUp()
	{
		if (blacklistedCharacterIDs != null)
		{
			RuinarchListPool<string>.Release(blacklistedCharacterIDs);
			blacklistedCharacterIDs = null;
		}
	}
}

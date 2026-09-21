using System;
using Inner_Maps.Location_Structures;

public class DemonStealPartyQuest : PartyQuest
{
	public TileObject targetItem { get; private set; }

	public LocationStructure dropStructure { get; private set; }

	public override IPartyQuestTarget target => targetItem;

	public override Type serializedData => typeof(SaveDataDemonStealPartyQuest);

	public override bool waitingToWorkingStateImmediately => true;

	public DemonStealPartyQuest()
		: base(PARTY_QUEST_TYPE.Demon_Steal)
	{
		base.minimumPartySize = 1;
		base.priority = 5;
		base.relatedBehaviour = typeof(DemonStealBehaviour);
	}

	public DemonStealPartyQuest(SaveDataDemonStealPartyQuest data)
		: base(data)
	{
	}

	public override void OnAcceptQuest(Party partyThatAcceptedQuest)
	{
		base.OnAcceptQuest(partyThatAcceptedQuest);
		Messenger.AddListener<Character, GoapPlanJob>(CharacterSignals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, CheckIfStealJobIsFinished);
		Messenger.AddListener<IPointOfInterest>(CharacterSignals.ON_UNSEIZE_POI, OnUnseizePOI);
		Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnStealJobRemoved);
	}

	protected override void OnEndQuest()
	{
		base.OnEndQuest();
		Messenger.RemoveListener<IPointOfInterest>(CharacterSignals.ON_UNSEIZE_POI, OnUnseizePOI);
		Messenger.RemoveListener<Character, GoapPlanJob>(CharacterSignals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, CheckIfStealJobIsFinished);
		Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnStealJobRemoved);
	}

	public override IPartyTargetDestination GetTargetDestination()
	{
		if (targetItem.currentStructure != null && targetItem.currentStructure.structureType != STRUCTURE_TYPE.WILDERNESS)
		{
			return targetItem.currentStructure;
		}
		if (targetItem.gridTileLocation != null)
		{
			return targetItem.gridTileLocation.area;
		}
		return base.GetTargetDestination();
	}

	public override string GetPartyQuestName()
	{
		return base.localizedPartialQuestName + " " + targetItem.name;
	}

	protected override bool IsConnectedToStructure(LocationStructure p_structure)
	{
		return false;
	}

	public override bool IsStillEligibleFor(Faction p_faction)
	{
		return targetItem != null;
	}

	public void SetTargetItem(TileObject p_item)
	{
		targetItem = p_item;
	}

	public void SetDropStructure(LocationStructure p_structure)
	{
		dropStructure = p_structure;
	}

	private void CheckIfStealJobIsFinished(Character p_character, GoapPlanJob p_job)
	{
		if (p_job.jobType == JOB_TYPE.SNATCH && p_job.poiTarget == targetItem)
		{
			SetIsSuccessful(state: true);
		}
	}

	private void OnUnseizePOI(IPointOfInterest poi)
	{
		if (poi != targetItem || base.assignedParty == null)
		{
			return;
		}
		if (targetItem.gridTileLocation != null || targetItem.isBeingCarriedBy != null)
		{
			CreateStealJob(targetItem, base.assignedParty, dropStructure);
		}
		for (int i = 0; i < base.assignedParty.membersThatJoinedQuest.Count; i++)
		{
			Character character = base.assignedParty.membersThatJoinedQuest[i];
			if (character.currentJob != null && character.currentJob.isThisAPartyJob && (character.currentJob.jobType == JOB_TYPE.PARTY_GO_TO || character.currentJob.jobType == JOB_TYPE.GO_TO))
			{
				character.currentJob.CancelJob();
			}
		}
	}

	private void OnStealJobRemoved(JobQueueItem job, Character character)
	{
		if (job.jobType != JOB_TYPE.DEMON_STEAL || !(job is GoapPlanJob goapPlanJob) || goapPlanJob.poiTarget != targetItem || base.assignedParty == null)
		{
			return;
		}
		for (int i = 0; i < base.assignedParty.membersThatJoinedQuest.Count; i++)
		{
			Character character2 = base.assignedParty.membersThatJoinedQuest[i];
			if (character2.currentJob != null && character2.currentJob.isThisAPartyJob && (character2.currentJob.jobType == JOB_TYPE.GO_TO || character2.currentJob.jobType == JOB_TYPE.PARTY_GO_TO))
			{
				character2.currentJob.CancelJob();
			}
		}
	}

	public void CreateStealJob(TileObject p_target, Party p_party, LocationStructure p_dropStructure)
	{
		p_party.jobComponent.CreateStealJob(p_target, p_dropStructure);
	}

	public override void LoadReferences(SaveDataPartyQuest data)
	{
		base.LoadReferences(data);
		if (data is SaveDataDemonStealPartyQuest saveDataDemonStealPartyQuest)
		{
			if (!string.IsNullOrEmpty(saveDataDemonStealPartyQuest.targetItem))
			{
				targetItem = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(saveDataDemonStealPartyQuest.targetItem);
			}
			if (!string.IsNullOrEmpty(saveDataDemonStealPartyQuest.dropStructure))
			{
				dropStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentIDSafe(saveDataDemonStealPartyQuest.dropStructure);
			}
		}
	}

	public override void LoadReferencesInMainThread(SaveDataPartyQuest data)
	{
		base.LoadReferencesInMainThread(data);
		Messenger.AddListener<Character, GoapPlanJob>(CharacterSignals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, CheckIfStealJobIsFinished);
		Messenger.AddListener<IPointOfInterest>(CharacterSignals.ON_UNSEIZE_POI, OnUnseizePOI);
		Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnStealJobRemoved);
	}

	public override void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		base.CheckIfStructureIsStillReferenced(p_structure);
		_ = dropStructure;
	}
}

using Inner_Maps.Location_Structures;

public class FactionJobTriggerComponent : JobTriggerComponent
{
	private readonly Faction _owner;

	public FactionJobTriggerComponent(Faction owner)
	{
		_owner = owner;
	}

	public void TriggerJoinGatheringJob(Gathering gathering)
	{
		GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.JOIN_GATHERING, INTERACTION_TYPE.JOIN_GATHERING, gathering.host, _owner);
		goapPlanJob.SetCanTakeThisJobChecker("CanTakeJoinGathering");
		_owner.AddToAvailableJobs(goapPlanJob);
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}

	public void OnDisbandFaction()
	{
	}
}

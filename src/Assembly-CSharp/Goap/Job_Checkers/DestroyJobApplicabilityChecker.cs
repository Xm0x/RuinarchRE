namespace Goap.Job_Checkers;

public class DestroyJobApplicabilityChecker : JobApplicabilityChecker
{
	public override string key => "IsDestroyApplicable";

	public override bool IsJobStillApplicable(JobQueueItem job)
	{
		return (job as GoapPlanJob).targetPOI.gridTileLocation != null;
	}
}

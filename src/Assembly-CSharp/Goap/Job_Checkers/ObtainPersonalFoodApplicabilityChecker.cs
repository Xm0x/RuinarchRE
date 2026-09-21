namespace Goap.Job_Checkers;

public class ObtainPersonalFoodApplicabilityChecker : JobApplicabilityChecker
{
	public override string key => "IsObtainPersonalFoodApplicable";

	public override bool IsJobStillApplicable(JobQueueItem job)
	{
		return (job as GoapPlanJob).targetPOI.gridTileLocation != null;
	}
}

namespace Goap.Job_Checkers;

public class RemoveTrapApplicabilityChecker : JobApplicabilityChecker
{
	public override string key => "IsRemoveTrapApplicable";

	public override bool IsJobStillApplicable(JobQueueItem job)
	{
		GoapPlanJob goapPlanJob = job as GoapPlanJob;
		IPointOfInterest targetPOI = goapPlanJob.targetPOI;
		if (targetPOI == null)
		{
			return false;
		}
		string conditionKey = goapPlanJob.goal.conditionKey;
		if (targetPOI.gridTileLocation == null || targetPOI.isDead)
		{
			return false;
		}
		if (!targetPOI.traitContainer.HasTrait(conditionKey))
		{
			return false;
		}
		return true;
	}
}

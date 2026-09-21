namespace Goap.Job_Checkers;

public class RepairApplicabilityChecker : JobApplicabilityChecker
{
	public override string key => "IsRepairApplicable";

	public override bool IsJobStillApplicable(JobQueueItem job)
	{
		GoapPlanJob goapPlanJob = job as GoapPlanJob;
		NPCSettlement settlement = job.originalOwner as NPCSettlement;
		if (goapPlanJob.targetPOI.currentHP < goapPlanJob.targetPOI.maxHP && goapPlanJob.targetPOI.gridTileLocation != null)
		{
			return goapPlanJob.targetPOI.gridTileLocation.IsPartOfSettlement(settlement);
		}
		return false;
	}
}

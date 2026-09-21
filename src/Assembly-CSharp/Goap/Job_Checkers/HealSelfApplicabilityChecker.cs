namespace Goap.Job_Checkers;

public class HealSelfApplicabilityChecker : JobApplicabilityChecker
{
	public override string key => "IsHealSelfApplicable";

	public override bool IsJobStillApplicable(JobQueueItem job)
	{
		return !((job as GoapPlanJob).targetPOI as Character).IsHealthFull();
	}
}

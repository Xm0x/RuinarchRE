namespace Goap.Job_Checkers;

public class FeedApplicabilityChecker : JobApplicabilityChecker
{
	public override string key => "IsFeedStillApplicable";

	public override bool IsJobStillApplicable(JobQueueItem job)
	{
		if (job.poiTarget is Character character)
		{
			if (character.limiterComponent.canMove && character.limiterComponent.canPerform)
			{
				return false;
			}
			return true;
		}
		return false;
	}
}

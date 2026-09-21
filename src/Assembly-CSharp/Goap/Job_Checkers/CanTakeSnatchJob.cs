namespace Goap.Job_Checkers;

public class CanTakeSnatchJob : CanTakeJobChecker
{
	public override string key => "CanTakeSnatchJob";

	public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem)
	{
		if (!jobQueueItem.poiTarget.isBeingSeized && jobQueueItem.poiTarget.isBeingCarriedBy == null)
		{
			return jobQueueItem.poiTarget.numOfNonSecretActionsBeingPerformedOnThis <= 0;
		}
		return false;
	}
}

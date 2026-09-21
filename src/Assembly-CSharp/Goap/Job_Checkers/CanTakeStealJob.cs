namespace Goap.Job_Checkers;

public class CanTakeStealJob : CanTakeJobChecker
{
	public override string key => "CanTakeStealJob";

	public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem)
	{
		if (!jobQueueItem.poiTarget.isBeingSeized && jobQueueItem.poiTarget.isBeingCarriedBy == null)
		{
			return character.movementComponent.HasPathToEvenIfDiffRegion(jobQueueItem.poiTarget.gridTileLocation);
		}
		return false;
	}
}

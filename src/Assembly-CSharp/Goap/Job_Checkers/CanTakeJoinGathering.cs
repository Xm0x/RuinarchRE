namespace Goap.Job_Checkers;

public class CanTakeJoinGathering : CanTakeJobChecker
{
	public override string key => "CanTakeJoinGathering";

	public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem)
	{
		Gathering currentGathering = ((jobQueueItem as GoapPlanJob).targetPOI as Character).gatheringComponent.currentGathering;
		if (!character.gatheringComponent.hasGathering && currentGathering != null && !currentGathering.isWaitTimeOver && !currentGathering.isDisbanded)
		{
			return currentGathering.IsAllowedToJoin(character);
		}
		return false;
	}
}

namespace Goap.Job_Checkers;

public class CanTakeHaulJob : CanTakeJobChecker
{
	public override string key => "CanTakeHaul";

	public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem)
	{
		if (jobQueueItem is GoapPlanJob goapPlanJob && goapPlanJob.targetPOI.gridTileLocation != null)
		{
			return !character.movementComponent.ShouldAvoidStructureLocationOfTarget(goapPlanJob.targetPOI);
		}
		return false;
	}
}

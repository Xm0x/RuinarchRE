namespace Goap.Job_Checkers;

public class CanTakeObtainPersonalFood : CanTakeJobChecker
{
	public override string key => "CanTakeObtainPersonalFood";

	public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem)
	{
		GoapPlanJob goapPlanJob = jobQueueItem as GoapPlanJob;
		if (goapPlanJob.targetPOI != null && goapPlanJob.targetPOI.gridTileLocation != null)
		{
			return goapPlanJob.targetPOI.gridTileLocation.structure.IsResident(character);
		}
		if (goapPlanJob.targetPOI != null && goapPlanJob.targetPOI.gridTileLocation != null && goapPlanJob.targetPOI is TileObject tileObject && tileObject.IsOwnedBy(character))
		{
			return true;
		}
		return false;
	}
}

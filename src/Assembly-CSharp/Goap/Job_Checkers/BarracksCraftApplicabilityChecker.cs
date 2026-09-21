using Inner_Maps.Location_Structures;

namespace Goap.Job_Checkers;

public class BarracksCraftApplicabilityChecker : JobApplicabilityChecker
{
	public override string key => "IsBarracksCraftStillApplicable";

	public override bool IsJobStillApplicable(JobQueueItem job)
	{
		TileObject tileObject = (job as GoapPlanJob).targetPOI as TileObject;
		if (tileObject.gridTileLocation == null)
		{
			return false;
		}
		if (!(tileObject.gridTileLocation.structure is Barracks barracks))
		{
			return false;
		}
		if (barracks.HasEnoughOfObject(tileObject))
		{
			return tileObject.mapObjectState != MAP_OBJECT_STATE.UNBUILT;
		}
		return true;
	}
}

using Inner_Maps.Location_Structures;

namespace Goap.Job_Checkers;

public class MagicAcademyCraftApplicabilityChecker : JobApplicabilityChecker
{
	public override string key => "IsMagicAcademyCraftStillApplicable";

	public override bool IsJobStillApplicable(JobQueueItem job)
	{
		TileObject tileObject = (job as GoapPlanJob).targetPOI as TileObject;
		if (tileObject.gridTileLocation == null)
		{
			return false;
		}
		if (!(tileObject.gridTileLocation.structure is MagicAcademy magicAcademy))
		{
			return false;
		}
		if (magicAcademy.HasEnoughOfObject(tileObject))
		{
			return tileObject.mapObjectState != MAP_OBJECT_STATE.UNBUILT;
		}
		return true;
	}
}

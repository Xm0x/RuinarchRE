namespace Goap.Job_Checkers;

public class CanTakeBuildJob : CanTakeJobChecker
{
	public override string key => "CanTakeBuildJob";

	public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem)
	{
		GenericTileObject genericTileObject = jobQueueItem.poiTarget as GenericTileObject;
		if (genericTileObject.gridTileLocation.tileObjectComponent.IsAffectedByAOESpell(TILE_OBJECT_TYPE.BLIZZARD_TILE_OBJECT) || genericTileObject.gridTileLocation.tileObjectComponent.IsAffectedByAOESpell(TILE_OBJECT_TYPE.HEAT_WAVE_TILE_OBJECT))
		{
			return false;
		}
		return true;
	}
}

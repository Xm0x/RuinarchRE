namespace Goap.Job_Checkers;

public class CanTakeRepairJob : CanTakeJobChecker
{
	public override string key => "CanTakeRepair";

	public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem)
	{
		if (jobQueueItem.poiTarget.gridTileLocation != null && (jobQueueItem.poiTarget.gridTileLocation.tileObjectComponent.IsAffectedByAOESpell(TILE_OBJECT_TYPE.BLIZZARD_TILE_OBJECT) || jobQueueItem.poiTarget.gridTileLocation.tileObjectComponent.IsAffectedByAOESpell(TILE_OBJECT_TYPE.HEAT_WAVE_TILE_OBJECT)))
		{
			return false;
		}
		bool result = false;
		if (jobQueueItem.poiTarget is TileObject tileObject)
		{
			result = tileObject.canBeRepaired;
		}
		return result;
	}
}

namespace Goap.Job_Checkers;

public class CanTakeRemoveFire : CanTakeJobChecker
{
	public override string key => "CanTakeRemoveFire";

	public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem)
	{
		GoapPlanJob goapPlanJob = jobQueueItem as GoapPlanJob;
		if (character.jobQueue.HasJob(JOB_TYPE.DOUSE_FIRE) || character.jobQueue.HasJob(JOB_TYPE.DOUSE_FIRE_SELF))
		{
			return false;
		}
		if (character.traitContainer.HasTrait("Pyromaniac"))
		{
			return false;
		}
		if (goapPlanJob.targetPOI is Character character2)
		{
			if (character == character2)
			{
				return HasWaterAvailable(character);
			}
			if (!character.traitContainer.HasTrait("Burning", "Pyrophobic") && !character.relationshipContainer.IsEnemiesWith(character2))
			{
				return HasWaterAvailable(character);
			}
			return false;
		}
		bool flag = false;
		if (jobQueueItem.originalOwner is NPCSettlement nPCSettlement)
		{
			for (int i = 0; i < nPCSettlement.firesToDouseInSettlement.Count; i++)
			{
				if (!nPCSettlement.firesToDouseInSettlement[i].traitContainer.IsResponsibleForTrait("Burning", character))
				{
					flag = true;
					break;
				}
			}
		}
		if (flag && !character.traitContainer.HasTrait("Burning", "Pyrophobic"))
		{
			return HasWaterAvailable(character);
		}
		return false;
	}

	private bool HasWaterAvailable(Character character)
	{
		if (!character.currentRegion.HasTileObjectOfType(TILE_OBJECT_TYPE.WATER_WELL))
		{
			return character.currentRegion.HasTileObjectOfType(TILE_OBJECT_TYPE.FISHING_SPOT);
		}
		return true;
	}
}

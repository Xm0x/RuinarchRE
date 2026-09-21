namespace Goap.Job_Checkers;

public class CanTakePlaceBlueprintJob : CanTakeJobChecker
{
	public override string key => "CanTakePlaceBlueprintJob";

	public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem)
	{
		if (jobQueueItem.originalOwner is NPCSettlement { ruler: var ruler } nPCSettlement)
		{
			if (character.isSettlementRuler && ruler == character)
			{
				return true;
			}
			if (ruler == null || ruler.currentSettlement != nPCSettlement)
			{
				if (!character.race.IsSapient() && character.raceSetting.category != CHARACTER_CATEGORY.Humanoid)
				{
					return character.raceSetting.category == CHARACTER_CATEGORY.Demonic;
				}
				return true;
			}
			return false;
		}
		return true;
	}
}

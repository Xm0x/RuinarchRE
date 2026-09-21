namespace Goap.Job_Checkers;

public class CanBrewPotion : CanTakeJobChecker
{
	public override string key => "CanBrewPotion";

	public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem)
	{
		return TILE_OBJECT_TYPE.HEALING_POTION.CanBeCraftedBy(character);
	}
}

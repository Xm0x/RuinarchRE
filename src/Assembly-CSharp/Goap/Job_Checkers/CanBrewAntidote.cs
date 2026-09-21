namespace Goap.Job_Checkers;

public class CanBrewAntidote : CanTakeJobChecker
{
	public override string key => "CanBrewAntidote";

	public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem)
	{
		return TILE_OBJECT_TYPE.ANTIDOTE.CanBeCraftedBy(character);
	}
}

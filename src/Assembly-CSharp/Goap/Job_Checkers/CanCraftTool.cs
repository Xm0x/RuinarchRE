namespace Goap.Job_Checkers;

public class CanCraftTool : CanTakeJobChecker
{
	public override string key => "CanCraftTool";

	public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem)
	{
		return TILE_OBJECT_TYPE.TOOL.CanBeCraftedBy(character);
	}
}

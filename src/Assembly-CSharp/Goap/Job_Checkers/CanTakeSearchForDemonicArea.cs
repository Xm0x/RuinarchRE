namespace Goap.Job_Checkers;

public class CanTakeSearchForDemonicArea : CanTakeJobChecker
{
	public override string key => "CanTakeSearchForDemonicArea";

	public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem)
	{
		return !character.isNotHostileWithPlayer;
	}
}

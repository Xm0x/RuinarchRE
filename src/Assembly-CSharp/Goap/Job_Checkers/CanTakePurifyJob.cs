namespace Goap.Job_Checkers;

public class CanTakePurifyJob : CanTakeJobChecker
{
	public override string key => "CanTakePurifyJob";

	public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem)
	{
		if (character.isNotHostileWithPlayer)
		{
			return false;
		}
		return true;
	}
}

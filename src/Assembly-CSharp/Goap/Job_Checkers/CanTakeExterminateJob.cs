namespace Goap.Job_Checkers;

public class CanTakeExterminateJob : CanTakeJobChecker
{
	public override string key => "CanTakeExterminate";

	public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem)
	{
		_ = character.partyComponent.currentParty;
		return !character.partyComponent.hasParty;
	}
}

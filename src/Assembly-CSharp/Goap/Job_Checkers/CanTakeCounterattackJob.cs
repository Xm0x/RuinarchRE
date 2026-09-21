namespace Goap.Job_Checkers;

public class CanTakeCounterattackJob : CanTakeJobChecker
{
	public override string key => "CanTakeCounterattack";

	public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem)
	{
		_ = character.partyComponent.currentParty;
		return !character.partyComponent.hasParty;
	}
}

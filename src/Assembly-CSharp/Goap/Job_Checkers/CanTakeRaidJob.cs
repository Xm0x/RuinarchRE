namespace Goap.Job_Checkers;

public class CanTakeRaidJob : CanTakeJobChecker
{
	public override string key => "CanTakeRaid";

	public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem)
	{
		return !character.partyComponent.hasParty;
	}
}

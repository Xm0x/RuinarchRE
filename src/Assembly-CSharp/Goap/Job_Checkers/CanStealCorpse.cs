namespace Goap.Job_Checkers;

public class CanStealCorpse : CanTakeJobChecker
{
	public override string key => "CanStealCorpse";

	public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem)
	{
		return character.traitContainer.HasTrait("Demon Cultist");
	}
}

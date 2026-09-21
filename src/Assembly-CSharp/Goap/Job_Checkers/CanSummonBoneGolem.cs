namespace Goap.Job_Checkers;

public class CanSummonBoneGolem : CanTakeJobChecker
{
	public override string key => "CanSummonBoneGolem";

	public override bool CanTakeJob(Character character, JobQueueItem jobQueueItem)
	{
		return character.traitContainer.HasTrait("Demon Cultist");
	}
}

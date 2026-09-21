namespace Goap.Job_Checkers;

public class JudgeApplicabilityChecker : JobApplicabilityChecker
{
	public override string key => "IsJudgeApplicable";

	public override bool IsJobStillApplicable(JobQueueItem job)
	{
		Character character = (job as GoapPlanJob).targetPOI as Character;
		_ = job.originalOwner;
		if (character.isDead)
		{
			return false;
		}
		if (character.currentSettlement is NPCSettlement nPCSettlement && character.currentStructure != nPCSettlement.prison)
		{
			return false;
		}
		if (!character.traitContainer.HasTrait("Restrained"))
		{
			return false;
		}
		return true;
	}
}

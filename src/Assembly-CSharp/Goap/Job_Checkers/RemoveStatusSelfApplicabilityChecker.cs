namespace Goap.Job_Checkers;

public class RemoveStatusSelfApplicabilityChecker : JobApplicabilityChecker
{
	public override string key => "IsRemoveStatusSelfApplicable";

	public override bool IsJobStillApplicable(JobQueueItem job)
	{
		GoapPlanJob obj = job as GoapPlanJob;
		Character character = obj.targetPOI as Character;
		string conditionKey = obj.goal.conditionKey;
		if (character.gridTileLocation == null || character.isDead)
		{
			return false;
		}
		if (!character.traitContainer.HasTrait(conditionKey))
		{
			return false;
		}
		return true;
	}
}

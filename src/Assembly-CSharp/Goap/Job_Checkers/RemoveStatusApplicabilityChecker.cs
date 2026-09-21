using Traits;

namespace Goap.Job_Checkers;

public class RemoveStatusApplicabilityChecker : JobApplicabilityChecker
{
	public override string key => "IsRemoveStatusApplicable";

	public override bool IsJobStillApplicable(JobQueueItem job)
	{
		GoapPlanJob goapPlanJob = job as GoapPlanJob;
		IPointOfInterest targetPOI = goapPlanJob.targetPOI;
		if (goapPlanJob.goal == null)
		{
			return false;
		}
		string conditionKey = goapPlanJob.goal.conditionKey;
		if (targetPOI.gridTileLocation == null || targetPOI.isDead)
		{
			return false;
		}
		if (!targetPOI.gridTileLocation.IsNextToSettlementAreaOrPartOfSettlement(job.originalOwner as NPCSettlement))
		{
			return false;
		}
		if (targetPOI.traitContainer.HasTrait("Criminal"))
		{
			return false;
		}
		if (!targetPOI.traitContainer.HasTrait(conditionKey))
		{
			return false;
		}
		if (targetPOI.traitContainer.GetTraitOrStatus<Trait>(conditionKey).isGainedFromDoingStealth)
		{
			return false;
		}
		return true;
	}
}

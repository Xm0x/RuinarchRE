using Locations.Settlements;

namespace Goap.Job_Checkers;

public class HaulApplicabilityChecker : JobApplicabilityChecker
{
	public override string key => "IsHaulApplicable";

	public override bool IsJobStillApplicable(JobQueueItem job)
	{
		GoapPlanJob goapPlanJob = job as GoapPlanJob;
		NPCSettlement nPCSettlement = job.originalOwner as NPCSettlement;
		IPointOfInterest targetPOI = goapPlanJob.targetPOI;
		BaseSettlement settlement = null;
		if (targetPOI != null && targetPOI.gridTileLocation != null && (!targetPOI.gridTileLocation.IsPartOfSettlement(out settlement) || settlement == nPCSettlement || settlement.owner == null || (!settlement.owner.isMajorNonPlayer && settlement.owner.factionType.type != FACTION_TYPE.Ratmen)))
		{
			if (goapPlanJob.targetPOI.isBeingCarriedBy == null)
			{
				if (goapPlanJob.targetPOI.gridTileLocation != null)
				{
					return goapPlanJob.targetPOI.gridTileLocation.structure != nPCSettlement.mainStorage;
				}
				return false;
			}
			return true;
		}
		return false;
	}
}

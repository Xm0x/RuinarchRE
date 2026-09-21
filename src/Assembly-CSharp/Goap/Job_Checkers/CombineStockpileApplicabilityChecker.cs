namespace Goap.Job_Checkers;

public class CombineStockpileApplicabilityChecker : JobApplicabilityChecker
{
	public override string key => "IsCombineStockpileApplicable";

	public override bool IsJobStillApplicable(JobQueueItem job)
	{
		GoapPlanJob obj = job as GoapPlanJob;
		ResourcePile resourcePile = obj.targetPOI as ResourcePile;
		ResourcePile resourcePile2 = (obj.otherData[INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE][0] as TileObjectOtherData).tileObject as ResourcePile;
		NPCSettlement nPCSettlement = job.originalOwner as NPCSettlement;
		if (resourcePile2.gridTileLocation != null && resourcePile2.gridTileLocation.IsPartOfSettlement(nPCSettlement) && resourcePile2.structureLocation == nPCSettlement.mainStorage && resourcePile.gridTileLocation != null && resourcePile.gridTileLocation.IsPartOfSettlement(nPCSettlement) && resourcePile.structureLocation == nPCSettlement.mainStorage)
		{
			return resourcePile2.resourceStorageComponent.HasEnoughSpaceFor(resourcePile.providedResource, resourcePile.resourceInPile);
		}
		return false;
	}
}

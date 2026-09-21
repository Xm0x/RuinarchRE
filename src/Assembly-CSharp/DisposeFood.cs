public class DisposeFood : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;

	public DisposeFood()
		: base(INTERACTION_TYPE.DISPOSE_FOOD)
	{
		base.actionIconString = GoapActionStateDB.Haul_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
		base.canBeAdvertisedEvenIfTargetIsUnavailable = true;
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		string stateName = "Target Missing";
		bool isInvalid = IsTargetMissingOverride(node);
		GoapActionInvalidity invalidity = node.invalidity;
		invalidity.isInvalid = isInvalid;
		invalidity.stateName = stateName;
		invalidity.reason = "target_unavailable";
		return invalidity;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Dispose Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public void AfterDisposeSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.poiTarget is FoodPile foodPile)
		{
			foodPile.AdjustHP(-foodPile.maxHP, ELEMENTAL_TYPE.Normal);
		}
	}

	private bool IsTargetMissingOverride(ActualGoapNode node)
	{
		Character actor = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		if (!poiTarget.IsAvailable() || poiTarget.gridTileLocation == null || actor.currentRegion != poiTarget.currentRegion)
		{
			return true;
		}
		if (base.actionLocationType == ACTION_LOCATION_TYPE.NEAR_TARGET)
		{
			if (actor.gridTileLocation != poiTarget.gridTileLocation && !actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation, sameStructureOnly: true))
			{
				if (actor.hasMarker && actor.marker.IsCharacterInLineOfSightWith(poiTarget))
				{
					return false;
				}
				return true;
			}
		}
		else if (base.actionLocationType == ACTION_LOCATION_TYPE.NEAR_OTHER_TARGET && actor.gridTileLocation != node.targetTile && !actor.gridTileLocation.IsNeighbour(node.targetTile, sameStructureOnly: true))
		{
			return true;
		}
		return false;
	}
}

using Inner_Maps;

public class CreateWorkplacePotion : GoapAction
{
	public CreateWorkplacePotion()
		: base(INTERACTION_TYPE.CREATE_WORKPLACE_POTION)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		base.actionIconString = GoapActionStateDB.Work_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Create Workplace Potion Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public void AfterCreateWorkplacePotionSuccess(ActualGoapNode p_node)
	{
		TileObject poi = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.HEALING_POTION);
		p_node.actor.moneyComponent.AdjustCoins(28);
		LocationGridTile gridTileLocation = p_node.target.gridTileLocation;
		gridTileLocation.structure.RemovePOI(p_node.target);
		gridTileLocation.structure.AddPOI(poi, gridTileLocation);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		return base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
	}
}

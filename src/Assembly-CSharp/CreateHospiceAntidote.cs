using Inner_Maps;

public class CreateHospiceAntidote : GoapAction
{
	public CreateHospiceAntidote()
		: base(INTERACTION_TYPE.CREATE_HOSPICE_ANTIDOTE)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		base.actionIconString = GoapActionStateDB.Work_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Create Hospice Antidote Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public void AfterCreateHospiceAntidoteSuccess(ActualGoapNode p_node)
	{
		p_node.actor.moneyComponent.AdjustCoins(28);
		TileObject tileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.ANTIDOTE);
		LocationGridTile locationGridTile = p_node.actor.gridTileLocation;
		if (locationGridTile != null && locationGridTile.tileObjectComponent.objHere != null)
		{
			locationGridTile = p_node.actor.gridTileLocation.GetFirstNearestTileFromThisWithNoObject();
		}
		if (locationGridTile != null)
		{
			locationGridTile.structure.AddPOI(tileObject, locationGridTile);
			p_node.target.gridTileLocation?.structure.RemovePOI(p_node.target);
			if (p_node.actor.structureComponent.workPlaceStructure != null && locationGridTile.structure != p_node.actor.structureComponent.workPlaceStructure)
			{
				p_node.actor.jobComponent.CreateDropItemJob(JOB_TYPE.CREATE_HOSPICE_ANTIDOTE, tileObject, p_node.actor.structureComponent.workPlaceStructure);
			}
		}
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		return base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
	}
}

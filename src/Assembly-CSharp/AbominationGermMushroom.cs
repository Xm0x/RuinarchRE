using Inner_Maps;

public class AbominationGermMushroom : GoapAction
{
	public AbominationGermMushroom()
		: base(INTERACTION_TYPE.ABOMINATION_GERM_MUSHROOM)
	{
		base.actionIconString = GoapActionStateDB.Poison_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Produce Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		if (!goapActionInvalidity.isInvalid && node.poiTarget is GenericTileObject { gridTileLocation: var gridTileLocation } && (gridTileLocation.isOccupied || gridTileLocation.tileObjectComponent.objHere != null))
		{
			goapActionInvalidity.isInvalid = true;
			goapActionInvalidity.reason = "tile_occupied";
		}
		return goapActionInvalidity;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			return actor.gridTileLocation != null;
		}
		return false;
	}

	public void AfterProduceSuccess(ActualGoapNode goapNode)
	{
		LocationGridTile gridTileLocation = goapNode.poiTarget.gridTileLocation;
		if (gridTileLocation != null && !gridTileLocation.isOccupied && gridTileLocation.tileObjectComponent.objHere == null)
		{
			Mushroom mushroom = InnerMapManager.Instance.CreateNewTileObject<Mushroom>(TILE_OBJECT_TYPE.MUSHROOM);
			if (!mushroom.traitContainer.HasTrait("Abomination Germ"))
			{
				mushroom.traitContainer.AddTrait(mushroom, "Abomination Germ");
			}
			gridTileLocation.structure.AddPOI(mushroom, goapNode.actor.gridTileLocation);
		}
	}
}

public class PlaceSnareTrap : GoapAction
{
	public PlaceSnareTrap()
		: base(INTERACTION_TYPE.PLACE_SNARE_TRAP)
	{
		base.actionIconString = GoapActionStateDB.Work_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Place Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public void AfterPlaceSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.poiTarget is GenericTileObject genericTileObject)
		{
			genericTileObject.gridTileLocation.tileObjectComponent.SetHasSnareTrap(state: true, isPlayerSource: false, RACE.CENTAUR);
		}
	}
}

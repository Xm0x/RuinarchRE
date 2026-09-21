using Inner_Maps;

public class DrawMagicCircle : GoapAction
{
	public DrawMagicCircle()
		: base(INTERACTION_TYPE.DRAW_MAGIC_CIRCLE)
	{
		base.actionIconString = GoapActionStateDB.Magic_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
		base.canBeAdvertisedEvenIfTargetIsUnavailable = true;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Draw Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public void AfterDrawSuccess(ActualGoapNode goapNode)
	{
		LocationGridTile gridTileLocation = goapNode.poiTarget.gridTileLocation;
		MagicCircle poi = InnerMapManager.Instance.CreateNewTileObject<MagicCircle>(TILE_OBJECT_TYPE.MAGIC_CIRCLE);
		gridTileLocation.structure.AddPOI(poi, gridTileLocation);
	}
}

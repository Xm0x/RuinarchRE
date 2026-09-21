using Inner_Maps;

public class PurifyGround : GoapAction
{
	public PurifyGround()
		: base(INTERACTION_TYPE.PURIFY_GROUND)
	{
		base.actionIconString = GoapActionStateDB.Divine_Icon;
		base.shouldAddLogs = false;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Purify Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public void AfterPurifySuccess(ActualGoapNode goapNode)
	{
		LocationGridTile gridTileLocation = goapNode.target.gridTileLocation;
		if (gridTileLocation.corruptionComponent.isCorrupted)
		{
			gridTileLocation.corruptionComponent.BaseBuildingTileDemolition(p_includeStructures: true, p_includeDemonicWalls: true, p_includeDecorations: true, p_includeCorruptedTiles: true);
			goapNode.actor.behaviourComponent.AddNumberOfPurifiedGrounds();
		}
	}
}

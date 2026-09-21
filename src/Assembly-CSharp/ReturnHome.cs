using Inner_Maps;
using Inner_Maps.Location_Structures;

public class ReturnHome : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;

	public ReturnHome()
		: base(INTERACTION_TYPE.RETURN_HOME)
	{
		base.shouldAddLogs = false;
		base.actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION;
		base.actionIconString = GoapActionStateDB.No_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Return Home Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 3;
	}

	public override LocationStructure GetTargetStructure(ActualGoapNode node)
	{
		Character actor = node.actor;
		if (actor.homeStructure != null)
		{
			return actor.homeStructure;
		}
		if (actor.homeRegion != null)
		{
			return actor.homeRegion.wilderness;
		}
		return actor.currentRegion.wilderness;
	}

	public override LocationGridTile GetTargetTileToGoTo(ActualGoapNode goapNode)
	{
		return null;
	}
}

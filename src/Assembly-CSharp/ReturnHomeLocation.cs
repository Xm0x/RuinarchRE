using Inner_Maps;
using Inner_Maps.Location_Structures;

public class ReturnHomeLocation : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;

	public ReturnHomeLocation()
		: base(INTERACTION_TYPE.RETURN_HOME_LOCATION)
	{
		base.goapName = "Return Home Location";
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
		return node.actor.homeRegion.wilderness;
	}

	public override LocationGridTile GetTargetTileToGoTo(ActualGoapNode goapNode)
	{
		return null;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			return actor == poiTarget;
		}
		return false;
	}
}

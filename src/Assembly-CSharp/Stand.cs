using System.Collections.Generic;
using Inner_Maps;

public class Stand : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;

	public Stand()
		: base(INTERACTION_TYPE.STAND)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.NEARBY;
		base.actionIconString = GoapActionStateDB.No_Icon;
		base.shouldAddLogs = false;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Needs };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Stand Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 4;
	}

	public override void PopulateNearbyLocation(List<LocationGridTile> gridTiles, ActualGoapNode goapNode)
	{
		if (goapNode.actor is Summon && goapNode.actor.homeStructure != null && goapNode.actor.homeStructure == goapNode.actor.currentStructure)
		{
			gridTiles.AddRange(goapNode.actor.homeStructure.passableTiles);
		}
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

public class GoToTile : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.INDIRECT;

	public GoToTile()
		: base(INTERACTION_TYPE.GO_TO_TILE)
	{
		base.actionIconString = GoapActionStateDB.No_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.UPON_STRUCTURE_ARRIVAL;
		base.shouldAddLogs = false;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
		base.canBeAdvertisedEvenIfTargetIsUnavailable = true;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Go Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override void OnMoveToDoAction(ActualGoapNode node)
	{
		base.OnMoveToDoAction(node);
		if (node.associatedJobType == JOB_TYPE.GO_TO_WAITING && node.actor.partyComponent.hasParty)
		{
			node.actor.partyComponent.currentParty.AddMemberThatJoinedQuest(node.actor);
		}
	}

	public void AfterGoSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.associatedJobType == JOB_TYPE.GO_TO_WAITING && goapNode.actor.partyComponent.hasParty)
		{
			goapNode.actor.partyComponent.currentParty.AddMemberThatJoinedQuest(goapNode.actor);
		}
	}
}

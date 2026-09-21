public class CleanseTile : GoapAction
{
	public CleanseTile()
		: base(INTERACTION_TYPE.CLEANSE_TILE)
	{
		base.actionIconString = GoapActionStateDB.Clean_Icon;
		base.shouldAddLogs = false;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState(goapNode.actor.HasItem(TILE_OBJECT_TYPE.ICE_CRYSTAL) ? "Ice Cleanse Success" : "Cleanse Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public void AfterIceCleanseSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.UnobtainItem(TILE_OBJECT_TYPE.ICE_CRYSTAL);
		goapNode.target.traitContainer.RemoveStatusAndStacks(goapNode.target, "Poisoned", goapNode.actor);
	}

	public void AfterCleanseSuccess(ActualGoapNode goapNode)
	{
		goapNode.target.traitContainer.RemoveStatusAndStacks(goapNode.target, "Poisoned", goapNode.actor);
	}
}

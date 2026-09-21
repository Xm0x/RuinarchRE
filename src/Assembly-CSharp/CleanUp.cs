public class CleanUp : GoapAction
{
	public CleanUp()
		: base(INTERACTION_TYPE.CLEAN_UP)
	{
		base.actionIconString = GoapActionStateDB.Clean_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Clean Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public void AfterCleanSuccess(ActualGoapNode goapNode)
	{
		goapNode.target.traitContainer.RemoveStatusAndStacks(goapNode.target, "Wet", goapNode.actor);
		goapNode.target.traitContainer.RemoveStatusAndStacks(goapNode.target, "Dirty", goapNode.actor);
		goapNode.target.traitContainer.RemoveStatusAndStacks(goapNode.target, "Burnt", goapNode.actor);
		goapNode.target.traitContainer.RemoveStatusAndStacks(goapNode.target, "Poisoned", goapNode.actor);
	}
}

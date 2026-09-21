public class ScreamForHelp : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.VERBAL;

	public ScreamForHelp()
		: base(INTERACTION_TYPE.SCREAM_FOR_HELP)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
		base.actionIconString = GoapActionStateDB.Shock_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Social };
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Scream Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 1;
	}

	public void PerTickScreamSuccess(ActualGoapNode goapNode)
	{
		Messenger.Broadcast(JobSignals.SCREAM_FOR_HELP, goapNode.actor);
	}

	public void AfterScreamSuccess(ActualGoapNode goapNode)
	{
	}
}

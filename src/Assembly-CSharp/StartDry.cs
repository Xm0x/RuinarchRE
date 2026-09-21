using JetBrains.Annotations;

public class StartDry : GoapAction
{
	public StartDry()
		: base(INTERACTION_TYPE.START_DRY)
	{
		base.actionIconString = GoapActionStateDB.Clean_Icon;
		base.shouldAddLogs = false;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Start Dry Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	[UsedImplicitly]
	public void PreStartDrySuccess(ActualGoapNode goapNode)
	{
	}
}

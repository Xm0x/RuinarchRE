public class FeelingBrokenhearted : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.INDIRECT;

	public FeelingBrokenhearted()
		: base(INTERACTION_TYPE.FEELING_BROKENHEARTED)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
		base.actionIconString = GoapActionStateDB.Heartbroken_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Social };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Brokenhearted Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}
}

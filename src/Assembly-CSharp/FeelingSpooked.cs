public class FeelingSpooked : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.INDIRECT;

	public FeelingSpooked()
		: base(INTERACTION_TYPE.FEELING_SPOOKED)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
		base.actionIconString = GoapActionStateDB.Cowering_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Social };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Spooked Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}
}

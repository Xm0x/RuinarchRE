public class GoTo : GoapAction
{
	public GoTo()
		: base(INTERACTION_TYPE.GO_TO)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.TARGET_IN_VISION;
		base.actionIconString = GoapActionStateDB.No_Icon;
		base.doesNotStopTargetCharacter = true;
		base.canBeAdvertisedEvenIfTargetIsUnavailable = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Goto Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}
}

public class FeelingConcerned : GoapAction
{
	public FeelingConcerned()
		: base(INTERACTION_TYPE.FEELING_CONCERNED)
	{
		base.actionIconString = GoapActionStateDB.Heartbroken_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		base.doesNotStopTargetCharacter = true;
		base.canBeAdvertisedEvenIfTargetIsUnavailable = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Social };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Concerned Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 1;
	}

	public void PreConcernedSuccess()
	{
	}

	public void AfterConcernedSuccess()
	{
	}
}

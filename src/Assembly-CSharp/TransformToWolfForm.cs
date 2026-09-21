public class TransformToWolfForm : GoapAction
{
	public TransformToWolfForm()
		: base(INTERACTION_TYPE.TRANSFORM_TO_WOLF_FORM)
	{
		base.actionIconString = GoapActionStateDB.No_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
		base.logTags = new LOG_TAG[2]
		{
			LOG_TAG.Crimes,
			LOG_TAG.Life_Changes
		};
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Transform Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 5;
	}

	public void PreTransformSuccess(ActualGoapNode goapNode)
	{
	}

	public void AfterTransformSuccess(ActualGoapNode goapNode)
	{
	}
}

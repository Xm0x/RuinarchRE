public class Puke : GoapAction
{
	public Puke()
		: base(INTERACTION_TYPE.PUKE)
	{
		base.actionIconString = GoapActionStateDB.Sick_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Needs };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Puke Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 5;
	}

	public void PrePukeSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.SetPOIState(POI_STATE.INACTIVE);
	}

	public void AfterPukeSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.SetPOIState(POI_STATE.ACTIVE);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			return actor == poiTarget;
		}
		return false;
	}
}

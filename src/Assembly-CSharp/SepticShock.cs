public class SepticShock : GoapAction
{
	public SepticShock()
		: base(INTERACTION_TYPE.SEPTIC_SHOCK)
	{
		base.actionIconString = GoapActionStateDB.Death_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
		base.logTags = new LOG_TAG[1];
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Septic Shock Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 5;
	}

	public void PreSepticShockSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.SetPOIState(POI_STATE.INACTIVE);
	}

	public void AfterSepticShockSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.SetPOIState(POI_STATE.ACTIVE);
		goapNode.actor.Death("Septic Shock", goapNode, null, goapNode.descriptionLog);
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

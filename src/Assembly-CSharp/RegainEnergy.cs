public class RegainEnergy : GoapAction
{
	public RegainEnergy()
		: base(INTERACTION_TYPE.REGAIN_ENERGY)
	{
		base.actionIconString = GoapActionStateDB.Magic_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.NEARBY;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Needs };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Regain Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			return actor == poiTarget;
		}
		return false;
	}

	public void AfterRegainSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.actor.necromancerTrait != null)
		{
			goapNode.actor.necromancerTrait.AdjustEnergy(2);
		}
	}
}

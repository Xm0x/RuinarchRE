public class Sit : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;

	public Sit()
		: base(INTERACTION_TYPE.SIT)
	{
		base.actionIconString = GoapActionStateDB.No_Icon;
		base.shouldAddLogs = false;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Sit Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		IPointOfInterest poiTarget = node.poiTarget;
		if (!goapActionInvalidity.isInvalid && !poiTarget.IsAvailable())
		{
			goapActionInvalidity.isInvalid = true;
			goapActionInvalidity.stateName = "Sit Fail";
		}
		return goapActionInvalidity;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job) && poiTarget.gridTileLocation != null)
		{
			return poiTarget.IsAvailable();
		}
		return false;
	}
}

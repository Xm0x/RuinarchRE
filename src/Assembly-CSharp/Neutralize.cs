public class Neutralize : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.INDIRECT;

	public override bool shouldShowNotifRegardlessOfWatcher => true;

	public Neutralize()
		: base(INTERACTION_TYPE.NEUTRALIZE)
	{
		base.actionIconString = GoapActionStateDB.Hostile_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.TARGET_IN_VISION;
		base.logTags = new LOG_TAG[2]
		{
			LOG_TAG.Work,
			LOG_TAG.Major
		};
		base.showNotification = true;
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Neutralize Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (poiTarget is MovingTileObject { hasExpired: not false })
			{
				return false;
			}
			if (poiTarget.IsAvailable())
			{
				return poiTarget.gridTileLocation != null;
			}
			return false;
		}
		return false;
	}

	public void AfterNeutralizeSuccess(ActualGoapNode goapNode)
	{
		TileObject obj = goapNode.poiTarget as TileObject;
		if (obj is PoisonCloud poisonCloud)
		{
			poisonCloud.SetDoExpireEffect(state: false);
		}
		obj.Neutralize();
	}
}

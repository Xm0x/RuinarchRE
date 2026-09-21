public class ReadNecronomicon : GoapAction
{
	public ReadNecronomicon()
		: base(INTERACTION_TYPE.READ_NECRONOMICON)
	{
		base.actionIconString = GoapActionStateDB.Read_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.NEARBY;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
		base.showNotification = true;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Read Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			return actor.HasItem("Necronomicon");
		}
		return false;
	}

	public void PreReadSuccess(ActualGoapNode goapNode)
	{
		TileObject item = goapNode.actor.GetItem("Necronomicon");
		if (item != null)
		{
			goapNode.actor.ShowItemVisualCarryingPOI(item);
		}
	}

	public void AfterReadSuccess(ActualGoapNode goapNode)
	{
		TileObject item = goapNode.actor.GetItem("Necronomicon");
		if (item != null)
		{
			goapNode.actor.UncarryPOI(item, bringBackToInventory: true);
		}
	}
}

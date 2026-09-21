using Inner_Maps.Location_Structures;

public class CleanseHallowedGround : GoapAction
{
	public CleanseHallowedGround()
		: base(INTERACTION_TYPE.CLEANSE_HALLOWED_GROUND)
	{
		base.actionIconString = GoapActionStateDB.Clean_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Major };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Cleanse Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		log.AddInvolvedObjectManual(node.poiTarget.persistentID);
	}

	public void AfterCleanseSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.poiTarget is HallowedGround && goapNode.poiTarget.gridTileLocation?.structure is Inner_Maps.Location_Structures.HallowedGround hallowedGround)
		{
			hallowedGround.ClaimHallowedGround(RELIGION.None);
		}
	}
}

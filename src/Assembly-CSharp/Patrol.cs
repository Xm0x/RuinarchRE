using Inner_Maps;

public class Patrol : GoapAction
{
	public Patrol()
		: base(INTERACTION_TYPE.PATROL)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.OVERRIDE;
		base.actionIconString = GoapActionStateDB.Patrol_Icon;
		base.shouldAddLogs = false;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Patrol Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override LocationGridTile GetOverrideTargetTile(ActualGoapNode goapNode)
	{
		return goapNode.otherData[0].obj as LocationGridTile;
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		string stateName = "Target Missing";
		GoapActionInvalidity invalidity = node.invalidity;
		invalidity.isInvalid = false;
		invalidity.stateName = stateName;
		invalidity.reason = string.Empty;
		return invalidity;
	}
}

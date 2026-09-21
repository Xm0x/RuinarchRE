using Inner_Maps;

public class Roam : GoapAction
{
	public Roam()
		: base(INTERACTION_TYPE.ROAM)
	{
		base.actionIconString = GoapActionStateDB.No_Icon;
		base.doesNotStopTargetCharacter = true;
		base.shouldAddLogs = false;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Roam Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override LocationGridTile GetTargetTileToGoTo(ActualGoapNode goapNode)
	{
		OtherData[] otherData = goapNode.otherData;
		if (otherData != null && otherData.Length == 1 && otherData[0].obj is LocationGridTile)
		{
			return otherData[0].obj as LocationGridTile;
		}
		return base.GetTargetTileToGoTo(goapNode);
	}

	public override IPointOfInterest GetTargetToGoTo(ActualGoapNode goapNode)
	{
		return null;
	}
}

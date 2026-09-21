using Inner_Maps;
using Inner_Maps.Location_Structures;

public class AttackDemonicStructure : GoapAction
{
	public AttackDemonicStructure()
		: base(INTERACTION_TYPE.ATTACK_DEMONIC_STRUCTURE)
	{
		base.actionIconString = GoapActionStateDB.Hostile_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.UPON_STRUCTURE_ARRIVAL;
		base.doesNotStopTargetCharacter = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Combat };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Attack Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override LocationStructure GetTargetStructure(ActualGoapNode node)
	{
		OtherData[] otherData = node.otherData;
		if (otherData != null && otherData.Length == 1 && otherData[0].obj is LocationGridTile locationGridTile)
		{
			return locationGridTile.structure;
		}
		return base.GetTargetStructure(node);
	}

	public override LocationGridTile GetTargetTileToGoTo(ActualGoapNode goapNode)
	{
		OtherData[] otherData = goapNode.otherData;
		if (otherData != null && otherData.Length == 1 && otherData[0].obj is LocationGridTile result)
		{
			return result;
		}
		return base.GetTargetTileToGoTo(goapNode);
	}

	public override IPointOfInterest GetTargetToGoTo(ActualGoapNode goapNode)
	{
		return null;
	}

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		Character actor = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		LocationStructure targetStructure = node.targetStructure;
		log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddToFillers(poiTarget, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
		log.AddToFillers(targetStructure, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1, replaceExisting: true, overrideStringValue: true);
	}
}

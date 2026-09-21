using Inner_Maps;
using Inner_Maps.Location_Structures;

public class Visit : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.INDIRECT;

	public Visit()
		: base(INTERACTION_TYPE.VISIT)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION;
		base.actionIconString = GoapActionStateDB.Happy_Icon;
		base.doesNotStopTargetCharacter = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Social };
		base.shouldAddLogs = false;
	}

	public override LocationStructure GetTargetStructure(ActualGoapNode node)
	{
		OtherData[] otherData = node.otherData;
		if (otherData != null && otherData.Length >= 1 && otherData[0].obj is LocationStructure)
		{
			return otherData[0].obj as LocationStructure;
		}
		return null;
	}

	public override LocationGridTile GetTargetTileToGoTo(ActualGoapNode goapNode)
	{
		return null;
	}

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		OtherData[] otherData = node.otherData;
		if (otherData != null && otherData.Length >= 1 && otherData[0].obj is LocationStructure)
		{
			LocationStructure locationStructure = otherData[0].obj as LocationStructure;
			log.AddToFillers(locationStructure, locationStructure.GetNameRelativeTo(node.actor), LOG_IDENTIFIER.LANDMARK_1, replaceExisting: true, overrideStringValue: true);
		}
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Visit Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (otherData.Length == 2)
			{
				IPointOfInterest pointOfInterest = otherData[1].obj as IPointOfInterest;
				return poiTarget == pointOfInterest;
			}
			return actor == poiTarget;
		}
		return false;
	}

	public void AfterVisitSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.trapStructure.SetStructureAndDuration(goapNode.targetStructure, GameManager.Instance.GetTicksBasedOnHour(2) + GameManager.Instance.GetTicksBasedOnMinutes(30));
	}
}

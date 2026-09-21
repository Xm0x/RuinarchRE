using Inner_Maps;
using Inner_Maps.Location_Structures;

public class ReportCorruptedStructure : GoapAction
{
	public ReportCorruptedStructure()
		: base(INTERACTION_TYPE.REPORT_CORRUPTED_STRUCTURE)
	{
		base.actionIconString = GoapActionStateDB.Work_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.UPON_STRUCTURE_ARRIVAL;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Player };
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		Character actor = node.actor;
		_ = node.poiTarget;
		if (!goapActionInvalidity.isInvalid)
		{
			LocationStructure targetStructure = GetTargetStructure(node);
			if (actor.gridTileLocation.structure != targetStructure)
			{
				goapActionInvalidity.isInvalid = true;
				goapActionInvalidity.reason = "not_at_location";
			}
		}
		return goapActionInvalidity;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Report Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override LocationStructure GetTargetStructure(ActualGoapNode node)
	{
		return node.otherData[1].obj as LocationStructure;
	}

	public override LocationGridTile GetTargetTileToGoTo(ActualGoapNode goapNode)
	{
		if (goapNode.otherData[1].obj is LocationStructure locationStructure)
		{
			return locationStructure.GetRandomPassableTile();
		}
		return null;
	}

	public override IPointOfInterest GetTargetToGoTo(ActualGoapNode goapNode)
	{
		return null;
	}

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		LocationStructure locationStructure = node.otherData[0].obj as LocationStructure;
		log.AddToFillers(locationStructure, locationStructure.GetNameRelativeTo(node.actor), LOG_IDENTIFIER.LANDMARK_2, replaceExisting: true, overrideStringValue: true);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (poiTarget == actor && poiTarget.IsAvailable() && poiTarget.gridTileLocation != null)
			{
				return actor.homeSettlement != null;
			}
			return false;
		}
		return false;
	}

	public void AfterReportSuccess(ActualGoapNode goapNode)
	{
		LocationStructure structure = goapNode.otherData[0].obj as LocationStructure;
		if (!InnerMapManager.Instance.HasWorldKnownDemonicStructure(structure))
		{
			InnerMapManager.Instance.AddWorldKnownDemonicStructure(structure);
		}
		AkSoundEngine.PostEvent("Play_Location_Discovered", InnerMapCameraMove.Instance.gameObject);
		PlayerManager.Instance.player.AddCharacterThatHasReported(goapNode.actor);
		PlayerManager.Instance.player.threatComponent.AdjustThreatAndApplyModification(20);
		PlayerManager.Instance.player.retaliationComponent.ReportDemonicStructureRetaliation(goapNode.actor);
		goapNode.actor.faction?.SetIsAwareOfPlayer(p_state: true);
	}
}

using Inner_Maps;
using Inner_Maps.Location_Structures;

public class Open : GoapAction
{
	public Open()
		: base(INTERACTION_TYPE.OPEN)
	{
		base.actionIconString = GoapActionStateDB.Inspect_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Open Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		if (node.poiTarget is TreasureChest { objectInside: not null } treasureChest)
		{
			log.AddToFillers(treasureChest.objectInside, treasureChest.objectInside.name, LOG_IDENTIFIER.CHARACTER_3);
		}
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (poiTarget.IsAvailable())
			{
				return poiTarget.gridTileLocation != null;
			}
			return false;
		}
		return false;
	}

	public void AfterOpenSuccess(ActualGoapNode goapNode)
	{
		TreasureChest treasureChest = goapNode.poiTarget as TreasureChest;
		LocationGridTile gridTileLocation = treasureChest.gridTileLocation;
		LocationStructure structure = gridTileLocation.structure;
		if (treasureChest.objectInside is Mimic mimic)
		{
			if (mimic.marker == null)
			{
				treasureChest.SpawnInitialMimic(gridTileLocation, mimic);
			}
			else
			{
				mimic.SetIsTreasureChest(state: false);
				mimic.marker.PlaceMarkerAt(gridTileLocation);
				mimic.marker.SetVisualState(state: true);
				mimic.marker.SetLightState(p_state: true);
				TraitManager.Instance.CopyStatuses(treasureChest, mimic);
			}
			structure.RemovePOI(goapNode.poiTarget);
			mimic.UnsubscribeToAwakenMimicEvent(treasureChest);
			if (mimic.hasMarker)
			{
				mimic.marker.AddUnprocessedPOI(goapNode.actor);
			}
		}
		else
		{
			IPointOfInterest objectInside = treasureChest.objectInside;
			structure.RemovePOI(goapNode.poiTarget);
			if (treasureChest.objectInside is ResourcePile resourcePile)
			{
				resourcePile.SetResourceInPile(50);
			}
			structure.AddPOI(objectInside, gridTileLocation);
		}
	}
}

using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class DropItem : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;

	public DropItem()
		: base(INTERACTION_TYPE.DROP_ITEM)
	{
		base.actionIconString = GoapActionStateDB.Haul_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION_B;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override Precondition GetPrecondition(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType, out bool isOverridden)
	{
		Precondition result = ((!(target is TileObject tileObject)) ? new Precondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_POI, target.name, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), IsItemInInventory) : new Precondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_POI, tileObject.internalName, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), IsItemInInventory));
		isOverridden = true;
		return result;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Drop Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override LocationStructure GetTargetStructure(ActualGoapNode node)
	{
		return node.otherData[0]?.obj as LocationStructure;
	}

	public override void OnActionStarted(ActualGoapNode node)
	{
		node.actor.ShowItemVisualCarryingPOI(node.poiTarget as TileObject);
	}

	public override void OnStopWhileStarted(ActualGoapNode node)
	{
		base.OnStopWhileStarted(node);
		Character actor = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		if (node.associatedJobType == JOB_TYPE.DEMON_STEAL || node.associatedJobType == JOB_TYPE.DROP_ITEM_TO_WORKPLACE)
		{
			actor.UncarryPOI(poiTarget, bringBackToInventory: true);
		}
		else
		{
			actor.UncarryPOI(poiTarget, bringBackToInventory: false, addToLocation: true, actor.gridTileLocation);
		}
	}

	public override void OnStopWhilePerforming(ActualGoapNode node)
	{
		base.OnStopWhilePerforming(node);
		Character actor = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		if (node.associatedJobType == JOB_TYPE.DEMON_STEAL || node.associatedJobType == JOB_TYPE.DROP_ITEM_TO_WORKPLACE)
		{
			actor.UncarryPOI(poiTarget, bringBackToInventory: true);
		}
		else
		{
			actor.UncarryPOI(poiTarget);
		}
	}

	public override void OnInvalidAction(ActualGoapNode node)
	{
		base.OnInvalidAction(node);
		Character actor = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		if (node.associatedJobType == JOB_TYPE.DEMON_STEAL || node.associatedJobType == JOB_TYPE.DROP_ITEM_TO_WORKPLACE)
		{
			actor.UncarryPOI(poiTarget, bringBackToInventory: true);
		}
		else
		{
			actor.UncarryPOI(poiTarget, bringBackToInventory: false, addToLocation: true, actor.gridTileLocation);
		}
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		_ = node.actor;
		_ = node.poiTarget;
		string stateName = "Target Missing";
		string reason = "target_unavailable";
		bool isInvalid = IsTargetMissingOverride(node, ref reason);
		GoapActionInvalidity invalidity = node.invalidity;
		invalidity.isInvalid = isInvalid;
		invalidity.stateName = stateName;
		invalidity.reason = reason;
		return invalidity;
	}

	private bool IsItemInInventory(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType)
	{
		return actor.HasItemOrEquipment(poiTarget as TileObject);
	}

	public void AfterDropSuccess(ActualGoapNode goapNode)
	{
		LocationGridTile locationGridTile = goapNode.actor.gridTileLocation;
		LocationStructure targetStructure = GetTargetStructure(goapNode);
		if (locationGridTile != null && locationGridTile.tileObjectComponent.objHere != null)
		{
			locationGridTile = goapNode.actor.gridTileLocation.GetFirstNearestTileFromThisWithNoObject(thisStructureOnly: true);
			if (locationGridTile == null)
			{
				locationGridTile = goapNode.actor.gridTileLocation.GetFirstNearestTileFromThisWithNoObject();
			}
		}
		if (targetStructure != null && locationGridTile.structure != targetStructure && targetStructure.passableTiles.Count > 0)
		{
			locationGridTile = CollectionUtilities.GetRandomElement(targetStructure.passableTiles);
		}
		bool addToLocation = locationGridTile != null;
		goapNode.actor.UncarryPOI(goapNode.poiTarget as TileObject, bringBackToInventory: false, addToLocation, locationGridTile);
		if (goapNode.associatedJobType == JOB_TYPE.DEMON_STEAL)
		{
			PlayerManager.Instance.player.goalComponent.CompleteSubGoal(SUB_GOAL.GOAL_SNATCH_OBJECT);
		}
	}

	private bool IsTargetMissingOverride(ActualGoapNode node, ref string reason)
	{
		Character actor = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		LocationStructure targetStructure = GetTargetStructure(node);
		if (targetStructure != null && actor.gridTileLocation.structure != targetStructure)
		{
			reason = "target_unreachable";
			return true;
		}
		if (poiTarget is TileObject && actor.HasItemOrEquipment(poiTarget as TileObject))
		{
			return false;
		}
		if (actor.carryComponent.IsPOICarried(poiTarget))
		{
			return false;
		}
		if (!poiTarget.IsAvailable() || poiTarget.gridTileLocation == null || actor.currentRegion != poiTarget.currentRegion)
		{
			return true;
		}
		if (base.actionLocationType == ACTION_LOCATION_TYPE.NEAR_TARGET)
		{
			if (actor.gridTileLocation != poiTarget.gridTileLocation && !actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation, sameStructureOnly: true))
			{
				if (actor.hasMarker && actor.marker.IsCharacterInLineOfSightWith(poiTarget))
				{
					return false;
				}
				return true;
			}
		}
		else if (base.actionLocationType == ACTION_LOCATION_TYPE.NEAR_OTHER_TARGET && actor.gridTileLocation != node.targetTile && !actor.gridTileLocation.IsNeighbour(node.targetTile, sameStructureOnly: true))
		{
			return true;
		}
		return false;
	}
}

public class DropResourceToWorkStructure : GoapAction
{
	public DropResourceToWorkStructure()
		: base(INTERACTION_TYPE.DROP_RESOURCE_TO_WORK_STRUCTURE)
	{
		base.actionIconString = GoapActionStateDB.Haul_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_OTHER_TARGET;
		base.canBeAdvertisedEvenIfTargetIsUnavailable = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override Precondition GetPrecondition(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType, out bool isOverridden)
	{
		Precondition result = ((!(target is TileObject tileObject)) ? new Precondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.TAKE_POI, target.name, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), IsCarriedOrInInventory) : new Precondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.TAKE_POI, tileObject.internalName, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), IsCarriedOrInInventory));
		isOverridden = true;
		return result;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Drop Resource To Work Structure Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override void OnStopWhileStarted(ActualGoapNode node)
	{
		base.OnStopWhileStarted(node);
		Character actor = node.actor;
		if (actor.carryComponent.carriedPOI is ResourcePile poi)
		{
			actor.UncarryPOI(poi, bringBackToInventory: false, addToLocation: true, actor.gridTileLocation);
		}
	}

	public override void OnStopWhilePerforming(ActualGoapNode node)
	{
		base.OnStopWhilePerforming(node);
		Character actor = node.actor;
		if (actor.carryComponent.carriedPOI is ResourcePile poi)
		{
			actor.UncarryPOI(poi, bringBackToInventory: false, addToLocation: true, actor.gridTileLocation);
		}
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		string stateName = "Target Missing";
		bool isInvalid = IsTargetMissingOverride(node);
		GoapActionInvalidity invalidity = node.invalidity;
		invalidity.isInvalid = isInvalid;
		invalidity.stateName = stateName;
		invalidity.reason = "target_unavailable";
		return invalidity;
	}

	public override void AddFillersToLog(Log log, ActualGoapNode goapNode)
	{
		base.AddFillersToLog(log, goapNode);
		if (goapNode.actor.carryComponent.carriedPOI is ResourcePile resourcePile)
		{
			log.AddToFillers(null, resourcePile.strResourcesInPile, LOG_IDENTIFIER.STRING_1);
			log.AddToFillers(null, resourcePile.providedResource.LocalizedName(), LOG_IDENTIFIER.STRING_2);
		}
	}

	public override void OnActionStarted(ActualGoapNode node)
	{
		node.actor.ShowItemVisualCarryingPOI(node.poiTarget as TileObject);
	}

	private bool IsCarriedOrInInventory(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType)
	{
		if (poiTarget is TileObject tileObject)
		{
			return actor.HasResourcePileWithAmount<ResourcePile>(tileObject.tileObjectType, 40);
		}
		return false;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (actor.IsPOICarriedOrInInventory(poiTarget))
			{
				return true;
			}
			if (poiTarget.gridTileLocation == null)
			{
				return false;
			}
			if (poiTarget.gridTileLocation.IsPartOfSettlement() && actor.homeSettlement != null && !actor.homeSettlement.mainStorage.HasUnoccupiedTile())
			{
				return false;
			}
			return actor.homeRegion == poiTarget.gridTileLocation.parentMap.region;
		}
		return false;
	}

	public void AfterDropResourceToWorkStructureSuccess(ActualGoapNode goapNode)
	{
		Character actor = goapNode.actor;
		ResourcePile resourcePile = goapNode.poiTarget as ResourcePile;
		_ = goapNode.otherData;
		if (resourcePile == null)
		{
			return;
		}
		ResourcePile resourcePile2 = resourcePile;
		if (resourcePile2.mapObjectState == MAP_OBJECT_STATE.UNBUILT)
		{
			resourcePile2.gridTileLocation.structure.RemovePOI(resourcePile2);
			if (actor.carryComponent.isCarryingAnyPOI)
			{
				actor.UncarryPOI(actor.carryComponent.carriedPOI, bringBackToInventory: false, addToLocation: true, goapNode.targetTile);
				return;
			}
			TileObject item = actor.GetItem(resourcePile2.tileObjectType);
			if (item != null)
			{
				actor.DropItem(item, goapNode.targetTile);
			}
		}
		else
		{
			resourcePile2.AdjustResourceInPile(resourcePile.resourceInPile);
			resourcePile.traitContainer.RemoveStatusAndStacks(resourcePile, "Burnt");
			TraitManager.Instance.CopyStatuses(resourcePile, resourcePile2);
			if (actor.carryComponent.isCarryingAnyPOI)
			{
				actor.UncarryPOI(actor.carryComponent.carriedPOI, bringBackToInventory: false, addToLocation: true, goapNode.targetTile);
			}
			else
			{
				actor.UnobtainItem(resourcePile);
			}
		}
	}

	private bool IsTargetMissingOverride(ActualGoapNode node)
	{
		Character actor = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		if (actor.carryComponent.IsPOICarried(poiTarget))
		{
			return false;
		}
		if (poiTarget.gridTileLocation == null || actor.currentRegion != poiTarget.currentRegion)
		{
			return true;
		}
		if (actor.gridTileLocation != node.targetTile && !actor.gridTileLocation.IsNeighbour(node.targetTile))
		{
			return true;
		}
		return false;
	}
}

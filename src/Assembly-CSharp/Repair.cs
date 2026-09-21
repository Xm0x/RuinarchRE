public class Repair : GoapAction
{
	private Precondition _stonePrecondition;

	private Precondition _woodPrecondition;

	private Precondition _metalPrecondition;

	public Repair()
		: base(INTERACTION_TYPE.REPAIR)
	{
		base.actionIconString = GoapActionStateDB.Repair_Icon;
		base.canBeAdvertisedEvenIfTargetIsUnavailable = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
		_stonePrecondition = new Precondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.TAKE_POI, "Stone Pile", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), HasResource);
		_woodPrecondition = new Precondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.TAKE_POI, "Wood Pile", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), HasResource);
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Burnt", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
	}

	public override Precondition GetPrecondition(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType, out bool isOverridden)
	{
		Precondition result = null;
		if (actor.homeSettlement != null)
		{
			if (actor.homeSettlement.mainStorage.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.WOOD_PILE))
			{
				result = _woodPrecondition;
			}
			else if (actor.homeSettlement.mainStorage.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.STONE_PILE))
			{
				result = _stonePrecondition;
			}
		}
		else
		{
			result = _woodPrecondition;
		}
		isOverridden = true;
		return result;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Repair Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		_ = node.actor;
		_ = node.poiTarget;
		string stateName = "Target Missing";
		bool isInvalid = IsRepairTargetMissing(node);
		GoapActionInvalidity invalidity = node.invalidity;
		invalidity.isInvalid = isInvalid;
		invalidity.stateName = stateName;
		invalidity.reason = "target_unavailable";
		if (!invalidity.isInvalid && node.poiTarget is GenericTileObject genericTileObject && (genericTileObject.gridTileLocation.tileObjectComponent.IsAffectedByAOESpell(TILE_OBJECT_TYPE.BLIZZARD_TILE_OBJECT) || genericTileObject.gridTileLocation.tileObjectComponent.IsAffectedByAOESpell(TILE_OBJECT_TYPE.HEAT_WAVE_TILE_OBJECT)))
		{
			invalidity.isInvalid = true;
			invalidity.reason = "bad_weather";
		}
		return invalidity;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest target, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, target, otherData, job))
		{
			return target.gridTileLocation != null;
		}
		return false;
	}

	private bool IsRepairTargetMissing(ActualGoapNode node)
	{
		Character actor = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		if (poiTarget.gridTileLocation == null || actor.currentRegion != poiTarget.currentRegion)
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

	public override void OnStopWhileStarted(ActualGoapNode node)
	{
		base.OnStopWhileStarted(node);
		node.actor.UncarryPOI();
	}

	public override void OnStopWhilePerforming(ActualGoapNode node)
	{
		base.OnStopWhilePerforming(node);
		Character actor = node.actor;
		_ = node.poiTarget;
		actor.UncarryPOI();
	}

	public void PreRepairSuccess(ActualGoapNode goapNode)
	{
		TileObject tileObject = goapNode.poiTarget as TileObject;
		_ = goapNode.actor;
		int repairCost = TileObjectDB.GetTileObjectData(tileObject.tileObjectType).repairCost;
		if (goapNode.actor.carryComponent.carriedPOI != null && goapNode.actor.carryComponent.carriedPOI is ResourcePile resourcePile)
		{
			resourcePile.AdjustResourceInPile(-repairCost);
			tileObject.resourceStorageComponent.AdjustResource(resourcePile.specificProvidedResource, repairCost);
		}
	}

	public void AfterRepairSuccess(ActualGoapNode goapNode)
	{
		goapNode.poiTarget.traitContainer.RemoveTrait(goapNode.poiTarget, "Burnt");
		goapNode.poiTarget.traitContainer.RemoveTrait(goapNode.poiTarget, "Damaged");
		TileObject tileObject = goapNode.poiTarget as TileObject;
		_ = goapNode.actor;
		tileObject.resourceStorageComponent.ClearAllResources();
		int amount = tileObject.maxHP - tileObject.currentHP;
		tileObject.AdjustHP(amount, ELEMENTAL_TYPE.Normal);
	}

	private bool HasResource(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType)
	{
		int repairCost = TileObjectDB.GetTileObjectData((poiTarget as TileObject).tileObjectType).repairCost;
		if (poiTarget.resourceStorageComponent.HasResourceAmount(RESOURCE.WOOD, repairCost) || poiTarget.resourceStorageComponent.HasResourceAmount(RESOURCE.STONE, repairCost) || poiTarget.resourceStorageComponent.HasResourceAmount(RESOURCE.METAL, repairCost))
		{
			return true;
		}
		if (actor.carryComponent.carriedPOI is ResourcePile)
		{
			return !(actor.carryComponent.carriedPOI is FoodPile);
		}
		return false;
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Goap.Unique_Action_Data;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

public class CraftEquipment : GoapAction
{
	public override Type uniqueActionDataType => typeof(CraftEquipmentUAD);

	public CraftEquipment()
		: base(INTERACTION_TYPE.CRAFT_EQUIPMENT)
	{
		base.actionIconString = GoapActionStateDB.Work_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
		base.canBeAdvertisedEvenIfTargetIsUnavailable = true;
	}

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		string text = (node.poiTarget as TileObject).tileObjectType.LocalizedName();
		log.AddToFillers(null, Utilities.GetArticleForWord(text), LOG_IDENTIFIER.STRING_1);
		log.AddToFillers(null, text, LOG_IDENTIFIER.ITEM_1);
	}

	public override Precondition GetPrecondition(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType, out bool isOverridden)
	{
		if (target is TileObject tileObject)
		{
			List<CONCRETE_RESOURCES> resourcesNeeded = EquipmentDataHandler.Instance.GetResourcesNeeded(tileObject.tileObjectType);
			RESOURCE generalResourcesNeeded = EquipmentDataHandler.Instance.GetGeneralResourcesNeeded(tileObject.tileObjectType);
			int resourcesNeededAmount = EquipmentDataHandler.Instance.GetResourcesNeededAmount(tileObject.tileObjectType);
			Workshop workshop = actor.structureComponent.workPlaceStructure as Workshop;
			Precondition precondition = null;
			if (resourcesNeeded != null && resourcesNeeded.Count > 0)
			{
				if (workshop.CanBeCrafted(resourcesNeeded, resourcesNeededAmount, out var foundResourcePile))
				{
					precondition = new Precondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.TAKE_POI, foundResourcePile.internalName, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), HasResource);
					isOverridden = true;
					return precondition;
				}
				string p_conditionKey = resourcesNeeded.FirstOrDefault().ConvertResourcesToTileObjectType().ToStringEnumWithSpace();
				precondition = new Precondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.TAKE_POI, p_conditionKey, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), HasResource);
				isOverridden = true;
				return precondition;
			}
			switch (generalResourcesNeeded)
			{
			case RESOURCE.WOOD:
				precondition = new Precondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.TAKE_POI, "Wood Pile", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), HasWood);
				goto IL_01d9;
			case RESOURCE.STONE:
				precondition = new Precondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.TAKE_POI, "Stone Pile", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), HasStone);
				goto IL_01d9;
			case RESOURCE.METAL:
				precondition = new Precondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.TAKE_POI, "Metal Pile", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), HasMetal);
				goto IL_01d9;
			case RESOURCE.CLOTH:
				precondition = new Precondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.TAKE_POI, "Cloth Pile", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), HasCloth);
				goto IL_01d9;
			case RESOURCE.LEATHER:
				precondition = new Precondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.TAKE_POI, "Leather Pile", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), HasLeather);
				goto IL_01d9;
			default:
				throw new ArgumentOutOfRangeException();
			case RESOURCE.NONE:
				break;
				IL_01d9:
				isOverridden = true;
				return precondition;
			}
		}
		return base.GetPrecondition(actor, target, otherData, jobType, out isOverridden);
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Craft Equipment Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override int DetermineActionDuration(GoapActionState p_goapActionState, ActualGoapNode p_actualGoapNode)
	{
		return (p_actualGoapNode.target as TileObject).constructionComponent.GetRemainingTicksForConstruction();
	}

	public override void OnStopWhilePerforming(ActualGoapNode node)
	{
		base.OnStopWhilePerforming(node);
		TileObject tileObject = node.poiTarget as TileObject;
		tileObject.constructionComponent.OnConstructionCancelled(tileObject);
	}

	public override void OnActionStarted(ActualGoapNode node)
	{
		base.OnActionStarted(node);
		if (node.poiTarget is TileObject tileObject)
		{
			ResourcePile resourcePileToUse = GetResourcePileToUse(node.actor, tileObject);
			node.actor.ShowItemVisualCarryingPOI(resourcePileToUse);
		}
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (actor.structureComponent.HasWorkPlaceStructure())
			{
				return actor.structureComponent.workPlaceStructure is Workshop;
			}
			return false;
		}
		return false;
	}

	private bool HasWood(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType)
	{
		if (poiTarget is TileObject tileObject)
		{
			if (tileObject.mapObjectState == MAP_OBJECT_STATE.BUILDING)
			{
				return true;
			}
			if (actor.GetItem(TILE_OBJECT_TYPE.WOOD_PILE) is ResourcePile resourcePile)
			{
				return resourcePile.resourceInPile >= EquipmentDataHandler.Instance.GetResourcesNeededAmount(tileObject.tileObjectType);
			}
			return false;
		}
		return false;
	}

	private bool HasStone(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType)
	{
		if (poiTarget is TileObject tileObject)
		{
			if (tileObject.mapObjectState == MAP_OBJECT_STATE.BUILDING)
			{
				return true;
			}
			if (actor.GetItem(TILE_OBJECT_TYPE.STONE_PILE) is ResourcePile resourcePile)
			{
				return resourcePile.resourceInPile >= EquipmentDataHandler.Instance.GetResourcesNeededAmount(tileObject.tileObjectType);
			}
			return false;
		}
		return false;
	}

	private bool HasMetal(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType)
	{
		return HasResource<MetalPile>(actor, poiTarget, otherData, jobType);
	}

	private bool HasCloth(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType)
	{
		return HasResource<ClothPile>(actor, poiTarget, otherData, jobType);
	}

	private bool HasResource<T>(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType) where T : ResourcePile
	{
		if (poiTarget is TileObject tileObject)
		{
			if (tileObject.mapObjectState == MAP_OBJECT_STATE.BUILDING)
			{
				return true;
			}
			ResourcePile item = actor.GetItem<T>();
			if (item != null)
			{
				return item.resourceInPile >= EquipmentDataHandler.Instance.GetResourcesNeededAmount(tileObject.tileObjectType);
			}
			return false;
		}
		return false;
	}

	private bool HasLeather(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType)
	{
		if (poiTarget is TileObject tileObject)
		{
			ResourcePile item = actor.GetItem<LeatherPile>();
			if (item != null)
			{
				return item.resourceInPile >= EquipmentDataHandler.Instance.GetResourcesNeededAmount(tileObject.tileObjectType);
			}
		}
		return false;
	}

	private bool HasResource(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType)
	{
		if (poiTarget is TileObject tileObject)
		{
			if (tileObject.mapObjectState == MAP_OBJECT_STATE.BUILDING)
			{
				return true;
			}
			return GetResourcePileToUse(actor, tileObject) != null;
		}
		return false;
	}

	public void PreCraftEquipmentSuccess(ActualGoapNode p_node)
	{
		TileObject tileObject = p_node.target as TileObject;
		if (tileObject.mapObjectState == MAP_OBJECT_STATE.UNBUILT)
		{
			ResourcePile resourcePile = p_node.actor.carryComponent.carriedPOI as ResourcePile;
			p_node.GetConvertedUniqueActionData<CraftEquipmentUAD>().SetResourceUsedForCrafting(resourcePile.specificProvidedResource);
			int resourceInPile = resourcePile.resourceInPile;
			tileObject.resourceStorageComponent.AdjustResource(resourcePile.specificProvidedResource, resourceInPile);
			resourcePile.AdjustResourceInPile(-resourceInPile);
			tileObject.SetMapObjectState(MAP_OBJECT_STATE.BUILDING);
		}
		else if (tileObject.mapObjectState == MAP_OBJECT_STATE.BUILDING)
		{
			CraftEquipmentUAD convertedUniqueActionData = p_node.GetConvertedUniqueActionData<CraftEquipmentUAD>();
			CONCRETE_RESOURCES firstResourceWithValue = tileObject.resourceStorageComponent.GetFirstResourceWithValue();
			convertedUniqueActionData.SetResourceUsedForCrafting(firstResourceWithValue);
		}
		tileObject.constructionComponent.OnConstructionResumed(tileObject);
	}

	public void PerTickCraftEquipmentSuccess(ActualGoapNode p_node)
	{
		TileObject tileObject = p_node.target as TileObject;
		tileObject.constructionComponent.IncreaseConstructionTick(1, tileObject);
		int resourcesNeededAmount = EquipmentDataHandler.Instance.GetResourcesNeededAmount(tileObject.tileObjectType);
		int constructionTimeInTicks = TileObjectDB.GetTileObjectData(tileObject.tileObjectType).constructionTimeInTicks;
		int num = Mathf.CeilToInt((float)resourcesNeededAmount / (float)constructionTimeInTicks);
		CraftEquipmentUAD convertedUniqueActionData = p_node.GetConvertedUniqueActionData<CraftEquipmentUAD>();
		tileObject.resourceStorageComponent.AdjustResource(convertedUniqueActionData.resourceUsedForCrafting, -num);
		AkSoundEngine.PostEvent("Play_Crafting_Equipment", p_node.actor.marker.gameObject);
	}

	public void AfterCraftEquipmentSuccess(ActualGoapNode p_node)
	{
		TileObject tileObject = p_node.target as TileObject;
		tileObject.SetMapObjectState(MAP_OBJECT_STATE.BUILT);
		tileObject.resourceStorageComponent.ClearAllResources();
		EquipmentItem equipmentItem = tileObject as EquipmentItem;
		p_node.actor.moneyComponent.AdjustCoins(28);
		equipmentItem.TryAddRandomPrefix();
		if (p_node.actor.TryGetTalentLevel(CHARACTER_TALENT.Crafting) >= 5)
		{
			if (GameUtilities.RollChance(20))
			{
				equipmentItem.MakeQualityPremium();
			}
			else if (GameUtilities.RollChance(20))
			{
				equipmentItem.MakeQualityHigh();
			}
		}
		else if (p_node.actor.TryGetTalentLevel(CHARACTER_TALENT.Crafting) >= 3 && GameUtilities.RollChance(20))
		{
			equipmentItem.MakeQualityHigh();
		}
		if (p_node.actor.structureComponent.workPlaceStructure is Workshop workshop)
		{
			workshop.RemoveFirstRequestThatIsFulfilledBy(tileObject);
		}
		p_node.actor.talentComponent?.GetTalent(CHARACTER_TALENT.Crafting).AdjustExperience(25, p_node.actor);
	}

	private ResourcePile GetResourcePileToUse(Character actor, TileObject tileObject)
	{
		List<CONCRETE_RESOURCES> resourcesNeeded = EquipmentDataHandler.Instance.GetResourcesNeeded(tileObject.tileObjectType);
		RESOURCE generalResourcesNeeded = EquipmentDataHandler.Instance.GetGeneralResourcesNeeded(tileObject.tileObjectType);
		int resourcesNeededAmount = EquipmentDataHandler.Instance.GetResourcesNeededAmount(tileObject.tileObjectType);
		if (resourcesNeeded.Count > 0)
		{
			for (int i = 0; i < resourcesNeeded.Count; i++)
			{
				CONCRETE_RESOURCES p_resrouce = resourcesNeeded[i];
				if (actor.GetItem(p_resrouce.ConvertResourcesToTileObjectType()) is ResourcePile resourcePile && resourcePile.resourceInPile >= resourcesNeededAmount)
				{
					return resourcePile;
				}
			}
		}
		else
		{
			switch (generalResourcesNeeded)
			{
			case RESOURCE.WOOD:
			{
				ResourcePile item = actor.GetItem<WoodPile>();
				if (item != null && item.resourceInPile >= resourcesNeededAmount)
				{
					return item;
				}
				break;
			}
			case RESOURCE.STONE:
			{
				ResourcePile item = actor.GetItem<StonePile>();
				if (item != null && item.resourceInPile >= resourcesNeededAmount)
				{
					return item;
				}
				break;
			}
			case RESOURCE.METAL:
			{
				ResourcePile item = actor.GetItem<MetalPile>();
				if (item != null && item.resourceInPile >= resourcesNeededAmount)
				{
					return item;
				}
				break;
			}
			case RESOURCE.CLOTH:
			{
				ResourcePile item = actor.GetItem<ClothPile>();
				if (item != null && item.resourceInPile >= resourcesNeededAmount)
				{
					return item;
				}
				break;
			}
			case RESOURCE.LEATHER:
			{
				ResourcePile item = actor.GetItem<LeatherPile>();
				if (item != null && item.resourceInPile >= resourcesNeededAmount)
				{
					return item;
				}
				break;
			}
			default:
				throw new ArgumentOutOfRangeException();
			case RESOURCE.NONE:
				break;
			}
		}
		return null;
	}
}

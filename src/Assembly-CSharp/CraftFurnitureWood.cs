using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

public class CraftFurnitureWood : GoapAction
{
	public CraftFurnitureWood()
		: base(INTERACTION_TYPE.CRAFT_FURNITURE_WOOD)
	{
		base.actionIconString = GoapActionStateDB.Build_Icon;
		base.canBeAdvertisedEvenIfTargetIsUnavailable = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		SetPrecondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.TAKE_POI, "Wood Pile", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), HasWood);
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Craft Success", goapNode);
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

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		TileObject tileObject = node.poiTarget as TileObject;
		log.AddToFillers(null, Utilities.GetArticleForWord(tileObject.name), LOG_IDENTIFIER.STRING_1);
		log.AddToFillers(null, tileObject.name, LOG_IDENTIFIER.ITEM_1);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override void OnStopWhilePerforming(ActualGoapNode node)
	{
		base.OnStopWhilePerforming(node);
		TileObject tileObject = node.poiTarget as TileObject;
		tileObject.constructionComponent.OnConstructionCancelled(tileObject);
	}

	public override void OnActionStarted(ActualGoapNode node)
	{
		TileObject item = node.actor.GetItem(TILE_OBJECT_TYPE.WOOD_PILE);
		if (item != null)
		{
			node.actor.ShowItemVisualCarryingPOI(item);
		}
	}

	public override int DetermineActionDuration(GoapActionState p_goapActionState, ActualGoapNode p_actualGoapNode)
	{
		return (p_actualGoapNode.target as TileObject).constructionComponent.GetRemainingTicksForConstruction();
	}

	public void PreCraftSuccess(ActualGoapNode goapNode)
	{
		Character actor = goapNode.actor;
		TileObject tileObject = goapNode.poiTarget as TileObject;
		if (tileObject.mapObjectState == MAP_OBJECT_STATE.UNBUILT)
		{
			List<WoodPile> list = RuinarchListPool<WoodPile>.Claim();
			actor.PopulateItemsOfType(list);
			int craftResourceCost = TileObjectDB.GetTileObjectData(tileObject.tileObjectType).craftResourceCost;
			int num = craftResourceCost;
			for (int i = 0; i < list.Count; i++)
			{
				if (num <= 0)
				{
					break;
				}
				WoodPile woodPile = list[i];
				int num2 = num;
				if (num2 > woodPile.resourceInPile)
				{
					num2 = woodPile.resourceInPile;
				}
				woodPile.AdjustResourceInPile(-num2);
				num -= num2;
			}
			tileObject.SetMapObjectState(MAP_OBJECT_STATE.BUILDING);
			tileObject.resourceStorageComponent.AdjustResource(CONCRETE_RESOURCES.Wood, craftResourceCost);
			if (tileObject.tileObjectType == TILE_OBJECT_TYPE.BED)
			{
				BedTileObjectScriptableObject tileObjectScriptableObject = InnerMapManager.Instance.GetTileObjectScriptableObject<BedTileObjectScriptableObject>(TILE_OBJECT_TYPE.BED);
				tileObject.mapObjectVisual.SetVisual(tileObjectScriptableObject.GetBedSpriteToUseForRace(actor.race));
			}
			else if (tileObject.tileObjectType == TILE_OBJECT_TYPE.TABLE)
			{
				TableTileObjectScriptableObject tileObjectScriptableObject2 = InnerMapManager.Instance.GetTileObjectScriptableObject<TableTileObjectScriptableObject>(TILE_OBJECT_TYPE.TABLE);
				tileObject.mapObjectVisual.SetVisual(tileObjectScriptableObject2.GetTableSpriteToUseForRace(actor.race));
			}
		}
		tileObject.constructionComponent.OnConstructionResumed(tileObject);
		goapNode.descriptionLog.AddToFillers(null, Utilities.GetArticleForWord(tileObject.name), LOG_IDENTIFIER.STRING_1);
		goapNode.descriptionLog.AddToFillers(null, tileObject.name, LOG_IDENTIFIER.ITEM_1);
		if (goapNode.thoughtBubbleLog != null)
		{
			goapNode.thoughtBubbleLog.AddToFillers(null, Utilities.GetArticleForWord(tileObject.name), LOG_IDENTIFIER.STRING_1);
			goapNode.thoughtBubbleLog.AddToFillers(null, tileObject.name, LOG_IDENTIFIER.ITEM_1);
		}
	}

	public void PerTickCraftSuccess(ActualGoapNode p_node)
	{
		TileObject tileObject = p_node.target as TileObject;
		tileObject.constructionComponent.IncreaseConstructionTick(1, tileObject);
		TileObjectData tileObjectData = TileObjectDB.GetTileObjectData(tileObject.tileObjectType);
		int craftResourceCost = tileObjectData.craftResourceCost;
		int constructionTimeInTicks = tileObjectData.constructionTimeInTicks;
		int num = Mathf.CeilToInt((float)craftResourceCost / (float)constructionTimeInTicks);
		tileObject.resourceStorageComponent.AdjustResource(CONCRETE_RESOURCES.Wood, -num);
	}

	public void AfterCraftSuccess(ActualGoapNode goapNode)
	{
		TileObject tileObject = goapNode.poiTarget as TileObject;
		tileObject.SetMapObjectState(MAP_OBJECT_STATE.BUILT);
		if (tileObject.tileObjectType == TILE_OBJECT_TYPE.BED)
		{
			BedTileObjectScriptableObject tileObjectScriptableObject = InnerMapManager.Instance.GetTileObjectScriptableObject<BedTileObjectScriptableObject>(TILE_OBJECT_TYPE.BED);
			tileObject.mapObjectVisual.SetVisual(tileObjectScriptableObject.GetBedSpriteToUseForRace(goapNode.actor.race));
		}
		else if (tileObject.tileObjectType == TILE_OBJECT_TYPE.TABLE)
		{
			TableTileObjectScriptableObject tileObjectScriptableObject2 = InnerMapManager.Instance.GetTileObjectScriptableObject<TableTileObjectScriptableObject>(TILE_OBJECT_TYPE.TABLE);
			tileObject.mapObjectVisual.SetVisual(tileObjectScriptableObject2.GetTableSpriteToUseForRace(goapNode.actor.race));
			tileObject.RevalidateTileObjectSlots();
		}
		if (goapNode.associatedJobType == JOB_TYPE.CRAFT_MISSING_FURNITURE && tileObject.gridTileLocation != null && tileObject.gridTileLocation.structure == goapNode.actor.homeStructure)
		{
			LocationStructure homeStructure = goapNode.actor.homeStructure;
			if (homeStructure == null || homeStructure.structureType != STRUCTURE_TYPE.BANDIT_CAMP)
			{
				tileObject.SetCharacterOwner(goapNode.actor);
			}
		}
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
				return resourcePile.resourceInPile >= TileObjectDB.GetTileObjectData(tileObject.tileObjectType).craftResourceCost;
			}
			return false;
		}
		return false;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job) && poiTarget is TileObject)
		{
			TileObject tileObject = poiTarget as TileObject;
			if (tileObject.mapObjectState != MAP_OBJECT_STATE.UNBUILT)
			{
				return tileObject.mapObjectState == MAP_OBJECT_STATE.BUILDING;
			}
			return true;
		}
		return false;
	}
}

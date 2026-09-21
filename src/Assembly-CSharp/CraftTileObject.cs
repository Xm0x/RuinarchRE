using System;
using Goap.Unique_Action_Data;
using UnityEngine;
using UtilityScripts;

public class CraftTileObject : GoapAction
{
	public override Type uniqueActionDataType => typeof(CraftTileObjectUAD);

	public CraftTileObject()
		: base(INTERACTION_TYPE.CRAFT_TILE_OBJECT)
	{
		base.actionIconString = GoapActionStateDB.Build_Icon;
		base.canBeAdvertisedEvenIfTargetIsUnavailable = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override Precondition GetPrecondition(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType, out bool isOverridden)
	{
		if (target is TileObject tileObject)
		{
			TileObjectRecipe possibleRecipe = default(TileObjectRecipe);
			if (otherData != null && otherData.Length >= 1)
			{
				possibleRecipe = (TileObjectRecipe)otherData[0].obj;
			}
			else
			{
				TileObjectData tileObjectData = TileObjectDB.GetTileObjectData(tileObject.tileObjectType);
				if (tileObjectData?.craftRecipes != null)
				{
					tileObjectData.TryGetPossibleRecipe(actor.currentRegion, out possibleRecipe);
				}
			}
			Precondition result = null;
			if (possibleRecipe.hasValue && !string.IsNullOrEmpty(possibleRecipe.ingredient.ingredientName))
			{
				TileObjectRecipeIngredient ingredient = possibleRecipe.ingredient;
				string req = ingredient.ingredientName;
				result = ((req == "Wood Pile") ? new Precondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.TAKE_POI, req, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), HasWood) : ((req == "Stone Pile") ? new Precondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.TAKE_POI, req, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), HasStone) : ((!(req == "Metal Pile")) ? new Precondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_POI, req, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), (Character thisActor, IPointOfInterest thisTarget, OtherData[] thisOtherData, JOB_TYPE thisJobType) => IsCarriedOrInInventory(thisActor, thisTarget, thisOtherData, req)) : new Precondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.TAKE_POI, req, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), HasStone))));
			}
			isOverridden = true;
			return result;
		}
		return base.GetPrecondition(actor, target, otherData, jobType, out isOverridden);
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
		string text = (node.poiTarget as TileObject).tileObjectType.LocalizedName();
		log.AddToFillers(null, Utilities.GetArticleForWord(text), LOG_IDENTIFIER.STRING_1);
		log.AddToFillers(null, text, LOG_IDENTIFIER.ITEM_1);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return Utilities.Rng.Next(150, 201);
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
		TileObject tileObject = node.poiTarget as TileObject;
		actor.UncarryPOI();
		tileObject.constructionComponent.OnConstructionCancelled(tileObject);
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
			TileObjectRecipe recipeToUse = GetRecipeToUse(goapNode);
			goapNode.GetConvertedUniqueActionData<CraftTileObjectUAD>().SetRecipeUsedForCrafting(recipeToUse);
			if (recipeToUse.hasValue && !string.IsNullOrEmpty(recipeToUse.ingredient.ingredientName))
			{
				TileObjectRecipeIngredient ingredient = recipeToUse.ingredient;
				string ingredientName = ingredient.ingredientName;
				if (ingredientName == "Wood Pile")
				{
					(actor.GetItem(TILE_OBJECT_TYPE.WOOD_PILE) as ResourcePile)?.AdjustResourceInPile(-ingredient.amount);
					tileObject.resourceStorageComponent.AdjustResource(CONCRETE_RESOURCES.Wood, ingredient.amount);
				}
				else if (ingredientName == "Stone Pile")
				{
					(actor.GetItem(TILE_OBJECT_TYPE.STONE_PILE) as ResourcePile)?.AdjustResourceInPile(-ingredient.amount);
					tileObject.resourceStorageComponent.AdjustResource(CONCRETE_RESOURCES.Stone, ingredient.amount);
				}
				else if (actor.GetItem(ingredientName) is ResourcePile resourcePile)
				{
					resourcePile.AdjustResourceInPile(-ingredient.amount);
					tileObject.resourceStorageComponent.AdjustResource(resourcePile.specificProvidedResource, ingredient.amount);
				}
				else
				{
					actor.UnobtainItem(ingredientName);
				}
			}
			tileObject.SetMapObjectState(MAP_OBJECT_STATE.BUILDING);
		}
		else if (tileObject.mapObjectState == MAP_OBJECT_STATE.BUILDING)
		{
			CraftTileObjectUAD convertedUniqueActionData = goapNode.GetConvertedUniqueActionData<CraftTileObjectUAD>();
			TileObjectRecipe recipeToUse2 = GetRecipeToUse(goapNode);
			convertedUniqueActionData.SetRecipeUsedForCrafting(recipeToUse2);
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

	public void PerTickCraftSuccess(ActualGoapNode goapNode)
	{
		TileObject tileObject = goapNode.target as TileObject;
		CraftTileObjectUAD convertedUniqueActionData = goapNode.GetConvertedUniqueActionData<CraftTileObjectUAD>();
		tileObject.constructionComponent.IncreaseConstructionTick(1, tileObject);
		if (convertedUniqueActionData.recipeUsed.hasValue)
		{
			int amount = convertedUniqueActionData.recipeUsed.ingredient.amount;
			int constructionTimeInTicks = TileObjectDB.GetTileObjectData(tileObject.tileObjectType).constructionTimeInTicks;
			int num = Mathf.CeilToInt((float)amount / (float)constructionTimeInTicks);
			if (convertedUniqueActionData.recipeUsed.ingredient.ingredientName == "Wood Pile")
			{
				tileObject.resourceStorageComponent.AdjustResource(CONCRETE_RESOURCES.Wood, -num);
			}
			else if (convertedUniqueActionData.recipeUsed.ingredient.ingredientName == "Stone Pile")
			{
				tileObject.resourceStorageComponent.AdjustResource(CONCRETE_RESOURCES.Stone, -num);
			}
		}
	}

	public void AfterCraftSuccess(ActualGoapNode goapNode)
	{
		TileObject tileObject = goapNode.poiTarget as TileObject;
		tileObject.resourceStorageComponent.ClearAllResources();
		tileObject.SetMapObjectState(MAP_OBJECT_STATE.BUILT);
		if (goapNode.associatedJobType == JOB_TYPE.CRAFT_MISSING_FURNITURE)
		{
			tileObject.SetCharacterOwner(goapNode.actor);
		}
		if (tileObject is WardLight wardLight && goapNode.otherData != null && goapNode.otherData.Length >= 1)
		{
			NPCSettlement settlementOwner = (NPCSettlement)goapNode.otherData[1].obj;
			wardLight.SetSettlementOwner(settlementOwner);
		}
	}

	private bool IsCarriedOrInInventory(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, string itemName)
	{
		if (poiTarget is TileObject tileObject)
		{
			if (tileObject.mapObjectState == MAP_OBJECT_STATE.BUILDING)
			{
				return true;
			}
			return actor.IsPOICarriedOrInInventory(itemName);
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
			if (actor.GetItem(TILE_OBJECT_TYPE.WOOD_PILE) is ResourcePile { resourceInPile: var resourceInPile })
			{
				return resourceInPile >= TileObjectDB.GetTileObjectData(tileObject.tileObjectType).mainRecipe.GetNeededAmountForIngredient(TILE_OBJECT_TYPE.WOOD_PILE);
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
			if (actor.GetItem(TILE_OBJECT_TYPE.STONE_PILE) is ResourcePile { resourceInPile: var resourceInPile })
			{
				return resourceInPile >= TileObjectDB.GetTileObjectData(tileObject.tileObjectType).mainRecipe.GetNeededAmountForIngredient(TILE_OBJECT_TYPE.STONE_PILE);
			}
			return false;
		}
		return false;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job) && poiTarget is TileObject tileObject)
		{
			if (tileObject.mapObjectState != MAP_OBJECT_STATE.UNBUILT)
			{
				return tileObject.mapObjectState == MAP_OBJECT_STATE.BUILDING;
			}
			return true;
		}
		return false;
	}

	private TileObjectRecipe GetRecipeToUse(ActualGoapNode goapNode)
	{
		Character actor = goapNode.actor;
		TileObject tileObject = goapNode.poiTarget as TileObject;
		if (goapNode.otherData != null && goapNode.otherData.Length >= 1)
		{
			return (TileObjectRecipe)goapNode.otherData[0].obj;
		}
		TileObjectData tileObjectData = TileObjectDB.GetTileObjectData(tileObject.tileObjectType);
		if (tileObjectData?.craftRecipes != null)
		{
			if (actor.carryComponent.carriedPOI is TileObject tileObject2)
			{
				return tileObjectData.GetRecipeThatUses(tileObject2.tileObjectType);
			}
			if (tileObject.resourceStorageComponent.TryGetFirstResourceWithValue(out var p_resource))
			{
				return tileObjectData.GetRecipeThatUses(p_resource.ConvertResourcesToTileObjectType());
			}
			return tileObjectData.mainRecipe;
		}
		return default(TileObjectRecipe);
	}
}

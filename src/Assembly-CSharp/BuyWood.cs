using System;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class BuyWood : GoapAction
{
	public BuyWood()
		: base(INTERACTION_TYPE.BUY_WOOD)
	{
		base.actionIconString = GoapActionStateDB.Steal_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Needs };
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.TAKE_POI, "Wood Pile", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_POI, "Wood Pile", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.BUY_OBJECT, "Wood Pile", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		if (goapNode.poiTarget is WoodPile && goapNode.poiTarget.gridTileLocation != null && goapNode.poiTarget.gridTileLocation.structure is ManMadeStructure manMadeStructure && manMadeStructure.CanPurchaseFromHere(goapNode.actor, out var needsToPay, out var _))
		{
			SetState(needsToPay ? "Buy Success" : "Take Success", goapNode);
		}
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		if (!goapActionInvalidity.isInvalid && node.poiTarget is WoodPile woodPile && node.poiTarget.gridTileLocation != null && node.poiTarget.gridTileLocation.structure is ManMadeStructure manMadeStructure)
		{
			if (manMadeStructure.CanPurchaseFromHere(node.actor, out var needsToPay, out var _))
			{
				if (needsToPay && !node.actor.moneyComponent.CanAfford(GetBuyCost(node)))
				{
					goapActionInvalidity.isInvalid = true;
					goapActionInvalidity.reason = "not_enough_money";
				}
				else if (woodPile.resourceInPile < GetResourceAmount(node))
				{
					goapActionInvalidity.isInvalid = true;
					goapActionInvalidity.reason = "not_enough_resources";
				}
			}
			else
			{
				goapActionInvalidity.isInvalid = true;
				goapActionInvalidity.reason = "cannot_buy";
			}
		}
		return goapActionInvalidity;
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		if (actor.movementComponent.ShouldAvoidStructureLocationOfTarget(target))
		{
			return 2000;
		}
		return 10;
	}

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		log.AddToFillers(null, GetBuyCost(node).ToString(), LOG_IDENTIFIER.STRING_1);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (job.jobType != JOB_TYPE.CRAFT_MISSING_FURNITURE)
			{
				return false;
			}
			if (poiTarget is WoodPile woodPile && poiTarget.gridTileLocation != null && poiTarget.gridTileLocation.structure is ManMadeStructure manMadeStructure && manMadeStructure.CanPurchaseFromHere(actor, out var _, out var _))
			{
				if (woodPile.resourceInPile < GetResourceAmount(otherData, job, poiTarget))
				{
					return false;
				}
				return true;
			}
			return false;
		}
		return false;
	}

	public void AfterBuySuccess(ActualGoapNode goapNode)
	{
		TakeWood(goapNode);
		goapNode.actor.moneyComponent.AdjustCoins(-GetBuyCost(goapNode));
	}

	public void AfterTakeSuccess(ActualGoapNode goapNode)
	{
		TakeWood(goapNode);
	}

	private void TakeWood(ActualGoapNode goapNode)
	{
		int resourceAmount = GetResourceAmount(goapNode);
		WoodPile woodPile = goapNode.target as WoodPile;
		if (woodPile.resourceInPile <= resourceAmount)
		{
			goapNode.actor.PickUpItem(woodPile);
			return;
		}
		WoodPile woodPile2 = InnerMapManager.Instance.CreateNewTileObject<WoodPile>(woodPile.tileObjectType);
		woodPile2.SetResourceInPile(resourceAmount);
		goapNode.actor.PickUpItem(woodPile2);
		woodPile.AdjustResourceInPile(-resourceAmount);
	}

	private int GetBuyCost(ActualGoapNode goapNode)
	{
		if (goapNode.otherData != null && goapNode.otherData.Length >= 1)
		{
			return ((IntOtherData)goapNode.otherData[0]).integer;
		}
		return 10;
	}

	private int GetResourceAmount(ActualGoapNode goapNode)
	{
		return GetResourceAmount(goapNode.otherData, goapNode.associatedJob, goapNode.poiTarget);
	}

	private int GetResourceAmount(OtherData[] otherData, JobQueueItem job, IPointOfInterest poiTarget)
	{
		if (otherData != null && otherData.Length >= 2)
		{
			return ((IntOtherData)otherData[1]).integer;
		}
		if (job is GoapPlanJob { goal: not null } goapPlanJob && goapPlanJob.goal.conditionType == GOAP_EFFECT_CONDITION.HAS_POI && poiTarget is ResourcePile resourcePile && Enum.TryParse<TILE_OBJECT_TYPE>(Utilities.NotNormalizedConversionStringToEnum(goapPlanJob.goal.conditionKey.ToUpperInvariant()), out var result))
		{
			return TileObjectDB.GetTileObjectData(result).GetRecipeThatUses(resourcePile.tileObjectType).GetNeededAmountForIngredient(resourcePile.tileObjectType);
		}
		return 10;
	}
}

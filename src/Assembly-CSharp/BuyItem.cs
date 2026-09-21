using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class BuyItem : GoapAction
{
	public BuyItem()
		: base(INTERACTION_TYPE.BUY_ITEM)
	{
		base.actionIconString = GoapActionStateDB.Steal_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Needs };
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_POI, GOAP_EFFECT_TARGET.ACTOR));
	}

	protected override List<GoapEffect> GetExpectedEffects(Character actor, IPointOfInterest target, OtherData[] otherData, out bool isOverridden)
	{
		List<GoapEffect> list = RuinarchListPool<GoapEffect>.Claim(4);
		AddBaseExpectedEffectsToList(list);
		TileObject tileObject = target as TileObject;
		list.Add(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_POI, tileObject.internalName, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
		isOverridden = true;
		return list;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		if (goapNode.poiTarget.gridTileLocation != null && goapNode.poiTarget.gridTileLocation.structure is ManMadeStructure manMadeStructure && manMadeStructure.CanPurchaseFromHere(goapNode.actor, out var needsToPay, out var _))
		{
			SetState(needsToPay ? "Buy Success" : "Take Success", goapNode);
		}
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		if (!goapActionInvalidity.isInvalid && node.poiTarget is TileObject p_tileObject && node.poiTarget.gridTileLocation != null && node.poiTarget.gridTileLocation.structure is ManMadeStructure manMadeStructure)
		{
			if (manMadeStructure.CanPurchaseFromHere(node.actor, out var needsToPay, out var _))
			{
				if (needsToPay && !node.actor.moneyComponent.CanAfford(GetPurchaseCost(p_tileObject)))
				{
					goapActionInvalidity.isInvalid = true;
					goapActionInvalidity.reason = "not_enough_money";
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
		TileObject p_tileObject = node.poiTarget as TileObject;
		log.AddToFillers(null, GetPurchaseCost(p_tileObject).ToString(), LOG_IDENTIFIER.STRING_1);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (poiTarget is TileObject && poiTarget.gridTileLocation != null && poiTarget.gridTileLocation.structure is ManMadeStructure manMadeStructure && manMadeStructure.CanPurchaseFromHere(actor, out var _, out var _))
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public void AfterBuySuccess(ActualGoapNode goapNode)
	{
		TakeItem(goapNode);
		TileObject p_tileObject = goapNode.poiTarget as TileObject;
		goapNode.actor.moneyComponent.AdjustCoins(-GetPurchaseCost(p_tileObject));
	}

	public void AfterTakeSuccess(ActualGoapNode goapNode)
	{
		TakeItem(goapNode);
	}

	private void TakeItem(ActualGoapNode goapNode)
	{
		TileObject item = goapNode.poiTarget as TileObject;
		goapNode.actor.PickUpItem(item);
	}

	private int GetPurchaseCost(TileObject p_tileObject)
	{
		if (p_tileObject is EquipmentItem { equipmentData: var equipmentData } && equipmentData != null)
		{
			return equipmentData.purchaseCost;
		}
		return TileObjectDB.GetTileObjectData(p_tileObject.tileObjectType).purchaseCost;
	}
}

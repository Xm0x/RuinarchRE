using Inner_Maps;
using Inner_Maps.Location_Structures;

public class BuyFood : GoapAction
{
	public const int FoodCost = 10;

	public BuyFood()
		: base(INTERACTION_TYPE.BUY_FOOD)
	{
		base.actionIconString = GoapActionStateDB.Steal_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Needs };
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_POI, "Food Pile", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.BUY_OBJECT, "Food Pile", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.FEED, "Food Pile", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		if (goapNode.poiTarget is FoodPile && goapNode.poiTarget.gridTileLocation != null && goapNode.poiTarget.gridTileLocation.structure is ManMadeStructure manMadeStructure && manMadeStructure.CanPurchaseFromHere(goapNode.actor, out var needsToPay, out var _))
		{
			SetState(needsToPay ? "Buy Success" : "Take Success", goapNode);
		}
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		if (!goapActionInvalidity.isInvalid && node.poiTarget is FoodPile && node.poiTarget.gridTileLocation != null && node.poiTarget.gridTileLocation.structure is ManMadeStructure manMadeStructure)
		{
			if (manMadeStructure.CanPurchaseFromHere(node.actor, out var needsToPay, out var _))
			{
				if (needsToPay && !node.actor.moneyComponent.CanAfford(10))
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

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (poiTarget is FoodPile foodPile && poiTarget.gridTileLocation != null && poiTarget.gridTileLocation.structure is ManMadeStructure manMadeStructure && foodPile.characterOwner == null)
			{
				if (!actor.traitContainer.HasTrait("Cannibal") && (foodPile.tileObjectType == TILE_OBJECT_TYPE.HUMAN_MEAT || foodPile.tileObjectType == TILE_OBJECT_TYPE.ELF_MEAT))
				{
					return false;
				}
				if (manMadeStructure.CanPurchaseFromHere(actor, out var _, out var _))
				{
					if (actor.homeStructure == null)
					{
						return true;
					}
					return !actor.homeStructure.HasBuiltTileObjectOfType(foodPile.tileObjectType);
				}
			}
			return false;
		}
		return false;
	}

	public void PreBuySuccess(ActualGoapNode goapNode)
	{
		goapNode.descriptionLog.AddToFillers(null, 10.ToString(), LOG_IDENTIFIER.STRING_1);
	}

	public void AfterBuySuccess(ActualGoapNode goapNode)
	{
		TakeFood(goapNode);
		goapNode.actor.moneyComponent.AdjustCoins(-10);
	}

	public void AfterTakeSuccess(ActualGoapNode goapNode)
	{
		TakeFood(goapNode);
	}

	private void TakeFood(ActualGoapNode goapNode)
	{
		int num = 60;
		if (goapNode.otherData != null && goapNode.otherData.Length != 0 && goapNode.otherData[0] is IntOtherData intOtherData)
		{
			num = intOtherData.integer;
		}
		FoodPile foodPile = goapNode.target as FoodPile;
		if (foodPile.resourceInPile <= num)
		{
			goapNode.actor.PickUpItem(foodPile);
			Messenger.Broadcast(CharacterSignals.STOP_CURRENT_ACTION_TARGETING_POI_EXCEPT_ACTOR, (TileObject)foodPile, goapNode.actor);
			return;
		}
		FoodPile foodPile2 = InnerMapManager.Instance.CreateNewTileObject<FoodPile>(foodPile.tileObjectType);
		foodPile2.SetResourceInPile(num);
		goapNode.actor.PickUpItem(foodPile2);
		foodPile.AdjustResourceInPile(-num);
	}
}

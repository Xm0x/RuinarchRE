using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class StockpileFood : GoapAction
{
	public StockpileFood()
		: base(INTERACTION_TYPE.STOCKPILE_FOOD)
	{
		base.actionIconString = GoapActionStateDB.Haul_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Needs };
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		SetPrecondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.BUY_OBJECT, "Food Pile", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), HasItemInInventory);
	}

	public override LocationStructure GetTargetStructure(ActualGoapNode node)
	{
		return node.actor.homeStructure;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Stockpile Success", goapNode);
	}

	public override void OnActionStarted(ActualGoapNode node)
	{
		FoodPile item = node.actor.GetItem<FoodPile>();
		if (item != null)
		{
			node.actor.ShowItemVisualCarryingPOI(item);
		}
	}

	public override LocationGridTile GetTargetTileToGoTo(ActualGoapNode goapNode)
	{
		return null;
	}

	private bool HasItemInInventory(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType)
	{
		return actor.HasItem<FoodPile>();
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (actor.homeStructure != null)
			{
				return actor == poiTarget;
			}
			return false;
		}
		return false;
	}

	public void AfterStockpileSuccess(ActualGoapNode goapNode)
	{
		List<FoodPile> list = RuinarchListPool<FoodPile>.Claim();
		goapNode.actor.PopulateItemsOfType(list);
		for (int i = 0; i < list.Count; i++)
		{
			TileObject poi = list[i];
			LocationGridTile locationGridTile = goapNode.actor.gridTileLocation;
			if (locationGridTile != null && locationGridTile.tileObjectComponent.objHere != null)
			{
				locationGridTile = goapNode.actor.gridTileLocation.GetFirstNearestTileFromThisWithNoObject(thisStructureOnly: true);
				if (locationGridTile == null)
				{
					locationGridTile = goapNode.actor.gridTileLocation.GetFirstNearestTileFromThisWithNoObject();
				}
			}
			bool addToLocation = locationGridTile != null;
			goapNode.actor.UncarryPOI(poi, bringBackToInventory: false, addToLocation, locationGridTile);
		}
		RuinarchListPool<FoodPile>.Release(list);
	}
}

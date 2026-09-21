using Inner_Maps;
using UnityEngine;

public class HarvestCrops : GoapAction
{
	public int m_amountProducedPerTick = 3;

	private const float _coinGainMultiplier = 0.344f;

	public HarvestCrops()
		: base(INTERACTION_TYPE.HARVEST_CROPS)
	{
		base.actionIconString = GoapActionStateDB.Harvest_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
		base.shouldAddLogs = false;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Harvest Crops Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		return base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
	}

	public override void OnStopWhilePerforming(ActualGoapNode node)
	{
		base.OnStopWhilePerforming(node);
		if (!node.poiTarget.isBeingSeized && node.ticksPerformingCurrentState > 0)
		{
			ProduceMatsPile(node, pickUpItem: false);
		}
	}

	public void AfterHarvestCropsSuccess(ActualGoapNode p_node)
	{
		ResourcePile resourcePile = ProduceMatsPile(p_node, pickUpItem: true);
		if (resourcePile != null && resourcePile.resourceInPile > 0)
		{
			p_node.actor.jobComponent.TryCreateHaulToWorkplaceJob(resourcePile);
		}
	}

	private ResourcePile ProduceMatsPile(ActualGoapNode p_node, bool pickUpItem)
	{
		TileObject tileObject = p_node.target as TileObject;
		Crops crops = tileObject as Crops;
		int num = p_node.ticksPerformingCurrentState * m_amountProducedPerTick;
		if (crops.count - num < 0)
		{
			num = crops.count;
		}
		if (num <= 0)
		{
			return null;
		}
		crops.count = (int)Mathf.Clamp(crops.count - num, 0f, 1000f);
		if (tileObject.gridTileLocation != null && crops.count <= 0)
		{
			tileObject.gridTileLocation.structure.RemovePOI(tileObject);
		}
		FoodPile foodPile = InnerMapManager.Instance.CreateNewTileObject<FoodPile>(crops.producedObjectOnHarvest);
		p_node.actor.moneyComponent.AdjustCoins(Mathf.CeilToInt((float)num * 0.344f));
		foodPile.SetResourceInPile(num);
		if (pickUpItem)
		{
			p_node.actor.PickUpItem(foodPile, changeCharacterOwnership: false, setOwnership: false);
		}
		else
		{
			LocationGridTile locationGridTile = p_node.actor.gridTileLocation;
			if (locationGridTile != null && locationGridTile.tileObjectComponent.objHere != null)
			{
				locationGridTile = p_node.actor.gridTileLocation.GetFirstNearestTileFromThisWithNoObject();
			}
			locationGridTile?.structure.AddPOI(foodPile, locationGridTile);
		}
		TraitManager.Instance.CopyStatuses(tileObject, foodPile);
		ProduceLogs(p_node, crops);
		p_node.actor.talentComponent?.GetTalent(CHARACTER_TALENT.Food).AdjustExperience(8, p_node.actor);
		return foodPile;
	}

	private void ProduceLogs(ActualGoapNode p_node, Crops pcrops)
	{
		string value = ResourcePile.GetResourceQuantityString(p_node.ticksPerformingCurrentState * m_amountProducedPerTick) + " " + pcrops.producedObjectOnHarvest.LocalizedName();
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", p_node.action.goapName + " produced_resources", LOG_TAG.Work, null);
		log.AddToFillers(p_node.actor, p_node.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddToFillers(null, value, LOG_IDENTIFIER.STRING_1);
		p_node.LogAction(log, ignoreShouldAddLog: true);
	}
}

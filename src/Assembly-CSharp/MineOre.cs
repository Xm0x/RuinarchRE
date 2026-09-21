using Inner_Maps;
using UnityEngine;

public class MineOre : GoapAction
{
	public int m_amountProducedPerTick = 4;

	private const float _coinGainMultiplier = 0.33f;

	public MineOre()
		: base(INTERACTION_TYPE.MINE_ORE)
	{
		base.actionIconString = GoapActionStateDB.Mine_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
		base.shouldAddLogs = false;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Mine Ore Success", goapNode);
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

	public void AfterMineOreSuccess(ActualGoapNode p_node)
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
		Ore ore = tileObject as Ore;
		int num = p_node.ticksPerformingCurrentState * m_amountProducedPerTick;
		if (ore.count - num < 0)
		{
			num = ore.count;
		}
		if (tileObject.gridTileLocation != null && ore.count <= 0)
		{
			tileObject.gridTileLocation.structure.RemovePOI(tileObject);
		}
		if (num <= 0)
		{
			return null;
		}
		ore.count = (int)Mathf.Clamp(ore.count - num, 0f, 1000f);
		MetalPile metalPile = InnerMapManager.Instance.CreateNewTileObject<MetalPile>(ore.providedMetal.ConvertResourcesToTileObjectType());
		p_node.actor.moneyComponent.AdjustCoins(Mathf.CeilToInt((float)num * 0.33f));
		metalPile.SetResourceInPile(num);
		if (pickUpItem)
		{
			p_node.actor.PickUpItem(metalPile, changeCharacterOwnership: false, setOwnership: false);
		}
		else
		{
			LocationGridTile locationGridTile = p_node.actor.gridTileLocation;
			if (locationGridTile != null && locationGridTile.tileObjectComponent.objHere != null)
			{
				locationGridTile = p_node.actor.gridTileLocation.GetFirstNearestTileFromThisWithNoObject();
			}
			locationGridTile?.structure.AddPOI(metalPile, locationGridTile);
		}
		ProduceLogs(p_node);
		p_node.actor.talentComponent?.GetTalent(CHARACTER_TALENT.Resources).AdjustExperience(12, p_node.actor);
		return metalPile;
	}

	private void ProduceLogs(ActualGoapNode p_node)
	{
		Ore ore = p_node.target as Ore;
		string value = ResourcePile.GetResourceQuantityString(p_node.ticksPerformingCurrentState * m_amountProducedPerTick) + " " + ore.providedMetal.ConvertResourcesToTileObjectType().LocalizedName();
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", p_node.action.goapName + " produced_resources", LOG_TAG.Work, null);
		log.AddToFillers(p_node.actor, p_node.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddToFillers(null, value, LOG_IDENTIFIER.STRING_1);
		p_node.LogAction(log, ignoreShouldAddLog: true);
	}
}

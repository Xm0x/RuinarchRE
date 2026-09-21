using Inner_Maps;
using UnityEngine;

public class ChopWood : GoapAction
{
	public int m_amountProducedPerTick = 1;

	private const float _coinGainMultiplier = 0.516f;

	public ChopWood()
		: base(INTERACTION_TYPE.CHOP_WOOD)
	{
		base.actionIconString = GoapActionStateDB.Chop_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
		base.shouldAddLogs = false;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Chop Success", goapNode);
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

	public void AfterChopSuccess(ActualGoapNode p_node)
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
		TreeObject treeObject = tileObject as TreeObject;
		int num = p_node.ticksPerformingCurrentState * m_amountProducedPerTick;
		if (treeObject.count - num < 0)
		{
			num = treeObject.count;
		}
		treeObject.count = (int)Mathf.Clamp(treeObject.count - num, 0f, 1000f);
		if (tileObject.gridTileLocation != null && treeObject.count <= 0)
		{
			tileObject.gridTileLocation.structure.RemovePOI(tileObject);
		}
		if (num <= 0)
		{
			return null;
		}
		WoodPile woodPile = InnerMapManager.Instance.CreateNewTileObject<WoodPile>(TILE_OBJECT_TYPE.WOOD_PILE);
		p_node.actor.moneyComponent.AdjustCoins(Mathf.CeilToInt((float)num * 0.516f));
		woodPile.SetResourceInPile(num);
		if (pickUpItem)
		{
			p_node.actor.PickUpItem(woodPile, changeCharacterOwnership: false, setOwnership: false);
		}
		else
		{
			LocationGridTile locationGridTile = p_node.actor.gridTileLocation;
			if (locationGridTile != null && locationGridTile.tileObjectComponent.objHere != null)
			{
				locationGridTile = p_node.actor.gridTileLocation.GetFirstNearestTileFromThisWithNoObject();
			}
			locationGridTile?.structure.AddPOI(woodPile, locationGridTile);
		}
		ProduceLogs(p_node);
		p_node.actor.talentComponent?.GetTalent(CHARACTER_TALENT.Resources).AdjustExperience(8, p_node.actor);
		return woodPile;
	}

	private void ProduceLogs(ActualGoapNode p_node)
	{
		string value = (p_node.ticksPerformingCurrentState * m_amountProducedPerTick).ToString();
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", p_node.action.goapName + " produced_resources", LOG_TAG.Work, null);
		log.AddToFillers(p_node.actor, p_node.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddToFillers(null, value, LOG_IDENTIFIER.STRING_1);
		p_node.LogAction(log, ignoreShouldAddLog: true);
	}
}

using Inner_Maps;
using UnityEngine;

public class MineStone : GoapAction
{
	public int m_amountProducedPerTick = 1;

	private const float _coinGainMultiplier = 0.206f;

	public MineStone()
		: base(INTERACTION_TYPE.MINE_STONE)
	{
		base.actionIconString = GoapActionStateDB.Mine_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
		base.shouldAddLogs = false;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Mine Success", goapNode);
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

	public void AfterMineSuccess(ActualGoapNode p_node)
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
		Rock rock = tileObject as Rock;
		int num = p_node.ticksPerformingCurrentState * m_amountProducedPerTick;
		if (rock.count - num < 0)
		{
			num = rock.count;
		}
		if (tileObject.gridTileLocation != null && rock.count <= 0)
		{
			tileObject.gridTileLocation.structure.RemovePOI(tileObject);
		}
		if (num <= 0)
		{
			return null;
		}
		rock.count = (int)Mathf.Clamp(rock.count - num, 0f, 1000f);
		StonePile stonePile = InnerMapManager.Instance.CreateNewTileObject<StonePile>(TILE_OBJECT_TYPE.STONE_PILE);
		p_node.actor.moneyComponent.AdjustCoins(Mathf.CeilToInt((float)num * 0.206f));
		stonePile.SetResourceInPile(num);
		if (pickUpItem)
		{
			p_node.actor.PickUpItem(stonePile, changeCharacterOwnership: false, setOwnership: false);
		}
		else
		{
			LocationGridTile locationGridTile = p_node.actor.gridTileLocation;
			if (locationGridTile != null && locationGridTile.tileObjectComponent.objHere != null)
			{
				locationGridTile = p_node.actor.gridTileLocation.GetFirstNearestTileFromThisWithNoObject();
			}
			locationGridTile?.structure.AddPOI(stonePile, locationGridTile);
		}
		ProduceLogs(p_node);
		p_node.actor.talentComponent?.GetTalent(CHARACTER_TALENT.Resources).AdjustExperience(8, p_node.actor);
		return stonePile;
	}

	private void ProduceLogs(ActualGoapNode p_node)
	{
		string value = (p_node.ticksPerformingCurrentState * m_amountProducedPerTick).ToString();
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", base.goapName + " produced_resources", LOG_TAG.Work, null);
		log.AddToFillers(p_node.actor, p_node.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddToFillers(null, value, LOG_IDENTIFIER.STRING_1);
		p_node.LogAction(log, ignoreShouldAddLog: true);
	}
}

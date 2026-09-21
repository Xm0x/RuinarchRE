using Inner_Maps;
using UnityEngine;

public class FindFish : GoapAction
{
	public int m_amountProducedPerTick = 20;

	private const float _coinGainMultiplier = 0.516f;

	public FindFish()
		: base(INTERACTION_TYPE.FIND_FISH)
	{
		base.actionIconString = GoapActionStateDB.Fish_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
		base.shouldAddLogs = false;
		base.canBePerformedEvenIfPathImpossible = true;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		goapNode.actor.jobComponent.fishPile = null;
		goapNode.actor.jobComponent.producedFish = 0;
		SetState("Find Fish Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		string stateName = "Target Missing";
		bool isInvalid = IsTargetMissingOverride(node);
		GoapActionInvalidity invalidity = node.invalidity;
		invalidity.isInvalid = isInvalid;
		invalidity.stateName = stateName;
		invalidity.reason = "target_unavailable";
		return invalidity;
	}

	private bool IsTargetMissingOverride(ActualGoapNode node)
	{
		Character actor = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		if ((!poiTarget.IsAvailable() && !base.canBeAdvertisedEvenIfTargetIsUnavailable) || poiTarget.gridTileLocation == null)
		{
			return true;
		}
		if (base.actionLocationType != ACTION_LOCATION_TYPE.IN_PLACE && actor.currentRegion != poiTarget.gridTileLocation.structure.region)
		{
			return true;
		}
		_ = poiTarget.gridTileLocation;
		if (base.actionLocationType == ACTION_LOCATION_TYPE.NEAR_TARGET)
		{
			if (actor.gridTileLocation != poiTarget.gridTileLocation && !actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation))
			{
				return true;
			}
		}
		else if (base.actionLocationType == ACTION_LOCATION_TYPE.NEAR_OTHER_TARGET)
		{
			if (actor.gridTileLocation != node.targetTile && !actor.gridTileLocation.IsNeighbour(node.targetTile, sameStructureOnly: true))
			{
				return true;
			}
		}
		else if ((base.actionLocationType == ACTION_LOCATION_TYPE.NEARBY || base.actionLocationType == ACTION_LOCATION_TYPE.RANDOM_LOCATION || base.actionLocationType == ACTION_LOCATION_TYPE.RANDOM_LOCATION_B || base.actionLocationType == ACTION_LOCATION_TYPE.OVERRIDE) && actor.gridTileLocation != node.targetTile && !actor.gridTileLocation.IsNeighbour(node.targetTile, sameStructureOnly: true))
		{
			return true;
		}
		return false;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		return base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
	}

	public override void OnStopWhilePerforming(ActualGoapNode p_node)
	{
		base.OnStopWhilePerforming(p_node);
		if (p_node.ticksPerformingCurrentState > 0 && p_node.actor.jobComponent.fishPile != null)
		{
			p_node.actor.jobComponent.TryCreateHaulToWorkplaceJob(p_node.actor.jobComponent.fishPile);
		}
		if (p_node.actor.jobComponent.producedFish <= 0)
		{
			ProduceNoneLogs(p_node);
		}
		else
		{
			ProduceLogsPerTick(p_node);
		}
		p_node.actor.jobComponent.producedFish = 0;
		p_node.actor.jobComponent.fishPile = null;
	}

	public void AfterFindFishSuccess(ActualGoapNode p_node)
	{
		p_node.actor.jobQueue.CancelAllJobs(JOB_TYPE.STOCKPILE_FOOD);
		if (p_node.actor.jobComponent.fishPile != null)
		{
			p_node.actor.jobComponent.TryCreateHaulToWorkplaceJob(p_node.actor.jobComponent.fishPile);
		}
		if (p_node.actor.jobComponent.producedFish <= 0)
		{
			ProduceNoneLogs(p_node);
		}
		p_node.actor.jobComponent.producedFish = 0;
		p_node.actor.jobComponent.fishPile = null;
	}

	public void PerTickFindFishSuccess(ActualGoapNode p_node)
	{
		p_node.actor.jobComponent.fishPile = null;
		int countOfNeighboursThatHasTileObjectOfType = p_node.actor.gridTileLocation.GetCountOfNeighboursThatHasTileObjectOfType(TILE_OBJECT_TYPE.FISH_PILE);
		if (ChanceData.RollChance(CHANCE_TYPE.Find_Fish) && (countOfNeighboursThatHasTileObjectOfType > 0 || p_node.actor.gridTileLocation.GetFirstNeighborThatIsPassableAndNoObject() != null))
		{
			p_node.actor.jobComponent.producedFish += m_amountProducedPerTick;
			ProduceMatsPile(p_node);
			ProduceLogsPerTick(p_node);
		}
	}

	private ResourcePile ProduceMatsPile(ActualGoapNode goapNode)
	{
		if (goapNode.actor.jobComponent.fishPile == null)
		{
			if (goapNode.actor.gridTileLocation.GetCountOfNeighboursThatHasTileObjectOfType(TILE_OBJECT_TYPE.FISH_PILE) > 0)
			{
				for (int i = 0; i < goapNode.actor.gridTileLocation.neighbourList.Count; i++)
				{
					TileObject objHere = goapNode.actor.gridTileLocation.neighbourList[i].tileObjectComponent.objHere;
					if (objHere != null && objHere.tileObjectType == TILE_OBJECT_TYPE.FISH_PILE)
					{
						goapNode.actor.jobComponent.fishPile = goapNode.actor.gridTileLocation.neighbourList[i].tileObjectComponent.objHere as FishPile;
						break;
					}
				}
				goapNode.actor.jobComponent.fishPile.AdjustResourceInPile(m_amountProducedPerTick);
				goapNode.actor.moneyComponent.AdjustCoins(Mathf.CeilToInt((float)m_amountProducedPerTick * 0.516f));
			}
			else
			{
				LocationGridTile firstNeighborThatIsPassableAndNoObject = goapNode.actor.gridTileLocation.GetFirstNeighborThatIsPassableAndNoObject();
				if (firstNeighborThatIsPassableAndNoObject != null && firstNeighborThatIsPassableAndNoObject.tileObjectComponent.objHere != null)
				{
					firstNeighborThatIsPassableAndNoObject = goapNode.actor.gridTileLocation.GetFirstNeighborThatIsPassableAndNoObject();
				}
				goapNode.actor.jobComponent.fishPile = InnerMapManager.Instance.CreateNewTileObject<FishPile>(TILE_OBJECT_TYPE.FISH_PILE);
				firstNeighborThatIsPassableAndNoObject.structure.AddPOI(goapNode.actor.jobComponent.fishPile, firstNeighborThatIsPassableAndNoObject);
				goapNode.actor.talentComponent?.GetTalent(CHARACTER_TALENT.Food).AdjustExperience(4, goapNode.actor);
				goapNode.actor.jobComponent.fishPile.SetResourceInPile(m_amountProducedPerTick);
				goapNode.actor.moneyComponent.AdjustCoins(Mathf.CeilToInt((float)m_amountProducedPerTick * 0.516f));
			}
		}
		return goapNode.actor.jobComponent.fishPile;
	}

	private void ProduceNoneLogs(ActualGoapNode p_node)
	{
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", p_node.action.goapName + " none_produced", LOG_TAG.Work, null);
		log.AddToFillers(p_node.actor, p_node.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		p_node.LogAction(log, ignoreShouldAddLog: true);
	}

	private void ProduceLogsPerTick(ActualGoapNode p_node)
	{
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", p_node.action.goapName + " produced_resources_per_tick", LOG_TAG.Work, null);
		log.AddToFillers(p_node.actor, p_node.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		p_node.LogAction(log, ignoreShouldAddLog: true);
	}
}

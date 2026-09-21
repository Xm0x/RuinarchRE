using Inner_Maps;
using UtilityScripts;

public class Fish : GoapAction
{
	public Fish()
		: base(INTERACTION_TYPE.FISH)
	{
		base.actionIconString = GoapActionStateDB.Fish_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.PRODUCE_FOOD, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Fish Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		if (job.jobType == JOB_TYPE.PRODUCE_FOOD_FOR_CAMP && target.gridTileLocation != null && actor.gridTileLocation != null)
		{
			LocationGridTile centerGridTile = target.gridTileLocation.area.gridTileComponent.centerGridTile;
			float distanceTo = actor.gridTileLocation.area.gridTileComponent.centerGridTile.GetDistanceTo(centerGridTile);
			int num = InnerMapManager.AreaLocationGridTileSize.x * 3;
			if (distanceTo > (float)num)
			{
				return 2000;
			}
		}
		return Utilities.Rng.Next(80, 101);
	}

	public override bool IsHappinessRecoveryAction()
	{
		return true;
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
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (poiTarget is FishingSpot fishingSpot)
			{
				if (actor.homeSettlement != null && fishingSpot.connectedFishingShack != null && fishingSpot.connectedFishingShack.settlementLocation == actor.homeSettlement && poiTarget.IsAvailable())
				{
					return poiTarget.gridTileLocation != null;
				}
				return false;
			}
			if (poiTarget.IsAvailable())
			{
				return poiTarget.gridTileLocation != null;
			}
			return false;
		}
		return false;
	}

	public void PreFishSuccess(ActualGoapNode goapNode)
	{
		goapNode.descriptionLog.AddToFillers(null, "50", LOG_IDENTIFIER.STRING_1);
	}

	public void PerTickFishSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.actor.characterClass.IsCombatant())
		{
			goapNode.actor.needsComponent.AdjustHappiness(-4f);
		}
	}

	public void AfterFishSuccess(ActualGoapNode goapNode)
	{
		LocationGridTile locationGridTile = goapNode.actor.gridTileLocation;
		if (locationGridTile != null && locationGridTile.tileObjectComponent.objHere != null)
		{
			locationGridTile = goapNode.actor.gridTileLocation.GetFirstNearestTileFromThisWithNoObject();
		}
		if (locationGridTile != null)
		{
			if (goapNode.associatedJobType == JOB_TYPE.PRODUCE_FOOD_FOR_CAMP)
			{
				FoodPile foodPile = InnerMapManager.Instance.CreateNewTileObject<FoodPile>(TILE_OBJECT_TYPE.FISH_PILE);
				foodPile.SetResourceInPile(50);
				locationGridTile.structure.AddPOI(foodPile, locationGridTile);
				if (goapNode.actor.partyComponent.hasParty && goapNode.actor.partyComponent.currentParty.targetCamp != null)
				{
					goapNode.actor.partyComponent.currentParty.jobComponent.CreateHaulForCampJob(foodPile, goapNode.actor.partyComponent.currentParty.targetCamp);
					goapNode.actor.marker.AddPOIAsInVisionRange(foodPile);
				}
			}
			else
			{
				InnerMapManager.Instance.CreateNewResourcePileAndTryCreateHaulJob<FoodPile>(TILE_OBJECT_TYPE.FISH_PILE, 50, goapNode.actor, locationGridTile);
			}
		}
		goapNode.actor.talentComponent?.GetTalent(CHARACTER_TALENT.Food).AdjustExperience(8, goapNode.actor);
	}
}

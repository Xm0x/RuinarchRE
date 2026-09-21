using System.Collections.Generic;
using Inner_Maps;
using UtilityScripts;

public class HarvestPlant : GoapAction
{
	public HarvestPlant()
		: base(INTERACTION_TYPE.HARVEST_PLANT)
	{
		base.actionIconString = GoapActionStateDB.Harvest_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.PRODUCE_FOOD, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Harvest Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		if (actor.movementComponent.ShouldAvoidStructureLocationOfTarget(target))
		{
			return 2000;
		}
		if (target.gridTileLocation != null && target.gridTileLocation.IsPartOfSettlement(out var settlement) && settlement.owner != null && actor.homeSettlement != settlement)
		{
			return 2000;
		}
		if (job.jobType == JOB_TYPE.PRODUCE_FOOD_FOR_CAMP && target.gridTileLocation != null && actor.gridTileLocation != null)
		{
			LocationGridTile centerGridTile = target.gridTileLocation.area.gridTileComponent.centerGridTile;
			float distanceTo = actor.areaLocation.gridTileComponent.centerGridTile.GetDistanceTo(centerGridTile);
			int num = InnerMapManager.AreaLocationGridTileSize.x * 3;
			if (distanceTo > (float)num)
			{
				return 2000;
			}
		}
		return Utilities.Rng.Next(40, 51);
	}

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		log.AddToFillers(null, GetTargetString(node.poiTarget), LOG_IDENTIFIER.STRING_2);
	}

	public override bool IsHappinessRecoveryAction()
	{
		return true;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (poiTarget is Crops { currentGrowthState: not Crops.Growth_State.Ripe })
			{
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

	public void PreHarvestSuccess(ActualGoapNode goapNode)
	{
		goapNode.descriptionLog.AddToFillers(null, "30", LOG_IDENTIFIER.STRING_1);
	}

	public void PerTickHarvestSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.actor.characterClass.IsCombatant())
		{
			goapNode.actor.needsComponent.AdjustHappiness(-4f);
		}
	}

	public void AfterHarvestSuccess(ActualGoapNode goapNode)
	{
		IPointOfInterest poiTarget = goapNode.poiTarget;
		if (!(poiTarget is Crops crops))
		{
			return;
		}
		crops.SetGrowthState(Crops.Growth_State.Growing);
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		poiTarget.gridTileLocation.PopulateTilesInRadius(list, 1, 0, includeCenterTile: false, includeTilesInDifferentStructure: true, includeImpassable: false);
		if (list.Count > 0)
		{
			FoodPile foodPile = CharacterManager.Instance.CreateFoodPileForPOI(poiTarget, CollectionUtilities.GetRandomElement(list));
			if (goapNode.associatedJobType == JOB_TYPE.PRODUCE_FOOD_FOR_CAMP)
			{
				if (goapNode.actor.partyComponent.hasParty && goapNode.actor.partyComponent.currentParty.targetCamp != null)
				{
					goapNode.actor.partyComponent.currentParty.jobComponent.CreateHaulForCampJob(foodPile, goapNode.actor.partyComponent.currentParty.targetCamp);
					goapNode.actor.marker.AddPOIAsInVisionRange(foodPile);
				}
			}
			else if (foodPile != null && goapNode.actor.homeSettlement != null)
			{
				goapNode.actor.homeSettlement.settlementJobTriggerComponent.TryCreateHaulJob(foodPile);
				goapNode.actor.marker.AddPOIAsInVisionRange(foodPile);
			}
		}
		RuinarchListPool<LocationGridTile>.Release(list);
	}

	private string GetTargetString(IPointOfInterest poi)
	{
		if (poi is BerryShrub)
		{
			return LocalizationManager.Instance.GetLocalizedValue("TileObjects_Table", "Vegetables");
		}
		if (poi is CornCrop)
		{
			return LocalizationManager.Instance.GetLocalizedValue("TileObjects_Table", "Corn");
		}
		return poi.name;
	}
}

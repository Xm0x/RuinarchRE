using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;

public class TakeResource : GoapAction
{
	public TakeResource()
		: base(INTERACTION_TYPE.TAKE_RESOURCE)
	{
		base.actionIconString = GoapActionStateDB.Haul_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.TAKE_POI, GOAP_EFFECT_TARGET.ACTOR));
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.FEED, "Food Pile", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
	}

	protected override List<GoapEffect> GetExpectedEffects(Character actor, IPointOfInterest target, OtherData[] otherData, out bool isOverridden)
	{
		if (target is ResourcePile)
		{
			List<GoapEffect> list = RuinarchListPool<GoapEffect>.Claim(4);
			AddBaseExpectedEffectsToList(list);
			ResourcePile resourcePile = target as ResourcePile;
			list.Add(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.TAKE_POI, resourcePile.internalName, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
			isOverridden = true;
			return list;
		}
		return base.GetExpectedEffects(actor, target, otherData, out isOverridden);
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Take Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		int num = 0;
		if (actor.movementComponent.ShouldAvoidStructureLocationOfTarget(target) && !actor.partyComponent.hasParty)
		{
			return 2000;
		}
		if (target.gridTileLocation != null && target.gridTileLocation.IsPartOfSettlement(out var settlement) && settlement.owner != null && actor.homeSettlement != settlement)
		{
			return 2000;
		}
		if (job.jobType == JOB_TYPE.CRAFT_MISSING_FURNITURE && target is ResourcePile resourcePile && resourcePile.characterOwner == actor)
		{
			int neededResource = GetNeededResource(job, otherData, resourcePile, actor);
			if (resourcePile.resourceInPile >= neededResource)
			{
				return 1;
			}
		}
		if (job.jobType.IsFullnessRecoveryTypeJob() || job.jobType == JOB_TYPE.OBTAIN_PERSONAL_FOOD)
		{
			if (target is ElfMeat || target is HumanMeat)
			{
				if (actor.traitContainer.HasTrait("Cannibal") && !actor.traitContainer.HasTrait("Vampire"))
				{
					int num2 = 450;
					num += num2;
				}
				else if (actor.needsComponent.isStarving)
				{
					int num3 = 700;
					num += num3;
				}
				else
				{
					num += 2000;
				}
			}
			else
			{
				num = ((actor.homeStructure == null || target.gridTileLocation == null || target.gridTileLocation.structure == actor.homeStructure) ? 400 : 2000);
			}
		}
		else
		{
			num = ((target.gridTileLocation == null || !target.gridTileLocation.IsPartOfSettlement(out var settlement2) || settlement2.locationType != LOCATION_TYPE.VILLAGE || settlement2 == actor.homeSettlement) ? 400 : 2000);
			if (target is ResourcePile resourcePile2)
			{
				if (job.jobType == JOB_TYPE.BUILD_BLUEPRINT || job.jobType == JOB_TYPE.HAUL || job.jobType == JOB_TYPE.CRAFT_OBJECT || job.jobType == JOB_TYPE.OBTAIN_PERSONAL_ITEM || job.jobType == JOB_TYPE.CREATE_WARD_LIGHT)
				{
					if (resourcePile2.characterOwner != null && resourcePile2.characterOwner != actor)
					{
						num = 2000;
					}
					else if (actor.homeSettlement != null)
					{
						int neededResource2 = GetNeededResource(job, otherData, resourcePile2, actor);
						if (resourcePile2.resourceInPile < neededResource2 && !actor.homeSettlement.settlementJobTriggerComponent.HasTotalResource(resourcePile2.providedResource, neededResource2))
						{
							num = 2000;
						}
					}
				}
				else if (job.jobType == JOB_TYPE.CRAFT_MISSING_FURNITURE || job.jobType == JOB_TYPE.CRAFT_EQUIPMENT)
				{
					if (resourcePile2.characterOwner != null && resourcePile2.characterOwner != actor)
					{
						num = 2000;
					}
					else if (actor.homeSettlement != null)
					{
						int neededResource3 = GetNeededResource(job, otherData, resourcePile2, actor);
						if (resourcePile2.resourceInPile < neededResource3)
						{
							num = 2000;
						}
					}
				}
			}
		}
		return num;
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		IPointOfInterest poiTarget = node.poiTarget;
		if (!goapActionInvalidity.isInvalid)
		{
			ResourcePile resourcePile = poiTarget as ResourcePile;
			int neededResource = GetNeededResource(node.associatedJob, node.otherData, resourcePile, node.actor);
			if (node.associatedJobType == JOB_TYPE.BUILD_BLUEPRINT && resourcePile.resourceInPile < neededResource)
			{
				goapActionInvalidity.isInvalid = true;
				goapActionInvalidity.reason = "not_enough_resources";
			}
			else if (resourcePile.resourceInPile <= 0)
			{
				goapActionInvalidity.isInvalid = true;
				goapActionInvalidity.stateName = "Take Fail";
			}
			else if (node.associatedJobType == JOB_TYPE.HAUL && resourcePile.resourceInPile < neededResource)
			{
				goapActionInvalidity.isInvalid = true;
				goapActionInvalidity.reason = "not_enough_resources";
			}
		}
		return goapActionInvalidity;
	}

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		ResourcePile resourcePile = node.poiTarget as ResourcePile;
		log.AddToFillers(null, resourcePile.providedResource.LocalizedName(), LOG_IDENTIFIER.STRING_2);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (job.jobType == JOB_TYPE.FEED)
			{
				if (!(poiTarget is FoodPile))
				{
					return false;
				}
				if (poiTarget.gridTileLocation == null || poiTarget.gridTileLocation.structure != actor.homeStructure)
				{
					return false;
				}
			}
			if (poiTarget.gridTileLocation == null && poiTarget.isBeingCarriedBy != actor)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public void PreTakeSuccess(ActualGoapNode goapNode)
	{
		ResourcePile resourcePile = goapNode.poiTarget as ResourcePile;
		int num = GetNeededResource(goapNode.associatedJob, goapNode.otherData, resourcePile, goapNode.actor);
		if (num > resourcePile.resourceInPile)
		{
			num = resourcePile.resourceInPile;
		}
		goapNode.descriptionLog.AddToFillers(null, ResourcePile.GetResourceQuantityString(num), LOG_IDENTIFIER.STRING_1);
	}

	public void AfterTakeSuccess(ActualGoapNode goapNode)
	{
		ResourcePile resourcePile = goapNode.poiTarget as ResourcePile;
		int num = GetNeededResource(goapNode.associatedJob, goapNode.otherData, resourcePile, goapNode.actor);
		if (num > resourcePile.resourceInPile)
		{
			num = resourcePile.resourceInPile;
		}
		goapNode.actor.UncarryPOI(bringBackToInventory: true);
		bool setOwnership = goapNode.associatedJobType != JOB_TYPE.HAUL && goapNode.associatedJobType != JOB_TYPE.FULLNESS_RECOVERY_NORMAL && goapNode.associatedJobType != JOB_TYPE.FULLNESS_RECOVERY_URGENT && goapNode.associatedJobType != JOB_TYPE.OBTAIN_PERSONAL_FOOD && goapNode.associatedJobType != JOB_TYPE.BUILD_BLUEPRINT && goapNode.associatedJobType != JOB_TYPE.CRAFT_EQUIPMENT && goapNode.associatedJobType != JOB_TYPE.REPAIR && goapNode.associatedJobType != JOB_TYPE.CREATE_WARD_LIGHT;
		CarryResourcePile(goapNode.actor, resourcePile, num, setOwnership);
	}

	private void CarryResourcePile(Character carrier, ResourcePile pile, int amount, bool setOwnership)
	{
		if (pile.isBeingCarriedBy == null || pile.isBeingCarriedBy != carrier)
		{
			if (pile.resourceInPile > amount)
			{
				ResourcePile resourcePile = InnerMapManager.Instance.CreateNewTileObject<ResourcePile>(pile.tileObjectType);
				resourcePile.SetResourceInPile(amount);
				resourcePile.SetGridTileLocation(pile.gridTileLocation);
				resourcePile.InitializeMapObject(resourcePile);
				resourcePile.SetPOIState(POI_STATE.ACTIVE);
				LocationAwarenessUtility.AddToAwarenessList(resourcePile, resourcePile.gridTileLocation);
				resourcePile.SetGridTileLocation(null);
				carrier.CarryPOI(resourcePile, changeOwnership: false, setOwnership);
				carrier.ShowItemVisualCarryingPOI(resourcePile);
				TraitManager.Instance.CopyStatuses(pile, resourcePile);
				pile.AdjustResourceInPile(-amount);
			}
			else
			{
				carrier.CarryPOI(pile, changeOwnership: false, setOwnership);
				carrier.ShowItemVisualCarryingPOI(pile);
			}
		}
		else
		{
			carrier.ShowItemVisualCarryingPOI(pile);
		}
	}

	private int GetNeededResource(JobQueueItem jobQueueItem, OtherData[] otherData, ResourcePile resourcePile, Character actor)
	{
		if (otherData != null && otherData.Length == 1)
		{
			OtherData otherData2 = otherData[0];
			if (!(otherData2 is IntOtherData { integer: var integer }))
			{
				if (otherData2 is TileObjectRecipeOtherData { recipe: var recipe })
				{
					return recipe.GetNeededAmountForIngredient(resourcePile.tileObjectType);
				}
				return 10;
			}
			return integer;
		}
		if (jobQueueItem is GoapPlanJob goapPlanJob)
		{
			if (goapPlanJob.jobType == JOB_TYPE.DARK_RITUAL || goapPlanJob.jobType == JOB_TYPE.PREACH || goapPlanJob.jobType == JOB_TYPE.CULTIST_INSTRUCTION)
			{
				return TileObjectDB.GetTileObjectData(TILE_OBJECT_TYPE.CULTIST_KIT).GetRecipeThatUses(resourcePile.tileObjectType).GetNeededAmountForIngredient(resourcePile.tileObjectType);
			}
			if (goapPlanJob.targetPOI is TileObject tileObject && !goapPlanJob.jobType.IsFullnessRecoveryTypeJob())
			{
				TileObjectData tileObjectData = TileObjectDB.GetTileObjectData(tileObject.tileObjectType);
				if (tileObjectData != null && tileObjectData.craftRecipes != null)
				{
					return tileObjectData.GetRecipeThatUses(resourcePile.tileObjectType).GetNeededAmountForIngredient(resourcePile.tileObjectType);
				}
				return Mathf.Min(20, resourcePile.resourceInPile);
			}
			return Mathf.Min(20, resourcePile.resourceInPile);
		}
		return Mathf.Min(20, resourcePile.resourceInPile);
	}
}

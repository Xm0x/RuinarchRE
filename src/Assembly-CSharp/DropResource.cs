using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class DropResource : GoapAction
{
	private Precondition _foodPrecondition;

	private Precondition _buyFoodPrecondition;

	public DropResource()
		: base(INTERACTION_TYPE.DROP_RESOURCE)
	{
		base.actionIconString = GoapActionStateDB.Haul_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
		_foodPrecondition = new Precondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.TAKE_POI, "Food Pile", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), HasTakenEnoughFood);
		_buyFoodPrecondition = new Precondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.BUY_OBJECT, "Food Pile", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), HasBoughtFood);
	}

	protected override List<GoapEffect> GetExpectedEffects(Character actor, IPointOfInterest target, OtherData[] otherData, out bool isOverridden)
	{
		List<GoapEffect> list = RuinarchListPool<GoapEffect>.Claim(4);
		AddBaseExpectedEffectsToList(list);
		if (target is Table)
		{
			list.Add(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_POI, "Food Pile", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
		}
		else if (target is TileObject tileObject)
		{
			list.Add(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_POI, tileObject.internalName, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
		}
		else
		{
			list.Add(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_POI, target.name, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
		}
		isOverridden = true;
		return list;
	}

	public override Precondition GetPrecondition(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType, out bool isOverridden)
	{
		Precondition precondition = null;
		precondition = ((target is Table) ? ((jobType != JOB_TYPE.BUY_FOOD_FOR_TAVERN) ? _foodPrecondition : _buyFoodPrecondition) : ((!(target is TileObject tileObject)) ? new Precondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.TAKE_POI, target.name, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), HasTakenEnoughAmount) : new Precondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.TAKE_POI, tileObject.internalName, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), HasTakenEnoughAmount)));
		isOverridden = true;
		return precondition;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Drop Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		ResourcePile resourcePile = null;
		if (node.otherData != null && node.otherData.Length == 1)
		{
			resourcePile = node.otherData[0].obj as ResourcePile;
		}
		else if (node.poiTarget is Table)
		{
			resourcePile = node.actor.carryComponent.carriedPOI as FoodPile;
			if (resourcePile == null)
			{
				resourcePile = node.actor.GetItem<FoodPile>();
			}
		}
		else
		{
			resourcePile = node.actor.carryComponent.carriedPOI as ResourcePile;
			if (resourcePile == null)
			{
				resourcePile = node.actor.GetItem<ResourcePile>();
			}
		}
		if (resourcePile != null)
		{
			log.AddToFillers(null, resourcePile.providedResource.LocalizedName(), LOG_IDENTIFIER.STRING_2);
		}
	}

	public override void OnActionStarted(ActualGoapNode node)
	{
		base.OnActionStarted(node);
		if (node.associatedJobType == JOB_TYPE.BUY_FOOD_FOR_TAVERN)
		{
			FoodPile item = node.actor.GetItem<FoodPile>();
			node.actor.ShowItemVisualCarryingPOI(item);
			return;
		}
		TileObject tileObject = null;
		if (node.poiTarget is Table)
		{
			tileObject = node.actor.carryComponent.carriedPOI as FoodPile;
			if (tileObject == null)
			{
				tileObject = node.actor.GetItem<FoodPile>();
			}
		}
		else
		{
			tileObject = node.actor.carryComponent.carriedPOI as ResourcePile;
			if (tileObject == null)
			{
				tileObject = node.actor.GetItem<ResourcePile>();
			}
		}
		if (node.actor.carryComponent.carriedPOI != tileObject)
		{
			node.actor.UncarryPOI();
			node.actor.ShowItemVisualCarryingPOI(tileObject);
		}
	}

	public override void OnStopWhileStarted(ActualGoapNode node)
	{
		base.OnStopWhileStarted(node);
		node.actor.UncarryPOI();
	}

	public override void OnStopWhilePerforming(ActualGoapNode node)
	{
		base.OnStopWhilePerforming(node);
		node.actor.UncarryPOI();
	}

	private bool HasTakenEnoughAmount(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType)
	{
		if (actor.carryComponent.isCarryingAnyPOI && actor.carryComponent.carriedPOI is ResourcePile)
		{
			return true;
		}
		if (actor.items.Count > 0)
		{
			for (int i = 0; i < actor.items.Count; i++)
			{
				if (actor.items[i] is ResourcePile)
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool HasTakenEnoughFood(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType)
	{
		if (actor.carryComponent.isCarryingAnyPOI && actor.carryComponent.carriedPOI is FoodPile)
		{
			return true;
		}
		if (actor.items.Count > 0)
		{
			for (int i = 0; i < actor.items.Count; i++)
			{
				if (actor.items[i] is FoodPile)
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool HasBoughtFood(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType)
	{
		if (actor.HasItem<FoodPile>())
		{
			return true;
		}
		return false;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (job.jobType.IsFullnessRecoveryTypeJob())
			{
				LocationStructure locationStructure = poiTarget.gridTileLocation?.structure;
				if (locationStructure != null)
				{
					if (locationStructure is Dwelling)
					{
						if (!locationStructure.IsResident(actor))
						{
							return false;
						}
					}
					else if (locationStructure.structureType.IsFoodProducingStructure() && locationStructure is ManMadeStructure manMadeStructure && !manMadeStructure.DoesCharacterWorkHere(actor))
					{
						return false;
					}
				}
			}
			if (poiTarget.gridTileLocation == null)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public void PreDropSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.actor.carryComponent.carriedPOI is ResourcePile resourcePile)
		{
			goapNode.descriptionLog.AddToFillers(null, resourcePile.strResourcesInPile, LOG_IDENTIFIER.STRING_1);
		}
	}

	public void AfterDropSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.actor.carryComponent.carriedPOI is ResourcePile resourcePile)
		{
			if (goapNode.poiTarget is Table table)
			{
				table.AdjustFood(resourcePile.specificProvidedResource, resourcePile.resourceInPile);
			}
			else if (goapNode.poiTarget is ResourcePile resourcePile2)
			{
				resourcePile2.AdjustResourceInPile(resourcePile.resourceInPile);
			}
			resourcePile.traitContainer.RemoveStatusAndStacks(resourcePile, "Burnt");
			TraitManager.Instance.CopyStatuses(resourcePile, goapNode.poiTarget);
			resourcePile.AdjustResourceInPile(-resourcePile.resourceInPile);
		}
	}
}

using Inner_Maps;
using UnityEngine;

public class BuildBlueprint : GoapAction
{
	private Precondition _stonePrecondition;

	private Precondition _woodPrecondition;

	private Precondition _metalPrecondition;

	public BuildBlueprint()
		: base(INTERACTION_TYPE.BUILD_BLUEPRINT)
	{
		base.actionIconString = GoapActionStateDB.Build_Icon;
		base.showNotification = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
		_stonePrecondition = new Precondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.TAKE_POI, "Stone Pile", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), HasResource);
		_woodPrecondition = new Precondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.TAKE_POI, "Wood Pile", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), HasResource);
		_metalPrecondition = new Precondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.TAKE_POI, "Metal Pile", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), HasResource);
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		if (!goapActionInvalidity.isInvalid && node.poiTarget is GenericTileObject genericTileObject && (genericTileObject.gridTileLocation.tileObjectComponent.IsAffectedByAOESpell(TILE_OBJECT_TYPE.BLIZZARD_TILE_OBJECT) || genericTileObject.gridTileLocation.tileObjectComponent.IsAffectedByAOESpell(TILE_OBJECT_TYPE.HEAT_WAVE_TILE_OBJECT)))
		{
			goapActionInvalidity.isInvalid = true;
			goapActionInvalidity.reason = "bad_weather";
		}
		return goapActionInvalidity;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Build Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override void AddFillersToLog(Log log, ActualGoapNode goapNode)
	{
		base.AddFillersToLog(log, goapNode);
		if (goapNode.poiTarget is GenericTileObject genericTileObject && genericTileObject.blueprintOnTile != null)
		{
			log.AddToFillers(null, genericTileObject.blueprintOnTile.structureType.LocalizedStructureName(), LOG_IDENTIFIER.STRING_1);
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
		Character actor = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		actor.UncarryPOI();
		if (poiTarget is GenericTileObject genericTileObject)
		{
			genericTileObject.CancelBuilding();
		}
	}

	public override int DetermineActionDuration(GoapActionState p_goapActionState, ActualGoapNode p_actualGoapNode)
	{
		if (p_actualGoapNode.poiTarget is GenericTileObject genericTileObject)
		{
			return genericTileObject.constructionComponent.GetRemainingTicksForConstruction();
		}
		return base.DetermineActionDuration(p_goapActionState, p_actualGoapNode);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (poiTarget.gridTileLocation == null)
			{
				return false;
			}
			if (poiTarget is GenericTileObject genericTileObject)
			{
				if (genericTileObject.blueprintOnTile == null)
				{
					return false;
				}
				return actor.homeSettlement != null;
			}
			return false;
		}
		return false;
	}

	public override Precondition GetPrecondition(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType, out bool isOverridden)
	{
		if (target is GenericTileObject genericTileObject && genericTileObject.blueprintOnTile != null)
		{
			Precondition precondition = null;
			if (genericTileObject.blueprintOnTile.craftCost > 0)
			{
				precondition = ((genericTileObject.blueprintOnTile.thinWallResource == WALL_RESOURCE.Divine_Stone) ? ((!actor.homeSettlement.settlementJobTriggerComponent.HasAccessToResource(RESOURCE.WOOD)) ? _stonePrecondition : _woodPrecondition) : ((genericTileObject.blueprintOnTile.thinWallResource == WALL_RESOURCE.Nature_Vines) ? ((!actor.homeSettlement.settlementJobTriggerComponent.HasAccessToResource(RESOURCE.STONE)) ? _woodPrecondition : _stonePrecondition) : (genericTileObject.blueprintOnTile.thinWallResource.GetResourceForWall() switch
				{
					RESOURCE.STONE => _stonePrecondition, 
					RESOURCE.WOOD => _woodPrecondition, 
					RESOURCE.METAL => _metalPrecondition, 
					_ => _woodPrecondition, 
				})));
				isOverridden = true;
				return precondition;
			}
		}
		return base.GetPrecondition(actor, target, otherData, jobType, out isOverridden);
	}

	private bool HasResource(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType)
	{
		if (poiTarget is GenericTileObject genericTileObject && genericTileObject.blueprintOnTile != null)
		{
			if (genericTileObject.hasStartedBuildingBlueprintOnTile)
			{
				return true;
			}
			if (actor.carryComponent.isCarryingAnyPOI && actor.carryComponent.carriedPOI is ResourcePile resourcePile)
			{
				RESOURCE resourceForWall = genericTileObject.blueprintOnTile.thinWallResource.GetResourceForWall();
				if (resourcePile.providedResource == resourceForWall || genericTileObject.blueprintOnTile.thinWallResource == WALL_RESOURCE.Divine_Stone || genericTileObject.blueprintOnTile.thinWallResource == WALL_RESOURCE.Nature_Vines)
				{
					return resourcePile.resourceInPile >= genericTileObject.blueprintOnTile.craftCost;
				}
				return false;
			}
		}
		return false;
	}

	public void PreBuildSuccess(ActualGoapNode goapNode)
	{
		if (!(goapNode.poiTarget is GenericTileObject genericTileObject))
		{
			return;
		}
		if (!genericTileObject.hasStartedBuildingBlueprintOnTile)
		{
			if (goapNode.actor.carryComponent.carriedPOI is ResourcePile resourcePile)
			{
				resourcePile.AdjustResourceInPile(-genericTileObject.blueprintOnTile.craftCost);
				genericTileObject.resourceStorageComponent.AdjustResource(resourcePile.specificProvidedResource, genericTileObject.blueprintOnTile.craftCost);
			}
			genericTileObject.StartBuilding();
		}
		genericTileObject.ResumeBuilding();
		goapNode.descriptionLog.AddToFillers(null, genericTileObject.blueprintOnTile.structureType.LocalizedStructureName(), LOG_IDENTIFIER.STRING_1);
	}

	public void PerTickBuildSuccess(ActualGoapNode goapNode)
	{
		GenericTileObject genericTileObject = goapNode.target as GenericTileObject;
		genericTileObject.constructionComponent.IncreaseConstructionTick(1, genericTileObject);
		RESOURCE resourceForWall = genericTileObject.blueprintOnTile.thinWallResource.GetResourceForWall();
		if (resourceForWall == RESOURCE.WOOD || resourceForWall == RESOURCE.STONE)
		{
			int craftCost = genericTileObject.blueprintOnTile.craftCost;
			int buildBlueprintDuration = GoapActionStateDB.BuildBlueprintDuration;
			int num = Mathf.CeilToInt((float)craftCost / (float)buildBlueprintDuration);
			CONCRETE_RESOURCES p_resource = CONCRETE_RESOURCES.Wood;
			if (resourceForWall == RESOURCE.STONE)
			{
				p_resource = CONCRETE_RESOURCES.Stone;
			}
			genericTileObject.resourceStorageComponent.AdjustResource(p_resource, -num);
		}
		AkSoundEngine.PostEvent("Play_Build_Repair_Structure", goapNode.actor.marker.gameObject);
	}

	public void AfterBuildSuccess(ActualGoapNode goapNode)
	{
		if (!(goapNode.poiTarget is GenericTileObject genericTileObject))
		{
			return;
		}
		LocationGridTile p_usedConnector = (LocationGridTile)goapNode.otherData[0].obj;
		genericTileObject.resourceStorageComponent.ClearAllResources();
		genericTileObject.BuildBlueprintOnTile(goapNode.actor.homeSettlement, p_usedConnector);
		if (!(genericTileObject.blueprintOnTile != null) || genericTileObject.blueprintOnTile.structureType != STRUCTURE_TYPE.DWELLING || goapNode.actor.homeSettlement == null)
		{
			return;
		}
		Character character = null;
		for (int i = 0; i < goapNode.actor.homeSettlement.residents.Count; i++)
		{
			Character character2 = goapNode.actor.homeSettlement.residents[i];
			if ((character2.isFactionLeader || character2.isSettlementRuler || character2.characterClass.className == "Noble") && (character2.homeStructure == null || (character2.homeStructure.structureType != STRUCTURE_TYPE.DWELLING && character2.homeStructure.structureType != STRUCTURE_TYPE.VAMPIRE_CASTLE)))
			{
				character = character2;
				break;
			}
		}
		character?.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, character);
	}
}

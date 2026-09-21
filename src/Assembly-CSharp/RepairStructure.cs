using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class RepairStructure : GoapAction
{
	private Precondition _stonePrecondition;

	private Precondition _woodPrecondition;

	public RepairStructure()
		: base(INTERACTION_TYPE.REPAIR_STRUCTURE)
	{
		base.actionIconString = GoapActionStateDB.Repair_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
		_stonePrecondition = new Precondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.TAKE_POI, "Stone Pile", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), HasResource);
		_woodPrecondition = new Precondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.TAKE_POI, "Wood Pile", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), HasResource);
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
	}

	public override Precondition GetPrecondition(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType, out bool isOverridden)
	{
		Precondition result = null;
		if (actor.homeSettlement != null)
		{
			result = ((!actor.homeSettlement.settlementJobTriggerComponent.HasAccessToResource(RESOURCE.WOOD)) ? _stonePrecondition : _woodPrecondition);
		}
		else if ((target as StructureTileObject).structureParent is ManMadeStructure manMadeStructure)
		{
			result = manMadeStructure.wallsAreMadeOf.GetResourceForWall() switch
			{
				RESOURCE.WOOD => _woodPrecondition, 
				RESOURCE.STONE => _stonePrecondition, 
				_ => _woodPrecondition, 
			};
		}
		isOverridden = true;
		return result;
	}

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		log.AddToFillers(node.poiTarget.gridTileLocation.structure, node.poiTarget.gridTileLocation.structure.GetNameRelativeTo(node.actor), LOG_IDENTIFIER.LANDMARK_1, replaceExisting: true, overrideStringValue: true);
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Repair Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 2;
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
		_ = node.poiTarget;
		actor.UncarryPOI();
	}

	private bool HasResource(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType)
	{
		ManMadeStructure manMadeStructure = (poiTarget as StructureTileObject).structureParent as ManMadeStructure;
		RESOURCE resourceForWall = manMadeStructure.wallsAreMadeOf.GetResourceForWall();
		if (poiTarget.resourceStorageComponent.HasResourceAmount(resourceForWall, manMadeStructure.structureObj.repairCost))
		{
			return true;
		}
		if (actor.carryComponent.carriedPOI is ResourcePile)
		{
			return !(actor.carryComponent.carriedPOI is FoodPile);
		}
		return false;
	}

	public void PreRepairSuccess(ActualGoapNode goapNode)
	{
		goapNode.descriptionLog.AddToFillers(goapNode.poiTarget.gridTileLocation.structure, goapNode.poiTarget.gridTileLocation.structure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1, replaceExisting: true, overrideStringValue: true);
		if (goapNode.actor.carryComponent.carriedPOI != null)
		{
			ResourcePile resourcePile = goapNode.actor.carryComponent.carriedPOI as ResourcePile;
			goapNode.poiTarget.resourceStorageComponent.AdjustResource(resourcePile.specificProvidedResource, resourcePile.resourceInPile);
			resourcePile.AdjustResourceInPile(-resourcePile.resourceInPile);
		}
	}

	public void PerTickRepairSuccess(ActualGoapNode goapNode)
	{
		AkSoundEngine.PostEvent("Play_Build_Repair_Structure", goapNode.actor.marker.gameObject);
	}

	public void AfterRepairSuccess(ActualGoapNode goapNode)
	{
		LocationStructure structure = goapNode.poiTarget.gridTileLocation.structure;
		for (int i = 0; i < structure.tiles.Count; i++)
		{
			LocationGridTile locationGridTile = structure.tiles.ElementAt(i);
			locationGridTile.tileObjectComponent.genericTileObject.AdjustHP(locationGridTile.tileObjectComponent.genericTileObject.maxHP, ELEMENTAL_TYPE.Normal);
			locationGridTile.tileObjectComponent.genericTileObject.traitContainer.RemoveTrait(locationGridTile.tileObjectComponent.genericTileObject, "Burnt");
			for (int j = 0; j < locationGridTile.tileObjectComponent.walls.Count; j++)
			{
				ThinWall thinWall = locationGridTile.tileObjectComponent.walls[j];
				thinWall.traitContainer.RemoveTrait(thinWall, "Burnt");
				thinWall.AdjustHP(thinWall.maxHP, ELEMENTAL_TYPE.Normal);
			}
		}
		if (goapNode.poiTarget is StructureTileObject structureTileObject && structureTileObject.structureParent is ManMadeStructure)
		{
			goapNode.poiTarget.resourceStorageComponent.ClearAllResources();
		}
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest target, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, target, otherData, job))
		{
			if (target is StructureTileObject structureTileObject && (structureTileObject.gridTileLocation == null || structureTileObject.structureParent.hasBeenDestroyed || !(structureTileObject.structureParent is ManMadeStructure)))
			{
				return false;
			}
			return true;
		}
		return false;
	}
}

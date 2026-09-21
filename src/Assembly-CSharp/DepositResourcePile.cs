using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

public class DepositResourcePile : GoapAction
{
	public DepositResourcePile()
		: base(INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE)
	{
		base.actionIconString = GoapActionStateDB.Haul_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_OTHER_TARGET;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.DEPOSIT_RESOURCE, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
	}

	public override Precondition GetPrecondition(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType, out bool isOverridden)
	{
		Precondition result = ((!(target is TileObject tileObject)) ? new Precondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_POI, target.name, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET), IsCarriedOrInInventory) : new Precondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_POI, tileObject.internalName, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET), IsCarriedOrInInventory));
		isOverridden = true;
		return result;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Deposit Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override LocationStructure GetTargetStructure(ActualGoapNode node)
	{
		OtherData[] otherData = node.otherData;
		if (otherData != null && otherData.Length == 1)
		{
			if (otherData[0].obj is IPointOfInterest pointOfInterest)
			{
				if (pointOfInterest.gridTileLocation != null)
				{
					return pointOfInterest.gridTileLocation.structure;
				}
				return node.actor.homeSettlement.mainStorage;
			}
			if (otherData[0].obj is Area area)
			{
				return area.gridTileComponent.centerGridTile.structure;
			}
			if (otherData[0].obj is LocationStructure result)
			{
				return result;
			}
			if (node.actor.homeSettlement != null)
			{
				return node.actor.homeSettlement.mainStorage;
			}
		}
		else if (node.actor.homeSettlement != null)
		{
			return node.actor.homeSettlement.mainStorage;
		}
		return null;
	}

	public override IPointOfInterest GetTargetToGoTo(ActualGoapNode goapNode)
	{
		OtherData[] otherData = goapNode.otherData;
		if (otherData != null && otherData.Length == 1 && otherData[0].obj is IPointOfInterest pointOfInterest)
		{
			if (pointOfInterest.gridTileLocation == null)
			{
				return null;
			}
			return pointOfInterest;
		}
		return null;
	}

	public override LocationGridTile GetTargetTileToGoTo(ActualGoapNode goapNode)
	{
		OtherData[] otherData = goapNode.otherData;
		if (otherData != null && otherData.Length == 1 && otherData[0].obj is Area area)
		{
			LocationGridTile locationGridTile = area.GetRandomPassableTile();
			if (locationGridTile == null)
			{
				locationGridTile = area.gridTileComponent.GetRandomTile();
			}
			return locationGridTile;
		}
		List<LocationGridTile> list = goapNode.targetStructure.unoccupiedTiles.ToList();
		if (list.Count > 0)
		{
			return list[Random.Range(0, list.Count)];
		}
		return null;
	}

	public override void OnStopWhileStarted(ActualGoapNode node)
	{
		base.OnStopWhileStarted(node);
		Character actor = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		if (poiTarget != null)
		{
			actor.UncarryPOI(poiTarget, bringBackToInventory: false, addToLocation: true, actor.gridTileLocation);
		}
	}

	public override void OnStopWhilePerforming(ActualGoapNode node)
	{
		base.OnStopWhilePerforming(node);
		Character actor = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		if (poiTarget != null)
		{
			actor.UncarryPOI(poiTarget, bringBackToInventory: false, addToLocation: true, actor.gridTileLocation);
		}
	}

	public override void OnInvalidAction(ActualGoapNode node)
	{
		base.OnInvalidAction(node);
		Character actor = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		if (poiTarget != null)
		{
			actor.UncarryPOI(poiTarget, bringBackToInventory: false, addToLocation: true, actor.gridTileLocation);
		}
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		_ = node.actor;
		_ = node.poiTarget;
		string stateName = "Target Missing";
		bool isInvalid = IsTargetMissingOverride(node);
		GoapActionInvalidity invalidity = node.invalidity;
		invalidity.isInvalid = isInvalid;
		invalidity.stateName = stateName;
		invalidity.reason = "target_unreachable";
		return invalidity;
	}

	public override void AddFillersToLog(Log log, ActualGoapNode goapNode)
	{
		base.AddFillersToLog(log, goapNode);
		ResourcePile resourcePile = goapNode.poiTarget as ResourcePile;
		log.AddToFillers(null, resourcePile.providedResource.LocalizedName(), LOG_IDENTIFIER.STRING_1);
	}

	public override void OnActionStarted(ActualGoapNode node)
	{
		node.actor.ShowItemVisualCarryingPOI(node.poiTarget as TileObject);
	}

	private bool IsCarriedOrInInventory(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType)
	{
		return actor.IsPOICarriedOrInInventory(poiTarget);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (otherData != null && otherData.Length == 1 && otherData[0].obj is IPointOfInterest { gridTileLocation: null })
			{
				return false;
			}
			if (actor.IsPOICarriedOrInInventory(poiTarget))
			{
				return true;
			}
			if (poiTarget.gridTileLocation == null)
			{
				return false;
			}
			if (poiTarget.gridTileLocation.IsPartOfSettlement() && actor.homeSettlement != null && !actor.homeSettlement.mainStorage.HasUnoccupiedTile())
			{
				return false;
			}
			return actor.homeRegion == poiTarget.gridTileLocation.parentMap.region;
		}
		return false;
	}

	public void PreDepositSuccess(ActualGoapNode goapNode)
	{
		ResourcePile resourcePile = goapNode.poiTarget as ResourcePile;
		goapNode.descriptionLog.AddToFillers(null, resourcePile.strResourcesInPile, LOG_IDENTIFIER.STRING_1);
		goapNode.descriptionLog.AddToFillers(null, resourcePile.providedResource.LocalizedName(), LOG_IDENTIFIER.STRING_2);
	}

	public void AfterDepositSuccess(ActualGoapNode goapNode)
	{
		Character actor = goapNode.actor;
		ResourcePile resourcePile = goapNode.poiTarget as ResourcePile;
		OtherData[] otherData = goapNode.otherData;
		ResourcePile resourcePile2 = null;
		if (otherData != null && otherData.Length == 1 && otherData[0].obj is ResourcePile)
		{
			resourcePile2 = otherData[0].obj as ResourcePile;
		}
		if (resourcePile2 != null && resourcePile2.gridTileLocation == goapNode.targetTile)
		{
			if (resourcePile2.mapObjectState == MAP_OBJECT_STATE.UNBUILT)
			{
				resourcePile2.gridTileLocation.structure.RemovePOI(resourcePile2);
				actor.UncarryPOI(resourcePile, bringBackToInventory: false, addToLocation: true, goapNode.targetTile);
			}
			else if (!resourcePile2.resourceStorageComponent.IsAtMaxResource(resourcePile.providedResource))
			{
				if (resourcePile2.mapObjectState == MAP_OBJECT_STATE.UNBUILT)
				{
					resourcePile2.SetMapObjectState(MAP_OBJECT_STATE.BUILT);
				}
				resourcePile2.AdjustResourceInPile(resourcePile.resourceInPile);
				resourcePile.traitContainer.RemoveStatusAndStacks(resourcePile, "Burnt");
				TraitManager.Instance.CopyStatuses(resourcePile, resourcePile2);
				actor.UncarryPOI(resourcePile, bringBackToInventory: false, addToLocation: false);
				resourcePile.OnPileCombinedToOtherPile();
			}
			else
			{
				actor.UncarryPOI(resourcePile);
			}
		}
		else if (otherData != null && otherData.Length == 1 && otherData[0] is LocationStructureOtherData { locationStructure: var locationStructure })
		{
			LocationGridTile locationGridTile = null;
			if (locationStructure.HasUnoccupiedTile())
			{
				locationGridTile = CollectionUtilities.GetRandomElement(locationStructure.unoccupiedTiles);
			}
			if (locationGridTile != null)
			{
				actor.UncarryPOI(resourcePile, bringBackToInventory: false, addToLocation: true, locationGridTile);
			}
			else
			{
				actor.UncarryPOI(resourcePile, bringBackToInventory: false, addToLocation: false);
			}
			if (goapNode.associatedJobType == JOB_TYPE.HAUL && resourcePile.gridTileLocation != null)
			{
				ResourcePile firstBuiltTileObjectOfType = locationStructure.GetFirstBuiltTileObjectOfType<ResourcePile>(resourcePile.tileObjectType, resourcePile);
				if (firstBuiltTileObjectOfType != null)
				{
					int resourceInPile = resourcePile.resourceInPile;
					firstBuiltTileObjectOfType.AdjustResourceInPile(resourceInPile);
					InnerMapManager.Instance.ShowAreaMapTextPopup($"+{resourceInPile}", firstBuiltTileObjectOfType.worldPosition, Color.green);
					resourcePile.traitContainer.RemoveStatusAndStacks(resourcePile, "Burnt");
					TraitManager.Instance.CopyStatuses(resourcePile, firstBuiltTileObjectOfType);
					resourcePile.gridTileLocation.structure.RemovePOI(resourcePile);
				}
				else if (locationStructure.structureType.IsSpecialStructure() && locationStructure.IsResident(actor))
				{
					resourcePile.SetCharacterOwner(actor);
				}
			}
		}
		else
		{
			actor.UncarryPOI(resourcePile);
		}
		if (goapNode.associatedJobType != JOB_TYPE.STEAL_RAID || !goapNode.actor.partyComponent.hasParty || !goapNode.actor.partyComponent.currentParty.isActive)
		{
			return;
		}
		if (goapNode.actor.partyComponent.currentParty.currentQuest is RaidPartyQuest raidPartyQuest)
		{
			raidPartyQuest.SetIsSuccessful(state: true);
			if (!raidPartyQuest.TryTriggerRetreat(PartyQuest.GetLocalizedEndQuestReason("Raid_Successful")))
			{
				goapNode.actor.partyComponent.currentParty.RemoveMemberThatJoinedQuest(goapNode.actor, broadcastSignal: true, shouldDropQuest: true, shouldGainRewards: true);
			}
		}
		else
		{
			goapNode.actor.partyComponent.currentParty.RemoveMemberThatJoinedQuest(goapNode.actor, broadcastSignal: true, shouldDropQuest: true, shouldGainRewards: true);
		}
	}

	private bool IsTargetMissingOverride(ActualGoapNode node)
	{
		Character actor = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		if (actor.carryComponent.IsPOICarried(poiTarget))
		{
			return false;
		}
		if (!poiTarget.IsAvailable() || poiTarget.gridTileLocation == null || actor.currentRegion != poiTarget.currentRegion)
		{
			return true;
		}
		OtherData[] otherData = node.otherData;
		if (otherData != null && otherData.Length == 1 && otherData[0].obj is IPointOfInterest { gridTileLocation: null })
		{
			return true;
		}
		if (base.actionLocationType == ACTION_LOCATION_TYPE.NEAR_TARGET)
		{
			if (actor.gridTileLocation != poiTarget.gridTileLocation && !actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation, sameStructureOnly: true))
			{
				if (actor.hasMarker && actor.marker.IsCharacterInLineOfSightWith(poiTarget))
				{
					return false;
				}
				return true;
			}
		}
		else if (base.actionLocationType == ACTION_LOCATION_TYPE.NEAR_OTHER_TARGET && actor.gridTileLocation != node.targetTile && !actor.gridTileLocation.IsNeighbour(node.targetTile, sameStructureOnly: true))
		{
			return true;
		}
		return false;
	}
}

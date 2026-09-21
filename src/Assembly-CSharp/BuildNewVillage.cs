using System.Collections.Generic;
using Inner_Maps.Location_Structures;

public class BuildNewVillage : GoapAction
{
	public BuildNewVillage()
		: base(INTERACTION_TYPE.BUILD_NEW_VILLAGE)
	{
		base.actionIconString = GoapActionStateDB.Found_Icon;
		base.showNotification = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Major };
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

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		if (!goapActionInvalidity.isInvalid && node.poiTarget is GenericTileObject genericTileObject)
		{
			string structurePrefabName = (string)node.otherData[0].obj;
			if (!LandmarkManager.Instance.HasEnoughSpaceForStructure(structurePrefabName, genericTileObject.gridTileLocation))
			{
				goapActionInvalidity.isInvalid = true;
				goapActionInvalidity.reason = "no_space_village";
			}
			else if (genericTileObject.allJobsTargetingThis != null)
			{
				for (int i = 0; i < genericTileObject.allJobsTargetingThis.Count; i++)
				{
					JobQueueItem jobQueueItem = genericTileObject.allJobsTargetingThis[i];
					if (jobQueueItem.jobType == JOB_TYPE.FIND_NEW_VILLAGE && jobQueueItem.assignedCharacter != null && jobQueueItem.assignedCharacter != node.actor)
					{
						ActualGoapNode currentActionNode = jobQueueItem.assignedCharacter.currentActionNode;
						if (currentActionNode != null && currentActionNode.associatedJob == jobQueueItem && currentActionNode.isPerformingActualAction)
						{
							goapActionInvalidity.isInvalid = true;
							goapActionInvalidity.reason = "no_space_village";
						}
					}
				}
			}
		}
		return goapActionInvalidity;
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
				if (genericTileObject.blueprintOnTile != null)
				{
					return false;
				}
				if (genericTileObject.gridTileLocation.structure.structureType != STRUCTURE_TYPE.WILDERNESS)
				{
					return false;
				}
				return true;
			}
			return false;
		}
		return false;
	}

	public void AfterBuildSuccess(ActualGoapNode goapNode)
	{
		if (!(goapNode.poiTarget is GenericTileObject genericTileObject))
		{
			return;
		}
		string text = (string)goapNode.otherData[0].obj;
		if (!LandmarkManager.Instance.HasEnoughSpaceForStructure(text, genericTileObject.gridTileLocation))
		{
			return;
		}
		NPCSettlement nPCSettlement = LandmarkManager.Instance.CreateNewSettlement(goapNode.actor.currentRegion, LOCATION_TYPE.VILLAGE, null);
		if (goapNode.actor.faction != null && goapNode.actor.faction.isMajorNonPlayer)
		{
			LandmarkManager.Instance.OwnSettlement(goapNode.actor.faction, nPCSettlement);
		}
		Area area = genericTileObject.gridTileLocation.area;
		nPCSettlement.AddAreaToSettlement(area);
		VillageSpot coreVillageSpotOnArea = goapNode.actor.currentRegion.GetCoreVillageSpotOnArea(area);
		nPCSettlement.SetOccupiedVillageSpot(coreVillageSpotOnArea);
		List<LocationStructure> obj = new List<LocationStructure> { LandmarkManager.Instance.PlaceIndividualBuiltStructureForSettlement(nPCSettlement, goapNode.actor.currentRegion.innerMap, genericTileObject.gridTileLocation, text) };
		nPCSettlement.PlaceInitialObjects();
		LocationStructure dwelling = obj[0];
		goapNode.actor.MigrateHomeStructureTo(dwelling);
		if (goapNode.actor.faction != null && goapNode.actor.faction.isMajorNonPlayer)
		{
			for (int i = 0; i < goapNode.actor.faction.characters.Count; i++)
			{
				Character character = goapNode.actor.faction.characters[i];
				if (!character.isDead && character.homeSettlement == null && character.homeStructure == null)
				{
					character.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, goapNode.actor);
				}
			}
		}
		nPCSettlement.SetSettlementType(LandmarkManager.Instance.GetSettlementTypeForCharacter(goapNode.actor));
		goapNode.actor.jobComponent.AddAbleJob(JOB_TYPE.BUILD_BLUEPRINT);
		if (goapNode.actor.behaviourComponent.shouldTryToBuildNewVillage)
		{
			goapNode.actor.behaviourComponent.DropBuildVillagePriority();
		}
	}
}

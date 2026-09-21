using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class PlaceBlueprint : GoapAction
{
	public PlaceBlueprint()
		: base(INTERACTION_TYPE.PLACE_BLUEPRINT)
	{
		base.actionIconString = GoapActionStateDB.Blueprint_Icon;
		base.showNotification = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		if (!goapActionInvalidity.isInvalid && ((StructureSetting)node.otherData[2].obj).structureType == STRUCTURE_TYPE.MAGIC_ACADEMY && (node.actor.currentRegion.HasStructure(STRUCTURE_TYPE.MAGIC_ACADEMY) || node.actor.currentRegion.HasStructureBlueprint(STRUCTURE_TYPE.MAGIC_ACADEMY)))
		{
			goapActionInvalidity.isInvalid = true;
			goapActionInvalidity.reason = "magic_academy_exists";
		}
		return goapActionInvalidity;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Place Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 3;
	}

	public override void AddFillersToLog(Log log, ActualGoapNode goapNode)
	{
		base.AddFillersToLog(log, goapNode);
		log.AddToFillers(null, ((StructureSetting)goapNode.otherData[2].obj).structureType.LocalizedStructureName(), LOG_IDENTIFIER.STRING_1);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (poiTarget.gridTileLocation == null)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public void PrePlaceSuccess(ActualGoapNode goapNode)
	{
		string text = (string)goapNode.otherData[0].obj;
		LocationGridTile locationGridTile = (LocationGridTile)goapNode.otherData[1].obj;
		StructureSetting structureSetting = (StructureSetting)goapNode.otherData[2].obj;
		if (!(goapNode.poiTarget is GenericTileObject genericTileObject))
		{
			return;
		}
		bool flag = false;
		if (!LandmarkManager.Instance.HasAffectedCorruptedTilesForStructure(text, genericTileObject.gridTileLocation) && genericTileObject.PlaceExpiringBlueprintOnTile(text))
		{
			flag = true;
			NPCSettlement homeSettlement = goapNode.actor.homeSettlement;
			if (homeSettlement != null)
			{
				GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BUILD_BLUEPRINT, INTERACTION_TYPE.BUILD_BLUEPRINT, goapNode.poiTarget, homeSettlement);
				goapPlanJob.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[1] { genericTileObject.blueprintOnTile.craftCost });
				goapPlanJob.AddOtherData(INTERACTION_TYPE.BUILD_BLUEPRINT, new object[1] { locationGridTile });
				goapPlanJob.SetCanTakeThisJobChecker("CanTakeBuildJob");
				JobUtilities.PopulatePriorityLocationsForTakingNonEdibleResources(homeSettlement, goapPlanJob, INTERACTION_TYPE.TAKE_RESOURCE);
				List<LocationStructure> structuresOfType = homeSettlement.GetStructuresOfType(STRUCTURE_TYPE.MINE);
				if (structuresOfType != null)
				{
					for (int i = 0; i < structuresOfType.Count; i++)
					{
						LocationStructure location = structuresOfType[i];
						goapPlanJob.AddPriorityLocation(INTERACTION_TYPE.TAKE_RESOURCE, location);
					}
				}
				List<LocationStructure> structuresOfType2 = homeSettlement.GetStructuresOfType(STRUCTURE_TYPE.LUMBERYARD);
				if (structuresOfType2 != null)
				{
					for (int j = 0; j < structuresOfType2.Count; j++)
					{
						LocationStructure location2 = structuresOfType2[j];
						goapPlanJob.AddPriorityLocation(INTERACTION_TYPE.TAKE_RESOURCE, location2);
					}
				}
				homeSettlement.AddToAvailableJobs(goapPlanJob);
			}
			goapNode.descriptionLog.AddToFillers(null, structureSetting.structureType.LocalizedStructureName(), LOG_IDENTIFIER.STRING_1);
		}
		if (!flag)
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", base.goapName + " fail", LOG_TAG.Work, null);
			log.AddToFillers(goapNode.actor, goapNode.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(null, structureSetting.structureType.LocalizedStructureName(), LOG_IDENTIFIER.STRING_1);
			goapNode.OverrideDescriptionLog(log);
		}
	}
}

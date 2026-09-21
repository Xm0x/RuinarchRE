using System.Collections.Generic;
using Inner_Maps.Location_Structures;

public class BuildVampireCastle : GoapAction
{
	public BuildVampireCastle()
		: base(INTERACTION_TYPE.BUILD_VAMPIRE_CASTLE)
	{
		base.actionIconString = GoapActionStateDB.Found_Icon;
		base.showNotification = true;
		base.logTags = new LOG_TAG[2]
		{
			LOG_TAG.Life_Changes,
			LOG_TAG.Work
		};
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
		if (LandmarkManager.Instance.HasEnoughSpaceForStructure(text, genericTileObject.gridTileLocation))
		{
			NPCSettlement nPCSettlement = goapNode.actor.homeSettlement;
			Area area = genericTileObject.gridTileLocation.area;
			if (goapNode.actor.homeSettlement == null)
			{
				nPCSettlement = LandmarkManager.Instance.CreateNewSettlement(goapNode.actor.currentRegion, LOCATION_TYPE.VILLAGE, null);
				LandmarkManager.Instance.OwnSettlement(goapNode.actor.faction, nPCSettlement);
				nPCSettlement.SetSettlementType(LandmarkManager.Instance.GetSettlementTypeForCharacter(goapNode.actor));
				VillageSpot coreVillageSpotOnArea = goapNode.actor.currentRegion.GetCoreVillageSpotOnArea(area);
				nPCSettlement.SetOccupiedVillageSpot(coreVillageSpotOnArea);
			}
			nPCSettlement.AddAreaToSettlement(area);
			LocationStructure dwelling = new List<LocationStructure> { LandmarkManager.Instance.PlaceIndividualBuiltStructureForSettlement(nPCSettlement, goapNode.actor.currentRegion.innerMap, genericTileObject.gridTileLocation, text) }[0];
			goapNode.actor.MigrateHomeStructureTo(dwelling);
		}
	}
}

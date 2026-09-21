using System.Collections.Generic;
using Inner_Maps.Location_Structures;

namespace UtilityScripts;

public static class JobUtilities
{
	public static void PopulatePriorityLocationsForFullnessRecovery(Character actor, GoapPlanJob job)
	{
		if (actor.traitContainer.HasTrait("Travelling"))
		{
			return;
		}
		bool flag = false;
		LocationStructure homeStructure = actor.homeStructure;
		if (homeStructure != null)
		{
			job.AddPriorityLocation(INTERACTION_TYPE.NONE, homeStructure);
			flag = true;
		}
		if (actor.structureComponent.workPlaceStructure != null)
		{
			job.AddPriorityLocation(INTERACTION_TYPE.NONE, actor.structureComponent.workPlaceStructure);
			flag = true;
		}
		if (actor.homeSettlement != null && actor.homeSettlement == actor.currentSettlement)
		{
			job.AddPriorityLocation(INTERACTION_TYPE.NONE, actor.homeSettlement);
		}
		if (actor.isVagrantOrFactionless || (actor.homeStructure != null && actor.homeStructure.structureType.IsSpecialStructure()))
		{
			ILocation currentLocation = GetCurrentLocation(actor);
			if (currentLocation != null)
			{
				job.AddPriorityLocation(INTERACTION_TYPE.NONE, currentLocation);
			}
			List<LocationStructure> list = RuinarchListPool<LocationStructure>.Claim();
			GridMap.Instance.mainRegion.PopulateForageStructuresForVagrantEating(list, actor);
			for (int i = 0; i < list.Count; i++)
			{
				job.AddPriorityLocation(INTERACTION_TYPE.NONE, list[i]);
			}
			RuinarchListPool<LocationStructure>.Release(list);
		}
		if (flag)
		{
			ILocation currentLocation2 = GetCurrentLocation(actor);
			if (currentLocation2 != null)
			{
				job.AddPriorityLocation(INTERACTION_TYPE.NONE, currentLocation2);
			}
		}
	}

	private static ILocation GetCurrentLocation(Character actor)
	{
		LocationStructure currentStructure = actor.currentStructure;
		ILocation result = currentStructure;
		if ((currentStructure == null || currentStructure.structureType == STRUCTURE_TYPE.WILDERNESS || currentStructure.structureType == STRUCTURE_TYPE.OCEAN) && actor.gridTileLocation != null)
		{
			result = actor.areaLocation;
		}
		return result;
	}

	public static void PopulatePriorityLocationsForHappinessRecovery(Character actor, GoapPlanJob job)
	{
		if (actor.traitContainer.HasTrait("Travelling"))
		{
			return;
		}
		bool flag = false;
		if (actor.homeStructure != null)
		{
			job.AddPriorityLocation(INTERACTION_TYPE.NONE, actor.homeStructure);
			flag = true;
		}
		if (actor.homeSettlement != null)
		{
			LocationStructure randomStructureOfType = actor.homeSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.TAVERN);
			if (randomStructureOfType != null)
			{
				job.AddPriorityLocation(INTERACTION_TYPE.NONE, randomStructureOfType);
				flag = true;
			}
		}
		if (flag)
		{
			LocationStructure currentStructure = actor.currentStructure;
			ILocation location = currentStructure;
			if ((currentStructure == null || currentStructure.structureType == STRUCTURE_TYPE.WILDERNESS || currentStructure.structureType == STRUCTURE_TYPE.OCEAN) && actor.gridTileLocation != null)
			{
				location = actor.areaLocation;
			}
			if (location != null)
			{
				job.AddPriorityLocation(INTERACTION_TYPE.NONE, location);
			}
		}
	}

	public static void PopulatePriorityLocationsForTakingNonEdibleResources(Character actor, GoapPlanJob job, INTERACTION_TYPE actionType)
	{
		NPCSettlement homeSettlement = actor.homeSettlement;
		LocationStructure homeStructure = actor.homeStructure;
		if (homeStructure != null)
		{
			job.AddPriorityLocation(actionType, homeStructure);
		}
		if (homeSettlement != null)
		{
			LocationStructure firstStructureOfType = homeSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
			LocationStructure firstStructureOfType2 = homeSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.LUMBERYARD);
			if (firstStructureOfType != null)
			{
				job.AddPriorityLocation(actionType, firstStructureOfType);
			}
			if (firstStructureOfType2 != null)
			{
				job.AddPriorityLocation(actionType, firstStructureOfType2);
			}
		}
	}

	public static void PopulatePriorityLocationsForTakingNonEdibleResources(NPCSettlement settlement, GoapPlanJob job, INTERACTION_TYPE actionType)
	{
		if (settlement != null)
		{
			LocationStructure firstStructureOfType = settlement.GetFirstStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
			LocationStructure firstStructureOfType2 = settlement.GetFirstStructureOfType(STRUCTURE_TYPE.LUMBERYARD);
			LocationStructure firstStructureOfType3 = settlement.GetFirstStructureOfType(STRUCTURE_TYPE.MINE);
			if (firstStructureOfType != null)
			{
				job.AddPriorityLocation(actionType, firstStructureOfType);
			}
			if (firstStructureOfType2 != null)
			{
				job.AddPriorityLocation(actionType, firstStructureOfType2);
			}
			if (firstStructureOfType3 != null)
			{
				job.AddPriorityLocation(actionType, firstStructureOfType3);
			}
		}
	}

	public static void PopulatePriorityLocationsForTakingEdibleResources(Character actor, GoapPlanJob job, INTERACTION_TYPE actionType)
	{
		NPCSettlement homeSettlement = actor.homeSettlement;
		LocationStructure homeStructure = actor.homeStructure;
		if (homeStructure != null)
		{
			job.AddPriorityLocation(actionType, homeStructure);
		}
		if (homeSettlement != null)
		{
			LocationStructure firstStructureOfType = homeSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
			if (firstStructureOfType != null)
			{
				job.AddPriorityLocation(actionType, firstStructureOfType);
			}
		}
	}

	public static void PopulatePriorityLocationsForTakingPersonalItem(Character actor, GoapPlanJob job, INTERACTION_TYPE actionType)
	{
		NPCSettlement homeSettlement = actor.homeSettlement;
		if (homeSettlement != null)
		{
			LocationStructure firstStructureOfType = homeSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
			if (firstStructureOfType != null)
			{
				job.AddPriorityLocation(actionType, firstStructureOfType);
			}
		}
	}

	public static void PopulatePriorityLocationsForTakingPersonalItem(NPCSettlement settlement, GoapPlanJob job, INTERACTION_TYPE actionType)
	{
		if (settlement != null)
		{
			LocationStructure firstStructureOfType = settlement.GetFirstStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
			if (firstStructureOfType != null)
			{
				job.AddPriorityLocation(actionType, firstStructureOfType);
			}
		}
	}

	public static void PopulatePriorityLocationsForSuicide(Character actor, GoapPlanJob job)
	{
		NPCSettlement homeSettlement = actor.homeSettlement;
		if (homeSettlement == null)
		{
			return;
		}
		LocationStructure firstStructureOfType = homeSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
		if (firstStructureOfType != null)
		{
			job.AddPriorityLocation(INTERACTION_TYPE.NONE, firstStructureOfType);
			LocationStructure currentStructure = actor.currentStructure;
			ILocation location = currentStructure;
			if ((currentStructure == null || currentStructure.structureType == STRUCTURE_TYPE.WILDERNESS || currentStructure.structureType == STRUCTURE_TYPE.OCEAN) && actor.gridTileLocation != null)
			{
				location = actor.areaLocation;
			}
			if (location != null)
			{
				job.AddPriorityLocation(INTERACTION_TYPE.NONE, location);
			}
		}
	}

	public static void PopulatePriorityLocationsForCraftingCultistKit(Character actor, GoapPlanJob job, INTERACTION_TYPE actionType)
	{
		NPCSettlement homeSettlement = actor.homeSettlement;
		if (homeSettlement != null)
		{
			LocationStructure firstStructureOfType = homeSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
			if (firstStructureOfType != null)
			{
				job.AddPriorityLocation(actionType, firstStructureOfType);
			}
		}
		if (actor.homeStructure != null)
		{
			job.AddPriorityLocation(actionType, actor.homeStructure);
		}
	}

	public static void PopulatePriorityLocationsForProduceResources(NPCSettlement settlement, GoapPlanJob job, RESOURCE resourceType)
	{
		if (settlement != null)
		{
			switch (resourceType)
			{
			case RESOURCE.FOOD:
				PopulatePriorityLocationsForProduceFood(settlement, job);
				break;
			case RESOURCE.WOOD:
				PopulatePriorityLocationsForProduceWood(settlement, job);
				break;
			case RESOURCE.STONE:
				PopulatePriorityLocationsForProduceStone(settlement, job);
				break;
			case RESOURCE.METAL:
				PopulatePriorityLocationsForProduceMetal(settlement, job);
				break;
			}
		}
	}

	private static void PopulatePriorityLocationsForProduceFood(NPCSettlement settlement, GoapPlanJob job)
	{
		List<LocationStructure> structuresOfType = settlement.GetStructuresOfType(STRUCTURE_TYPE.FARM);
		List<LocationStructure> structuresOfType2 = settlement.GetStructuresOfType(STRUCTURE_TYPE.FISHERY);
		List<LocationStructure> structuresOfType3 = settlement.GetStructuresOfType(STRUCTURE_TYPE.BUTCHERS_SHOP);
		if (structuresOfType != null)
		{
			for (int i = 0; i < structuresOfType.Count; i++)
			{
				LocationStructure location = structuresOfType[i];
				job.AddPriorityLocation(INTERACTION_TYPE.NONE, location);
			}
		}
		if (structuresOfType2 != null)
		{
			for (int j = 0; j < structuresOfType2.Count; j++)
			{
				Fishery fishery = structuresOfType2[j] as Fishery;
				job.AddPriorityLocation(INTERACTION_TYPE.NONE, fishery.connectedOcean);
				List<TileObject> list = RuinarchListPool<TileObject>.Claim();
				fishery.connectedOcean.PopulateTileObjectsOfType<FishingSpot>(list);
				for (int k = 0; k < list.Count; k++)
				{
					TileObject tileObject = list[k];
					if (tileObject.gridTileLocation != null)
					{
						job.AddPriorityLocation(INTERACTION_TYPE.NONE, tileObject.gridTileLocation.area);
					}
				}
				RuinarchListPool<TileObject>.Release(list);
			}
		}
		if (structuresOfType3 != null)
		{
			for (int l = 0; l < structuresOfType3.Count; l++)
			{
				LocationStructure locationStructure = structuresOfType3[l];
				job.AddPriorityLocation(INTERACTION_TYPE.NONE, locationStructure);
				job.AddPriorityLocation(INTERACTION_TYPE.NONE, locationStructure.occupiedArea);
			}
		}
	}

	private static void PopulatePriorityLocationsForProduceWood(NPCSettlement settlement, GoapPlanJob job)
	{
		List<LocationStructure> structuresOfType = settlement.GetStructuresOfType(STRUCTURE_TYPE.LUMBERYARD);
		if (structuresOfType != null)
		{
			for (int i = 0; i < structuresOfType.Count; i++)
			{
				job.AddPriorityLocation(INTERACTION_TYPE.NONE, structuresOfType[i]);
				job.AddPriorityLocation(INTERACTION_TYPE.NONE, structuresOfType[i].occupiedArea);
			}
		}
	}

	private static void PopulatePriorityLocationsForProduceStone(NPCSettlement settlement, GoapPlanJob job)
	{
		List<LocationStructure> structuresOfType = settlement.GetStructuresOfType(STRUCTURE_TYPE.QUARRY);
		if (structuresOfType != null)
		{
			for (int i = 0; i < structuresOfType.Count; i++)
			{
				job.AddPriorityLocation(INTERACTION_TYPE.NONE, structuresOfType[i]);
				job.AddPriorityLocation(INTERACTION_TYPE.NONE, structuresOfType[i].occupiedArea);
			}
		}
	}

	private static void PopulatePriorityLocationsForProduceMetal(NPCSettlement settlement, GoapPlanJob job)
	{
		List<LocationStructure> structuresOfType = settlement.GetStructuresOfType(STRUCTURE_TYPE.MINE);
		if (structuresOfType != null)
		{
			for (int i = 0; i < structuresOfType.Count; i++)
			{
				Mine mine = structuresOfType[i] as Mine;
				job.AddPriorityLocation(INTERACTION_TYPE.NONE, mine.connectedCave);
			}
		}
	}
}

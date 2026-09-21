using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UtilityScripts;

public class SettlementResources
{
	public enum StructureRequirement
	{
		NONE = 0,
		ROCK = 1,
		TREE = 2,
		FISHING_SPOT = 3,
		MINE_SHACK_SPOT = 5
	}

	public bool IsRequirementAvailable(StructureRequirement p_structureRequirement, BaseSettlement p_settlement)
	{
		switch (p_structureRequirement)
		{
		case StructureRequirement.TREE:
			if (HasTrees(p_settlement))
			{
				return true;
			}
			return false;
		case StructureRequirement.FISHING_SPOT:
			if (HasFishingSpot(p_settlement))
			{
				return true;
			}
			return false;
		case StructureRequirement.MINE_SHACK_SPOT:
			if (HasMineSpotShack(p_settlement))
			{
				return true;
			}
			return false;
		default:
			return true;
		}
	}

	public bool HasTrees(BaseSettlement p_settlement)
	{
		for (int i = 0; i < p_settlement.areas.Count; i++)
		{
			if (p_settlement.areas[i].tileObjectComponent.HasTree())
			{
				return true;
			}
		}
		return false;
	}

	public int GetTreesCount(BaseSettlement p_settlement)
	{
		int num = 0;
		for (int i = 0; i < p_settlement.areas.Count; i++)
		{
			Area area = p_settlement.areas[i];
			num += area.tileObjectComponent.GetTreeCount();
		}
		return num;
	}

	public void PopulateAllTrees(List<TileObject> p_trees, BaseSettlement p_settlement)
	{
		for (int i = 0; i < p_settlement.areas.Count; i++)
		{
			p_settlement.areas[i].tileObjectComponent.PopulateAllTrees(p_trees);
		}
	}

	public int GetFishingSpotCount(BaseSettlement p_settlement)
	{
		int num = 0;
		for (int i = 0; i < p_settlement.areas.Count; i++)
		{
			Area area = p_settlement.areas[i];
			num += area.tileObjectComponent.GetFishingSpotCount();
		}
		return num;
	}

	public bool HasFishingSpot(BaseSettlement p_settlement)
	{
		for (int i = 0; i < p_settlement.areas.Count; i++)
		{
			if (p_settlement.areas[i].tileObjectComponent.HasFishingSpot())
			{
				return true;
			}
		}
		return false;
	}

	public void PopulateAllFishingSpots(List<TileObject> p_spots, BaseSettlement p_settlement)
	{
		for (int i = 0; i < p_settlement.areas.Count; i++)
		{
			p_settlement.areas[i].tileObjectComponent.PopulateAllFishingSpots(p_spots);
		}
	}

	public int GetMineShackSpotCount(BaseSettlement p_settlement)
	{
		int num = 0;
		for (int i = 0; i < p_settlement.areas.Count; i++)
		{
			Area area = p_settlement.areas[i];
			num += area.tileObjectComponent.GetMineShackSpotCount();
		}
		return num;
	}

	public bool HasMineSpotShack(BaseSettlement p_settlement)
	{
		for (int i = 0; i < p_settlement.areas.Count; i++)
		{
			if (p_settlement.areas[i].tileObjectComponent.HasMineShackSpot())
			{
				return true;
			}
		}
		return false;
	}

	public void PopulateAllMineShackSpots(List<LocationGridTile> p_spots, BaseSettlement p_settlement)
	{
		for (int i = 0; i < p_settlement.areas.Count; i++)
		{
			p_settlement.areas[i].tileObjectComponent.PopulateAllMineShackSpots(p_spots);
		}
	}

	public int GetCharacterCount(BaseSettlement p_settlement)
	{
		int num = 0;
		for (int i = 0; i < p_settlement.areas.Count; i++)
		{
			Area area = p_settlement.areas[i];
			num += area.locationCharacterTracker.GetCharacterCount();
		}
		return num;
	}

	public void PopulateAllAnimalsForSkinnersLodgeSkinning(List<Character> allAvailableAnimals, BaseSettlement p_settlement, LocationStructure p_workerStructure)
	{
		if (p_settlement is NPCSettlement { occupiedVillageSpot: not null } nPCSettlement)
		{
			for (int i = 0; i < nPCSettlement.occupiedVillageSpot.reservedAreas.Count; i++)
			{
				nPCSettlement.occupiedVillageSpot.reservedAreas[i].locationCharacterTracker.PopulateAllAnimalsForSkinnersLodgeSkinning(allAvailableAnimals, p_workerStructure);
			}
		}
		else
		{
			for (int j = 0; j < p_settlement.areas.Count; j++)
			{
				p_settlement.areas[j].locationCharacterTracker.PopulateAllAnimalsForSkinnersLodgeSkinning(allAvailableAnimals, p_workerStructure);
			}
		}
	}

	public void PopulateAllAnimalsForSkinnersLodgeShearing(List<Character> ableToShearTodayList, BaseSettlement p_settlement)
	{
		if (p_settlement is NPCSettlement { occupiedVillageSpot: not null } nPCSettlement)
		{
			for (int i = 0; i < nPCSettlement.occupiedVillageSpot.reservedAreas.Count; i++)
			{
				nPCSettlement.occupiedVillageSpot.reservedAreas[i].locationCharacterTracker.PopulateAllAnimalsForSkinnersLodgeShearing(ableToShearTodayList);
			}
		}
		else
		{
			for (int j = 0; j < p_settlement.areas.Count; j++)
			{
				p_settlement.areas[j].locationCharacterTracker.PopulateAllAnimalsForSkinnersLodgeShearing(ableToShearTodayList);
			}
		}
	}

	public Summon GetFirstButcherableAnimal(BaseSettlement p_settlement)
	{
		for (int i = 0; i < p_settlement.areas.Count; i++)
		{
			Summon firstButcherableAnimal = p_settlement.areas[i].locationCharacterTracker.GetFirstButcherableAnimal();
			if (firstButcherableAnimal != null)
			{
				return firstButcherableAnimal;
			}
		}
		if (p_settlement is NPCSettlement { occupiedVillageSpot: not null } nPCSettlement)
		{
			for (int j = 0; j < nPCSettlement.occupiedVillageSpot.reservedAreas.Count; j++)
			{
				Summon firstButcherableAnimal2 = nPCSettlement.occupiedVillageSpot.reservedAreas[j].locationCharacterTracker.GetFirstButcherableAnimal();
				if (firstButcherableAnimal2 != null)
				{
					return firstButcherableAnimal2;
				}
			}
		}
		return null;
	}

	public int GetBuiltResourcePileCount(BaseSettlement p_settlement)
	{
		int num = 0;
		for (int i = 0; i < p_settlement.areas.Count; i++)
		{
			Area area = p_settlement.areas[i];
			num += area.tileObjectComponent.GetBuiltResourcePileCount();
		}
		return num;
	}

	public ResourcePile GetRandomPileOfCropsForFarmHaul(BaseSettlement p_settlement)
	{
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		for (int i = 0; i < p_settlement.areas.Count; i++)
		{
			ResourcePile randomPileOfCropsForFarmHaul = p_settlement.areas[i].tileObjectComponent.GetRandomPileOfCropsForFarmHaul();
			if (randomPileOfCropsForFarmHaul != null)
			{
				list.Add(randomPileOfCropsForFarmHaul);
			}
		}
		ResourcePile result = null;
		if (list.Count > 0)
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)] as ResourcePile;
		}
		RuinarchListPool<TileObject>.Release(list);
		return result;
	}

	public ResourcePile GetRandomPileOfClothOrLeatherForSkinnersLodgeHaul(BaseSettlement p_settlement)
	{
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		for (int i = 0; i < p_settlement.areas.Count; i++)
		{
			ResourcePile randomPileOfClothOrLeatherForSkinnersLodgeHaul = p_settlement.areas[i].tileObjectComponent.GetRandomPileOfClothOrLeatherForSkinnersLodgeHaul();
			if (randomPileOfClothOrLeatherForSkinnersLodgeHaul != null)
			{
				list.Add(randomPileOfClothOrLeatherForSkinnersLodgeHaul);
			}
		}
		ResourcePile result = null;
		if (list.Count > 0)
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)] as ResourcePile;
		}
		RuinarchListPool<TileObject>.Release(list);
		return result;
	}

	public ResourcePile GetRandomPileOfMeatsForButchersShopHaul(BaseSettlement p_settlement, Character p_butcher)
	{
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		for (int i = 0; i < p_settlement.areas.Count; i++)
		{
			ResourcePile randomPileOfMeatsForButchersShopHaul = p_settlement.areas[i].tileObjectComponent.GetRandomPileOfMeatsForButchersShopHaul(p_butcher);
			if (randomPileOfMeatsForButchersShopHaul != null)
			{
				list.Add(randomPileOfMeatsForButchersShopHaul);
			}
		}
		ResourcePile result = null;
		if (list.Count > 0)
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)] as ResourcePile;
		}
		RuinarchListPool<TileObject>.Release(list);
		return result;
	}

	public ResourcePile GetRandomPileOfFishesForFisheryHaul(BaseSettlement p_settlement)
	{
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		for (int i = 0; i < p_settlement.areas.Count; i++)
		{
			ResourcePile randomPileOfFishesForFisheryHaul = p_settlement.areas[i].tileObjectComponent.GetRandomPileOfFishesForFisheryHaul();
			if (randomPileOfFishesForFisheryHaul != null)
			{
				list.Add(randomPileOfFishesForFisheryHaul);
			}
		}
		ResourcePile result = null;
		if (list.Count > 0)
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)] as ResourcePile;
		}
		RuinarchListPool<TileObject>.Release(list);
		return result;
	}

	public ResourcePile GetRandomPileOfWoodsForLumberyardHaul(BaseSettlement p_settlement)
	{
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		for (int i = 0; i < p_settlement.areas.Count; i++)
		{
			ResourcePile randomPileOfWoodsForLumberyardHaul = p_settlement.areas[i].tileObjectComponent.GetRandomPileOfWoodsForLumberyardHaul();
			if (randomPileOfWoodsForLumberyardHaul != null)
			{
				list.Add(randomPileOfWoodsForLumberyardHaul);
			}
		}
		ResourcePile result = null;
		if (list.Count > 0)
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)] as ResourcePile;
		}
		RuinarchListPool<TileObject>.Release(list);
		return result;
	}

	public ResourcePile GetRandomPileOfMetalOrStoneForMineHaul(BaseSettlement p_settlement)
	{
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		for (int i = 0; i < p_settlement.areas.Count; i++)
		{
			ResourcePile randomPileOfMetalOrStoneForMineHaul = p_settlement.areas[i].tileObjectComponent.GetRandomPileOfMetalOrStoneForMineHaul();
			if (randomPileOfMetalOrStoneForMineHaul != null)
			{
				list.Add(randomPileOfMetalOrStoneForMineHaul);
			}
		}
		ResourcePile result = null;
		if (list.Count > 0)
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)] as ResourcePile;
		}
		RuinarchListPool<TileObject>.Release(list);
		return result;
	}

	public ResourcePile GetRandomPileOfResourceTypeForWorkshopHaul(RESOURCE p_resourceType, STRUCTURE_TYPE p_structureType, BaseSettlement p_settlement)
	{
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		if (p_settlement.HasStructure(p_structureType))
		{
			List<LocationStructure> structuresOfType = p_settlement.GetStructuresOfType(p_structureType);
			for (int i = 0; i < structuresOfType.Count; i++)
			{
				LocationStructure locationStructure = structuresOfType[i];
				List<ResourcePile> list2 = RuinarchListPool<ResourcePile>.Claim();
				locationStructure.PopulateResourcesInStructure(list2, p_resourceType);
				for (int j = 0; j < list2.Count; j++)
				{
					ResourcePile resourcePile = list2[j];
					if (resourcePile.providedResource == p_resourceType && resourcePile.resourceInPile >= 40 && !resourcePile.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE))
					{
						list.Add(resourcePile);
					}
				}
				RuinarchListPool<ResourcePile>.Release(list2);
			}
		}
		if (list.Count <= 0 && p_settlement is NPCSettlement { cityCenter: not null } nPCSettlement)
		{
			List<ResourcePile> list3 = RuinarchListPool<ResourcePile>.Claim();
			nPCSettlement.cityCenter.PopulateResourcesInStructure(list3, p_resourceType);
			for (int k = 0; k < list3.Count; k++)
			{
				ResourcePile resourcePile2 = list3[k];
				if (resourcePile2.providedResource == p_resourceType && resourcePile2.resourceInPile >= 40 && !resourcePile2.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE))
				{
					list.Add(resourcePile2);
				}
			}
			RuinarchListPool<ResourcePile>.Release(list3);
		}
		ResourcePile result = null;
		if (list.Count > 0)
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)] as ResourcePile;
		}
		RuinarchListPool<TileObject>.Release(list);
		return result;
	}

	public ResourcePile GetNearestResourcePileFrom(LocationGridTile p_tile, BaseSettlement p_settlement)
	{
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		for (int i = 0; i < p_settlement.areas.Count; i++)
		{
			ResourcePile randomResourcePile = p_settlement.areas[i].tileObjectComponent.GetRandomResourcePile();
			if (randomResourcePile != null)
			{
				list.Add(randomResourcePile);
			}
		}
		ResourcePile resourcePile = null;
		if (list.Count > 0)
		{
			float num = 0f;
			for (int j = 0; j < list.Count; j++)
			{
				TileObject tileObject = list[j];
				float distanceTo = p_tile.GetDistanceTo(tileObject.gridTileLocation);
				if (resourcePile == null || distanceTo < num)
				{
					resourcePile = tileObject as ResourcePile;
					num = distanceTo;
				}
			}
		}
		RuinarchListPool<TileObject>.Release(list);
		return resourcePile;
	}

	public HerbPlant GetFirstAvailableHerbPlant(BaseSettlement p_settlement, Character p_worker)
	{
		for (int i = 0; i < p_settlement.areas.Count; i++)
		{
			HerbPlant firstAvailableHerbPlant = p_settlement.areas[i].tileObjectComponent.GetFirstAvailableHerbPlant(p_worker);
			if (firstAvailableHerbPlant != null)
			{
				return firstAvailableHerbPlant;
			}
		}
		return null;
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}
}

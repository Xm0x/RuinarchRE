using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

public class AreaTileObjectComponent : AreaComponent
{
	public List<TileObject> itemsInArea { get; private set; }

	public List<TreeObject> trees { get; private set; }

	public List<FishingSpot> fishingSpots { get; private set; }

	public List<HerbPlant> herbPlants { get; private set; }

	public List<LocationGridTile> mineShackSpots { get; private set; }

	public List<ResourcePile> resourcePiles { get; private set; }

	public AreaTileObjectComponent()
	{
		itemsInArea = new List<TileObject>();
		trees = new List<TreeObject>();
		fishingSpots = new List<FishingSpot>();
		herbPlants = new List<HerbPlant>();
		mineShackSpots = new List<LocationGridTile>();
		resourcePiles = new List<ResourcePile>();
	}

	public void AddItemInArea(TileObject item)
	{
		if (!itemsInArea.Contains(item))
		{
			itemsInArea.Add(item);
			if (item is ResourcePile p_newPile)
			{
				AddResourcePile(p_newPile);
			}
			else if (item is TreeObject p_tree)
			{
				AddTree(p_tree);
			}
			else if (item is FishingSpot p_spot)
			{
				AddFishingSpot(p_spot);
			}
			else if (item is HerbPlant p_plant)
			{
				AddHerbPlant(p_plant);
			}
		}
	}

	public bool RemoveItemInArea(TileObject item)
	{
		if (itemsInArea.Remove(item))
		{
			if (item is ResourcePile p_newPile)
			{
				RemoveResourcePile(p_newPile);
			}
			else if (item is TreeObject p_tree)
			{
				RemoveTree(p_tree);
			}
			else if (item is FishingSpot p_spot)
			{
				RemoveFishingSpot(p_spot);
			}
			else if (item is HerbPlant p_plant)
			{
				RemoveHerbPlant(p_plant);
			}
			return true;
		}
		return false;
	}

	public bool HasTileObjectOfTypeInHexTile(TILE_OBJECT_TYPE type)
	{
		for (int i = 0; i < itemsInArea.Count; i++)
		{
			if (itemsInArea[i].tileObjectType == type)
			{
				return true;
			}
		}
		return false;
	}

	public int GetNumberOfTileObjects(TILE_OBJECT_TYPE type)
	{
		int num = 0;
		for (int i = 0; i < itemsInArea.Count; i++)
		{
			if (itemsInArea[i].tileObjectType == type)
			{
				num++;
			}
		}
		return num;
	}

	public int GetNumberOfBuiltTileObjects(TILE_OBJECT_TYPE type)
	{
		int num = 0;
		for (int i = 0; i < itemsInArea.Count; i++)
		{
			TileObject tileObject = itemsInArea[i];
			if (tileObject.tileObjectType == type && tileObject.mapObjectState == MAP_OBJECT_STATE.BUILT)
			{
				num++;
			}
		}
		return num;
	}

	public int GetNumberOfTileObjectsInHexTile(TILE_OBJECT_TYPE type, TILE_OBJECT_TYPE type2)
	{
		int num = 0;
		for (int i = 0; i < itemsInArea.Count; i++)
		{
			TileObject tileObject = itemsInArea[i];
			if (tileObject.tileObjectType == type || tileObject.tileObjectType == type2)
			{
				num++;
			}
		}
		return num;
	}

	public int GetNumberOfTileObjectsInHexTile(TILE_OBJECT_TYPE type, TILE_OBJECT_TYPE type2, MapGenerationData p_data)
	{
		int num = 0;
		for (int i = 0; i < base.owner.gridTileComponent.gridTiles.Count; i++)
		{
			LocationGridTile locationGridTile = base.owner.gridTileComponent.gridTiles[i];
			if (locationGridTile.tileObjectComponent.objHere != null)
			{
				TileObject objHere = locationGridTile.tileObjectComponent.objHere;
				if (objHere.tileObjectType == type || objHere.tileObjectType == type2)
				{
					num++;
				}
			}
			else
			{
				TILE_OBJECT_TYPE generatedObjectOnTile = p_data.GetGeneratedObjectOnTile(locationGridTile);
				if (generatedObjectOnTile == type || generatedObjectOnTile == type2)
				{
					num++;
				}
			}
		}
		return num;
	}

	public void PopulateTileObjectsInArea<T>(List<TileObject> tileObjects) where T : TileObject
	{
		for (int i = 0; i < itemsInArea.Count; i++)
		{
			if (itemsInArea[i] is T item)
			{
				tileObjects.Add(item);
			}
		}
	}

	public void PopulateBuiltTileObjectsInArea(List<TileObject> tileObjects, TILE_OBJECT_TYPE p_type)
	{
		for (int i = 0; i < itemsInArea.Count; i++)
		{
			TileObject tileObject = itemsInArea[i];
			if (tileObject.tileObjectType == p_type && tileObject.mapObjectState == MAP_OBJECT_STATE.BUILT)
			{
				tileObjects.Add(tileObject);
			}
		}
	}

	public bool HasBuiltFoodPileInArea()
	{
		for (int i = 0; i < itemsInArea.Count; i++)
		{
			TileObject tileObject = itemsInArea[i];
			if (tileObject is FoodPile && tileObject.mapObjectState == MAP_OBJECT_STATE.BUILT)
			{
				return true;
			}
		}
		return false;
	}

	public TileObject GetRandomTileObject()
	{
		if (itemsInArea.Count > 0)
		{
			return itemsInArea[GameUtilities.RandomBetweenTwoNumbers(0, itemsInArea.Count - 1)];
		}
		return null;
	}

	public TileObject GetRandomTileObjectForRaidAttack()
	{
		TileObject result = null;
		if (itemsInArea.Count > 0)
		{
			List<TileObject> list = RuinarchListPool<TileObject>.Claim();
			for (int i = 0; i < itemsInArea.Count; i++)
			{
				TileObject tileObject = itemsInArea[i];
				if (!tileObject.traitContainer.HasTrait("Indestructible") && !tileObject.IsUnpassable() && (tileObject.tileObjectType.IsTileObjectAnItem() || tileObject.tileObjectType.IsTileObjectImportant() || tileObject.tileObjectType.CanBeRepaired()) && tileObject.mapObjectState == MAP_OBJECT_STATE.BUILT)
				{
					list.Add(tileObject);
				}
			}
			if (list.Count > 0)
			{
				result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
			}
			RuinarchListPool<TileObject>.Release(list);
		}
		return result;
	}

	public TileObject GetAvailableTileObject(TILE_OBJECT_TYPE p_type)
	{
		if (itemsInArea.Count > 0)
		{
			for (int i = 0; i < itemsInArea.Count; i++)
			{
				TileObject tileObject = itemsInArea[i];
				if (p_type == tileObject.tileObjectType && tileObject.mapObjectState == MAP_OBJECT_STATE.BUILT && tileObject.IsAvailable())
				{
					return tileObject;
				}
			}
		}
		return null;
	}

	public T GetFirstTileObject<T>() where T : TileObject
	{
		if (itemsInArea.Count > 0)
		{
			for (int i = 0; i < itemsInArea.Count; i++)
			{
				TileObject tileObject = itemsInArea[i];
				if (tileObject is T result && tileObject.mapObjectState == MAP_OBJECT_STATE.BUILT)
				{
					return result;
				}
			}
		}
		return null;
	}

	public void PopulateTileObjectsWithTraitThatActorCanReach(string p_traitName, List<TileObject> p_objects, Character p_actor)
	{
		if (itemsInArea.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < itemsInArea.Count; i++)
		{
			TileObject tileObject = itemsInArea[i];
			if (tileObject.IsAvailable() && tileObject.traitContainer.HasTrait(p_traitName) && p_actor.movementComponent.HasPathToEvenIfDiffRegion(tileObject.gridTileLocation))
			{
				p_objects.Add(tileObject);
			}
		}
	}

	public void AddTree(TreeObject p_tree)
	{
		trees.Add(p_tree);
	}

	public void RemoveTree(TreeObject p_tree)
	{
		trees.Remove(p_tree);
	}

	public int GetTreeCount()
	{
		return trees.Count;
	}

	public bool HasTree()
	{
		return trees.Count > 0;
	}

	public void PopulateAllTrees(List<TileObject> p_trees)
	{
		p_trees.AddRange(trees);
	}

	public void AddFishingSpot(FishingSpot p_spot)
	{
		fishingSpots.Add(p_spot);
	}

	public void RemoveFishingSpot(FishingSpot p_spot)
	{
		fishingSpots.Remove(p_spot);
	}

	public int GetFishingSpotCount()
	{
		return fishingSpots.Count;
	}

	public bool HasFishingSpot()
	{
		return fishingSpots.Count > 0;
	}

	public void PopulateAllFishingSpots(List<TileObject> p_spots)
	{
		p_spots.AddRange(fishingSpots);
	}

	public void AddMineShackSpot(LocationGridTile p_spot)
	{
		if (!mineShackSpots.Contains(p_spot))
		{
			mineShackSpots.Add(p_spot);
		}
	}

	public void RemoveMineShackSpot(LocationGridTile p_spot)
	{
		mineShackSpots.Remove(p_spot);
	}

	public int GetMineShackSpotCount()
	{
		return mineShackSpots.Count;
	}

	public bool HasMineShackSpot()
	{
		return mineShackSpots.Count > 0;
	}

	public void PopulateAllMineShackSpots(List<LocationGridTile> p_spots)
	{
		p_spots.AddRange(mineShackSpots);
	}

	public void AddResourcePile(ResourcePile p_newPile)
	{
		resourcePiles.Add(p_newPile);
	}

	public void RemoveResourcePile(ResourcePile p_newPile)
	{
		resourcePiles.Remove(p_newPile);
	}

	public ResourcePile GetRandomPileOfCropsForFarmHaul()
	{
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		for (int i = 0; i < resourcePiles.Count; i++)
		{
			ResourcePile resourcePile = resourcePiles[i];
			LocationStructure currentStructure = resourcePile.currentStructure;
			if (resourcePile.characterOwner == null && (resourcePile.tileObjectType == TILE_OBJECT_TYPE.CORN || resourcePile.tileObjectType == TILE_OBJECT_TYPE.ICEBERRY || resourcePile.tileObjectType == TILE_OBJECT_TYPE.PINEAPPLE || resourcePile.tileObjectType == TILE_OBJECT_TYPE.HYPNO_HERB || resourcePile.tileObjectType == TILE_OBJECT_TYPE.POTATO) && currentStructure != null && currentStructure.structureType != STRUCTURE_TYPE.CITY_CENTER && currentStructure.structureType != STRUCTURE_TYPE.FARM && currentStructure.structureType != STRUCTURE_TYPE.DWELLING && !resourcePile.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE))
			{
				list.Add(resourcePile);
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

	public ResourcePile GetRandomPileOfClothOrLeatherForSkinnersLodgeHaul()
	{
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		for (int i = 0; i < resourcePiles.Count; i++)
		{
			ResourcePile resourcePile = resourcePiles[i];
			LocationStructure currentStructure = resourcePile.currentStructure;
			if (resourcePile.characterOwner == null && (resourcePile.tileObjectType == TILE_OBJECT_TYPE.MINK_CLOTH || resourcePile.tileObjectType == TILE_OBJECT_TYPE.MOONCRAWLER_CLOTH || resourcePile.tileObjectType == TILE_OBJECT_TYPE.RABBIT_CLOTH || resourcePile.tileObjectType == TILE_OBJECT_TYPE.BEAR_HIDE || resourcePile.tileObjectType == TILE_OBJECT_TYPE.BOAR_HIDE || resourcePile.tileObjectType == TILE_OBJECT_TYPE.DRAGON_HIDE || resourcePile.tileObjectType == TILE_OBJECT_TYPE.SCALE_HIDE || resourcePile.tileObjectType == TILE_OBJECT_TYPE.WOLF_HIDE) && currentStructure != null && currentStructure.structureType != STRUCTURE_TYPE.WORKSHOP && currentStructure.structureType != STRUCTURE_TYPE.CITY_CENTER && currentStructure.structureType != STRUCTURE_TYPE.HUNTER_LODGE && !resourcePile.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE))
			{
				list.Add(resourcePile);
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

	public ResourcePile GetRandomPileOfMeatsForButchersShopHaul(Character p_butcher)
	{
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		for (int i = 0; i < resourcePiles.Count; i++)
		{
			ResourcePile resourcePile = resourcePiles[i];
			LocationStructure currentStructure = resourcePile.currentStructure;
			bool flag = ((!p_butcher.traitContainer.HasTrait("Cannibal")) ? (resourcePile.tileObjectType == TILE_OBJECT_TYPE.ANIMAL_MEAT) : (resourcePile.tileObjectType == TILE_OBJECT_TYPE.ANIMAL_MEAT || resourcePile.tileObjectType == TILE_OBJECT_TYPE.ELF_MEAT || resourcePile.tileObjectType == TILE_OBJECT_TYPE.HUMAN_MEAT));
			if (flag && resourcePile.characterOwner == null && currentStructure != null && currentStructure.structureType != STRUCTURE_TYPE.CITY_CENTER && currentStructure.structureType != STRUCTURE_TYPE.BUTCHERS_SHOP && currentStructure.structureType != STRUCTURE_TYPE.FARM && currentStructure.structureType != STRUCTURE_TYPE.DWELLING && !resourcePile.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE))
			{
				list.Add(resourcePile);
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

	public ResourcePile GetRandomPileOfFishesForFisheryHaul()
	{
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		for (int i = 0; i < resourcePiles.Count; i++)
		{
			ResourcePile resourcePile = resourcePiles[i];
			LocationStructure currentStructure = resourcePile.currentStructure;
			if (resourcePile.tileObjectType == TILE_OBJECT_TYPE.FISH_PILE && resourcePile.characterOwner == null && currentStructure != null && currentStructure.structureType != STRUCTURE_TYPE.CITY_CENTER && currentStructure.structureType != STRUCTURE_TYPE.FISHERY && currentStructure.structureType != STRUCTURE_TYPE.DWELLING && !resourcePile.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE))
			{
				list.Add(resourcePile);
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

	public ResourcePile GetRandomPileOfWoodsForLumberyardHaul()
	{
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		for (int i = 0; i < resourcePiles.Count; i++)
		{
			ResourcePile resourcePile = resourcePiles[i];
			LocationStructure currentStructure = resourcePile.currentStructure;
			if (resourcePile.tileObjectType == TILE_OBJECT_TYPE.WOOD_PILE && resourcePile.characterOwner == null && resourcePile.mapObjectState == MAP_OBJECT_STATE.BUILT && currentStructure != null && currentStructure.structureType != STRUCTURE_TYPE.CITY_CENTER && currentStructure.structureType != STRUCTURE_TYPE.LUMBERYARD && currentStructure.structureType != STRUCTURE_TYPE.WORKSHOP && !resourcePile.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE))
			{
				list.Add(resourcePile);
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

	public ResourcePile GetRandomPileOfMetalOrStoneForMineHaul()
	{
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		for (int i = 0; i < resourcePiles.Count; i++)
		{
			ResourcePile resourcePile = resourcePiles[i];
			LocationStructure currentStructure = resourcePile.currentStructure;
			if (resourcePile.characterOwner == null && (resourcePile.tileObjectType.IsMetal() || resourcePile.tileObjectType == TILE_OBJECT_TYPE.STONE_PILE) && resourcePile.gridTileLocation != null && resourcePile.mapObjectState == MAP_OBJECT_STATE.BUILT && currentStructure != null && currentStructure.structureType != STRUCTURE_TYPE.CITY_CENTER && currentStructure.structureType != STRUCTURE_TYPE.MINE && currentStructure.structureType != STRUCTURE_TYPE.WORKSHOP && !resourcePile.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE))
			{
				list.Add(resourcePile);
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

	public int GetBuiltResourcePileCount()
	{
		int num = 0;
		for (int i = 0; i < resourcePiles.Count; i++)
		{
			if (resourcePiles[i].mapObjectState == MAP_OBJECT_STATE.BUILT)
			{
				num++;
			}
		}
		return num;
	}

	public int GetTotalResourceCount(RESOURCE p_resource)
	{
		int num = 0;
		for (int i = 0; i < resourcePiles.Count; i++)
		{
			ResourcePile resourcePile = resourcePiles[i];
			if (resourcePile.providedResource == p_resource && resourcePile.mapObjectState == MAP_OBJECT_STATE.BUILT)
			{
				num += resourcePile.resourceInPile;
			}
		}
		return num;
	}

	public T GetRandomTileObjectOfTypeForBanditForage<T>() where T : TileObject
	{
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		for (int i = 0; i < itemsInArea.Count; i++)
		{
			if (!(itemsInArea[i] is T val) || !val.traitContainer.HasTrait("Edible"))
			{
				continue;
			}
			LocationGridTile gridTileLocation = val.gridTileLocation;
			if (val.mapObjectState == MAP_OBJECT_STATE.BUILT && val.characterOwner == null && gridTileLocation != null && !val.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE))
			{
				bool flag = true;
				if (gridTileLocation.structure is Cave { hasConnectedMine: not false })
				{
					flag = false;
				}
				else if (gridTileLocation.structure.settlementLocation != null && gridTileLocation.structure.settlementLocation.owner != null && gridTileLocation.structure.settlementLocation.owner != FactionManager.Instance.banditFaction)
				{
					flag = false;
				}
				if (flag)
				{
					list.Add(val);
				}
			}
		}
		T result = null;
		if (list.Count > 0)
		{
			result = list[Random.Range(0, list.Count)] as T;
		}
		RuinarchListPool<TileObject>.Release(list);
		return result;
	}

	public ResourcePile GetRandomResourcePile()
	{
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		for (int i = 0; i < resourcePiles.Count; i++)
		{
			ResourcePile resourcePile = resourcePiles[i];
			if (resourcePile.gridTileLocation != null && resourcePile.mapObjectState == MAP_OBJECT_STATE.BUILT && !resourcePile.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE) && resourcePile.isBeingCarriedBy == null && !resourcePile.isBeingSeized)
			{
				list.Add(resourcePile);
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

	public void AddHerbPlant(HerbPlant p_plant)
	{
		herbPlants.Add(p_plant);
	}

	public void RemoveHerbPlant(HerbPlant p_plant)
	{
		herbPlants.Remove(p_plant);
	}

	public HerbPlant GetFirstAvailableHerbPlant(Character p_worker)
	{
		for (int i = 0; i < herbPlants.Count; i++)
		{
			HerbPlant herbPlant = herbPlants[i];
			if (herbPlant.HasJobTargetingThis(JOB_TYPE.GATHER_HERB, JOB_TYPE.HAUL))
			{
				continue;
			}
			LocationStructure currentStructure = herbPlant.currentStructure;
			if (currentStructure == null || currentStructure.structureType != STRUCTURE_TYPE.HOSPICE)
			{
				LocationStructure currentStructure2 = herbPlant.currentStructure;
				if ((currentStructure2 == null || currentStructure2.structureType != STRUCTURE_TYPE.TAVERN) && (herbPlant.characterOwner == null || herbPlant.characterOwner == p_worker))
				{
					return herbPlant;
				}
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

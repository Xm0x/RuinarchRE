using System;
using System.Collections.Generic;
using System.Linq;
using UtilityScripts;

namespace Inner_Maps.Location_Structures;

public class Farm : ManMadeStructure
{
	public List<LocationGridTile> farmTiles { get; private set; }

	public override Type serializedData => typeof(SaveDataFarm);

	public override int maxWorkerCapacity => 1;

	public Farm(Region location)
		: base(STRUCTURE_TYPE.FARM, location)
	{
		base.wallsAreMadeOf = WALL_RESOURCE.Wood;
		farmTiles = new List<LocationGridTile>();
	}

	public Farm(Region location, SaveDataManMadeStructure data)
		: base(location, data)
	{
		base.wallsAreMadeOf = WALL_RESOURCE.Wood;
		farmTiles = new List<LocationGridTile>();
	}

	public override void LoadReferences(SaveDataLocationStructure saveDataLocationStructure)
	{
		base.LoadReferences(saveDataLocationStructure);
		SaveDataFarm saveDataFarm = saveDataLocationStructure as SaveDataFarm;
		for (int i = 0; i < saveDataFarm.farmTiles.Length; i++)
		{
			TileLocationSave tileLocationSave = saveDataFarm.farmTiles[i];
			LocationGridTile tileBySavedData = DatabaseManager.Instance.locationGridTileDatabase.GetTileBySavedData(tileLocationSave);
			farmTiles.Add(tileBySavedData);
		}
	}

	private void PopulateListOfFoodPilesOfSameType(List<TileObject> p_list)
	{
		PopulateListOfFoodPilesOfType(p_list, TILE_OBJECT_TYPE.CORN);
		if (p_list.Count > 1)
		{
			return;
		}
		p_list.Clear();
		PopulateListOfFoodPilesOfType(p_list, TILE_OBJECT_TYPE.PINEAPPLE);
		if (p_list.Count > 1)
		{
			return;
		}
		p_list.Clear();
		PopulateListOfFoodPilesOfType(p_list, TILE_OBJECT_TYPE.HYPNO_HERB);
		if (p_list.Count <= 1)
		{
			p_list.Clear();
			PopulateListOfFoodPilesOfType(p_list, TILE_OBJECT_TYPE.ICEBERRY);
			if (p_list.Count <= 1)
			{
				p_list.Clear();
				PopulateListOfFoodPilesOfType(p_list, TILE_OBJECT_TYPE.POTATO);
			}
		}
	}

	private void PopulateListOfFoodPilesOfType(List<TileObject> p_list, TILE_OBJECT_TYPE p_type)
	{
		List<TileObject> tileObjectsOfType = GetTileObjectsOfType(p_type);
		if (tileObjectsOfType == null || tileObjectsOfType.Count <= 1)
		{
			return;
		}
		for (int i = 0; i < tileObjectsOfType.Count; i++)
		{
			TileObject tileObject = tileObjectsOfType[i];
			if (tileObject.mapObjectState == MAP_OBJECT_STATE.BUILT && tileObject.characterOwner == null && !tileObject.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE))
			{
				p_list.Add(tileObject);
			}
		}
	}

	private GenericTileObject GetUntilledFarmTile()
	{
		for (int i = 0; i < farmTiles.Count; i++)
		{
			LocationGridTile locationGridTile = farmTiles[i];
			if (!CheckIfTileIsTilled(locationGridTile) && !locationGridTile.tileObjectComponent.genericTileObject.HasJobTargetingThis(JOB_TYPE.TILL_TILE))
			{
				return locationGridTile.tileObjectComponent.genericTileObject;
			}
		}
		return null;
	}

	private bool CheckIfTileIsTilled(LocationGridTile p_targetTile)
	{
		if (p_targetTile.tileObjectComponent.objHere == null)
		{
			return false;
		}
		if (p_targetTile.tileObjectComponent.objHere.tileObjectType != TILE_OBJECT_TYPE.CORN_CROP && p_targetTile.tileObjectComponent.objHere.tileObjectType != TILE_OBJECT_TYPE.HYPNO_HERB_CROP && p_targetTile.tileObjectComponent.objHere.tileObjectType != TILE_OBJECT_TYPE.ICEBERRY_CROP && p_targetTile.tileObjectComponent.objHere.tileObjectType != TILE_OBJECT_TYPE.PINEAPPLE_CROP)
		{
			return p_targetTile.tileObjectComponent.objHere.tileObjectType == TILE_OBJECT_TYPE.POTATO_CROP;
		}
		return true;
	}

	private TileObject GetHarvestableCrop()
	{
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		for (int i = 0; i < farmTiles.Count; i++)
		{
			LocationGridTile locationGridTile = farmTiles[i];
			if (CheckIfTileHasHarvestableCrop(locationGridTile))
			{
				list.Add(locationGridTile.tileObjectComponent.objHere);
			}
		}
		if (list.Count > 0)
		{
			TileObject randomElement = CollectionUtilities.GetRandomElement(list);
			RuinarchListPool<TileObject>.Release(list);
			return randomElement;
		}
		RuinarchListPool<TileObject>.Release(list);
		return null;
	}

	private bool CheckIfTileHasHarvestableCrop(LocationGridTile p_targetTile)
	{
		if (p_targetTile.tileObjectComponent.objHere == null)
		{
			return false;
		}
		if (!(p_targetTile.tileObjectComponent.objHere is Crops crops))
		{
			return false;
		}
		return crops.currentGrowthState == Crops.Growth_State.Ripe;
	}

	public void AddFarmTile(LocationGridTile p_tile)
	{
		farmTiles.Add(p_tile);
	}

	public override bool CanHireAWorker()
	{
		return !HasReachedMaxWorkerCapacity();
	}

	public override bool CanPurchaseFromHere(Character p_buyer, out bool needsToPay, out int buyerOpinionOfWorker)
	{
		needsToPay = true;
		buyerOpinionOfWorker = 0;
		return true;
	}

	protected override void ProcessWorkStructureJobsByWorker(Character p_worker, out JobQueueItem producedJob)
	{
		producedJob = null;
		ResourcePile randomPileOfCropsForFarmHaul = p_worker.homeSettlement.SettlementResources.GetRandomPileOfCropsForFarmHaul(p_worker.homeSettlement);
		if (randomPileOfCropsForFarmHaul != null && p_worker.structureComponent.workPlaceStructure.HasUnoccupiedTile())
		{
			p_worker.jobComponent.TryCreateHaulJob(randomPileOfCropsForFarmHaul, this, out producedJob);
			if (producedJob != null)
			{
				return;
			}
		}
		if (GameUtilities.RollChance(35))
		{
			GenericTileObject untilledFarmTile = GetUntilledFarmTile();
			if (untilledFarmTile != null)
			{
				p_worker.jobComponent.TriggerTillTile(untilledFarmTile, out producedJob);
				if (producedJob != null)
				{
					return;
				}
			}
		}
		if (!p_worker.behaviourComponent.HasTendedMaximumAmountOfCrops(p_worker))
		{
			Crops randomTileObjectsOfTypeThatIsUntended = GetRandomTileObjectsOfTypeThatIsUntended();
			if (randomTileObjectsOfTypeThatIsUntended != null)
			{
				p_worker.jobComponent.TriggerTendCrop(p_worker, randomTileObjectsOfTypeThatIsUntended, out producedJob);
				return;
			}
		}
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		PopulateListOfFoodPilesOfSameType(list);
		if (list.Count > 1)
		{
			p_worker.jobComponent.TryCreateCombineStockpile(list[1] as ResourcePile, list[0] as ResourcePile, out producedJob);
			if (producedJob != null)
			{
				RuinarchListPool<TileObject>.Release(list);
				return;
			}
		}
		RuinarchListPool<TileObject>.Release(list);
		if (p_worker.HasTalents() && p_worker.talentComponent.GetTalent(CHARACTER_TALENT.Food).level >= 3)
		{
			List<TileObject> list2 = RuinarchListPool<TileObject>.Claim();
			PopulateBuiltTileObjectsThatHaveTrait<Crops>(list2, "Poisoned");
			if (list2.Count > 0)
			{
				TileObject randomElement = CollectionUtilities.GetRandomElement(list2);
				if (p_worker.jobComponent.TriggerRemoveStatusTarget(randomElement, "Poisoned", out producedJob))
				{
					return;
				}
			}
		}
		int chance = 100;
		if (GetTotalResourceInStructure(RESOURCE.FOOD) >= 180)
		{
			chance = 35;
		}
		if (GameUtilities.RollChance(chance))
		{
			TileObject harvestableCrop = GetHarvestableCrop();
			if (harvestableCrop != null)
			{
				p_worker.jobComponent.TriggerHarvestCrops(harvestableCrop, out producedJob);
				if (producedJob != null)
				{
					return;
				}
			}
		}
		TryCreateCleanJob(p_worker, out producedJob);
	}

	public Crops GetRandomTileObjectsOfTypeThatIsUntended()
	{
		List<Crops> list = RuinarchListPool<Crops>.Claim();
		for (int i = 0; i < base.pointsOfInterest.Count; i++)
		{
			if (base.pointsOfInterest.ElementAt(i) is Crops crops && !crops.traitContainer.HasTrait("Tended") && crops.state == POI_STATE.ACTIVE)
			{
				list.Add(crops);
			}
		}
		if (list.Count > 0)
		{
			Crops randomElement = CollectionUtilities.GetRandomElement(list);
			RuinarchListPool<Crops>.Release(list);
			return randomElement;
		}
		return null;
	}

	public override void OnTileDamaged(LocationGridTile tile, int amount, bool isPlayerSource)
	{
		AdjustHP(amount, null, isPlayerSource);
		OnStructureDamaged();
	}

	public override bool DoesTileContributeToDamage(LocationGridTile tile)
	{
		return true;
	}

	public override string GetTestingInfo()
	{
		return string.Concat(base.GetTestingInfo() + "\nFarm Tiles: " + farmTiles.ComafyList(), "\nTotal Food: ", GetTotalResourceInStructure(RESOURCE.FOOD).ToString());
	}

	public override void CleanUp()
	{
		if (DatabaseManager.Instance.structureDatabase.HasStructure(base.persistentID))
		{
			farmTiles.Clear();
			base.CleanUp();
		}
	}
}

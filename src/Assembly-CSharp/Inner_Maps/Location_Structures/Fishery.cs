using System;
using System.Collections.Generic;
using UtilityScripts;

namespace Inner_Maps.Location_Structures;

public class Fishery : ManMadeStructure
{
	public override Type serializedData => typeof(SaveDataFishery);

	public override int maxWorkerCapacity => 1;

	public Ocean connectedOcean { get; private set; }

	public FishingSpot connectedFishingSpot { get; private set; }

	public LocationGridTile targetFishingLocation { get; private set; }

	public Fishery(Region location)
		: base(STRUCTURE_TYPE.FISHERY, location)
	{
		SetMaxHPAndReset(4000);
	}

	public Fishery(Region location, SaveDataManMadeStructure data)
		: base(location, data)
	{
		SetMaxHP(4000);
	}

	public override void LoadReferences(SaveDataLocationStructure saveDataLocationStructure)
	{
		base.LoadReferences(saveDataLocationStructure);
		SaveDataFishery saveDataFishery = saveDataLocationStructure as SaveDataFishery;
		if (!string.IsNullOrEmpty(saveDataFishery.connectedFishingShackID))
		{
			connectedOcean = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(saveDataFishery.connectedFishingShackID) as Ocean;
		}
		if (!string.IsNullOrEmpty(saveDataFishery.connectedFishingSpotID))
		{
			connectedFishingSpot = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(saveDataFishery.connectedFishingSpotID) as FishingSpot;
		}
		if (saveDataFishery.hasTargetFishingLocation)
		{
			Point point = saveDataFishery.targetFishingLocation;
			LocationGridTile locationGridTile = base.region.innerMap.map[point.X, point.Y];
			targetFishingLocation = locationGridTile;
		}
		else
		{
			if (connectedFishingSpot == null || connectedFishingSpot.gridTileLocation == null)
			{
				return;
			}
			List<LocationGridTile> list = connectedFishingSpot.gridTileLocation.FourNeighbours();
			for (int i = 0; i < list.Count; i++)
			{
				LocationGridTile locationGridTile2 = list[i];
				if (locationGridTile2.structure == this)
				{
					targetFishingLocation = locationGridTile2;
					break;
				}
			}
		}
	}

	public override bool CanHireAWorker()
	{
		return !HasReachedMaxWorkerCapacity();
	}

	protected override bool HasSettlementOrLocalResidentThatCanWorkHereBase()
	{
		if (base.settlementLocation is NPCSettlement nPCSettlement)
		{
			return nPCSettlement.HasResidentThatCanWorkAt(STRUCTURE_TYPE.FISHERY);
		}
		for (int i = 0; i < base.residents.Count; i++)
		{
			Character character = base.residents[i];
			if (!character.structureComponent.HasWorkPlaceStructure() && CharacterManager.Instance.CanCharacterWorkAt(character, STRUCTURE_TYPE.FISHERY))
			{
				return true;
			}
		}
		return false;
	}

	public override bool CanPurchaseFromHere(Character p_buyer, out bool needsToPay, out int buyerOpinionOfWorker)
	{
		needsToPay = true;
		buyerOpinionOfWorker = 0;
		return true;
	}

	public override string GetTestingInfo()
	{
		return base.GetTestingInfo() + "\nConnected Ocean " + connectedOcean?.name + "\nTarget Fishing Location: " + targetFishingLocation;
	}

	public override void OnUseStructureConnector(LocationGridTile p_usedConnector)
	{
		base.OnUseStructureConnector(p_usedConnector);
		connectedOcean = p_usedConnector.structure as Ocean;
		FishingSpot fishingSpot = (connectedFishingSpot = p_usedConnector.tileObjectComponent.objHere as FishingSpot);
		List<LocationGridTile> list = p_usedConnector.FourNeighbours();
		for (int i = 0; i < list.Count; i++)
		{
			LocationGridTile locationGridTile = list[i];
			if (locationGridTile.structure == this)
			{
				targetFishingLocation = locationGridTile;
				break;
			}
		}
		fishingSpot.SetConnectedFishingShack(this);
	}

	protected override void AfterStructureDestruction(Character p_responsibleCharacter = null)
	{
		base.AfterStructureDestruction(p_responsibleCharacter);
		connectedFishingSpot?.SetConnectedFishingShack(null);
		connectedOcean = null;
		connectedFishingSpot = null;
	}

	private void PopulateFishPileListInsideStructureForCombine(List<TileObject> builtPilesInSideStructure)
	{
		List<TileObject> tileObjectsOfType = GetTileObjectsOfType(TILE_OBJECT_TYPE.FISH_PILE);
		if (tileObjectsOfType == null)
		{
			return;
		}
		for (int i = 0; i < tileObjectsOfType.Count; i++)
		{
			TileObject tileObject = tileObjectsOfType[i];
			if (tileObject.mapObjectState == MAP_OBJECT_STATE.BUILT && tileObject.characterOwner == null && !tileObject.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE))
			{
				builtPilesInSideStructure.Add(tileObject);
			}
		}
	}

	private TileObject GetTargetFishingSpot(Character p_actor)
	{
		if (connectedFishingSpot != null && connectedFishingSpot.gridTileLocation != null && connectedFishingSpot.IsAvailable() && targetFishingLocation != null && p_actor.movementComponent.HasPathToEvenIfDiffRegion(targetFishingLocation))
		{
			return connectedFishingSpot;
		}
		List<TileObject> list = connectedOcean?.GetTileObjectsOfType(TILE_OBJECT_TYPE.FISHING_SPOT);
		if (list != null && list.Count > 0)
		{
			List<TileObject> list2 = RuinarchListPool<TileObject>.Claim();
			for (int i = 0; i < list.Count; i++)
			{
				TileObject tileObject = list[i];
				if (tileObject != connectedFishingSpot && tileObject.gridTileLocation != null && tileObject.gridTileLocation.area.HasSettlementOnArea(base.settlementLocation) && tileObject.IsAvailable())
				{
					list2.Add(tileObject);
				}
			}
			if (list2.Count > 0)
			{
				TileObject randomElement = CollectionUtilities.GetRandomElement(list2);
				RuinarchListPool<TileObject>.Release(list2);
				return randomElement;
			}
			RuinarchListPool<TileObject>.Release(list2);
		}
		return null;
	}

	protected override void ProcessWorkStructureJobsByWorker(Character p_worker, out JobQueueItem producedJob)
	{
		producedJob = null;
		ResourcePile randomPileOfFishesForFisheryHaul = p_worker.homeSettlement.SettlementResources.GetRandomPileOfFishesForFisheryHaul(p_worker.homeSettlement);
		if (randomPileOfFishesForFisheryHaul != null && p_worker.structureComponent.workPlaceStructure.HasUnoccupiedTile())
		{
			p_worker.jobComponent.TryCreateHaulJob(randomPileOfFishesForFisheryHaul, this, out producedJob);
			if (producedJob != null)
			{
				return;
			}
		}
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		PopulateFishPileListInsideStructureForCombine(list);
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
		if (!TryCreateCleanJob(p_worker, out producedJob))
		{
			TileObject targetFishingSpot = GetTargetFishingSpot(p_worker);
			if (targetFishingSpot != null)
			{
				p_worker.jobComponent.TriggerFindFish(targetFishingSpot as FishingSpot, out producedJob);
			}
		}
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

	public override void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		base.CheckIfStructureIsStillReferenced(p_structure);
		_ = connectedOcean;
	}
}

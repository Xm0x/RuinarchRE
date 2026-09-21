using System;
using System.Collections.Generic;
using UtilityScripts;

namespace Inner_Maps.Location_Structures;

public class Mine : ManMadeStructure
{
	public Cave connectedCave { get; private set; }

	public override Type serializedData => typeof(SaveDataMine);

	public override int maxWorkerCapacity => 3;

	public Mine(Region location)
		: base(STRUCTURE_TYPE.MINE, location)
	{
		SetMaxHPAndReset(4000);
	}

	public Mine(Region location, SaveDataManMadeStructure data)
		: base(location, data)
	{
		SetMaxHP(4000);
	}

	public override void LoadReferences(SaveDataLocationStructure saveDataLocationStructure)
	{
		base.LoadReferences(saveDataLocationStructure);
		SaveDataMine saveDataMine = saveDataLocationStructure as SaveDataMine;
		if (!string.IsNullOrEmpty(saveDataMine.connectedCaveID))
		{
			connectedCave = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(saveDataMine.connectedCaveID) as Cave;
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

	public override string GetTestingInfo()
	{
		return base.GetTestingInfo() + "\nConnected Cave " + connectedCave?.name;
	}

	public override void OnUseStructureConnector(LocationGridTile p_usedConnector)
	{
		base.OnUseStructureConnector(p_usedConnector);
		connectedCave = p_usedConnector.structure as Cave;
		connectedCave.ConnectMine(this);
		_ = p_usedConnector.area;
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		List<LocationGridTile> list2 = RuinarchListPool<LocationGridTile>.Claim();
		p_usedConnector.PopulateTilesInRadius(list, 10, 0, includeCenterTile: false, includeTilesInDifferentStructure: true);
		for (int i = 0; i < list.Count; i++)
		{
			LocationGridTile locationGridTile = list[i];
			if (locationGridTile.IsPassable() && locationGridTile.structure == connectedCave)
			{
				list2.Add(locationGridTile);
			}
		}
		RuinarchListPool<LocationGridTile>.Release(list);
		LocationGridTile locationGridTile2 = null;
		if (list2.Count > 0)
		{
			locationGridTile2 = CollectionUtilities.GetRandomElement(list2);
		}
		else if (connectedCave.passableTiles.Count > 0)
		{
			locationGridTile2 = CollectionUtilities.GetRandomElement(connectedCave.passableTiles);
		}
		else
		{
			if (connectedCave.tiles.Count <= 0)
			{
				throw new Exception("No random passable tile");
			}
			locationGridTile2 = CollectionUtilities.GetRandomElement(connectedCave.tiles);
		}
		RuinarchListPool<LocationGridTile>.Release(list2);
		List<LocationGridTile> path = PathGenerator.Instance.GetPath(p_usedConnector, locationGridTile2, GRID_PATHFINDING_MODE.CAVE_INTERCONNECTION);
		if (path == null)
		{
			return;
		}
		path.Add(p_usedConnector);
		for (int j = 0; j < path.Count; j++)
		{
			LocationGridTile locationGridTile3 = path[j];
			if (locationGridTile3.tileObjectComponent.objHere is BlockWall || locationGridTile3.tileObjectComponent.objHere is OreVein)
			{
				locationGridTile3.structure.RemovePOI(locationGridTile3.tileObjectComponent.objHere);
				if (!GameManager.Instance.gameHasStarted && WorldConfigManager.Instance.mapGenerationData != null)
				{
					WorldConfigManager.Instance.mapGenerationData.SetGeneratedMapPerlinDetails(locationGridTile3, TILE_OBJECT_TYPE.NONE);
				}
			}
			else if (!GameManager.Instance.gameHasStarted && WorldConfigManager.Instance.mapGenerationData != null)
			{
				TILE_OBJECT_TYPE generatedObjectOnTile = WorldConfigManager.Instance.mapGenerationData.GetGeneratedObjectOnTile(locationGridTile3);
				if (generatedObjectOnTile == TILE_OBJECT_TYPE.BLOCK_WALL || generatedObjectOnTile == TILE_OBJECT_TYPE.ORE_VEIN)
				{
					WorldConfigManager.Instance.mapGenerationData.SetGeneratedMapPerlinDetails(locationGridTile3, TILE_OBJECT_TYPE.NONE);
					locationGridTile3.SetStructureTilemapVisual(null);
				}
			}
		}
	}

	protected override void DestroyStructure(Character p_responsibleCharacter = null, bool isPlayerSource = false, bool shouldBeCleanedUp = true)
	{
		base.DestroyStructure(p_responsibleCharacter, isPlayerSource, shouldBeCleanedUp: false);
		connectedCave.DisconnectMine(this);
		connectedCave = null;
		if (shouldBeCleanedUp)
		{
			MarkForCleanup();
		}
	}

	private void PopulateList(List<TileObject> p_list, TILE_OBJECT_TYPE p_type)
	{
		p_list.Clear();
		List<TileObject> tileObjectsOfType = GetTileObjectsOfType(p_type);
		if (tileObjectsOfType == null)
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

	private void SetListToVariable(List<TileObject> builtPilesInSideStructure)
	{
		PopulateList(builtPilesInSideStructure, TILE_OBJECT_TYPE.COPPER);
		if (builtPilesInSideStructure.Count > 1)
		{
			return;
		}
		PopulateList(builtPilesInSideStructure, TILE_OBJECT_TYPE.IRON);
		if (builtPilesInSideStructure.Count > 1)
		{
			return;
		}
		PopulateList(builtPilesInSideStructure, TILE_OBJECT_TYPE.ORICHALCUM);
		if (builtPilesInSideStructure.Count <= 1)
		{
			PopulateList(builtPilesInSideStructure, TILE_OBJECT_TYPE.MITHRIL);
			if (builtPilesInSideStructure.Count <= 1)
			{
				PopulateList(builtPilesInSideStructure, TILE_OBJECT_TYPE.STONE_PILE);
				_ = builtPilesInSideStructure.Count;
				_ = 1;
			}
		}
	}

	protected override void ProcessWorkStructureJobsByWorker(Character p_worker, out JobQueueItem producedJob)
	{
		producedJob = null;
		ResourcePile resourcePile = p_worker.homeSettlement.SettlementResources.GetRandomPileOfMetalOrStoneForMineHaul(p_worker.homeSettlement);
		if (resourcePile == null && connectedCave != null && !p_worker.movementComponent.structuresToAvoid.Contains(connectedCave))
		{
			resourcePile = connectedCave.GetRandomTileObjectOfTypeThatHasTileLocationAndIsBuiltAndIsNotTargetedByHaulOrCombine<StonePile>();
			if (resourcePile == null)
			{
				resourcePile = connectedCave.GetRandomTileObjectOfTypeThatHasTileLocationAndIsBuiltAndIsNotTargetedByHaulOrCombine<MetalPile>();
			}
		}
		if (resourcePile != null && p_worker.structureComponent.workPlaceStructure.HasUnoccupiedTile())
		{
			p_worker.jobComponent.TryCreateHaulJob(resourcePile, this, out producedJob);
			if (producedJob != null)
			{
				return;
			}
		}
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		SetListToVariable(list);
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
		if (!p_worker.movementComponent.structuresToAvoid.Contains(connectedCave))
		{
			List<TileObject> list2 = RuinarchListPool<TileObject>.Claim();
			if (p_worker.TryGetTalentLevel(CHARACTER_TALENT.Resources) >= 4)
			{
				PopulateMetalsIncave(list2);
				PopulateStonesInCave(list2);
				if (list2.Count > 0)
				{
					TileObject tileObject = list2[GameUtilities.RandomBetweenTwoNumbers(0, list2.Count - 1)];
					if (tileObject.tileObjectType == TILE_OBJECT_TYPE.ORE)
					{
						p_worker.jobComponent.TriggerMineOre(tileObject, out producedJob);
					}
					else
					{
						p_worker.jobComponent.TriggerMineStone(tileObject, out producedJob);
					}
					if (producedJob != null)
					{
						RuinarchListPool<TileObject>.Release(list2);
						return;
					}
				}
			}
			list2.Clear();
			PopulateStonesInCave(list2);
			if (list2.Count > 0)
			{
				TileObject p_tileObject = list2[GameUtilities.RandomBetweenTwoNumbers(0, list2.Count - 1)];
				p_worker.jobComponent.TriggerMineStone(p_tileObject, out producedJob);
				if (producedJob != null)
				{
					RuinarchListPool<TileObject>.Release(list2);
					return;
				}
			}
			RuinarchListPool<TileObject>.Release(list2);
		}
		TryCreateCleanJob(p_worker, out producedJob);
	}

	public void PopulateMetalsIncave(List<TileObject> availMetals)
	{
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		connectedCave.PopulateTileObjectsOfType<Ore>(list);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].mapObjectState == MAP_OBJECT_STATE.BUILT)
			{
				availMetals.Add(list[i]);
			}
		}
		RuinarchListPool<TileObject>.Release(list);
	}

	public void PopulateStonesInCave(List<TileObject> availStones)
	{
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		connectedCave.PopulateTileObjectsOfType<Rock>(list);
		for (int i = 0; i < list.Count; i++)
		{
			TileObject tileObject = list[i];
			if (tileObject.mapObjectState == MAP_OBJECT_STATE.BUILT)
			{
				availStones.Add(tileObject);
			}
		}
		RuinarchListPool<TileObject>.Release(list);
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

	public override void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		base.CheckIfStructureIsStillReferenced(p_structure);
		_ = connectedCave;
	}
}

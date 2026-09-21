using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

namespace Inner_Maps.Location_Structures;

public class Lumberyard : ManMadeStructure
{
	public override int maxWorkerCapacity => 3;

	public override Vector3 worldPosition
	{
		get
		{
			Vector3 position = base.structureObj.transform.position;
			position.x -= 0.5f;
			position.y -= 0.5f;
			return position;
		}
	}

	public Lumberyard(Region location)
		: base(STRUCTURE_TYPE.LUMBERYARD, location)
	{
		SetMaxHPAndReset(8000);
	}

	public Lumberyard(Region location, SaveDataManMadeStructure data)
		: base(location, data)
	{
		SetMaxHP(8000);
	}

	private void PopulateWoodPileListInsideStructureForCombining(List<TileObject> builtPilesInSideStructure)
	{
		List<TileObject> tileObjectsOfType = GetTileObjectsOfType(TILE_OBJECT_TYPE.WOOD_PILE);
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

	private TileObject GetFirstTree()
	{
		for (int i = 0; i < base.occupiedArea.tileObjectComponent.itemsInArea.Count; i++)
		{
			if (base.occupiedArea.tileObjectComponent.itemsInArea[i] is TreeObject)
			{
				return base.occupiedArea.tileObjectComponent.itemsInArea[i];
			}
		}
		return null;
	}

	protected override void ProcessWorkStructureJobsByWorker(Character p_worker, out JobQueueItem producedJob)
	{
		producedJob = null;
		ResourcePile randomPileOfWoodsForLumberyardHaul = p_worker.homeSettlement.SettlementResources.GetRandomPileOfWoodsForLumberyardHaul(p_worker.homeSettlement);
		if (randomPileOfWoodsForLumberyardHaul != null)
		{
			if (p_worker.structureComponent.workPlaceStructure.HasUnoccupiedTile())
			{
				p_worker.jobComponent.TryCreateHaulJob(randomPileOfWoodsForLumberyardHaul, this, out producedJob);
				if (producedJob != null)
				{
					return;
				}
			}
			else
			{
				ResourcePile firstBuiltTileObjectOfType = GetFirstBuiltTileObjectOfType<ResourcePile>(TILE_OBJECT_TYPE.WOOD_PILE, randomPileOfWoodsForLumberyardHaul);
				if (firstBuiltTileObjectOfType != null)
				{
					p_worker.jobComponent.TryCreateCombineStockpile(randomPileOfWoodsForLumberyardHaul, firstBuiltTileObjectOfType, out producedJob);
					if (producedJob != null)
					{
						return;
					}
				}
			}
		}
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		PopulateWoodPileListInsideStructureForCombining(list);
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
		if (p_worker.structureComponent.workPlaceStructure.HasUnoccupiedTile())
		{
			TileObject firstTree = GetFirstTree();
			if (firstTree != null)
			{
				p_worker.jobComponent.TriggerChopWood(firstTree, out producedJob);
				if (producedJob != null)
				{
					return;
				}
			}
		}
		TryCreateCleanJob(p_worker, out producedJob);
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
}

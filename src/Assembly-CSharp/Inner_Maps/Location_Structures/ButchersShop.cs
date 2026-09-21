using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

namespace Inner_Maps.Location_Structures;

public class ButchersShop : ManMadeStructure
{
	public override Vector3 worldPosition => base.structureObj.transform.position;

	public override int maxWorkerCapacity => 1;

	public ButchersShop(Region location)
		: base(STRUCTURE_TYPE.BUTCHERS_SHOP, location)
	{
		SetMaxHPAndReset(4000);
	}

	public ButchersShop(Region location, SaveDataManMadeStructure data)
		: base(location, data)
	{
		SetMaxHP(4000);
	}

	private void SetListToVariable(List<TileObject> builtPilesInSideStructure)
	{
		PopulateMeatList(builtPilesInSideStructure, TILE_OBJECT_TYPE.ANIMAL_MEAT);
		if (builtPilesInSideStructure.Count > 1)
		{
			return;
		}
		builtPilesInSideStructure.Clear();
		PopulateMeatList(builtPilesInSideStructure, TILE_OBJECT_TYPE.HUMAN_MEAT);
		if (builtPilesInSideStructure.Count <= 1)
		{
			builtPilesInSideStructure.Clear();
			PopulateMeatList(builtPilesInSideStructure, TILE_OBJECT_TYPE.ELF_MEAT);
			if (builtPilesInSideStructure.Count <= 1)
			{
				builtPilesInSideStructure.Clear();
				PopulateMeatList(builtPilesInSideStructure, TILE_OBJECT_TYPE.RAT_MEAT);
				_ = builtPilesInSideStructure.Count;
				_ = 1;
			}
		}
	}

	private void PopulateMeatList(List<TileObject> p_list, TILE_OBJECT_TYPE p_type)
	{
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

	public override bool CanHireAWorker()
	{
		return !HasReachedMaxWorkerCapacity();
	}

	protected override bool HasSettlementOrLocalResidentThatCanWorkHereBase()
	{
		if (base.settlementLocation is NPCSettlement nPCSettlement)
		{
			return nPCSettlement.HasResidentThatCanWorkAt(STRUCTURE_TYPE.BUTCHERS_SHOP);
		}
		for (int i = 0; i < base.residents.Count; i++)
		{
			Character character = base.residents[i];
			if (!character.structureComponent.HasWorkPlaceStructure() && CharacterManager.Instance.CanCharacterWorkAt(character, STRUCTURE_TYPE.BUTCHERS_SHOP))
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

	protected override void ProcessWorkStructureJobsByWorker(Character p_worker, out JobQueueItem producedJob)
	{
		producedJob = null;
		ResourcePile randomPileOfMeatsForButchersShopHaul = p_worker.homeSettlement.SettlementResources.GetRandomPileOfMeatsForButchersShopHaul(p_worker.homeSettlement, p_worker);
		if (randomPileOfMeatsForButchersShopHaul != null && p_worker.structureComponent.workPlaceStructure.HasUnoccupiedTile())
		{
			p_worker.jobComponent.TryCreateHaulJob(randomPileOfMeatsForButchersShopHaul, this, out producedJob);
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
		Summon firstButcherableAnimal = p_worker.homeSettlement.SettlementResources.GetFirstButcherableAnimal(p_worker.homeSettlement);
		if (firstButcherableAnimal != null)
		{
			p_worker.jobComponent.CreateButcherJob(firstButcherableAnimal, JOB_TYPE.BUTCHER, out producedJob);
			if (producedJob != null)
			{
				return;
			}
		}
		TryCreateCleanJob(p_worker, out producedJob);
	}
}

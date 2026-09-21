using System.Collections.Generic;
using UtilityScripts;

namespace Inner_Maps.Location_Structures;

public class HunterLodge : ManMadeStructure
{
	public HunterLodge(Region location)
		: base(STRUCTURE_TYPE.HUNTER_LODGE, location)
	{
		SetMaxHPAndReset(3000);
	}

	public HunterLodge(Region location, SaveDataManMadeStructure data)
		: base(location, data)
	{
		SetMaxHP(3000);
	}

	private void PopulateClothAndLeatherList(List<TileObject> p_list, TILE_OBJECT_TYPE p_type)
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

	private void SetListToVariable(List<TileObject> builtPilesInSideStructure)
	{
		PopulateClothAndLeatherList(builtPilesInSideStructure, TILE_OBJECT_TYPE.MINK_CLOTH);
		if (builtPilesInSideStructure.Count > 1)
		{
			return;
		}
		builtPilesInSideStructure.Clear();
		PopulateClothAndLeatherList(builtPilesInSideStructure, TILE_OBJECT_TYPE.MOONCRAWLER_CLOTH);
		if (builtPilesInSideStructure.Count > 1)
		{
			return;
		}
		builtPilesInSideStructure.Clear();
		PopulateClothAndLeatherList(builtPilesInSideStructure, TILE_OBJECT_TYPE.RABBIT_CLOTH);
		if (builtPilesInSideStructure.Count > 1)
		{
			return;
		}
		builtPilesInSideStructure.Clear();
		PopulateClothAndLeatherList(builtPilesInSideStructure, TILE_OBJECT_TYPE.BEAR_HIDE);
		if (builtPilesInSideStructure.Count > 1)
		{
			return;
		}
		builtPilesInSideStructure.Clear();
		PopulateClothAndLeatherList(builtPilesInSideStructure, TILE_OBJECT_TYPE.BOAR_HIDE);
		if (builtPilesInSideStructure.Count > 1)
		{
			return;
		}
		builtPilesInSideStructure.Clear();
		PopulateClothAndLeatherList(builtPilesInSideStructure, TILE_OBJECT_TYPE.DRAGON_HIDE);
		if (builtPilesInSideStructure.Count <= 1)
		{
			builtPilesInSideStructure.Clear();
			PopulateClothAndLeatherList(builtPilesInSideStructure, TILE_OBJECT_TYPE.SCALE_HIDE);
			if (builtPilesInSideStructure.Count <= 1)
			{
				builtPilesInSideStructure.Clear();
				PopulateClothAndLeatherList(builtPilesInSideStructure, TILE_OBJECT_TYPE.WOLF_HIDE);
				_ = builtPilesInSideStructure.Count;
				_ = 1;
			}
		}
	}

	protected override void ProcessWorkStructureJobsByWorker(Character p_worker, out JobQueueItem producedJob)
	{
		producedJob = null;
		ResourcePile randomPileOfClothOrLeatherForSkinnersLodgeHaul = p_worker.homeSettlement.SettlementResources.GetRandomPileOfClothOrLeatherForSkinnersLodgeHaul(p_worker.homeSettlement);
		if (randomPileOfClothOrLeatherForSkinnersLodgeHaul != null && p_worker.structureComponent.workPlaceStructure.HasUnoccupiedTile())
		{
			p_worker.jobComponent.TryCreateHaulJob(randomPileOfClothOrLeatherForSkinnersLodgeHaul, this, out producedJob);
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
		List<Character> list2 = RuinarchListPool<Character>.Claim();
		p_worker.homeSettlement.SettlementResources.PopulateAllAnimalsForSkinnersLodgeSkinning(list2, p_worker.homeSettlement, this);
		Character randomElement = CollectionUtilities.GetRandomElement(list2);
		if (randomElement != null)
		{
			p_worker.jobComponent.TriggerSkinAnimal(randomElement, out producedJob);
			if (producedJob != null)
			{
				RuinarchListPool<Character>.Release(list2);
				return;
			}
		}
		RuinarchListPool<Character>.Release(list2);
		list2 = RuinarchListPool<Character>.Claim();
		p_worker.homeSettlement.SettlementResources.PopulateAllAnimalsForSkinnersLodgeShearing(list2, p_worker.homeSettlement);
		randomElement = CollectionUtilities.GetRandomElement(list2);
		if (randomElement != null)
		{
			if (randomElement is Animal)
			{
				p_worker.jobComponent.TriggerShearAnimal(randomElement, out producedJob);
			}
			if (producedJob != null)
			{
				RuinarchListPool<Character>.Release(list2);
				return;
			}
		}
		RuinarchListPool<Character>.Release(list2);
		TryCreateCleanJob(p_worker, out producedJob);
	}

	protected override void AfterStructureDestruction(Character p_responsibleCharacter = null)
	{
		base.AfterStructureDestruction(p_responsibleCharacter);
		Messenger.Broadcast(CharacterSignals.TRY_CREATE_BURY_JOBS, base.settlementLocation);
	}

	public override void OnBuiltNewStructure()
	{
		base.OnBuiltNewStructure();
		Messenger.Broadcast(JobSignals.CHECK_JOB_APPLICABILITY_OF_ALL_JOBS_OF_TYPE, JOB_TYPE.BURY);
	}

	public override bool CanHireAWorker()
	{
		return !HasAssignedWorker();
	}

	protected override bool HasSettlementOrLocalResidentThatCanWorkHereBase()
	{
		if (base.settlementLocation is NPCSettlement nPCSettlement)
		{
			return nPCSettlement.HasResidentThatCanWorkAt(STRUCTURE_TYPE.HUNTER_LODGE);
		}
		for (int i = 0; i < base.residents.Count; i++)
		{
			Character character = base.residents[i];
			if (!character.structureComponent.HasWorkPlaceStructure() && CharacterManager.Instance.CanCharacterWorkAt(character, STRUCTURE_TYPE.HUNTER_LODGE))
			{
				return true;
			}
		}
		return false;
	}

	public override bool CanPurchaseFromHere(Character p_buyer, out bool needsToPay, out int buyerOpinionOfWorker)
	{
		return DefaultCanPurchaseFromHereForSingleWorkerStructures(p_buyer, out needsToPay, out buyerOpinionOfWorker);
	}
}

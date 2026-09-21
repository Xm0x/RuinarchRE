using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilityScripts;

namespace Inner_Maps.Location_Structures;

public class Workshop : ManMadeStructure
{
	public List<WorkShopRequestForm> requests = new List<WorkShopRequestForm>();

	private List<TileObject> m_metals = new List<TileObject>();

	private List<TileObject> m_stones = new List<TileObject>();

	private List<TileObject> m_cloth = new List<TileObject>();

	private List<TileObject> m_leather = new List<TileObject>();

	private List<TileObject> m_woods = new List<TileObject>();

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

	public override Type serializedData => typeof(SaveDataWorkshop);

	public Workshop(Region location)
		: base(STRUCTURE_TYPE.WORKSHOP, location)
	{
		SetMaxHPAndReset(8000);
	}

	public Workshop(Region location, SaveDataManMadeStructure data)
		: base(location, data)
	{
		SetMaxHP(8000);
	}

	public override void LoadReferences(SaveDataLocationStructure saveDataLocationStructure)
	{
		base.LoadReferences(saveDataLocationStructure);
		SaveDataWorkshop saveDataWorkshop = saveDataLocationStructure as SaveDataWorkshop;
		if (saveDataWorkshop.requestForms == null)
		{
			return;
		}
		for (int i = 0; i < saveDataWorkshop.requestForms.Length; i++)
		{
			SaveDataWorkShopRequestForm saveDataWorkShopRequestForm = saveDataWorkshop.requestForms[i];
			WorkShopRequestForm workShopRequestForm = saveDataWorkShopRequestForm.Load();
			Character characterByPersistentID = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(saveDataWorkShopRequestForm.requestingCharacterID);
			if (characterByPersistentID != null)
			{
				workShopRequestForm.requestingCharacter = characterByPersistentID;
			}
			requests.Add(workShopRequestForm);
		}
	}

	protected override void SubscribeListeners(bool shouldLock)
	{
		base.SubscribeListeners(shouldLock);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied, shouldLock);
		Messenger.AddListener<Character, EquipmentItem>(CharacterSignals.CHARACTER_EQUIPPED_ITEM, OnCharacterEquippedItem, shouldLock);
	}

	protected override void UnsubscribeListeners()
	{
		base.UnsubscribeListeners();
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
		Messenger.RemoveListener<Character, EquipmentItem>(CharacterSignals.CHARACTER_EQUIPPED_ITEM, OnCharacterEquippedItem);
	}

	private void OnCharacterEquippedItem(Character p_character, EquipmentItem p_equipment)
	{
		if (IsCharacterAlreadyHasRequest(p_character))
		{
			EQUIPMENT_TYPE equipmentType = EQUIPMENT_TYPE.WEAPON;
			if (p_equipment is WeaponItem)
			{
				equipmentType = EQUIPMENT_TYPE.WEAPON;
			}
			else if (p_equipment is ArmorItem)
			{
				equipmentType = EQUIPMENT_TYPE.ARMOR;
			}
			else if (p_equipment is AccessoryItem)
			{
				equipmentType = EQUIPMENT_TYPE.ACCESSORY;
			}
			RemoveAllRequestFromCharacter(p_character, equipmentType);
		}
	}

	private void OnCharacterDied(Character p_Character)
	{
		RemoveAllRequestFromCharacter(p_Character);
	}

	public void PostRequest(WorkShopRequestForm p_requestForm)
	{
		requests.Add(p_requestForm);
	}

	public bool IsCharacterAlreadyHasRequest(Character p_requestor)
	{
		for (int i = 0; i < requests.Count; i++)
		{
			if (requests[i].requestingCharacter == p_requestor)
			{
				return true;
			}
		}
		return false;
	}

	public void RemoveAllRequestFromCharacter(Character p_character)
	{
		requests.RemoveAll((WorkShopRequestForm item) => item.requestingCharacter == p_character);
	}

	private void RemoveAllRequestFromCharacter(Character p_character, EQUIPMENT_TYPE equipmentType)
	{
		List<WorkShopRequestForm> list = RuinarchListPool<WorkShopRequestForm>.Claim();
		list.AddRange(requests);
		for (int i = 0; i < list.Count; i++)
		{
			WorkShopRequestForm workShopRequestForm = list[i];
			if (workShopRequestForm.requestingCharacter == p_character && workShopRequestForm.equipmentType == equipmentType)
			{
				requests.Remove(workShopRequestForm);
			}
		}
		RuinarchListPool<WorkShopRequestForm>.Release(list);
	}

	private void EvaluateRequests()
	{
		requests.RemoveAll((WorkShopRequestForm item) => item.isSubjectForRemoval);
	}

	protected override void AfterStructureDestruction(Character p_responsibleCharacter = null)
	{
		base.AfterStructureDestruction(p_responsibleCharacter);
		requests.Clear();
	}

	public void RemoveFirstRequestThatIsFulfilledBy(TileObject p_object)
	{
		for (int i = 0; i < requests.Count; i++)
		{
			WorkShopRequestForm workShopRequestForm = requests[i];
			if (p_object is WeaponItem && workShopRequestForm.equipmentType == EQUIPMENT_TYPE.WEAPON)
			{
				if (workShopRequestForm.requestingCharacter.characterClass.craftableWeapons.Contains(p_object.tileObjectType))
				{
					requests.RemoveAt(i);
					break;
				}
			}
			else if (p_object is ArmorItem && workShopRequestForm.equipmentType == EQUIPMENT_TYPE.ARMOR)
			{
				if (workShopRequestForm.requestingCharacter.characterClass.craftableArmors.Contains(p_object.tileObjectType))
				{
					requests.RemoveAt(i);
					break;
				}
			}
			else if (p_object is AccessoryItem && workShopRequestForm.equipmentType == EQUIPMENT_TYPE.ACCESSORY && workShopRequestForm.requestingCharacter.characterClass.craftableAccessories.Contains(p_object.tileObjectType))
			{
				requests.RemoveAt(i);
				break;
			}
		}
	}

	private TILE_OBJECT_TYPE GetNonLegendaryEquipmentToMakeFromRequestList()
	{
		TILE_OBJECT_TYPE tILE_OBJECT_TYPE = TILE_OBJECT_TYPE.NONE;
		for (int i = 0; i < requests.Count; i++)
		{
			WorkShopRequestForm workShopRequestForm = requests[i];
			if (workShopRequestForm.requestingCharacter != null && !workShopRequestForm.requestingCharacter.isDead)
			{
				CharacterClass characterClass = workShopRequestForm.requestingCharacter.characterClass;
				List<TILE_OBJECT_TYPE> list = null;
				switch (workShopRequestForm.equipmentType)
				{
				case EQUIPMENT_TYPE.WEAPON:
					list = characterClass.craftableWeapons;
					break;
				case EQUIPMENT_TYPE.ARMOR:
					list = characterClass.craftableArmors;
					break;
				case EQUIPMENT_TYPE.ACCESSORY:
					list = characterClass.craftableAccessories;
					break;
				}
				if (list != null)
				{
					for (int num = list.Count - 1; num >= 0; num--)
					{
						TILE_OBJECT_TYPE tILE_OBJECT_TYPE2 = list[num];
						if (!EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(tILE_OBJECT_TYPE2.ToStringEnumWithSpace()).isLegendary)
						{
							List<CONCRETE_RESOURCES> resourcesNeeded = EquipmentDataHandler.Instance.GetResourcesNeeded(tILE_OBJECT_TYPE2);
							RESOURCE generalResourcesNeeded = EquipmentDataHandler.Instance.GetGeneralResourcesNeeded(tILE_OBJECT_TYPE2);
							int resourcesNeededAmount = EquipmentDataHandler.Instance.GetResourcesNeededAmount(tILE_OBJECT_TYPE2);
							if (resourcesNeeded != null && resourcesNeeded.Count > 0)
							{
								if (CanBeCrafted(resourcesNeeded, resourcesNeededAmount, out var _))
								{
									tILE_OBJECT_TYPE = tILE_OBJECT_TYPE2;
									break;
								}
								workShopRequestForm.isSubjectForRemoval = true;
							}
							else if (generalResourcesNeeded != RESOURCE.NONE)
							{
								if (CanBeCrafted(generalResourcesNeeded, resourcesNeededAmount))
								{
									tILE_OBJECT_TYPE = tILE_OBJECT_TYPE2;
									break;
								}
								workShopRequestForm.isSubjectForRemoval = true;
							}
						}
					}
				}
			}
			if (tILE_OBJECT_TYPE != TILE_OBJECT_TYPE.NONE)
			{
				break;
			}
		}
		EvaluateRequests();
		return tILE_OBJECT_TYPE;
	}

	private EquipmentItem GetFirstEquipmentThatIsUnfinished()
	{
		for (int i = 0; i < base.pointsOfInterest.Count; i++)
		{
			if (base.pointsOfInterest.ElementAt(i) is EquipmentItem { mapObjectState: MAP_OBJECT_STATE.BUILDING } equipmentItem)
			{
				return equipmentItem;
			}
		}
		return null;
	}

	private TILE_OBJECT_TYPE GetLegendaryEquipmentToMakeFromRequestList()
	{
		for (int i = 0; i < requests.Count; i++)
		{
			WorkShopRequestForm workShopRequestForm = requests[i];
			if (workShopRequestForm.requestingCharacter == null || workShopRequestForm.requestingCharacter.isDead)
			{
				continue;
			}
			CharacterClass characterClass = CharacterManager.Instance.GetCharacterClass(workShopRequestForm.requestingCharacter.characterClass.className);
			List<TILE_OBJECT_TYPE> list = null;
			switch (workShopRequestForm.equipmentType)
			{
			case EQUIPMENT_TYPE.WEAPON:
				list = characterClass.craftableWeapons;
				break;
			case EQUIPMENT_TYPE.ARMOR:
				list = characterClass.craftableArmors;
				break;
			case EQUIPMENT_TYPE.ACCESSORY:
				list = characterClass.craftableAccessories;
				break;
			}
			if (list == null)
			{
				continue;
			}
			for (int num = list.Count - 1; num >= 0; num--)
			{
				TILE_OBJECT_TYPE tILE_OBJECT_TYPE = list[num];
				if (EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(tILE_OBJECT_TYPE.ToStringEnumWithSpace()).isLegendary)
				{
					return tILE_OBJECT_TYPE;
				}
			}
		}
		return TILE_OBJECT_TYPE.NONE;
	}

	protected override void ProcessWorkStructureJobsByWorker(Character p_worker, out JobQueueItem producedJob)
	{
		producedJob = null;
		EquipmentItem firstEquipmentThatIsUnfinished = GetFirstEquipmentThatIsUnfinished();
		if (firstEquipmentThatIsUnfinished != null && p_worker.jobComponent.ResumeCraftEquipment(firstEquipmentThatIsUnfinished, this, out producedJob))
		{
			return;
		}
		if (p_worker.faction != null && ChanceData.RollChance(CHANCE_TYPE.Craft_Legendary_Equipment))
		{
			LegendaryForgeTileObject legendaryForgeTileObject = null;
			for (int i = 0; i < p_worker.faction.ownedSettlements.Count; i++)
			{
				LocationStructure randomStructureOfType = p_worker.faction.ownedSettlements[i].GetRandomStructureOfType(STRUCTURE_TYPE.LEGENDARY_FORGE);
				if (randomStructureOfType != null)
				{
					legendaryForgeTileObject = randomStructureOfType.GetFirstTileObjectOfType<LegendaryForgeTileObject>(TILE_OBJECT_TYPE.LEGENDARY_FORGE_TILE_OBJECT);
					break;
				}
			}
			if (legendaryForgeTileObject != null)
			{
				TILE_OBJECT_TYPE legendaryEquipmentToMakeFromRequestList = GetLegendaryEquipmentToMakeFromRequestList();
				if (legendaryEquipmentToMakeFromRequestList != TILE_OBJECT_TYPE.NONE)
				{
					p_worker.jobComponent.TriggerCraftLegendaryEquipmentJob(legendaryEquipmentToMakeFromRequestList, legendaryForgeTileObject, out producedJob);
					if (producedJob != null)
					{
						return;
					}
				}
			}
		}
		GetReferenceForAllMetals();
		GetReferenceForStones();
		GetReferenceForWood();
		GetReferenceForCloths();
		GetReferenceForLeathers();
		TILE_OBJECT_TYPE nonLegendaryEquipmentToMakeFromRequestList = GetNonLegendaryEquipmentToMakeFromRequestList();
		if (nonLegendaryEquipmentToMakeFromRequestList != TILE_OBJECT_TYPE.NONE)
		{
			p_worker.jobComponent.TriggerCraftEquipmentJob(nonLegendaryEquipmentToMakeFromRequestList, this, out producedJob);
			if (producedJob != null)
			{
				return;
			}
		}
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		list = CreateListForHaulCombineCandidate(m_metals);
		if (list.Count > 1)
		{
			p_worker.jobComponent.TryCreateCombineStockpile(list[0] as ResourcePile, list[1] as ResourcePile, out producedJob);
			if (producedJob != null)
			{
				RuinarchListPool<TileObject>.Release(list);
				return;
			}
		}
		list = CreateListForHaulCombineCandidate(m_stones);
		if (list.Count > 1)
		{
			p_worker.jobComponent.TryCreateCombineStockpile(list[0] as ResourcePile, list[1] as ResourcePile, out producedJob);
			if (producedJob != null)
			{
				RuinarchListPool<TileObject>.Release(list);
				return;
			}
		}
		list = CreateListForHaulCombineCandidate(m_woods);
		if (list.Count > 1)
		{
			p_worker.jobComponent.TryCreateCombineStockpile(list[0] as ResourcePile, list[1] as ResourcePile, out producedJob);
			if (producedJob != null)
			{
				RuinarchListPool<TileObject>.Release(list);
				return;
			}
		}
		list = CreateListForHaulCombineCandidate(m_cloth);
		if (list.Count > 1)
		{
			p_worker.jobComponent.TryCreateCombineStockpile(list[0] as ResourcePile, list[1] as ResourcePile, out producedJob);
			if (producedJob != null)
			{
				RuinarchListPool<TileObject>.Release(list);
				return;
			}
		}
		list = CreateListForHaulCombineCandidate(m_leather);
		if (list.Count > 1)
		{
			p_worker.jobComponent.TryCreateCombineStockpile(list[0] as ResourcePile, list[1] as ResourcePile, out producedJob);
			if (producedJob != null)
			{
				RuinarchListPool<TileObject>.Release(list);
				return;
			}
		}
		RuinarchListPool<TileObject>.Release(list);
		if (!ShouldIgnoreHaul(m_metals))
		{
			ResourcePile randomPileOfResourceTypeForWorkshopHaul = p_worker.homeSettlement.SettlementResources.GetRandomPileOfResourceTypeForWorkshopHaul(RESOURCE.METAL, STRUCTURE_TYPE.MINE, p_worker.homeSettlement);
			if (randomPileOfResourceTypeForWorkshopHaul != null)
			{
				p_worker.jobComponent.TryCreateHaulJobForCrafter(randomPileOfResourceTypeForWorkshopHaul, out producedJob, 40);
				if (producedJob != null)
				{
					return;
				}
			}
		}
		if (!ShouldIgnoreHaul(m_stones))
		{
			ResourcePile randomPileOfResourceTypeForWorkshopHaul2 = p_worker.homeSettlement.SettlementResources.GetRandomPileOfResourceTypeForWorkshopHaul(RESOURCE.STONE, STRUCTURE_TYPE.MINE, p_worker.homeSettlement);
			if (randomPileOfResourceTypeForWorkshopHaul2 != null)
			{
				p_worker.jobComponent.TryCreateHaulJobForCrafter(randomPileOfResourceTypeForWorkshopHaul2, out producedJob, 40);
				if (producedJob != null)
				{
					return;
				}
			}
		}
		if (!ShouldIgnoreHaul(m_cloth))
		{
			ResourcePile randomPileOfResourceTypeForWorkshopHaul3 = p_worker.homeSettlement.SettlementResources.GetRandomPileOfResourceTypeForWorkshopHaul(RESOURCE.CLOTH, STRUCTURE_TYPE.HUNTER_LODGE, p_worker.homeSettlement);
			if (randomPileOfResourceTypeForWorkshopHaul3 != null)
			{
				p_worker.jobComponent.TryCreateHaulJobForCrafter(randomPileOfResourceTypeForWorkshopHaul3, out producedJob, 40);
				if (producedJob != null)
				{
					return;
				}
			}
		}
		if (!ShouldIgnoreHaul(m_leather))
		{
			ResourcePile randomPileOfResourceTypeForWorkshopHaul4 = p_worker.homeSettlement.SettlementResources.GetRandomPileOfResourceTypeForWorkshopHaul(RESOURCE.LEATHER, STRUCTURE_TYPE.HUNTER_LODGE, p_worker.homeSettlement);
			if (randomPileOfResourceTypeForWorkshopHaul4 != null)
			{
				p_worker.jobComponent.TryCreateHaulJobForCrafter(randomPileOfResourceTypeForWorkshopHaul4, out producedJob, 40);
				if (producedJob != null)
				{
					return;
				}
			}
		}
		if (!ShouldIgnoreHaul(m_woods))
		{
			ResourcePile randomPileOfResourceTypeForWorkshopHaul5 = p_worker.homeSettlement.SettlementResources.GetRandomPileOfResourceTypeForWorkshopHaul(RESOURCE.WOOD, STRUCTURE_TYPE.LUMBERYARD, p_worker.homeSettlement);
			if (randomPileOfResourceTypeForWorkshopHaul5 != null)
			{
				p_worker.jobComponent.TryCreateHaulJobForCrafter(randomPileOfResourceTypeForWorkshopHaul5, out producedJob, 40);
				if (producedJob != null)
				{
					return;
				}
			}
		}
		TryCreateCleanJob(p_worker, out producedJob);
	}

	private bool ShouldIgnoreHaul(List<TileObject> p_list)
	{
		for (int i = 0; i < p_list.Count; i++)
		{
			if (p_list[i] is ResourcePile { resourceInPile: >=40 })
			{
				return true;
			}
		}
		return false;
	}

	public bool CanBeCrafted(List<CONCRETE_RESOURCES> p_needs, int p_count, out ResourcePile foundResourcePile)
	{
		for (int i = 0; i < p_needs.Count; i++)
		{
			List<TileObject> list = RuinarchListPool<TileObject>.Claim();
			CONCRETE_RESOURCES cONCRETE_RESOURCES = p_needs[i];
			switch (cONCRETE_RESOURCES.GetResourceCategory())
			{
			case RESOURCE.METAL:
				PopulateTileObjectsOfType<MetalPile>(list);
				break;
			case RESOURCE.STONE:
				PopulateTileObjectsOfType(list, TILE_OBJECT_TYPE.STONE_PILE);
				break;
			case RESOURCE.WOOD:
				PopulateTileObjectsOfType(list, TILE_OBJECT_TYPE.WOOD_PILE);
				break;
			case RESOURCE.CLOTH:
				PopulateTileObjectsOfType<ClothPile>(list);
				break;
			case RESOURCE.LEATHER:
				PopulateTileObjectsOfType<LeatherPile>(list);
				break;
			}
			for (int j = 0; j < list.Count; j++)
			{
				TileObject tileObject = list[j];
				if (tileObject is ResourcePile { characterOwner: null } resourcePile && tileObject.tileObjectType == cONCRETE_RESOURCES.ConvertResourcesToTileObjectType() && resourcePile.resourceInPile >= p_count)
				{
					foundResourcePile = resourcePile;
					RuinarchListPool<TileObject>.Release(list);
					return true;
				}
			}
			RuinarchListPool<TileObject>.Release(list);
		}
		foundResourcePile = null;
		return false;
	}

	public bool CanBeCrafted(RESOURCE p_generalResource, int p_count)
	{
		List<TileObject> list = null;
		switch (p_generalResource)
		{
		case RESOURCE.METAL:
			list = m_metals;
			break;
		case RESOURCE.STONE:
			list = m_stones;
			break;
		case RESOURCE.WOOD:
			list = m_woods;
			break;
		case RESOURCE.CLOTH:
			list = m_cloth;
			break;
		case RESOURCE.LEATHER:
			list = m_leather;
			break;
		}
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] is ResourcePile { characterOwner: null } resourcePile && resourcePile.resourceInPile >= p_count)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void GetReferenceForAllMetals()
	{
		m_metals.Clear();
		PopulateTileObjectsOfType<MetalPile>(m_metals);
	}

	private List<TileObject> CreateListForHaulCombineCandidate(List<TileObject> p_list)
	{
		List<TileObject> list = new List<TileObject>();
		for (int i = 0; i < p_list.Count; i++)
		{
			for (int j = 0; j < p_list.Count; j++)
			{
				if (i == j)
				{
					continue;
				}
				TileObject tileObject = p_list[i];
				TileObject tileObject2 = p_list[j];
				if (tileObject.characterOwner == null && tileObject2.characterOwner == null && tileObject.tileObjectType == tileObject2.tileObjectType)
				{
					if (!list.Contains(tileObject))
					{
						list.Add(tileObject);
					}
					if (!list.Contains(tileObject2))
					{
						list.Add(tileObject2);
					}
					return list;
				}
			}
		}
		return list;
	}

	private void GetReferenceForStones()
	{
		m_stones.Clear();
		PopulateTileObjectsOfType(m_stones, TILE_OBJECT_TYPE.STONE_PILE);
	}

	private void GetReferenceForWood()
	{
		m_woods.Clear();
		PopulateTileObjectsOfType(m_woods, TILE_OBJECT_TYPE.WOOD_PILE);
	}

	private void GetReferenceForCloths()
	{
		m_cloth.Clear();
		PopulateTileObjectsOfType<ClothPile>(m_cloth);
	}

	private void GetReferenceForLeathers()
	{
		m_leather.Clear();
		PopulateTileObjectsOfType<LeatherPile>(m_leather);
	}

	public override string GetTestingInfo()
	{
		string testingInfo = base.GetTestingInfo();
		testingInfo += "\nRequests: ";
		for (int i = 0; i < requests.Count; i++)
		{
			WorkShopRequestForm workShopRequestForm = requests[i];
			testingInfo = testingInfo + "\n" + workShopRequestForm.ToString();
		}
		return testingInfo;
	}

	public override bool CanHireAWorker()
	{
		return !HasAssignedWorker();
	}

	public override bool CanPurchaseFromHere(Character p_buyer, out bool needsToPay, out int buyerOpinionOfWorker)
	{
		return DefaultCanPurchaseFromHereForSingleWorkerStructures(p_buyer, out needsToPay, out buyerOpinionOfWorker);
	}
}

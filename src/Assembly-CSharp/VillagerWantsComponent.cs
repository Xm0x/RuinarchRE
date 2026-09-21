using System;
using System.Collections.Generic;
using System.Linq;
using Characters.Components;
using Characters.Villager_Wants;
using Inner_Maps.Location_Structures;
using Traits;

public class VillagerWantsComponent : CharacterComponent, CharacterEventDispatcher.IHomeStructureListener, CharacterEventDispatcher.IEquipmentListener, CharacterEventDispatcher.IInventoryListener, CharacterEventDispatcher.IFactionListener, CharacterEventDispatcher.ITraitListener
{
	private readonly List<VillagerWant> _wantsToProcess;

	public List<VillagerWant> wantsToProcess => _wantsToProcess;

	public VillagerWantsComponent()
	{
		_wantsToProcess = new List<VillagerWant>();
	}

	public VillagerWantsComponent(SaveDataVillagerWantsComponent p_data)
	{
		_wantsToProcess = new List<VillagerWant>();
		for (int i = 0; i < p_data.wantsToProcess.Length; i++)
		{
			Type p_type = p_data.wantsToProcess[i];
			VillagerWant villagerWantInstance = CharacterManager.Instance.GetVillagerWantInstance<VillagerWant>(p_type);
			_wantsToProcess.Add(villagerWantInstance);
		}
	}

	public void LoadReferences(SaveDataVillagerWantsComponent p_data, Character p_character)
	{
		SubscribeListeners(p_character);
	}

	public void Initialize(Character p_character)
	{
		SubscribeListeners(p_character);
		EvaluateAllWants(p_character);
	}

	private void SubscribeListeners(Character p_character)
	{
		p_character.eventDispatcher.SubscribeToCharacterSetHomeStructure(this);
		p_character.eventDispatcher.SubscribeToObjectPlacedInCharactersDwelling(this);
		p_character.eventDispatcher.SubscribeToObjectRemovedFromCharactersDwelling(this);
		p_character.eventDispatcher.SubscribeToWeaponEvents(this);
		p_character.eventDispatcher.SubscribeToInventoryEvents(this);
		p_character.eventDispatcher.SubscribeToFactionEvents(this);
		p_character.eventDispatcher.SubscribeToCharacterGainedTrait(this);
		p_character.eventDispatcher.SubscribeToCharacterLostTrait(this);
	}

	private void UnsubscribeListeners(Character p_character)
	{
		p_character.eventDispatcher.UnsubscribeToCharacterSetHomeStructure(this);
		p_character.eventDispatcher.UnsubscribeToObjectPlacedInCharactersHome(this);
		p_character.eventDispatcher.UnsubscribeToObjectRemovedFromCharactersHome(this);
		p_character.eventDispatcher.UnsubscribeToWeaponEvents(this);
		p_character.eventDispatcher.UnsubscribeToInventoryEvents(this);
		p_character.eventDispatcher.UnsubscribeToFactionEvents(this);
		p_character.eventDispatcher.UnsubscribeToCharacterGainedTrait(this);
		p_character.eventDispatcher.UnsubscribeToCharacterLostTrait(this);
	}

	private bool TryToggleWantOn(VillagerWant p_want)
	{
		if (p_want.IsWantValid(base.owner))
		{
			return ToggleWantOnWithoutValidityCheck(p_want);
		}
		return false;
	}

	private bool ToggleWantOnWithoutValidityCheck(VillagerWant p_want)
	{
		if (!wantsToProcess.Contains(p_want))
		{
			bool flag = false;
			if (wantsToProcess.Count > 0)
			{
				for (int i = 0; i < wantsToProcess.Count; i++)
				{
					VillagerWant villagerWant = wantsToProcess[i];
					if (p_want.priority > villagerWant.priority)
					{
						wantsToProcess.Insert(i, p_want);
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				wantsToProcess.Add(p_want);
			}
			p_want.OnWantToggledOn(base.owner);
			return true;
		}
		return false;
	}

	private bool ToggleWantOff(VillagerWant p_want)
	{
		if (wantsToProcess.Remove(p_want))
		{
			p_want.OnWantToggledOff(base.owner);
			return true;
		}
		return false;
	}

	private void EvaluateAllWants(Character p_character)
	{
		List<VillagerWant> list = CharacterManager.Instance.allWants.Values.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			VillagerWant villagerWant = list[i];
			if (villagerWant.IsWantValid(p_character))
			{
				ToggleWantOnWithoutValidityCheck(villagerWant);
			}
			else
			{
				ToggleWantOff(villagerWant);
			}
		}
	}

	public VillagerWant GetTopPriorityWant(Character p_character, out LocationStructure p_chosenStructure, out TileObject p_foundObject)
	{
		for (int i = 0; i < wantsToProcess.Count; i++)
		{
			VillagerWant villagerWant = wantsToProcess[i];
			if (villagerWant.CanVillagerObtainWant(p_character, out p_chosenStructure, out p_foundObject))
			{
				return villagerWant;
			}
		}
		p_chosenStructure = null;
		p_foundObject = null;
		return null;
	}

	public bool IsWantToggledOn<T>() where T : VillagerWant
	{
		for (int i = 0; i < _wantsToProcess.Count; i++)
		{
			if (_wantsToProcess[i] is T)
			{
				return true;
			}
		}
		return false;
	}

	public bool CanItemSatisfyWant(TileObject p_tileObject)
	{
		for (int i = 0; i < _wantsToProcess.Count; i++)
		{
			if (_wantsToProcess[i] is ItemWant itemWant && itemWant.CanObjectSatisfyWant(p_tileObject, base.owner))
			{
				return true;
			}
		}
		return false;
	}

	public void OnCharacterSetHomeStructure(Character p_character, LocationStructure p_homeStructure)
	{
		if (p_homeStructure != null)
		{
			if (p_homeStructure is Dwelling || p_homeStructure is VampireCastle)
			{
				DwellingWant villagerWantInstance = CharacterManager.Instance.GetVillagerWantInstance<DwellingWant>();
				ToggleWantOff(villagerWantInstance);
			}
			EvaluateAllWants(p_character);
		}
		else
		{
			DwellingWant villagerWantInstance2 = CharacterManager.Instance.GetVillagerWantInstance<DwellingWant>();
			TryToggleWantOn(villagerWantInstance2);
			EvaluateAllWants(p_character);
		}
	}

	public void OnObjectPlacedInHomeDwelling(Character p_character, LocationStructure p_homeStructure, TileObject p_placedObject)
	{
		if (p_placedObject is FoodPile)
		{
			FoodWant_1 villagerWantInstance = CharacterManager.Instance.GetVillagerWantInstance<FoodWant_1>();
			ToggleWantOff(villagerWantInstance);
			if (p_homeStructure is Dwelling dwelling)
			{
				if (dwelling.differentFoodPileKindsInDwelling >= 2)
				{
					FoodWant_2 villagerWantInstance2 = CharacterManager.Instance.GetVillagerWantInstance<FoodWant_2>();
					ToggleWantOff(villagerWantInstance2);
				}
				if (dwelling.differentFoodPileKindsInDwelling >= 3)
				{
					FoodWant_3 villagerWantInstance3 = CharacterManager.Instance.GetVillagerWantInstance<FoodWant_3>();
					ToggleWantOff(villagerWantInstance3);
				}
			}
		}
		else if (p_placedObject is Table)
		{
			TableWant villagerWantInstance4 = CharacterManager.Instance.GetVillagerWantInstance<TableWant>();
			ToggleWantOff(villagerWantInstance4);
		}
		else if (p_placedObject is Bed)
		{
			BedWant villagerWantInstance5 = CharacterManager.Instance.GetVillagerWantInstance<BedWant>();
			ToggleWantOff(villagerWantInstance5);
		}
		else if (p_placedObject is Torch || p_placedObject is DivineOrb)
		{
			HomeLightingWant villagerWantInstance6 = CharacterManager.Instance.GetVillagerWantInstance<HomeLightingWant>();
			if (p_homeStructure.HasBuiltTileObjectOfType(villagerWantInstance6.GetFurnitureWanted(base.owner)))
			{
				ToggleWantOff(villagerWantInstance6);
			}
		}
		else if (p_placedObject is Guitar)
		{
			GuitarWant villagerWantInstance7 = CharacterManager.Instance.GetVillagerWantInstance<GuitarWant>();
			ToggleWantOff(villagerWantInstance7);
		}
	}

	public void OnObjectRemovedFromHomeDwelling(Character p_character, LocationStructure p_homeStructure, TileObject p_removedObject)
	{
		if (p_removedObject is FoodPile)
		{
			if (!p_homeStructure.HasTileObjectThatIsBuiltFoodPile())
			{
				FoodWant_1 villagerWantInstance = CharacterManager.Instance.GetVillagerWantInstance<FoodWant_1>();
				TryToggleWantOn(villagerWantInstance);
			}
			if (p_homeStructure is Dwelling dwelling)
			{
				if (dwelling.differentFoodPileKindsInDwelling < 2)
				{
					FoodWant_2 villagerWantInstance2 = CharacterManager.Instance.GetVillagerWantInstance<FoodWant_2>();
					TryToggleWantOn(villagerWantInstance2);
				}
				if (dwelling.differentFoodPileKindsInDwelling < 3)
				{
					FoodWant_3 villagerWantInstance3 = CharacterManager.Instance.GetVillagerWantInstance<FoodWant_3>();
					TryToggleWantOn(villagerWantInstance3);
				}
			}
		}
		else if (p_removedObject is Table)
		{
			if (!p_homeStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.TABLE))
			{
				TableWant villagerWantInstance4 = CharacterManager.Instance.GetVillagerWantInstance<TableWant>();
				TryToggleWantOn(villagerWantInstance4);
			}
		}
		else if (p_removedObject is Bed)
		{
			if (!p_homeStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.BED))
			{
				BedWant villagerWantInstance5 = CharacterManager.Instance.GetVillagerWantInstance<BedWant>();
				TryToggleWantOn(villagerWantInstance5);
			}
		}
		else if (p_removedObject is Torch || p_removedObject is DivineOrb)
		{
			HomeLightingWant villagerWantInstance6 = CharacterManager.Instance.GetVillagerWantInstance<HomeLightingWant>();
			if (!p_homeStructure.HasBuiltTileObjectOfType(villagerWantInstance6.GetFurnitureWanted(base.owner)))
			{
				TryToggleWantOn(villagerWantInstance6);
			}
		}
		else if (p_removedObject is Guitar && !p_homeStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.GUITAR))
		{
			GuitarWant villagerWantInstance7 = CharacterManager.Instance.GetVillagerWantInstance<GuitarWant>();
			if (villagerWantInstance7.IsWantValid(p_character))
			{
				TryToggleWantOn(villagerWantInstance7);
			}
		}
	}

	public void OnWeaponEquipped(Character p_character, EquipmentItem p_weapon)
	{
		WeaponWant villagerWantInstance = CharacterManager.Instance.GetVillagerWantInstance<WeaponWant>();
		ToggleWantOff(villagerWantInstance);
	}

	public void OnWeaponUnequipped(Character p_character, EquipmentItem p_weapon)
	{
		WeaponWant villagerWantInstance = CharacterManager.Instance.GetVillagerWantInstance<WeaponWant>();
		TryToggleWantOn(villagerWantInstance);
	}

	public void OnArmorEquipped(Character p_character, EquipmentItem p_weapon)
	{
		ArmorWant villagerWantInstance = CharacterManager.Instance.GetVillagerWantInstance<ArmorWant>();
		ToggleWantOff(villagerWantInstance);
	}

	public void OnArmorUnequipped(Character p_character, EquipmentItem p_weapon)
	{
		ArmorWant villagerWantInstance = CharacterManager.Instance.GetVillagerWantInstance<ArmorWant>();
		TryToggleWantOn(villagerWantInstance);
	}

	public void OnAccessoryEquipped(Character p_character, EquipmentItem p_weapon)
	{
		AccessoryWant villagerWantInstance = CharacterManager.Instance.GetVillagerWantInstance<AccessoryWant>();
		ToggleWantOff(villagerWantInstance);
	}

	public void OnAccessoryUnequipped(Character p_character, EquipmentItem p_weapon)
	{
		AccessoryWant villagerWantInstance = CharacterManager.Instance.GetVillagerWantInstance<AccessoryWant>();
		TryToggleWantOn(villagerWantInstance);
	}

	public void OnItemObtained(Character p_character, TileObject p_obtainedItem)
	{
		if (p_obtainedItem is HealingPotion)
		{
			HealingPotionWant villagerWantInstance = CharacterManager.Instance.GetVillagerWantInstance<HealingPotionWant>();
			ToggleWantOff(villagerWantInstance);
		}
	}

	public void OnItemLost(Character p_character, TileObject p_lostItem)
	{
		if (p_lostItem is HealingPotion && !p_character.HasItem(TILE_OBJECT_TYPE.HEALING_POTION))
		{
			HealingPotionWant villagerWantInstance = CharacterManager.Instance.GetVillagerWantInstance<HealingPotionWant>();
			TryToggleWantOn(villagerWantInstance);
		}
	}

	public void OnJoinFaction(Character p_character, Faction p_newFaction)
	{
		EvaluateAllWants(p_character);
	}

	public void OnCharacterGainedTrait(Character p_character, Trait p_gainedTrait)
	{
		if (p_gainedTrait is MusicHater)
		{
			GuitarWant villagerWantInstance = CharacterManager.Instance.GetVillagerWantInstance<GuitarWant>();
			ToggleWantOff(villagerWantInstance);
		}
		else if (p_gainedTrait is Vampire)
		{
			FoodWant_1 villagerWantInstance2 = CharacterManager.Instance.GetVillagerWantInstance<FoodWant_1>();
			ToggleWantOff(villagerWantInstance2);
			FoodWant_2 villagerWantInstance3 = CharacterManager.Instance.GetVillagerWantInstance<FoodWant_2>();
			if (!ToggleWantOff(villagerWantInstance3))
			{
				p_character.traitContainer.RemoveTrait(p_character, "Stocked Up");
			}
			FoodWant_3 villagerWantInstance4 = CharacterManager.Instance.GetVillagerWantInstance<FoodWant_3>();
			ToggleWantOff(villagerWantInstance4);
		}
	}

	public void OnCharacterLostTrait(Character p_character, Trait p_lostTrait, Character p_removedBy)
	{
		if (p_lostTrait is MusicHater)
		{
			GuitarWant villagerWantInstance = CharacterManager.Instance.GetVillagerWantInstance<GuitarWant>();
			if (villagerWantInstance.IsWantValid(p_character))
			{
				TryToggleWantOn(villagerWantInstance);
			}
		}
		else if (p_lostTrait is Vampire)
		{
			FoodWant_1 villagerWantInstance2 = CharacterManager.Instance.GetVillagerWantInstance<FoodWant_1>();
			if (villagerWantInstance2.IsWantValid(p_character))
			{
				TryToggleWantOn(villagerWantInstance2);
			}
			FoodWant_2 villagerWantInstance3 = CharacterManager.Instance.GetVillagerWantInstance<FoodWant_2>();
			if (villagerWantInstance3.IsWantValid(p_character))
			{
				TryToggleWantOn(villagerWantInstance3);
			}
			FoodWant_3 villagerWantInstance4 = CharacterManager.Instance.GetVillagerWantInstance<FoodWant_3>();
			if (villagerWantInstance4.IsWantValid(p_character))
			{
				TryToggleWantOn(villagerWantInstance4);
			}
		}
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure pStructure)
	{
	}

	public override void CleanUp()
	{
		UnsubscribeListeners(base.owner);
		base.CleanUp();
	}
}

using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

public class EquipmentComponent
{
	public List<EquipmentItem> allEquipments = new List<EquipmentItem>();

	public EquipmentItem currentWeapon { get; set; }

	public EquipmentItem currentArmor { get; set; }

	public EquipmentItem currentAccessory { get; set; }

	public EquipmentComponent()
	{
		Reset();
	}

	public EquipmentComponent(SaveDataEquipmentComponent p_copy)
	{
		currentWeapon = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(p_copy.currentWeaponID) as EquipmentItem;
		currentArmor = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(p_copy.currentArmorID) as EquipmentItem;
		currentAccessory = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(p_copy.currentAccessoryID) as EquipmentItem;
		if (currentWeapon != null)
		{
			allEquipments.Add(currentWeapon);
		}
		if (currentArmor != null)
		{
			allEquipments.Add(currentArmor);
		}
		if (currentAccessory != null)
		{
			allEquipments.Add(currentAccessory);
		}
	}

	public void LoadReferences(SaveDataEquipmentComponent p_data)
	{
		EquipmentComponent equipmentComponent = p_data.Load();
		currentWeapon = equipmentComponent.currentWeapon;
		currentArmor = equipmentComponent.currentArmor;
		currentAccessory = equipmentComponent.currentAccessory;
		if (currentWeapon != null)
		{
			allEquipments.Add(currentWeapon);
		}
		if (currentArmor != null)
		{
			allEquipments.Add(currentArmor);
		}
		if (currentAccessory != null)
		{
			allEquipments.Add(currentAccessory);
		}
	}

	private void Reset()
	{
		currentWeapon = null;
		currentArmor = null;
		currentAccessory = null;
	}

	private void SetWeapon(EquipmentItem p_newWeapon, Character p_targetCharacter, bool p_initializedStackCountOnly = false)
	{
		currentWeapon = p_newWeapon;
		EquipmentBonusProcessor.ApplyEquipBonusToTarget(currentWeapon, p_targetCharacter, p_initializedStackCountOnly);
		currentWeapon.ProcessEffectsOnEquip(p_targetCharacter);
		p_targetCharacter.eventDispatcher.ExecuteWeaponEquipped(p_targetCharacter, p_newWeapon);
		Messenger.Broadcast(CharacterSignals.CHARACTER_EQUIPPED_ITEM, p_targetCharacter, p_newWeapon);
	}

	private void SetArmor(EquipmentItem p_newArmor, Character p_targetCharacter, bool p_initializedStackCountOnly = false)
	{
		currentArmor = p_newArmor;
		EquipmentBonusProcessor.ApplyEquipBonusToTarget(currentArmor, p_targetCharacter, p_initializedStackCountOnly);
		currentArmor.ProcessEffectsOnEquip(p_targetCharacter);
		p_targetCharacter.eventDispatcher.ExecuteArmorEquipped(p_targetCharacter, p_newArmor);
		Messenger.Broadcast(CharacterSignals.CHARACTER_EQUIPPED_ITEM, p_targetCharacter, p_newArmor);
	}

	private void SetAccessory(EquipmentItem p_newAccessory, Character p_targetCharacter, bool p_initializedStackCountOnly = false)
	{
		currentAccessory = p_newAccessory;
		EquipmentBonusProcessor.ApplyEquipBonusToTarget(currentAccessory, p_targetCharacter, p_initializedStackCountOnly);
		currentAccessory.ProcessEffectsOnEquip(p_targetCharacter);
		p_targetCharacter.eventDispatcher.ExecuteAccessoryEquipped(p_targetCharacter, p_newAccessory);
		Messenger.Broadcast(CharacterSignals.CHARACTER_EQUIPPED_ITEM, p_targetCharacter, p_newAccessory);
	}

	public void SetEquipment(EquipmentItem p_newItem, Character p_targetCharacter, bool p_initializedStackCountOnly = false)
	{
		if (p_newItem is WeaponItem)
		{
			SetWeapon(p_newItem, p_targetCharacter, p_initializedStackCountOnly);
		}
		if (p_newItem is ArmorItem)
		{
			SetArmor(p_newItem, p_targetCharacter, p_initializedStackCountOnly);
		}
		if (p_newItem is AccessoryItem)
		{
			SetAccessory(p_newItem, p_targetCharacter, p_initializedStackCountOnly);
		}
		if (!p_initializedStackCountOnly)
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "Character_Equipped_Item", LOG_TAG.Major);
			log.AddToFillers(p_targetCharacter, p_targetCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(p_newItem, p_newItem.name, LOG_IDENTIFIER.ITEM_1);
			log.AddLogToDatabase();
		}
	}

	public bool RemoveEquipment(EquipmentItem p_removedItem, Character p_targetCharacter)
	{
		if (allEquipments.Remove(p_removedItem))
		{
			EquipmentBonusProcessor.RemoveEquipBonusToTarget(p_removedItem, p_targetCharacter);
			p_removedItem.ProcessEffectsOnUnEquip(p_targetCharacter);
			if (p_removedItem is WeaponItem)
			{
				currentWeapon = null;
				Messenger.Broadcast(CharacterSignals.WEAPON_UNEQUIPPED, p_targetCharacter, p_removedItem);
				p_targetCharacter.eventDispatcher.ExecuteWeaponUnequipped(p_targetCharacter, p_removedItem);
			}
			else if (p_removedItem is ArmorItem)
			{
				currentArmor = null;
				Messenger.Broadcast(CharacterSignals.ARMOR_UNEQUIPPED, p_targetCharacter, p_removedItem);
				p_targetCharacter.eventDispatcher.ExecuteArmorUnequipped(p_targetCharacter, p_removedItem);
			}
			else if (p_removedItem is AccessoryItem)
			{
				currentAccessory = null;
				Messenger.Broadcast(CharacterSignals.ACCESSORY_UNEQUIPPED, p_targetCharacter, p_removedItem);
				p_targetCharacter.eventDispatcher.ExecuteAccessoryUnequipped(p_targetCharacter, p_removedItem);
			}
			p_removedItem.SetInventoryOwner(null);
			p_removedItem.OnDestroyPOI();
			return true;
		}
		return false;
	}

	public void RemoveEquipmentInSlotFor(EquipmentItem equipment, Character p_owner)
	{
		if (equipment is WeaponItem)
		{
			RemoveEquipment(currentWeapon, p_owner);
		}
		else if (equipment is ArmorItem)
		{
			RemoveEquipment(currentArmor, p_owner);
		}
		else if (equipment is AccessoryItem)
		{
			RemoveEquipment(currentAccessory, p_owner);
		}
	}

	public EquipmentItem GetRandomRemainingEquipment(EquipmentItem p_equipToBeremoved)
	{
		List<EquipmentItem> list = new List<EquipmentItem>();
		if (currentWeapon != null && !(p_equipToBeremoved is WeaponItem))
		{
			list.Add(currentWeapon);
		}
		if (currentArmor != null && !(p_equipToBeremoved is ArmorItem))
		{
			list.Add(currentArmor);
		}
		if (currentAccessory != null && !(p_equipToBeremoved is AccessoryItem))
		{
			list.Add(currentAccessory);
		}
		if (list.Count <= 0)
		{
			return null;
		}
		int index = Random.Range(0, list.Count);
		return list[index];
	}

	public bool HasEquips()
	{
		if (currentWeapon == null && currentArmor == null)
		{
			return currentAccessory != null;
		}
		return true;
	}

	public bool HasDeadlyEquipment()
	{
		for (int i = 0; i < allEquipments.Count; i++)
		{
			if (allEquipments[i] != null && allEquipments[i].isDeadly)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasFesteringEquipment()
	{
		for (int i = 0; i < allEquipments.Count; i++)
		{
			if (allEquipments[i] != null && allEquipments[i].isFestering)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasHauntedEquipment()
	{
		for (int i = 0; i < allEquipments.Count; i++)
		{
			if (allEquipments[i] != null && allEquipments[i].isHaunted)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasMentorEquipment()
	{
		for (int i = 0; i < allEquipments.Count; i++)
		{
			if (allEquipments[i].isMentor)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasEquipment(EquipmentItem equipment)
	{
		return allEquipments.Contains(equipment);
	}

	public bool HasEquipment(TILE_OBJECT_TYPE equipmentType)
	{
		for (int i = 0; i < allEquipments.Count; i++)
		{
			if (allEquipments[i].tileObjectType == equipmentType)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasEquipmentInSlotFor(EquipmentItem equipment)
	{
		if (equipment is WeaponItem)
		{
			return currentWeapon != null;
		}
		if (equipment is ArmorItem)
		{
			return currentArmor != null;
		}
		if (equipment is AccessoryItem)
		{
			return currentAccessory != null;
		}
		return false;
	}

	public bool IsCurrentEquipmentBetterThanOrEqualTo(EquipmentItem p_newEquipment)
	{
		if (p_newEquipment is WeaponItem)
		{
			if (currentWeapon != null)
			{
				if (currentWeapon.equipmentData.tier >= p_newEquipment.equipmentData.tier)
				{
					return true;
				}
				return false;
			}
			return false;
		}
		if (p_newEquipment is ArmorItem)
		{
			if (currentArmor != null)
			{
				if (currentArmor.equipmentData.tier >= p_newEquipment.equipmentData.tier)
				{
					return true;
				}
				return false;
			}
			return false;
		}
		if (p_newEquipment is AccessoryItem)
		{
			if (currentAccessory != null)
			{
				if (currentAccessory.equipmentData.tier >= p_newEquipment.equipmentData.tier)
				{
					return true;
				}
				return false;
			}
			return false;
		}
		return false;
	}

	public bool EvaluateNewEquipment(EquipmentItem p_newEquipment, Character p_character)
	{
		if (!CanEquipItem(p_newEquipment, p_character))
		{
			return false;
		}
		if (p_newEquipment is WeaponItem)
		{
			if (currentWeapon != null)
			{
				if (currentWeapon.equipmentData.tier < p_newEquipment.equipmentData.tier)
				{
					return true;
				}
				return false;
			}
			return true;
		}
		if (p_newEquipment is ArmorItem)
		{
			if (currentArmor != null)
			{
				if (currentArmor.equipmentData.tier < p_newEquipment.equipmentData.tier)
				{
					return true;
				}
				return false;
			}
			return true;
		}
		if (p_newEquipment is AccessoryItem)
		{
			if (currentAccessory != null)
			{
				if (currentAccessory.equipmentData.tier < p_newEquipment.equipmentData.tier)
				{
					return true;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public bool CanEquipItem(EquipmentItem p_newEquipment, Character p_character)
	{
		CharacterClass characterClass = p_character.characterClass;
		RaceData raceData = RaceManager.Instance.GetRaceData(p_character.race);
		if (p_newEquipment is ExcaliburSword)
		{
			return true;
		}
		if (p_character.isNormalCharacter && p_character.race != RACE.RATMAN)
		{
			if (!characterClass.craftableAccessories.Contains(p_newEquipment.tileObjectType) && !characterClass.craftableArmors.Contains(p_newEquipment.tileObjectType) && !characterClass.craftableWeapons.Contains(p_newEquipment.tileObjectType))
			{
				return false;
			}
			return true;
		}
		if (raceData.category == CHARACTER_CATEGORY.Humanoid || raceData.category == CHARACTER_CATEGORY.Demonic || raceData.category == CHARACTER_CATEGORY.Undead)
		{
			if (p_newEquipment is AccessoryItem)
			{
				return true;
			}
			if (p_character.characterClass.attackType == ATTACK_TYPE.PHYSICAL)
			{
				if (p_character.characterClass.attackRange <= 1f)
				{
					if (p_newEquipment is ArmorItem)
					{
						return true;
					}
					return (p_newEquipment.equipmentData as WeaponData).weaponType == WEAPON_TYPE.Melee;
				}
				if (p_newEquipment is ArmorItem)
				{
					if (p_newEquipment.equipmentData.resourceType != RESOURCE.NONE)
					{
						if (p_newEquipment.equipmentData.resourceType != RESOURCE.LEATHER)
						{
							return p_newEquipment.equipmentData.resourceType == RESOURCE.CLOTH;
						}
						return true;
					}
					for (int i = 0; i < p_newEquipment.equipmentData.specificResource.Count; i++)
					{
						RESOURCE resourceCategory = p_newEquipment.equipmentData.specificResource[i].GetResourceCategory();
						if (resourceCategory == RESOURCE.LEATHER || resourceCategory == RESOURCE.CLOTH || resourceCategory == RESOURCE.WOOD || resourceCategory == RESOURCE.STONE)
						{
							return true;
						}
					}
					return false;
				}
				return (p_newEquipment.equipmentData as WeaponData).weaponType == WEAPON_TYPE.Ranged;
			}
			if (p_newEquipment is ArmorItem)
			{
				if (p_newEquipment.equipmentData.resourceType != RESOURCE.NONE)
				{
					return p_newEquipment.equipmentData.resourceType == RESOURCE.CLOTH;
				}
				for (int j = 0; j < p_newEquipment.equipmentData.specificResource.Count; j++)
				{
					RESOURCE resourceCategory2 = p_newEquipment.equipmentData.specificResource[j].GetResourceCategory();
					if (resourceCategory2 == RESOURCE.CLOTH || resourceCategory2 == RESOURCE.WOOD || resourceCategory2 == RESOURCE.STONE)
					{
						return true;
					}
				}
				return false;
			}
			return (p_newEquipment.equipmentData as WeaponData).weaponType == WEAPON_TYPE.Magical;
		}
		return false;
	}

	public EquipmentItem GetEquipment(TILE_OBJECT_TYPE p_type)
	{
		for (int i = 0; i < allEquipments.Count; i++)
		{
			EquipmentItem equipmentItem = allEquipments[i];
			if (equipmentItem.tileObjectType == p_type)
			{
				return equipmentItem;
			}
		}
		return null;
	}

	public EquipmentItem GetEquipment(string p_name)
	{
		for (int i = 0; i < allEquipments.Count; i++)
		{
			EquipmentItem equipmentItem = allEquipments[i];
			if (equipmentItem.name == p_name)
			{
				return equipmentItem;
			}
		}
		return null;
	}

	public void RemoveAllIncompatibleEquipment(Character p_character)
	{
		List<EquipmentItem> list = RuinarchListPool<EquipmentItem>.Claim();
		list.AddRange(allEquipments);
		for (int i = 0; i < list.Count; i++)
		{
			EquipmentItem equipmentItem = list[i];
			if (!CanEquipItem(equipmentItem, p_character))
			{
				RemoveEquipment(equipmentItem, p_character);
			}
		}
		RuinarchListPool<EquipmentItem>.Release(list);
	}

	public void RemoveAllEquipment(Character p_character)
	{
		List<EquipmentItem> list = RuinarchListPool<EquipmentItem>.Claim();
		list.AddRange(allEquipments);
		for (int i = 0; i < list.Count; i++)
		{
			EquipmentItem item = list[i];
			p_character.UnobtainItem(item);
		}
		RuinarchListPool<EquipmentItem>.Release(list);
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}
}

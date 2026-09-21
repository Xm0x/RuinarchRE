using System;
using Inner_Maps.Location_Structures;
using Traits;

namespace Characters.Components;

public class CharacterEventDispatcher
{
	public interface ITraitListener
	{
		void OnCharacterGainedTrait(Character p_character, Trait p_gainedTrait);

		void OnCharacterLostTrait(Character p_character, Trait p_lostTrait, Character p_removedBy);
	}

	public interface ICarryListener
	{
		void OnCharacterCarried(Character p_character, Character p_carriedBy);
	}

	public interface ILocationListener
	{
		void OnCharacterLeftStructure(Character p_character, LocationStructure p_leftStructure);

		void OnCharacterArrivedAtStructure(Character p_character, LocationStructure p_leftStructure);

		void OnCharacterArrivedAtSettlement(Character p_character, NPCSettlement p_settlement);
	}

	public interface IDeathListener
	{
		void OnCharacterSubscribedToDied(Character p_character);
	}

	public interface IHomeStructureListener
	{
		void OnCharacterSetHomeStructure(Character p_character, LocationStructure p_homeStructure);

		void OnObjectPlacedInHomeDwelling(Character p_character, LocationStructure p_homeStructure, TileObject p_placedObject);

		void OnObjectRemovedFromHomeDwelling(Character p_character, LocationStructure p_homeStructure, TileObject p_removedObject);
	}

	public interface IEquipmentListener
	{
		void OnWeaponEquipped(Character p_character, EquipmentItem p_weapon);

		void OnWeaponUnequipped(Character p_character, EquipmentItem p_weapon);

		void OnArmorEquipped(Character p_character, EquipmentItem p_weapon);

		void OnArmorUnequipped(Character p_character, EquipmentItem p_weapon);

		void OnAccessoryEquipped(Character p_character, EquipmentItem p_weapon);

		void OnAccessoryUnequipped(Character p_character, EquipmentItem p_weapon);
	}

	public interface IInventoryListener
	{
		void OnItemObtained(Character p_character, TileObject p_obtainedItem);

		void OnItemLost(Character p_character, TileObject p_lostItem);
	}

	public interface IFactionListener
	{
		void OnJoinFaction(Character p_character, Faction p_newFaction);
	}

	public interface IMoodListener
	{
		void OnMoodChanged(Character p_character, MOOD_STATE p_previousMood, MOOD_STATE p_newMood);
	}

	private Action<Character, Trait> _characterGainedTrait;

	private Action<Character, Trait, Character> _characterLostTrait;

	private Action<Character, Character> _characterCarried;

	private Action<Character, LocationStructure> _characterLeftStructure;

	private Action<Character, LocationStructure> _characterArrivedAtStructure;

	private Action<Character> _characterDied;

	private Action<Character, LocationStructure> _characterSetHomeStructure;

	private Action<Character, LocationStructure, TileObject> _objectPlacedInCharacterDwelling;

	private Action<Character, LocationStructure, TileObject> _objectRemovedFromCharacterDwelling;

	private Action<Character, EquipmentItem> _weaponEquipped;

	private Action<Character, EquipmentItem> _weaponUnequipped;

	private Action<Character, EquipmentItem> _armorEquipped;

	private Action<Character, EquipmentItem> _armorUnequipped;

	private Action<Character, EquipmentItem> _accessoryEquipped;

	private Action<Character, EquipmentItem> _accessoryUnequipped;

	private Action<Character, TileObject> _itemObtained;

	private Action<Character, TileObject> _itemLost;

	private Action<Character, Faction> _joinedFaction;

	private Action<Character, NPCSettlement> _characterArrivedAtSettlement;

	private Action<Character, MOOD_STATE, MOOD_STATE> _moodChanged;

	public void SubscribeToCharacterGainedTrait(ITraitListener p_traitListener)
	{
		_characterGainedTrait = (Action<Character, Trait>)Delegate.Combine(_characterGainedTrait, new Action<Character, Trait>(p_traitListener.OnCharacterGainedTrait));
	}

	public void UnsubscribeToCharacterGainedTrait(ITraitListener p_traitListener)
	{
		_characterGainedTrait = (Action<Character, Trait>)Delegate.Remove(_characterGainedTrait, new Action<Character, Trait>(p_traitListener.OnCharacterGainedTrait));
	}

	public void ExecuteCharacterGainedTrait(Character p_character, Trait p_gainedTrait)
	{
		_characterGainedTrait?.Invoke(p_character, p_gainedTrait);
	}

	public void SubscribeToCharacterLostTrait(ITraitListener p_traitListener)
	{
		_characterLostTrait = (Action<Character, Trait, Character>)Delegate.Combine(_characterLostTrait, new Action<Character, Trait, Character>(p_traitListener.OnCharacterLostTrait));
	}

	public void UnsubscribeToCharacterLostTrait(ITraitListener p_traitListener)
	{
		_characterLostTrait = (Action<Character, Trait, Character>)Delegate.Remove(_characterLostTrait, new Action<Character, Trait, Character>(p_traitListener.OnCharacterLostTrait));
	}

	public void ExecuteCharacterLostTrait(Character p_character, Trait p_lostTrait, Character p_removedBy)
	{
		_characterLostTrait?.Invoke(p_character, p_lostTrait, p_removedBy);
	}

	public void SubscribeToCharacterCarried(ICarryListener p_carryListener)
	{
		_characterCarried = (Action<Character, Character>)Delegate.Combine(_characterCarried, new Action<Character, Character>(p_carryListener.OnCharacterCarried));
	}

	public void UnsubscribeToCharacterCarried(ICarryListener p_carryListener)
	{
		_characterCarried = (Action<Character, Character>)Delegate.Remove(_characterCarried, new Action<Character, Character>(p_carryListener.OnCharacterCarried));
	}

	public void ExecuteCarried(Character p_character, Character p_carriedBy)
	{
		_characterCarried?.Invoke(p_character, p_carriedBy);
	}

	public void SubscribeToCharacterLeftStructure(ILocationListener p_listener)
	{
		_characterLeftStructure = (Action<Character, LocationStructure>)Delegate.Combine(_characterLeftStructure, new Action<Character, LocationStructure>(p_listener.OnCharacterLeftStructure));
	}

	public void UnsubscribeToCharacterLeftStructure(ILocationListener p_listener)
	{
		_characterLeftStructure = (Action<Character, LocationStructure>)Delegate.Remove(_characterLeftStructure, new Action<Character, LocationStructure>(p_listener.OnCharacterLeftStructure));
	}

	public void ExecuteCharacterLeftStructure(Character p_character, LocationStructure p_leftStructure)
	{
		_characterLeftStructure?.Invoke(p_character, p_leftStructure);
	}

	public void SubscribeToCharacterArrivedAtStructure(ILocationListener p_listener)
	{
		_characterArrivedAtStructure = (Action<Character, LocationStructure>)Delegate.Combine(_characterArrivedAtStructure, new Action<Character, LocationStructure>(p_listener.OnCharacterArrivedAtStructure));
	}

	public void UnsubscribeToCharacterArrivedAtStructure(ILocationListener p_listener)
	{
		_characterArrivedAtStructure = (Action<Character, LocationStructure>)Delegate.Remove(_characterArrivedAtStructure, new Action<Character, LocationStructure>(p_listener.OnCharacterLeftStructure));
	}

	public void ExecuteCharacterArrivedAtStructure(Character p_character, LocationStructure p_arrivedStructure)
	{
		_characterArrivedAtStructure?.Invoke(p_character, p_arrivedStructure);
	}

	public void SubscribeToCharacterDied(IDeathListener p_listener)
	{
		_characterDied = (Action<Character>)Delegate.Combine(_characterDied, new Action<Character>(p_listener.OnCharacterSubscribedToDied));
	}

	public void UnsubscribeToCharacterDied(IDeathListener p_listener)
	{
		_characterDied = (Action<Character>)Delegate.Remove(_characterDied, new Action<Character>(p_listener.OnCharacterSubscribedToDied));
	}

	public void ExecuteCharacterDied(Character p_character)
	{
		_characterDied?.Invoke(p_character);
	}

	public void SubscribeToCharacterSetHomeStructure(IHomeStructureListener p_listener)
	{
		_characterSetHomeStructure = (Action<Character, LocationStructure>)Delegate.Combine(_characterSetHomeStructure, new Action<Character, LocationStructure>(p_listener.OnCharacterSetHomeStructure));
	}

	public void UnsubscribeToCharacterSetHomeStructure(IHomeStructureListener p_listener)
	{
		_characterSetHomeStructure = (Action<Character, LocationStructure>)Delegate.Remove(_characterSetHomeStructure, new Action<Character, LocationStructure>(p_listener.OnCharacterSetHomeStructure));
	}

	public void ExecuteCharacterSetHomeStructure(Character p_character, LocationStructure p_structure)
	{
		_characterSetHomeStructure?.Invoke(p_character, p_structure);
	}

	public void SubscribeToObjectPlacedInCharactersDwelling(IHomeStructureListener p_listener)
	{
		_objectPlacedInCharacterDwelling = (Action<Character, LocationStructure, TileObject>)Delegate.Combine(_objectPlacedInCharacterDwelling, new Action<Character, LocationStructure, TileObject>(p_listener.OnObjectPlacedInHomeDwelling));
	}

	public void UnsubscribeToObjectPlacedInCharactersHome(IHomeStructureListener p_listener)
	{
		_objectPlacedInCharacterDwelling = (Action<Character, LocationStructure, TileObject>)Delegate.Remove(_objectPlacedInCharacterDwelling, new Action<Character, LocationStructure, TileObject>(p_listener.OnObjectPlacedInHomeDwelling));
	}

	public void ExecuteObjectPlacedInCharactersHome(Character p_character, LocationStructure p_structure, TileObject p_tileObject)
	{
		_objectPlacedInCharacterDwelling?.Invoke(p_character, p_structure, p_tileObject);
	}

	public void SubscribeToObjectRemovedFromCharactersDwelling(IHomeStructureListener p_listener)
	{
		_objectRemovedFromCharacterDwelling = (Action<Character, LocationStructure, TileObject>)Delegate.Combine(_objectRemovedFromCharacterDwelling, new Action<Character, LocationStructure, TileObject>(p_listener.OnObjectRemovedFromHomeDwelling));
	}

	public void UnsubscribeToObjectRemovedFromCharactersHome(IHomeStructureListener p_listener)
	{
		_objectRemovedFromCharacterDwelling = (Action<Character, LocationStructure, TileObject>)Delegate.Remove(_objectRemovedFromCharacterDwelling, new Action<Character, LocationStructure, TileObject>(p_listener.OnObjectRemovedFromHomeDwelling));
	}

	public void ExecuteObjectRemovedFromCharactersHome(Character p_character, LocationStructure p_structure, TileObject p_tileObject)
	{
		_objectRemovedFromCharacterDwelling?.Invoke(p_character, p_structure, p_tileObject);
	}

	public void SubscribeToWeaponEvents(IEquipmentListener p_listener)
	{
		_weaponEquipped = (Action<Character, EquipmentItem>)Delegate.Combine(_weaponEquipped, new Action<Character, EquipmentItem>(p_listener.OnWeaponEquipped));
		_weaponUnequipped = (Action<Character, EquipmentItem>)Delegate.Combine(_weaponUnequipped, new Action<Character, EquipmentItem>(p_listener.OnWeaponUnequipped));
		_armorEquipped = (Action<Character, EquipmentItem>)Delegate.Combine(_armorEquipped, new Action<Character, EquipmentItem>(p_listener.OnArmorEquipped));
		_armorUnequipped = (Action<Character, EquipmentItem>)Delegate.Combine(_armorUnequipped, new Action<Character, EquipmentItem>(p_listener.OnArmorUnequipped));
		_accessoryEquipped = (Action<Character, EquipmentItem>)Delegate.Combine(_accessoryEquipped, new Action<Character, EquipmentItem>(p_listener.OnAccessoryEquipped));
		_accessoryUnequipped = (Action<Character, EquipmentItem>)Delegate.Combine(_accessoryUnequipped, new Action<Character, EquipmentItem>(p_listener.OnAccessoryUnequipped));
	}

	public void UnsubscribeToWeaponEvents(IEquipmentListener p_listener)
	{
		_weaponEquipped = (Action<Character, EquipmentItem>)Delegate.Remove(_weaponEquipped, new Action<Character, EquipmentItem>(p_listener.OnWeaponEquipped));
		_weaponUnequipped = (Action<Character, EquipmentItem>)Delegate.Remove(_weaponUnequipped, new Action<Character, EquipmentItem>(p_listener.OnWeaponUnequipped));
		_armorEquipped = (Action<Character, EquipmentItem>)Delegate.Remove(_armorEquipped, new Action<Character, EquipmentItem>(p_listener.OnArmorEquipped));
		_armorUnequipped = (Action<Character, EquipmentItem>)Delegate.Remove(_armorUnequipped, new Action<Character, EquipmentItem>(p_listener.OnArmorUnequipped));
		_accessoryEquipped = (Action<Character, EquipmentItem>)Delegate.Remove(_accessoryEquipped, new Action<Character, EquipmentItem>(p_listener.OnAccessoryEquipped));
		_accessoryUnequipped = (Action<Character, EquipmentItem>)Delegate.Remove(_accessoryUnequipped, new Action<Character, EquipmentItem>(p_listener.OnAccessoryUnequipped));
	}

	public void ExecuteWeaponEquipped(Character p_character, EquipmentItem p_equipment)
	{
		_weaponEquipped?.Invoke(p_character, p_equipment);
	}

	public void ExecuteWeaponUnequipped(Character p_character, EquipmentItem p_equipment)
	{
		_weaponUnequipped?.Invoke(p_character, p_equipment);
	}

	public void ExecuteArmorEquipped(Character p_character, EquipmentItem p_equipment)
	{
		_armorEquipped?.Invoke(p_character, p_equipment);
	}

	public void ExecuteArmorUnequipped(Character p_character, EquipmentItem p_equipment)
	{
		_armorUnequipped?.Invoke(p_character, p_equipment);
	}

	public void ExecuteAccessoryEquipped(Character p_character, EquipmentItem p_equipment)
	{
		_accessoryEquipped?.Invoke(p_character, p_equipment);
	}

	public void ExecuteAccessoryUnequipped(Character p_character, EquipmentItem p_equipment)
	{
		_accessoryUnequipped?.Invoke(p_character, p_equipment);
	}

	public void SubscribeToInventoryEvents(IInventoryListener p_listener)
	{
		_itemObtained = (Action<Character, TileObject>)Delegate.Combine(_itemObtained, new Action<Character, TileObject>(p_listener.OnItemObtained));
		_itemLost = (Action<Character, TileObject>)Delegate.Combine(_itemLost, new Action<Character, TileObject>(p_listener.OnItemLost));
	}

	public void UnsubscribeToInventoryEvents(IInventoryListener p_listener)
	{
		_itemObtained = (Action<Character, TileObject>)Delegate.Remove(_itemObtained, new Action<Character, TileObject>(p_listener.OnItemObtained));
		_itemLost = (Action<Character, TileObject>)Delegate.Remove(_itemLost, new Action<Character, TileObject>(p_listener.OnItemLost));
	}

	public void ExecuteItemObtained(Character p_character, TileObject p_item)
	{
		_itemObtained?.Invoke(p_character, p_item);
	}

	public void ExecuteItemLost(Character p_character, TileObject p_item)
	{
		_itemLost?.Invoke(p_character, p_item);
	}

	public void SubscribeToFactionEvents(IFactionListener p_listener)
	{
		_joinedFaction = (Action<Character, Faction>)Delegate.Combine(_joinedFaction, new Action<Character, Faction>(p_listener.OnJoinFaction));
	}

	public void UnsubscribeToFactionEvents(IFactionListener p_listener)
	{
		_joinedFaction = (Action<Character, Faction>)Delegate.Remove(_joinedFaction, new Action<Character, Faction>(p_listener.OnJoinFaction));
	}

	public void ExecuteJoinedFaction(Character p_character, Faction p_faction)
	{
		_joinedFaction?.Invoke(p_character, p_faction);
	}

	public void SubscribeToCharacterArrivedAtSettlement(ILocationListener p_listener)
	{
		_characterArrivedAtSettlement = (Action<Character, NPCSettlement>)Delegate.Combine(_characterArrivedAtSettlement, new Action<Character, NPCSettlement>(p_listener.OnCharacterArrivedAtSettlement));
	}

	public void UnsubscribeToCharacterArrivedAtSettlement(ILocationListener p_listener)
	{
		_characterArrivedAtSettlement = (Action<Character, NPCSettlement>)Delegate.Remove(_characterArrivedAtSettlement, new Action<Character, NPCSettlement>(p_listener.OnCharacterArrivedAtSettlement));
	}

	public void ExecuteCharacterArrivedAtSettlement(Character p_character, NPCSettlement p_settlement)
	{
		_characterArrivedAtSettlement?.Invoke(p_character, p_settlement);
	}

	public void SubscribeToMoodEvents(IMoodListener p_listener)
	{
		_moodChanged = (Action<Character, MOOD_STATE, MOOD_STATE>)Delegate.Combine(_moodChanged, new Action<Character, MOOD_STATE, MOOD_STATE>(p_listener.OnMoodChanged));
	}

	public void UnsubscribeToMoodEvents(IMoodListener p_listener)
	{
		_moodChanged = (Action<Character, MOOD_STATE, MOOD_STATE>)Delegate.Remove(_moodChanged, new Action<Character, MOOD_STATE, MOOD_STATE>(p_listener.OnMoodChanged));
	}

	public void ExecuteMoodChangedEvent(Character p_character, MOOD_STATE p_previousMood, MOOD_STATE p_newMood)
	{
		_moodChanged?.Invoke(p_character, p_previousMood, p_newMood);
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}
}

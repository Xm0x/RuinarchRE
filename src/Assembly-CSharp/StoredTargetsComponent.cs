using System;
using System.Collections.Generic;
using Characters.Components;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Maccima_Games.Util;
using UtilityScripts;

public class StoredTargetsComponent : CharacterEventDispatcher.IDeathListener, TileObjectEventDispatcher.IDestroyedListener, LocationStructureEventDispatcher.IDestroyedListener
{
	public const int MaxCapacity = 8;

	public List<IStoredTarget> allStoredTargets { get; }

	public List<IStoredTarget> storedVillagers { get; }

	public List<IStoredTarget> storedMonsters { get; }

	public List<IStoredTarget> storedTileObjects { get; }

	public List<IStoredTarget> storedStructures { get; }

	public List<IStoredTarget> validRaidTargets { get; }

	public StoredTargetsComponent()
	{
		allStoredTargets = new List<IStoredTarget>();
		storedVillagers = new List<IStoredTarget>();
		storedMonsters = new List<IStoredTarget>();
		storedTileObjects = new List<IStoredTarget>();
		storedStructures = new List<IStoredTarget>();
		validRaidTargets = new List<IStoredTarget>();
	}

	public void SubscribeListeners()
	{
		Messenger.AddListener<Character, Character>(CharacterSignals.ON_SWITCH_FROM_LIMBO, OnCharacterSwitchedFromLimbo);
		Messenger.AddListener<BaseSettlement>(SettlementSignals.SETTLEMENT_CREATED, OnSettlementCreated);
		Messenger.AddListener<NPCSettlement>(SettlementSignals.DISCONNECT_FROM_SETTLEMENT, DisconnectFromSettlement);
	}

	private void OnCharacterSwitchedFromLimbo(Character p_inLimbo, Character p_active)
	{
		if (IsAlreadyStored((IStoredTarget)p_inLimbo))
		{
			Remove((IStoredTarget)p_inLimbo);
			Store((IStoredTarget)p_active);
		}
	}

	private void OnSettlementCreated(BaseSettlement p_settlement)
	{
		if (p_settlement is NPCSettlement p_settlement2)
		{
			UpdateRaidTargetsList(p_settlement2);
		}
	}

	private void DisconnectFromSettlement(NPCSettlement p_settlement)
	{
		validRaidTargets.Remove(p_settlement);
	}

	public void Store(IStoredTarget p_target)
	{
		p_target.SetAsStoredTarget(p_state: true);
		switch (p_target.storedTargetType)
		{
		case STORED_TARGET_TYPE.Monster:
		{
			Summon summon = p_target as Summon;
			allStoredTargets.Add(summon);
			Store(summon);
			break;
		}
		case STORED_TARGET_TYPE.Character:
			allStoredTargets.Add(p_target);
			Store(p_target as Character);
			break;
		case STORED_TARGET_TYPE.Tile_Objects:
			allStoredTargets.Add(p_target);
			Store(p_target as TileObject);
			break;
		case STORED_TARGET_TYPE.Structures:
			allStoredTargets.Add(p_target);
			Store(p_target as LocationStructure);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		Messenger.Broadcast(PlayerSignals.PLAYER_STORED_TARGET, p_target);
		if (allStoredTargets.Count > 8)
		{
			IStoredTarget p_target2 = allStoredTargets[0];
			Remove(p_target2);
		}
	}

	public bool HasStoredMaxCapacity()
	{
		return allStoredTargets.Count >= 8;
	}

	private void Store(Summon p_monster)
	{
		storedMonsters.Add(p_monster);
		Messenger.Broadcast(PlayerSignals.PLAYER_STORED_CHARACTER, (Character)p_monster);
		p_monster.eventDispatcher.SubscribeToCharacterDied(this);
	}

	private void Store(Character p_character)
	{
		storedVillagers.Add(p_character);
		Messenger.Broadcast(PlayerSignals.PLAYER_STORED_CHARACTER, p_character);
		p_character.eventDispatcher.SubscribeToCharacterDied(this);
	}

	private void Store(TileObject p_tileObject)
	{
		storedTileObjects.Add(p_tileObject);
		p_tileObject.eventDispatcher.SubscribeToTileObjectDestroyed(this);
	}

	private void Store(LocationStructure p_structure)
	{
		storedStructures.Add(p_structure);
		p_structure.eventDispatcher.SubscribeToStructureDestroyed(this);
	}

	public void Remove(IStoredTarget p_target)
	{
		allStoredTargets.Remove(p_target);
		p_target.SetAsStoredTarget(p_state: false);
		switch (p_target.storedTargetType)
		{
		case STORED_TARGET_TYPE.Monster:
			Remove(p_target as Summon);
			break;
		case STORED_TARGET_TYPE.Character:
			Remove(p_target as Character);
			break;
		case STORED_TARGET_TYPE.Tile_Objects:
			Remove(p_target as TileObject);
			break;
		case STORED_TARGET_TYPE.Structures:
			Remove(p_target as LocationStructure);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		Messenger.Broadcast(PlayerSignals.PLAYER_REMOVED_STORED_TARGET, p_target);
		Messenger.Broadcast(PlayerSkillSignals.FORCE_RELOAD_PLAYER_ACTIONS);
	}

	private void Remove(Summon p_monster)
	{
		if (storedMonsters.Remove(p_monster))
		{
			Messenger.Broadcast(PlayerSignals.PLAYER_REMOVED_STORED_CHARACTER, (Character)p_monster);
		}
		p_monster.eventDispatcher.UnsubscribeToCharacterDied(this);
	}

	private void Remove(Character p_character)
	{
		if (storedVillagers.Remove(p_character))
		{
			Messenger.Broadcast(PlayerSignals.PLAYER_REMOVED_STORED_CHARACTER, p_character);
		}
		p_character.eventDispatcher.UnsubscribeToCharacterDied(this);
	}

	private void Remove(TileObject p_tileObject)
	{
		storedTileObjects.Remove(p_tileObject);
		p_tileObject.eventDispatcher.UnsubscribeToTileObjectDestroyed(this);
	}

	private void Remove(LocationStructure p_structure)
	{
		storedStructures.Remove(p_structure);
		p_structure.eventDispatcher.UnsubscribeToStructureDestroyed(this);
	}

	public bool IsAlreadyStored(IStoredTarget p_target)
	{
		switch (p_target.storedTargetType)
		{
		case STORED_TARGET_TYPE.Monster:
		{
			Summon summon = p_target as Summon;
			bool flag2 = false;
			if (summon.lycanData != null)
			{
				flag2 = IsAlreadyStored(summon.lycanData.originalForm);
			}
			if (!flag2)
			{
				flag2 = IsAlreadyStored(summon);
			}
			return flag2;
		}
		case STORED_TARGET_TYPE.Character:
		{
			Character character = p_target as Character;
			bool flag = false;
			if (character.lycanData != null && character.lycanData.lycanthropeForm != null)
			{
				flag = IsAlreadyStored(character.lycanData.lycanthropeForm as Summon);
			}
			if (!flag)
			{
				flag = IsAlreadyStored(character);
			}
			return flag;
		}
		case STORED_TARGET_TYPE.Tile_Objects:
			return IsAlreadyStored(p_target as TileObject);
		case STORED_TARGET_TYPE.Structures:
			return IsAlreadyStored(p_target as LocationStructure);
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private bool IsAlreadyStored(Character p_character)
	{
		return p_character.isStoredAsTarget;
	}

	private bool IsAlreadyStored(Summon p_monster)
	{
		return p_monster.isStoredAsTarget;
	}

	private bool IsAlreadyStored(TileObject p_tileObject)
	{
		return p_tileObject.isStoredAsTarget;
	}

	private bool IsAlreadyStored(LocationStructure p_structure)
	{
		return p_structure.isStoredAsTarget;
	}

	public void LoadReferences(SaveDataStoredTargetsComponent data)
	{
		for (int i = 0; i < data.allStoredTargets.Count; i++)
		{
			string id = data.allStoredTargets[i];
			IStoredTarget p_target;
			switch (data.allStoredTargetTypes[i])
			{
			case STORED_TARGET_TYPE.Character:
			case STORED_TARGET_TYPE.Monster:
				p_target = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(id);
				break;
			case STORED_TARGET_TYPE.Tile_Objects:
				p_target = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(id);
				break;
			case STORED_TARGET_TYPE.Structures:
				p_target = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(id);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			Store(p_target);
		}
		if (data.validRaidTargets != null)
		{
			for (int j = 0; j < data.validRaidTargets.Length; j++)
			{
				string id2 = data.validRaidTargets[j];
				BaseSettlement settlementByPersistentID = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(id2);
				validRaidTargets.Add(settlementByPersistentID);
			}
		}
	}

	public void OnCharacterSubscribedToDied(Character p_character)
	{
		Remove((IStoredTarget)p_character);
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
		dictionary.Add("name", p_character.name);
		PopUpNotificationUI.Instance.ShowPlayerPoppingTextNotif(Utilities.YellowDotIcon() + LocalizationManager.Instance.GetLocalizedValue("OtherUI_Table", "Lost_Target", dictionary), 5, PlayerUI.Instance.popUpDisplayPoint);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
	}

	public void OnTileObjectDestroyed(TileObject p_tileObject)
	{
		Remove((IStoredTarget)p_tileObject);
	}

	public void OnStructureDestroyed(LocationStructure p_structure)
	{
		Remove((IStoredTarget)p_structure);
	}

	public void UpdateRaidTargetsList(NPCSettlement p_settlement = null)
	{
		validRaidTargets.Clear();
		for (int i = 0; i < LandmarkManager.Instance.allNonPlayerSettlements.Count; i++)
		{
			BaseSettlement baseSettlement = LandmarkManager.Instance.allNonPlayerSettlements[i];
			if (baseSettlement.areas.Count > 0 && IsValidRaidTarget(baseSettlement))
			{
				validRaidTargets.Add(baseSettlement);
			}
		}
		if (p_settlement != null && !LandmarkManager.Instance.allNonPlayerSettlements.Contains(p_settlement) && IsValidRaidTarget(p_settlement) && !validRaidTargets.Contains(p_settlement))
		{
			validRaidTargets.Add(p_settlement);
		}
	}

	private bool IsValidRaidTarget(BaseSettlement p_settlement)
	{
		if (p_settlement.locationType == LOCATION_TYPE.VILLAGE || p_settlement.HasStructure(STRUCTURE_TYPE.HALLOWED_GROUND))
		{
			return !p_settlement.hasBeenDestroyed;
		}
		return false;
	}
}

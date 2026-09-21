using System;
using System.Collections.Generic;
using System.Linq;
using Factions.Faction_Succession;
using Factions.Faction_Types;
using Inner_Maps;
using Locations.Settlements;
using Object_Pools;
using Traits;
using UnityEngine;
using UnityEngine.Localization;
using UtilityScripts;

public class FactionManager : BaseMonoBehaviour, LocalizationManagerEventDispatcher.ILocaleChangeListener
{
	public static FactionManager Instance;

	private Faction _undeadFaction;

	[Space(10f)]
	[Header("Character Name Colors")]
	public Color factionNameColor;

	private string _factionNameColorHex;

	private Dictionary<FACTION_SUCCESSION_TYPE, FactionSuccession> _factionSuccessions = new Dictionary<FACTION_SUCCESSION_TYPE, FactionSuccession>();

	public Faction wildMonsterFaction { get; private set; }

	public Faction vagrantFaction { get; private set; }

	public Faction disguisedFaction { get; private set; }

	public Faction ratmenFaction { get; private set; }

	public Faction banditFaction { get; private set; }

	public Faction retaliatorFaction { get; private set; }

	public Faction demonCultFaction { get; private set; }

	public Faction divineCultFaction { get; private set; }

	public Faction natureCultFaction { get; private set; }

	public Faction undeadFaction
	{
		get
		{
			if (_undeadFaction == null)
			{
				_undeadFaction = CreateUndeadFaction();
			}
			return _undeadFaction;
		}
	}

	public bool hasUndeadFaction => _undeadFaction != null;

	public List<Faction> allFactions => DatabaseManager.Instance.factionDatabase.allFactionsList;

	public int maxActiveVillagerFactions => WorldSettings.Instance.worldSettingsData.mapSettings.GetMaxActiveFactionsForMapSize();

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		_factionNameColorHex = ColorUtility.ToHtmlStringRGB(factionNameColor);
		ConstructFactionSuccessionTypes();
	}

	protected override void OnDestroy()
	{
		_ = LocalizationManager.Instance != null;
		wildMonsterFaction = null;
		vagrantFaction = null;
		disguisedFaction = null;
		_undeadFaction = null;
		_factionSuccessions?.Clear();
		_factionSuccessions = null;
		base.OnDestroy();
		Instance = null;
	}

	public void CreateWildMonsterFaction()
	{
		Faction faction = new Faction(FACTION_TYPE.Wild_Monsters);
		faction.SetIsMajorFaction(state: false);
		faction.SetName(LocalizationManager.Instance.GetLocalizedValue("Faction_Table", "Wild_Monsters"));
		faction.SetFactionActiveState(state: false);
		faction.SetEmblem(FactionEmblemRandomizer.wildMonsterFactionEmblem);
		faction.factionType.SetAsDefault(faction);
		DatabaseManager.Instance.factionDatabase.RegisterFaction(faction);
		SetWildMonsterFaction(faction);
		CreateRelationshipsForFaction(faction);
		Messenger.Broadcast(FactionSignals.FACTION_CREATED, faction);
	}

	public void CreateVagrantFaction()
	{
		Faction faction = new Faction(FACTION_TYPE.Vagrants);
		faction.SetIsMajorFaction(state: false);
		faction.SetName(LocalizationManager.Instance.GetLocalizedValue("Faction_Table", "Vagrants"));
		faction.SetFactionActiveState(state: false);
		faction.SetEmblem(FactionEmblemRandomizer.vagrantFactionEmblem);
		faction.factionType.SetAsDefault(faction);
		DatabaseManager.Instance.factionDatabase.RegisterFaction(faction);
		SetVagrantFaction(faction);
		CreateRelationshipsForFaction(faction);
		Messenger.Broadcast(FactionSignals.FACTION_CREATED, faction);
		faction.SetIsInfoUnlocked(p_state: true);
	}

	public void CreateDisguisedFaction()
	{
		Faction faction = new Faction(FACTION_TYPE.Disguised);
		faction.SetIsMajorFaction(state: false);
		faction.SetName(LocalizationManager.Instance.GetLocalizedValue("Faction_Table", "Disguised"));
		faction.SetFactionActiveState(state: false);
		faction.SetEmblem(FactionEmblemRandomizer.disguisedFactionEmblem);
		faction.factionType.SetAsDefault(faction);
		DatabaseManager.Instance.factionDatabase.RegisterFaction(faction);
		SetDisguisedFaction(faction);
		CreateRelationshipsForFaction(faction);
		Messenger.Broadcast(FactionSignals.FACTION_CREATED, faction);
	}

	public void CreateRatmenFaction()
	{
		Faction faction = new Faction(FACTION_TYPE.Ratmen);
		faction.SetIsMajorFaction(state: false);
		faction.SetName(LocalizationManager.Instance.GetLocalizedValue("Faction_Table", "Ratmen"));
		faction.SetFactionActiveState(state: false);
		faction.SetEmblem(FactionEmblemRandomizer.ratmenFactionEmblem);
		faction.factionType.SetAsDefault(faction);
		faction.SetPathfindingTag(7u);
		faction.SetPathfindingDoorTag(8u);
		DatabaseManager.Instance.factionDatabase.RegisterFaction(faction);
		SetRatmenFaction(faction);
		CreateRelationshipsForFaction(faction);
		Messenger.Broadcast(FactionSignals.FACTION_CREATED, faction);
	}

	public void CreateBanditFaction()
	{
		Faction faction = new Faction(FACTION_TYPE.Bandits);
		faction.SetIsMajorFaction(state: false);
		faction.SetName(LocalizationManager.Instance.GetLocalizedValue("Faction_Table", "Bandits"));
		faction.SetFactionActiveState(state: false);
		faction.SetEmblem(FactionEmblemRandomizer.banditFactionEmblem);
		faction.factionType.SetAsDefault(faction);
		faction.SetPathfindingTag(9u);
		faction.SetPathfindingDoorTag(10u);
		DatabaseManager.Instance.factionDatabase.RegisterFaction(faction);
		SetBanditFaction(faction);
		CreateRelationshipsForFaction(faction);
		Messenger.Broadcast(FactionSignals.FACTION_CREATED, faction);
	}

	public void CreateRetaliatorFaction()
	{
		Faction faction = new Faction(FACTION_TYPE.Retaliator);
		faction.SetIsMajorFaction(state: false);
		faction.SetName(LocalizationManager.Instance.GetLocalizedValue("Faction_Table", "Retaliator"));
		faction.SetFactionActiveState(state: false);
		faction.SetEmblem(FactionEmblemRandomizer.retaliatorFactionEmblem);
		faction.factionType.SetAsDefault(faction);
		DatabaseManager.Instance.factionDatabase.RegisterFaction(faction);
		SetRetaliatorFaction(faction);
		CreateRelationshipsForFaction(faction);
		Messenger.Broadcast(FactionSignals.FACTION_CREATED, faction);
	}

	public void CreateDemonCultFaction()
	{
		Faction faction = new Faction(FACTION_TYPE.Demon_Cult);
		faction.SetIsMajorFaction(state: true);
		faction.SetName(LocalizationManager.Instance.GetLocalizedValue("Faction_Table", "Demon_Cult"));
		faction.SetEmblem(FactionEmblemRandomizer.cultFactionEmblem);
		faction.factionType.SetAsDefault(faction);
		faction.SetPathfindingTag(11u);
		faction.SetPathfindingDoorTag(12u);
		DatabaseManager.Instance.factionDatabase.RegisterFaction(faction);
		SetDemonCultFaction(faction);
		CreateRelationshipsForFaction(faction);
		Messenger.Broadcast(FactionSignals.FACTION_CREATED, faction);
	}

	public void CreateDivineCultFaction()
	{
		Faction faction = new Faction(FACTION_TYPE.Divine_Church);
		faction.SetIsMajorFaction(state: true);
		faction.SetName(LocalizationManager.Instance.GetLocalizedValue("Faction_Table", "Divine_Church"));
		faction.SetEmblem(FactionEmblemRandomizer.cultFactionEmblem);
		faction.factionType.SetAsDefault(faction);
		faction.SetIsInfoUnlocked(p_state: false);
		faction.SetPathfindingTag(13u);
		faction.SetPathfindingDoorTag(14u);
		DatabaseManager.Instance.factionDatabase.RegisterFaction(faction);
		SetDivineCultFaction(faction);
		CreateRelationshipsForFaction(faction);
		Messenger.Broadcast(FactionSignals.FACTION_CREATED, faction);
	}

	public void CreateNatureCultFaction()
	{
		Faction faction = new Faction(FACTION_TYPE.Wiccans);
		faction.SetIsMajorFaction(state: true);
		faction.SetName(LocalizationManager.Instance.GetLocalizedValue("Faction_Table", "Wiccans"));
		faction.SetEmblem(FactionEmblemRandomizer.cultFactionEmblem);
		faction.factionType.SetAsDefault(faction);
		faction.SetIsInfoUnlocked(p_state: false);
		faction.SetPathfindingTag(15u);
		faction.SetPathfindingDoorTag(16u);
		DatabaseManager.Instance.factionDatabase.RegisterFaction(faction);
		SetNatureCultFaction(faction);
		CreateRelationshipsForFaction(faction);
		Messenger.Broadcast(FactionSignals.FACTION_CREATED, faction);
	}

	public Faction CreateReligiousCultFactionForReligion(RELIGION p_religion)
	{
		switch (p_religion)
		{
		case RELIGION.Demon_Worship:
			if (demonCultFaction == null)
			{
				CreateDemonCultFaction();
			}
			return demonCultFaction;
		case RELIGION.Divine_Worship:
			if (divineCultFaction == null)
			{
				CreateDivineCultFaction();
			}
			return divineCultFaction;
		case RELIGION.Nature_Worship:
			if (natureCultFaction == null)
			{
				CreateNatureCultFaction();
			}
			return natureCultFaction;
		default:
			throw new ArgumentOutOfRangeException("p_religion", p_religion, null);
		}
	}

	private void SetWildMonsterFaction(Faction faction)
	{
		wildMonsterFaction = faction;
	}

	private void SetVagrantFaction(Faction faction)
	{
		vagrantFaction = faction;
	}

	private void SetDisguisedFaction(Faction faction)
	{
		disguisedFaction = faction;
	}

	private void SetUndeadFaction(Faction faction)
	{
		_undeadFaction = faction;
	}

	private void SetRatmenFaction(Faction faction)
	{
		ratmenFaction = faction;
	}

	private void SetBanditFaction(Faction faction)
	{
		banditFaction = faction;
	}

	private void SetRetaliatorFaction(Faction faction)
	{
		retaliatorFaction = faction;
	}

	private void SetDemonCultFaction(Faction faction)
	{
		demonCultFaction = faction;
	}

	private void SetDivineCultFaction(Faction faction)
	{
		divineCultFaction = faction;
	}

	private void SetNatureCultFaction(Faction faction)
	{
		natureCultFaction = faction;
	}

	public Faction CreateNewFaction(FACTION_TYPE factionType, string factionName = "", Sprite factionEmblem = null, RACE race = RACE.NONE)
	{
		Faction faction = new Faction(factionType, race);
		DatabaseManager.Instance.factionDatabase.RegisterFaction(faction);
		faction.SetIsMajorFaction(state: true);
		if (factionEmblem == null)
		{
			DetermineFactionEmblem(faction);
		}
		else
		{
			faction.SetEmblem(factionEmblem);
		}
		DetermineFactionPathfindingTags(faction);
		if (!string.IsNullOrEmpty(factionName))
		{
			faction.SetName(factionName);
		}
		CreateRelationshipsForFaction(faction);
		if (!faction.isPlayerFaction)
		{
			Messenger.Broadcast(FactionSignals.FACTION_CREATED, faction);
		}
		if (faction.race.IsSapient())
		{
			if (faction.factionType.type == FACTION_TYPE.Undead || faction.factionType.type == FACTION_TYPE.Demon_Cult)
			{
				faction.SetIsInfoUnlocked(p_state: true);
			}
			else
			{
				faction.SetIsInfoUnlocked(p_state: false);
			}
		}
		if (faction.factionType.type == FACTION_TYPE.Demon_Cult)
		{
			SetDemonCultFaction(faction);
		}
		return faction;
	}

	private void DetermineFactionEmblem(Faction faction)
	{
		switch (faction.factionType.type)
		{
		case FACTION_TYPE.Demons:
			faction.SetEmblem(FactionEmblemRandomizer.playerFactionEmblem);
			return;
		case FACTION_TYPE.Undead:
			faction.SetEmblem(FactionEmblemRandomizer.undeadFactionEmblem);
			return;
		case FACTION_TYPE.Ratmen:
			faction.SetEmblem(FactionEmblemRandomizer.ratmenFactionEmblem);
			return;
		case FACTION_TYPE.Bandits:
			faction.SetEmblem(FactionEmblemRandomizer.banditFactionEmblem);
			return;
		case FACTION_TYPE.Demon_Cult:
			faction.SetEmblem(FactionEmblemRandomizer.cultFactionEmblem);
			return;
		case FACTION_TYPE.Divine_Church:
			faction.SetEmblem(FactionEmblemRandomizer.cultFactionEmblem);
			return;
		case FACTION_TYPE.Wiccans:
			faction.SetEmblem(FactionEmblemRandomizer.cultFactionEmblem);
			return;
		}
		Sprite unusedFactionEmblem = FactionEmblemRandomizer.GetUnusedFactionEmblem();
		faction.SetEmblem(unusedFactionEmblem);
		FactionEmblemRandomizer.SetEmblemAsUsed(unusedFactionEmblem);
	}

	private void DetermineFactionPathfindingTags(Faction faction)
	{
		switch (faction.factionType.type)
		{
		case FACTION_TYPE.Demons:
			faction.SetPathfindingTag(3u);
			faction.SetPathfindingDoorTag(4u);
			return;
		case FACTION_TYPE.Undead:
			faction.SetPathfindingTag(5u);
			faction.SetPathfindingDoorTag(6u);
			return;
		case FACTION_TYPE.Ratmen:
			faction.SetPathfindingTag(7u);
			faction.SetPathfindingDoorTag(8u);
			return;
		case FACTION_TYPE.Bandits:
			faction.SetPathfindingTag(9u);
			faction.SetPathfindingDoorTag(10u);
			return;
		case FACTION_TYPE.Demon_Cult:
			faction.SetPathfindingTag(11u);
			faction.SetPathfindingDoorTag(12u);
			return;
		case FACTION_TYPE.Divine_Church:
			faction.SetPathfindingTag(13u);
			faction.SetPathfindingDoorTag(14u);
			return;
		case FACTION_TYPE.Wiccans:
			faction.SetPathfindingTag(15u);
			faction.SetPathfindingDoorTag(16u);
			return;
		}
		if (faction.isMajorNonPlayer)
		{
			PathfindingTagPair pathfindingTagPair = InnerMapManager.Instance.ClaimNextPathfindingTagPair();
			faction.SetPathfindingTag(pathfindingTagPair.groundTag);
			faction.SetPathfindingDoorTag(pathfindingTagPair.doorsTag);
		}
	}

	private Faction CreateUndeadFaction()
	{
		Faction faction = CreateNewFaction(FACTION_TYPE.Undead, LocalizationManager.Instance.GetLocalizedValue("Faction_Table", "Undead"));
		faction.SetIsMajorFaction(state: false);
		CreateRelationshipsForFaction(faction);
		return faction;
	}

	public Faction CreateNewFaction(SaveDataFaction data)
	{
		Faction faction = new Faction(data);
		if (data.factionType.type == FACTION_TYPE.Disguised)
		{
			SetDisguisedFaction(faction);
		}
		else if (data.factionType.type == FACTION_TYPE.Undead)
		{
			SetUndeadFaction(faction);
		}
		else if (data.factionType.type == FACTION_TYPE.Wild_Monsters)
		{
			SetWildMonsterFaction(faction);
		}
		else if (data.factionType.type == FACTION_TYPE.Vagrants)
		{
			SetVagrantFaction(faction);
		}
		else if (data.factionType.type == FACTION_TYPE.Ratmen)
		{
			SetRatmenFaction(faction);
		}
		else if (data.factionType.type == FACTION_TYPE.Bandits)
		{
			SetBanditFaction(faction);
		}
		else if (data.factionType.type == FACTION_TYPE.Retaliator)
		{
			SetRetaliatorFaction(faction);
		}
		else if (data.factionType.type == FACTION_TYPE.Demon_Cult)
		{
			SetDemonCultFaction(faction);
		}
		else if (data.factionType.type == FACTION_TYPE.Divine_Church)
		{
			SetDivineCultFaction(faction);
		}
		else if (data.factionType.type == FACTION_TYPE.Wiccans)
		{
			SetNatureCultFaction(faction);
		}
		DatabaseManager.Instance.factionDatabase.RegisterFaction(faction);
		if (!faction.isPlayerFaction)
		{
			Messenger.Broadcast(FactionSignals.FACTION_CREATED, faction);
		}
		return faction;
	}

	public void DeleteFaction(Faction faction)
	{
	}

	public Faction GetRandomMajorNonPlayerFaction()
	{
		return DatabaseManager.Instance.factionDatabase.GetRandomMajorNonPlayerFaction();
	}

	public bool LeaveFaction(Character character)
	{
		return character.ChangeToDefaultFaction();
	}

	public Sprite GetFactionEmblem(SaveDataFaction p_data)
	{
		if (p_data.factionType.type == FACTION_TYPE.Wild_Monsters)
		{
			return FactionEmblemRandomizer.wildMonsterFactionEmblem;
		}
		if (p_data.factionType.type == FACTION_TYPE.Vagrants)
		{
			return FactionEmblemRandomizer.vagrantFactionEmblem;
		}
		if (p_data.factionType.type == FACTION_TYPE.Disguised)
		{
			return FactionEmblemRandomizer.disguisedFactionEmblem;
		}
		if (p_data.factionType.type == FACTION_TYPE.Undead)
		{
			return FactionEmblemRandomizer.undeadFactionEmblem;
		}
		if (p_data.factionType.type == FACTION_TYPE.Demons)
		{
			return FactionEmblemRandomizer.playerFactionEmblem;
		}
		if (p_data.factionType.type == FACTION_TYPE.Ratmen)
		{
			return FactionEmblemRandomizer.ratmenFactionEmblem;
		}
		if (p_data.factionType.type == FACTION_TYPE.Bandits)
		{
			return FactionEmblemRandomizer.banditFactionEmblem;
		}
		if (p_data.factionType.type == FACTION_TYPE.Retaliator)
		{
			return FactionEmblemRandomizer.retaliatorFactionEmblem;
		}
		if (p_data.factionType.type == FACTION_TYPE.Demon_Cult)
		{
			return FactionEmblemRandomizer.cultFactionEmblem;
		}
		if (p_data.factionType.type == FACTION_TYPE.Divine_Church)
		{
			return FactionEmblemRandomizer.cultFactionEmblem;
		}
		if (p_data.factionType.type == FACTION_TYPE.Wiccans)
		{
			return FactionEmblemRandomizer.cultFactionEmblem;
		}
		for (int i = 0; i < FactionEmblemRandomizer.allEmblems.Count; i++)
		{
			Sprite sprite = FactionEmblemRandomizer.allEmblems[i];
			if (sprite.name == p_data.emblemName)
			{
				return sprite;
			}
		}
		return null;
	}

	public Faction GetFactionBasedOnID(int id)
	{
		return DatabaseManager.Instance.factionDatabase.GetFactionBasedOnID(id);
	}

	public Faction GetFactionByPersistentID(string id)
	{
		return DatabaseManager.Instance.factionDatabase.GetFactionByPersistentID(id);
	}

	public Faction GetFactionBasedOnName(string name)
	{
		return DatabaseManager.Instance.factionDatabase.GetFactionBasedOnName(name);
	}

	public List<Faction> GetMajorFactionWithRace(RACE race)
	{
		return DatabaseManager.Instance.factionDatabase.GetMajorFactionWithRace(race);
	}

	public string GetFactionNameColorHex()
	{
		return _factionNameColorHex;
	}

	public Faction GetDefaultFactionForMonster(SUMMON_TYPE summonType)
	{
		switch (summonType)
		{
		case SUMMON_TYPE.Skeleton:
		case SUMMON_TYPE.Ghost:
		case SUMMON_TYPE.Vengeful_Ghost:
		case SUMMON_TYPE.Revenant:
		case SUMMON_TYPE.Ghoul:
		case SUMMON_TYPE.Whisperer:
			return undeadFaction;
		default:
			return wildMonsterFaction;
		}
	}

	public int GetActiveVillagerFactionCount()
	{
		int num = 0;
		for (int i = 0; i < DatabaseManager.Instance.factionDatabase.allFactionsList.Count; i++)
		{
			Faction faction = DatabaseManager.Instance.factionDatabase.allFactionsList[i];
			if (faction.isMajorNonPlayer && !faction.isDisbanded && faction.isActive)
			{
				num++;
			}
		}
		return num;
	}

	public bool HasMajorFactionHostileWithPlayer()
	{
		for (int i = 0; i < allFactions.Count; i++)
		{
			Faction faction = allFactions[i];
			if (faction.isMajorNonPlayer && faction.IsHostileWith(PlayerManager.Instance.player.playerFaction))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasMajorFactionWithAliveMembers()
	{
		for (int i = 0; i < allFactions.Count; i++)
		{
			Faction faction = allFactions[i];
			if (faction.isMajorNonPlayer && faction.characters.Any((Character c) => !c.isDead))
			{
				return true;
			}
		}
		return false;
	}

	private void CreateRelationshipsForFaction(Faction faction)
	{
		for (int i = 0; i < DatabaseManager.Instance.factionDatabase.allFactionsList.Count; i++)
		{
			Faction faction2 = DatabaseManager.Instance.factionDatabase.allFactionsList[i];
			if (faction2.id != faction.id && ((faction2.isMajorNonPlayer && !faction2.isDisbanded) || !faction2.isMajorNonPlayer || faction2.isPlayerFaction))
			{
				CreateNewRelationshipBetween(faction2, faction);
			}
		}
	}

	public void RemoveRelationshipsWith(Faction faction)
	{
		for (int i = 0; i < DatabaseManager.Instance.factionDatabase.allFactionsList.Count; i++)
		{
			Faction faction2 = DatabaseManager.Instance.factionDatabase.allFactionsList[i];
			if (faction2.id != faction.id)
			{
				faction2.RemoveRelationshipWith(faction);
				faction.RemoveRelationshipWith(faction2);
			}
		}
	}

	public FactionRelationship CreateNewRelationshipBetween(Faction faction1, Faction faction2)
	{
		FactionRelationship factionRelationship = new FactionRelationship(faction1, faction2);
		faction1.AddNewRelationship(faction2, factionRelationship);
		faction2.AddNewRelationship(faction1, factionRelationship);
		FACTION_RELATIONSHIP_STATUS initialFactionRelationshipStatus = GetInitialFactionRelationshipStatus(faction1, faction2);
		faction1.SetRelationshipFor(faction2, initialFactionRelationshipStatus);
		faction2.SetRelationshipFor(faction1, initialFactionRelationshipStatus);
		return factionRelationship;
	}

	private FACTION_RELATIONSHIP_STATUS GetInitialFactionRelationshipStatus(Faction faction1, Faction faction2)
	{
		if (faction1.isPlayerFaction || faction2.isPlayerFaction)
		{
			if (faction1.factionType.type == FACTION_TYPE.Wild_Monsters || faction2.factionType.type == FACTION_TYPE.Wild_Monsters)
			{
				return FACTION_RELATIONSHIP_STATUS.Neutral;
			}
			if (faction1.factionType.type == FACTION_TYPE.Demon_Cult || faction2.factionType.type == FACTION_TYPE.Demon_Cult)
			{
				return FACTION_RELATIONSHIP_STATUS.Friendly;
			}
			return FACTION_RELATIONSHIP_STATUS.Hostile;
		}
		if (faction1.factionType.type == FACTION_TYPE.Wild_Monsters || faction2.factionType.type == FACTION_TYPE.Wild_Monsters)
		{
			if (faction1.isPlayerFaction || faction2.isPlayerFaction)
			{
				return FACTION_RELATIONSHIP_STATUS.Neutral;
			}
			return FACTION_RELATIONSHIP_STATUS.Hostile;
		}
		if (faction1.factionType.type == FACTION_TYPE.Undead || faction2.factionType.type == FACTION_TYPE.Undead)
		{
			if (faction1.factionType.type == FACTION_TYPE.Ratmen || faction2.factionType.type == FACTION_TYPE.Ratmen)
			{
				return FACTION_RELATIONSHIP_STATUS.Neutral;
			}
			return FACTION_RELATIONSHIP_STATUS.Hostile;
		}
		if (faction1.factionType.type == FACTION_TYPE.Ratmen || faction2.factionType.type == FACTION_TYPE.Ratmen)
		{
			if (faction1.factionType.type == FACTION_TYPE.Undead || faction2.factionType.type == FACTION_TYPE.Undead)
			{
				return FACTION_RELATIONSHIP_STATUS.Neutral;
			}
			return FACTION_RELATIONSHIP_STATUS.Hostile;
		}
		if (faction1.factionType.type == FACTION_TYPE.Bandits || faction2.factionType.type == FACTION_TYPE.Bandits)
		{
			return FACTION_RELATIONSHIP_STATUS.Hostile;
		}
		if (faction1.factionType.type == FACTION_TYPE.Wiccans || faction2.factionType.type == FACTION_TYPE.Wiccans)
		{
			if (faction1.factionType.type == FACTION_TYPE.Divine_Church || faction2.factionType.type == FACTION_TYPE.Divine_Church)
			{
				return FACTION_RELATIONSHIP_STATUS.Hostile;
			}
			if (faction1.factionType.type == FACTION_TYPE.Demon_Cult || faction2.factionType.type == FACTION_TYPE.Demon_Cult)
			{
				return FACTION_RELATIONSHIP_STATUS.Hostile;
			}
			return FACTION_RELATIONSHIP_STATUS.Neutral;
		}
		if (faction1.factionType.type == FACTION_TYPE.Divine_Church || faction2.factionType.type == FACTION_TYPE.Divine_Church)
		{
			if (faction1.factionType.type == FACTION_TYPE.Wiccans || faction2.factionType.type == FACTION_TYPE.Wiccans)
			{
				return FACTION_RELATIONSHIP_STATUS.Hostile;
			}
			if (faction1.factionType.type == FACTION_TYPE.Demon_Cult || faction2.factionType.type == FACTION_TYPE.Demon_Cult)
			{
				return FACTION_RELATIONSHIP_STATUS.Hostile;
			}
			return FACTION_RELATIONSHIP_STATUS.Neutral;
		}
		if (faction1.factionType.type == FACTION_TYPE.Demon_Cult || faction2.factionType.type == FACTION_TYPE.Demon_Cult)
		{
			if (faction1.factionType.type == FACTION_TYPE.Wiccans || faction2.factionType.type == FACTION_TYPE.Wiccans)
			{
				return FACTION_RELATIONSHIP_STATUS.Hostile;
			}
			if (faction1.factionType.type == FACTION_TYPE.Divine_Church || faction2.factionType.type == FACTION_TYPE.Divine_Church)
			{
				return FACTION_RELATIONSHIP_STATUS.Hostile;
			}
			if (faction1.isPlayerFaction || faction2.isPlayerFaction)
			{
				return FACTION_RELATIONSHIP_STATUS.Friendly;
			}
			return FACTION_RELATIONSHIP_STATUS.Neutral;
		}
		return FACTION_RELATIONSHIP_STATUS.Neutral;
	}

	public FactionRelationship GetRelationshipBetween(Faction faction1, Faction faction2)
	{
		FactionRelationship relationshipWith = faction1.GetRelationshipWith(faction2);
		FactionRelationship relationshipWith2 = faction2.GetRelationshipWith(faction1);
		if (relationshipWith == relationshipWith2)
		{
			return relationshipWith;
		}
		throw new Exception(faction1.name + " does not have the same relationship object as " + faction2.name + "!");
	}

	public void RerollFactionRelationships(Faction faction, Character leader, bool isRerollForNewFaction, bool logRelationshipChangeFromLeaderRelationship)
	{
		for (int i = 0; i < allFactions.Count; i++)
		{
			Faction faction2 = allFactions[i];
			if (faction2.id == faction.id)
			{
				continue;
			}
			FactionRelationship relationshipWith = faction.GetRelationshipWith(faction2);
			if (relationshipWith == null)
			{
				continue;
			}
			FACTION_RELATIONSHIP_STATUS relationshipStatus = relationshipWith.relationshipStatus;
			if (faction.factionType.type == FACTION_TYPE.Demon_Cult)
			{
				if (faction2.factionType.type == FACTION_TYPE.Divine_Church || faction2.factionType.type == FACTION_TYPE.Wiccans)
				{
					relationshipStatus = FACTION_RELATIONSHIP_STATUS.Hostile;
					relationshipWith.SetRelationshipStatus(relationshipStatus);
					continue;
				}
			}
			else if (faction.factionType.type == FACTION_TYPE.Divine_Church)
			{
				if (faction2.factionType.type == FACTION_TYPE.Demon_Cult || faction2.factionType.type == FACTION_TYPE.Wiccans)
				{
					relationshipStatus = FACTION_RELATIONSHIP_STATUS.Hostile;
					relationshipWith.SetRelationshipStatus(relationshipStatus);
					continue;
				}
			}
			else if (faction.factionType.type == FACTION_TYPE.Wiccans && (faction2.factionType.type == FACTION_TYPE.Divine_Church || faction2.factionType.type == FACTION_TYPE.Demon_Cult))
			{
				relationshipStatus = FACTION_RELATIONSHIP_STATUS.Hostile;
				relationshipWith.SetRelationshipStatus(relationshipStatus);
				continue;
			}
			if (leader.traitContainer.IsReligiousCultist(out var p_religion))
			{
				Character character = faction2.leader as Character;
				RELIGION p_religion2;
				if (faction2.factionType is CultFaction cultFaction)
				{
					if (character != null)
					{
						IRelationshipData relationshipDataWith = character.relationshipContainer.GetRelationshipDataWith(leader);
						if (relationshipDataWith != null && relationshipDataWith.opinions.HasOpinion("Rebellion"))
						{
							relationshipWith.SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.Hostile);
							continue;
						}
					}
					switch (p_religion)
					{
					case RELIGION.Demon_Worship:
						if (faction2.isPlayerFaction)
						{
							relationshipWith.SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.Friendly);
							continue;
						}
						if (cultFaction.cultReligion != p_religion)
						{
							relationshipWith.SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.Hostile);
							continue;
						}
						if (cultFaction.cultReligion == p_religion)
						{
							relationshipWith.SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.Friendly);
							continue;
						}
						break;
					case RELIGION.Divine_Worship:
					case RELIGION.Nature_Worship:
						if (cultFaction.cultReligion != p_religion)
						{
							relationshipWith.SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.Hostile);
							continue;
						}
						if (cultFaction.cultReligion == p_religion)
						{
							relationshipWith.SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.Friendly);
							continue;
						}
						break;
					}
				}
				else if (character != null && character.traitContainer.IsReligiousCultist(out p_religion2) && p_religion == p_religion2)
				{
					IRelationshipData relationshipDataWith2 = character.relationshipContainer.GetRelationshipDataWith(leader);
					if (relationshipDataWith2 != null && relationshipDataWith2.opinions.HasOpinion("Rebellion"))
					{
						relationshipWith.SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.Hostile);
					}
					else
					{
						relationshipWith.SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.Friendly);
					}
					continue;
				}
			}
			if (faction.factionType.type == FACTION_TYPE.Bandits)
			{
				relationshipWith.SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.Hostile);
				continue;
			}
			if (faction.factionType.HasIdeology(FACTION_IDEOLOGY.Demon_Worship))
			{
				if (faction2.isPlayerFaction || faction2.factionType.HasIdeology(FACTION_IDEOLOGY.Demon_Worship))
				{
					relationshipStatus = FACTION_RELATIONSHIP_STATUS.Friendly;
				}
				else if (faction2.factionType.type == FACTION_TYPE.Retaliator)
				{
					relationshipStatus = FACTION_RELATIONSHIP_STATUS.Hostile;
				}
			}
			else if (faction2.isPlayerFaction)
			{
				relationshipStatus = FACTION_RELATIONSHIP_STATUS.Hostile;
			}
			else if (faction2.factionType.type == FACTION_TYPE.Retaliator)
			{
				relationshipStatus = FACTION_RELATIONSHIP_STATUS.Friendly;
			}
			if (faction2.factionType.type == FACTION_TYPE.Vampire_Clan)
			{
				if (faction.factionType.type == FACTION_TYPE.Lycan_Clan)
				{
					relationshipStatus = FACTION_RELATIONSHIP_STATUS.Hostile;
				}
				else if (faction.factionType.type == FACTION_TYPE.Vampire_Clan)
				{
					relationshipStatus = FACTION_RELATIONSHIP_STATUS.Neutral;
				}
			}
			if (faction2.factionType.type == FACTION_TYPE.Lycan_Clan)
			{
				if (faction.factionType.type == FACTION_TYPE.Vampire_Clan)
				{
					relationshipStatus = FACTION_RELATIONSHIP_STATUS.Hostile;
				}
				else if (faction.factionType.type == FACTION_TYPE.Lycan_Clan)
				{
					relationshipStatus = FACTION_RELATIONSHIP_STATUS.Neutral;
				}
			}
			if (faction2.leader != null && faction2.leader is Character character2)
			{
				if (leader.relationshipContainer.IsEnemiesWith(character2) || character2.relationshipContainer.IsEnemiesWith(leader))
				{
					relationshipWith.SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.Hostile);
					if (logRelationshipChangeFromLeaderRelationship)
					{
						LogRelationshipChangeBasedOnLeadersRelationship(FACTION_RELATIONSHIP_STATUS.Hostile, faction, faction2);
					}
				}
				else if (leader.relationshipContainer.IsFriendsWith(character2) && character2.relationshipContainer.IsFriendsWith(leader))
				{
					relationshipWith.SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.Friendly);
					if (logRelationshipChangeFromLeaderRelationship)
					{
						LogRelationshipChangeBasedOnLeadersRelationship(FACTION_RELATIONSHIP_STATUS.Friendly, faction, faction2);
					}
				}
				else if (isRerollForNewFaction && leader.relationshipContainer.IsFriendsOrAcquaintancesWith(character2) && character2.relationshipContainer.IsFriendsOrAcquaintancesWith(leader))
				{
					relationshipWith.SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.Friendly);
					if (logRelationshipChangeFromLeaderRelationship)
					{
						LogRelationshipChangeBasedOnLeadersRelationship(FACTION_RELATIONSHIP_STATUS.Friendly, faction, faction2);
					}
				}
				else
				{
					relationshipWith.SetRelationshipStatus(relationshipStatus);
				}
			}
			else
			{
				relationshipWith.SetRelationshipStatus(relationshipStatus);
			}
		}
	}

	private void LogRelationshipChangeBasedOnLeadersRelationship(FACTION_RELATIONSHIP_STATUS status, Faction faction, Faction otherFaction)
	{
		if (!otherFaction.isPlayerFaction)
		{
			switch (status)
			{
			case FACTION_RELATIONSHIP_STATUS.Hostile:
			{
				Log log3 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Faction", "Faction_Table", "dislike_leader", LOG_TAG.Major);
				log3.AddToFillers(faction.leader as Character, faction.leader.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log3.AddToFillers(otherFaction.leader as Character, otherFaction.leader.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				Log log4 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Faction", "Faction_Table", "declare_war", LOG_TAG.Major);
				log4.AddToFillers(faction, faction.name, LOG_IDENTIFIER.FACTION_1);
				log4.AddToFillers(otherFaction, otherFaction.name, LOG_IDENTIFIER.FACTION_2);
				log4.AddToFillers(log3.fillers);
				log4.AddToFillers(null, log3.unreplacedText, LOG_IDENTIFIER.APPEND);
				log4.AddLogToDatabase();
				PlayerManager.Instance.player.ShowNotificationFromPlayer(log4, releaseLogAfter: true);
				LogPool.Release(log3);
				break;
			}
			case FACTION_RELATIONSHIP_STATUS.Friendly:
			{
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Faction", "Faction_Table", "like_leader", LOG_TAG.Major);
				log.AddToFillers(faction.leader as Character, faction.leader.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log.AddToFillers(otherFaction.leader as Character, otherFaction.leader.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Faction", "Faction_Table", "declare_peace", LOG_TAG.Major);
				log2.AddToFillers(faction, faction.name, LOG_IDENTIFIER.FACTION_1);
				log2.AddToFillers(otherFaction, otherFaction.name, LOG_IDENTIFIER.FACTION_2);
				log2.AddToFillers(log.fillers);
				log2.AddToFillers(null, log.unreplacedText, LOG_IDENTIFIER.APPEND);
				log2.AddLogToDatabase();
				PlayerManager.Instance.player.ShowNotificationFromPlayer(log2, releaseLogAfter: true);
				LogPool.Release(log);
				break;
			}
			}
		}
	}

	public T CreateIdeology<T>(FACTION_IDEOLOGY ideologyType) where T : FactionIdeology
	{
		string text = ideologyType.ToStringEnumNoSpace() + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
		Type type = Type.GetType(text);
		if (type != null)
		{
			T obj = Activator.CreateInstance(type) as T;
			obj.InitializeFactionIdeology();
			return obj;
		}
		throw new Exception(text + " has no data!");
	}

	public void RerollPeaceTypeIdeology(Faction faction, Character leader)
	{
		if (faction.factionType.type == FACTION_TYPE.Bandits)
		{
			Warmonger ideology = CreateIdeology<Warmonger>(FACTION_IDEOLOGY.Warmonger);
			faction.factionType.AddIdeology(ideology, faction);
			return;
		}
		if (leader.traitContainer.HasTrait("Diplomatic"))
		{
			Peaceful ideology2 = CreateIdeology<Peaceful>(FACTION_IDEOLOGY.Peaceful);
			faction.factionType.AddIdeology(ideology2, faction);
			return;
		}
		if (leader.traitContainer.HasTrait("Hothead", "Treacherous", "Evil", "Demon Cultist"))
		{
			Warmonger ideology3 = CreateIdeology<Warmonger>(FACTION_IDEOLOGY.Warmonger);
			faction.factionType.AddIdeology(ideology3, faction);
			return;
		}
		if (leader.traitContainer.HasTrait("Vampire"))
		{
			if (!leader.traitContainer.GetTraitOrStatus<Vampire>("Vampire").dislikedBeingVampire)
			{
				Warmonger ideology4 = CreateIdeology<Warmonger>(FACTION_IDEOLOGY.Warmonger);
				faction.factionType.AddIdeology(ideology4, faction);
				return;
			}
		}
		else if (leader.traitContainer.HasTrait("Lycanthrope") && leader.lycanData != null && !leader.lycanData.dislikesBeingLycan)
		{
			Warmonger ideology5 = CreateIdeology<Warmonger>(FACTION_IDEOLOGY.Warmonger);
			faction.factionType.AddIdeology(ideology5, faction);
			return;
		}
		Peaceful ideology6 = CreateIdeology<Peaceful>(FACTION_IDEOLOGY.Peaceful);
		faction.factionType.AddIdeology(ideology6, faction);
	}

	public void RerollInclusiveTypeIdeology(Faction faction, Character leader, bool shouldRerollChanceBased = true)
	{
		if (faction.factionType.type == FACTION_TYPE.Demon_Cult)
		{
			faction.factionType.RemoveIdeology(FACTION_IDEOLOGY.Exclusive, faction);
			Exclusive exclusive = CreateIdeology<Exclusive>(FACTION_IDEOLOGY.Exclusive);
			exclusive.SetRequirement(RELIGION.Demon_Worship);
			faction.factionType.AddIdeology(exclusive, faction);
		}
		else if (faction.factionType.type == FACTION_TYPE.Divine_Church)
		{
			faction.factionType.RemoveIdeology(FACTION_IDEOLOGY.Exclusive, faction);
			Exclusive exclusive2 = CreateIdeology<Exclusive>(FACTION_IDEOLOGY.Exclusive);
			exclusive2.SetRequirement(RELIGION.Divine_Worship);
			faction.factionType.AddIdeology(exclusive2, faction);
		}
		else if (faction.factionType.type == FACTION_TYPE.Wiccans)
		{
			faction.factionType.RemoveIdeology(FACTION_IDEOLOGY.Exclusive, faction);
			Exclusive exclusive3 = CreateIdeology<Exclusive>(FACTION_IDEOLOGY.Exclusive);
			exclusive3.SetRequirement(RELIGION.Nature_Worship);
			faction.factionType.AddIdeology(exclusive3, faction);
		}
		else if (faction.factionType.type == FACTION_TYPE.Undead)
		{
			if (!faction.factionType.HasIdeology(FACTION_IDEOLOGY.Inclusive))
			{
				Inclusive ideology = CreateIdeology<Inclusive>(FACTION_IDEOLOGY.Inclusive);
				faction.factionType.AddIdeology(ideology, faction);
			}
		}
		else if (faction.factionType.type == FACTION_TYPE.Bandits)
		{
			if (!faction.factionType.HasIdeology(FACTION_IDEOLOGY.Inclusive))
			{
				Inclusive ideology2 = CreateIdeology<Inclusive>(FACTION_IDEOLOGY.Inclusive);
				faction.factionType.AddIdeology(ideology2, faction);
			}
		}
		else
		{
			if (!shouldRerollChanceBased)
			{
				return;
			}
			if (GameUtilities.RollChance(70))
			{
				if (!faction.factionType.HasIdeology(FACTION_IDEOLOGY.Inclusive))
				{
					Inclusive ideology3 = CreateIdeology<Inclusive>(FACTION_IDEOLOGY.Inclusive);
					faction.factionType.AddIdeology(ideology3, faction);
				}
				return;
			}
			faction.factionType.RemoveIdeology(FACTION_IDEOLOGY.Exclusive, faction);
			if (faction.factionType.type == FACTION_TYPE.Vampire_Clan && GameUtilities.RollChance(35))
			{
				Exclusive exclusive4 = CreateIdeology<Exclusive>(FACTION_IDEOLOGY.Exclusive);
				exclusive4.SetRequirement("Vampire");
				faction.factionType.AddIdeology(exclusive4, faction);
				return;
			}
			if (faction.factionType.type == FACTION_TYPE.Lycan_Clan && GameUtilities.RollChance(35))
			{
				Exclusive exclusive5 = CreateIdeology<Exclusive>(FACTION_IDEOLOGY.Exclusive);
				exclusive5.SetRequirement("Lycanthrope");
				faction.factionType.AddIdeology(exclusive5, faction);
				return;
			}
			if (faction.factionType.type == FACTION_TYPE.Human_Empire && GameUtilities.RollChance(60))
			{
				Exclusive exclusive6 = CreateIdeology<Exclusive>(FACTION_IDEOLOGY.Exclusive);
				exclusive6.SetRequirement(RACE.HUMANS);
				faction.factionType.AddIdeology(exclusive6, faction);
				return;
			}
			if (faction.factionType.type == FACTION_TYPE.Elven_Kingdom && GameUtilities.RollChance(60))
			{
				Exclusive exclusive7 = CreateIdeology<Exclusive>(FACTION_IDEOLOGY.Exclusive);
				exclusive7.SetRequirement(RACE.ELVES);
				faction.factionType.AddIdeology(exclusive7, faction);
				return;
			}
			if (GameManager.Instance.gameHasStarted)
			{
				Exclusive exclusive8 = CreateIdeology<Exclusive>(FACTION_IDEOLOGY.Exclusive);
				exclusive8.SetRequirement(leader.gender);
				faction.factionType.AddIdeology(exclusive8, faction);
				return;
			}
			bool flag = false;
			for (int i = 0; i < faction.characters.Count; i++)
			{
				if (faction.characters[i].gender != leader.gender)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				if (!faction.factionType.HasIdeology(FACTION_IDEOLOGY.Inclusive))
				{
					Inclusive ideology4 = CreateIdeology<Inclusive>(FACTION_IDEOLOGY.Inclusive);
					faction.factionType.AddIdeology(ideology4, faction);
				}
			}
			else
			{
				Exclusive exclusive9 = CreateIdeology<Exclusive>(FACTION_IDEOLOGY.Exclusive);
				exclusive9.SetRequirement(leader.gender);
				faction.factionType.AddIdeology(exclusive9, faction);
			}
		}
	}

	public void RerollReligionTypeIdeology(Faction faction, Character leader)
	{
		RELIGION p_religion;
		if (faction.factionType.type == FACTION_TYPE.Demon_Cult)
		{
			DemonWorship ideology = CreateIdeology<DemonWorship>(FACTION_IDEOLOGY.Demon_Worship);
			faction.factionType.AddIdeology(ideology, faction);
		}
		else if (faction.factionType.type == FACTION_TYPE.Divine_Church)
		{
			DivineWorship ideology2 = CreateIdeology<DivineWorship>(FACTION_IDEOLOGY.Divine_Worship);
			faction.factionType.AddIdeology(ideology2, faction);
		}
		else if (faction.factionType.type == FACTION_TYPE.Wiccans)
		{
			NatureWorship ideology3 = CreateIdeology<NatureWorship>(FACTION_IDEOLOGY.Nature_Worship);
			faction.factionType.AddIdeology(ideology3, faction);
		}
		else if (leader.traitContainer.IsReligiousCultist(out p_religion))
		{
			FactionIdeology ideology4 = p_religion switch
			{
				RELIGION.Demon_Worship => CreateIdeology<DemonWorship>(FACTION_IDEOLOGY.Demon_Worship), 
				RELIGION.Nature_Worship => CreateIdeology<NatureWorship>(FACTION_IDEOLOGY.Nature_Worship), 
				RELIGION.Divine_Worship => CreateIdeology<DivineWorship>(FACTION_IDEOLOGY.Divine_Worship), 
				_ => throw new Exception($"No ideology case for religion {p_religion}"), 
			};
			faction.factionType.AddIdeology(ideology4, faction);
		}
		else if (leader.race == RACE.ELVES)
		{
			NatureWorship ideology5 = CreateIdeology<NatureWorship>(FACTION_IDEOLOGY.Nature_Worship);
			faction.factionType.AddIdeology(ideology5, faction);
		}
		else if (leader.race == RACE.HUMANS)
		{
			DivineWorship ideology6 = CreateIdeology<DivineWorship>(FACTION_IDEOLOGY.Divine_Worship);
			faction.factionType.AddIdeology(ideology6, faction);
		}
	}

	public void RerollFactionLeaderTraitIdeology(Faction faction, Character leader, Trait p_gainedOrLostTrait = null)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		bool flag6 = false;
		bool flag7 = true;
		bool flag8 = true;
		if (p_gainedOrLostTrait != null)
		{
			if (!(p_gainedOrLostTrait is Vampire))
			{
				flag7 = false;
			}
			if (!(p_gainedOrLostTrait is Lycanthrope))
			{
				flag8 = false;
			}
		}
		if (flag7)
		{
			if (leader.traitContainer.HasTrait("Vampire") && !leader.traitContainer.GetTraitOrStatus<Vampire>("Vampire").dislikedBeingVampire && GameUtilities.RollChance(50))
			{
				flag = true;
				flag6 = true;
			}
			if (leader.traitContainer.HasTrait("Hemophiliac"))
			{
				flag = true;
				flag6 = false;
			}
			else if (leader.traitContainer.HasTrait("Hemophobic"))
			{
				flag3 = true;
			}
			if (flag)
			{
				faction.factionType.AddIdeology(FACTION_IDEOLOGY.Reveres_Vampires, faction);
				if (flag6)
				{
					faction.factionType.RemoveIdeology(FACTION_IDEOLOGY.Reveres_Werewolves, faction);
				}
			}
			else if (flag3 && faction.factionType.type != FACTION_TYPE.Vampire_Clan)
			{
				faction.factionType.AddIdeology(FACTION_IDEOLOGY.Hates_Vampires, faction);
			}
		}
		if (flag8)
		{
			if (leader.isLycanthrope && !leader.lycanData.dislikesBeingLycan && GameUtilities.RollChance(50))
			{
				flag2 = true;
				flag5 = true;
			}
			if (leader.traitContainer.HasTrait("Lycanphiliac"))
			{
				flag2 = true;
				flag5 = false;
			}
			else if (leader.traitContainer.HasTrait("Lycanphobic"))
			{
				flag4 = true;
			}
			if (flag2)
			{
				faction.factionType.AddIdeology(FACTION_IDEOLOGY.Reveres_Werewolves, faction);
				if (flag5)
				{
					faction.factionType.RemoveIdeology(FACTION_IDEOLOGY.Reveres_Vampires, faction);
				}
			}
			else if (flag4 && faction.factionType.type != FACTION_TYPE.Lycan_Clan)
			{
				faction.factionType.AddIdeology(FACTION_IDEOLOGY.Hates_Werewolves, faction);
			}
		}
		if (leader.traitContainer.HasTrait("Evil", "Psychopath", "Treacherous"))
		{
			if (GameUtilities.RollChance(15))
			{
				faction.factionType.AddIdeology(FACTION_IDEOLOGY.Necromantic, faction);
			}
			else if (GameUtilities.RollChance(15))
			{
				faction.factionType.AddIdeology(FACTION_IDEOLOGY.Blood_Sacrifices, faction);
			}
		}
	}

	public void RerollSpecialIdeologies(Faction faction, Character leader, bool isSuccession = false)
	{
		bool flag = false;
		if (leader.characterClass.className == "Mage" && GameUtilities.RollChance(isSuccession ? 5 : 20))
		{
			faction.factionType.AddIdeology(FACTION_IDEOLOGY.Mage_Guild, faction);
			flag = true;
		}
		else
		{
			faction.factionType.RemoveIdeology(FACTION_IDEOLOGY.Mage_Guild, faction);
		}
		if (leader.characterClass.className == "Druid" && GameUtilities.RollChance(isSuccession ? 5 : 20))
		{
			faction.factionType.AddIdeology(FACTION_IDEOLOGY.Beastmasters, faction);
			flag = true;
		}
		else
		{
			faction.factionType.RemoveIdeology(FACTION_IDEOLOGY.Beastmasters, faction);
		}
		if (flag)
		{
			return;
		}
		if (leader.race == RACE.ELVES && GameUtilities.RollChance(isSuccession ? 3 : 10))
		{
			faction.factionType.AddIdeology(FACTION_IDEOLOGY.Entkin, faction);
		}
		else if (GameUtilities.RollChance(isSuccession ? 3 : 10))
		{
			if (GameUtilities.RollChance(50))
			{
				faction.factionType.AddIdeology(FACTION_IDEOLOGY.Golem_Makers, faction);
			}
			else
			{
				faction.factionType.AddIdeology(FACTION_IDEOLOGY.Infested, faction);
			}
		}
	}

	public void RevalidateFactionCrimes(Faction faction, Character leader)
	{
		if (leader.traitContainer.HasTrait("Demon Cultist"))
		{
			faction.factionType.RemoveCrime(CRIME_TYPE.Demon_Worship);
			faction.factionType.AddCrime(CRIME_TYPE.Divine_Worship, CRIME_SEVERITY.Heinous);
			faction.factionType.AddCrime(CRIME_TYPE.Nature_Worship, CRIME_SEVERITY.Heinous);
			faction.factionType.RemoveIdeology(FACTION_IDEOLOGY.Divine_Worship, faction);
			faction.factionType.RemoveIdeology(FACTION_IDEOLOGY.Nature_Worship, faction);
		}
		else if (leader.characterClass.className == "Priest" || leader.traitContainer.HasTrait("Cleric"))
		{
			faction.factionType.RemoveCrime(CRIME_TYPE.Divine_Worship);
			faction.factionType.AddCrime(CRIME_TYPE.Demon_Worship, CRIME_SEVERITY.Heinous);
			faction.factionType.AddCrime(CRIME_TYPE.Nature_Worship, CRIME_SEVERITY.Heinous);
		}
		else if (leader.characterClass.className == "Great Witch" || leader.traitContainer.HasTrait("Witch"))
		{
			faction.factionType.RemoveCrime(CRIME_TYPE.Nature_Worship);
			faction.factionType.AddCrime(CRIME_TYPE.Demon_Worship, CRIME_SEVERITY.Heinous);
			faction.factionType.AddCrime(CRIME_TYPE.Divine_Worship, CRIME_SEVERITY.Heinous);
		}
		else if (leader.religionComponent.religion == RELIGION.Divine_Worship)
		{
			faction.factionType.RemoveCrime(CRIME_TYPE.Divine_Worship);
		}
		else if (leader.religionComponent.religion == RELIGION.Nature_Worship)
		{
			faction.factionType.RemoveCrime(CRIME_TYPE.Nature_Worship);
		}
		else if (leader.religionComponent.religion == RELIGION.Demon_Worship)
		{
			faction.factionType.RemoveCrime(CRIME_TYPE.Demon_Worship);
		}
		if (faction.factionType.type != FACTION_TYPE.Vampire_Clan)
		{
			if (leader.traitContainer.HasTrait("Vampire"))
			{
				if (!leader.traitContainer.GetTraitOrStatus<Vampire>("Vampire").dislikedBeingVampire)
				{
					faction.factionType.RemoveCrime(CRIME_TYPE.Vampire);
				}
			}
			else if (leader.traitContainer.HasTrait("Hemophiliac"))
			{
				faction.factionType.RemoveCrime(CRIME_TYPE.Vampire);
			}
			else if (leader.traitContainer.HasTrait("Hemophobic"))
			{
				faction.factionType.AddCrime(CRIME_TYPE.Vampire, CRIME_SEVERITY.Heinous);
			}
			else if (!leader.traitContainer.HasTrait("Vampire") && !leader.traitContainer.HasTrait("Hemophiliac"))
			{
				if (!faction.factionType.GetCrimeSeverity(CRIME_TYPE.Vampire).IsConsideredACrime())
				{
					faction.factionType.AddCrime(CRIME_TYPE.Vampire, CRIME_SEVERITY.Heinous);
				}
			}
			else if (!leader.traitContainer.HasTrait("Hemophobic") && faction.factionType.GetCrimeSeverity(CRIME_TYPE.Vampire).IsConsideredACrime())
			{
				faction.factionType.RemoveCrime(CRIME_TYPE.Vampire);
			}
		}
		if (faction.factionType.type != FACTION_TYPE.Lycan_Clan)
		{
			if ((leader.lycanData != null && !leader.lycanData.dislikesBeingLycan) || leader.traitContainer.HasTrait("Lycanphiliac"))
			{
				faction.factionType.RemoveCrime(CRIME_TYPE.Werewolf);
			}
			else if (leader.traitContainer.HasTrait("Lycanphobic"))
			{
				faction.factionType.AddCrime(CRIME_TYPE.Werewolf, CRIME_SEVERITY.Heinous);
			}
			else if (!leader.traitContainer.HasTrait("Lycanthrope") && !leader.traitContainer.HasTrait("Lycanphiliac"))
			{
				if (!faction.factionType.GetCrimeSeverity(CRIME_TYPE.Werewolf).IsConsideredACrime())
				{
					faction.factionType.AddCrime(CRIME_TYPE.Werewolf, CRIME_SEVERITY.Heinous);
				}
			}
			else if (!leader.traitContainer.HasTrait("Lycanphobic") && faction.factionType.GetCrimeSeverity(CRIME_TYPE.Werewolf).IsConsideredACrime())
			{
				faction.factionType.RemoveCrime(CRIME_TYPE.Werewolf);
			}
		}
		if (leader.traitContainer.HasTrait("Kleptomaniac"))
		{
			faction.factionType.RemoveCrime(CRIME_TYPE.Theft);
		}
		else
		{
			faction.factionType.AddCrime(CRIME_TYPE.Theft, faction.factionType.GetDefaultSeverity(CRIME_TYPE.Theft));
		}
		if (leader.traitContainer.HasTrait("Evil", "Psychopath"))
		{
			CRIME_TYPE randomNonReligionSeriousCrime = faction.factionType.GetRandomNonReligionSeriousCrime();
			if (randomNonReligionSeriousCrime != CRIME_TYPE.None && faction.factionType.RemoveCrime(randomNonReligionSeriousCrime))
			{
				faction.ideologyComponent.SetLastRemovedNonReligionCrimeTypeForEvilOrPsychopath(randomNonReligionSeriousCrime);
			}
		}
		else if (!leader.traitContainer.HasTrait("Evil", "Psychopath") && faction.ideologyComponent.lastRemovedNonReligionCrimeTypeForEvilOrPsychopath != CRIME_TYPE.Unset)
		{
			faction.factionType.AddCrime(faction.ideologyComponent.lastRemovedNonReligionCrimeTypeForEvilOrPsychopath, CRIME_SEVERITY.Serious);
			faction.ideologyComponent.SetLastRemovedNonReligionCrimeTypeForEvilOrPsychopath(CRIME_TYPE.Unset);
		}
		if (leader.traitContainer.HasTrait("Cannibal"))
		{
			faction.factionType.RemoveCrime(CRIME_TYPE.Cannibalism);
		}
		else if (!faction.factionType.GetCrimeSeverity(CRIME_TYPE.Cannibalism).IsConsideredACrime())
		{
			faction.factionType.AddCrime(CRIME_TYPE.Cannibalism, faction.factionType.GetDefaultSeverity(CRIME_TYPE.Cannibalism));
		}
		if (leader.traitContainer.HasTrait("Pyromaniac"))
		{
			faction.factionType.RemoveCrime(CRIME_TYPE.Arson);
		}
		else if (leader.traitContainer.HasTrait("Pyrophobic"))
		{
			faction.factionType.AddCrime(CRIME_TYPE.Arson, CRIME_SEVERITY.Heinous);
		}
		else
		{
			faction.factionType.AddCrime(CRIME_TYPE.Arson, faction.factionType.GetDefaultSeverity(CRIME_TYPE.Arson));
		}
	}

	public void RevalidateFactionCrimesOnLeaderRemoved(Faction faction, Character leader)
	{
		if (leader.traitContainer.HasTrait("Demon Cultist"))
		{
			faction.factionType.RemoveIdeology(FACTION_IDEOLOGY.Demon_Worship, faction);
		}
		if (!(faction.factionType is CultFaction))
		{
			if (leader.religionComponent.religion == RELIGION.Divine_Worship)
			{
				faction.factionType.RemoveCrime(CRIME_TYPE.Divine_Worship);
				faction.factionType.AddIdeology(FACTION_IDEOLOGY.Divine_Worship, faction);
			}
			else if (leader.religionComponent.religion == RELIGION.Nature_Worship)
			{
				faction.factionType.RemoveCrime(CRIME_TYPE.Nature_Worship);
				faction.factionType.AddIdeology(FACTION_IDEOLOGY.Nature_Worship, faction);
			}
			else if (leader.religionComponent.religion == RELIGION.Demon_Worship)
			{
				faction.factionType.RemoveCrime(CRIME_TYPE.Demon_Worship);
				faction.factionType.AddIdeology(FACTION_IDEOLOGY.Demon_Worship, faction);
			}
		}
		if (faction.factionType.type != FACTION_TYPE.Vampire_Clan)
		{
			if (leader.traitContainer.HasTrait("Vampire"))
			{
				faction.factionType.AddCrime(CRIME_TYPE.Vampire, CRIME_SEVERITY.Heinous);
			}
			else if (leader.traitContainer.HasTrait("Hemophiliac"))
			{
				faction.factionType.AddCrime(CRIME_TYPE.Vampire, CRIME_SEVERITY.Heinous);
			}
			else if (leader.traitContainer.HasTrait("Hemophobic"))
			{
				faction.factionType.RemoveCrime(CRIME_TYPE.Vampire);
			}
		}
		if (faction.factionType.type != FACTION_TYPE.Lycan_Clan)
		{
			if (leader.traitContainer.HasTrait("Lycanthrope") || leader.traitContainer.HasTrait("Lycanphiliac"))
			{
				faction.factionType.AddCrime(CRIME_TYPE.Werewolf, CRIME_SEVERITY.Heinous);
			}
			else if (leader.traitContainer.HasTrait("Lycanphobic"))
			{
				faction.factionType.RemoveCrime(CRIME_TYPE.Werewolf);
			}
		}
		if (leader.traitContainer.HasTrait("Kleptomaniac"))
		{
			faction.factionType.AddCrime(CRIME_TYPE.Theft, faction.factionType.GetDefaultSeverity(CRIME_TYPE.Theft));
		}
		if (leader.traitContainer.HasTrait("Evil", "Psychopath"))
		{
			faction.factionType.AddCrime(faction.ideologyComponent.lastRemovedNonReligionCrimeTypeForEvilOrPsychopath, CRIME_SEVERITY.Serious);
			faction.ideologyComponent.SetLastRemovedNonReligionCrimeTypeForEvilOrPsychopath(CRIME_TYPE.Unset);
		}
		if (leader.traitContainer.HasTrait("Cannibal"))
		{
			faction.factionType.AddCrime(CRIME_TYPE.Cannibalism, faction.factionType.GetDefaultSeverity(CRIME_TYPE.Cannibalism));
		}
		if (leader.traitContainer.HasTrait("Pyromaniac"))
		{
			faction.factionType.AddCrime(CRIME_TYPE.Arson, CRIME_SEVERITY.Misdemeanor);
		}
		if (leader.traitContainer.HasTrait("Pyrophobic"))
		{
			faction.factionType.AddCrime(CRIME_TYPE.Arson, faction.factionType.GetDefaultSeverity(CRIME_TYPE.Arson));
		}
		Messenger.Broadcast(FactionSignals.FACTION_CRIMES_CHANGED, faction);
	}

	public FactionType CreateFactionType(FACTION_TYPE factionType)
	{
		string text = "Factions.Faction_Types." + factionType.ToStringEnumNoSpace() + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
		Type type = Type.GetType(text);
		if (type != null)
		{
			return Activator.CreateInstance(type) as FactionType;
		}
		throw new Exception(text + " has no data!");
	}

	public FactionType CreateFactionType(FACTION_TYPE factionType, SaveDataFactionType saveData)
	{
		string text = "Factions.Faction_Types." + factionType.ToStringEnumNoSpace() + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
		Type type = Type.GetType(text);
		if (type != null)
		{
			return Activator.CreateInstance(type, saveData) as FactionType;
		}
		throw new Exception(text + " has no data!");
	}

	public FACTION_TYPE GetFactionTypeForCharacter(Character character)
	{
		if (character.traitContainer.HasTrait("Vampire") && PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.VAMPIRISM).HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Create_Vampire_Clan_Faction))
		{
			return FACTION_TYPE.Vampire_Clan;
		}
		if (character.isLycanthrope && character.lycanData.isMaster && PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.LYCANTHROPY).HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Form_Lycan_Clan_Faction))
		{
			return FACTION_TYPE.Lycan_Clan;
		}
		return GetFactionTypeForRace(character.race);
	}

	public FACTION_TYPE GetFactionTypeForRace(RACE race)
	{
		return race switch
		{
			RACE.HUMANS => FACTION_TYPE.Human_Empire, 
			RACE.ELVES => FACTION_TYPE.Elven_Kingdom, 
			_ => FACTION_TYPE.Human_Empire, 
		};
	}

	private void ConstructFactionSuccessionTypes()
	{
		FACTION_SUCCESSION_TYPE[] enumValues = CollectionUtilities.GetEnumValues<FACTION_SUCCESSION_TYPE>();
		foreach (FACTION_SUCCESSION_TYPE fACTION_SUCCESSION_TYPE in enumValues)
		{
			if (fACTION_SUCCESSION_TYPE == FACTION_SUCCESSION_TYPE.None)
			{
				_factionSuccessions.Add(fACTION_SUCCESSION_TYPE, new FactionSuccession(fACTION_SUCCESSION_TYPE));
			}
			else
			{
				_factionSuccessions.Add(fACTION_SUCCESSION_TYPE, CreateFactionSuccession(fACTION_SUCCESSION_TYPE));
			}
		}
	}

	public FactionSuccession CreateFactionSuccession(FACTION_SUCCESSION_TYPE successionType)
	{
		string text = "Factions.Faction_Succession." + successionType.ToStringEnumNoSpace() + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
		Type type = Type.GetType(text);
		if (type != null)
		{
			return Activator.CreateInstance(type) as FactionSuccession;
		}
		throw new Exception(text + " has no data!");
	}

	public FactionSuccession GetFactionSuccession(FACTION_SUCCESSION_TYPE type)
	{
		if (_factionSuccessions.ContainsKey(type))
		{
			return _factionSuccessions[type];
		}
		return null;
	}

	public void JoinOrCreateBanditFaction(Character p_character, bool useInterrupt = true)
	{
		if (banditFaction == null)
		{
			p_character.interruptComponent.TriggerInterrupt(INTERRUPT.Create_Faction, p_character, "create_bandits");
			return;
		}
		if (useInterrupt)
		{
			p_character.interruptComponent.TriggerInterrupt(INTERRUPT.Join_Faction, p_character, "join_bandits");
		}
		else
		{
			p_character.ChangeFactionTo(banditFaction);
		}
		Faction faction = p_character.faction;
		if (faction != null && faction.factionType.type == FACTION_TYPE.Bandits)
		{
			p_character.MigrateHomeStructureTo(null);
		}
		if (banditFaction.leader == null && p_character.faction == banditFaction)
		{
			p_character.interruptComponent.TriggerInterrupt(INTERRUPT.Become_Faction_Leader, p_character);
		}
	}

	public Faction GetReligiousCultFactionForReligion(RELIGION p_religion)
	{
		return p_religion switch
		{
			RELIGION.Demon_Worship => demonCultFaction, 
			RELIGION.Divine_Worship => divineCultFaction, 
			RELIGION.Nature_Worship => natureCultFaction, 
			_ => throw new ArgumentOutOfRangeException("p_religion", p_religion, null), 
		};
	}

	public bool FactionLeaderCultistProcessing(Character factionLeader, RELIGION religion, out bool wasFactionMergedWithReligiousCult, out bool createdReligiousCultFaction)
	{
		Faction faction = factionLeader.faction;
		wasFactionMergedWithReligiousCult = false;
		createdReligiousCultFaction = false;
		FACTION_TYPE factionTypeForReligion = religion.GetFactionTypeForReligion();
		if (faction.factionType.type == factionTypeForReligion)
		{
			return false;
		}
		Faction faction2 = GetReligiousCultFactionForReligion(religion);
		if (faction2 == null || faction2.CanCharacterJoinFactionBasedOnNonReligionIdeologiesAndBanning(factionLeader))
		{
			if (faction2 == null)
			{
				createdReligiousCultFaction = true;
				faction2 = CreateReligiousCultFactionForReligion(religion);
			}
			if (faction.factionType.HasIdeology(FACTION_IDEOLOGY.Wyvern_Tamers))
			{
				faction2.factionType.AddIdeology(FACTION_IDEOLOGY.Wyvern_Tamers, faction);
			}
			if (faction.factionType.HasIdeology(FACTION_IDEOLOGY.Tower_Defense))
			{
				faction2.factionType.AddIdeology(FACTION_IDEOLOGY.Tower_Defense, faction);
			}
			if (faction.factionType.HasIdeology(FACTION_IDEOLOGY.Lightning_Tower_Defense))
			{
				faction2.factionType.AddIdeology(FACTION_IDEOLOGY.Lightning_Tower_Defense, faction);
			}
			if (faction.factionType.HasIdeology(FACTION_IDEOLOGY.Breeders))
			{
				faction2.factionType.AddIdeology(FACTION_IDEOLOGY.Breeders, faction);
			}
			List<BaseSettlement> list = RuinarchListPool<BaseSettlement>.Claim();
			list.AddRange(faction.ownedSettlements);
			for (int i = 0; i < list.Count; i++)
			{
				BaseSettlement baseSettlement = list[i];
				if (baseSettlement is NPCSettlement nPCSettlement && (nPCSettlement.settlementType == null || nPCSettlement.settlementType.settlementType != SETTLEMENT_TYPE.Cult_Town))
				{
					nPCSettlement.SetSettlementType(SETTLEMENT_TYPE.Cult_Town);
				}
				LandmarkManager.Instance.OwnSettlement(faction2, baseSettlement);
			}
			RuinarchListPool<BaseSettlement>.Release(list);
			factionLeader.ChangeFactionTo(faction2);
			wasFactionMergedWithReligiousCult = true;
			if (createdReligiousCultFaction || !faction2.HasAliveMemberExcept(factionLeader))
			{
				faction2.SetLeader(factionLeader);
			}
			List<Character> list2 = RuinarchListPool<Character>.Claim();
			list2.AddRange(faction.characters);
			for (int j = 0; j < list2.Count; j++)
			{
				Character character = list2[j];
				if (character != factionLeader && !character.isDead && character.petComponent.petOwner == null)
				{
					character.interruptComponent.TriggerInterrupt(INTERRUPT.Evaluate_Cultist_Affiliation, factionLeader, religion.ToStringEnum());
				}
			}
			RuinarchListPool<Character>.Release(list2);
			if (createdReligiousCultFaction)
			{
				Messenger.Broadcast(FactionSignals.CREATE_FACTION_INTERRUPT, faction2, factionLeader);
			}
			return true;
		}
		return false;
	}

	public void OnLocaleChanged(Locale locale)
	{
		if (wildMonsterFaction != null)
		{
			wildMonsterFaction.SetName(LocalizationManager.Instance.GetLocalizedValue("Faction_Table", "Wild_Monsters"));
		}
		if (vagrantFaction != null)
		{
			vagrantFaction.SetName(LocalizationManager.Instance.GetLocalizedValue("Faction_Table", "Vagrants"));
		}
		if (disguisedFaction != null)
		{
			disguisedFaction.SetName(LocalizationManager.Instance.GetLocalizedValue("Faction_Table", "Disguised"));
		}
		if (ratmenFaction != null)
		{
			ratmenFaction.SetName(LocalizationManager.Instance.GetLocalizedValue("Faction_Table", "Ratmen"));
		}
		if (banditFaction != null)
		{
			banditFaction.SetName(LocalizationManager.Instance.GetLocalizedValue("Faction_Table", "Bandits"));
		}
		if (retaliatorFaction != null)
		{
			retaliatorFaction.SetName(LocalizationManager.Instance.GetLocalizedValue("Faction_Table", "Retaliator"));
		}
		if (demonCultFaction != null)
		{
			demonCultFaction.SetName(LocalizationManager.Instance.GetLocalizedValue("Faction_Table", "Demon_Cult"));
		}
		if (divineCultFaction != null)
		{
			divineCultFaction.SetName(LocalizationManager.Instance.GetLocalizedValue("Faction_Table", "Divine_Church"));
		}
		if (natureCultFaction != null)
		{
			natureCultFaction.SetName(LocalizationManager.Instance.GetLocalizedValue("Faction_Table", "Wiccans"));
		}
		if (_undeadFaction != null)
		{
			_undeadFaction.SetName(LocalizationManager.Instance.GetLocalizedValue("Faction_Table", "Undead"));
		}
	}
}

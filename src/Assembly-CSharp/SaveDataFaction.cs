using System;
using System.Collections.Generic;
using BayatGames.SaveGameFree.Types;
using Factions.Faction_Types;
using UtilityScripts;

[Serializable]
public class SaveDataFaction : SaveData<Faction>, ISavableCounterpart
{
	public int id;

	public string name;

	public string description;

	public bool isMajorFaction;

	public string emblemName;

	public bool isLeaderPlayer;

	public string leaderID;

	public bool isActive;

	public bool isAwareOfPlayer;

	public ColorSave factionColor;

	public RACE race;

	public List<string> characterIDs;

	public List<string> bannedCharacterIDs;

	public List<string> ownedSettlementIDs;

	public Dictionary<string, SaveDataFactionRelationship> relationships;

	public SaveDataFactionType factionType;

	public int newLeaderDesignationChance;

	public SaveDataPartyQuestBoard partyQuestBoard;

	public uint pathfindingTag;

	public uint pathfindingDoorTag;

	public SaveDataFactionIdeologyComponent ideologyComponent;

	public SaveDataFactionSuccessionComponent successionComponent;

	public SaveDataFactionCrimeComponent crimeComponent;

	public SaveDataFactionOpinionComponent opinionComponent;

	public SaveDataFactionCharactersComponent charactersComponent;

	public bool isInfoUnlocked;

	public bool isDisbanded;

	public string persistentID { get; set; }

	public OBJECT_TYPE objectType => OBJECT_TYPE.Faction;

	public override void Save(Faction data)
	{
		persistentID = data.persistentID;
		id = data.id;
		name = data.name;
		description = data.description;
		isMajorFaction = data.isMajorFaction;
		emblemName = data.emblemName;
		factionColor = data.factionColor;
		isActive = data.isActive;
		race = data.race;
		isAwareOfPlayer = data.isAwareOfPlayer;
		if (data.leader == null)
		{
			leaderID = string.Empty;
		}
		else
		{
			isLeaderPlayer = data.leader.objectType == OBJECT_TYPE.Player;
			leaderID = data.leader.persistentID;
		}
		characterIDs = RuinarchListPool<string>.Claim();
		if (data.characters != null)
		{
			for (int i = 0; i < data.characters.Count; i++)
			{
				characterIDs.Add(data.characters[i].persistentID);
			}
		}
		bannedCharacterIDs = RuinarchListPool<string>.Claim();
		if (data.bannedCharacters != null)
		{
			for (int j = 0; j < data.bannedCharacters.Count; j++)
			{
				bannedCharacterIDs.Add(data.bannedCharacters[j].persistentID);
			}
		}
		ownedSettlementIDs = RuinarchListPool<string>.Claim();
		if (data.ownedSettlements != null)
		{
			for (int k = 0; k < data.ownedSettlements.Count; k++)
			{
				ownedSettlementIDs.Add(data.ownedSettlements[k].persistentID);
			}
		}
		relationships = new Dictionary<string, SaveDataFactionRelationship>();
		foreach (KeyValuePair<Faction, FactionRelationship> relationship in data.relationships)
		{
			SaveDataFactionRelationship saveDataFactionRelationship = new SaveDataFactionRelationship();
			saveDataFactionRelationship.Save(relationship.Value);
			relationships.Add(relationship.Key.persistentID, saveDataFactionRelationship);
		}
		factionType = CreateNewSaveDataForFactionType(data.factionType);
		factionType.Save(data.factionType);
		partyQuestBoard = new SaveDataPartyQuestBoard();
		partyQuestBoard.Save(data.partyQuestBoard);
		ideologyComponent = new SaveDataFactionIdeologyComponent();
		ideologyComponent.Save(data.ideologyComponent);
		successionComponent = new SaveDataFactionSuccessionComponent();
		successionComponent.Save(data.successionComponent);
		crimeComponent = new SaveDataFactionCrimeComponent();
		crimeComponent.Save(data.crimeComponent);
		opinionComponent = new SaveDataFactionOpinionComponent();
		opinionComponent.Save(data.opinionComponent);
		charactersComponent = new SaveDataFactionCharactersComponent();
		charactersComponent.Save(data.charactersComponent);
		newLeaderDesignationChance = data.newLeaderDesignationChance;
		pathfindingTag = data.pathfindingTag;
		isInfoUnlocked = data.isInfoUnlocked;
		pathfindingDoorTag = data.pathfindingDoorTag;
		isDisbanded = data.isDisbanded;
	}

	public override Faction Load()
	{
		return FactionManager.Instance.CreateNewFaction(this);
	}

	private static SaveDataFactionType CreateNewSaveDataForFactionType(FactionType factionType)
	{
		return Activator.CreateInstance(factionType.serializedData) as SaveDataFactionType;
	}

	public override void CleanUp()
	{
		if (characterIDs != null)
		{
			RuinarchListPool<string>.Release(characterIDs);
			characterIDs = null;
		}
		if (bannedCharacterIDs != null)
		{
			RuinarchListPool<string>.Release(bannedCharacterIDs);
			bannedCharacterIDs = null;
		}
		if (ownedSettlementIDs != null)
		{
			RuinarchListPool<string>.Release(ownedSettlementIDs);
			ownedSettlementIDs = null;
		}
	}
}

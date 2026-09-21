using System;
using System.Collections.Generic;
using Interrupts;
using Quests;
using Quests.Alerts;
using Traits;
using Tutorial;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UtilityScripts;

public class SaveDataCurrentProgress
{
	public string fileName;

	public DateTime timeStamp;

	public string gameVersion;

	public string language;

	public int month;

	public int day;

	public int year;

	public int tick;

	public int continuousDays;

	public bool[][] portraitAvailability;

	public WorldMapSave worldMapSave;

	public WorldSettingsData worldSettingsData;

	public FamilyTreeDatabase familyTreeDatabase;

	public SaveDataPlayerGame playerSave;

	public SaveDataVictoryCondition victoryCondition;

	public bool hasPlagueDisease;

	public SaveDataPlagueDisease savedPlagueDisease;

	public Dictionary<OBJECT_TYPE, BaseSaveDataHub> objectHub;

	public void Initialize()
	{
		timeStamp = DateTime.Now;
		gameVersion = Application.version;
		language = LocalizationSettings.SelectedLocale.LocaleName;
		if (objectHub == null)
		{
			ConstructObjectHub();
		}
	}

	private void ConstructObjectHub()
	{
		objectHub = new Dictionary<OBJECT_TYPE, BaseSaveDataHub>
		{
			{
				OBJECT_TYPE.Faction,
				new SaveDataFactionHub()
			},
			{
				OBJECT_TYPE.Tile_Object,
				new SaveDataTileObjectHub()
			},
			{
				OBJECT_TYPE.Action,
				new SaveDataActionHub()
			},
			{
				OBJECT_TYPE.Interrupt,
				new SaveDataInterruptHub()
			},
			{
				OBJECT_TYPE.Party,
				new SaveDataPartyHub()
			},
			{
				OBJECT_TYPE.Party_Quest,
				new SaveDataPartyQuestHub()
			},
			{
				OBJECT_TYPE.Crime,
				new SaveDataCrimeHub()
			},
			{
				OBJECT_TYPE.Character,
				new SaveDataCharacterHub()
			},
			{
				OBJECT_TYPE.Trait,
				new SaveDataTraitHub()
			},
			{
				OBJECT_TYPE.Job,
				new SaveDataJobHub()
			},
			{
				OBJECT_TYPE.Gathering,
				new SaveDataGatheringHub()
			},
			{
				OBJECT_TYPE.Game_Alert,
				new SaveDataGameAlertHub()
			},
			{
				OBJECT_TYPE.Shared_Opinion_Modifier,
				new SaveDataSharedOpinionModifierHub()
			}
		};
	}

	public bool AddToSaveHub<T>(T data) where T : ISavable
	{
		if (objectHub.ContainsKey(data.objectType))
		{
			if (objectHub[data.objectType].GetData(data.persistentID) == null)
			{
				SaveData<T> saveData = (SaveData<T>)Activator.CreateInstance(data.serializedData);
				saveData.Save(data);
				return AddToSaveHub(saveData, data.objectType);
			}
			return false;
		}
		throw new NullReferenceException("Trying to add object type " + data.objectType.ToString() + " in Object Hub but there is no entry for it. Make sure you add it in ConstructObjectHub");
	}

	private bool AddToSaveHub<T>(T data, OBJECT_TYPE objectType)
	{
		if (objectHub.ContainsKey(objectType))
		{
			return objectHub[objectType].AddToSave(data);
		}
		throw new NullReferenceException("Trying to add object type " + objectType.ToString() + " in Object Hub but there is no entry for it. Make sure you add it in ConstructObjectHub");
	}

	private bool RemoveFromSaveHub<T>(T data, OBJECT_TYPE objectType)
	{
		if (objectHub.ContainsKey(objectType))
		{
			return objectHub[objectType].RemoveFromSave(data);
		}
		throw new NullReferenceException("Trying to remove object type " + objectType.ToString() + " in Object Hub but there is no entry for it. Make sure you add it in ConstructObjectHub");
	}

	public T GetFromSaveHub<T>(OBJECT_TYPE objectType, string persistenID)
	{
		if (objectHub.ContainsKey(objectType))
		{
			return (T)objectHub[objectType].GetData(persistenID);
		}
		throw new NullReferenceException("Trying to get object type " + objectType.ToString() + " in Object Hub but there is no entry for it. Make sure you add it in ConstructObjectHub");
	}

	public void SaveDate()
	{
		GameDate gameDate = GameManager.Instance.Today();
		month = gameDate.month;
		day = gameDate.day;
		year = gameDate.year;
		tick = gameDate.tick;
		continuousDays = GameManager.Instance.continuousDays;
	}

	public void SaveWorldSettings()
	{
		worldSettingsData = WorldSettings.Instance.worldSettingsData;
	}

	public void SavePlayer()
	{
		playerSave = new SaveDataPlayerGame();
		playerSave.Save();
	}

	public void SaveFactions()
	{
		for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++)
		{
			Faction data = FactionManager.Instance.allFactions[i];
			SaveDataFaction saveDataFaction = new SaveDataFaction();
			saveDataFaction.Save(data);
			AddToSaveHub(saveDataFaction, saveDataFaction.objectType);
		}
	}

	public void SaveCharacters()
	{
		for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++)
		{
			Character character = CharacterManager.Instance.allCharacters[i];
			SaveDataCharacter saveDataCharacter = CharacterManager.Instance.CreateNewSaveDataCharacter(character);
			AddToSaveHub(saveDataCharacter, saveDataCharacter.objectType);
		}
		for (int j = 0; j < CharacterManager.Instance.limboCharacters.Count; j++)
		{
			Character character2 = CharacterManager.Instance.limboCharacters[j];
			SaveDataCharacter saveDataCharacter2 = CharacterManager.Instance.CreateNewSaveDataCharacter(character2);
			AddToSaveHub(saveDataCharacter2, saveDataCharacter2.objectType);
		}
	}

	public void SaveJobs()
	{
		for (int i = 0; i < DatabaseManager.Instance.jobDatabase.allJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = DatabaseManager.Instance.jobDatabase.allJobs[i];
			if (jobQueueItem.jobType != JOB_TYPE.NONE)
			{
				AddToSaveHub(jobQueueItem);
			}
		}
	}

	public void SavePlagueDisease()
	{
		hasPlagueDisease = PlagueDisease.HasInstance();
		if (hasPlagueDisease)
		{
			savedPlagueDisease = new SaveDataPlagueDisease();
			savedPlagueDisease.Save();
		}
	}

	public void SaveGameAlerts()
	{
		for (int i = 0; i < TutorialManager.Instance.spawnedAlerts.Count; i++)
		{
			GameAlert data = TutorialManager.Instance.spawnedAlerts[i];
			AddToSaveHub(data);
		}
	}

	public void SaveSharedOpinionModifiers()
	{
		foreach (KeyValuePair<string, SharedOpinionModifier> allSharedOpinionModifier in DatabaseManager.Instance.sharedOpinionDatabase.allSharedOpinionModifiers)
		{
			if (allSharedOpinionModifier.Value.targetCharacter != null)
			{
				AddToSaveHub(allSharedOpinionModifier.Value);
			}
		}
	}

	public void SavePortraitAvailability()
	{
		List<CharacterPortraitSpriteCollection> list = RuinarchListPool<CharacterPortraitSpriteCollection>.Claim();
		CharacterManager.Instance.portraitCollection.PopulateAllSpriteCollection(list);
		portraitAvailability = new bool[list.Count][];
		for (int i = 0; i < list.Count; i++)
		{
			bool[] array = list[i].portraitAvailability;
			if (array != null)
			{
				portraitAvailability[i] = new bool[array.Length];
				for (int j = 0; j < array.Length; j++)
				{
					portraitAvailability[i][j] = array[j];
				}
			}
			else
			{
				portraitAvailability[i] = null;
			}
		}
		RuinarchListPool<CharacterPortraitSpriteCollection>.Release(list);
	}

	public void SaveVictoryCondition()
	{
		VictoryCondition victoryCondition = QuestManager.Instance.victoryCondition;
		this.victoryCondition = Activator.CreateInstance(victoryCondition.serializedData) as SaveDataVictoryCondition;
		this.victoryCondition.Save(victoryCondition);
	}

	public void SaveTileObjects(List<TileObject> allTileObjects)
	{
		for (int i = 0; i < allTileObjects.Count; i++)
		{
			TileObject tileObject = allTileObjects[i];
			SaveTileObject(tileObject);
		}
	}

	public void SaveGenericTileObjects(List<TileObject> allTileObjects)
	{
		for (int i = 0; i < allTileObjects.Count; i++)
		{
			GenericTileObject tileObject = allTileObjects[i] as GenericTileObject;
			SaveGenericTileObject(tileObject);
		}
	}

	public void SaveDestroyedTileObjects()
	{
		List<WeakReference> list = RuinarchListPool<WeakReference>.Claim();
		list.AddRange(DatabaseManager.Instance.tileObjectDatabase.destroyedTileObjects);
		for (int i = 0; i < list.Count; i++)
		{
			WeakReference weakReference = list[i];
			if (weakReference.IsAlive && weakReference.Target is TileObject tileObject)
			{
				SaveDestroyedTileObject(tileObject);
			}
		}
		RuinarchListPool<WeakReference>.Release(list);
	}

	private void SaveTileObject(TileObject tileObject)
	{
		lock (SaveCurrentProgressManager.THREAD_LOCKER)
		{
			SaveDataTileObject saveDataTileObject = CreateNewSaveDataForTileObject(tileObject);
			saveDataTileObject.Save(tileObject);
			AddToSaveHub(saveDataTileObject, saveDataTileObject.objectType);
		}
	}

	private bool SaveGenericTileObject(GenericTileObject tileObject)
	{
		if (tileObject.gridTileLocation.isDefault)
		{
			return false;
		}
		lock (SaveCurrentProgressManager.THREAD_LOCKER)
		{
			SaveDataTileObject saveDataTileObject = CreateNewSaveDataForTileObject(tileObject);
			saveDataTileObject.Save(tileObject);
			AddToSaveHub(saveDataTileObject, saveDataTileObject.objectType);
		}
		return true;
	}

	private bool SaveDestroyedTileObject(TileObject tileObject)
	{
		if (tileObject is GenericTileObject genericTileObject && genericTileObject.gridTileLocation.isDefault)
		{
			return false;
		}
		lock (SaveCurrentProgressManager.THREAD_LOCKER)
		{
			SaveDataTileObject saveDataTileObject = CreateNewSaveDataForTileObject(tileObject);
			saveDataTileObject.Save(tileObject);
			AddToSaveHub(saveDataTileObject, saveDataTileObject.objectType);
		}
		return true;
	}

	private static SaveDataTileObject CreateNewSaveDataForTileObject(TileObject tileObject)
	{
		return Activator.CreateInstance(tileObject.serializedData) as SaveDataTileObject;
	}

	public void LoadDate()
	{
		GameDate today = GameManager.Instance.Today();
		today.day = day;
		today.month = month;
		today.year = year;
		today.tick = tick;
		GameManager.Instance.continuousDays = continuousDays;
		GameManager.Instance.SetToday(today);
	}

	public void LoadFactions()
	{
		if (!objectHub.ContainsKey(OBJECT_TYPE.Faction) || !(objectHub[OBJECT_TYPE.Faction] is SaveDataFactionHub saveDataFactionHub))
		{
			return;
		}
		foreach (SaveDataFaction value in saveDataFactionHub.hub.Values)
		{
			value.Load();
		}
	}

	public void LoadTileObjects()
	{
		if (!objectHub.ContainsKey(OBJECT_TYPE.Tile_Object) || !(objectHub[OBJECT_TYPE.Tile_Object] is SaveDataTileObjectHub saveDataTileObjectHub))
		{
			return;
		}
		foreach (SaveDataTileObject value in saveDataTileObjectHub.hub.Values)
		{
			if (value.tileObjectType != TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT)
			{
				value.Load();
			}
		}
	}

	public void LoadTraits()
	{
		if (!objectHub.ContainsKey(OBJECT_TYPE.Trait) || !(objectHub[OBJECT_TYPE.Trait] is SaveDataTraitHub saveDataTraitHub))
		{
			return;
		}
		foreach (SaveDataTrait value in saveDataTraitHub.hub.Values)
		{
			value.Load();
		}
	}

	public void LoadJobs()
	{
		if (!objectHub.ContainsKey(OBJECT_TYPE.Job) || !(objectHub[OBJECT_TYPE.Job] is SaveDataJobHub saveDataJobHub))
		{
			return;
		}
		foreach (SaveDataJobQueueItem value in saveDataJobHub.hub.Values)
		{
			value.Load();
		}
	}

	public void LoadActions()
	{
		if (!objectHub.ContainsKey(OBJECT_TYPE.Action) || !(objectHub[OBJECT_TYPE.Action] is SaveDataActionHub saveDataActionHub))
		{
			return;
		}
		foreach (SaveDataActualGoapNode value in saveDataActionHub.hub.Values)
		{
			ActualGoapNode action = value.Load();
			DatabaseManager.Instance.actionDatabase.AddAction(action);
		}
	}

	public void LoadInterrupts()
	{
		if (!objectHub.ContainsKey(OBJECT_TYPE.Interrupt) || !(objectHub[OBJECT_TYPE.Interrupt] is SaveDataInterruptHub saveDataInterruptHub))
		{
			return;
		}
		foreach (SaveDataInterruptHolder value in saveDataInterruptHub.hub.Values)
		{
			InterruptHolder interrupt = value.Load();
			DatabaseManager.Instance.interruptDatabase.AddInterrupt(interrupt);
		}
	}

	public void LoadParties()
	{
		if (!objectHub.ContainsKey(OBJECT_TYPE.Party) || !(objectHub[OBJECT_TYPE.Party] is SaveDataPartyHub saveDataPartyHub))
		{
			return;
		}
		foreach (SaveDataParty value in saveDataPartyHub.hub.Values)
		{
			Party party = value.Load();
			DatabaseManager.Instance.partyDatabase.AddParty(party);
		}
	}

	public void LoadPartyQuests()
	{
		if (!objectHub.ContainsKey(OBJECT_TYPE.Party_Quest) || !(objectHub[OBJECT_TYPE.Party_Quest] is SaveDataPartyQuestHub saveDataPartyQuestHub))
		{
			return;
		}
		foreach (SaveDataPartyQuest value in saveDataPartyQuestHub.hub.Values)
		{
			PartyQuest party = value.Load();
			DatabaseManager.Instance.partyQuestDatabase.AddPartyQuest(party);
		}
	}

	public void LoadCrimes()
	{
		if (!objectHub.ContainsKey(OBJECT_TYPE.Crime) || !(objectHub[OBJECT_TYPE.Crime] is SaveDataCrimeHub saveDataCrimeHub))
		{
			return;
		}
		foreach (SaveDataCrimeData value in saveDataCrimeHub.hub.Values)
		{
			CrimeData crime = value.Load();
			DatabaseManager.Instance.crimeDatabase.AddCrime(crime);
		}
	}

	public void LoadCharacters()
	{
		if (!objectHub.ContainsKey(OBJECT_TYPE.Character) || !(objectHub[OBJECT_TYPE.Character] is SaveDataCharacterHub saveDataCharacterHub))
		{
			return;
		}
		foreach (SaveDataCharacter value in saveDataCharacterHub.hub.Values)
		{
			value.Load();
		}
	}

	public void LoadGatherings()
	{
		if (!objectHub.ContainsKey(OBJECT_TYPE.Gathering) || !(objectHub[OBJECT_TYPE.Gathering] is SaveDataGatheringHub saveDataGatheringHub))
		{
			return;
		}
		foreach (SaveDataGathering value in saveDataGatheringHub.hub.Values)
		{
			Gathering gathering = value.Load();
			DatabaseManager.Instance.gatheringDatabase.AddGathering(gathering);
		}
	}

	public Player LoadPlayer()
	{
		return playerSave.Load();
	}

	public void LoadPlagueDisease()
	{
		if (hasPlagueDisease)
		{
			new PlagueDisease(savedPlagueDisease);
		}
	}

	public void LoadSharedOpinionModifiers()
	{
		if (!objectHub.ContainsKey(OBJECT_TYPE.Shared_Opinion_Modifier) || !(objectHub[OBJECT_TYPE.Shared_Opinion_Modifier] is SaveDataSharedOpinionModifierHub saveDataSharedOpinionModifierHub))
		{
			return;
		}
		foreach (SaveDataSharedOpinionModifier value in saveDataSharedOpinionModifierHub.hub.Values)
		{
			value.Load();
		}
	}

	public void LoadFactionReferences()
	{
		for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++)
		{
			Faction faction = FactionManager.Instance.allFactions[i];
			SaveDataFaction fromSaveHub = GetFromSaveHub<SaveDataFaction>(OBJECT_TYPE.Faction, faction.persistentID);
			faction.LoadReferences(fromSaveHub);
		}
	}

	public void LoadPlayerReferences()
	{
		PlayerManager.Instance.player.LoadReferences(playerSave);
	}

	public void LoadCharacterReferences()
	{
		for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++)
		{
			Character character = CharacterManager.Instance.allCharacters[i];
			SaveDataCharacter fromSaveHub = GetFromSaveHub<SaveDataCharacter>(OBJECT_TYPE.Character, character.persistentID);
			character.LoadReferences(fromSaveHub);
		}
		for (int j = 0; j < CharacterManager.Instance.limboCharacters.Count; j++)
		{
			Character character2 = CharacterManager.Instance.limboCharacters[j];
			SaveDataCharacter fromSaveHub2 = GetFromSaveHub<SaveDataCharacter>(OBJECT_TYPE.Character, character2.persistentID);
			character2.LoadReferences(fromSaveHub2);
		}
	}

	public void LoadActionReferences()
	{
		foreach (KeyValuePair<string, ActualGoapNode> allAction in DatabaseManager.Instance.actionDatabase.allActions)
		{
			SaveDataActualGoapNode fromSaveHub = GetFromSaveHub<SaveDataActualGoapNode>(OBJECT_TYPE.Action, allAction.Key);
			allAction.Value.LoadReferences(fromSaveHub);
		}
	}

	public void LoadAdditionalActionReferences()
	{
		foreach (KeyValuePair<string, ActualGoapNode> allAction in DatabaseManager.Instance.actionDatabase.allActions)
		{
			SaveDataActualGoapNode fromSaveHub = GetFromSaveHub<SaveDataActualGoapNode>(OBJECT_TYPE.Action, allAction.Key);
			allAction.Value.LoadAdditionalReferences(fromSaveHub);
		}
	}

	public void LoadInterruptReferences()
	{
		foreach (KeyValuePair<string, InterruptHolder> allInterrupt in DatabaseManager.Instance.interruptDatabase.allInterrupts)
		{
			SaveDataInterruptHolder fromSaveHub = GetFromSaveHub<SaveDataInterruptHolder>(OBJECT_TYPE.Interrupt, allInterrupt.Key);
			allInterrupt.Value.LoadReferences(fromSaveHub);
		}
	}

	public void LoadPartyReferences()
	{
		foreach (KeyValuePair<string, Party> allParty in DatabaseManager.Instance.partyDatabase.allParties)
		{
			SaveDataParty fromSaveHub = GetFromSaveHub<SaveDataParty>(OBJECT_TYPE.Party, allParty.Key);
			allParty.Value.LoadReferences(fromSaveHub);
		}
	}

	public void LoadPartyQuestReferences()
	{
		foreach (KeyValuePair<string, PartyQuest> allPartyQuest in DatabaseManager.Instance.partyQuestDatabase.allPartyQuests)
		{
			SaveDataPartyQuest fromSaveHub = GetFromSaveHub<SaveDataPartyQuest>(OBJECT_TYPE.Party_Quest, allPartyQuest.Key);
			allPartyQuest.Value.LoadReferences(fromSaveHub);
		}
	}

	public void LoadCrimeReferences()
	{
		foreach (KeyValuePair<string, CrimeData> allCrime in DatabaseManager.Instance.crimeDatabase.allCrimes)
		{
			SaveDataCrimeData fromSaveHub = GetFromSaveHub<SaveDataCrimeData>(OBJECT_TYPE.Crime, allCrime.Key);
			allCrime.Value.LoadReferences(fromSaveHub);
		}
	}

	public void LoadGatheringReferences()
	{
		foreach (KeyValuePair<string, Gathering> allGathering in DatabaseManager.Instance.gatheringDatabase.allGatherings)
		{
			SaveDataGathering fromSaveHub = GetFromSaveHub<SaveDataGathering>(OBJECT_TYPE.Gathering, allGathering.Key);
			allGathering.Value.LoadReferences(fromSaveHub);
		}
	}

	public void LoadTraitsSecondWave()
	{
		foreach (KeyValuePair<string, Trait> item in DatabaseManager.Instance.traitDatabase.traitsByGUID)
		{
			SaveDataTrait fromSaveHub = GetFromSaveHub<SaveDataTrait>(OBJECT_TYPE.Trait, item.Key);
			if (fromSaveHub != null)
			{
				item.Value.LoadSecondWaveInstancedTrait(fromSaveHub);
			}
		}
	}

	public void LoadCharactersCurrentAction()
	{
		for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++)
		{
			CharacterManager.Instance.allCharacters[i].LoadCurrentlyDoingAction();
		}
	}

	public void LoadGameAlerts()
	{
		if (!objectHub.ContainsKey(OBJECT_TYPE.Game_Alert) || !(objectHub[OBJECT_TYPE.Game_Alert] is SaveDataGameAlertHub saveDataGameAlertHub))
		{
			return;
		}
		foreach (SaveDataGameAlert value in saveDataGameAlertHub.hub.Values)
		{
			value.Load().LoadReferences(value);
		}
	}

	public void LoadPortraitAvailability()
	{
		List<CharacterPortraitSpriteCollection> list = RuinarchListPool<CharacterPortraitSpriteCollection>.Claim();
		CharacterManager.Instance.portraitCollection.PopulateAllSpriteCollection(list);
		if (portraitAvailability != null)
		{
			for (int i = 0; i < portraitAvailability.Length; i++)
			{
				bool[] array = portraitAvailability[i];
				list[i].SetPortraitAvailability(array);
			}
		}
		RuinarchListPool<CharacterPortraitSpriteCollection>.Release(list);
	}

	public void CleanUp()
	{
		worldSettingsData = null;
		familyTreeDatabase = null;
		playerSave?.CleanUp();
		playerSave = null;
		savedPlagueDisease?.CleanUp();
		savedPlagueDisease = null;
		foreach (KeyValuePair<OBJECT_TYPE, BaseSaveDataHub> item in objectHub)
		{
			item.Value.CleanUp();
		}
		objectHub?.Clear();
		objectHub = null;
		worldMapSave?.CleanUp();
		worldMapSave = null;
		if (portraitAvailability != null)
		{
			for (int i = 0; i < portraitAvailability.Length; i++)
			{
				portraitAvailability[i] = null;
			}
		}
		portraitAvailability = null;
	}
}

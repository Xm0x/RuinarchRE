using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;

public class LoadMainThreadReferences : MapGenerationComponent
{
	public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData)
	{
		LevelLoaderManager.Instance.UpdateLoadingInfo("Loading_References");
		yield return MapGenerator.Instance.StartCoroutine(Load(data, saveData));
	}

	private IEnumerator Load(MapGenerationData data, SaveDataCurrentProgress saveData)
	{
		yield return MapGenerator.Instance.StartCoroutine(LoadRegionReferences(saveData));
		yield return MapGenerator.Instance.StartCoroutine(LoadFactionReferences(saveData));
		yield return MapGenerator.Instance.StartCoroutine(LoadSettlementReferences(saveData));
		yield return MapGenerator.Instance.StartCoroutine(LoadCharacterReferences(saveData));
		yield return MapGenerator.Instance.StartCoroutine(LoadPlayerReferences(data, saveData));
		yield return MapGenerator.Instance.StartCoroutine(LoadGridTileReferences(data, saveData));
		yield return MapGenerator.Instance.StartCoroutine(LoadStructureReferences(data, saveData));
		yield return MapGenerator.Instance.StartCoroutine(LoadPartyQuestReferences(data, saveData));
	}

	private IEnumerator LoadRegionReferences(SaveDataCurrentProgress saveData)
	{
		DatabaseManager.Instance.regionDatabase.mainRegion.LoadReferencesMainThread(saveData.worldMapSave.regionSave);
		yield return null;
	}

	private IEnumerator LoadFactionReferences(SaveDataCurrentProgress saveData)
	{
		for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++)
		{
			Faction faction = FactionManager.Instance.allFactions[i];
			SaveDataFaction fromSaveHub = saveData.GetFromSaveHub<SaveDataFaction>(OBJECT_TYPE.Faction, faction.persistentID);
			faction.LoadReferencesMainThread(fromSaveHub);
		}
		yield return null;
	}

	private IEnumerator LoadCharacterReferences(SaveDataCurrentProgress saveData)
	{
		for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++)
		{
			Character character = CharacterManager.Instance.allCharacters[i];
			SaveDataCharacter fromSaveHub = saveData.GetFromSaveHub<SaveDataCharacter>(OBJECT_TYPE.Character, character.persistentID);
			character.LoadReferencesMainThread(fromSaveHub);
			yield return null;
		}
		for (int j = 0; j < CharacterManager.Instance.limboCharacters.Count; j++)
		{
			Character character2 = CharacterManager.Instance.limboCharacters[j];
			SaveDataCharacter fromSaveHub2 = saveData.GetFromSaveHub<SaveDataCharacter>(OBJECT_TYPE.Character, character2.persistentID);
			character2.LoadReferencesMainThread(fromSaveHub2);
		}
		yield return null;
	}

	private IEnumerator LoadSettlementReferences(SaveDataCurrentProgress saveData)
	{
		for (int i = 0; i < saveData.worldMapSave.settlementSaves.Count; i++)
		{
			SaveDataBaseSettlement saveDataBaseSettlement = saveData.worldMapSave.settlementSaves[i];
			DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(saveDataBaseSettlement._persistentID).LoadReferencesMainThread(saveDataBaseSettlement);
			yield return null;
		}
	}

	private IEnumerator LoadPlayerReferences(MapGenerationData data, SaveDataCurrentProgress saveData)
	{
		PlayerManager.Instance.player.LoadReferencesMainThread(saveData.playerSave);
		yield return null;
	}

	private IEnumerator LoadGridTileReferences(MapGenerationData data, SaveDataCurrentProgress saveData)
	{
		SaveDataRegion saveDataRegion = saveData.worldMapSave.regionSave;
		int batchCount = 0;
		for (int j = 0; j < saveDataRegion.innerMapSave.tileSaves.Values.Count; j++)
		{
			SaveDataLocationGridTile saveDataLocationGridTile = saveDataRegion.innerMapSave.tileSaves.Values.ElementAt(j);
			DatabaseManager.Instance.locationGridTileDatabase.GetTileByPersistentID(saveDataLocationGridTile.persistentID).LoadSecondWave(saveDataLocationGridTile);
			batchCount++;
			if (batchCount == MapGenerationData.LocationGridTileSecondaryWaveBatches)
			{
				batchCount = 0;
				yield return null;
			}
		}
	}

	private IEnumerator LoadStructureReferences(MapGenerationData data, SaveDataCurrentProgress saveData)
	{
		int batchCount = 0;
		for (int i = 0; i < saveData.worldMapSave.structureSaves.Count; i++)
		{
			SaveDataLocationStructure saveDataLocationStructure = saveData.worldMapSave.structureSaves[i];
			LocationStructure structureByPersistentID = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(saveDataLocationStructure.persistentID);
			if (DatabaseManager.Instance.structureDatabase.structuresToBeLoadedOnMainThread.Contains(structureByPersistentID))
			{
				structureByPersistentID.LoadStructureSecondWaveInMainThread(saveDataLocationStructure);
				batchCount++;
				if (batchCount == MapGenerationData.LocationStructureSecondaryWaveBatches)
				{
					batchCount = 0;
					yield return null;
				}
			}
		}
	}

	private IEnumerator LoadPartyQuestReferences(MapGenerationData data, SaveDataCurrentProgress saveData)
	{
		foreach (KeyValuePair<string, PartyQuest> allPartyQuest in DatabaseManager.Instance.partyQuestDatabase.allPartyQuests)
		{
			SaveDataPartyQuest fromSaveHub = saveData.GetFromSaveHub<SaveDataPartyQuest>(OBJECT_TYPE.Party_Quest, allPartyQuest.Key);
			allPartyQuest.Value.LoadReferencesInMainThread(fromSaveHub);
			yield return null;
		}
	}
}

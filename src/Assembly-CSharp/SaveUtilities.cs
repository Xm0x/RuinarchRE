using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Newtonsoft.Json;
using UnityEngine;
using UtilityScripts;

public static class SaveUtilities
{
	public static List<string> compatibleSaveFileVersions = new List<string> { "1.05", "1.1", "1.101", "1.102" };

	public static List<string> ConvertSavableListToIDs<T>(List<T> savables) where T : ISavable
	{
		List<string> list = RuinarchListPool<string>.Claim();
		if (savables != null && savables.Count > 0)
		{
			for (int i = 0; i < savables.Count; i++)
			{
				list.Add(savables[i].persistentID);
			}
		}
		return list;
	}

	public static string[] ConvertSavableListToIDsArray<T>(List<T> savables) where T : ISavable
	{
		if (savables != null && savables.Count > 0)
		{
			string[] array = new string[savables.Count];
			for (int i = 0; i < savables.Count; i++)
			{
				array[i] = savables[i].persistentID;
			}
			return array;
		}
		return null;
	}

	public static List<POIData> ConvertPOIListToPOIData(List<IPointOfInterest> savables)
	{
		List<POIData> list = RuinarchListPool<POIData>.Claim();
		if (savables != null && savables.Count > 0)
		{
			for (int i = 0; i < savables.Count; i++)
			{
				list.Add(new POIData(savables[i]));
			}
		}
		return list;
	}

	public static List<Character> ConvertIDListToCharacters(List<string> ids)
	{
		List<Character> list = new List<Character>();
		for (int i = 0; i < ids.Count; i++)
		{
			string id = ids[i];
			Character characterByPersistentID = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(id);
			if (characterByPersistentID != null)
			{
				list.Add(characterByPersistentID);
			}
		}
		return list;
	}

	public static void ConvertIDArrayToCharacters(string[] ids, List<Character> p_list)
	{
		foreach (string id in ids)
		{
			Character characterByPersistentID = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(id);
			if (characterByPersistentID != null)
			{
				p_list.Add(characterByPersistentID);
			}
		}
	}

	public static List<Summon> ConvertIDListToMonsters(List<string> ids)
	{
		List<Summon> list = new List<Summon>();
		for (int i = 0; i < ids.Count; i++)
		{
			string id = ids[i];
			if (DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(id) is Summon item)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static List<TileObject> ConvertIDListToTileObjects(List<string> ids)
	{
		List<TileObject> list = new List<TileObject>();
		for (int i = 0; i < ids.Count; i++)
		{
			string id = ids[i];
			TileObject tileObjectByPersistentID = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(id);
			list.Add(tileObjectByPersistentID);
		}
		return list;
	}

	public static List<LocationGridTile> ConvertIDListToLocationGridTiles(List<string> ids)
	{
		List<LocationGridTile> list = new List<LocationGridTile>();
		for (int i = 0; i < ids.Count; i++)
		{
			string id = ids[i];
			LocationGridTile tileByPersistentID = DatabaseManager.Instance.locationGridTileDatabase.GetTileByPersistentID(id);
			list.Add(tileByPersistentID);
		}
		return list;
	}

	public static List<LocationStructure> ConvertIDListToStructures(List<string> ids)
	{
		List<LocationStructure> list = new List<LocationStructure>();
		for (int i = 0; i < ids.Count; i++)
		{
			string id = ids[i];
			LocationStructure structureByPersistentIDSafe = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentIDSafe(id);
			if (structureByPersistentIDSafe != null)
			{
				list.Add(structureByPersistentIDSafe);
			}
		}
		return list;
	}

	public static List<Faction> ConvertIDListToFactions(List<string> ids)
	{
		List<Faction> list = new List<Faction>();
		for (int i = 0; i < ids.Count; i++)
		{
			string id = ids[i];
			Faction factionBasedOnPersistentID = DatabaseManager.Instance.factionDatabase.GetFactionBasedOnPersistentID(id);
			list.Add(factionBasedOnPersistentID);
		}
		return list;
	}

	public static List<IPointOfInterest> ConvertPOIDataListToPOIList(List<POIData> p_data)
	{
		List<IPointOfInterest> list = new List<IPointOfInterest>();
		for (int i = 0; i < p_data.Count; i++)
		{
			POIData pOIData = p_data[i];
			IPointOfInterest pointOfInterest = pOIData.poiType switch
			{
				POINT_OF_INTEREST_TYPE.CHARACTER => DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(pOIData.poiID), 
				POINT_OF_INTEREST_TYPE.TILE_OBJECT => DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(pOIData.poiID), 
				_ => throw new ArgumentOutOfRangeException(), 
			};
			if (pointOfInterest != null)
			{
				list.Add(pointOfInterest);
			}
		}
		return list;
	}

	public static SaveDataJobNode createSaveDataJobNode(JobNode jobNode)
	{
		return new SaveDataSingleJobNode();
	}

	public static SaveDataTileObject CreateNewSaveDataForTileObject(string tileObjectTypeString)
	{
		Type type = Type.GetType("SaveData" + tileObjectTypeString + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
		if (type != null)
		{
			return Activator.CreateInstance(type) as SaveDataTileObject;
		}
		return new SaveDataTileObject();
	}

	public static SaveDataTileObject CreateNewSaveDataForArtifact(string tileObjectTypeString)
	{
		Type type = Type.GetType("SaveData" + tileObjectTypeString + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
		if (type != null)
		{
			return Activator.CreateInstance(type) as SaveDataTileObject;
		}
		return new SaveDataArtifact();
	}

	public static SaveDataStructureRoom CreateSaveDataForRoom(StructureRoom structureRoom)
	{
		if (structureRoom is PrisonCell)
		{
			return new SaveDataPrisonCell();
		}
		if (structureRoom is KennelCell)
		{
			return new SaveDataKennelCell();
		}
		return new SaveDataStructureRoom();
	}

	public static string GetGameVersionOfSaveFile(string json)
	{
		JsonTextReader jsonTextReader = new JsonTextReader(new StringReader(json));
		string text = string.Empty;
		while (jsonTextReader.Read())
		{
			if (jsonTextReader.Value != null)
			{
				if (jsonTextReader.TokenType == JsonToken.PropertyName)
				{
					text = jsonTextReader.Value.ToString();
				}
				if (text == "gameVersion" && jsonTextReader.TokenType == JsonToken.String)
				{
					return jsonTextReader.Value.ToString();
				}
			}
		}
		return string.Empty;
	}

	public static bool IsSaveFileValid(string path)
	{
		string text = string.Empty;
		using (ZipArchive zipArchive = ZipFile.Open(path, ZipArchiveMode.Read))
		{
			foreach (ZipArchiveEntry entry in zipArchive.Entries)
			{
				if (entry.Name == "mainSave.sav")
				{
					using (StreamReader streamReader = new StreamReader(entry.Open()))
					{
						text = streamReader.ReadToEnd();
					}
					break;
				}
			}
		}
		if (!string.IsNullOrEmpty(text))
		{
			return IsSaveFileVersionCompatible(GetGameVersionOfSaveFile(text));
		}
		return false;
	}

	public static bool IsSaveFileVersionCompatible(string p_version)
	{
		if (!(p_version == Application.version))
		{
			return compatibleSaveFileVersions.Contains(p_version);
		}
		return true;
	}

	public static bool AreAppliedModsCompatible(SaveDataQuickInfo loadedInfo)
	{
		List<InstalledModData> installedModCollection = ExternalFileManager.Instance.GetInstalledModCollection();
		if (loadedInfo.appliedMods == null || loadedInfo.appliedMods.Count <= 0)
		{
			for (int i = 0; i < installedModCollection.Count; i++)
			{
				if (installedModCollection[i].applyMod)
				{
					return false;
				}
			}
			return true;
		}
		int num = 0;
		for (int j = 0; j < installedModCollection.Count; j++)
		{
			InstalledModData installedModData = installedModCollection[j];
			if (installedModData.applyMod)
			{
				num++;
				if (!loadedInfo.IsModInSaveFile(installedModData))
				{
					return false;
				}
			}
		}
		return num == loadedInfo.appliedMods.Count;
	}
}

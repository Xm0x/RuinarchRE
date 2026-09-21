using System;
using System.IO;
using System.Linq;
using BayatGames.SaveGameFree;
using Locations.Area_Features;
using Scenario_Maps;
using UnityEngine;
using UnityEngine.SceneManagement;
using UtilityScripts;

public class SaveManager : MonoBehaviour
{
	public static SaveManager Instance;

	public SavePlayerManager savePlayerManager;

	public SaveCurrentProgressManager saveCurrentProgressManager;

	public const int HexTile_Save_Batches = 200;

	public const int Settlement_Save_Batches = 200;

	public const int Structure_Save_Batches = 200;

	public bool useSaveData { get; private set; }

	public bool doNotContinueSaving { get; private set; }

	public SaveDataPlayer currentSaveDataPlayer => savePlayerManager.currentSaveDataPlayer;

	public SaveDataCurrentProgress currentSaveDataProgress => saveCurrentProgressManager.currentSaveDataProgress;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			SceneManager.sceneUnloaded += OnSceneUnloaded;
			if (!Directory.Exists(Utilities.gameSavePath))
			{
				Directory.CreateDirectory(Utilities.gameSavePath);
			}
			if (!Directory.Exists(Utilities.autosavePath))
			{
				Directory.CreateDirectory(Utilities.autosavePath);
			}
			PrepareTempDirectory();
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void OnApplicationQuit()
	{
		savePlayerManager.SavePlayerData();
	}

	private void OnEditorQuit()
	{
		savePlayerManager.SavePlayerData();
	}

	private void OnSceneUnloaded(Scene unloaded)
	{
		if (unloaded.name == "Game" && (saveCurrentProgressManager.isSaving || saveCurrentProgressManager.isWritingToDisk))
		{
			SetDoNotContinueSaving(p_state: true);
		}
	}

	public void SetDoNotContinueSaving(bool p_state)
	{
		doNotContinueSaving = p_state;
	}

	public void PrepareTempDirectory()
	{
		if (!saveCurrentProgressManager.isSaving && !saveCurrentProgressManager.isWritingToDisk)
		{
			if (Directory.Exists(Utilities.tempPath))
			{
				Directory.Delete(Utilities.tempPath, recursive: true);
			}
			Directory.CreateDirectory(Utilities.tempPath);
			Directory.CreateDirectory(Utilities.tempZipPath);
		}
	}

	public void DeleteSaveFilesInTempDirectory()
	{
		string[] files = Directory.GetFiles(Utilities.tempPath, "*.sav");
		for (int i = 0; i < files.Length; i++)
		{
			File.Delete(files[i]);
		}
	}

	public void SetUseSaveData(bool state)
	{
		useSaveData = state;
	}

	public void SaveScenario(string fileName = "")
	{
		ScenarioMapData scenarioMapData = new ScenarioMapData();
		ScenarioWorldMapSave scenarioWorldMapSave = new ScenarioWorldMapSave();
		scenarioWorldMapSave.SaveWorld(WorldConfigManager.Instance.mapGenerationData.chosenWorldMapTemplate, GridMap.Instance.allAreas, GridMap.Instance.mainRegion.innerMap.elevationPerlinSettings, GridMap.Instance.mainRegion.innerMap.warpWeight, GridMap.Instance.mainRegion.innerMap.temperatureSeed, GridMap.Instance.mainRegion.villageSpots);
		scenarioMapData.worldMapSave = scenarioWorldMapSave;
		scenarioMapData.SaveVillageSettlements(LandmarkManager.Instance.allNonPlayerSettlements.Where((NPCSettlement x) => x.locationType == LOCATION_TYPE.VILLAGE).ToList());
		if (string.IsNullOrEmpty(fileName))
		{
			fileName = "SAVED_CURRENT_PROGRESS";
		}
		SaveGame.Save(Application.streamingAssetsPath + "/Scenario Maps/" + fileName + ".json", scenarioMapData);
	}

	public void LoadSaveDataPlayer()
	{
		savePlayerManager.LoadSaveDataPlayer();
	}

	public static SaveDataAreaFeature ConvertAreaFeatureToSaveData(AreaFeature p_areaFeature)
	{
		SaveDataAreaFeature saveDataAreaFeature = null;
		Type serializedData = p_areaFeature.serializedData;
		if (serializedData != null)
		{
			return Activator.CreateInstance(serializedData) as SaveDataAreaFeature;
		}
		return new SaveDataAreaFeature();
	}

	public ScenarioMapData GetScenarioMapData(string path)
	{
		return SaveGame.Load<ScenarioMapData>(path);
	}
}

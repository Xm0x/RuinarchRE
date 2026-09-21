using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using BayatGames.SaveGameFree;
using Inner_Maps.Location_Structures;
using Managers;
using Settings;
using UnityEngine;
using UtilityScripts;

public class SaveCurrentProgressManager : MonoBehaviour
{
	public const string savedCurrentProgressFileName = "SAVED_CURRENT_PROGRESS";

	public static readonly object THREAD_LOCKER = new object();

	private bool generalSaveFlag;

	private bool saveFileWriteFlag;

	private readonly List<SaveTileObjectThreadQueueItem> objectThreadItems = new List<SaveTileObjectThreadQueueItem>();

	private string _filePath;

	private string _loadSavePath;

	private string _fileName;

	private bool _isAutosave;

	private int _currentAutosaveCount;

	private bool _shouldAutosaveAfterSaving;

	public SaveDataCurrentProgress currentSaveDataProgress { get; private set; }

	public bool isSaving { get; private set; }

	public bool isWritingToDisk { get; private set; }

	public string currentSaveDataPath { get; private set; }

	private void LateUpdate()
	{
		if (isWritingToDisk && saveFileWriteFlag)
		{
			DoneSaveFileWriteToDisk();
		}
	}

	public void AddToSaveHub<T>(T data) where T : ISavable
	{
		currentSaveDataProgress.AddToSaveHub(data);
	}

	public bool CanSaveCurrentProgress()
	{
		if (UIManager.Instance != null)
		{
			if (SaveWindowUIController.Instance.isShowing && SaveWindowUIController.Instance.windowFunction == SaveWindowUIController.Window_Function.Load)
			{
				return false;
			}
			if (UIManager.Instance.IsShowingEndScreen())
			{
				return false;
			}
		}
		if (!GameManager.Instance.gameHasStarted)
		{
			return false;
		}
		return true;
	}

	public void DoManualSave(string fileName = "", Action saveCallback = null, bool isAutosave = false)
	{
		StartCoroutine(SaveThisGame(fileName, saveCallback, isAutosave));
	}

	private SaveDataQuickInfo CreateSaveDataQuickInfo()
	{
		SaveDataQuickInfo saveDataQuickInfo = new SaveDataQuickInfo();
		saveDataQuickInfo.saveVersion = Application.version;
		saveDataQuickInfo.scenarioName = Utilities.NotNormalizedConversionEnumToString(WorldSettings.Instance.worldSettingsData.worldType.ToString());
		saveDataQuickInfo.omnipotentMode = WorldSettings.Instance.worldSettingsData.playerSkillSettings.omnipotentMode;
		saveDataQuickInfo.archetype = PlayerSkillManager.Instance.selectedArchetype;
		saveDataQuickInfo.SaveAppliedMods();
		if (PlayerManager.Instance.player.playerSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.THE_PORTAL) is ThePortal thePortal)
		{
			saveDataQuickInfo.portalLevel = thePortal.level;
		}
		else
		{
			saveDataQuickInfo.portalLevel = 1;
		}
		return saveDataQuickInfo;
	}

	public string GetFileName()
	{
		return WorldSettings.Instance.worldSettingsData.worldType.ToString() + "-" + GameManager.Instance.continuousDays + "_" + GameManager.Instance.ConvertTickToTime(GameManager.Instance.currentTick, "-");
	}

	public string GetAutosaveFileName()
	{
		return string.Format("{0}_Autosave (Day {1} {2})", DateTime.Now.ToString("s").Replace(":", ""), GameManager.Instance.continuousDays, GameManager.Instance.ConvertTickToTime(GameManager.Instance.currentTick, "-"));
	}

	private IEnumerator SaveThisGame(string fileName, Action saveCallback = null, bool isAutosave = false)
	{
		isSaving = true;
		_isAutosave = isAutosave;
		if (isAutosave)
		{
			_fileName = GetAutosaveFileName();
		}
		else
		{
			_fileName = fileName;
		}
		UIManager.Instance.canvas.enabled = false;
		UIManager.Instance.smallInfoCanvas.enabled = false;
		yield return GameUtilities.waitForEndOfFrame;
		ScreenCapture.CaptureScreenshot(Utilities.tempZipPath + "screen.png");
		UIManager.Instance.canvas.enabled = true;
		UIManager.Instance.smallInfoCanvas.enabled = true;
		InnerMapCameraMove.Instance.DisableMovement();
		UIManager.Instance.optionsMenu.ShowSaveLoading();
		SaveDataQuickInfo obj = CreateSaveDataQuickInfo();
		SaveGame.Save(Utilities.tempZipPath + "quickInfo.json", obj);
		_filePath = Utilities.tempZipPath + "mainSave.sav";
		objectThreadItems.Clear();
		saveFileWriteFlag = false;
		generalSaveFlag = false;
		currentSaveDataProgress = new SaveDataCurrentProgress();
		currentSaveDataProgress.fileName = _fileName;
		currentSaveDataProgress.Initialize();
		CharacterManager.Instance.ProcessCharactersMarkedForCleanUp();
		LandmarkManager.Instance.ProcessStructuresMarkedForCleanUp();
		while (MultiThreadPool.Instance.IsThereStillFunctionsToBeResolved())
		{
			yield return null;
		}
		ThreadPool.QueueUserWorkItem(SaveGeneralMultithread);
		foreach (KeyValuePair<TILE_OBJECT_TYPE, List<TileObject>> allTileObject in DatabaseManager.Instance.tileObjectDatabase.allTileObjects)
		{
			List<TileObject> list = RuinarchListPool<TileObject>.Claim(allTileObject.Value.Count);
			list.AddRange(allTileObject.Value);
			SaveTileObjectThreadQueueItem saveTileObjectThreadQueueItem = new SaveTileObjectThreadQueueItem
			{
				list = list,
				isDone = false
			};
			objectThreadItems.Add(saveTileObjectThreadQueueItem);
			if (allTileObject.Key == TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT)
			{
				ThreadPool.QueueUserWorkItem(SaveGenericTileObjectsMultithread, saveTileObjectThreadQueueItem);
			}
			else
			{
				ThreadPool.QueueUserWorkItem(SaveTileObjectsMultithread, saveTileObjectThreadQueueItem);
			}
		}
		SaveTileObjectThreadQueueItem saveTileObjectThreadQueueItem2 = new SaveTileObjectThreadQueueItem
		{
			list = null,
			isDone = false
		};
		objectThreadItems.Add(saveTileObjectThreadQueueItem2);
		ThreadPool.QueueUserWorkItem(SaveDestroyedTileObjectsMultithread, saveTileObjectThreadQueueItem2);
		currentSaveDataProgress.SavePortraitAvailability();
		DatabaseManager.Instance.mainSQLDatabase.SaveInMemoryDatabaseToFile(Utilities.tempZipPath + "gameDB.db");
		yield return null;
		while (IsThereStillAProcessingThread())
		{
			yield return null;
		}
		UIManager.Instance.optionsMenu.HideSaveLoading();
		isSaving = false;
		InnerMapCameraMove.Instance.EnableMovement();
		saveCallback?.Invoke();
		SaveFileWriteToDisk();
	}

	private IEnumerator DoneSaveFileWriteToDiskEnumerator()
	{
		if (SaveManager.Instance.doNotContinueSaving)
		{
			if (File.Exists(_filePath))
			{
				File.Delete(_filePath);
			}
			if (File.Exists(Utilities.tempZipPath + "gameDB.db"))
			{
				File.Delete(Utilities.tempZipPath + "gameDB.db");
			}
			if (File.Exists(Utilities.tempZipPath + "quickInfo.json"))
			{
				File.Delete(Utilities.tempZipPath + "quickInfo.json");
			}
			if (File.Exists(Utilities.tempZipPath + "screen.png"))
			{
				File.Delete(Utilities.tempZipPath + "screen.png");
			}
			SetIsWritingToDisk(p_state: false);
			yield return null;
		}
		else
		{
			string text = Utilities.gameSavePath;
			if (_isAutosave)
			{
				text = Utilities.autosavePath;
			}
			string destinationArchiveFileName = text + "/" + _fileName + ".zip";
			ZipFile.CreateFromDirectory(Utilities.tempZipPath, destinationArchiveFileName);
			yield return null;
			File.Delete(_filePath);
			File.Delete(Utilities.tempZipPath + "gameDB.db");
			File.Delete(Utilities.tempZipPath + "quickInfo.json");
			File.Delete(Utilities.tempZipPath + "screen.png");
			yield return null;
			if (_isAutosave)
			{
				CheckAutosaves();
			}
			SetIsWritingToDisk(p_state: false);
		}
		SaveManager.Instance.SetDoNotContinueSaving(p_state: false);
		yield return null;
		if (_shouldAutosaveAfterSaving)
		{
			SetShouldAutosaveAfterSaving(p_state: false);
			Autosave();
		}
	}

	private void SaveFileWriteToDisk()
	{
		SetIsWritingToDisk(p_state: true);
		ThreadPool.QueueUserWorkItem(SaveToFileMultithread);
	}

	private void DoneSaveFileWriteToDisk()
	{
		saveFileWriteFlag = false;
		CleanUp();
		StartCoroutine(DoneSaveFileWriteToDiskEnumerator());
	}

	private void SetIsWritingToDisk(bool p_state)
	{
		isWritingToDisk = p_state;
		if (UIManager.Instance != null)
		{
			if (isWritingToDisk)
			{
				UIManager.Instance.ShowSaveWritingToDisk();
			}
			else
			{
				UIManager.Instance.HideSaveWritingToDisk();
			}
			UIManager.Instance.optionsMenu.UpdateButtonsForSaving();
		}
	}

	private bool IsThereStillAProcessingThread()
	{
		if (!generalSaveFlag)
		{
			return true;
		}
		for (int i = 0; i < objectThreadItems.Count; i++)
		{
			if (!objectThreadItems[i].isDone)
			{
				return true;
			}
		}
		return false;
	}

	private void SaveGeneralMultithread(object state)
	{
		try
		{
			lock (THREAD_LOCKER)
			{
				currentSaveDataProgress.SaveDate();
				currentSaveDataProgress.SaveWorldSettings();
				currentSaveDataProgress.SavePlayer();
				currentSaveDataProgress.SaveVictoryCondition();
				currentSaveDataProgress.SavePlagueDisease();
				currentSaveDataProgress.SaveFactions();
				currentSaveDataProgress.SaveCharacters();
				currentSaveDataProgress.SaveJobs();
				currentSaveDataProgress.SaveGameAlerts();
				currentSaveDataProgress.SaveSharedOpinionModifiers();
				currentSaveDataProgress.familyTreeDatabase = DatabaseManager.Instance.familyTreeDatabase;
				SaveWorldMultithread();
				generalSaveFlag = true;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.StackTrace + "\n" + ex.Message);
		}
	}

	private void SaveTileObjectsMultithread(object state)
	{
		try
		{
			SaveTileObjectThreadQueueItem obj = state as SaveTileObjectThreadQueueItem;
			List<TileObject> list = obj.list;
			currentSaveDataProgress.SaveTileObjects(list);
			obj.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.StackTrace + "\n" + ex.Message);
		}
	}

	private void SaveGenericTileObjectsMultithread(object state)
	{
		try
		{
			SaveTileObjectThreadQueueItem obj = state as SaveTileObjectThreadQueueItem;
			List<TileObject> list = obj.list;
			currentSaveDataProgress.SaveGenericTileObjects(list);
			obj.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.StackTrace + "\n" + ex.Message);
		}
	}

	private void SaveDestroyedTileObjectsMultithread(object state)
	{
		try
		{
			SaveTileObjectThreadQueueItem obj = state as SaveTileObjectThreadQueueItem;
			currentSaveDataProgress.SaveDestroyedTileObjects();
			obj.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.StackTrace + "\n" + ex.Message);
		}
	}

	private void SaveWorldMultithread()
	{
		WorldMapSave worldMapSave = new WorldMapSave();
		worldMapSave.SaveWorld(WorldConfigManager.Instance.mapGenerationData.chosenWorldMapTemplate, DatabaseManager.Instance.areaDatabase, DatabaseManager.Instance.regionDatabase, DatabaseManager.Instance.settlementDatabase, DatabaseManager.Instance.structureDatabase, WorldEventManager.Instance.activeEvents);
		currentSaveDataProgress.worldMapSave = worldMapSave;
	}

	private void SaveToFileMultithread(object state)
	{
		if (string.IsNullOrEmpty(_fileName))
		{
			string text = currentSaveDataProgress.timeStamp.ToString("yyyy-MM-dd_HHmmss") ?? "";
			_fileName = string.Format("{0}_{1}_{2}({3})", currentSaveDataProgress.worldMapSave.worldType, currentSaveDataProgress.continuousDays, GameManager.Instance.ConvertTickToTime(currentSaveDataProgress.tick, "-"), text);
		}
		SaveCurrentDataToFile();
		saveFileWriteFlag = true;
	}

	private void SaveCurrentDataToFile()
	{
		SaveGame.Save(_filePath, currentSaveDataProgress);
	}

	public void Autosave()
	{
		if (SettingsManager.Instance.settings.shouldAutosave && SaveManager.Instance.saveCurrentProgressManager.CanSaveCurrentProgress())
		{
			if (!SaveManager.Instance.saveCurrentProgressManager.isSaving && !SaveManager.Instance.saveCurrentProgressManager.isWritingToDisk)
			{
				UIManager.Instance.Pause();
				UIManager.Instance.SetSpeedTogglesState(state: false);
				SaveManager.Instance.saveCurrentProgressManager.DoManualSave("", UIManager.Instance.ResumeLastProgressionSpeed, isAutosave: true);
			}
			else
			{
				SaveManager.Instance.saveCurrentProgressManager.SetShouldAutosaveAfterSaving(p_state: true);
			}
		}
	}

	public void SetShouldAutosaveAfterSaving(bool p_state)
	{
		_shouldAutosaveAfterSaving = p_state;
	}

	public void SetCurrentSaveDataPath(string path)
	{
		currentSaveDataPath = path;
	}

	public bool TryLoadSaveDataWithoutReadingFile()
	{
		if (currentSaveDataProgress != null && Path.GetFileNameWithoutExtension(currentSaveDataPath).Equals(currentSaveDataProgress.fileName))
		{
			return true;
		}
		return false;
	}

	public void LoadSaveDataCurrentProgress(LoadThreadQueueItem threadItem)
	{
		ZipFile.ExtractToDirectory(currentSaveDataPath, Utilities.tempPath);
		_loadSavePath = Utilities.tempPath + "mainSave.sav";
		ThreadPool.QueueUserWorkItem(ReadSaveDataFileInOtherThread, threadItem);
	}

	public void ReadSaveDataFileInOtherThread(object state)
	{
		LoadThreadQueueItem obj = state as LoadThreadQueueItem;
		currentSaveDataProgress = GetSaveFileData(_loadSavePath);
		obj.isDone = true;
	}

	private SaveDataCurrentProgress GetSaveFileData(string path)
	{
		return SaveGame.Load<SaveDataCurrentProgress>(path);
	}

	public bool HasAnySaveFiles()
	{
		string[] files = Directory.GetFiles(Utilities.autosavePath, "*.zip");
		if (Directory.GetFiles(Utilities.gameSavePath, "*.zip").Length == 0)
		{
			return files.Length != 0;
		}
		return true;
	}

	public string GetLatestSaveFile()
	{
		string[] files = Directory.GetFiles(Utilities.gameSavePath, "*.zip");
		string[] files2 = Directory.GetFiles(Utilities.autosavePath, "*.zip");
		string text = string.Empty;
		foreach (string text2 in files)
		{
			if (string.IsNullOrEmpty(text))
			{
				text = text2;
				continue;
			}
			DateTime lastWriteTime = File.GetLastWriteTime(text2);
			DateTime lastWriteTime2 = File.GetLastWriteTime(text);
			if (lastWriteTime > lastWriteTime2)
			{
				text = text2;
			}
		}
		foreach (string text3 in files2)
		{
			if (string.IsNullOrEmpty(text))
			{
				text = text3;
				continue;
			}
			DateTime lastWriteTime3 = File.GetLastWriteTime(text3);
			DateTime lastWriteTime4 = File.GetLastWriteTime(text);
			if (lastWriteTime3 > lastWriteTime4)
			{
				text = text3;
			}
		}
		return text;
	}

	public void CleanUpLoadedData()
	{
		currentSaveDataProgress?.CleanUp();
		currentSaveDataProgress = null;
	}

	private void CheckAutosaves()
	{
		_currentAutosaveCount = 0;
		string[] files = Directory.GetFiles(Utilities.autosavePath, "*.zip");
		if (files != null)
		{
			_currentAutosaveCount = files.Length;
		}
		if (_currentAutosaveCount <= 3)
		{
			return;
		}
		string text = string.Empty;
		foreach (string text2 in files)
		{
			if (string.IsNullOrEmpty(text))
			{
				text = text2;
				continue;
			}
			DateTime lastWriteTime = File.GetLastWriteTime(text2);
			DateTime lastWriteTime2 = File.GetLastWriteTime(text);
			if (lastWriteTime <= lastWriteTime2)
			{
				text = text2;
			}
		}
		if (!string.IsNullOrEmpty(text))
		{
			File.Delete(text);
		}
	}

	public void CleanUp()
	{
		for (int i = 0; i < objectThreadItems.Count; i++)
		{
			objectThreadItems[i].Reset();
		}
		objectThreadItems.Clear();
		currentSaveDataProgress?.CleanUp();
		currentSaveDataProgress = null;
	}
}

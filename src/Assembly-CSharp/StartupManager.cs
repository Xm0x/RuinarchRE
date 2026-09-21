using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class StartupManager : MonoBehaviour
{
	public MapGenerator mapGenerator;

	public Initializer initializer;

	private void Start()
	{
		Messenger.AddListener(Signals.GAME_LOADED, OnGameLoaded);
		Messenger.AddListener(UISignals.START_GAME_AFTER_LOADOUT_SELECT, OnLoadoutSelected);
		StartCoroutine(PerformStartup());
	}

	private IEnumerator PerformStartup()
	{
		LevelLoaderManager.Instance.SetLoadingState(state: true);
		LevelLoaderManager.Instance.UpdateLoadingInfo("Initializing_Data");
		initializer.InitializeDataBeforeWorldCreationMainThread();
		if (!string.IsNullOrEmpty(SaveManager.Instance.saveCurrentProgressManager.currentSaveDataPath))
		{
			yield return StartCoroutine(PerformStartUpLoadGame());
		}
		else
		{
			yield return StartCoroutine(PerformStartUpNewGame());
		}
	}

	private IEnumerator PerformStartUpLoadGame()
	{
		LevelLoaderManager.Instance.UpdateLoadingInfo("Reading_Save_File");
		LevelLoaderManager.Instance.UpdateLoadingBar(0.4f, 8f);
		LoadThreadQueueItem threadItemReadSaveFile = new LoadThreadQueueItem();
		LoadThreadQueueItem threadItemDataBeforeWorldCreation = new LoadThreadQueueItem();
		ThreadPool.QueueUserWorkItem(initializer.InitializeDataBeforeWorldCreationOtherThread, threadItemDataBeforeWorldCreation);
		SaveManager.Instance.saveCurrentProgressManager.LoadSaveDataCurrentProgress(threadItemReadSaveFile);
		while (!threadItemReadSaveFile.isDone || !threadItemDataBeforeWorldCreation.isDone)
		{
			yield return null;
		}
		if (!string.IsNullOrEmpty(SaveManager.Instance.saveCurrentProgressManager.currentSaveDataProgress.language))
		{
			for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; i++)
			{
				Locale locale = LocalizationSettings.AvailableLocales.Locales[i];
				if (locale.LocaleName == SaveManager.Instance.saveCurrentProgressManager.currentSaveDataProgress.language)
				{
					LocalizationSettings.SelectedLocale = locale;
					break;
				}
			}
		}
		yield return StartCoroutine(mapGenerator.InitializeSavedWorld(SaveManager.Instance.saveCurrentProgressManager.currentSaveDataProgress));
		SaveManager.Instance.DeleteSaveFilesInTempDirectory();
	}

	private IEnumerator PerformStartUpNewGame()
	{
		LoadThreadQueueItem threadItemDataBeforeWorldCreation = new LoadThreadQueueItem();
		ThreadPool.QueueUserWorkItem(initializer.InitializeDataBeforeWorldCreationOtherThread, threadItemDataBeforeWorldCreation);
		while (!threadItemDataBeforeWorldCreation.isDone)
		{
			yield return null;
		}
		LevelLoaderManager.Instance.UpdateLoadingInfo("Initializing_World");
		yield return StartCoroutine(mapGenerator.InitializeWorld());
	}

	private void OnGameLoaded()
	{
		Messenger.RemoveListener(Signals.GAME_LOADED, OnGameLoaded);
		initializer.InitializeDataAfterWorldCreation();
	}

	private void OnLoadoutSelected()
	{
		Messenger.RemoveListener(UISignals.START_GAME_AFTER_LOADOUT_SELECT, OnLoadoutSelected);
		initializer.InitializeDataAfterLoadoutSelection();
	}
}

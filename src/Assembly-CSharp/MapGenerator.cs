using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using Generator.Map_Generation.Components;
using UnityEngine;
using UnityEngine.SceneManagement;
using UtilityScripts;

public class MapGenerator : BaseMonoBehaviour
{
	public static MapGenerator Instance;

	private AreaGeneration _areaGeneration;

	private SupportingFactionGeneration _supportingFactionGeneration;

	private WorldMapRegionGeneration _worldMapRegionGeneration;

	private FamilyTreeGeneration _familyTreeGeneration;

	private RegionInnerMapGeneration _regionInnerMapGeneration;

	private TileFeatureGeneration _tileFeatureGeneration;

	private VillageGeneration _villageGeneration;

	private SpecialStructureGeneration _specialStructureGeneration;

	private FactionFinalization _factionFinalization;

	private CharacterFinalization _characterFinalization;

	private FeaturesActivation _featuresActivation;

	private MonsterGeneration _monsterGeneration;

	private MapGenerationFinalization _mapGenerationFinalization;

	private LoadMainThreadReferences _loadMainThreadReferences;

	private LoadSecondWaveTileObjectsCoroutine _loadSecondWaveTileObjectsCoroutine;

	private LoadSecondWaveTraitsInMainThread _loadSecondWaveTraitsInMainThread;

	private LoadCharactersCurrentAction _loadCharactersCurrentAction;

	private LoadAdditionalPlayerRelatedSaveData _loadAdditionalPlayerRelatedSaveData;

	private ConvertSaveFileLanguage _convertSaveFileLanguage;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			_areaGeneration = new AreaGeneration();
			_supportingFactionGeneration = new SupportingFactionGeneration();
			_worldMapRegionGeneration = new WorldMapRegionGeneration();
			_familyTreeGeneration = new FamilyTreeGeneration();
			_regionInnerMapGeneration = new RegionInnerMapGeneration();
			_tileFeatureGeneration = new TileFeatureGeneration();
			_villageGeneration = new VillageGeneration();
			_specialStructureGeneration = new SpecialStructureGeneration();
			_factionFinalization = new FactionFinalization();
			_characterFinalization = new CharacterFinalization();
			_featuresActivation = new FeaturesActivation();
			_monsterGeneration = new MonsterGeneration();
			_mapGenerationFinalization = new MapGenerationFinalization();
			_loadMainThreadReferences = new LoadMainThreadReferences();
			_loadSecondWaveTileObjectsCoroutine = new LoadSecondWaveTileObjectsCoroutine();
			_loadSecondWaveTraitsInMainThread = new LoadSecondWaveTraitsInMainThread();
			_loadCharactersCurrentAction = new LoadCharactersCurrentAction();
			_loadAdditionalPlayerRelatedSaveData = new LoadAdditionalPlayerRelatedSaveData();
			_convertSaveFileLanguage = new ConvertSaveFileLanguage();
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	internal IEnumerator InitializeWorld()
	{
		SaveManager.Instance.SetUseSaveData(state: false);
		DatabaseManager.Instance.mainSQLDatabase.InitializeDatabase();
		MapGenerationComponent[] components = new MapGenerationComponent[13]
		{
			_areaGeneration, _supportingFactionGeneration, _worldMapRegionGeneration, _familyTreeGeneration, _regionInnerMapGeneration, _tileFeatureGeneration, _villageGeneration, _specialStructureGeneration, _factionFinalization, _characterFinalization,
			_featuresActivation, _monsterGeneration, _mapGenerationFinalization
		};
		yield return StartCoroutine(InitializeWorldCoroutine(components));
	}

	private IEnumerator InitializeWorldCoroutine(MapGenerationComponent[] components)
	{
		Stopwatch loadingWatch = new Stopwatch();
		loadingWatch.Start();
		string loadingDetails = "Loading details";
		bool flag = false;
		MapGenerationData data = new MapGenerationData();
		WorldConfigManager.Instance.mapGenerationData = data;
		Stopwatch componentWatch = new Stopwatch();
		float progressPerComponent = 1f / (float)components.Length;
		float currentProgress = 0f;
		foreach (MapGenerationComponent currComponent in components)
		{
			componentWatch.Start();
			currentProgress += progressPerComponent;
			LevelLoaderManager.Instance.UpdateLoadingBar(currentProgress, 2f);
			yield return StartCoroutine(currComponent.ExecuteRandomGeneration(data));
			componentWatch.Stop();
			loadingDetails = loadingDetails + "\n" + currComponent.ToString() + " took " + componentWatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.";
			if (!string.IsNullOrEmpty(currComponent.log))
			{
				loadingDetails = loadingDetails + "\n" + currComponent.log;
			}
			componentWatch.Reset();
			flag = !currComponent.succeess;
			if (flag)
			{
				break;
			}
		}
		componentWatch.Stop();
		if (flag)
		{
			WorldConfigManager.Instance.mapGenerationData = null;
			UnityEngine.Debug.LogWarning("A component in world generation failed! Reloading scene...");
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
			yield break;
		}
		LevelLoaderManager.Instance.UpdateLoadingBar(1f, 0.5f);
		yield return GameUtilities.waitForHalfSecond;
		loadingWatch.Stop();
		data.SetFinishedMapGenerationCoroutine(p_state: true);
		UIManager.Instance.initialWorldSetupMenu.Initialize();
		LevelLoaderManager.Instance.SetLoadingState(state: false);
		AudioManager.Instance.UpdateAmbientSoundStateBasedOnCurrentScene();
		Messenger.Broadcast(Signals.GAME_LOADED);
		UIManager.Instance.initialWorldSetupMenu.Show();
		yield return GameUtilities.waitFor1Second;
	}

	public IEnumerator InitializeSavedWorld(SaveDataCurrentProgress saveData)
	{
		LevelLoaderManager.Instance.UpdateLoadingInfo("Loading_Initial_Data");
		SaveManager.Instance.SetUseSaveData(state: true);
		WorldSettings.Instance.SetWorldSettingsData(saveData.worldSettingsData);
		DatabaseManager.Instance.InitializeDatabases();
		WorldSettings.Instance.worldSettingsData.SetWorldType(saveData.worldMapSave.worldType);
		MapGenerationData mapData = new MapGenerationData
		{
			chosenWorldMapTemplate = saveData.worldMapSave.worldMapTemplate
		};
		GridMap.Instance.SetupInitialData(mapData.width, mapData.height);
		float num = 2.56f * ((float)mapData.width / 2f);
		float num2 = 1.93f * ((float)mapData.height / 2f);
		GridMap.Instance.transform.localPosition = new Vector2(0f - num, 0f - num2);
		WorldConfigManager.Instance.mapGenerationData = mapData;
		saveData.LoadDate();
		Region region = new Region(saveData.worldMapSave.regionSave);
		DatabaseManager.Instance.regionDatabase.RegisterRegion(region);
		region.CreateStructureList();
		List<MapGenerationComponent> threadedMapGenerationComponents = new List<MapGenerationComponent>();
		List<LoadThreadQueueItem> threadItems = new List<LoadThreadQueueItem>();
		threadedMapGenerationComponents.Add(new LoadInitialAreaData());
		threadedMapGenerationComponents.Add(new LoadInitialStructureData());
		threadedMapGenerationComponents.Add(new SettlementLoading());
		threadedMapGenerationComponents.Add(_familyTreeGeneration);
		threadedMapGenerationComponents.Add(new SingletonDataGeneration());
		threadedMapGenerationComponents.Add(new PlayerDataGeneration());
		for (int i = 0; i < threadedMapGenerationComponents.Count; i++)
		{
			LoadThreadQueueItem loadThreadQueueItem = new LoadThreadQueueItem();
			loadThreadQueueItem.mapData = mapData;
			loadThreadQueueItem.saveData = saveData;
			threadItems.Add(loadThreadQueueItem);
			ThreadPool.QueueUserWorkItem(threadedMapGenerationComponents[i].LoadSavedData, loadThreadQueueItem);
		}
		while (!AreAllThreadItemsDone(threadItems))
		{
			yield return null;
		}
		LevelLoaderManager.Instance.UpdateLoadingInfo("Loading_First_Wave");
		threadedMapGenerationComponents.Clear();
		threadItems.Clear();
		threadedMapGenerationComponents.Add(new LoadSecondaryStructureData());
		threadedMapGenerationComponents.Add(new LoadRegionAreas());
		threadedMapGenerationComponents.Add(new LoadAreaNeighbours());
		threadedMapGenerationComponents.Add(new LoadFirstWaveFactions());
		threadedMapGenerationComponents.Add(new LoadFirstWaveCharacters());
		threadedMapGenerationComponents.Add(new LoadFirstWaveTileObjects());
		threadedMapGenerationComponents.Add(new LoadFirstWaveActions());
		threadedMapGenerationComponents.Add(new LoadFirstWaveInterrupts());
		threadedMapGenerationComponents.Add(new LoadFirstWaveParties());
		threadedMapGenerationComponents.Add(new LoadFirstWavePartyQuests());
		threadedMapGenerationComponents.Add(new LoadFirstWaveCrimes());
		threadedMapGenerationComponents.Add(new LoadFirstWaveTraits());
		threadedMapGenerationComponents.Add(new LoadFirstWaveJobs());
		threadedMapGenerationComponents.Add(new LoadFirstWaveGatherings());
		threadedMapGenerationComponents.Add(new LoadFirstWaveSharedOpinions());
		for (int j = 0; j < threadedMapGenerationComponents.Count; j++)
		{
			LoadThreadQueueItem loadThreadQueueItem2 = new LoadThreadQueueItem();
			loadThreadQueueItem2.mapData = mapData;
			loadThreadQueueItem2.saveData = saveData;
			threadItems.Add(loadThreadQueueItem2);
			ThreadPool.QueueUserWorkItem(threadedMapGenerationComponents[j].LoadSavedData, loadThreadQueueItem2);
		}
		while (!AreAllThreadItemsDone(threadItems))
		{
			yield return null;
		}
		LevelLoaderManager.Instance.UpdateLoadingInfo("Loading_Map");
		RegionInnerMapGeneration regionInnerMapGeneration = _regionInnerMapGeneration;
		yield return StartCoroutine(regionInnerMapGeneration.LoadSavedData(mapData, saveData));
		LevelLoaderManager.Instance.UpdateLoadingInfo("Loading_Second_Wave");
		threadedMapGenerationComponents.Clear();
		threadItems.Clear();
		threadedMapGenerationComponents.Add(new LoadSecondWaveRegion());
		threadedMapGenerationComponents.Add(new LoadSecondWaveStructures());
		threadedMapGenerationComponents.Add(new LoadSecondWaveFactions());
		threadedMapGenerationComponents.Add(new LoadSecondWaveSettlements());
		threadedMapGenerationComponents.Add(new LoadSecondWaveStructureWalls());
		threadedMapGenerationComponents.Add(new LoadSecondWaveInterrupts());
		threadedMapGenerationComponents.Add(new LoadSecondWaveParties());
		threadedMapGenerationComponents.Add(new LoadSecondWavePartyQuests());
		threadedMapGenerationComponents.Add(new LoadSecondWaveCrimes());
		threadedMapGenerationComponents.Add(new LoadSecondWaveGatherings());
		threadedMapGenerationComponents.Add(new LoadSecondWaveSharedOpinions());
		for (int k = 0; k < threadedMapGenerationComponents.Count; k++)
		{
			LoadThreadQueueItem loadThreadQueueItem3 = new LoadThreadQueueItem();
			loadThreadQueueItem3.mapData = mapData;
			loadThreadQueueItem3.saveData = saveData;
			threadItems.Add(loadThreadQueueItem3);
			ThreadPool.QueueUserWorkItem(threadedMapGenerationComponents[k].LoadSavedData, loadThreadQueueItem3);
		}
		while (!AreAllThreadItemsDone(threadItems))
		{
			yield return null;
		}
		threadedMapGenerationComponents.Clear();
		threadItems.Clear();
		threadedMapGenerationComponents.Add(new LoadSecondWaveActions());
		threadedMapGenerationComponents.Add(new LoadSecondWaveCharacters());
		threadedMapGenerationComponents.Add(new LoadSecondWaveStructuresAdditional());
		threadedMapGenerationComponents.Add(new LoadSecondWaveTraits());
		for (int l = 0; l < threadedMapGenerationComponents.Count; l++)
		{
			LoadThreadQueueItem loadThreadQueueItem4 = new LoadThreadQueueItem();
			loadThreadQueueItem4.mapData = mapData;
			loadThreadQueueItem4.saveData = saveData;
			threadItems.Add(loadThreadQueueItem4);
			ThreadPool.QueueUserWorkItem(threadedMapGenerationComponents[l].LoadSavedData, loadThreadQueueItem4);
		}
		while (!AreAllThreadItemsDone(threadItems))
		{
			yield return null;
		}
		threadedMapGenerationComponents.Clear();
		threadItems.Clear();
		threadedMapGenerationComponents.Add(new LoadSecondWaveJobs());
		threadedMapGenerationComponents.Add(new LoadSecondWavePlayer());
		for (int m = 0; m < threadedMapGenerationComponents.Count; m++)
		{
			LoadThreadQueueItem loadThreadQueueItem5 = new LoadThreadQueueItem();
			loadThreadQueueItem5.mapData = mapData;
			loadThreadQueueItem5.saveData = saveData;
			threadItems.Add(loadThreadQueueItem5);
			ThreadPool.QueueUserWorkItem(threadedMapGenerationComponents[m].LoadSavedData, loadThreadQueueItem5);
		}
		while (!AreAllThreadItemsDone(threadItems))
		{
			yield return null;
		}
		LevelLoaderManager.Instance.UpdateLoadingInfo("Loading_Area_Spells");
		MapGenerationComponent[] components = new MapGenerationComponent[8] { _loadMainThreadReferences, _loadSecondWaveTileObjectsCoroutine, _loadSecondWaveTraitsInMainThread, _tileFeatureGeneration, _mapGenerationFinalization, _loadCharactersCurrentAction, _loadAdditionalPlayerRelatedSaveData, _convertSaveFileLanguage };
		yield return StartCoroutine(InitializeSavedWorldCoroutine(components, saveData, mapData));
	}

	private bool AreAllThreadItemsDone(List<LoadThreadQueueItem> threadItems)
	{
		for (int i = 0; i < threadItems.Count; i++)
		{
			if (!threadItems[i].isDone)
			{
				return false;
			}
		}
		return true;
	}

	private IEnumerator InitializeSavedWorldCoroutine(MapGenerationComponent[] components, SaveDataCurrentProgress saveData, MapGenerationData mapData)
	{
		Stopwatch loadingWatch = new Stopwatch();
		loadingWatch.Start();
		string loadingDetails = "Loading details";
		bool flag = false;
		Stopwatch componentWatch = new Stopwatch();
		float progressPerComponent = 0.6f / (float)components.Length;
		float currentProgress = 0.4f;
		foreach (MapGenerationComponent currComponent in components)
		{
			componentWatch.Start();
			currentProgress += progressPerComponent;
			LevelLoaderManager.Instance.UpdateLoadingBar(currentProgress, 2f);
			yield return StartCoroutine(currComponent.LoadSavedData(mapData, saveData));
			componentWatch.Stop();
			loadingDetails = loadingDetails + "\n" + currComponent.ToString() + " took " + componentWatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.";
			if (!string.IsNullOrEmpty(currComponent.log))
			{
				loadingDetails = loadingDetails + "\n" + currComponent.log;
			}
			componentWatch.Reset();
			flag = !currComponent.succeess;
			if (flag)
			{
				break;
			}
		}
		componentWatch.Stop();
		if (flag)
		{
			WorldConfigManager.Instance.mapGenerationData = null;
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
			yield break;
		}
		LevelLoaderManager.Instance.UpdateLoadingBar(1f, 0.5f);
		yield return GameUtilities.waitForTenthOfSecond;
		loadingWatch.Stop();
		saveData.LoadPortraitAvailability();
		UIManager.Instance.initialWorldSetupMenu.Initialize();
		Messenger.Broadcast(Signals.GAME_LOADED);
		UIManager.Instance.initialWorldSetupMenu.loadOutMenu.LoadLoadout(saveData.playerSave.archetype);
		DatabaseManager.Instance.ClearVolatileDatabases();
		SaveManager.Instance.saveCurrentProgressManager.CleanUpLoadedData();
		GC.Collect();
		AsyncOperation unloader = Resources.UnloadUnusedAssets();
		while (!unloader.isDone)
		{
			yield return null;
		}
		LevelLoaderManager.Instance.SetLoadingState(state: false);
		AudioManager.Instance.UpdateAmbientSoundStateBasedOnCurrentScene();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Instance = null;
	}
}

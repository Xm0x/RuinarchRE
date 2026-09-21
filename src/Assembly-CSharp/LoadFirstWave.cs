using System;
using System.Collections;
using UnityEngine;

public class LoadFirstWave : MapGenerationComponent
{
	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data)
	{
		yield return null;
	}

	public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData)
	{
		yield return MapGenerator.Instance.StartCoroutine(Load(saveData));
	}

	public override void LoadSavedData(object state)
	{
		base.LoadSavedData(state);
	}

	private IEnumerator Load(SaveDataCurrentProgress saveData)
	{
		yield return MapGenerator.Instance.StartCoroutine(LoadFactions(saveData));
		yield return MapGenerator.Instance.StartCoroutine(LoadJobs(saveData));
		yield return MapGenerator.Instance.StartCoroutine(LoadCharacters(saveData));
		yield return MapGenerator.Instance.StartCoroutine(LoadTileObjects(saveData));
		yield return MapGenerator.Instance.StartCoroutine(LoadActions(saveData));
		yield return MapGenerator.Instance.StartCoroutine(LoadInterrupts(saveData));
		yield return MapGenerator.Instance.StartCoroutine(LoadParties(saveData));
		yield return MapGenerator.Instance.StartCoroutine(LoadPartyQuests(saveData));
		yield return MapGenerator.Instance.StartCoroutine(LoadCrimes(saveData));
		yield return MapGenerator.Instance.StartCoroutine(LoadGatherings(saveData));
		yield return MapGenerator.Instance.StartCoroutine(LoadTraits(saveData));
	}

	private IEnumerator LoadFactions(SaveDataCurrentProgress saveData)
	{
		LevelLoaderManager.Instance.UpdateLoadingInfo("Loading_Factions");
		saveData.LoadFactions();
		yield return null;
	}

	private IEnumerator LoadCharacters(SaveDataCurrentProgress saveData)
	{
		LevelLoaderManager.Instance.UpdateLoadingInfo("Loading_Characters");
		saveData.LoadCharacters();
		yield return null;
	}

	private IEnumerator LoadTileObjects(SaveDataCurrentProgress saveData)
	{
		LevelLoaderManager.Instance.UpdateLoadingInfo("Loading_Objects");
		saveData.LoadTileObjects();
		yield return null;
	}

	private IEnumerator LoadActions(SaveDataCurrentProgress saveData)
	{
		LevelLoaderManager.Instance.UpdateLoadingInfo("Loading_Actions");
		saveData.LoadActions();
		yield return null;
	}

	private IEnumerator LoadInterrupts(SaveDataCurrentProgress saveData)
	{
		LevelLoaderManager.Instance.UpdateLoadingInfo("Loading_Actions");
		saveData.LoadInterrupts();
		yield return null;
	}

	private IEnumerator LoadParties(SaveDataCurrentProgress saveData)
	{
		LevelLoaderManager.Instance.UpdateLoadingInfo("Loading_Parties");
		saveData.LoadParties();
		yield return null;
	}

	private IEnumerator LoadPartyQuests(SaveDataCurrentProgress saveData)
	{
		LevelLoaderManager.Instance.UpdateLoadingInfo("Loading_Party_Quests");
		saveData.LoadPartyQuests();
		yield return null;
	}

	private IEnumerator LoadCrimes(SaveDataCurrentProgress saveData)
	{
		LevelLoaderManager.Instance.UpdateLoadingInfo("Loading_Crimes");
		saveData.LoadCrimes();
		yield return null;
	}

	private IEnumerator LoadTraits(SaveDataCurrentProgress saveData)
	{
		LevelLoaderManager.Instance.UpdateLoadingInfo("Loading_Traits");
		saveData.LoadTraits();
		yield return null;
	}

	private IEnumerator LoadJobs(SaveDataCurrentProgress saveData)
	{
		LevelLoaderManager.Instance.UpdateLoadingInfo("Loading_Jobs");
		saveData.LoadJobs();
		yield return null;
	}

	private IEnumerator LoadGatherings(SaveDataCurrentProgress saveData)
	{
		LevelLoaderManager.Instance.UpdateLoadingInfo("Loading_Gatherings");
		saveData.LoadGatherings();
		yield return null;
	}

	private void LoadFactions(object state)
	{
		try
		{
			LoadThreadQueueItem obj = state as LoadThreadQueueItem;
			_ = obj.mapData;
			obj.saveData.LoadFactions();
			obj.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}

	private void LoadCharacters(object state)
	{
		try
		{
			LoadThreadQueueItem obj = state as LoadThreadQueueItem;
			_ = obj.mapData;
			obj.saveData.LoadCharacters();
			obj.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}

	private void LoadTileObjects(object state)
	{
		try
		{
			LoadThreadQueueItem obj = state as LoadThreadQueueItem;
			_ = obj.mapData;
			obj.saveData.LoadTileObjects();
			obj.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}

	private void LoadActions(object state)
	{
		try
		{
			LoadThreadQueueItem obj = state as LoadThreadQueueItem;
			_ = obj.mapData;
			obj.saveData.LoadActions();
			obj.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}

	private void LoadInterrupts(object state)
	{
		try
		{
			LoadThreadQueueItem obj = state as LoadThreadQueueItem;
			_ = obj.mapData;
			obj.saveData.LoadInterrupts();
			obj.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}

	private void LoadParties(object state)
	{
		try
		{
			LoadThreadQueueItem obj = state as LoadThreadQueueItem;
			_ = obj.mapData;
			obj.saveData.LoadParties();
			obj.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}

	private void LoadPartyQuests(object state)
	{
		try
		{
			LoadThreadQueueItem obj = state as LoadThreadQueueItem;
			_ = obj.mapData;
			obj.saveData.LoadPartyQuests();
			obj.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}

	private void LoadCrimes(object state)
	{
		try
		{
			LoadThreadQueueItem obj = state as LoadThreadQueueItem;
			_ = obj.mapData;
			obj.saveData.LoadCrimes();
			obj.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}

	private void LoadTraits(object state)
	{
		try
		{
			LoadThreadQueueItem obj = state as LoadThreadQueueItem;
			_ = obj.mapData;
			obj.saveData.LoadTraits();
			obj.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}

	private void LoadJobs(object state)
	{
		try
		{
			LoadThreadQueueItem obj = state as LoadThreadQueueItem;
			_ = obj.mapData;
			obj.saveData.LoadJobs();
			obj.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}

	private void LoadGatherings(object state)
	{
		try
		{
			LoadThreadQueueItem obj = state as LoadThreadQueueItem;
			_ = obj.mapData;
			obj.saveData.LoadGatherings();
			obj.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}
}

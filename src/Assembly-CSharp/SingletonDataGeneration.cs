using System;
using System.Collections;
using UnityEngine;

public class SingletonDataGeneration : MapGenerationComponent
{
	public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData)
	{
		yield return MapGenerator.Instance.StartCoroutine(LoadSaveDataPlagueDisease(data, saveData));
	}

	public override void LoadSavedData(object state)
	{
		try
		{
			LoadThreadQueueItem obj = state as LoadThreadQueueItem;
			_ = obj.mapData;
			obj.saveData.LoadPlagueDisease();
			obj.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}

	private IEnumerator LoadSaveDataPlagueDisease(MapGenerationData data, SaveDataCurrentProgress saveData)
	{
		LevelLoaderManager.Instance.UpdateLoadingInfo("Loading_Data");
		saveData.LoadPlagueDisease();
		yield return null;
	}
}

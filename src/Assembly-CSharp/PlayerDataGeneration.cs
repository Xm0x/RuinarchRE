using System;
using System.Collections;
using UnityEngine;

public class PlayerDataGeneration : MapGenerationComponent
{
	public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData)
	{
		yield return MapGenerator.Instance.StartCoroutine(LoadSaveDataPlayerGame(data, saveData));
	}

	private IEnumerator LoadSaveDataPlayerGame(MapGenerationData data, SaveDataCurrentProgress saveData)
	{
		LevelLoaderManager.Instance.UpdateLoadingInfo("Loading_Player_Data");
		PlayerManager.Instance.InitializePlayer(saveData);
		yield return null;
	}

	public override void LoadSavedData(object state)
	{
		try
		{
			LoadThreadQueueItem obj = state as LoadThreadQueueItem;
			_ = obj.mapData;
			SaveDataCurrentProgress saveData = obj.saveData;
			PlayerManager.Instance.InitializePlayer(saveData);
			obj.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}
}

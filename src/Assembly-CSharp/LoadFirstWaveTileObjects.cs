using System;
using UnityEngine;

public class LoadFirstWaveTileObjects : MapGenerationComponent
{
	public override void LoadSavedData(object state)
	{
		LoadTileObjects(state);
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
}

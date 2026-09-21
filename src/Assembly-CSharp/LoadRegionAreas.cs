using System;
using UnityEngine;

public class LoadRegionAreas : MapGenerationComponent
{
	public override void LoadSavedData(object state)
	{
		try
		{
			LoadThreadQueueItem loadThreadQueueItem = state as LoadThreadQueueItem;
			_ = loadThreadQueueItem.mapData;
			SaveDataCurrentProgress saveData = loadThreadQueueItem.saveData;
			Region mainRegion = DatabaseManager.Instance.regionDatabase.mainRegion;
			for (int i = 0; i < saveData.worldMapSave.worldMapTemplate.worldMapWidth; i++)
			{
				for (int j = 0; j < saveData.worldMapSave.worldMapTemplate.worldMapHeight; j++)
				{
					Area tile = GridMap.Instance.map[i, j];
					mainRegion.AddTile(tile);
				}
			}
			loadThreadQueueItem.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}
}

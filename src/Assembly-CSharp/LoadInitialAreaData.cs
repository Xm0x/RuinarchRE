using System;
using UnityEngine;

public class LoadInitialAreaData : MapGenerationComponent
{
	public override void LoadSavedData(object state)
	{
		try
		{
			LoadThreadQueueItem loadThreadQueueItem = state as LoadThreadQueueItem;
			MapGenerationData mapData = loadThreadQueueItem.mapData;
			SaveDataArea[,] saveDataMap = loadThreadQueueItem.saveData.worldMapSave.GetSaveDataMap();
			Area[,] array = new Area[mapData.width, mapData.height];
			for (int i = 0; i < mapData.width; i++)
			{
				for (int j = 0; j < mapData.height; j++)
				{
					Area p_area = (array[i, j] = saveDataMap[i, j].Load());
					DatabaseManager.Instance.areaDatabase.RegisterArea(p_area);
				}
			}
			GridMap.Instance.SetMap(array);
			loadThreadQueueItem.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}
}

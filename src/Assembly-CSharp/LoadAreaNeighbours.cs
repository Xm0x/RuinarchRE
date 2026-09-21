using System;
using UnityEngine;

public class LoadAreaNeighbours : MapGenerationComponent
{
	public override void LoadSavedData(object state)
	{
		try
		{
			LoadThreadQueueItem loadThreadQueueItem = state as LoadThreadQueueItem;
			_ = loadThreadQueueItem.mapData;
			_ = loadThreadQueueItem.saveData;
			for (int i = 0; i < DatabaseManager.Instance.areaDatabase.allAreas.Count; i++)
			{
				Area area = DatabaseManager.Instance.areaDatabase.allAreas[i];
				area.neighbourComponent.FindNeighbours(area, GridMap.Instance.map);
			}
			loadThreadQueueItem.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}
}

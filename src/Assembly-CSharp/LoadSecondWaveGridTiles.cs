using System;
using System.Linq;
using UnityEngine;

public class LoadSecondWaveGridTiles : MapGenerationComponent
{
	public override void LoadSavedData(object state)
	{
		base.LoadSavedData(state);
		Load(state);
	}

	private void Load(object state)
	{
		try
		{
			LoadThreadQueueItem loadThreadQueueItem = state as LoadThreadQueueItem;
			_ = loadThreadQueueItem.mapData;
			SaveDataRegion regionSave = loadThreadQueueItem.saveData.worldMapSave.regionSave;
			for (int i = 0; i < regionSave.innerMapSave.tileSaves.Values.Count; i++)
			{
				SaveDataLocationGridTile saveDataLocationGridTile = regionSave.innerMapSave.tileSaves.Values.ElementAt(i);
				DatabaseManager.Instance.locationGridTileDatabase.GetTileByPersistentID(saveDataLocationGridTile.persistentID).LoadSecondWave(saveDataLocationGridTile);
			}
			loadThreadQueueItem.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}
}

using System;
using UnityEngine;

public class LoadSecondWaveRegion : MapGenerationComponent
{
	public override void LoadSavedData(object state)
	{
		base.LoadSavedData(state);
		LoadRegion(state);
	}

	private void LoadRegion(object state)
	{
		try
		{
			LoadThreadQueueItem obj = state as LoadThreadQueueItem;
			_ = obj.mapData;
			SaveDataCurrentProgress saveData = obj.saveData;
			DatabaseManager.Instance.regionDatabase.mainRegion.LoadReferences(saveData.worldMapSave.regionSave);
			obj.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}
}

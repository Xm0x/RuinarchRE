using System;
using UnityEngine;

public class LoadFirstWaveActions : MapGenerationComponent
{
	public override void LoadSavedData(object state)
	{
		LoadActions(state);
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
}

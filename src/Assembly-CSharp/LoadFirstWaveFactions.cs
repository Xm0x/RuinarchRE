using System;
using UnityEngine;

public class LoadFirstWaveFactions : MapGenerationComponent
{
	public override void LoadSavedData(object state)
	{
		LoadFactions(state);
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
}

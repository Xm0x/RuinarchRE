using System;
using UnityEngine;

public class LoadFirstWaveTraits : MapGenerationComponent
{
	public override void LoadSavedData(object state)
	{
		LoadTraits(state);
	}

	private void LoadTraits(object state)
	{
		try
		{
			LoadThreadQueueItem obj = state as LoadThreadQueueItem;
			_ = obj.mapData;
			obj.saveData.LoadTraits();
			obj.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}
}

using System;
using UnityEngine;

public class LoadFirstWaveParties : MapGenerationComponent
{
	public override void LoadSavedData(object state)
	{
		LoadParties(state);
	}

	private void LoadParties(object state)
	{
		try
		{
			LoadThreadQueueItem obj = state as LoadThreadQueueItem;
			_ = obj.mapData;
			obj.saveData.LoadParties();
			obj.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}
}

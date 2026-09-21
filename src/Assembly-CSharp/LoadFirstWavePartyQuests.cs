using System;
using UnityEngine;

public class LoadFirstWavePartyQuests : MapGenerationComponent
{
	public override void LoadSavedData(object state)
	{
		LoadPartyQuests(state);
	}

	private void LoadPartyQuests(object state)
	{
		try
		{
			LoadThreadQueueItem obj = state as LoadThreadQueueItem;
			_ = obj.mapData;
			obj.saveData.LoadPartyQuests();
			obj.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}
}

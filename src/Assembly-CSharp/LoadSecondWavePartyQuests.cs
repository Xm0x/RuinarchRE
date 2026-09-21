using System;
using UnityEngine;

public class LoadSecondWavePartyQuests : MapGenerationComponent
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
			LoadThreadQueueItem obj = state as LoadThreadQueueItem;
			_ = obj.mapData;
			obj.saveData.LoadPartyQuestReferences();
			obj.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}
}

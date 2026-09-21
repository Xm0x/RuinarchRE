using System;
using UnityEngine;

public class LoadSecondWaveSettlements : MapGenerationComponent
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
			SaveDataCurrentProgress saveData = loadThreadQueueItem.saveData;
			for (int i = 0; i < saveData.worldMapSave.settlementSaves.Count; i++)
			{
				SaveDataBaseSettlement saveDataBaseSettlement = saveData.worldMapSave.settlementSaves[i];
				DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(saveDataBaseSettlement._persistentID).LoadReferences(saveDataBaseSettlement);
			}
			loadThreadQueueItem.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}
}

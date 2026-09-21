using System;
using Inner_Maps.Location_Structures;
using UnityEngine;

public class LoadSecondaryStructureData : MapGenerationComponent
{
	public override void LoadSavedData(object state)
	{
		try
		{
			LoadThreadQueueItem loadThreadQueueItem = state as LoadThreadQueueItem;
			_ = loadThreadQueueItem.mapData;
			SaveDataCurrentProgress saveData = loadThreadQueueItem.saveData;
			_ = DatabaseManager.Instance.regionDatabase.mainRegion;
			for (int i = 0; i < saveData.worldMapSave.structureSaves.Count; i++)
			{
				SaveDataLocationStructure saveDataLocationStructure = saveData.worldMapSave.structureSaves[i];
				LocationStructure structureByPersistentIDSafe = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentIDSafe(saveDataLocationStructure.persistentID);
				if (structureByPersistentIDSafe != null && !structureByPersistentIDSafe.hasBeenDestroyed && !string.IsNullOrEmpty(saveDataLocationStructure.settlementLocationID))
				{
					DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(saveDataLocationStructure.settlementLocationID).AddStructure(structureByPersistentIDSafe);
				}
			}
			loadThreadQueueItem.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}
}

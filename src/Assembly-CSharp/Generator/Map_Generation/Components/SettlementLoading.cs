using System;
using System.Collections;
using Locations.Settlements;
using UnityEngine;

namespace Generator.Map_Generation.Components;

public class SettlementLoading : MapGenerationComponent
{
	public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData)
	{
		LevelLoaderManager.Instance.UpdateLoadingInfo("Loading_Settlements");
		AddLog("Loading settlements");
		for (int i = 0; i < saveData.worldMapSave.settlementSaves.Count; i++)
		{
			BaseSettlement baseSettlement = saveData.worldMapSave.settlementSaves[i].Load();
			AddLog($" - Loaded settlement {baseSettlement.name} with {baseSettlement.areas.Count} tiles.");
			yield return null;
		}
	}

	public override void LoadSavedData(object state)
	{
		try
		{
			LoadThreadQueueItem loadThreadQueueItem = state as LoadThreadQueueItem;
			_ = loadThreadQueueItem.mapData;
			SaveDataCurrentProgress saveData = loadThreadQueueItem.saveData;
			AddLog("Loading settlements");
			for (int i = 0; i < saveData.worldMapSave.settlementSaves.Count; i++)
			{
				BaseSettlement baseSettlement = saveData.worldMapSave.settlementSaves[i].Load();
				AddLog($" - Loaded settlement {baseSettlement.name} with {baseSettlement.areas.Count} tiles.");
			}
			loadThreadQueueItem.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}
}

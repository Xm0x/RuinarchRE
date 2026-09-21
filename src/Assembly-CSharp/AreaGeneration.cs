using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AreaGeneration : MapGenerationComponent
{
	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data)
	{
		LevelLoaderManager.Instance.UpdateLoadingInfo("Generating_World_Map");
		WorldMapTemplate chosenWorldMapTemplate = default(WorldMapTemplate);
		Vector2 mapSize = WorldSettings.Instance.worldSettingsData.mapSettings.GetMapSize();
		chosenWorldMapTemplate.worldMapWidth = (int)mapSize.x;
		chosenWorldMapTemplate.worldMapHeight = (int)mapSize.y;
		data.chosenWorldMapTemplate = chosenWorldMapTemplate;
		yield return MapGenerator.Instance.StartCoroutine(GenerateGrid(data));
	}

	private IEnumerator GenerateGrid(MapGenerationData data)
	{
		GridMap.Instance.SetupInitialData(data.width, data.height);
		float num = 2.56f * ((float)data.width / 2f);
		float num2 = 1.93f * ((float)data.height / 2f);
		GridMap.Instance.transform.localPosition = new Vector2(0f - num, 0f - num2);
		Area[,] map = new Area[data.width, data.height];
		List<Area> areas = new List<Area>();
		int id = 0;
		int batchCount = 0;
		for (int x = 0; x < data.width; x++)
		{
			for (int y = 0; y < data.height; y++)
			{
				Area area = new Area(id, x, y);
				areas.Add(area);
				map[x, y] = area;
				id++;
				batchCount++;
				if (batchCount == MapGenerationData.WorldMapTileGenerationBatches)
				{
					batchCount = 0;
					yield return null;
				}
			}
		}
		GridMap.Instance.SetMap(map, areas);
		Parallel.ForEach(areas, delegate(Area hexTile)
		{
			hexTile.neighbourComponent.FindNeighbours(hexTile, map);
		});
		yield return null;
	}

	public override void LoadSavedData(object state)
	{
		try
		{
			LoadThreadQueueItem loadThreadQueueItem = state as LoadThreadQueueItem;
			MapGenerationData mapData = loadThreadQueueItem.mapData;
			SaveDataCurrentProgress saveData = loadThreadQueueItem.saveData;
			GridMap.Instance.SetupInitialData(mapData.width, mapData.height);
			Area[,] array = new Area[mapData.width, mapData.height];
			List<Area> list = new List<Area>();
			SaveDataArea[,] saveDataMap = saveData.worldMapSave.GetSaveDataMap();
			for (int i = 0; i < mapData.width; i++)
			{
				for (int j = 0; j < mapData.height; j++)
				{
					Area area = saveDataMap[i, j].Load();
					list.Add(area);
					array[i, j] = area;
				}
			}
			GridMap.Instance.SetMap(array, list);
			for (int k = 0; k < list.Count; k++)
			{
				Area area2 = list[k];
				area2.neighbourComponent.FindNeighbours(area2, array);
			}
			loadThreadQueueItem.isDone = true;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace);
		}
	}

	public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData)
	{
		LevelLoaderManager.Instance.UpdateLoadingInfo("Loading_World_Map");
		data.chosenWorldMapTemplate = saveData.worldMapSave.worldMapTemplate;
		WorldSettings.Instance.worldSettingsData.SetWorldType(saveData.worldMapSave.worldType);
		saveData.LoadDate();
		yield return MapGenerator.Instance.StartCoroutine(GenerateGrid(data, saveData));
	}

	private IEnumerator GenerateGrid(MapGenerationData data, SaveDataCurrentProgress saveData)
	{
		GridMap.Instance.SetupInitialData(data.width, data.height);
		float num = 2.56f * ((float)data.width / 2f);
		float num2 = 1.93f * ((float)data.height / 2f);
		GridMap.Instance.transform.localPosition = new Vector2(0f - num, 0f - num2);
		Area[,] map = new Area[data.width, data.height];
		List<Area> normalHexTiles = new List<Area>();
		SaveDataArea[,] savedMap = saveData.worldMapSave.GetSaveDataMap();
		int batchCount = 0;
		for (int x = 0; x < data.width; x++)
		{
			for (int y = 0; y < data.height; y++)
			{
				Area area = savedMap[x, y].Load();
				normalHexTiles.Add(area);
				map[x, y] = area;
				batchCount++;
				if (batchCount == MapGenerationData.WorldMapTileGenerationBatches)
				{
					batchCount = 0;
					yield return null;
				}
			}
		}
		GridMap.Instance.SetMap(map, normalHexTiles);
		Parallel.ForEach(normalHexTiles, delegate(Area area2)
		{
			area2.neighbourComponent.FindNeighbours(area2, map);
		});
		yield return null;
	}
}

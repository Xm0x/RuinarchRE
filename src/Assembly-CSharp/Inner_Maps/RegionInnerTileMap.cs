using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Inner_Maps;

public class RegionInnerTileMap : InnerTileMap
{
	public IEnumerator GenerateMap(MapGenerationComponent mapGenerationComponent, MapGenerationData data)
	{
		base.name = base.region.name + "'s Inner Map";
		base.region.SetRegionInnerMap(this);
		ClearAllTileMaps();
		Stopwatch stopwatch = new Stopwatch();
		Vector2Int regionDimensions = GetRegionDimensions(base.region);
		Vector2Int innerMapSizeGivenRegionDimensions = GetInnerMapSizeGivenRegionDimensions(regionDimensions);
		yield return StartCoroutine(GenerateGrid(innerMapSizeGivenRegionDimensions.x, innerMapSizeGivenRegionDimensions.y, mapGenerationComponent, stopwatch));
		PopulateNeededAreaDataAfterGridGeneration(mapGenerationComponent, regionDimensions.x, regionDimensions.y, stopwatch);
		int xSize = width - 1;
		int ySize = height - 1;
		yield return StartCoroutine(GroundPerlin(base.allTiles, xSize, ySize, base.xSeed, base.ySeed, data));
		yield return StartCoroutine(GenerateElevationMap(mapGenerationComponent, data, stopwatch));
		StartCoroutine(GraduallyGenerateTileObjects(data));
	}

	public IEnumerator LoadMap(MapGenerationComponent mapGenerationComponent, SaveDataInnerMap saveDataInnerMap, SaveDataCurrentProgress saveData)
	{
		base.name = base.region.name + "'s Inner Map";
		base.region.SetRegionInnerMap(this);
		ClearAllTileMaps();
		Stopwatch stopwatch = new Stopwatch();
		Vector2Int regionDimensions = GetRegionDimensions(base.region);
		Vector2Int innerMapSizeGivenRegionDimensions = GetInnerMapSizeGivenRegionDimensions(regionDimensions);
		yield return StartCoroutine(LoadGrid(innerMapSizeGivenRegionDimensions.x, innerMapSizeGivenRegionDimensions.y, mapGenerationComponent, saveDataInnerMap, saveData));
		PopulateNeededAreaDataAfterGridGeneration(mapGenerationComponent, regionDimensions.x, regionDimensions.y, stopwatch);
		int num = base.allTiles.Min((LocationGridTile t) => t.localPlace.x);
		int num2 = base.allTiles.Max((LocationGridTile t) => t.localPlace.x);
		int num3 = base.allTiles.Min((LocationGridTile t) => t.localPlace.y);
		int num4 = base.allTiles.Max((LocationGridTile t) => t.localPlace.y);
		int xSize = num2 - num;
		int ySize = num4 - num3;
		yield return StartCoroutine(GroundPerlin(base.allTiles, xSize, ySize, saveDataInnerMap.xSeed, saveDataInnerMap.ySeed, null));
	}

	private void PopulateNeededAreaDataAfterGridGeneration(MapGenerationComponent mapGenerationComponent, int gridWidth, int gridHeight, Stopwatch stopwatch)
	{
		stopwatch.Reset();
		stopwatch.Start();
		for (int i = 0; i < gridWidth; i++)
		{
			for (int j = 0; j < gridHeight; j++)
			{
				GameObject obj = Object.Instantiate(areaItemPrefab, elevationTilemap.transform);
				float x = (float)((i + 1) * InnerMapManager.AreaLocationGridTileSize.x) - (float)InnerMapManager.AreaLocationGridTileSize.x / 2f;
				float y = (float)((j + 1) * InnerMapManager.AreaLocationGridTileSize.y) - (float)InnerMapManager.AreaLocationGridTileSize.y / 2f;
				Area area = GridMap.Instance.map[i, j];
				AreaItem component = obj.GetComponent<AreaItem>();
				component.Initialize(this, i, j);
				obj.transform.localPosition = new Vector2(x, y);
				area.SetAreaItem(component);
				area.gridTileComponent.SetCenterGridTile(GetCenterLocationGridTile(area));
			}
		}
		stopwatch.Stop();
		mapGenerationComponent.AddLog(base.region.name + " CreateTileCollectionGrid took " + stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.");
	}

	private LocationGridTile GetCenterLocationGridTile(Area p_area)
	{
		int num = p_area.gridTileComponent.gridTiles.Min((LocationGridTile t) => t.localPlace.x);
		int num2 = p_area.gridTileComponent.gridTiles.Min((LocationGridTile t) => t.localPlace.y);
		int num3 = num + InnerMapManager.AreaLocationGridTileSize.x / 2;
		int num4 = num2 + InnerMapManager.AreaLocationGridTileSize.y / 2;
		return base.region.innerMap.map[num3, num4];
	}

	private Vector2Int GetRegionDimensions(Region p_region)
	{
		int num = p_region.areas.Max((Area t) => t.areaData.xCoordinate);
		int num2 = p_region.areas.Min((Area t) => t.areaData.xCoordinate);
		int num3 = num - num2;
		int num4 = p_region.areas.Max((Area t) => t.areaData.yCoordinate);
		int num5 = p_region.areas.Min((Area t) => t.areaData.yCoordinate);
		int num6 = num4 - num5;
		return new Vector2Int(num3 + 1, num6 + 1);
	}

	private Vector2Int GetInnerMapSizeGivenRegionDimensions(Vector2Int p_dimensions)
	{
		return new Vector2Int(p_dimensions.x * InnerMapManager.AreaLocationGridTileSize.x, p_dimensions.y * InnerMapManager.AreaLocationGridTileSize.y);
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Cellular_Automata;

public static class CellularAutomataGenerator
{
	public static int[,] GenerateMap(LocationGridTile[,] tileMap, List<LocationGridTile> allTiles, int smoothing, int randomFillPercent, string seed = "", bool edgesAreAlwaysWalls = true)
	{
		int num = tileMap.GetUpperBound(0) + 1;
		int num2 = tileMap.GetUpperBound(1) + 1;
		int[,] array = new int[num, num2];
		RandomFillMap(num, num2, array, randomFillPercent, tileMap, allTiles, seed, edgesAreAlwaysWalls);
		for (int i = 0; i < smoothing; i++)
		{
			array = SmoothMap(array, num, num2, tileMap, allTiles, edgesAreAlwaysWalls);
		}
		return array;
	}

	private static void RandomFillMap(int width, int height, int[,] map, int randomFillPercent, LocationGridTile[,] tileMap, List<LocationGridTile> allTiles, string seed, bool edgesAreAlwaysWalls = true)
	{
		if (string.IsNullOrEmpty(seed))
		{
			seed = Time.time.ToString();
		}
		System.Random random = new System.Random(seed.GetHashCode());
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				LocationGridTile locationGridTile = tileMap[i, j];
				if (edgesAreAlwaysWalls && (locationGridTile == null || IsAtEdgeOfMap(locationGridTile, allTiles)))
				{
					map[i, j] = 1;
				}
				else
				{
					map[i, j] = ((random.Next(0, 100) < randomFillPercent) ? 1 : 0);
				}
			}
		}
	}

	private static int[,] SmoothMap(int[,] map, int width, int height, LocationGridTile[,] tileMap, List<LocationGridTile> allTiles, bool edgesAreAlwaysWalls = true)
	{
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				LocationGridTile locationGridTile = tileMap[i, j];
				if (edgesAreAlwaysWalls && locationGridTile != null && IsAtEdgeOfMap(locationGridTile, allTiles))
				{
					map[i, j] = 1;
					continue;
				}
				int surroundingWallCount = GetSurroundingWallCount(map, i, j, width, height);
				if (surroundingWallCount > 4)
				{
					map[i, j] = 1;
				}
				else if (surroundingWallCount < 4)
				{
					map[i, j] = 0;
				}
			}
		}
		return map;
	}

	private static int GetSurroundingWallCount(int[,] map, int gridX, int gridY, int width, int height)
	{
		int num = 0;
		for (int i = gridX - 1; i <= gridX + 1; i++)
		{
			for (int j = gridY - 1; j <= gridY + 1; j++)
			{
				if (i >= 0 && i < width && j >= 0 && j < height)
				{
					if (i != gridX || j != gridY)
					{
						num += map[i, j];
					}
				}
				else
				{
					num++;
				}
			}
		}
		return num;
	}

	public static bool IsAtEdgeOfMap(LocationGridTile tile, List<LocationGridTile> allTiles)
	{
		if (!tile.HasNeighbourNotInList(allTiles))
		{
			return tile.IsAtEdgeOfMap();
		}
		return true;
	}

	public static LocationGridTile[,] ConvertListToGridMap(List<LocationGridTile> locationGridTiles)
	{
		int minX = locationGridTiles.Min((LocationGridTile t) => t.localPlace.x);
		int num = locationGridTiles.Max((LocationGridTile t) => t.localPlace.x);
		int minY = locationGridTiles.Min((LocationGridTile t) => t.localPlace.y);
		int num2 = locationGridTiles.Max((LocationGridTile t) => t.localPlace.y);
		int num3 = num - minX + 1;
		int num4 = num2 - minY + 1;
		LocationGridTile[,] arrangedMap = new LocationGridTile[num3, num4];
		Parallel.ForEach(locationGridTiles, delegate(LocationGridTile tile)
		{
			int num5 = tile.localPlace.x - minX;
			int num6 = tile.localPlace.y - minY;
			arrangedMap[num5, num6] = tile;
		});
		return arrangedMap;
	}

	public static IEnumerator DrawElevationMapCoroutine(LocationGridTile[,] tileMap, int[,] cellAutomata, TileBase wallAsset, TileBase groundAsset, ELEVATION elevation, LocationStructure elevationStructure, MapGenerationData mapGenerationData)
	{
		int upperBoundX = tileMap.GetUpperBound(0) + 1;
		int upperBoundY = tileMap.GetUpperBound(1) + 1;
		int batchCount = 0;
		for (int x = 0; x < upperBoundX; x++)
		{
			for (int y = 0; y < upperBoundY; y++)
			{
				LocationGridTile locationGridTile = tileMap[x, y];
				int num = cellAutomata[x, y];
				if (locationGridTile == null)
				{
					continue;
				}
				if (num == 1)
				{
					locationGridTile.SetStructureTilemapVisual(wallAsset);
					switch (elevation)
					{
					case ELEVATION.WATER:
						throw new NotImplementedException();
					case ELEVATION.MOUNTAIN:
						locationGridTile.parentMap.SetAsMountainWall(locationGridTile, elevationStructure, mapGenerationData);
						break;
					}
				}
				else
				{
					locationGridTile.SetStructureTilemapVisual(groundAsset);
				}
				batchCount++;
				if (batchCount == MapGenerationData.InnerMapElevationBatches)
				{
					batchCount = 0;
					yield return null;
				}
			}
		}
	}

	public static void DrawMap(LocationGridTile[,] tileMap, int[,] cellAutomata, TileBase wallAsset, TileBase groundAsset, Action<LocationGridTile> wallAction, Action<LocationGridTile> groundAction)
	{
		int num = tileMap.GetUpperBound(0) + 1;
		int num2 = tileMap.GetUpperBound(1) + 1;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				LocationGridTile locationGridTile = tileMap[i, j];
				int num3 = cellAutomata[i, j];
				if (locationGridTile != null)
				{
					if (num3 == 1)
					{
						locationGridTile.SetStructureTilemapVisual(wallAsset);
						wallAction?.Invoke(locationGridTile);
					}
					else
					{
						locationGridTile.SetStructureTilemapVisual(groundAsset);
						groundAction?.Invoke(locationGridTile);
					}
				}
			}
		}
	}
}

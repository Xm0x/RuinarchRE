using System.Collections.Generic;
using UnityEngine;

public static class AreaExtensions
{
	public static void PopulateAreasInRange(this Area p_area, List<Area> areasInRange, int p_range, bool sameRegionOnly = true)
	{
		CubeCoordinate cubeCoordinate = OddRToCube(new Vector2Int(p_area.areaData.xCoordinate, p_area.areaData.yCoordinate));
		for (int i = -p_range; i <= p_range; i++)
		{
			for (int j = Mathf.Max(-p_range, -i - p_range); j <= Mathf.Min(p_range, -i + p_range); j++)
			{
				int num = -i - j;
				HexCoordinate hexCoordinate = CubeToOddR(new Vector3Int(cubeCoordinate.x + i, cubeCoordinate.y + j, cubeCoordinate.z + num));
				if (hexCoordinate.x >= 0 && hexCoordinate.y >= 0 && hexCoordinate.x < GridMap.Instance.width && hexCoordinate.y < GridMap.Instance.height && (hexCoordinate.x != p_area.areaData.xCoordinate || hexCoordinate.y != p_area.areaData.yCoordinate))
				{
					Area area = GridMap.Instance.map[hexCoordinate.x, hexCoordinate.y];
					if (!sameRegionOnly || area.region == p_area.region)
					{
						areasInRange.Add(area);
					}
				}
			}
		}
	}

	private static HexCoordinate CubeToOddR(Vector3Int cube)
	{
		int num = 0;
		if (cube.z % 2 == 1)
		{
			num = 1;
		}
		int x = cube.x + (cube.z - num) / 2;
		int z = cube.z;
		return new HexCoordinate(x, z);
	}

	private static CubeCoordinate OddRToCube(Vector2Int hex)
	{
		int num = 0;
		if (hex.y % 2 == 1)
		{
			num = 1;
		}
		int num2 = hex.x - (hex.y - num) / 2;
		int y = hex.y;
		int y2 = -num2 - y;
		return new CubeCoordinate(num2, y2, y);
	}
}

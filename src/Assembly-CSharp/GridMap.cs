using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class GridMap : BaseMonoBehaviour
{
	public static GridMap Instance;

	[Space(10f)]
	[Header("Map Settings")]
	public int width;

	public int height;

	[SerializeField]
	internal int _borderThickness;

	[Space(10f)]
	public Area[,] map;

	public List<Area> edgeAreas;

	public List<Area> allAreas => DatabaseManager.Instance.areaDatabase.allAreas;

	public Region mainRegion => DatabaseManager.Instance.regionDatabase.mainRegion;

	private void Awake()
	{
		Instance = this;
	}

	public void SetupInitialData(int width, int height)
	{
		this.width = width;
		this.height = height;
	}

	public void SetMap(Area[,] p_map, List<Area> p_areas)
	{
		map = p_map;
		for (int i = 0; i < p_areas.Count; i++)
		{
			Area p_area = p_areas[i];
			DatabaseManager.Instance.areaDatabase.RegisterArea(p_area);
		}
		edgeAreas = new List<Area>();
		for (int j = 0; j < width; j++)
		{
			for (int k = 0; k < height; k++)
			{
				Area item = map[j, k];
				if (j == 0 || j == width - 1 || k == 0 || k == height - 1)
				{
					edgeAreas.Add(item);
				}
			}
		}
	}

	public void SetMap(Area[,] p_map)
	{
		map = p_map;
		edgeAreas = new List<Area>();
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				Area item = map[i, j];
				if (i == 0 || i == width - 1 || j == 0 || j == height - 1)
				{
					edgeAreas.Add(item);
				}
			}
		}
	}

	public LocationGridTile GetFirstPassableGridTile()
	{
		for (int i = 0; i < allAreas.Count; i++)
		{
			Area area = allAreas[i];
			if (area.gridTileComponent.passableTiles.Count > 0)
			{
				return area.gridTileComponent.passableTiles[0];
			}
		}
		return null;
	}

	public LocationGridTile GetFirstPassableUnoccupiedGridTile()
	{
		for (int i = 0; i < allAreas.Count; i++)
		{
			Area area = allAreas[i];
			for (int j = 0; j < area.gridTileComponent.passableTiles.Count; j++)
			{
				LocationGridTile locationGridTile = area.gridTileComponent.passableTiles[j];
				if (!locationGridTile.isOccupied && locationGridTile.tileObjectComponent.objHere == null && locationGridTile.tileObjectComponent.hiddenObjHere == null)
				{
					return locationGridTile;
				}
			}
		}
		return null;
	}

	protected override void OnDestroy()
	{
		mainRegion?.CleanUp();
		map = null;
		base.OnDestroy();
		Instance = null;
	}
}

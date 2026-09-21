using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Inner_Maps;

public abstract class BaseIsland
{
	public Color color;

	public HashSet<LocationGridTile> tiles { get; }

	public List<LocationGridTile> borderTiles { get; }

	public List<Area> occupiedAreas { get; }

	public BaseIsland()
	{
		tiles = new HashSet<LocationGridTile>();
		borderTiles = new List<LocationGridTile>();
		occupiedAreas = new List<Area>();
		color = Random.ColorHSV();
	}

	public virtual void AddTile(LocationGridTile tile, MapGenerationData mapGenerationData)
	{
		tiles.Add(tile);
		AddOccupiedArea(tile.area);
		if (ShouldTileBeABorderTile(tile))
		{
			AddBorderTile(tile, mapGenerationData);
		}
	}

	public virtual void RemoveTile(LocationGridTile tile, MapGenerationData mapGenerationData)
	{
		tiles.Remove(tile);
		RemoveBorderTile(tile, mapGenerationData);
	}

	public void RemoveAllTiles()
	{
		tiles.Clear();
		borderTiles.Clear();
		occupiedAreas.Clear();
	}

	public void MergeWithIsland(BaseIsland otherIsland, MapGenerationData mapGenerationData)
	{
		for (int i = 0; i < otherIsland.tiles.Count; i++)
		{
			LocationGridTile tile = otherIsland.tiles.ElementAt(i);
			AddTile(tile, mapGenerationData);
		}
		otherIsland.RemoveAllTiles();
	}

	public bool IsAdjacentToIsland(BaseIsland otherIsland)
	{
		for (int i = 0; i < borderTiles.Count; i++)
		{
			LocationGridTile locationGridTile = borderTiles[i];
			for (int j = 0; j < locationGridTile.neighbourList.Count; j++)
			{
				LocationGridTile item = locationGridTile.neighbourList[j];
				if (otherIsland.tiles.Contains(item))
				{
					return true;
				}
			}
		}
		return false;
	}

	public T GetFirstAdjacentIsland<T>(List<T> p_choices) where T : BaseIsland
	{
		for (int i = 0; i < p_choices.Count; i++)
		{
			T val = p_choices[i];
			if (val != this && IsAdjacentToIsland(val))
			{
				return val;
			}
		}
		return null;
	}

	private bool ShouldTileBeABorderTile(LocationGridTile p_tile)
	{
		for (int i = 0; i < p_tile.neighbourList.Count; i++)
		{
			LocationGridTile item = p_tile.neighbourList[i];
			if (!tiles.Contains(item))
			{
				return true;
			}
		}
		return false;
	}

	protected virtual void AddBorderTile(LocationGridTile p_tile, MapGenerationData mapGenerationData)
	{
		borderTiles.Add(p_tile);
		RevalidateBorderTilesNexTo(p_tile, mapGenerationData);
	}

	protected virtual bool RemoveBorderTile(LocationGridTile p_tile, MapGenerationData mapGenerationData)
	{
		return borderTiles.Remove(p_tile);
	}

	private void RevalidateBorderTilesNexTo(LocationGridTile p_tile, MapGenerationData mapGenerationData)
	{
		for (int i = 0; i < p_tile.neighbourList.Count; i++)
		{
			LocationGridTile locationGridTile = p_tile.neighbourList[i];
			if (borderTiles.Contains(locationGridTile) && !ShouldTileBeABorderTile(locationGridTile))
			{
				RemoveBorderTile(locationGridTile, mapGenerationData);
			}
		}
	}

	public void AddOccupiedArea(Area p_area)
	{
		if (!occupiedAreas.Contains(p_area))
		{
			occupiedAreas.Add(p_area);
		}
	}
}

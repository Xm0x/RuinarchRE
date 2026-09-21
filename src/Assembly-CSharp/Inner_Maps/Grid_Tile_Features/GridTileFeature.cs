using System;
using System.Collections.Generic;

namespace Inner_Maps.Grid_Tile_Features;

public abstract class GridTileFeature
{
	protected List<LocationGridTile> _tilesWithFeature;

	protected Dictionary<Area, List<LocationGridTile>> _tilesWithFeatureCategorizedByArea;

	public List<LocationGridTile> tilesWithFeature => _tilesWithFeature;

	public virtual Type serializedData => typeof(SaveDataGridTileFeature);

	public GridTileFeature()
	{
		_tilesWithFeature = new List<LocationGridTile>();
		_tilesWithFeatureCategorizedByArea = new Dictionary<Area, List<LocationGridTile>>();
	}

	public GridTileFeature(SaveDataGridTileFeature p_data)
		: this()
	{
	}

	public virtual void LoadReferences(SaveDataGridTileFeature p_data)
	{
		for (int i = 0; i < p_data.tiles.Length; i++)
		{
			TileLocationSave tileLocationSave = p_data.tiles[i];
			LocationGridTile tileBySavedData = DatabaseManager.Instance.locationGridTileDatabase.GetTileBySavedData(tileLocationSave);
			_tilesWithFeature.Add(tileBySavedData);
			if (!_tilesWithFeatureCategorizedByArea.ContainsKey(tileBySavedData.area))
			{
				_tilesWithFeatureCategorizedByArea.Add(tileBySavedData.area, new List<LocationGridTile>());
			}
			_tilesWithFeatureCategorizedByArea[tileBySavedData.area].Add(tileBySavedData);
		}
	}

	public abstract void Initialize();

	public void AddTile(LocationGridTile p_tile)
	{
		if (!_tilesWithFeature.Contains(p_tile))
		{
			_tilesWithFeature.Add(p_tile);
			if (!_tilesWithFeatureCategorizedByArea.ContainsKey(p_tile.area))
			{
				_tilesWithFeatureCategorizedByArea.Add(p_tile.area, new List<LocationGridTile>());
			}
			_tilesWithFeatureCategorizedByArea[p_tile.area].Add(p_tile);
		}
	}

	public virtual bool RemoveTile(LocationGridTile p_tile)
	{
		if (_tilesWithFeature.Remove(p_tile))
		{
			if (_tilesWithFeatureCategorizedByArea.ContainsKey(p_tile.area))
			{
				_tilesWithFeatureCategorizedByArea[p_tile.area].Remove(p_tile);
			}
			return true;
		}
		return false;
	}

	public List<LocationGridTile> GetFeatureTilesInArea(Area p_area)
	{
		if (_tilesWithFeatureCategorizedByArea.ContainsKey(p_area))
		{
			return _tilesWithFeatureCategorizedByArea[p_area];
		}
		return null;
	}
}

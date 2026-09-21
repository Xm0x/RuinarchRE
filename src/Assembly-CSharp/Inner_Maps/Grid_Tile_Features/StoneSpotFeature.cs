using System;
using System.Collections.Generic;
using UtilityScripts;

namespace Inner_Maps.Grid_Tile_Features;

public class StoneSpotFeature : GridTileFeature
{
	private List<LocationGridTile> _unoccupiedSpots;

	public List<LocationGridTile> unoccupiedSpots => _unoccupiedSpots;

	public override Type serializedData => typeof(SaveDataStoneSpotFeature);

	public StoneSpotFeature()
	{
		_unoccupiedSpots = new List<LocationGridTile>();
	}

	public StoneSpotFeature(SaveDataStoneSpotFeature p_data)
		: base(p_data)
	{
		_unoccupiedSpots = new List<LocationGridTile>();
	}

	public override void Initialize()
	{
		Messenger.AddListener(Signals.DAY_STARTED, OnDayStarted);
		Messenger.AddListener<TileObject, Character, LocationGridTile>(GridTileSignals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
	}

	public override void LoadReferences(SaveDataGridTileFeature p_data)
	{
		base.LoadReferences(p_data);
		SaveDataStoneSpotFeature saveDataStoneSpotFeature = p_data as SaveDataStoneSpotFeature;
		for (int i = 0; i < saveDataStoneSpotFeature.unoccupiedTiles.Length; i++)
		{
			TileLocationSave tileLocationSave = saveDataStoneSpotFeature.unoccupiedTiles[i];
			LocationGridTile tileBySavedData = DatabaseManager.Instance.locationGridTileDatabase.GetTileBySavedData(tileLocationSave);
			_unoccupiedSpots.Add(tileBySavedData);
		}
	}

	private void OnTileObjectRemoved(TileObject p_tileObject, Character p_removedBy, LocationGridTile p_removedFrom)
	{
		if (p_tileObject.tileObjectType == TILE_OBJECT_TYPE.ROCK && base.tilesWithFeature.Contains(p_removedFrom) && !_unoccupiedSpots.Contains(p_removedFrom))
		{
			_unoccupiedSpots.Add(p_removedFrom);
		}
	}

	public override bool RemoveTile(LocationGridTile p_tile)
	{
		if (base.RemoveTile(p_tile))
		{
			_unoccupiedSpots.Remove(p_tile);
			return true;
		}
		return false;
	}

	private void OnDayStarted()
	{
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		list.AddRange(_unoccupiedSpots);
		for (int i = 0; i < list.Count; i++)
		{
			LocationGridTile locationGridTile = list[i];
			_ = locationGridTile.structure;
			if (locationGridTile.tileObjectComponent.objHere == null && GameUtilities.RollChance(50))
			{
				_unoccupiedSpots.Remove(locationGridTile);
				TileObject poi = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.ROCK);
				locationGridTile.structure.AddPOI(poi, locationGridTile);
			}
		}
		RuinarchListPool<LocationGridTile>.Release(list);
	}
}

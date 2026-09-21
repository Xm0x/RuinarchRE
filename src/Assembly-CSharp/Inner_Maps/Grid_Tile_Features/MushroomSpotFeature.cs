using System;
using System.Collections.Generic;
using UtilityScripts;

namespace Inner_Maps.Grid_Tile_Features;

public class MushroomSpotFeature : GridTileFeature
{
	private List<LocationGridTile> _unoccupiedSpots;

	public List<LocationGridTile> unoccupiedSpots => _unoccupiedSpots;

	public override Type serializedData => typeof(SaveDataMushroomSpotFeature);

	public MushroomSpotFeature()
	{
		_unoccupiedSpots = new List<LocationGridTile>();
	}

	public MushroomSpotFeature(SaveDataMushroomSpotFeature p_data)
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
		SaveDataMushroomSpotFeature saveDataMushroomSpotFeature = p_data as SaveDataMushroomSpotFeature;
		for (int i = 0; i < saveDataMushroomSpotFeature.unoccupiedTiles.Length; i++)
		{
			TileLocationSave tileLocationSave = saveDataMushroomSpotFeature.unoccupiedTiles[i];
			LocationGridTile tileBySavedData = DatabaseManager.Instance.locationGridTileDatabase.GetTileBySavedData(tileLocationSave);
			_unoccupiedSpots.Add(tileBySavedData);
		}
	}

	private void OnTileObjectRemoved(TileObject p_tileObject, Character p_removedBy, LocationGridTile p_removedFrom)
	{
		if (p_tileObject.tileObjectType == TILE_OBJECT_TYPE.MUSHROOM && base.tilesWithFeature.Contains(p_removedFrom) && !_unoccupiedSpots.Contains(p_removedFrom))
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
			if (locationGridTile.tileObjectComponent.objHere == null && ChanceData.RollChance(CHANCE_TYPE.Spawn_Mushroom_Day_Start))
			{
				_unoccupiedSpots.Remove(locationGridTile);
				TileObject poi = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.MUSHROOM);
				locationGridTile.structure.AddPOI(poi, locationGridTile);
			}
		}
		RuinarchListPool<LocationGridTile>.Release(list);
	}
}

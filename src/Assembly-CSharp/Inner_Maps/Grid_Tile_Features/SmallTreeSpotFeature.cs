using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UtilityScripts;

namespace Inner_Maps.Grid_Tile_Features;

public class SmallTreeSpotFeature : GridTileFeature
{
	private List<LocationGridTile> _unoccupiedSpots;

	public List<LocationGridTile> unoccupiedSpots => _unoccupiedSpots;

	public override Type serializedData => typeof(SaveDataSmallTreeSpotFeature);

	public SmallTreeSpotFeature()
	{
		_unoccupiedSpots = new List<LocationGridTile>();
	}

	public SmallTreeSpotFeature(SaveDataSmallTreeSpotFeature p_data)
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
		SaveDataSmallTreeSpotFeature saveDataSmallTreeSpotFeature = p_data as SaveDataSmallTreeSpotFeature;
		for (int i = 0; i < saveDataSmallTreeSpotFeature.unoccupiedTiles.Length; i++)
		{
			TileLocationSave tileLocationSave = saveDataSmallTreeSpotFeature.unoccupiedTiles[i];
			LocationGridTile tileBySavedData = DatabaseManager.Instance.locationGridTileDatabase.GetTileBySavedData(tileLocationSave);
			_unoccupiedSpots.Add(tileBySavedData);
		}
	}

	private void OnTileObjectRemoved(TileObject p_tileObject, Character p_removedBy, LocationGridTile p_removedFrom)
	{
		if (p_tileObject.tileObjectType == TILE_OBJECT_TYPE.SMALL_TREE_OBJECT && base.tilesWithFeature.Contains(p_removedFrom) && !unoccupiedSpots.Contains(p_removedFrom))
		{
			unoccupiedSpots.Add(p_removedFrom);
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
		for (int i = 0; i < unoccupiedSpots.Count; i++)
		{
			LocationGridTile locationGridTile = unoccupiedSpots[i];
			if (locationGridTile.tileObjectComponent.objHere == null && locationGridTile.structure is Wilderness && !locationGridTile.corruptionComponent.isCorrupted && GameUtilities.RollChance(30))
			{
				_unoccupiedSpots.Remove(locationGridTile);
				TileObject poi = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.SMALL_TREE_OBJECT);
				locationGridTile.structure.AddPOI(poi, locationGridTile);
			}
		}
		RuinarchListPool<LocationGridTile>.Release(list);
	}
}

using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UtilityScripts;

namespace Inner_Maps.Grid_Tile_Features;

public class MetalOreSpotFeature : GridTileFeature
{
	private List<LocationGridTile> _unoccupiedSpots;

	public List<LocationGridTile> unoccupiedSpots => _unoccupiedSpots;

	public override Type serializedData => typeof(SaveDataMetalOreSpotFeature);

	public MetalOreSpotFeature()
	{
		_unoccupiedSpots = new List<LocationGridTile>();
	}

	public MetalOreSpotFeature(SaveDataMetalOreSpotFeature p_data)
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
		SaveDataMetalOreSpotFeature saveDataMetalOreSpotFeature = p_data as SaveDataMetalOreSpotFeature;
		for (int i = 0; i < saveDataMetalOreSpotFeature.unoccupiedTiles.Length; i++)
		{
			TileLocationSave tileLocationSave = saveDataMetalOreSpotFeature.unoccupiedTiles[i];
			LocationGridTile tileBySavedData = DatabaseManager.Instance.locationGridTileDatabase.GetTileBySavedData(tileLocationSave);
			_unoccupiedSpots.Add(tileBySavedData);
		}
	}

	private void OnTileObjectRemoved(TileObject p_tileObject, Character p_removedBy, LocationGridTile p_removedFrom)
	{
		if (p_tileObject.tileObjectType == TILE_OBJECT_TYPE.ORE && base.tilesWithFeature.Contains(p_removedFrom) && !_unoccupiedSpots.Contains(p_removedFrom))
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
			Cave cave = locationGridTile.structure as Cave;
			if (locationGridTile.tileObjectComponent.objHere == null && GameUtilities.RollChance(30))
			{
				_unoccupiedSpots.Remove(locationGridTile);
				Ore ore = InnerMapManager.Instance.CreateNewTileObject<Ore>(TILE_OBJECT_TYPE.ORE);
				ore.SetProvidedMetal(cave.producedResource);
				locationGridTile.structure.AddPOI(ore);
			}
		}
		RuinarchListPool<LocationGridTile>.Release(list);
	}
}

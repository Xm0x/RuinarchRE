using System;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class LocationGridTileDatabase
{
	public Dictionary<string, LocationGridTile> tileByGUID { get; }

	public List<LocationGridTile> locationGridTiles { get; }

	public LocationGridTileDatabase()
	{
		tileByGUID = new Dictionary<string, LocationGridTile>();
		locationGridTiles = new List<LocationGridTile>();
	}

	public void RegisterTile(LocationGridTile tile)
	{
		tileByGUID.Add(tile.persistentID, tile);
		locationGridTiles.Add(tile);
	}

	public LocationGridTile GetTileByPersistentID(string id)
	{
		if (tileByGUID.ContainsKey(id))
		{
			return tileByGUID[id];
		}
		throw new Exception("There is no Location Grid Tile with id " + id);
	}

	public LocationGridTile GetTileBySavedData(TileLocationSave tileLocationSave)
	{
		return DatabaseManager.Instance.regionDatabase.mainRegion.innerMap.GetTileFromMapCoordinates(tileLocationSave.xPos, tileLocationSave.yPos);
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		for (int i = 0; i < locationGridTiles.Count; i++)
		{
			locationGridTiles[i].CheckIfStructureIsStillReferenced(p_structure);
		}
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		for (int i = 0; i < locationGridTiles.Count; i++)
		{
			locationGridTiles[i].CheckIfCharacterIsStillReferenced(p_character);
		}
	}
}

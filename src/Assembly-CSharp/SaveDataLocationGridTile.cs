using BayatGames.SaveGameFree.Types;
using Inner_Maps;
using UnityEngine.Tilemaps;

public class SaveDataLocationGridTile : SaveData<LocationGridTile>
{
	public string persistentID;

	public Vector3Save localPlace;

	public Vector3Save worldLocation;

	public Vector3Save centeredWorldLocation;

	public Vector3Save localLocation;

	public Vector3Save centeredLocalLocation;

	public LocationGridTile.Tile_Type tileType;

	public LocationGridTile.Tile_State tileState;

	public int meteorCount;

	public int nonPlayerMeteorCount;

	public int connectorsCount;

	public ELEVATION elevation;

	public int walkedOnCount;

	public int nonDefaultCounter;

	public GameDate scheduleToRevert;

	public string groundTileMapAssetName;

	public string wallTileMapAssetName;

	public string shoreTileMapAssetName;

	public float floorSample;

	public SaveDataGridTileCorruptionComponent corruptionComponent;

	public SaveDataGridTileMouseEventsComponent mouseEventsComponent;

	public SaveDataGridTileTileObjectComponent tileObjectComponent;

	public override void Save(LocationGridTile gridTile)
	{
		persistentID = gridTile.persistentID;
		localPlace = new Vector3Save(gridTile.localPlace);
		worldLocation = gridTile.worldLocation;
		centeredWorldLocation = gridTile.centeredWorldLocation;
		localLocation = gridTile.localLocation;
		centeredLocalLocation = gridTile.centeredLocalLocation;
		tileType = gridTile.tileType;
		tileState = gridTile.tileState;
		meteorCount = gridTile.meteorCount;
		nonPlayerMeteorCount = gridTile.nonPlayerMeteorCount;
		connectorsCount = gridTile.connectorsOnTile;
		elevation = gridTile.elevationType;
		walkedOnCount = gridTile.walkedOnCount;
		nonDefaultCounter = gridTile.nonDefaultCounter;
		scheduleToRevert = gridTile.scheduleToRevert;
		groundTileMapAssetName = gridTile.groundTileMapAssetName;
		wallTileMapAssetName = gridTile.wallTileMapAssetName;
		shoreTileMapAssetName = gridTile.shoreTileMapAssetName;
		floorSample = gridTile.floorSample;
		corruptionComponent = new SaveDataGridTileCorruptionComponent();
		corruptionComponent.Save(gridTile.corruptionComponent);
		mouseEventsComponent = new SaveDataGridTileMouseEventsComponent();
		mouseEventsComponent.Save(gridTile.mouseEventsComponent);
		tileObjectComponent = new SaveDataGridTileTileObjectComponent();
		tileObjectComponent.Save(gridTile.tileObjectComponent);
	}

	public LocationGridTile InitialLoad(Tilemap tilemap, InnerTileMap parentAreaMap, SaveDataCurrentProgress saveData, Area p_area)
	{
		LocationGridTile locationGridTile = new LocationGridTile(this, tilemap, parentAreaMap, p_area);
		locationGridTile.SetFloorSample(floorSample);
		SaveDataTileObject fromSaveHub = saveData.GetFromSaveHub<SaveDataTileObject>(OBJECT_TYPE.Tile_Object, tileObjectComponent.genericTileObjectID);
		GenericTileObject genericTileObject = fromSaveHub.Load() as GenericTileObject;
		genericTileObject.SetTileOwner(locationGridTile);
		genericTileObject.ManualInitializeLoad(locationGridTile, fromSaveHub);
		locationGridTile.tileObjectComponent.LoadGenericTileObject(genericTileObject);
		return locationGridTile;
	}

	public override void CleanUp()
	{
		corruptionComponent.CleanUp();
		corruptionComponent = null;
		mouseEventsComponent.CleanUp();
		mouseEventsComponent = null;
		tileObjectComponent.CleanUp();
		tileObjectComponent = null;
	}
}

public class TableWeapons : TileObject
{
	public TableWeapons()
	{
		Initialize(TILE_OBJECT_TYPE.TABLE_WEAPONS);
	}

	public TableWeapons(SaveDataTileObject data)
		: base(data)
	{
	}
}

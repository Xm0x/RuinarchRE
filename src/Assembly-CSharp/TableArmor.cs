public class TableArmor : TileObject
{
	public TableArmor()
	{
		Initialize(TILE_OBJECT_TYPE.TABLE_ARMOR);
	}

	public TableArmor(SaveDataTileObject data)
		: base(data)
	{
	}
}

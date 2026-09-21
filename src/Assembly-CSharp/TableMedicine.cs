public class TableMedicine : TileObject
{
	public TableMedicine()
	{
		Initialize(TILE_OBJECT_TYPE.TABLE_MEDICINE);
	}

	public TableMedicine(SaveDataTileObject data)
		: base(data)
	{
	}
}

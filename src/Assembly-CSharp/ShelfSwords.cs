public class ShelfSwords : TileObject
{
	public ShelfSwords()
	{
		Initialize(TILE_OBJECT_TYPE.SHELF_SWORDS);
	}

	public ShelfSwords(SaveDataTileObject data)
		: base(data)
	{
	}
}

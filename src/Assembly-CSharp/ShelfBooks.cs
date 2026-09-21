public class ShelfBooks : TileObject
{
	public ShelfBooks()
	{
		Initialize(TILE_OBJECT_TYPE.SHELF_BOOKS);
	}

	public ShelfBooks(SaveDataTileObject data)
		: base(data)
	{
	}
}

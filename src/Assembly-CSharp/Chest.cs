public class Chest : TileObject
{
	public Chest()
	{
		Initialize(TILE_OBJECT_TYPE.CHEST);
	}

	public Chest(SaveDataTileObject data)
		: base(data)
	{
	}
}

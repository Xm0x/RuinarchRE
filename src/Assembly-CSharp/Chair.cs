public class Chair : TileObject
{
	public Chair()
	{
		Initialize(TILE_OBJECT_TYPE.CHAIR);
	}

	public Chair(SaveDataTileObject data)
		: base(data)
	{
	}
}

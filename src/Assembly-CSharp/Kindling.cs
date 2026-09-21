public class Kindling : TileObject
{
	public Kindling()
	{
		Initialize(TILE_OBJECT_TYPE.KINDLING);
	}

	public Kindling(SaveDataTileObject data)
		: base(data)
	{
	}
}

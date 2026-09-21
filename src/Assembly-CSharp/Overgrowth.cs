public class Overgrowth : TileObject
{
	public Overgrowth()
	{
		Initialize(TILE_OBJECT_TYPE.OVERGROWTH);
	}

	public Overgrowth(SaveDataTileObject data)
		: base(data)
	{
	}
}

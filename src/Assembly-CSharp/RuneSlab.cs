public class RuneSlab : TileObject
{
	public RuneSlab()
	{
		Initialize(TILE_OBJECT_TYPE.RUNE_SLAB);
	}

	public RuneSlab(SaveDataTileObject data)
		: base(data)
	{
	}
}

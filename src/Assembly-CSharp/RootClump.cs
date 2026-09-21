public class RootClump : TileObject
{
	public RootClump()
	{
		Initialize(TILE_OBJECT_TYPE.ROOT_CLUMP);
	}

	public RootClump(SaveDataTileObject data)
		: base(data)
	{
	}
}

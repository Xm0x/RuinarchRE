public class RackStaves : TileObject
{
	public RackStaves()
	{
		Initialize(TILE_OBJECT_TYPE.RACK_STAVES);
	}

	public RackStaves(SaveDataTileObject data)
		: base(data)
	{
	}
}

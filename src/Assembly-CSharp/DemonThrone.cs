public class DemonThrone : TileObject
{
	public DemonThrone()
	{
		Initialize(TILE_OBJECT_TYPE.DEMON_THRONE);
	}

	public DemonThrone(SaveDataTileObject data)
		: base(data)
	{
	}
}

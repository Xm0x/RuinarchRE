public class MagmaContainers : TileObject
{
	public MagmaContainers()
	{
		Initialize(TILE_OBJECT_TYPE.MAGMA_CONTAINERS);
	}

	public MagmaContainers(SaveDataTileObject data)
		: base(data)
	{
	}
}

public class Reliquary : TileObject
{
	public Reliquary()
	{
		Initialize(TILE_OBJECT_TYPE.RELIQUARY);
	}

	public Reliquary(SaveDataTileObject data)
		: base(data)
	{
	}
}

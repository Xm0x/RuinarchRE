public class Plinth : TileObject
{
	public Plinth()
	{
		Initialize(TILE_OBJECT_TYPE.PLINTH);
	}

	public Plinth(SaveDataTileObject data)
		: base(data)
	{
	}
}

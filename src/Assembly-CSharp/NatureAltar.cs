public class NatureAltar : TileObject
{
	public NatureAltar()
	{
		Initialize(TILE_OBJECT_TYPE.NATURE_ALTAR);
	}

	public NatureAltar(SaveDataTileObject data)
		: base(data)
	{
	}
}

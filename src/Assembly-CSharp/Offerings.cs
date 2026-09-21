public class Offerings : TileObject
{
	public Offerings()
	{
		Initialize(TILE_OBJECT_TYPE.OFFERINGS);
	}

	public Offerings(SaveDataTileObject data)
		: base(data)
	{
	}
}

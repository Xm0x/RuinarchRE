public class Stickman : TileObject
{
	public Stickman()
	{
		Initialize(TILE_OBJECT_TYPE.STICKMAN);
	}

	public Stickman(SaveDataTileObject data)
		: base(data)
	{
	}
}

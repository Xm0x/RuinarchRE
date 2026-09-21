public class DivineAltar : TileObject
{
	public DivineAltar()
	{
		Initialize(TILE_OBJECT_TYPE.DIVINE_ALTAR);
	}

	public DivineAltar(SaveDataTileObject data)
		: base(data)
	{
	}
}

public class HerbPots : TileObject
{
	public HerbPots()
	{
		Initialize(TILE_OBJECT_TYPE.HERB_POTS);
	}

	public HerbPots(SaveDataTileObject data)
		: base(data)
	{
	}
}

public class CoalPile : TileObject
{
	public CoalPile()
	{
		Initialize(TILE_OBJECT_TYPE.COAL_PILE);
	}

	public CoalPile(SaveDataTileObject data)
		: base(data)
	{
	}
}

public class Glyph : TileObject
{
	public Glyph()
	{
		Initialize(TILE_OBJECT_TYPE.GLYPH);
	}

	public Glyph(SaveDataTileObject data)
		: base(data)
	{
	}
}

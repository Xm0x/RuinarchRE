public class Wool : ClothPile
{
	public override CONCRETE_RESOURCES specificProvidedResource => CONCRETE_RESOURCES.Wool;

	public Wool()
		: base(TILE_OBJECT_TYPE.WOOL)
	{
	}

	public Wool(SaveDataTileObject saveDataTileObject)
		: base(saveDataTileObject)
	{
	}

	public override string ToString()
	{
		return "Wool " + base.id;
	}
}

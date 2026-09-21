public class HypnoHerb : FoodPile
{
	public override CONCRETE_RESOURCES specificProvidedResource => CONCRETE_RESOURCES.Hypno_Herb;

	public HypnoHerb()
		: base(TILE_OBJECT_TYPE.HYPNO_HERB)
	{
	}

	public HypnoHerb(SaveDataTileObject saveDataTileObject)
		: base(saveDataTileObject)
	{
	}

	public override string ToString()
	{
		return "Hypno Herb " + base.id;
	}
}

public class DragonHide : LeatherPile
{
	public override CONCRETE_RESOURCES specificProvidedResource => CONCRETE_RESOURCES.Dragon_Hide;

	public DragonHide()
		: base(TILE_OBJECT_TYPE.DRAGON_HIDE)
	{
	}

	public DragonHide(SaveDataTileObject saveDataTileObject)
		: base(saveDataTileObject)
	{
	}

	public override string ToString()
	{
		return "Dragon Hide " + base.id;
	}
}

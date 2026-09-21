public abstract class LeatherPile : ResourcePile
{
	protected LeatherPile(TILE_OBJECT_TYPE tileObjectType)
		: base(RESOURCE.LEATHER)
	{
		Initialize(tileObjectType, shouldAddCommonAdvertisements: false);
		SetResourceInPile(100);
	}

	protected LeatherPile(SaveDataTileObject saveDataTileObject)
		: base(saveDataTileObject, RESOURCE.LEATHER)
	{
	}

	public override string ToString()
	{
		return "Leather Pile " + base.id;
	}
}

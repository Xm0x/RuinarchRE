public abstract class ClothPile : ResourcePile
{
	protected ClothPile(TILE_OBJECT_TYPE tileObjectType)
		: base(RESOURCE.CLOTH)
	{
		Initialize(tileObjectType, shouldAddCommonAdvertisements: false);
		SetResourceInPile(100);
	}

	protected ClothPile(SaveDataTileObject saveDataTileObject)
		: base(saveDataTileObject, RESOURCE.CLOTH)
	{
	}

	public override string ToString()
	{
		return "Cloth Pile " + base.id;
	}
}

public class SaveDataFoodPile : SaveDataTileObject
{
	public bool isFromManifestFood;

	public FOOD_INFUSE_TYPE infusedType;

	public override void Save(TileObject tileObject)
	{
		base.Save(tileObject);
		FoodPile foodPile = tileObject as FoodPile;
		isFromManifestFood = foodPile.isFromManifestFood;
		infusedType = foodPile.infusedType;
	}
}

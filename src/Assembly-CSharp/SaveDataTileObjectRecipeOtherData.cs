public class SaveDataTileObjectRecipeOtherData : SaveDataOtherData
{
	public TileObjectRecipe recipe;

	public override void Save(OtherData data)
	{
		base.Save(data);
		TileObjectRecipeOtherData tileObjectRecipeOtherData = data as TileObjectRecipeOtherData;
		recipe = tileObjectRecipeOtherData.recipe;
	}

	public override OtherData Load()
	{
		return new TileObjectRecipeOtherData(this);
	}
}

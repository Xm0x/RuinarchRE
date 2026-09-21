public class TileObjectRecipeOtherData : OtherData
{
	public TileObjectRecipe recipe { get; private set; }

	public override object obj => recipe;

	public TileObjectRecipeOtherData(TileObjectRecipe recipe)
	{
		this.recipe = recipe;
	}

	public TileObjectRecipeOtherData(SaveDataTileObjectRecipeOtherData data)
	{
		recipe = data.recipe;
	}

	public override SaveDataOtherData Save()
	{
		SaveDataTileObjectRecipeOtherData saveDataTileObjectRecipeOtherData = new SaveDataTileObjectRecipeOtherData();
		saveDataTileObjectRecipeOtherData.Save(this);
		return saveDataTileObjectRecipeOtherData;
	}

	public override void CleanUp()
	{
		recipe = default(TileObjectRecipe);
	}
}

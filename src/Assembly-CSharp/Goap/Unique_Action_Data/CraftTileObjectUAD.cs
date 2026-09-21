namespace Goap.Unique_Action_Data;

public class CraftTileObjectUAD : UniqueActionData
{
	public TileObjectRecipe recipeUsed;

	public CraftTileObjectUAD()
	{
		recipeUsed = default(TileObjectRecipe);
	}

	public CraftTileObjectUAD(SaveDataCraftTileObjectUAD saveData)
	{
		recipeUsed = saveData.recipeUsed;
	}

	public override SaveDataUniqueActionData Save()
	{
		SaveDataCraftTileObjectUAD saveDataCraftTileObjectUAD = new SaveDataCraftTileObjectUAD();
		saveDataCraftTileObjectUAD.Save(this);
		return saveDataCraftTileObjectUAD;
	}

	public void SetRecipeUsedForCrafting(TileObjectRecipe p_recipe)
	{
		recipeUsed = p_recipe;
	}
}

namespace Goap.Unique_Action_Data;

public class SaveDataCraftTileObjectUAD : SaveDataUniqueActionData
{
	public TileObjectRecipe recipeUsed;

	public override void Save(UniqueActionData data)
	{
		base.Save(data);
		CraftTileObjectUAD craftTileObjectUAD = data as CraftTileObjectUAD;
		recipeUsed = craftTileObjectUAD.recipeUsed;
	}

	public override UniqueActionData Load()
	{
		return new CraftTileObjectUAD(this);
	}
}

public struct TileObjectRecipe
{
	public TileObjectRecipeIngredient ingredient;

	public bool hasValue;

	public TileObjectRecipe(TileObjectRecipeIngredient ingredient)
	{
		this.ingredient = ingredient;
		hasValue = true;
	}

	public int GetNeededAmountForIngredient(TILE_OBJECT_TYPE ingredient)
	{
		TileObjectRecipeIngredient tileObjectRecipeIngredient = this.ingredient;
		if (tileObjectRecipeIngredient.ingredient == ingredient)
		{
			return tileObjectRecipeIngredient.amount;
		}
		return 0;
	}

	public bool UsesIngredient(TILE_OBJECT_TYPE tileObjectType)
	{
		if (ingredient.ingredient == tileObjectType)
		{
			return true;
		}
		return false;
	}

	public override string ToString()
	{
		return ingredient.ToString();
	}
}

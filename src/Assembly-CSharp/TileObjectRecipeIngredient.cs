public struct TileObjectRecipeIngredient
{
	public TILE_OBJECT_TYPE ingredient;

	public int amount;

	public string ingredientName;

	public TileObjectRecipeIngredient(TILE_OBJECT_TYPE ingredient, int amount)
	{
		this.ingredient = ingredient;
		this.amount = amount;
		ingredientName = ingredient.ToStringEnumWithSpace();
	}

	public override string ToString()
	{
		return amount + " " + ingredientName;
	}
}

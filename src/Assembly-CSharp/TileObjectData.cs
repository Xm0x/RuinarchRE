using System.Linq;

public class TileObjectData
{
	public int maxHP;

	public string[] neededCharacterClass;

	public Point occupiedSize;

	public TileObjectRecipe[] craftRecipes;

	public int repairCost;

	public int purchaseCost;

	public int craftResourceCost;

	public int constructionTimeInTicks;

	public TileObjectRecipe mainRecipe => craftRecipes.FirstOrDefault();

	public bool TryGetPossibleRecipe(Region region, out TileObjectRecipe possibleRecipe)
	{
		for (int i = 0; i < craftRecipes.Length; i++)
		{
			TileObjectRecipe tileObjectRecipe = craftRecipes[i];
			if (region.tileObjectsComponent.GetTileObjectInRegionCount(tileObjectRecipe.ingredient.ingredient) > 0)
			{
				possibleRecipe = tileObjectRecipe;
				return true;
			}
		}
		possibleRecipe = mainRecipe;
		return false;
	}

	public bool TryGetPossibleRecipe(NPCSettlement p_settlement, out TileObjectRecipe possibleRecipe)
	{
		for (int i = 0; i < craftRecipes.Length; i++)
		{
			TileObjectRecipe tileObjectRecipe = craftRecipes[i];
			if ((tileObjectRecipe.ingredient.ingredient == TILE_OBJECT_TYPE.WOOD_PILE) ? (p_settlement.settlementJobTriggerComponent.HasAccessToResource(RESOURCE.WOOD) || (p_settlement.owner != null && p_settlement.owner.factionType.type == FACTION_TYPE.Elven_Kingdom)) : ((tileObjectRecipe.ingredient.ingredient != TILE_OBJECT_TYPE.STONE_PILE) ? p_settlement.HasTileObjectOfType(tileObjectRecipe.ingredient.ingredient) : (p_settlement.settlementJobTriggerComponent.HasAccessToResource(RESOURCE.STONE) || (p_settlement.owner != null && p_settlement.owner.factionType.type == FACTION_TYPE.Human_Empire))))
			{
				possibleRecipe = tileObjectRecipe;
				return true;
			}
		}
		possibleRecipe = mainRecipe;
		return false;
	}

	public TileObjectRecipe GetRecipeThatUses(TILE_OBJECT_TYPE tileObjectType)
	{
		for (int i = 0; i < craftRecipes.Length; i++)
		{
			TileObjectRecipe result = craftRecipes[i];
			if (result.UsesIngredient(tileObjectType))
			{
				return result;
			}
		}
		return mainRecipe;
	}
}

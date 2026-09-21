using System.Collections.Generic;

public static class TileObjectDB
{
	public static TileObjectData Default = new TileObjectData
	{
		maxHP = 200,
		neededCharacterClass = new string[1] { "Crafter" },
		occupiedSize = new Point(1, 1),
		constructionTimeInTicks = 10
	};

	public static Dictionary<TILE_OBJECT_TYPE, TileObjectData> tileObjectData = new Dictionary<TILE_OBJECT_TYPE, TileObjectData>
	{
		{
			TILE_OBJECT_TYPE.WOOD_PILE,
			new TileObjectData
			{
				maxHP = 600,
				neededCharacterClass = new string[1] { "Crafter" }
			}
		},
		{
			TILE_OBJECT_TYPE.ANIMAL_MEAT,
			new TileObjectData
			{
				maxHP = 300
			}
		},
		{
			TILE_OBJECT_TYPE.RAT_MEAT,
			new TileObjectData
			{
				maxHP = 300
			}
		},
		{
			TILE_OBJECT_TYPE.ELF_MEAT,
			new TileObjectData
			{
				maxHP = 300
			}
		},
		{
			TILE_OBJECT_TYPE.HUMAN_MEAT,
			new TileObjectData
			{
				maxHP = 300
			}
		},
		{
			TILE_OBJECT_TYPE.FISH_PILE,
			new TileObjectData
			{
				maxHP = 300
			}
		},
		{
			TILE_OBJECT_TYPE.STONE_PILE,
			new TileObjectData
			{
				maxHP = 600
			}
		},
		{
			TILE_OBJECT_TYPE.WURM_HOLE,
			new TileObjectData
			{
				maxHP = 1
			}
		},
		{
			TILE_OBJECT_TYPE.BED_CLINIC,
			new TileObjectData
			{
				maxHP = 400,
				craftResourceCost = 5,
				constructionTimeInTicks = 10
			}
		},
		{
			TILE_OBJECT_TYPE.BED,
			new TileObjectData
			{
				maxHP = 400,
				neededCharacterClass = new string[1] { "Crafter" },
				repairCost = 5,
				craftResourceCost = 15,
				constructionTimeInTicks = 10,
				craftRecipes = new TileObjectRecipe[2]
				{
					new TileObjectRecipe(new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.WOOD_PILE, 15)),
					new TileObjectRecipe(new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.STONE_PILE, 15))
				}
			}
		},
		{
			TILE_OBJECT_TYPE.DESK,
			new TileObjectData
			{
				maxHP = 250,
				neededCharacterClass = new string[1] { "Crafter" },
				repairCost = 5,
				constructionTimeInTicks = 10,
				craftRecipes = new TileObjectRecipe[2]
				{
					new TileObjectRecipe(new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.WOOD_PILE, 10)),
					new TileObjectRecipe(new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.STONE_PILE, 10))
				}
			}
		},
		{
			TILE_OBJECT_TYPE.GUITAR,
			new TileObjectData
			{
				maxHP = 120,
				neededCharacterClass = new string[1] { "Crafter" },
				repairCost = 5,
				craftResourceCost = 10,
				constructionTimeInTicks = 10,
				craftRecipes = new TileObjectRecipe[2]
				{
					new TileObjectRecipe(new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.WOOD_PILE, 10)),
					new TileObjectRecipe(new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.STONE_PILE, 10))
				}
			}
		},
		{
			TILE_OBJECT_TYPE.TABLE,
			new TileObjectData
			{
				maxHP = 250,
				neededCharacterClass = new string[1] { "Crafter" },
				repairCost = 5,
				craftResourceCost = 20,
				constructionTimeInTicks = 10,
				craftRecipes = new TileObjectRecipe[2]
				{
					new TileObjectRecipe(new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.WOOD_PILE, 20)),
					new TileObjectRecipe(new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.STONE_PILE, 20))
				}
			}
		},
		{
			TILE_OBJECT_TYPE.SMALL_TREE_OBJECT,
			new TileObjectData
			{
				maxHP = 600,
				neededCharacterClass = new string[1] { "Crafter" }
			}
		},
		{
			TILE_OBJECT_TYPE.BIG_TREE_OBJECT,
			new TileObjectData
			{
				maxHP = 1200,
				neededCharacterClass = new string[1] { "Crafter" },
				occupiedSize = new Point(2, 2)
			}
		},
		{
			TILE_OBJECT_TYPE.HEALING_POTION,
			new TileObjectData
			{
				maxHP = 150,
				neededCharacterClass = null,
				purchaseCost = 5,
				constructionTimeInTicks = 10,
				craftRecipes = new TileObjectRecipe[1]
				{
					new TileObjectRecipe(new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.HERB_PLANT, 1))
				}
			}
		},
		{
			TILE_OBJECT_TYPE.POISON_FLASK,
			new TileObjectData
			{
				maxHP = 150,
				neededCharacterClass = null,
				constructionTimeInTicks = 10,
				craftRecipes = new TileObjectRecipe[1]
				{
					new TileObjectRecipe(new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.WATER_FLASK, 1))
				}
			}
		},
		{
			TILE_OBJECT_TYPE.ANTIDOTE,
			new TileObjectData
			{
				maxHP = 150,
				neededCharacterClass = null,
				constructionTimeInTicks = 10,
				craftRecipes = new TileObjectRecipe[1]
				{
					new TileObjectRecipe(new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.POISON_FLASK, 1))
				}
			}
		},
		{
			TILE_OBJECT_TYPE.LOCUST_SWARM,
			new TileObjectData
			{
				maxHP = 100
			}
		},
		{
			TILE_OBJECT_TYPE.POISON_CLOUD,
			new TileObjectData
			{
				maxHP = 3000
			}
		},
		{
			TILE_OBJECT_TYPE.TORNADO,
			new TileObjectData
			{
				maxHP = 3000
			}
		},
		{
			TILE_OBJECT_TYPE.BALL_LIGHTNING,
			new TileObjectData
			{
				maxHP = 500
			}
		},
		{
			TILE_OBJECT_TYPE.FIRE_BALL,
			new TileObjectData
			{
				maxHP = 500
			}
		},
		{
			TILE_OBJECT_TYPE.FROSTY_FOG,
			new TileObjectData
			{
				maxHP = 3000
			}
		},
		{
			TILE_OBJECT_TYPE.VAPOR,
			new TileObjectData
			{
				maxHP = 3000
			}
		},
		{
			TILE_OBJECT_TYPE.PORTAL_TILE_OBJECT,
			new TileObjectData
			{
				maxHP = 15000,
				occupiedSize = new Point(4, 3)
			}
		},
		{
			TILE_OBJECT_TYPE.FIRE_CRYSTAL,
			new TileObjectData
			{
				maxHP = 300
			}
		},
		{
			TILE_OBJECT_TYPE.WATER_CRYSTAL,
			new TileObjectData
			{
				maxHP = 300
			}
		},
		{
			TILE_OBJECT_TYPE.ELECTRIC_CRYSTAL,
			new TileObjectData
			{
				maxHP = 300
			}
		},
		{
			TILE_OBJECT_TYPE.POISON_CRYSTAL,
			new TileObjectData
			{
				maxHP = 300
			}
		},
		{
			TILE_OBJECT_TYPE.ICE_CRYSTAL,
			new TileObjectData
			{
				maxHP = 300
			}
		},
		{
			TILE_OBJECT_TYPE.BLOCK_WALL,
			new TileObjectData
			{
				maxHP = 500
			}
		},
		{
			TILE_OBJECT_TYPE.ROCK,
			new TileObjectData
			{
				maxHP = 200
			}
		},
		{
			TILE_OBJECT_TYPE.MAGIC_CIRCLE,
			new TileObjectData
			{
				maxHP = 750
			}
		},
		{
			TILE_OBJECT_TYPE.ORE,
			new TileObjectData
			{
				maxHP = 350
			}
		},
		{
			TILE_OBJECT_TYPE.BERRY_SHRUB,
			new TileObjectData
			{
				maxHP = 100
			}
		},
		{
			TILE_OBJECT_TYPE.WATER_WELL,
			new TileObjectData
			{
				maxHP = 1000,
				repairCost = 5,
				constructionTimeInTicks = 10,
				craftRecipes = new TileObjectRecipe[2]
				{
					new TileObjectRecipe(new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.STONE_PILE, 30)),
					new TileObjectRecipe(new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.WOOD_PILE, 30))
				}
			}
		},
		{
			TILE_OBJECT_TYPE.TOMBSTONE,
			new TileObjectData
			{
				maxHP = 400
			}
		},
		{
			TILE_OBJECT_TYPE.MUSHROOM,
			new TileObjectData
			{
				maxHP = 100
			}
		},
		{
			TILE_OBJECT_TYPE.CORN_CROP,
			new TileObjectData
			{
				maxHP = 1000
			}
		},
		{
			TILE_OBJECT_TYPE.SMITHING_FORGE,
			new TileObjectData
			{
				maxHP = 350,
				constructionTimeInTicks = 10
			}
		},
		{
			TILE_OBJECT_TYPE.TABLE_HERBALISM,
			new TileObjectData
			{
				maxHP = 250,
				constructionTimeInTicks = 10
			}
		},
		{
			TILE_OBJECT_TYPE.TABLE_ALCHEMY,
			new TileObjectData
			{
				maxHP = 250,
				constructionTimeInTicks = 10
			}
		},
		{
			TILE_OBJECT_TYPE.CAULDRON,
			new TileObjectData
			{
				maxHP = 300,
				constructionTimeInTicks = 10
			}
		},
		{
			TILE_OBJECT_TYPE.HERB_PLANT,
			new TileObjectData
			{
				maxHP = 100
			}
		},
		{
			TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT,
			new TileObjectData
			{
				maxHP = 1000
			}
		},
		{
			TILE_OBJECT_TYPE.TOOL,
			new TileObjectData
			{
				maxHP = 150,
				constructionTimeInTicks = 10
			}
		},
		{
			TILE_OBJECT_TYPE.EMBER,
			new TileObjectData
			{
				maxHP = 150
			}
		},
		{
			TILE_OBJECT_TYPE.CINDER,
			new TileObjectData
			{
				maxHP = 1000
			}
		},
		{
			TILE_OBJECT_TYPE.ICE_BLOCK_WALL,
			new TileObjectData
			{
				maxHP = 1000
			}
		},
		{
			TILE_OBJECT_TYPE.WATER_FLASK,
			new TileObjectData
			{
				maxHP = 150,
				constructionTimeInTicks = 10
			}
		},
		{
			TILE_OBJECT_TYPE.WINTER_ROSE,
			new TileObjectData
			{
				maxHP = 700
			}
		},
		{
			TILE_OBJECT_TYPE.DESERT_ROSE,
			new TileObjectData
			{
				maxHP = 700
			}
		},
		{
			TILE_OBJECT_TYPE.RACK_STAVES,
			new TileObjectData
			{
				maxHP = 250,
				constructionTimeInTicks = 10
			}
		},
		{
			TILE_OBJECT_TYPE.QUICKSAND,
			new TileObjectData
			{
				maxHP = 3000
			}
		},
		{
			TILE_OBJECT_TYPE.SNOW_MOUND,
			new TileObjectData
			{
				maxHP = 200
			}
		},
		{
			TILE_OBJECT_TYPE.ICE,
			new TileObjectData
			{
				maxHP = 200
			}
		},
		{
			TILE_OBJECT_TYPE.TREASURE_CHEST,
			new TileObjectData
			{
				maxHP = 200
			}
		},
		{
			TILE_OBJECT_TYPE.CULTIST_KIT,
			new TileObjectData
			{
				maxHP = 150,
				neededCharacterClass = null,
				constructionTimeInTicks = 10,
				craftRecipes = new TileObjectRecipe[2]
				{
					new TileObjectRecipe(new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.STONE_PILE, 2)),
					new TileObjectRecipe(new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.WOOD_PILE, 2))
				}
			}
		},
		{
			TILE_OBJECT_TYPE.TORCH,
			new TileObjectData
			{
				maxHP = 200,
				neededCharacterClass = new string[1] { "Crafter" },
				repairCost = 5,
				craftResourceCost = 5,
				constructionTimeInTicks = 5,
				craftRecipes = new TileObjectRecipe[2]
				{
					new TileObjectRecipe(new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.WOOD_PILE, 5)),
					new TileObjectRecipe(new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.STONE_PILE, 5))
				}
			}
		},
		{
			TILE_OBJECT_TYPE.PHYLACTERY,
			new TileObjectData
			{
				maxHP = 200,
				neededCharacterClass = new string[1] { "Shaman" },
				repairCost = 5,
				constructionTimeInTicks = 10,
				craftRecipes = new TileObjectRecipe[2]
				{
					new TileObjectRecipe(new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.WOOD_PILE, 10)),
					new TileObjectRecipe(new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.STONE_PILE, 10))
				}
			}
		},
		{
			TILE_OBJECT_TYPE.DEMON_EYE,
			new TileObjectData
			{
				maxHP = 48
			}
		},
		{
			TILE_OBJECT_TYPE.IMP_HUT_TILE_OBJECT,
			new TileObjectData
			{
				maxHP = 10000,
				occupiedSize = new Point(6, 5)
			}
		},
		{
			TILE_OBJECT_TYPE.BIOLAB_TILE_OBJECT,
			new TileObjectData
			{
				maxHP = 10000,
				occupiedSize = new Point(6, 6)
			}
		},
		{
			TILE_OBJECT_TYPE.PRISM_TILE_OBJECT,
			new TileObjectData
			{
				maxHP = 10000,
				occupiedSize = new Point(4, 4)
			}
		},
		{
			TILE_OBJECT_TYPE.MANA_PIT_TILE_OBJECT,
			new TileObjectData
			{
				maxHP = 10000,
				occupiedSize = new Point(4, 4)
			}
		},
		{
			TILE_OBJECT_TYPE.MARAUD_TILE_OBJECT,
			new TileObjectData
			{
				maxHP = 10000,
				occupiedSize = new Point(5, 7)
			}
		},
		{
			TILE_OBJECT_TYPE.SPIRE_TILE_OBJECT,
			new TileObjectData
			{
				maxHP = 10000,
				occupiedSize = new Point(4, 7)
			}
		},
		{
			TILE_OBJECT_TYPE.WATCHER_TILE_OBJECT,
			new TileObjectData
			{
				maxHP = 10000,
				occupiedSize = new Point(5, 6)
			}
		},
		{
			TILE_OBJECT_TYPE.MEDDLER_TILE_OBJECT,
			new TileObjectData
			{
				maxHP = 10000,
				occupiedSize = new Point(5, 7)
			}
		},
		{
			TILE_OBJECT_TYPE.CRYPT_TILE_OBJECT,
			new TileObjectData
			{
				maxHP = 10000,
				occupiedSize = new Point(6, 4)
			}
		},
		{
			TILE_OBJECT_TYPE.DEFILER_TILE_OBJECT,
			new TileObjectData
			{
				maxHP = 10000,
				occupiedSize = new Point(3, 5)
			}
		},
		{
			TILE_OBJECT_TYPE.TORTURE_CHAMBERS_TILE_OBJECT,
			new TileObjectData
			{
				maxHP = 10000,
				occupiedSize = new Point(1, 1)
			}
		},
		{
			TILE_OBJECT_TYPE.STRUCTURE_BLOCKER_TILE_OBJECT,
			new TileObjectData
			{
				maxHP = 10000,
				occupiedSize = new Point(1, 1)
			}
		},
		{
			TILE_OBJECT_TYPE.KENNEL_TILE_OBJECT,
			new TileObjectData
			{
				maxHP = 10000,
				occupiedSize = new Point(1, 1)
			}
		},
		{
			TILE_OBJECT_TYPE.BOAR_DEN,
			new TileObjectData
			{
				maxHP = 3000,
				occupiedSize = new Point(2, 2)
			}
		},
		{
			TILE_OBJECT_TYPE.WOLF_DEN,
			new TileObjectData
			{
				maxHP = 3000,
				occupiedSize = new Point(2, 2)
			}
		},
		{
			TILE_OBJECT_TYPE.BEAR_DEN,
			new TileObjectData
			{
				maxHP = 3000,
				occupiedSize = new Point(2, 2)
			}
		},
		{
			TILE_OBJECT_TYPE.RABBIT_HOLE,
			new TileObjectData
			{
				maxHP = 3000,
				occupiedSize = new Point(2, 2)
			}
		},
		{
			TILE_OBJECT_TYPE.MINK_HOLE,
			new TileObjectData
			{
				maxHP = 3000,
				occupiedSize = new Point(2, 2)
			}
		},
		{
			TILE_OBJECT_TYPE.MOONCRAWLER_HOLE,
			new TileObjectData
			{
				maxHP = 3000,
				occupiedSize = new Point(2, 2)
			}
		},
		{
			TILE_OBJECT_TYPE.PRIMORDIAL_POOL_TILE_OBJECT,
			new TileObjectData
			{
				maxHP = 10000,
				occupiedSize = new Point(3, 3)
			}
		},
		{
			TILE_OBJECT_TYPE.TRAINING_DUMMY,
			new TileObjectData
			{
				maxHP = 200,
				repairCost = 5,
				craftResourceCost = 10,
				constructionTimeInTicks = 10,
				craftRecipes = new TileObjectRecipe[2]
				{
					new TileObjectRecipe(new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.WOOD_PILE, 10)),
					new TileObjectRecipe(new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.STONE_PILE, 10))
				}
			}
		},
		{
			TILE_OBJECT_TYPE.BOOK,
			new TileObjectData
			{
				maxHP = 200,
				repairCost = 5,
				craftResourceCost = 10,
				constructionTimeInTicks = 10,
				craftRecipes = new TileObjectRecipe[2]
				{
					new TileObjectRecipe(new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.WOOD_PILE, 10)),
					new TileObjectRecipe(new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.STONE_PILE, 10))
				}
			}
		},
		{
			TILE_OBJECT_TYPE.WATER_BASIN,
			new TileObjectData
			{
				maxHP = 200,
				repairCost = 5,
				craftResourceCost = 10,
				constructionTimeInTicks = 10,
				craftRecipes = new TileObjectRecipe[2]
				{
					new TileObjectRecipe(new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.WOOD_PILE, 10)),
					new TileObjectRecipe(new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.STONE_PILE, 10))
				}
			}
		},
		{
			TILE_OBJECT_TYPE.TOWN_MESSAGE_BOARD,
			new TileObjectData
			{
				maxHP = 1000,
				repairCost = 5,
				constructionTimeInTicks = 10,
				craftRecipes = new TileObjectRecipe[2]
				{
					new TileObjectRecipe(new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.STONE_PILE, 30)),
					new TileObjectRecipe(new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.WOOD_PILE, 30))
				}
			}
		},
		{
			TILE_OBJECT_TYPE.ARCHERY_TARGET,
			new TileObjectData
			{
				maxHP = 200,
				repairCost = 5,
				craftResourceCost = 10,
				constructionTimeInTicks = 10,
				craftRecipes = new TileObjectRecipe[2]
				{
					new TileObjectRecipe(new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.WOOD_PILE, 10)),
					new TileObjectRecipe(new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.STONE_PILE, 10))
				}
			}
		},
		{
			TILE_OBJECT_TYPE.DIVINE_ORB,
			new TileObjectData
			{
				maxHP = 200,
				repairCost = 5,
				craftResourceCost = 5,
				constructionTimeInTicks = 5,
				craftRecipes = new TileObjectRecipe[2]
				{
					new TileObjectRecipe(new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.WOOD_PILE, 5)),
					new TileObjectRecipe(new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.STONE_PILE, 5))
				}
			}
		},
		{
			TILE_OBJECT_TYPE.LEGENDARY_FORGE_TILE_OBJECT,
			new TileObjectData
			{
				maxHP = 8000,
				occupiedSize = new Point(1, 1)
			}
		},
		{
			TILE_OBJECT_TYPE.WARD_LIGHT,
			new TileObjectData
			{
				maxHP = 500,
				repairCost = 5,
				craftResourceCost = 10,
				constructionTimeInTicks = 10,
				craftRecipes = new TileObjectRecipe[2]
				{
					new TileObjectRecipe(new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.WOOD_PILE, 10)),
					new TileObjectRecipe(new TileObjectRecipeIngredient(TILE_OBJECT_TYPE.STONE_PILE, 10))
				}
			}
		}
	};

	public static bool HasTileObjectData(TILE_OBJECT_TYPE objType)
	{
		return tileObjectData.ContainsKey(objType);
	}

	public static TileObjectData GetTileObjectData(TILE_OBJECT_TYPE objType)
	{
		if (tileObjectData.ContainsKey(objType))
		{
			return tileObjectData[objType];
		}
		return Default;
	}

	public static bool OccupiesMoreThan1Tile(TILE_OBJECT_TYPE objType)
	{
		if (tileObjectData.ContainsKey(objType))
		{
			Point occupiedSize = tileObjectData[objType].occupiedSize;
			if (occupiedSize.X <= 1)
			{
				return occupiedSize.Y > 1;
			}
			return true;
		}
		return false;
	}

	public static bool TryGetTileObjectData(TILE_OBJECT_TYPE objType, out TileObjectData data)
	{
		if (tileObjectData.ContainsKey(objType))
		{
			data = tileObjectData[objType];
			return true;
		}
		data = Default;
		return false;
	}
}

using System;
using System.Linq;
using Inner_Maps;
using Locations.Settlements;
using Settings;
using Tutorial;
using UnityEngine;
using UtilityScripts;

public static class Extensions
{
	public static bool IsLessThan(this CRIME_SEVERITY sub, CRIME_SEVERITY other)
	{
		return sub < other;
	}

	public static bool IsGreaterThanOrEqual(this CRIME_SEVERITY sub, CRIME_SEVERITY other)
	{
		return sub >= other;
	}

	public static bool IsReligiousCrime(this CRIME_TYPE crimeType)
	{
		if ((uint)(crimeType - 11) <= 2u)
		{
			return true;
		}
		return false;
	}

	public static bool IsConsideredACrime(this CRIME_SEVERITY p_crimeSeverity)
	{
		if (p_crimeSeverity != CRIME_SEVERITY.None)
		{
			return p_crimeSeverity != CRIME_SEVERITY.Unapplicable;
		}
		return false;
	}

	public static bool IsOpenSpace(this STRUCTURE_TYPE sub)
	{
		switch (sub)
		{
		case STRUCTURE_TYPE.WILDERNESS:
		case STRUCTURE_TYPE.CEMETERY:
		case STRUCTURE_TYPE.CITY_CENTER:
		case STRUCTURE_TYPE.THE_PORTAL:
		case STRUCTURE_TYPE.OCEAN:
		case STRUCTURE_TYPE.MEDDLER:
		case STRUCTURE_TYPE.WATCHER:
		case STRUCTURE_TYPE.FARM:
		case STRUCTURE_TYPE.MINE:
		case STRUCTURE_TYPE.ANCIENT_GRAVEYARD:
		case STRUCTURE_TYPE.FISHERY:
		case STRUCTURE_TYPE.SPIRE:
		case STRUCTURE_TYPE.MANA_PIT:
		case STRUCTURE_TYPE.MARAUD:
		case STRUCTURE_TYPE.PRISM:
		case STRUCTURE_TYPE.IMP_HUT:
		case STRUCTURE_TYPE.BOAR_DEN:
		case STRUCTURE_TYPE.WOLF_DEN:
		case STRUCTURE_TYPE.BEAR_DEN:
		case STRUCTURE_TYPE.RABBIT_HOLE:
		case STRUCTURE_TYPE.MINK_HOLE:
		case STRUCTURE_TYPE.MOONCRAWLER_HOLE:
		case STRUCTURE_TYPE.BEAST_LAIR:
		case STRUCTURE_TYPE.DRAGON_LAIR:
		case STRUCTURE_TYPE.PRIMORDIAL_POOL:
		case STRUCTURE_TYPE.HALLOWED_GROUND:
		case STRUCTURE_TYPE.ARROW_TOWER:
		case STRUCTURE_TYPE.LIGHTNING_TOWER:
		case STRUCTURE_TYPE.LICH_GRAVEYARD:
			return true;
		default:
			return false;
		}
	}

	public static bool IsVillageStructure(this STRUCTURE_TYPE sub)
	{
		switch (sub)
		{
		case STRUCTURE_TYPE.TAVERN:
		case STRUCTURE_TYPE.DWELLING:
		case STRUCTURE_TYPE.CEMETERY:
		case STRUCTURE_TYPE.PRISON:
		case STRUCTURE_TYPE.CITY_CENTER:
		case STRUCTURE_TYPE.BARRACKS:
		case STRUCTURE_TYPE.HOSPICE:
		case STRUCTURE_TYPE.HUNTER_LODGE:
		case STRUCTURE_TYPE.MAGE_QUARTERS:
		case STRUCTURE_TYPE.FARM:
		case STRUCTURE_TYPE.LUMBERYARD:
		case STRUCTURE_TYPE.MINE:
		case STRUCTURE_TYPE.CULT_TEMPLE:
		case STRUCTURE_TYPE.QUARRY:
		case STRUCTURE_TYPE.WORKSHOP:
		case STRUCTURE_TYPE.TAILORING:
		case STRUCTURE_TYPE.TANNERY:
		case STRUCTURE_TYPE.FISHERY:
		case STRUCTURE_TYPE.BUTCHERS_SHOP:
		case STRUCTURE_TYPE.MAGIC_ACADEMY:
		case STRUCTURE_TYPE.WYVERN_COOP:
		case STRUCTURE_TYPE.ARROW_TOWER:
		case STRUCTURE_TYPE.LIGHTNING_TOWER:
		case STRUCTURE_TYPE.BEAST_PEN:
			return true;
		default:
			return false;
		}
	}

	public static bool IsFacilityStructure(this STRUCTURE_TYPE sub)
	{
		switch (sub)
		{
		case STRUCTURE_TYPE.TAVERN:
		case STRUCTURE_TYPE.WAREHOUSE:
		case STRUCTURE_TYPE.CEMETERY:
		case STRUCTURE_TYPE.CITY_CENTER:
		case STRUCTURE_TYPE.BARRACKS:
		case STRUCTURE_TYPE.HOSPICE:
		case STRUCTURE_TYPE.HUNTER_LODGE:
		case STRUCTURE_TYPE.MAGE_QUARTERS:
		case STRUCTURE_TYPE.FARM:
		case STRUCTURE_TYPE.LUMBERYARD:
		case STRUCTURE_TYPE.MINE:
		case STRUCTURE_TYPE.CULT_TEMPLE:
		case STRUCTURE_TYPE.QUARRY:
		case STRUCTURE_TYPE.WORKSHOP:
		case STRUCTURE_TYPE.TAILORING:
		case STRUCTURE_TYPE.TANNERY:
		case STRUCTURE_TYPE.FISHERY:
		case STRUCTURE_TYPE.BUTCHERS_SHOP:
		case STRUCTURE_TYPE.MAGIC_ACADEMY:
			return true;
		default:
			return false;
		}
	}

	public static bool IsResourceProducingStructure(this STRUCTURE_TYPE sub)
	{
		if (sub == STRUCTURE_TYPE.HUNTER_LODGE || (uint)(sub - 40) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static bool IsFoodProducingStructure(this STRUCTURE_TYPE sub)
	{
		if (sub == STRUCTURE_TYPE.FARM || sub == STRUCTURE_TYPE.FISHERY || sub == STRUCTURE_TYPE.BUTCHERS_SHOP)
		{
			return true;
		}
		return false;
	}

	public static bool IsPlayerStructure(this STRUCTURE_TYPE type)
	{
		switch (type)
		{
		case STRUCTURE_TYPE.THE_PORTAL:
		case STRUCTURE_TYPE.OSTRACIZER:
		case STRUCTURE_TYPE.KENNEL:
		case STRUCTURE_TYPE.CRYPT:
		case STRUCTURE_TYPE.MEDDLER:
		case STRUCTURE_TYPE.DEFILER:
		case STRUCTURE_TYPE.THE_ANVIL:
		case STRUCTURE_TYPE.WATCHER:
		case STRUCTURE_TYPE.THE_NEEDLES:
		case STRUCTURE_TYPE.TORTURE_CHAMBERS:
		case STRUCTURE_TYPE.BIOLAB:
		case STRUCTURE_TYPE.SPIRE:
		case STRUCTURE_TYPE.MANA_PIT:
		case STRUCTURE_TYPE.MARAUD:
		case STRUCTURE_TYPE.PRISM:
		case STRUCTURE_TYPE.IMP_HUT:
		case STRUCTURE_TYPE.PRIMORDIAL_POOL:
			return true;
		default:
			return false;
		}
	}

	public static int StructurePriority(this STRUCTURE_TYPE sub)
	{
		return sub switch
		{
			STRUCTURE_TYPE.WILDERNESS => -1, 
			STRUCTURE_TYPE.DWELLING => 0, 
			STRUCTURE_TYPE.CITY_CENTER => 1, 
			STRUCTURE_TYPE.TAVERN => 2, 
			STRUCTURE_TYPE.WAREHOUSE => 3, 
			STRUCTURE_TYPE.PRISON => 5, 
			_ => 99, 
		};
	}

	public static bool IsInterior(this STRUCTURE_TYPE structureType)
	{
		switch (structureType)
		{
		case STRUCTURE_TYPE.TAVERN:
		case STRUCTURE_TYPE.WAREHOUSE:
		case STRUCTURE_TYPE.DWELLING:
		case STRUCTURE_TYPE.PRISON:
		case STRUCTURE_TYPE.BARRACKS:
		case STRUCTURE_TYPE.HOSPICE:
		case STRUCTURE_TYPE.HUNTER_LODGE:
		case STRUCTURE_TYPE.MAGE_QUARTERS:
		case STRUCTURE_TYPE.MONSTER_LAIR:
		case STRUCTURE_TYPE.ABANDONED_MINE:
		case STRUCTURE_TYPE.MAGE_TOWER:
		case STRUCTURE_TYPE.CAVE:
		case STRUCTURE_TYPE.OSTRACIZER:
		case STRUCTURE_TYPE.KENNEL:
		case STRUCTURE_TYPE.CRYPT:
		case STRUCTURE_TYPE.MEDDLER:
		case STRUCTURE_TYPE.TORTURE_CHAMBERS:
		case STRUCTURE_TYPE.LUMBERYARD:
		case STRUCTURE_TYPE.MINE:
		case STRUCTURE_TYPE.TEMPLE:
		case STRUCTURE_TYPE.CULT_TEMPLE:
		case STRUCTURE_TYPE.BIOLAB:
		case STRUCTURE_TYPE.QUARRY:
		case STRUCTURE_TYPE.WORKSHOP:
		case STRUCTURE_TYPE.TAILORING:
		case STRUCTURE_TYPE.TANNERY:
		case STRUCTURE_TYPE.FISHERY:
		case STRUCTURE_TYPE.SPIRE:
		case STRUCTURE_TYPE.MANA_PIT:
		case STRUCTURE_TYPE.MARAUD:
		case STRUCTURE_TYPE.PRISM:
		case STRUCTURE_TYPE.IMP_HUT:
		case STRUCTURE_TYPE.BUTCHERS_SHOP:
		case STRUCTURE_TYPE.FIERY_CAVE:
		case STRUCTURE_TYPE.NOXIOUS_CAVE:
		case STRUCTURE_TYPE.BEAST_LAIR:
		case STRUCTURE_TYPE.DRAGON_LAIR:
		case STRUCTURE_TYPE.FROZEN_SHRINE:
		case STRUCTURE_TYPE.JUNGLE_RAMPART:
		case STRUCTURE_TYPE.PRIMORDIAL_POOL:
		case STRUCTURE_TYPE.MAGIC_ACADEMY:
		case STRUCTURE_TYPE.LICH_GRAVEYARD:
			return true;
		default:
			return false;
		}
	}

	public static bool IsSpecialStructure(this STRUCTURE_TYPE structureType)
	{
		switch (structureType)
		{
		case STRUCTURE_TYPE.MONSTER_LAIR:
		case STRUCTURE_TYPE.ABANDONED_MINE:
		case STRUCTURE_TYPE.ANCIENT_RUIN:
		case STRUCTURE_TYPE.MAGE_TOWER:
		case STRUCTURE_TYPE.CAVE:
		case STRUCTURE_TYPE.ANCIENT_GRAVEYARD:
		case STRUCTURE_TYPE.TEMPLE:
		case STRUCTURE_TYPE.FIERY_CAVE:
		case STRUCTURE_TYPE.NOXIOUS_CAVE:
		case STRUCTURE_TYPE.BEAST_LAIR:
		case STRUCTURE_TYPE.DRAGON_LAIR:
		case STRUCTURE_TYPE.DEAD_GROUNDS:
		case STRUCTURE_TYPE.FROZEN_SHRINE:
		case STRUCTURE_TYPE.JUNGLE_RAMPART:
		case STRUCTURE_TYPE.MUSHROOM_HAVEN:
		case STRUCTURE_TYPE.BERRY_GARDEN:
		case STRUCTURE_TYPE.MONSTER_CAMP:
		case STRUCTURE_TYPE.NECROMANCER_LAIR:
		case STRUCTURE_TYPE.HALLOWED_GROUND:
		case STRUCTURE_TYPE.LEGENDARY_FORGE:
		case STRUCTURE_TYPE.LICH_GRAVEYARD:
			return true;
		default:
			return false;
		}
	}

	public static bool IsBeastDen(this STRUCTURE_TYPE sub)
	{
		if ((uint)(sub - 59) <= 2u)
		{
			return true;
		}
		return false;
	}

	public static SettlementResources.StructureRequirement GetRequiredObjectForBuilding(this STRUCTURE_TYPE structureType)
	{
		switch (structureType)
		{
		case STRUCTURE_TYPE.QUARRY:
			return SettlementResources.StructureRequirement.ROCK;
		case STRUCTURE_TYPE.MINE:
			return SettlementResources.StructureRequirement.MINE_SHACK_SPOT;
		case STRUCTURE_TYPE.MONSTER_LAIR:
		case STRUCTURE_TYPE.ABANDONED_MINE:
		case STRUCTURE_TYPE.ANCIENT_RUIN:
		case STRUCTURE_TYPE.CAVE:
		case STRUCTURE_TYPE.ANCIENT_GRAVEYARD:
		case STRUCTURE_TYPE.TEMPLE:
			return SettlementResources.StructureRequirement.NONE;
		default:
			return SettlementResources.StructureRequirement.NONE;
		}
	}

	public static bool IsValidCenterTileForStructure(this STRUCTURE_TYPE structureType, LocationGridTile p_tile, BaseSettlement p_settlement)
	{
		if (structureType.IsVillageStructure() && p_settlement is NPCSettlement nPCSettlement)
		{
			if (!nPCSettlement.occupiedVillageSpot.reservedAreas.Contains(p_tile.area))
			{
				return !p_tile.area.IsReservedByOtherVillage(nPCSettlement.occupiedVillageSpot);
			}
			return true;
		}
		return true;
	}

	public static string LocalizedStructureName(this STRUCTURE_TYPE structureType)
	{
		return LocalizationManager.Instance.GetLocalizedValue("Structures_Table", structureType.ToStringEnum());
	}

	public static bool RequiresResourceToBuild(this STRUCTURE_TYPE structureType, RESOURCE resource)
	{
		return structureType.GetResourceBuildCost(resource) > 0;
	}

	public static int GetResourceBuildCost(this STRUCTURE_TYPE structureType, RESOURCE resource)
	{
		switch (structureType)
		{
		case STRUCTURE_TYPE.FARM:
		case STRUCTURE_TYPE.LUMBERYARD:
		case STRUCTURE_TYPE.MINE:
		case STRUCTURE_TYPE.FISHERY:
		case STRUCTURE_TYPE.BUTCHERS_SHOP:
			return 0;
		case STRUCTURE_TYPE.DWELLING:
			if (resource == RESOURCE.STONE)
			{
				return 40;
			}
			return 60;
		default:
			return 60;
		}
	}

	public static bool IsBasicResourceProducingStructureForFaction(this STRUCTURE_TYPE structureType, FACTION_TYPE p_factionType)
	{
		switch (p_factionType)
		{
		case FACTION_TYPE.Human_Empire:
			return structureType == STRUCTURE_TYPE.MINE;
		case FACTION_TYPE.Elven_Kingdom:
			return structureType == STRUCTURE_TYPE.LUMBERYARD;
		case FACTION_TYPE.Vampire_Clan:
		case FACTION_TYPE.Lycan_Clan:
		case FACTION_TYPE.Demon_Cult:
			if (structureType != STRUCTURE_TYPE.LUMBERYARD)
			{
				return structureType == STRUCTURE_TYPE.MINE;
			}
			return true;
		default:
			return false;
		}
	}

	public static BIOMES GetNeededBiomeForSpecialStructure(this STRUCTURE_TYPE p_structureType)
	{
		switch (p_structureType)
		{
		case STRUCTURE_TYPE.FROZEN_SHRINE:
			return BIOMES.SNOW;
		case STRUCTURE_TYPE.JUNGLE_RAMPART:
		case STRUCTURE_TYPE.MONSTER_CAMP:
			return BIOMES.GRASSLAND;
		case STRUCTURE_TYPE.FIERY_CAVE:
			return BIOMES.DESERT;
		default:
			return BIOMES.NONE;
		}
	}

	public static bool CanSpecialStructureBePlacedOnBiome(this STRUCTURE_TYPE p_structureType, BIOMES p_biome)
	{
		switch (p_structureType)
		{
		case STRUCTURE_TYPE.MONSTER_LAIR:
			if (p_biome != BIOMES.GRASSLAND)
			{
				return p_biome == BIOMES.FOREST;
			}
			return true;
		case STRUCTURE_TYPE.ABANDONED_MINE:
			if (p_biome != BIOMES.GRASSLAND)
			{
				return p_biome == BIOMES.SNOW;
			}
			return true;
		case STRUCTURE_TYPE.TEMPLE:
			if (p_biome != BIOMES.FOREST)
			{
				return p_biome == BIOMES.SNOW;
			}
			return true;
		case STRUCTURE_TYPE.MAGE_TOWER:
			if (p_biome != BIOMES.DESERT)
			{
				return p_biome == BIOMES.GRASSLAND;
			}
			return true;
		case STRUCTURE_TYPE.ANCIENT_RUIN:
			if (p_biome != BIOMES.FOREST)
			{
				return p_biome == BIOMES.SNOW;
			}
			return true;
		case STRUCTURE_TYPE.ANCIENT_GRAVEYARD:
			if (p_biome != BIOMES.SNOW)
			{
				return p_biome == BIOMES.DESERT;
			}
			return true;
		case STRUCTURE_TYPE.NOXIOUS_CAVE:
			if (p_biome != BIOMES.DESERT)
			{
				return p_biome == BIOMES.FOREST;
			}
			return true;
		case STRUCTURE_TYPE.FIERY_CAVE:
			if (p_biome != BIOMES.DESERT)
			{
				return p_biome == BIOMES.GRASSLAND;
			}
			return true;
		case STRUCTURE_TYPE.MONSTER_CAMP:
			if (p_biome != BIOMES.FOREST)
			{
				return p_biome == BIOMES.GRASSLAND;
			}
			return true;
		case STRUCTURE_TYPE.JUNGLE_RAMPART:
			return p_biome == BIOMES.FOREST;
		case STRUCTURE_TYPE.FROZEN_SHRINE:
			return p_biome == BIOMES.SNOW;
		case STRUCTURE_TYPE.BEAST_LAIR:
			if (p_biome != BIOMES.DESERT)
			{
				return p_biome == BIOMES.SNOW;
			}
			return true;
		case STRUCTURE_TYPE.DRAGON_LAIR:
			if (p_biome != BIOMES.DESERT)
			{
				return p_biome == BIOMES.FOREST;
			}
			return true;
		case STRUCTURE_TYPE.DEAD_GROUNDS:
			if (p_biome != BIOMES.GRASSLAND)
			{
				return p_biome == BIOMES.SNOW;
			}
			return true;
		case STRUCTURE_TYPE.MUSHROOM_HAVEN:
			if (p_biome != BIOMES.SNOW && p_biome != BIOMES.DESERT)
			{
				return p_biome == BIOMES.TUNDRA;
			}
			return true;
		case STRUCTURE_TYPE.BERRY_GARDEN:
			if (p_biome != BIOMES.FOREST && p_biome != BIOMES.GRASSLAND && p_biome != BIOMES.SNOW && p_biome != BIOMES.DESERT)
			{
				return p_biome == BIOMES.TUNDRA;
			}
			return true;
		case STRUCTURE_TYPE.HALLOWED_GROUND:
			if (p_biome != BIOMES.FOREST)
			{
				return p_biome == BIOMES.GRASSLAND;
			}
			return true;
		default:
			return true;
		}
	}

	public static bool ShouldSpecialStructureBeUnique(this STRUCTURE_TYPE p_structureType)
	{
		if (p_structureType == STRUCTURE_TYPE.HALLOWED_GROUND || p_structureType == STRUCTURE_TYPE.LEGENDARY_FORGE)
		{
			return true;
		}
		return false;
	}

	public static Cardinal_Direction OppositeDirection(this Cardinal_Direction dir)
	{
		return dir switch
		{
			Cardinal_Direction.North => Cardinal_Direction.South, 
			Cardinal_Direction.South => Cardinal_Direction.North, 
			Cardinal_Direction.East => Cardinal_Direction.West, 
			Cardinal_Direction.West => Cardinal_Direction.East, 
			_ => throw new Exception($"No opposite direction for {dir}"), 
		};
	}

	public static bool IsCardinalDirection(this GridNeighbourDirection dir)
	{
		if ((uint)dir <= 3u)
		{
			return true;
		}
		return false;
	}

	public static bool IsRestingAction(this INTERACTION_TYPE p_type)
	{
		switch (p_type)
		{
		case INTERACTION_TYPE.SLEEP:
		case INTERACTION_TYPE.SLEEP_OUTSIDE:
		case INTERACTION_TYPE.NAP:
		case INTERACTION_TYPE.NARCOLEPTIC_NAP:
			return true;
		default:
			return false;
		}
	}

	public static bool IsReturnHome(this INTERACTION_TYPE p_type)
	{
		return p_type == INTERACTION_TYPE.RETURN_HOME;
	}

	public static string LocalizedActionName(this INTERACTION_TYPE interactionType)
	{
		return LocalizationManager.Instance.GetLocalizedValue("GoapActionsStrings_Table", interactionType.ToStringEnum());
	}

	public static bool IsCombatState(this CHARACTER_STATE type)
	{
		if (type == CHARACTER_STATE.COMBAT)
		{
			return true;
		}
		return false;
	}

	public static bool CanBeCraftedBy(this TILE_OBJECT_TYPE type, Character character)
	{
		if (type == TILE_OBJECT_TYPE.NONE)
		{
			return false;
		}
		TileObjectData tileObjectData = TileObjectDB.GetTileObjectData(type);
		if (tileObjectData.neededCharacterClass == null || tileObjectData.neededCharacterClass.Length == 0)
		{
			return true;
		}
		return tileObjectData.neededCharacterClass.Contains(character.characterClass.className);
	}

	public static bool IsArtifact(this TILE_OBJECT_TYPE tileObjectType, out ARTIFACT_TYPE artifactType)
	{
		switch (tileObjectType)
		{
		case TILE_OBJECT_TYPE.NECRONOMICON:
			artifactType = ARTIFACT_TYPE.Necronomicon;
			return true;
		case TILE_OBJECT_TYPE.ANKH_OF_ANUBIS:
			artifactType = ARTIFACT_TYPE.Ankh_Of_Anubis;
			return true;
		case TILE_OBJECT_TYPE.BERSERK_ORB:
			artifactType = ARTIFACT_TYPE.Berserk_Orb;
			return true;
		default:
			artifactType = ARTIFACT_TYPE.None;
			return false;
		}
	}

	public static bool IsTileObjectAnItem(this TILE_OBJECT_TYPE tileObjectType)
	{
		switch (tileObjectType)
		{
		case TILE_OBJECT_TYPE.HEALING_POTION:
		case TILE_OBJECT_TYPE.TOOL:
		case TILE_OBJECT_TYPE.WATER_FLASK:
		case TILE_OBJECT_TYPE.ARTIFACT:
		case TILE_OBJECT_TYPE.HERB_PLANT:
		case TILE_OBJECT_TYPE.POISON_FLASK:
		case TILE_OBJECT_TYPE.EMBER:
		case TILE_OBJECT_TYPE.ANTIDOTE:
		case TILE_OBJECT_TYPE.FIRE_CRYSTAL:
		case TILE_OBJECT_TYPE.ICE_CRYSTAL:
		case TILE_OBJECT_TYPE.WATER_CRYSTAL:
		case TILE_OBJECT_TYPE.POISON_CRYSTAL:
		case TILE_OBJECT_TYPE.ELECTRIC_CRYSTAL:
		case TILE_OBJECT_TYPE.ICE:
		case TILE_OBJECT_TYPE.CULTIST_KIT:
		case TILE_OBJECT_TYPE.EXCALIBUR:
		case TILE_OBJECT_TYPE.WEREWOLF_PELT:
		case TILE_OBJECT_TYPE.PHYLACTERY:
		case TILE_OBJECT_TYPE.COPPER_SWORD:
		case TILE_OBJECT_TYPE.IRON_SWORD:
		case TILE_OBJECT_TYPE.RING:
		case TILE_OBJECT_TYPE.BRACER:
		case TILE_OBJECT_TYPE.MINK_SHIRT:
		case TILE_OBJECT_TYPE.MITHRIL_SWORD:
		case TILE_OBJECT_TYPE.ORICHALCUM_SWORD:
		case TILE_OBJECT_TYPE.COPPER_AXE:
		case TILE_OBJECT_TYPE.IRON_AXE:
		case TILE_OBJECT_TYPE.MITHRIL_AXE:
		case TILE_OBJECT_TYPE.ORICHALCUM_AXE:
		case TILE_OBJECT_TYPE.COPPER_BOW:
		case TILE_OBJECT_TYPE.IRON_BOW:
		case TILE_OBJECT_TYPE.MITHRIL_BOW:
		case TILE_OBJECT_TYPE.ORICHALCUM_BOW:
		case TILE_OBJECT_TYPE.COPPER_STAFF:
		case TILE_OBJECT_TYPE.IRON_STAFF:
		case TILE_OBJECT_TYPE.MITHRIL_STAFF:
		case TILE_OBJECT_TYPE.ORICHALCUM_STAFF:
		case TILE_OBJECT_TYPE.COPPER_DAGGER:
		case TILE_OBJECT_TYPE.IRON_DAGGER:
		case TILE_OBJECT_TYPE.MITHRIL_DAGGER:
		case TILE_OBJECT_TYPE.ORICHALCUM_DAGGER:
		case TILE_OBJECT_TYPE.RABBIT_SHIRT:
		case TILE_OBJECT_TYPE.WOOL_SHIRT:
		case TILE_OBJECT_TYPE.SPIDER_SILK_SHIRT:
		case TILE_OBJECT_TYPE.MOONWALKER_SHIRT:
		case TILE_OBJECT_TYPE.BOAR_HIDE_ARMOR:
		case TILE_OBJECT_TYPE.WOLF_HIDE_ARMOR:
		case TILE_OBJECT_TYPE.BEAR_HIDE_ARMOR:
		case TILE_OBJECT_TYPE.SCALE_ARMOR:
		case TILE_OBJECT_TYPE.DRAGON_ARMOR:
		case TILE_OBJECT_TYPE.COPPER_ARMOR:
		case TILE_OBJECT_TYPE.IRON_ARMOR:
		case TILE_OBJECT_TYPE.MITHRIL_ARMOR:
		case TILE_OBJECT_TYPE.ORICHALCUM_ARMOR:
		case TILE_OBJECT_TYPE.NECKLACE:
		case TILE_OBJECT_TYPE.BELT:
		case TILE_OBJECT_TYPE.SCROLL:
		case TILE_OBJECT_TYPE.BASIC_SWORD:
		case TILE_OBJECT_TYPE.BASIC_AXE:
		case TILE_OBJECT_TYPE.BASIC_DAGGER:
		case TILE_OBJECT_TYPE.BASIC_STAFF:
		case TILE_OBJECT_TYPE.BASIC_SHIRT:
		case TILE_OBJECT_TYPE.BASIC_BOW:
		case TILE_OBJECT_TYPE.EXCALIBUR_SWORD:
		case TILE_OBJECT_TYPE.MANTRA:
		case TILE_OBJECT_TYPE.FANG:
		case TILE_OBJECT_TYPE.SAVAGE:
		case TILE_OBJECT_TYPE.ECLIPSE:
		case TILE_OBJECT_TYPE.CITRUS:
			return true;
		default:
			return false;
		}
	}

	public static bool IsTileObjectVisibleByDefault(this TILE_OBJECT_TYPE tileObjectType)
	{
		switch (tileObjectType)
		{
		case TILE_OBJECT_TYPE.WOOD_PILE:
		case TILE_OBJECT_TYPE.GUITAR:
		case TILE_OBJECT_TYPE.TABLE:
		case TILE_OBJECT_TYPE.TOMBSTONE:
		case TILE_OBJECT_TYPE.MUSHROOM:
		case TILE_OBJECT_TYPE.ANIMAL_MEAT:
		case TILE_OBJECT_TYPE.STONE_PILE:
		case TILE_OBJECT_TYPE.RAVENOUS_SPIRIT:
		case TILE_OBJECT_TYPE.FEEBLE_SPIRIT:
		case TILE_OBJECT_TYPE.FORLORN_SPIRIT:
		case TILE_OBJECT_TYPE.TREASURE_CHEST:
		case TILE_OBJECT_TYPE.PORTAL_TILE_OBJECT:
		case TILE_OBJECT_TYPE.ELF_MEAT:
		case TILE_OBJECT_TYPE.HUMAN_MEAT:
		case TILE_OBJECT_TYPE.FISH_PILE:
		case TILE_OBJECT_TYPE.EXCALIBUR:
		case TILE_OBJECT_TYPE.HEIRLOOM:
		case TILE_OBJECT_TYPE.FISHING_SPOT:
		case TILE_OBJECT_TYPE.RAT_MEAT:
		case TILE_OBJECT_TYPE.SPIRE_TILE_OBJECT:
		case TILE_OBJECT_TYPE.MANA_PIT_TILE_OBJECT:
		case TILE_OBJECT_TYPE.MARAUD_TILE_OBJECT:
		case TILE_OBJECT_TYPE.PRISM_TILE_OBJECT:
		case TILE_OBJECT_TYPE.WATCHER_TILE_OBJECT:
		case TILE_OBJECT_TYPE.BIOLAB_TILE_OBJECT:
		case TILE_OBJECT_TYPE.IMP_HUT_TILE_OBJECT:
		case TILE_OBJECT_TYPE.MEDDLER_TILE_OBJECT:
		case TILE_OBJECT_TYPE.CRYPT_TILE_OBJECT:
		case TILE_OBJECT_TYPE.TORTURE_CHAMBERS_TILE_OBJECT:
		case TILE_OBJECT_TYPE.KENNEL_TILE_OBJECT:
		case TILE_OBJECT_TYPE.POWER_CRYSTAL:
		case TILE_OBJECT_TYPE.CORN:
		case TILE_OBJECT_TYPE.POTATO:
		case TILE_OBJECT_TYPE.PINEAPPLE:
		case TILE_OBJECT_TYPE.ICEBERRY:
		case TILE_OBJECT_TYPE.HYPNO_HERB:
		case TILE_OBJECT_TYPE.PRIMORDIAL_POOL_TILE_OBJECT:
		case TILE_OBJECT_TYPE.MONSTER_SPAWNER:
		case TILE_OBJECT_TYPE.EXCALIBUR_SWORD:
		case TILE_OBJECT_TYPE.LUNCH_PACK:
		case TILE_OBJECT_TYPE.STRUCTURE_SCROLL:
		case TILE_OBJECT_TYPE.CINDER:
			return true;
		default:
			return tileObjectType.IsTileObjectAnItem();
		}
	}

	public static bool IsDemonicStructureTileObject(this TILE_OBJECT_TYPE tileObjectType)
	{
		switch (tileObjectType)
		{
		case TILE_OBJECT_TYPE.PORTAL_TILE_OBJECT:
		case TILE_OBJECT_TYPE.SPIRE_TILE_OBJECT:
		case TILE_OBJECT_TYPE.MANA_PIT_TILE_OBJECT:
		case TILE_OBJECT_TYPE.MARAUD_TILE_OBJECT:
		case TILE_OBJECT_TYPE.PRISM_TILE_OBJECT:
		case TILE_OBJECT_TYPE.WATCHER_TILE_OBJECT:
		case TILE_OBJECT_TYPE.BIOLAB_TILE_OBJECT:
		case TILE_OBJECT_TYPE.IMP_HUT_TILE_OBJECT:
		case TILE_OBJECT_TYPE.MEDDLER_TILE_OBJECT:
		case TILE_OBJECT_TYPE.CRYPT_TILE_OBJECT:
		case TILE_OBJECT_TYPE.DEFILER_TILE_OBJECT:
		case TILE_OBJECT_TYPE.TORTURE_CHAMBERS_TILE_OBJECT:
		case TILE_OBJECT_TYPE.KENNEL_TILE_OBJECT:
		case TILE_OBJECT_TYPE.PRIMORDIAL_POOL_TILE_OBJECT:
			return true;
		default:
			return false;
		}
	}

	public static bool CanBeRepaired(this TILE_OBJECT_TYPE tileObjectType)
	{
		switch (tileObjectType)
		{
		case TILE_OBJECT_TYPE.GUITAR:
		case TILE_OBJECT_TYPE.TABLE:
		case TILE_OBJECT_TYPE.BED:
		case TILE_OBJECT_TYPE.DESK:
		case TILE_OBJECT_TYPE.WATER_WELL:
		case TILE_OBJECT_TYPE.TORCH:
		case TILE_OBJECT_TYPE.DIVINE_ORB:
			return true;
		default:
			return false;
		}
	}

	public static bool IsTileObjectImportant(this TILE_OBJECT_TYPE tileObjectType)
	{
		switch (tileObjectType)
		{
		case TILE_OBJECT_TYPE.GUITAR:
		case TILE_OBJECT_TYPE.MAGIC_CIRCLE:
		case TILE_OBJECT_TYPE.TABLE:
		case TILE_OBJECT_TYPE.BED:
		case TILE_OBJECT_TYPE.DESK:
		case TILE_OBJECT_TYPE.WATER_WELL:
		case TILE_OBJECT_TYPE.HEALING_POTION:
		case TILE_OBJECT_TYPE.TOOL:
		case TILE_OBJECT_TYPE.WATER_FLASK:
		case TILE_OBJECT_TYPE.ARTIFACT:
		case TILE_OBJECT_TYPE.BLOCK_WALL:
		case TILE_OBJECT_TYPE.TREASURE_CHEST:
		case TILE_OBJECT_TYPE.POISON_FLASK:
		case TILE_OBJECT_TYPE.EMBER:
		case TILE_OBJECT_TYPE.ANTIDOTE:
		case TILE_OBJECT_TYPE.ICE:
		case TILE_OBJECT_TYPE.DESERT_ROSE:
		case TILE_OBJECT_TYPE.CULTIST_KIT:
		case TILE_OBJECT_TYPE.EXCALIBUR:
		case TILE_OBJECT_TYPE.COPPER_SWORD:
		case TILE_OBJECT_TYPE.IRON_SWORD:
		case TILE_OBJECT_TYPE.RING:
		case TILE_OBJECT_TYPE.BRACER:
		case TILE_OBJECT_TYPE.MINK_SHIRT:
		case TILE_OBJECT_TYPE.MITHRIL_SWORD:
		case TILE_OBJECT_TYPE.ORICHALCUM_SWORD:
		case TILE_OBJECT_TYPE.COPPER_AXE:
		case TILE_OBJECT_TYPE.IRON_AXE:
		case TILE_OBJECT_TYPE.MITHRIL_AXE:
		case TILE_OBJECT_TYPE.ORICHALCUM_AXE:
		case TILE_OBJECT_TYPE.COPPER_BOW:
		case TILE_OBJECT_TYPE.IRON_BOW:
		case TILE_OBJECT_TYPE.MITHRIL_BOW:
		case TILE_OBJECT_TYPE.ORICHALCUM_BOW:
		case TILE_OBJECT_TYPE.COPPER_STAFF:
		case TILE_OBJECT_TYPE.IRON_STAFF:
		case TILE_OBJECT_TYPE.MITHRIL_STAFF:
		case TILE_OBJECT_TYPE.ORICHALCUM_STAFF:
		case TILE_OBJECT_TYPE.COPPER_DAGGER:
		case TILE_OBJECT_TYPE.IRON_DAGGER:
		case TILE_OBJECT_TYPE.MITHRIL_DAGGER:
		case TILE_OBJECT_TYPE.ORICHALCUM_DAGGER:
		case TILE_OBJECT_TYPE.RABBIT_SHIRT:
		case TILE_OBJECT_TYPE.WOOL_SHIRT:
		case TILE_OBJECT_TYPE.SPIDER_SILK_SHIRT:
		case TILE_OBJECT_TYPE.MOONWALKER_SHIRT:
		case TILE_OBJECT_TYPE.BOAR_HIDE_ARMOR:
		case TILE_OBJECT_TYPE.WOLF_HIDE_ARMOR:
		case TILE_OBJECT_TYPE.BEAR_HIDE_ARMOR:
		case TILE_OBJECT_TYPE.SCALE_ARMOR:
		case TILE_OBJECT_TYPE.DRAGON_ARMOR:
		case TILE_OBJECT_TYPE.COPPER_ARMOR:
		case TILE_OBJECT_TYPE.IRON_ARMOR:
		case TILE_OBJECT_TYPE.MITHRIL_ARMOR:
		case TILE_OBJECT_TYPE.ORICHALCUM_ARMOR:
		case TILE_OBJECT_TYPE.NECKLACE:
		case TILE_OBJECT_TYPE.BELT:
		case TILE_OBJECT_TYPE.SCROLL:
		case TILE_OBJECT_TYPE.BASIC_SWORD:
		case TILE_OBJECT_TYPE.BASIC_AXE:
		case TILE_OBJECT_TYPE.BASIC_DAGGER:
		case TILE_OBJECT_TYPE.BASIC_STAFF:
		case TILE_OBJECT_TYPE.BASIC_SHIRT:
		case TILE_OBJECT_TYPE.BASIC_BOW:
		case TILE_OBJECT_TYPE.EXCALIBUR_SWORD:
		case TILE_OBJECT_TYPE.ICE_BLOCK_WALL:
		case TILE_OBJECT_TYPE.CINDER:
			return true;
		default:
			return false;
		}
	}

	public static string LocalizedName(this TILE_OBJECT_TYPE p_type)
	{
		if (p_type == TILE_OBJECT_TYPE.STAMPEDE)
		{
			string key = p_type.ToStringEnumWithSpace();
			return LocalizationManager.Instance.GetLocalizedValue("PlayerPowers_Table", key);
		}
		string key2 = p_type.ToStringEnumWithSpace();
		return LocalizationManager.Instance.GetLocalizedValue("TileObjects_Table", key2);
	}

	public static bool IsNeedsTypeJob(this JOB_TYPE type)
	{
		if ((uint)(type - 2) <= 4u || type == JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT)
		{
			return true;
		}
		return false;
	}

	public static bool IsApprehendTypeJob(this JOB_TYPE type)
	{
		if (type == JOB_TYPE.APPREHEND || type == JOB_TYPE.APPREHEND_RESTRAINED)
		{
			return true;
		}
		return false;
	}

	public static bool IsMetal(this TILE_OBJECT_TYPE type)
	{
		if ((uint)(type - 224) <= 3u)
		{
			return true;
		}
		return false;
	}

	public static bool IsCloth(this TILE_OBJECT_TYPE type)
	{
		if ((uint)(type - 233) <= 2u || type == TILE_OBJECT_TYPE.RABBIT_CLOTH || type == TILE_OBJECT_TYPE.SPIDER_SILK)
		{
			return true;
		}
		return false;
	}

	public static bool IsLeather(this TILE_OBJECT_TYPE type)
	{
		if ((uint)(type - 236) <= 3u || type == TILE_OBJECT_TYPE.SCALE_HIDE)
		{
			return true;
		}
		return false;
	}

	public static bool IsFullnessRecoveryTypeJob(this JOB_TYPE type)
	{
		if (type == JOB_TYPE.FULLNESS_RECOVERY_URGENT || type == JOB_TYPE.FULLNESS_RECOVERY_NORMAL || type == JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT)
		{
			return true;
		}
		return false;
	}

	public static bool IsTirednessRecoveryTypeJob(this JOB_TYPE type)
	{
		if (type == JOB_TYPE.ENERGY_RECOVERY_URGENT || type == JOB_TYPE.ENERGY_RECOVERY_NORMAL)
		{
			return true;
		}
		return false;
	}

	public static bool IsHappinessRecoveryTypeJob(this JOB_TYPE type)
	{
		if (type == JOB_TYPE.HAPPINESS_RECOVERY)
		{
			return true;
		}
		return false;
	}

	public static bool ShouldOnlyBeDoneIfTargetIsNearby(this JOB_TYPE type)
	{
		if (type == JOB_TYPE.TAKE_ITEM_ON_SIGHT || type == JOB_TYPE.OPEN_CHEST || type == JOB_TYPE.STEAL_RAID)
		{
			return true;
		}
		return false;
	}

	public static bool ShouldIgnoreHostilesIfCarryingPOI(this JOB_TYPE type)
	{
		switch (type)
		{
		case JOB_TYPE.ABDUCT:
		case JOB_TYPE.MONSTER_ABDUCT:
		case JOB_TYPE.FACTION_KIDNAP:
		case JOB_TYPE.SNATCH:
		case JOB_TYPE.CAPTURE_CHARACTER:
		case JOB_TYPE.IMPRISON_BLOOD_SOURCE:
		case JOB_TYPE.STEAL_CORPSE:
		case JOB_TYPE.TRITON_KIDNAP:
		case JOB_TYPE.RESCUE_MOVE_CHARACTER:
		case JOB_TYPE.DEMON_STEAL:
			return true;
		default:
			return false;
		}
	}

	public static int GetJobTypePriority(this JOB_TYPE jobType)
	{
		int result = 0;
		switch (jobType)
		{
		case JOB_TYPE.DIG_THROUGH:
			result = 1300;
			break;
		case JOB_TYPE.FLEE_TO_HOME:
		case JOB_TYPE.FLEE_CRIME:
			result = 1200;
			break;
		case JOB_TYPE.NEUTRALIZE_DANGER:
			result = 1100;
			break;
		case JOB_TYPE.COMBAT:
			result = 1090;
			break;
		case JOB_TYPE.BERSERK_ATTACK:
		case JOB_TYPE.BERSERK_STROLL:
		case JOB_TYPE.DOUSE_FIRE_SELF:
			result = 1089;
			break;
		case JOB_TYPE.NO_PATH_IDLE:
		case JOB_TYPE.SACRIFICE_SELF:
		case JOB_TYPE.CULTIST_INSTRUCTION:
		case JOB_TYPE.CRITICAL_BREAK:
			result = 1088;
			break;
		case JOB_TYPE.BUILD_CAMP:
		case JOB_TYPE.LYCAN_HUNT_PREY:
		case JOB_TYPE.MONSTER_EAT_CORPSE:
		case JOB_TYPE.SACRIFICE:
		case JOB_TYPE.TRIGGER_AROUSAL:
		case JOB_TYPE.GRUDGE:
		case JOB_TYPE.MANIFEST_FOOD_EAT:
			result = 1087;
			break;
		case JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT:
		case JOB_TYPE.DISPOSE_FOOD_PILE:
			result = 1086;
			break;
		case JOB_TYPE.DESTROY:
		case JOB_TYPE.RETURN_STOLEN_THING:
			result = 1085;
			break;
		case JOB_TYPE.REPORT_CORRUPTED_STRUCTURE:
		case JOB_TYPE.COUNTERATTACK:
		case JOB_TYPE.SEARCH_FOR_DEMONIC_AREA:
			result = 1080;
			break;
		case JOB_TYPE.RETURN_HOME_URGENT:
		case JOB_TYPE.STALKER_HUNT:
		case JOB_TYPE.PURIFY:
			result = 1055;
			break;
		case JOB_TYPE.TRIGGER_FLAW:
		case JOB_TYPE.REPORT_CRIME:
		case JOB_TYPE.PREACH:
		case JOB_TYPE.FIND_NEW_VILLAGE:
		case JOB_TYPE.STEAL_CORPSE:
		case JOB_TYPE.SUMMON_BONE_GOLEM:
		case JOB_TYPE.KLEPTOMANIAC_STEAL:
		case JOB_TYPE.LAZY_NAP:
		case JOB_TYPE.FIND_AFFAIR:
		case JOB_TYPE.AGITATED:
		case JOB_TYPE.ENHANCE_RELATIONSHIP:
		case JOB_TYPE.OPINION_REDUCTION_REACTION:
		case JOB_TYPE.CLAIM_LOCATION:
		case JOB_TYPE.CLEANSE_HALLOWED_GROUND:
		case JOB_TYPE.MUMMIFY:
		case JOB_TYPE.ASSASSINATE:
			result = 1050;
			break;
		case JOB_TYPE.HIDE_AT_HOME:
		case JOB_TYPE.SEEK_SHELTER:
		case JOB_TYPE.PURIFY_GROUND:
			result = 1040;
			break;
		case JOB_TYPE.APPREHEND:
			result = 1030;
			break;
		case JOB_TYPE.SCREAM:
			result = 1020;
			break;
		case JOB_TYPE.RELEASE_CHARACTER:
			result = 1015;
			break;
		case JOB_TYPE.BURY_SERIAL_KILLER_VICTIM:
			result = 1010;
			break;
		case JOB_TYPE.OFFER_BLOOD:
			result = 1009;
			break;
		case JOB_TYPE.REMOVE_STATUS:
		case JOB_TYPE.CURE_MAGICAL_AFFLICTION:
		case JOB_TYPE.REMOVE_TRAP:
			result = 1008;
			break;
		case JOB_TYPE.RECOVER_HP:
			result = 1005;
			break;
		case JOB_TYPE.FEED:
		case JOB_TYPE.RITUAL_KILLING:
		case JOB_TYPE.OBTAIN_PERSONAL_FOOD:
		case JOB_TYPE.MONSTER_ABDUCT:
		case JOB_TYPE.ARSON:
			result = 1003;
			break;
		case JOB_TYPE.ENERGY_RECOVERY_URGENT:
		case JOB_TYPE.FULLNESS_RECOVERY_URGENT:
		case JOB_TYPE.VISIT_DIFFERENT_VILLAGE:
		case JOB_TYPE.HUNT_PREY:
		case JOB_TYPE.VISIT_STRUCTURE:
		case JOB_TYPE.SOCIALIZE:
			result = 1000;
			break;
		case JOB_TYPE.KNOCKOUT:
		case JOB_TYPE.BRAWL:
		case JOB_TYPE.ABSORB_LIFE:
		case JOB_TYPE.ABSORB_POWER:
		case JOB_TYPE.SPAWN_SKELETON:
		case JOB_TYPE.RAISE_CORPSE:
		case JOB_TYPE.HOARD:
		case JOB_TYPE.STEAL_GIVE_TRAIT:
			result = 970;
			break;
		case JOB_TYPE.DOUSE_FIRE:
		case JOB_TYPE.SUICIDE_FOLLOW:
			result = 950;
			break;
		case JOB_TYPE.SLAY_TARGET:
			result = 930;
			break;
		case JOB_TYPE.MOVE_CHARACTER:
		case JOB_TYPE.CAPTURE_CHARACTER:
		case JOB_TYPE.TRITON_KIDNAP:
		case JOB_TYPE.RESCUE_MOVE_CHARACTER:
			result = 926;
			break;
		case JOB_TYPE.GO_TO:
			result = 925;
			break;
		case JOB_TYPE.TAKE_ITEM_ON_SIGHT:
			result = 922;
			break;
		case JOB_TYPE.ABSORB_CRYSTAL:
		case JOB_TYPE.MINE_ORE:
		case JOB_TYPE.FIND_FISH:
		case JOB_TYPE.TILL_TILE:
		case JOB_TYPE.SHEAR_ANIMAL:
		case JOB_TYPE.SKIN_ANIMAL:
		case JOB_TYPE.HARVEST_CROPS:
		case JOB_TYPE.CRAFT_EQUIPMENT:
		case JOB_TYPE.CHOP_WOOD:
		case JOB_TYPE.MINE_STONE:
		case JOB_TYPE.RECUPERATE:
		case JOB_TYPE.CREATE_WORKPLACE_POTION:
		case JOB_TYPE.CREATE_HOSPICE_ANTIDOTE:
		case JOB_TYPE.TRAIN:
		case JOB_TYPE.PILGRIMAGE:
			result = 920;
			break;
		case JOB_TYPE.ZOMBIE_STROLL:
			result = 915;
			break;
		case JOB_TYPE.POISON_FOOD:
		case JOB_TYPE.PLACE_TRAP:
		case JOB_TYPE.OPEN_CHEST:
		case JOB_TYPE.ROAM_AROUND_STRUCTURE:
		case JOB_TYPE.IDLE_RETURN_HOME_HIGHER:
		case JOB_TYPE.READ_SCROLL:
			result = 910;
			break;
		case JOB_TYPE.MONSTER_INVADE:
			result = 900;
			break;
		case JOB_TYPE.RESTRAIN:
			result = 970;
			break;
		case JOB_TYPE.BUILD_BLUEPRINT:
		case JOB_TYPE.PLACE_BLUEPRINT:
		case JOB_TYPE.SPAWN_LAIR:
		case JOB_TYPE.BUILD_VAMPIRE_CASTLE:
		case JOB_TYPE.BUILD_BANDIT_CAMP:
		case JOB_TYPE.APPREHEND_RESTRAINED:
			result = 850;
			break;
		case JOB_TYPE.DEMON_KILL:
		case JOB_TYPE.SPREAD_RUMOR:
		case JOB_TYPE.SABOTAGE_NEIGHBOUR:
		case JOB_TYPE.DECREASE_MOOD:
		case JOB_TYPE.DISABLE:
		case JOB_TYPE.DARK_RITUAL:
		case JOB_TYPE.CULTIST_TRANSFORM:
		case JOB_TYPE.CULTIST_POISON:
		case JOB_TYPE.CULTIST_BOOBY_TRAP:
		case JOB_TYPE.SNATCH:
		case JOB_TYPE.VAMPIRIC_EMBRACE:
		case JOB_TYPE.IMPRISON_BLOOD_SOURCE:
		case JOB_TYPE.SNATCH_RESTRAIN:
		case JOB_TYPE.SHAMAN_RITUAL:
		case JOB_TYPE.DEMON_STEAL:
			result = 830;
			break;
		case JOB_TYPE.BURY:
		case JOB_TYPE.CHANGE_CLASS:
		case JOB_TYPE.TORTURE:
		case JOB_TYPE.VISIT_HOSPICE:
		case JOB_TYPE.IMPREGNATE:
			result = 820;
			break;
		case JOB_TYPE.PRODUCE_WOOD:
		case JOB_TYPE.PRODUCE_FOOD:
		case JOB_TYPE.PRODUCE_STONE:
		case JOB_TYPE.PRODUCE_METAL:
		case JOB_TYPE.MONSTER_BUTCHER:
		case JOB_TYPE.PRODUCE_FOOD_FOR_CAMP:
		case JOB_TYPE.QUARANTINE:
		case JOB_TYPE.PLAGUE_CARE:
		case JOB_TYPE.HEALER_CURE:
		case JOB_TYPE.BUY_FOOD_FOR_TAVERN:
		case JOB_TYPE.BUTCHER:
		case JOB_TYPE.KICK_OUT:
		case JOB_TYPE.FORAGE_FOOD:
		case JOB_TYPE.TEND_WYVERN_COOP:
		case JOB_TYPE.CREATE_GOLEM:
		case JOB_TYPE.BLOOD_SACRIFICE:
			result = 800;
			break;
		case JOB_TYPE.CRAFT_OBJECT:
		case JOB_TYPE.CREATE_WARD_LIGHT:
			result = 750;
			break;
		case JOB_TYPE.HAUL:
			result = 700;
			break;
		case JOB_TYPE.REPAIR:
			result = 650;
			break;
		case JOB_TYPE.CLEANSE_TILES:
			result = 630;
			break;
		case JOB_TYPE.CLEANSE_CORRUPTION:
		case JOB_TYPE.RECRUIT:
		case JOB_TYPE.STOCKPILE_FOOD:
		case JOB_TYPE.HAUL_ON_SIGHT:
			result = 600;
			break;
		case JOB_TYPE.JUDGE_PRISONER:
			result = 570;
			break;
		case JOB_TYPE.FACTION_KIDNAP:
		case JOB_TYPE.KIDNAP_RAID:
		case JOB_TYPE.STEAL_RAID:
		case JOB_TYPE.ARSON_RAID:
			result = 530;
			break;
		case JOB_TYPE.INSPECT:
			result = 510;
			break;
		case JOB_TYPE.CONFIRM_RUMOR:
		case JOB_TYPE.SHARE_NEGATIVE_INFO:
			result = 505;
			break;
		case JOB_TYPE.ANGRY_DESTROY:
			result = 503;
			break;
		case JOB_TYPE.ENERGY_RECOVERY_NORMAL:
		case JOB_TYPE.FULLNESS_RECOVERY_NORMAL:
		case JOB_TYPE.HAPPINESS_RECOVERY:
			result = 500;
			break;
		case JOB_TYPE.DROP_ITEM_PARTY:
			result = 495;
			break;
		case JOB_TYPE.PARTY_GO_TO:
		case JOB_TYPE.PARTYING:
		case JOB_TYPE.GO_TO_WAITING:
			result = 490;
			break;
		case JOB_TYPE.PATROL:
		case JOB_TYPE.JOIN_GATHERING:
		case JOB_TYPE.EXPLORE:
		case JOB_TYPE.EXTERMINATE:
		case JOB_TYPE.RESCUE:
		case JOB_TYPE.COUNTERATTACK_PARTY:
		case JOB_TYPE.RAID:
		case JOB_TYPE.HOST_SOCIAL_PARTY:
		case JOB_TYPE.HUNT_HEIRLOOM:
		case JOB_TYPE.DEVASTATION_RITUAL:
			result = 450;
			break;
		case JOB_TYPE.TEND_FARM:
		case JOB_TYPE.MINE:
			result = 440;
			break;
		case JOB_TYPE.DRY_TILES:
			result = 430;
			break;
		case JOB_TYPE.CHECK_PARALYZED_FRIEND:
			result = 400;
			break;
		case JOB_TYPE.VISIT_FRIEND:
			result = 280;
			break;
		case JOB_TYPE.OBTAIN_PERSONAL_ITEM:
		case JOB_TYPE.ABDUCT:
		case JOB_TYPE.LEARN_MONSTER:
		case JOB_TYPE.TAKE_ARTIFACT:
		case JOB_TYPE.GATHER_HERB:
		case JOB_TYPE.BUY_ITEM:
		case JOB_TYPE.OBTAIN_WANTED_ITEM:
			result = 260;
			break;
		case JOB_TYPE.SEDUCE:
		case JOB_TYPE.IDLE_RETURN_HOME:
		case JOB_TYPE.IDLE_NAP:
		case JOB_TYPE.IDLE_SIT:
		case JOB_TYPE.IDLE_STAND:
		case JOB_TYPE.IDLE_GO_TO_INN:
		case JOB_TYPE.IDLE:
		case JOB_TYPE.ROAM_AROUND_TERRITORY:
		case JOB_TYPE.ROAM_AROUND_CORRUPTION:
		case JOB_TYPE.ROAM_AROUND_PORTAL:
		case JOB_TYPE.ROAM_AROUND_TILE:
		case JOB_TYPE.RETURN_TERRITORY:
		case JOB_TYPE.RETURN_PORTAL:
		case JOB_TYPE.STAND:
		case JOB_TYPE.STAND_STILL:
		case JOB_TYPE.DROP_ITEM:
		case JOB_TYPE.CRAFT_MISSING_FURNITURE:
		case JOB_TYPE.WARM_UP:
		case JOB_TYPE.BURY_IN_ACTIVE_PARTY:
		case JOB_TYPE.IDLE_CLEAN:
		case JOB_TYPE.DROP_ITEM_TO_WORKPLACE:
		case JOB_TYPE.IDLE_PRAY:
			result = 250;
			break;
		case JOB_TYPE.HAUL_ANIMAL_CORPSE:
			result = 230;
			break;
		case JOB_TYPE.COMBINE_STOCKPILE:
			result = 200;
			break;
		case JOB_TYPE.COMMIT_SUICIDE:
			result = 150;
			break;
		case JOB_TYPE.STROLL:
			result = 100;
			break;
		case JOB_TYPE.MONSTER_EAT:
			result = 90;
			break;
		}
		return result;
	}

	public static int GetJobTypeNodeDistanceLimit(this JOB_TYPE jobType)
	{
		switch (jobType)
		{
		case JOB_TYPE.ROAM_AROUND_TERRITORY:
		case JOB_TYPE.ROAM_AROUND_TILE:
		case JOB_TYPE.ROAM_AROUND_STRUCTURE:
			return 60;
		case JOB_TYPE.REPORT_CRIME:
			return 140;
		default:
			return -1;
		}
	}

	public static bool IsButcherableWhenDeadOrAlive(this RACE type)
	{
		if ((uint)(type - 24) <= 2u || type == RACE.RABBIT)
		{
			return true;
		}
		return false;
	}

	public static bool IsButcherableWhenDead(this RACE type)
	{
		return RaceManager.Instance.GetRaceData(type).category == CHARACTER_CATEGORY.Beast;
	}

	public static bool IsShearable(this RACE type)
	{
		if (type == RACE.SHEEP || (uint)(type - 47) <= 2u)
		{
			return true;
		}
		return false;
	}

	public static bool IsSkinnable(this RACE type)
	{
		switch (type)
		{
		case RACE.DRAGON:
		case RACE.WOLF:
		case RACE.SPIDER:
		case RACE.BEAR:
		case RACE.BOAR:
			return true;
		default:
			return false;
		}
	}

	public static bool IsJobLethal(this JOB_TYPE type)
	{
		switch (type)
		{
		case JOB_TYPE.RESTRAIN:
		case JOB_TYPE.KNOCKOUT:
		case JOB_TYPE.APPREHEND:
		case JOB_TYPE.MOVE_CHARACTER:
		case JOB_TYPE.RITUAL_KILLING:
		case JOB_TYPE.ABDUCT:
		case JOB_TYPE.LEARN_MONSTER:
		case JOB_TYPE.MONSTER_ABDUCT:
		case JOB_TYPE.BRAWL:
		case JOB_TYPE.BERSERK_ATTACK:
		case JOB_TYPE.FACTION_KIDNAP:
		case JOB_TYPE.SNATCH:
		case JOB_TYPE.KIDNAP_RAID:
		case JOB_TYPE.CAPTURE_CHARACTER:
		case JOB_TYPE.SNATCH_RESTRAIN:
		case JOB_TYPE.RESCUE_MOVE_CHARACTER:
		case JOB_TYPE.DEMON_STEAL:
		case JOB_TYPE.APPREHEND_RESTRAINED:
			return false;
		default:
			return true;
		}
	}

	public static bool IsJobAbduction(this JOB_TYPE type)
	{
		switch (type)
		{
		case JOB_TYPE.ABDUCT:
		case JOB_TYPE.MONSTER_ABDUCT:
		case JOB_TYPE.FACTION_KIDNAP:
		case JOB_TYPE.SNATCH:
		case JOB_TYPE.KIDNAP_RAID:
		case JOB_TYPE.CAPTURE_CHARACTER:
		case JOB_TYPE.SNATCH_RESTRAIN:
			return true;
		default:
			return false;
		}
	}

	public static bool IsCultistJob(this JOB_TYPE type)
	{
		if ((uint)(type - 104) <= 1u || type == JOB_TYPE.PREACH || type == JOB_TYPE.CULTIST_INSTRUCTION)
		{
			return true;
		}
		return false;
	}

	public static bool IsReturnHome(this JOB_TYPE p_type)
	{
		if (p_type != JOB_TYPE.IDLE_RETURN_HOME && p_type != JOB_TYPE.RETURN_HOME_URGENT)
		{
			return p_type == JOB_TYPE.FLEE_TO_HOME;
		}
		return true;
	}

	public static string LocalizedName(this SUMMON_TYPE type)
	{
		switch (type)
		{
		case SUMMON_TYPE.Wolf:
		{
			string p_className3 = "Ravager";
			return CharacterManager.Instance.GetCharacterClass(p_className3).displayName;
		}
		case SUMMON_TYPE.Dire_Wolf:
		{
			string p_className2 = "Dire";
			return CharacterManager.Instance.GetCharacterClass(p_className2).displayName;
		}
		default:
		{
			string p_className = type.ToStringEnumWithSpace();
			return CharacterManager.Instance.GetCharacterClass(p_className).displayName;
		}
		}
	}

	public static bool IsAnimalBeast(this SUMMON_TYPE type)
	{
		if (type == SUMMON_TYPE.Wolf || (uint)(type - 42) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static string ClassnameToUseForPortrait(this SUMMON_TYPE type)
	{
		switch (type)
		{
		case SUMMON_TYPE.Giant_Spider:
		case SUMMON_TYPE.Small_Spider:
		case SUMMON_TYPE.Tarantula:
		case SUMMON_TYPE.Broodmother:
			if (SettingsManager.Instance.settings.arachnophobiaToggle)
			{
				return CharacterVisuals.ClassToUseForArachnophobia;
			}
			return CharacterManager.Instance.GetSummonClassNameBySummonType(type);
		default:
			return CharacterManager.Instance.GetSummonClassNameBySummonType(type);
		}
	}

	public static Color GetColorTintToUseForPortrait(this SUMMON_TYPE type)
	{
		switch (type)
		{
		case SUMMON_TYPE.Giant_Spider:
		case SUMMON_TYPE.Small_Spider:
		case SUMMON_TYPE.Tarantula:
		case SUMMON_TYPE.Broodmother:
			if (SettingsManager.Instance.settings.arachnophobiaToggle)
			{
				return CharacterVisuals.ColorToUseForArachnophobia;
			}
			return Color.white;
		default:
			return Color.white;
		}
	}

	public static bool IsSpiderType(this SUMMON_TYPE type)
	{
		switch (type)
		{
		case SUMMON_TYPE.Giant_Spider:
		case SUMMON_TYPE.Small_Spider:
		case SUMMON_TYPE.Tarantula:
		case SUMMON_TYPE.Broodmother:
			return true;
		default:
			return false;
		}
	}

	public static bool CanBeSummoned(this ARTIFACT_TYPE type)
	{
		if (type == ARTIFACT_TYPE.None)
		{
			return true;
		}
		return false;
	}

	public static bool IsPlayerLandmark(this LANDMARK_TYPE type)
	{
		switch (type)
		{
		case LANDMARK_TYPE.THE_PORTAL:
		case LANDMARK_TYPE.OSTRACIZER:
		case LANDMARK_TYPE.CRYPT:
		case LANDMARK_TYPE.KENNEL:
		case LANDMARK_TYPE.THE_ANVIL:
		case LANDMARK_TYPE.MEDDLER:
		case LANDMARK_TYPE.EYE:
		case LANDMARK_TYPE.DEFILER:
		case LANDMARK_TYPE.THE_NEEDLES:
		case LANDMARK_TYPE.TORTURE_CHAMBERS:
		case LANDMARK_TYPE.SPIRE:
		case LANDMARK_TYPE.MANA_PIT:
		case LANDMARK_TYPE.MARAUD:
		case LANDMARK_TYPE.PRISM:
			return true;
		default:
			return false;
		}
	}

	public static STRUCTURE_TYPE GetStructureType(this LANDMARK_TYPE landmarkType)
	{
		switch (landmarkType)
		{
		case LANDMARK_TYPE.HOUSES:
			return STRUCTURE_TYPE.DWELLING;
		case LANDMARK_TYPE.VILLAGE:
			return STRUCTURE_TYPE.CITY_CENTER;
		default:
		{
			if (Enum.TryParse<STRUCTURE_TYPE>(landmarkType.ToString(), out var result))
			{
				return result;
			}
			throw new Exception("There is no corresponding structure type for " + landmarkType);
		}
		}
	}

	public static bool IsJobStructure(this STRUCTURE_TYPE p_type)
	{
		switch (p_type)
		{
		case STRUCTURE_TYPE.TAVERN:
		case STRUCTURE_TYPE.HOSPICE:
		case STRUCTURE_TYPE.HUNTER_LODGE:
		case STRUCTURE_TYPE.FARM:
		case STRUCTURE_TYPE.LUMBERYARD:
		case STRUCTURE_TYPE.MINE:
		case STRUCTURE_TYPE.WORKSHOP:
		case STRUCTURE_TYPE.FISHERY:
		case STRUCTURE_TYPE.BUTCHERS_SHOP:
			return true;
		default:
			return false;
		}
	}

	public static bool IsForageStructure(this STRUCTURE_TYPE p_type)
	{
		if ((uint)(p_type - 74) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static bool IsTileObjectWithCount(this TILE_OBJECT_TYPE p_type)
	{
		if ((uint)(p_type - 8) <= 1u || p_type == TILE_OBJECT_TYPE.ROCK || p_type == TILE_OBJECT_TYPE.BIG_TREE_OBJECT)
		{
			return true;
		}
		return false;
	}

	public static string Description(this DEADLY_SIN_ACTION sin)
	{
		return sin switch
		{
			DEADLY_SIN_ACTION.SPELL_SOURCE => "Knows three Spells that can be extracted by the Ruinarch", 
			DEADLY_SIN_ACTION.INSTIGATOR => "Can be assigned to spawn Chaos Events in The Fingers", 
			DEADLY_SIN_ACTION.BUILDER => "Can construct demonic structures", 
			DEADLY_SIN_ACTION.SABOTEUR => "Can interfere in Events spawned by non-combatant characters", 
			DEADLY_SIN_ACTION.INVADER => "Can invade adjacent regions", 
			DEADLY_SIN_ACTION.FIGHTER => "Can interfere in Events spawned by combat-ready characters", 
			DEADLY_SIN_ACTION.RESEARCHER => "Can be assigned to research upgrades in The Anvil", 
			_ => string.Empty, 
		};
	}

	public static bool UsesGenderNeutralPortrait(this RACE race)
	{
		if ((uint)(race - 1) <= 1u || race == RACE.LESSER_DEMON)
		{
			return false;
		}
		return true;
	}

	public static bool IsSapient(this RACE race)
	{
		if ((uint)(race - 1) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static bool HasHeadHair(this RACE race)
	{
		if ((uint)(race - 1) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static string FactionRelationshipColor(this FACTION_RELATIONSHIP_STATUS relationshipStatus)
	{
		return relationshipStatus switch
		{
			FACTION_RELATIONSHIP_STATUS.Friendly => "#19ff00", 
			FACTION_RELATIONSHIP_STATUS.Hostile => "#ff0000", 
			_ => "#F8E1A9", 
		};
	}

	public static RACE GetRaceForFactionType(this FACTION_TYPE p_factionType, bool randomizeDefault = false)
	{
		switch (p_factionType)
		{
		case FACTION_TYPE.Elven_Kingdom:
			return RACE.ELVES;
		case FACTION_TYPE.Human_Empire:
			return RACE.HUMANS;
		case FACTION_TYPE.Demons:
			return RACE.DEMON;
		case FACTION_TYPE.Ratmen:
			return RACE.RATMAN;
		default:
			if (randomizeDefault)
			{
				if (!GameUtilities.RollChance(50))
				{
					return RACE.HUMANS;
				}
				return RACE.ELVES;
			}
			return RACE.HUMANS;
		}
	}

	public static bool IsStructureType(this LocationGridTile.Ground_Type groundType)
	{
		switch (groundType)
		{
		case LocationGridTile.Ground_Type.Cobble:
		case LocationGridTile.Ground_Type.Wood:
		case LocationGridTile.Ground_Type.Cave:
		case LocationGridTile.Ground_Type.Corrupted:
		case LocationGridTile.Ground_Type.Bone:
		case LocationGridTile.Ground_Type.Demon_Stone:
		case LocationGridTile.Ground_Type.Flesh:
		case LocationGridTile.Ground_Type.Structure_Stone:
		case LocationGridTile.Ground_Type.Ruined_Stone:
		case LocationGridTile.Ground_Type.Blight:
			return true;
		default:
			return false;
		}
	}

	public static bool CaseInsensitiveContains(this string text, string value, StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
	{
		return text.IndexOf(value, stringComparison) >= 0;
	}

	public static bool IsFoodProducerClassName(this string text)
	{
		if (!(text == "Fisher") && !(text == "Farmer"))
		{
			return text == "Butcher";
		}
		return true;
	}

	public static bool IsResourceProducerClassName(this string text)
	{
		if (!(text == "Miner") && !(text == "Logger"))
		{
			return text == "Skinner";
		}
		return true;
	}

	public static bool IsSpecialCivilianClassName(this string text)
	{
		if (!(text == "Skinner") && !(text == "Crafter"))
		{
			return text == "Merchant";
		}
		return true;
	}

	public static bool RectOverlaps(this RectTransform rectTrans1, RectTransform rectTrans2)
	{
		return rectTrans1.WorldRect().Overlaps(rectTrans2.WorldRect());
	}

	public static Rect WorldRect(this RectTransform rectTransform)
	{
		Rect rect = rectTransform.rect;
		rect.center = rectTransform.TransformPoint(rect.center);
		rect.size = rectTransform.TransformVector(rect.size);
		return rect;
	}

	public static bool CanTemptCharacter(this TEMPTATION p_temptation, Character p_target)
	{
		return p_temptation switch
		{
			TEMPTATION.Dark_Blessing => !p_target.traitContainer.IsBlessed(), 
			TEMPTATION.Empower => !p_target.traitContainer.HasTrait("Mighty"), 
			TEMPTATION.Cleanse_Flaws => p_target.traitContainer.HasTraitOf(TRAIT_TYPE.FLAW), 
			_ => throw new ArgumentOutOfRangeException("p_temptation", p_temptation, null), 
		};
	}

	public static bool IsReligionType(this FACTION_IDEOLOGY p_factionIdeology)
	{
		if ((uint)(p_factionIdeology - 4) <= 2u)
		{
			return true;
		}
		return false;
	}

	public static bool IsInclusivityType(this FACTION_IDEOLOGY p_factionIdeology)
	{
		if ((uint)p_factionIdeology <= 1u)
		{
			return true;
		}
		return false;
	}

	public static bool IsPeaceType(this FACTION_IDEOLOGY p_factionIdeology)
	{
		if ((uint)(p_factionIdeology - 2) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static bool IsRavagerLoadout(this PLAYER_ARCHETYPE p_archetype)
	{
		if (p_archetype == PLAYER_ARCHETYPE.Progression_Ravager || p_archetype == PLAYER_ARCHETYPE.Attainment_Ravager || p_archetype == PLAYER_ARCHETYPE.Eradication_Ravager)
		{
			return true;
		}
		return false;
	}

	public static bool IsLichLoadout(this PLAYER_ARCHETYPE p_archetype)
	{
		if (p_archetype == PLAYER_ARCHETYPE.Progression_Lich || p_archetype == PLAYER_ARCHETYPE.Attainment_Lich || p_archetype == PLAYER_ARCHETYPE.Eradication_Lich)
		{
			return true;
		}
		return false;
	}

	public static bool IsPuppetmasterLoadout(this PLAYER_ARCHETYPE p_archetype)
	{
		if (p_archetype == PLAYER_ARCHETYPE.Progression_Puppet_Master || p_archetype == PLAYER_ARCHETYPE.Attainment_Puppet_Master || p_archetype == PLAYER_ARCHETYPE.Eradication_Puppet_Master)
		{
			return true;
		}
		return false;
	}

	public static bool IsSameBaseArchetype(this PLAYER_ARCHETYPE p_archetype, PLAYER_ARCHETYPE p_otherArchetype)
	{
		if (p_archetype.IsRavagerLoadout())
		{
			return p_otherArchetype.IsRavagerLoadout();
		}
		if (p_archetype.IsPuppetmasterLoadout())
		{
			return p_otherArchetype.IsPuppetmasterLoadout();
		}
		if (p_archetype.IsLichLoadout())
		{
			return p_otherArchetype.IsLichLoadout();
		}
		return false;
	}

	public static string LocalizedName(this RESISTANCE p_resistance)
	{
		string result = string.Empty;
		switch (p_resistance)
		{
		case RESISTANCE.None:
			result = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "None Resistance");
			break;
		case RESISTANCE.Fire:
			result = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Fire Resistance");
			break;
		case RESISTANCE.Poison:
			result = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Poison Resistance");
			break;
		case RESISTANCE.Water:
			result = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Water Resistance");
			break;
		case RESISTANCE.Ice:
			result = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Ice Resistance");
			break;
		case RESISTANCE.Electric:
			result = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Lightning Resistance");
			break;
		case RESISTANCE.Earth:
			result = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Earth Resistance");
			break;
		case RESISTANCE.Wind:
			result = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Wind Resistance");
			break;
		case RESISTANCE.Mental:
			result = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Mental_Resistance_Title");
			break;
		case RESISTANCE.Physical:
			result = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Physical_Resistance_Title");
			break;
		}
		return result;
	}

	public static ELEMENTAL_TYPE GetElement(this RESISTANCE p_resistance)
	{
		return p_resistance switch
		{
			RESISTANCE.Fire => ELEMENTAL_TYPE.Fire, 
			RESISTANCE.Poison => ELEMENTAL_TYPE.Poison, 
			RESISTANCE.Water => ELEMENTAL_TYPE.Water, 
			RESISTANCE.Ice => ELEMENTAL_TYPE.Ice, 
			RESISTANCE.Electric => ELEMENTAL_TYPE.Electric, 
			RESISTANCE.Earth => ELEMENTAL_TYPE.Earth, 
			RESISTANCE.Wind => ELEMENTAL_TYPE.Wind, 
			RESISTANCE.Physical => ELEMENTAL_TYPE.Normal, 
			RESISTANCE.Mental => throw new Exception("Cannot convert Mental resistance to an Element!"), 
			_ => ELEMENTAL_TYPE.Normal, 
		};
	}

	public static RESISTANCE GetResistance(this ELEMENTAL_TYPE p_element)
	{
		return p_element switch
		{
			ELEMENTAL_TYPE.Normal => RESISTANCE.Physical, 
			ELEMENTAL_TYPE.Fire => RESISTANCE.Fire, 
			ELEMENTAL_TYPE.Poison => RESISTANCE.Poison, 
			ELEMENTAL_TYPE.Water => RESISTANCE.Water, 
			ELEMENTAL_TYPE.Ice => RESISTANCE.Ice, 
			ELEMENTAL_TYPE.Electric => RESISTANCE.Electric, 
			ELEMENTAL_TYPE.Earth => RESISTANCE.Earth, 
			ELEMENTAL_TYPE.Wind => RESISTANCE.Wind, 
			_ => RESISTANCE.Physical, 
		};
	}

	public static bool IsElemental(this RESISTANCE p_resistance)
	{
		switch (p_resistance)
		{
		case RESISTANCE.Fire:
		case RESISTANCE.Water:
		case RESISTANCE.Earth:
		case RESISTANCE.Wind:
			return true;
		default:
			return false;
		}
	}

	public static bool IsSecondary(this RESISTANCE p_resistance)
	{
		if (p_resistance == RESISTANCE.Poison || (uint)(p_resistance - 5) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static bool IsElemental(this ELEMENTAL_TYPE p_resistance)
	{
		switch (p_resistance)
		{
		case ELEMENTAL_TYPE.Fire:
		case ELEMENTAL_TYPE.Water:
		case ELEMENTAL_TYPE.Earth:
		case ELEMENTAL_TYPE.Wind:
			return true;
		default:
			return false;
		}
	}

	public static bool IsSecondary(this ELEMENTAL_TYPE p_resistance)
	{
		if (p_resistance == ELEMENTAL_TYPE.Poison || (uint)(p_resistance - 4) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static STRUCTURE_TYPE GetStructureTypeForElevation(this ELEVATION p_elevation)
	{
		return p_elevation switch
		{
			ELEVATION.MOUNTAIN => STRUCTURE_TYPE.CAVE, 
			ELEVATION.WATER => STRUCTURE_TYPE.OCEAN, 
			_ => throw new ArgumentOutOfRangeException("p_elevation", p_elevation, null), 
		};
	}

	public static string GetCurrencyTextSprite(this CURRENCY p_currency)
	{
		return p_currency switch
		{
			CURRENCY.Mana => Utilities.ManaIcon(), 
			CURRENCY.Chaotic_Energy => Utilities.ChaoticEnergyIcon(), 
			CURRENCY.Spirit_Energy => Utilities.SpiritEnergyIcon(), 
			_ => Utilities.ManaIcon(), 
		};
	}

	public static int GetUpgradeOrderInTooltip(this UPGRADE_BONUS p_bonus)
	{
		return p_bonus switch
		{
			UPGRADE_BONUS.Damage => 0, 
			UPGRADE_BONUS.Pierce => 1, 
			UPGRADE_BONUS.Duration => 2, 
			UPGRADE_BONUS.Tile_Range => 3, 
			UPGRADE_BONUS.Movement_Speed => 4, 
			UPGRADE_BONUS.Atk_Percentage => 5, 
			UPGRADE_BONUS.Cooldown => 6, 
			UPGRADE_BONUS.Added_Effect_Level_0 => 7, 
			UPGRADE_BONUS.Added_Effect_Level_1 => 8, 
			UPGRADE_BONUS.Added_Effect_Level_2 => 9, 
			UPGRADE_BONUS.Added_Effect_Level_3 => 10, 
			_ => int.MaxValue, 
		};
	}

	public static string GetOpinionLabel(this OPINIONS p_opinion)
	{
		return p_opinion switch
		{
			OPINIONS.Rival => "Rival", 
			OPINIONS.Enemy => "Enemy", 
			OPINIONS.Acquaintance => "Acquaintance", 
			_ => string.Empty, 
		};
	}

	public static int GetTutorialOrder(this TutorialManager.Tutorial_Type p_type)
	{
		return p_type switch
		{
			TutorialManager.Tutorial_Type.Time_Management => 0, 
			TutorialManager.Tutorial_Type.Target_Menu => 1, 
			TutorialManager.Tutorial_Type.Chaotic_Energy => 2, 
			TutorialManager.Tutorial_Type.Base_Building => 3, 
			TutorialManager.Tutorial_Type.Unlocking_Bonus_Powers => 4, 
			TutorialManager.Tutorial_Type.Upgrading_The_Portal => 5, 
			TutorialManager.Tutorial_Type.Mana => 6, 
			TutorialManager.Tutorial_Type.Spirit_Energy => 7, 
			TutorialManager.Tutorial_Type.Storing_Targets => 8, 
			TutorialManager.Tutorial_Type.Maraud => 9, 
			TutorialManager.Tutorial_Type.Intel => 10, 
			TutorialManager.Tutorial_Type.Migration_Controls => 11, 
			TutorialManager.Tutorial_Type.Abilities => 12, 
			TutorialManager.Tutorial_Type.Resistances => 13, 
			_ => (int)p_type, 
		};
	}

	public static BIOMES GetMainBiomeForTileType(this Biome_Tile_Type p_tileType)
	{
		switch (p_tileType)
		{
		case Biome_Tile_Type.Desert:
		case Biome_Tile_Type.Oasis:
			return BIOMES.DESERT;
		case Biome_Tile_Type.Grassland:
			return BIOMES.GRASSLAND;
		case Biome_Tile_Type.Jungle:
			return BIOMES.FOREST;
		case Biome_Tile_Type.Taiga:
		case Biome_Tile_Type.Tundra:
		case Biome_Tile_Type.Snow:
			return BIOMES.SNOW;
		default:
			throw new ArgumentOutOfRangeException("p_tileType", p_tileType, null);
		}
	}

	public static RESOURCE GetResourceCategory(this CONCRETE_RESOURCES p_resource)
	{
		switch (p_resource)
		{
		case CONCRETE_RESOURCES.Copper:
		case CONCRETE_RESOURCES.Iron:
		case CONCRETE_RESOURCES.Mithril:
		case CONCRETE_RESOURCES.Orichalcum:
		case CONCRETE_RESOURCES.Diamond:
		case CONCRETE_RESOURCES.Gold:
			return RESOURCE.METAL;
		case CONCRETE_RESOURCES.Rabbit_Cloth:
		case CONCRETE_RESOURCES.Mink_Cloth:
		case CONCRETE_RESOURCES.Wool:
		case CONCRETE_RESOURCES.Spider_Silk:
		case CONCRETE_RESOURCES.Mooncrawler_Cloth:
			return RESOURCE.CLOTH;
		case CONCRETE_RESOURCES.Boar_Hide:
		case CONCRETE_RESOURCES.Scale_Hide:
		case CONCRETE_RESOURCES.Dragon_Hide:
		case CONCRETE_RESOURCES.Wolf_Hide:
		case CONCRETE_RESOURCES.Bear_Hide:
			return RESOURCE.LEATHER;
		case CONCRETE_RESOURCES.Stone:
			return RESOURCE.STONE;
		case CONCRETE_RESOURCES.Elf_Meat:
		case CONCRETE_RESOURCES.Human_Meat:
		case CONCRETE_RESOURCES.Animal_Meat:
		case CONCRETE_RESOURCES.Fish:
		case CONCRETE_RESOURCES.Corn:
		case CONCRETE_RESOURCES.Potato:
		case CONCRETE_RESOURCES.Pineapple:
		case CONCRETE_RESOURCES.Iceberry:
		case CONCRETE_RESOURCES.Mushroom:
		case CONCRETE_RESOURCES.Hypno_Herb:
		case CONCRETE_RESOURCES.Rat_Meat:
		case CONCRETE_RESOURCES.Vegetables:
			return RESOURCE.FOOD;
		case CONCRETE_RESOURCES.Wood:
			return RESOURCE.WOOD;
		default:
			throw new ArgumentOutOfRangeException("p_resource", p_resource, null);
		}
	}

	public static TILE_OBJECT_TYPE ConvertResourcesToTileObjectType(this CONCRETE_RESOURCES p_resrouce)
	{
		return p_resrouce switch
		{
			CONCRETE_RESOURCES.Copper => TILE_OBJECT_TYPE.COPPER, 
			CONCRETE_RESOURCES.Iron => TILE_OBJECT_TYPE.IRON, 
			CONCRETE_RESOURCES.Mithril => TILE_OBJECT_TYPE.MITHRIL, 
			CONCRETE_RESOURCES.Orichalcum => TILE_OBJECT_TYPE.ORICHALCUM, 
			CONCRETE_RESOURCES.Gold => TILE_OBJECT_TYPE.GOLD, 
			CONCRETE_RESOURCES.Diamond => TILE_OBJECT_TYPE.DIAMOND, 
			CONCRETE_RESOURCES.Wood => TILE_OBJECT_TYPE.WOOD_PILE, 
			CONCRETE_RESOURCES.Stone => TILE_OBJECT_TYPE.STONE_PILE, 
			CONCRETE_RESOURCES.Rabbit_Cloth => TILE_OBJECT_TYPE.RABBIT_CLOTH, 
			CONCRETE_RESOURCES.Mink_Cloth => TILE_OBJECT_TYPE.MINK_CLOTH, 
			CONCRETE_RESOURCES.Wool => TILE_OBJECT_TYPE.WOOL, 
			CONCRETE_RESOURCES.Spider_Silk => TILE_OBJECT_TYPE.SPIDER_SILK, 
			CONCRETE_RESOURCES.Boar_Hide => TILE_OBJECT_TYPE.BOAR_HIDE, 
			CONCRETE_RESOURCES.Scale_Hide => TILE_OBJECT_TYPE.SCALE_HIDE, 
			CONCRETE_RESOURCES.Dragon_Hide => TILE_OBJECT_TYPE.DRAGON_HIDE, 
			CONCRETE_RESOURCES.Elf_Meat => TILE_OBJECT_TYPE.ELF_MEAT, 
			CONCRETE_RESOURCES.Human_Meat => TILE_OBJECT_TYPE.HUMAN_MEAT, 
			CONCRETE_RESOURCES.Animal_Meat => TILE_OBJECT_TYPE.ANIMAL_MEAT, 
			CONCRETE_RESOURCES.Fish => TILE_OBJECT_TYPE.FISH_PILE, 
			CONCRETE_RESOURCES.Corn => TILE_OBJECT_TYPE.CORN, 
			CONCRETE_RESOURCES.Potato => TILE_OBJECT_TYPE.POTATO, 
			CONCRETE_RESOURCES.Pineapple => TILE_OBJECT_TYPE.PINEAPPLE, 
			CONCRETE_RESOURCES.Iceberry => TILE_OBJECT_TYPE.ICEBERRY, 
			CONCRETE_RESOURCES.Mushroom => TILE_OBJECT_TYPE.MUSHROOM, 
			CONCRETE_RESOURCES.Wolf_Hide => TILE_OBJECT_TYPE.WOLF_HIDE, 
			CONCRETE_RESOURCES.Bear_Hide => TILE_OBJECT_TYPE.BEAR_HIDE, 
			CONCRETE_RESOURCES.Mooncrawler_Cloth => TILE_OBJECT_TYPE.MOONCRAWLER_CLOTH, 
			CONCRETE_RESOURCES.Hypno_Herb => TILE_OBJECT_TYPE.HYPNO_HERB, 
			CONCRETE_RESOURCES.Rat_Meat => TILE_OBJECT_TYPE.RAT_MEAT, 
			CONCRETE_RESOURCES.Vegetables => TILE_OBJECT_TYPE.VEGETABLES, 
			_ => TILE_OBJECT_TYPE.NONE, 
		};
	}

	public static string LocalizedName(this RESOURCE p_resource)
	{
		return LocalizationManager.Instance.GetLocalizedValue("TileObjects_Table", p_resource.ToStringEnum());
	}

	public static bool IsDayTime(this TIME_IN_WORDS p_time)
	{
		if ((uint)(p_time - 1) <= 1u || p_time == TIME_IN_WORDS.LUNCH_TIME)
		{
			return true;
		}
		return false;
	}

	public static bool IsNightTime(this TIME_IN_WORDS p_time)
	{
		if (p_time == TIME_IN_WORDS.AFTER_MIDNIGHT || (uint)(p_time - 3) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static int GetBookmarkCategoryOrder(this BOOKMARK_CATEGORY p_category)
	{
		return p_category switch
		{
			BOOKMARK_CATEGORY.Win_Condition => 0, 
			BOOKMARK_CATEGORY.Major_Events => 1, 
			BOOKMARK_CATEGORY.Portal => 2, 
			BOOKMARK_CATEGORY.Sub_Goals => 3, 
			BOOKMARK_CATEGORY.Alerts => 4, 
			BOOKMARK_CATEGORY.Targets => 5, 
			BOOKMARK_CATEGORY.Player_Parties => 6, 
			_ => throw new ArgumentOutOfRangeException("p_category", p_category, null), 
		};
	}

	public static bool CanBeLearned(this COMBAT_SPECIAL_SKILL ability)
	{
		switch (ability)
		{
		case COMBAT_SPECIAL_SKILL.Heal:
		case COMBAT_SPECIAL_SKILL.Fast_Heal:
		case COMBAT_SPECIAL_SKILL.Strong_Heal:
		case COMBAT_SPECIAL_SKILL.Group_Heal:
		case COMBAT_SPECIAL_SKILL.Self_Heal:
		case COMBAT_SPECIAL_SKILL.Max_Heal:
		case COMBAT_SPECIAL_SKILL.Elemental_Protection:
		case COMBAT_SPECIAL_SKILL.Stoneskin:
		case COMBAT_SPECIAL_SKILL.Sharpen:
		case COMBAT_SPECIAL_SKILL.Thorns:
		case COMBAT_SPECIAL_SKILL.Summon_Wolf:
		case COMBAT_SPECIAL_SKILL.Summon_Bear:
		case COMBAT_SPECIAL_SKILL.Battle_Cry:
		case COMBAT_SPECIAL_SKILL.Endure:
		case COMBAT_SPECIAL_SKILL.Blitz:
		case COMBAT_SPECIAL_SKILL.Cleanse:
		case COMBAT_SPECIAL_SKILL.Pierce:
		case COMBAT_SPECIAL_SKILL.Shield:
		case COMBAT_SPECIAL_SKILL.Wind_Slice:
		case COMBAT_SPECIAL_SKILL.Siege_Arrows:
		case COMBAT_SPECIAL_SKILL.Polymorph:
			return true;
		default:
			return false;
		}
	}

	public static bool IsStaff(this TILE_OBJECT_TYPE type)
	{
		if ((uint)(type - 200) <= 3u || type == TILE_OBJECT_TYPE.BASIC_STAFF || type == TILE_OBJECT_TYPE.ECLIPSE)
		{
			return true;
		}
		return false;
	}

	public static bool IsBow(this TILE_OBJECT_TYPE type)
	{
		if ((uint)(type - 196) <= 3u || type == TILE_OBJECT_TYPE.BASIC_BOW || type == TILE_OBJECT_TYPE.CITRUS)
		{
			return true;
		}
		return false;
	}

	public static bool IsTutorialTypeAlert(this Game_Alert p_alert)
	{
		if ((uint)p_alert <= 11u || p_alert == Game_Alert.Spawn_Defensive_Units)
		{
			return true;
		}
		return false;
	}

	public static FACTION_TYPE GetFactionTypeForReligion(this RELIGION religion)
	{
		return religion switch
		{
			RELIGION.Demon_Worship => FACTION_TYPE.Demon_Cult, 
			RELIGION.Divine_Worship => FACTION_TYPE.Divine_Church, 
			RELIGION.Nature_Worship => FACTION_TYPE.Wiccans, 
			_ => throw new ArgumentOutOfRangeException("religion", religion, null), 
		};
	}

	public static string GetCultistTraitNameForReligion(this RELIGION religion)
	{
		return religion switch
		{
			RELIGION.Demon_Worship => "Demon Cultist", 
			RELIGION.Divine_Worship => "Cleric", 
			RELIGION.Nature_Worship => "Witch", 
			_ => string.Empty, 
		};
	}

	public static string GetCultLeaderClassNameForReligion(this RELIGION religion)
	{
		return religion switch
		{
			RELIGION.Demon_Worship => "Demon Cult Leader", 
			RELIGION.Divine_Worship => "Priest", 
			RELIGION.Nature_Worship => "Great Witch", 
			_ => string.Empty, 
		};
	}

	public static CRIME_TYPE GetCrimeTypeByReligion(this RELIGION religion)
	{
		return religion switch
		{
			RELIGION.Demon_Worship => CRIME_TYPE.Demon_Worship, 
			RELIGION.Divine_Worship => CRIME_TYPE.Divine_Worship, 
			RELIGION.Nature_Worship => CRIME_TYPE.Nature_Worship, 
			_ => CRIME_TYPE.None, 
		};
	}

	public static RELIGION GetReligionByCrimeType(this CRIME_TYPE crimeType)
	{
		return crimeType switch
		{
			CRIME_TYPE.Demon_Worship => RELIGION.Demon_Worship, 
			CRIME_TYPE.Divine_Worship => RELIGION.Divine_Worship, 
			CRIME_TYPE.Nature_Worship => RELIGION.Nature_Worship, 
			_ => RELIGION.None, 
		};
	}

	public static string LocalizedName(this RELIGION religion)
	{
		return LocalizationManager.Instance.GetLocalizedValue("FactionIdeologies_Table", religion.ToStringEnumWithSpace());
	}

	public static RESOURCE GetResourceForWall(this WALL_RESOURCE p_wallResource)
	{
		return p_wallResource switch
		{
			WALL_RESOURCE.Wood => RESOURCE.WOOD, 
			WALL_RESOURCE.Stone => RESOURCE.STONE, 
			WALL_RESOURCE.Elven_Wood => RESOURCE.WOOD, 
			WALL_RESOURCE.Divine_Stone => RESOURCE.STONE, 
			WALL_RESOURCE.Nature_Vines => RESOURCE.WOOD, 
			_ => RESOURCE.WOOD, 
		};
	}

	public static string LocalizedText(this BOOKMARK_CATEGORY p_category)
	{
		return LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", p_category.ToStringEnumWithPrefix());
	}

	public static string LocalizedName(this PLAGUE_FATALITY p_type)
	{
		return LocalizationManager.Instance.GetLocalizedValue("Plague_Table", p_type.ToStringEnumWithSpace());
	}

	public static string LocalizedName(this PLAGUE_SYMPTOM p_type)
	{
		return LocalizationManager.Instance.GetLocalizedValue("Plague_Table", p_type.ToStringEnumWithSpace());
	}

	public static Type ClassType(this SUB_GOAL p_subGoal)
	{
		return typeof(SimpleSubGoal);
	}

	public static ACHIEVEMENT GetAchievementType(this SUB_GOAL p_subGoal)
	{
		return p_subGoal switch
		{
			SUB_GOAL.GOAL_POISON_FOOD => ACHIEVEMENT.GOAL_POISON_FOOD, 
			SUB_GOAL.GOAL_IMPRISON_VILLAGER => ACHIEVEMENT.GOAL_IMPRISON_VILLAGER, 
			SUB_GOAL.GOAL_CAPTURE_TRITON => ACHIEVEMENT.GOAL_CAPTURE_TRITON, 
			SUB_GOAL.GOAL_FALLEN_ANGEL => ACHIEVEMENT.GOAL_FALLEN_ANGEL, 
			SUB_GOAL.GOAL_DEMON_CULTIST => ACHIEVEMENT.GOAL_DEMON_CULTIST, 
			SUB_GOAL.GOAL_DESTROY_RESOURCE => ACHIEVEMENT.GOAL_DESTROY_RESOURCE, 
			SUB_GOAL.GOAL_CREATE_PSYCHO => ACHIEVEMENT.GOAL_CREATE_PSYCHO, 
			SUB_GOAL.GOAL_REMOVE_BUFF => ACHIEVEMENT.GOAL_REMOVE_BUFF, 
			SUB_GOAL.GOAL_KILL_ELF => ACHIEVEMENT.GOAL_KILL_ELF, 
			SUB_GOAL.GOAL_KILL_HUMAN => ACHIEVEMENT.GOAL_KILL_HUMAN, 
			SUB_GOAL.GOAL_MAKE_VILLAGER_EVIL => ACHIEVEMENT.GOAL_MAKE_VILLAGER_EVIL, 
			SUB_GOAL.GOAL_SHARE_CRIME_INTEL => ACHIEVEMENT.GOAL_SHARE_CRIME_INTEL, 
			SUB_GOAL.GOAL_SNATCH_OBJECT => ACHIEVEMENT.GOAL_SNATCH_OBJECT, 
			SUB_GOAL.GOAL_TRIGGER_FLAW => ACHIEVEMENT.GOAL_TRIGGER_FLAW, 
			SUB_GOAL.GOAL_TRIGGER_AROUSAL => ACHIEVEMENT.GOAL_TRIGGER_AROUSAL, 
			SUB_GOAL.GOAL_POISON_CLOUD => ACHIEVEMENT.GOAL_POISON_CLOUD, 
			SUB_GOAL.GOAL_FROSTY_FOG => ACHIEVEMENT.GOAL_FROSTY_FOG, 
			SUB_GOAL.GOAL_BALL_LIGHTNING => ACHIEVEMENT.GOAL_BALL_LIGHTNING, 
			_ => throw new ArgumentOutOfRangeException("p_subGoal", p_subGoal, null), 
		};
	}

	public static bool HasEmotion(this string p_emotion)
	{
		if (!string.IsNullOrEmpty(p_emotion) && !LocalizationManager.Instance.HasLocalizedValue("ShareIntel_Table", p_emotion))
		{
			return true;
		}
		return false;
	}

	public static void LogString(this string p_string)
	{
	}

	public static string GetShortcutIcon(this string p_shortcut)
	{
		return p_shortcut switch
		{
			"A" => "<sprite=\"Text_Sprites\" name=\"XboxOne_A\">", 
			"B" => "<sprite=\"Text_Sprites\" name=\"XboxOne_B\">", 
			"X" => "<sprite=\"Text_Sprites\" name=\"XboxOne_X\">", 
			"Y" => "<sprite=\"Text_Sprites\" name=\"XboxOne_Y\">", 
			"Select" => "<sprite=\"Text_Sprites\" name=\"XboxOne_Windows\">", 
			"Start" => "<sprite=\"Text_Sprites\" name=\"XboxOne_Menu\">", 
			_ => string.Empty, 
		};
	}
}

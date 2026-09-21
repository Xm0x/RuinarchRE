using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UtilityScripts;

namespace Characters.Villager_Wants;

public abstract class VillagerWant
{
	public abstract int priority { get; }

	public abstract string name { get; }

	public abstract bool CanVillagerObtainWant(Character p_character, out LocationStructure p_preferredStructure, out TileObject p_foundObject);

	public virtual void OnWantToggledOn(Character p_character)
	{
	}

	public virtual void OnWantToggledOff(Character p_character)
	{
	}

	public abstract bool IsWantValid(Character p_character);

	protected bool CharacterLivesInAValidHomeForFoodWants(Character p_character)
	{
		if (p_character.homeStructure == null)
		{
			return false;
		}
		return true;
	}

	protected bool CharacterLivesInAValidHomeForFurnitureWants(Character p_character)
	{
		if (p_character.homeStructure == null || p_character.homeStructure.structureType == STRUCTURE_TYPE.CITY_CENTER)
		{
			return false;
		}
		return true;
	}

	protected bool CharacterLivesInAValidHome(Character p_character)
	{
		if (p_character.homeStructure == null || (p_character.homeStructure.structureType != STRUCTURE_TYPE.DWELLING && p_character.homeStructure.structureType != STRUCTURE_TYPE.VAMPIRE_CASTLE))
		{
			return false;
		}
		return true;
	}

	protected bool CharacterLivesInAVillage(Character p_character)
	{
		if (p_character.homeSettlement == null || p_character.homeSettlement.locationType != LOCATION_TYPE.VILLAGE)
		{
			return false;
		}
		return true;
	}

	protected bool CharacterIsInAVillage(Character p_character)
	{
		if (p_character.currentSettlement == null || p_character.currentSettlement.locationType != LOCATION_TYPE.VILLAGE)
		{
			return false;
		}
		return true;
	}

	protected bool CharacterHasFaction(Character p_character)
	{
		if (p_character.faction == null || !p_character.faction.isMajorFaction)
		{
			return false;
		}
		return true;
	}

	protected NPCSettlement GetValidSettlementForBasicResource(Character p_character)
	{
		if (p_character.currentSettlement is NPCSettlement result && p_character.currentSettlement.locationType == LOCATION_TYPE.VILLAGE)
		{
			return result;
		}
		if (p_character.homeSettlement != null && p_character.homeSettlement.locationType == LOCATION_TYPE.VILLAGE)
		{
			return p_character.homeSettlement;
		}
		return null;
	}

	protected NPCSettlement GetValidSettlementForFood(Character p_character)
	{
		if (p_character.currentSettlement is NPCSettlement result && p_character.currentSettlement.locationType == LOCATION_TYPE.VILLAGE)
		{
			return result;
		}
		if (p_character.homeSettlement != null && p_character.homeSettlement.locationType == LOCATION_TYPE.VILLAGE)
		{
			return p_character.homeSettlement;
		}
		return null;
	}

	protected bool HasBasicResourceProducingStructureInSameVillageOwnedByValidCharacter(Character p_character, NPCSettlement p_settlement, out bool needsToPay, out LocationStructure foundStructure, int neededResourceAmount)
	{
		if (!p_settlement.HasBasicResourceProducingStructure())
		{
			needsToPay = true;
			foundStructure = null;
			return false;
		}
		if (p_character.structureComponent.HasWorkPlaceStructure() && p_character.structureComponent.workPlaceStructure.structureType.IsBasicResourceProducingStructureForFaction(p_character.faction.factionType.type))
		{
			bool flag = false;
			if (p_character.faction.factionType.type == FACTION_TYPE.Human_Empire)
			{
				flag = p_character.structureComponent.workPlaceStructure.HasBuiltResourcePileOfTypeThatHasResourceAmount(TILE_OBJECT_TYPE.STONE_PILE, neededResourceAmount);
			}
			else if (p_character.faction.factionType.type == FACTION_TYPE.Elven_Kingdom)
			{
				flag = p_character.structureComponent.workPlaceStructure.HasBuiltResourcePileOfTypeThatHasResourceAmount(TILE_OBJECT_TYPE.WOOD_PILE, neededResourceAmount);
			}
			else if (p_character.faction.factionType.type == FACTION_TYPE.Demon_Cult || p_character.faction.factionType.type == FACTION_TYPE.Lycan_Clan || p_character.faction.factionType.type == FACTION_TYPE.Vampire_Clan || p_character.faction.factionType.type == FACTION_TYPE.Divine_Church || p_character.faction.factionType.type == FACTION_TYPE.Wiccans)
			{
				flag = p_character.structureComponent.workPlaceStructure.HasBuiltResourcePileOfTypeThatHasResourceAmount(TILE_OBJECT_TYPE.STONE_PILE, neededResourceAmount) || p_character.structureComponent.workPlaceStructure.HasBuiltResourcePileOfTypeThatHasResourceAmount(TILE_OBJECT_TYPE.WOOD_PILE, neededResourceAmount);
			}
			if (flag)
			{
				needsToPay = true;
				foundStructure = p_character.structureComponent.workPlaceStructure;
				return true;
			}
		}
		List<LocationStructure> list = RuinarchListPool<LocationStructure>.Claim();
		if (p_character.faction.factionType.type == FACTION_TYPE.Human_Empire || p_character.faction.factionType.type == FACTION_TYPE.Demon_Cult || p_character.faction.factionType.type == FACTION_TYPE.Lycan_Clan || p_character.faction.factionType.type == FACTION_TYPE.Vampire_Clan || p_character.faction.factionType.type == FACTION_TYPE.Divine_Church || p_character.faction.factionType.type == FACTION_TYPE.Wiccans)
		{
			List<LocationStructure> structuresOfType = p_settlement.GetStructuresOfType(STRUCTURE_TYPE.MINE);
			if (structuresOfType != null)
			{
				list.AddRange(structuresOfType);
			}
		}
		if (p_character.faction.factionType.type == FACTION_TYPE.Elven_Kingdom || p_character.faction.factionType.type == FACTION_TYPE.Demon_Cult || p_character.faction.factionType.type == FACTION_TYPE.Lycan_Clan || p_character.faction.factionType.type == FACTION_TYPE.Vampire_Clan || p_character.faction.factionType.type == FACTION_TYPE.Divine_Church || p_character.faction.factionType.type == FACTION_TYPE.Wiccans)
		{
			List<LocationStructure> structuresOfType2 = p_settlement.GetStructuresOfType(STRUCTURE_TYPE.LUMBERYARD);
			if (structuresOfType2 != null)
			{
				list.AddRange(structuresOfType2);
			}
		}
		foundStructure = null;
		needsToPay = true;
		if (list.Count > 0)
		{
			list.Shuffle();
			for (int i = 0; i < list.Count; i++)
			{
				ManMadeStructure manMadeStructure = list[i] as ManMadeStructure;
				bool flag2 = false;
				if (p_character.faction.factionType.type == FACTION_TYPE.Human_Empire)
				{
					flag2 = manMadeStructure.HasBuiltResourcePileOfTypeThatHasResourceAmount(TILE_OBJECT_TYPE.STONE_PILE, neededResourceAmount);
				}
				else if (p_character.faction.factionType.type == FACTION_TYPE.Elven_Kingdom)
				{
					flag2 = manMadeStructure.HasBuiltResourcePileOfTypeThatHasResourceAmount(TILE_OBJECT_TYPE.WOOD_PILE, neededResourceAmount);
				}
				else if (p_character.faction.factionType.type == FACTION_TYPE.Demon_Cult || p_character.faction.factionType.type == FACTION_TYPE.Lycan_Clan || p_character.faction.factionType.type == FACTION_TYPE.Vampire_Clan || p_character.faction.factionType.type == FACTION_TYPE.Divine_Church || p_character.faction.factionType.type == FACTION_TYPE.Wiccans)
				{
					flag2 = manMadeStructure.HasBuiltResourcePileOfTypeThatHasResourceAmount(TILE_OBJECT_TYPE.STONE_PILE, neededResourceAmount) || manMadeStructure.HasBuiltResourcePileOfTypeThatHasResourceAmount(TILE_OBJECT_TYPE.WOOD_PILE, neededResourceAmount);
				}
				if (flag2)
				{
					foundStructure = manMadeStructure;
					break;
				}
			}
		}
		RuinarchListPool<LocationStructure>.Release(list);
		return foundStructure != null;
	}

	protected bool HasWorkshopInSameVillageOwnedByValidCharacter(Character p_character, out bool needsToPay, out LocationStructure preferredStructure)
	{
		preferredStructure = null;
		if (!p_character.homeSettlement.HasStructure(STRUCTURE_TYPE.WORKSHOP))
		{
			needsToPay = true;
			return false;
		}
		if (p_character.structureComponent.HasWorkPlaceStructure() && p_character.structureComponent.workPlaceStructure is Workshop workshop && !workshop.IsCharacterAlreadyHasRequest(p_character))
		{
			needsToPay = false;
			preferredStructure = p_character.structureComponent.workPlaceStructure;
			return true;
		}
		List<LocationStructure> structuresOfType = p_character.homeSettlement.GetStructuresOfType(STRUCTURE_TYPE.WORKSHOP);
		needsToPay = true;
		int num = int.MinValue;
		for (int i = 0; i < structuresOfType.Count; i++)
		{
			LocationStructure locationStructure = structuresOfType[i];
			ManMadeStructure manMadeStructure = locationStructure as ManMadeStructure;
			Workshop workshop2 = locationStructure as Workshop;
			if (manMadeStructure.CanPurchaseFromHere(p_character, out var needsToPay2, out var buyerOpinionOfWorker) && !workshop2.IsCharacterAlreadyHasRequest(p_character) && buyerOpinionOfWorker > num)
			{
				num = buyerOpinionOfWorker;
				preferredStructure = manMadeStructure;
				needsToPay = needsToPay2;
			}
		}
		return preferredStructure != null;
	}

	protected bool HasHospiceOrTavernInSameVillageOwnedByValidCharacter(Character p_character, out bool needsToPay, out LocationStructure preferredStructure)
	{
		if (!p_character.homeSettlement.HasStructure(STRUCTURE_TYPE.HOSPICE) && !p_character.homeSettlement.HasStructure(STRUCTURE_TYPE.TAVERN))
		{
			needsToPay = true;
			preferredStructure = null;
			return false;
		}
		if (p_character.structureComponent.HasWorkPlaceStructure() && (p_character.structureComponent.workPlaceStructure.structureType == STRUCTURE_TYPE.HOSPICE || p_character.structureComponent.workPlaceStructure.structureType == STRUCTURE_TYPE.TAVERN) && p_character.structureComponent.workPlaceStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.HEALING_POTION))
		{
			needsToPay = false;
			preferredStructure = p_character.structureComponent.workPlaceStructure;
			return true;
		}
		List<LocationStructure> structuresOfType = p_character.homeSettlement.GetStructuresOfType(STRUCTURE_TYPE.HOSPICE);
		List<LocationStructure> structuresOfType2 = p_character.homeSettlement.GetStructuresOfType(STRUCTURE_TYPE.TAVERN);
		List<LocationStructure> list = RuinarchListPool<LocationStructure>.Claim();
		if (structuresOfType != null)
		{
			list.AddRange(structuresOfType);
		}
		if (structuresOfType2 != null)
		{
			list.AddRange(structuresOfType2);
		}
		preferredStructure = null;
		needsToPay = true;
		int num = int.MinValue;
		for (int i = 0; i < list.Count; i++)
		{
			ManMadeStructure manMadeStructure = list[i] as ManMadeStructure;
			if (manMadeStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.HEALING_POTION) && manMadeStructure.CanPurchaseFromHere(p_character, out var needsToPay2, out var buyerOpinionOfWorker) && buyerOpinionOfWorker > num)
			{
				num = buyerOpinionOfWorker;
				preferredStructure = manMadeStructure;
				needsToPay = needsToPay2;
			}
		}
		return preferredStructure != null;
	}

	public override string ToString()
	{
		return name;
	}
}

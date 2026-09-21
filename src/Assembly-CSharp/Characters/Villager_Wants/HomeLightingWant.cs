using Inner_Maps.Location_Structures;

namespace Characters.Villager_Wants;

public class HomeLightingWant : FurnitureWant
{
	public override int priority => 5;

	public override string name => "Home Torch";

	public override bool CanVillagerObtainWant(Character p_character, out LocationStructure p_preferredStructure, out TileObject p_foundObject)
	{
		if (!CharacterHasFaction(p_character))
		{
			p_preferredStructure = null;
			p_foundObject = null;
			return false;
		}
		if (!CharacterLivesInAValidHome(p_character))
		{
			p_preferredStructure = null;
			p_foundObject = null;
			return false;
		}
		if (!CharacterLivesInAVillage(p_character) && !CharacterIsInAVillage(p_character))
		{
			p_preferredStructure = null;
			p_foundObject = null;
			return false;
		}
		NPCSettlement validSettlementForBasicResource = GetValidSettlementForBasicResource(p_character);
		if (validSettlementForBasicResource == null)
		{
			p_preferredStructure = null;
			p_foundObject = null;
			return false;
		}
		if (!HasBuildingFurnitureInProgress(p_character))
		{
			TileObjectData tileObjectData = TileObjectDB.GetTileObjectData(GetFurnitureWanted(p_character));
			if (OwnsEnoughResource(p_character, tileObjectData.craftResourceCost, out p_foundObject))
			{
				p_preferredStructure = null;
				return true;
			}
			if (!HasBasicResourceProducingStructureInSameVillageOwnedByValidCharacter(p_character, validSettlementForBasicResource, out var needsToPay, out p_preferredStructure, tileObjectData.craftResourceCost))
			{
				p_foundObject = null;
				return false;
			}
			if (needsToPay && !p_character.moneyComponent.CanAfford(20))
			{
				p_foundObject = null;
				return false;
			}
		}
		else
		{
			p_preferredStructure = null;
			p_foundObject = null;
		}
		p_foundObject = null;
		return true;
	}

	public override bool IsWantValid(Character p_character)
	{
		if (!CharacterLivesInAValidHome(p_character))
		{
			return false;
		}
		if (!p_character.homeStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.DIVINE_ORB))
		{
			return !p_character.homeStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.TORCH);
		}
		return false;
	}

	public override TILE_OBJECT_TYPE GetFurnitureWanted(Character p_character)
	{
		if (p_character.faction != null && p_character.faction.factionType.type == FACTION_TYPE.Divine_Church)
		{
			return TILE_OBJECT_TYPE.DIVINE_ORB;
		}
		return TILE_OBJECT_TYPE.TORCH;
	}
}

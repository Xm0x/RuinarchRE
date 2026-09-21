using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UtilityScripts;

namespace Characters.Villager_Wants;

public abstract class FoodWant : ItemWant
{
	protected bool HasFoodProducingStructureInSameVillageOwnedByValidCharacter(Character p_character, NPCSettlement p_settlement, out bool needsToPay, out LocationStructure foundStructure)
	{
		if (!p_settlement.HasFoodProducingStructure())
		{
			needsToPay = true;
			foundStructure = null;
			return false;
		}
		if (p_character.structureComponent.HasWorkPlaceStructure() && p_character.structureComponent.workPlaceStructure.HasTileObjectThatIsBuiltFoodPileThatCharacterCanEatAndDoesntHaveAtHome(p_character))
		{
			needsToPay = false;
			foundStructure = p_character.structureComponent.workPlaceStructure;
			return true;
		}
		List<LocationStructure> list = RuinarchListPool<LocationStructure>.Claim();
		List<LocationStructure> structuresOfType = p_settlement.GetStructuresOfType(STRUCTURE_TYPE.BUTCHERS_SHOP);
		if (structuresOfType != null)
		{
			list.AddRange(structuresOfType);
		}
		List<LocationStructure> structuresOfType2 = p_settlement.GetStructuresOfType(STRUCTURE_TYPE.FARM);
		if (structuresOfType2 != null)
		{
			list.AddRange(structuresOfType2);
		}
		List<LocationStructure> structuresOfType3 = p_settlement.GetStructuresOfType(STRUCTURE_TYPE.FISHERY);
		if (structuresOfType3 != null)
		{
			list.AddRange(structuresOfType3);
		}
		foundStructure = null;
		needsToPay = true;
		if (list.Count > 0)
		{
			list.Shuffle();
			for (int i = 0; i < list.Count; i++)
			{
				ManMadeStructure manMadeStructure = list[i] as ManMadeStructure;
				if (manMadeStructure.HasTileObjectThatIsBuiltFoodPileThatCharacterCanEatAndDoesntHaveAtHome(p_character))
				{
					foundStructure = manMadeStructure;
					break;
				}
			}
		}
		RuinarchListPool<LocationStructure>.Release(list);
		return foundStructure != null;
	}

	protected bool HasOwnedFoodNotAtHome(Character p_character, out TileObject p_foundObject)
	{
		for (int i = 0; i < p_character.ownedItems.Count; i++)
		{
			TileObject tileObject = p_character.ownedItems[i];
			if (tileObject is FoodPile && tileObject.gridTileLocation != null && tileObject.gridTileLocation.structure != p_character.homeStructure)
			{
				p_foundObject = tileObject;
				return true;
			}
		}
		p_foundObject = null;
		return false;
	}

	public override bool CanObjectSatisfyWant(TileObject p_tileObject, Character p_character)
	{
		return p_tileObject is FoodPile;
	}
}

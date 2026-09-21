using Inner_Maps.Location_Structures;

namespace Characters.Villager_Wants;

public class FoodWant_3 : FoodWant
{
	public override int priority => 1;

	public override string name => "Food Want 3";

	public override bool CanVillagerObtainWant(Character p_character, out LocationStructure p_preferredStructure, out TileObject p_foundObject)
	{
		if (!CharacterHasFaction(p_character))
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
		if (!CharacterLivesInAValidHomeForFoodWants(p_character))
		{
			p_preferredStructure = null;
			p_foundObject = null;
			return false;
		}
		if (HasOwnedFoodNotAtHome(p_character, out p_foundObject))
		{
			p_preferredStructure = null;
			return true;
		}
		NPCSettlement validSettlementForFood = GetValidSettlementForFood(p_character);
		if (validSettlementForFood == null)
		{
			p_preferredStructure = null;
			p_foundObject = null;
			return false;
		}
		if (!HasFoodProducingStructureInSameVillageOwnedByValidCharacter(p_character, validSettlementForFood, out var needsToPay, out p_preferredStructure))
		{
			return false;
		}
		if (needsToPay && !p_character.moneyComponent.CanAfford(10))
		{
			return false;
		}
		return true;
	}

	public override bool IsWantValid(Character p_character)
	{
		if (p_character.traitContainer.HasTrait("Vampire"))
		{
			return false;
		}
		if (!CharacterLivesInAValidHomeForFoodWants(p_character))
		{
			return false;
		}
		if (p_character.homeStructure is Dwelling dwelling)
		{
			return dwelling.differentFoodPileKindsInDwelling < 3;
		}
		return false;
	}
}

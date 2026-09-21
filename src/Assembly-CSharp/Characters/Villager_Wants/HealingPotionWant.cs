using Inner_Maps.Location_Structures;

namespace Characters.Villager_Wants;

public class HealingPotionWant : ItemWant
{
	public override int priority => 7;

	public override string name => "Healing Potion";

	public override bool CanVillagerObtainWant(Character p_character, out LocationStructure p_preferredStructure, out TileObject p_foundObject)
	{
		if (!CharacterHasFaction(p_character))
		{
			p_preferredStructure = null;
			p_foundObject = null;
			return false;
		}
		if (!CharacterLivesInAVillage(p_character))
		{
			p_preferredStructure = null;
			p_foundObject = null;
			return false;
		}
		if (!HasHospiceOrTavernInSameVillageOwnedByValidCharacter(p_character, out var needsToPay, out p_preferredStructure))
		{
			p_foundObject = null;
			return false;
		}
		if (needsToPay && !p_character.moneyComponent.CanAfford(5))
		{
			p_foundObject = null;
			return false;
		}
		p_foundObject = null;
		return true;
	}

	public override bool IsWantValid(Character p_character)
	{
		return !p_character.HasItem(TILE_OBJECT_TYPE.HEALING_POTION);
	}

	public override bool CanObjectSatisfyWant(TileObject p_tileObject, Character p_character)
	{
		return p_tileObject.tileObjectType == TILE_OBJECT_TYPE.HEALING_POTION;
	}
}

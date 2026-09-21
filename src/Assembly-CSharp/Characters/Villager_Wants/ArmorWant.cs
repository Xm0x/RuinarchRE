using Inner_Maps.Location_Structures;

namespace Characters.Villager_Wants;

public class ArmorWant : EquipmentWant
{
	public override int priority => 8;

	public override string name => "Armor";

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
		if (!HasWorkshopInSameVillageOwnedByValidCharacter(p_character, out var needsToPay, out p_preferredStructure))
		{
			p_foundObject = null;
			return false;
		}
		if (needsToPay && !p_character.moneyComponent.CanAfford(30))
		{
			p_foundObject = null;
			return false;
		}
		p_foundObject = null;
		return true;
	}

	public override bool IsWantValid(Character p_character)
	{
		return p_character.equipmentComponent.currentArmor == null;
	}

	public override bool CanObjectSatisfyWant(TileObject p_tileObject, Character p_character)
	{
		return p_tileObject is ArmorItem;
	}
}

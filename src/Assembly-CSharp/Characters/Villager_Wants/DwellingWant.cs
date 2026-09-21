using Inner_Maps.Location_Structures;

namespace Characters.Villager_Wants;

public class DwellingWant : VillagerWant
{
	public override int priority => 12;

	public override string name => "Dwelling";

	public override bool CanVillagerObtainWant(Character p_character, out LocationStructure p_preferredStructure, out TileObject p_foundObject)
	{
		p_preferredStructure = null;
		p_foundObject = null;
		if (!p_character.moneyComponent.CanAfford(50))
		{
			return false;
		}
		if (p_character.faction == null || !p_character.faction.isMajorFaction)
		{
			return false;
		}
		if (p_character.homeSettlement == null || p_character.homeSettlement.locationType != LOCATION_TYPE.VILLAGE)
		{
			return false;
		}
		if (!p_character.homeSettlement.HasUnclaimedDwellingThatIsNotPreviousHome(p_character, out p_preferredStructure))
		{
			return false;
		}
		return true;
	}

	public override bool IsWantValid(Character p_character)
	{
		return !CharacterLivesInAValidHome(p_character);
	}
}

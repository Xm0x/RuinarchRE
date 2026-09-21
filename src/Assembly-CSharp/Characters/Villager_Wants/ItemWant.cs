namespace Characters.Villager_Wants;

public abstract class ItemWant : VillagerWant
{
	public abstract bool CanObjectSatisfyWant(TileObject p_tileObject, Character p_character);
}

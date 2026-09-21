using Traits;

public class SaveDataKnightPartyBonus : SaveDataTrait
{
	public int level;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		KnightPartyBonus knightPartyBonus = trait as KnightPartyBonus;
		level = knightPartyBonus.level;
	}
}

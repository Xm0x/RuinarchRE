using Traits;

public class SaveDataFrozen : SaveDataTrait
{
	public bool isPlayerSource;

	public GameDate lastFrostbiteStackDate;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Frozen frozen = trait as Frozen;
		isPlayerSource = frozen.isPlayerSource;
		lastFrostbiteStackDate = frozen.lastFrostbiteStackDate;
	}
}

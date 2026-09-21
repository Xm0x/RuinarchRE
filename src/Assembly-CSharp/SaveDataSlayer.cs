using Traits;

public class SaveDataSlayer : SaveDataTrait
{
	public int stackCount;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Slayer slayer = trait as Slayer;
		stackCount = slayer.stackCount;
	}
}

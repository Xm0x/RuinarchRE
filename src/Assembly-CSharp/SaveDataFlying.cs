using Traits;

public class SaveDataFlying : SaveDataTrait
{
	public int stackCount;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Flying flying = trait as Flying;
		stackCount = flying.stackCount;
	}
}

using Traits;

public class SaveDataWard : SaveDataTrait
{
	public int stackCount;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Ward ward = trait as Ward;
		stackCount = ward.stackCount;
	}
}

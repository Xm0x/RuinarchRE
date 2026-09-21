using Traits;

public class SaveDataCatatonic : SaveDataTrait
{
	public float chanceToRemove;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Catatonic catatonic = trait as Catatonic;
		chanceToRemove = catatonic.chanceToRemove;
	}
}

using Traits;

public class SaveDataEmpowered : SaveDataTrait
{
	public float modificationPercent;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Empowered empowered = trait as Empowered;
		modificationPercent = empowered.modificationPercent;
	}
}

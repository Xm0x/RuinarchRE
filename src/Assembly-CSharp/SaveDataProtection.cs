using Traits;

public class SaveDataProtection : SaveDataTrait
{
	public int appliedAllResistances;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Protection protection = trait as Protection;
		appliedAllResistances = protection.appliedAllResistances;
	}
}

using Traits;

public class SaveDataPolymorphed : SaveDataTrait
{
	public SUMMON_TYPE animalType;

	public ELEMENTAL_TYPE originalElement;

	public int strengthReduction;

	public int intelligenceReduction;

	public float piercingPowerReduction;

	public float fireResistanceReduction;

	public float poisonResistanceReduction;

	public float waterResistanceReduction;

	public float iceResistanceReduction;

	public float electricResistanceReduction;

	public float earthResistanceReduction;

	public float windResistanceReduction;

	public float mentalResistanceReduction;

	public float physicalResistanceReduction;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Polymorphed polymorphed = trait as Polymorphed;
		animalType = polymorphed.animalType;
		strengthReduction = polymorphed.strengthReduction;
		intelligenceReduction = polymorphed.intelligenceReduction;
		piercingPowerReduction = polymorphed.piercingPowerReduction;
		fireResistanceReduction = polymorphed.fireResistanceReduction;
		poisonResistanceReduction = polymorphed.poisonResistanceReduction;
		waterResistanceReduction = polymorphed.waterResistanceReduction;
		iceResistanceReduction = polymorphed.iceResistanceReduction;
		electricResistanceReduction = polymorphed.electricResistanceReduction;
		earthResistanceReduction = polymorphed.earthResistanceReduction;
		windResistanceReduction = polymorphed.windResistanceReduction;
		mentalResistanceReduction = polymorphed.mentalResistanceReduction;
		physicalResistanceReduction = polymorphed.physicalResistanceReduction;
		originalElement = polymorphed.originalElement;
	}
}

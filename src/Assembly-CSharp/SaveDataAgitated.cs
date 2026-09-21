using Traits;

public class SaveDataAgitated : SaveDataTrait
{
	public float addedAttackPercent;

	public float addedHPPercent;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Agitated agitated = trait as Agitated;
		addedAttackPercent = agitated.addedAttackPercent;
		addedHPPercent = agitated.addedHPPercent;
	}
}

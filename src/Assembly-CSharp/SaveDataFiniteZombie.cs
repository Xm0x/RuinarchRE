using Traits;

public class SaveDataFiniteZombie : SaveDataTrait
{
	public bool hasTurnedAtLeastOnce;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		FiniteZombie finiteZombie = trait as FiniteZombie;
		hasTurnedAtLeastOnce = finiteZombie.hasTurnedAtLeastOnce;
	}
}

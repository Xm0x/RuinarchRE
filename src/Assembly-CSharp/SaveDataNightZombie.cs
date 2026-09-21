using Traits;

public class SaveDataNightZombie : SaveDataTrait
{
	public bool hasTurnedAtLeastOnce;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		NightZombie nightZombie = trait as NightZombie;
		hasTurnedAtLeastOnce = nightZombie.hasTurnedAtLeastOnce;
	}
}

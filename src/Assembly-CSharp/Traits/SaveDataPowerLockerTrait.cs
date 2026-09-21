namespace Traits;

public class SaveDataPowerLockerTrait : SaveDataTrait
{
	public PLAYER_SKILL_TYPE lockedSkill;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		PowerLockerTrait powerLockerTrait = trait as PowerLockerTrait;
		lockedSkill = powerLockerTrait.lockedSkill;
	}
}

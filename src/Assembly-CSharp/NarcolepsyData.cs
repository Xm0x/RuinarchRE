public class NarcolepsyData : AfflictData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.NARCOLEPSY;

	public override string name => "Narcolepsy";

	public override string description => "This Affliction will make a Villager Narcoleptic. Narcoleptic villagers may involuntarily fall asleep at any time and will produce a Chaos Orb whenever they do this.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.AFFLICTION;

	public override string afflictionTraitName => "Narcoleptic";

	public NarcolepsyData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		PlayerApplyAfflictionToTarget(targetPOI);
		OnExecutePlayerSkill();
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		if (targetCharacter.isDead)
		{
			return false;
		}
		return base.CanPerformAbilityTowards(targetCharacter);
	}
}

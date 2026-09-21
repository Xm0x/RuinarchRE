public class PyrophobiaData : AfflictData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.PYROPHOBIA;

	public override string name => "Pyrophobia";

	public override string description => "This Affliction will make a Villager fear fire. Pyrophobic Villagers will always flee or cower whenever it gets close to a fire.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.AFFLICTION;

	public override string afflictionTraitName => "Pyrophobic";

	public PyrophobiaData()
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

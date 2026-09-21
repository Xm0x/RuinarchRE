public class GluttonyData : AfflictData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.GLUTTONY;

	public override string name => "Gluttony";

	public override string description => "This Affliction will turn a Villager into a Glutton. Gluttons get hungry faster.\nA Glutton produces a Chaos Orb each time it eats.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.AFFLICTION;

	public override string afflictionTraitName => "Glutton";

	public GluttonyData()
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

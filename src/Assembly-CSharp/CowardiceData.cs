public class CowardiceData : AfflictData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.COWARDICE;

	public override string name => "Cowardice";

	public override string description => "This Affliction will turn a Villager into a Coward. Cowards will often run away from combat and will produce a Chaos Orb whenever they do this.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.AFFLICTION;

	public override string afflictionTraitName => "Coward";

	public CowardiceData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		int overridenDuration = TraitManager.Instance.allTraits["Coward"].ticksDuration + PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.COWARDICE);
		PlayerApplyAfflictionToTarget(targetPOI, overridenDuration);
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

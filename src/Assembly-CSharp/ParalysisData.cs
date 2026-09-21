public class ParalysisData : AfflictData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.PARALYSIS;

	public override string name => "Paralysis";

	public override string description => "This Affliction will prevent a Villager from moving.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.AFFLICTION;

	public override string afflictionTraitName => "Paralyzed";

	public ParalysisData()
	{
		base.targetTypes = new SPELL_TARGET[2]
		{
			SPELL_TARGET.CHARACTER,
			SPELL_TARGET.TILE_OBJECT
		};
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		int durationBonusPerLevel = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(PLAYER_SKILL_TYPE.PARALYSIS).skillUpgradeData.GetDurationBonusPerLevel(base.currentLevel);
		PlayerApplyAfflictionToTarget(targetPOI, durationBonusPerLevel);
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

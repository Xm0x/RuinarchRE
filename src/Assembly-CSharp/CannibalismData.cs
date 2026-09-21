public class CannibalismData : AfflictData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.CANNIBALISM;

	public override string name => "Cannibalism";

	public override string description => "Makes a character eat other characters with the same race for sustenance.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.AFFLICTION;

	public override string afflictionTraitName => "Cannibal";

	public CannibalismData()
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
		if (targetCharacter.isDead || targetCharacter.race == RACE.SKELETON)
		{
			return false;
		}
		return base.CanPerformAbilityTowards(targetCharacter);
	}
}

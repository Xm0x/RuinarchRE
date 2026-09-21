public class HotheadedData : AfflictData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.HOTHEADED;

	public override string name => "Hotheaded";

	public override string description => "This Affliction will make the Villager more prone to angry fits. Often gets triggered when seeing other people.\nA hothead produces a Chaos Orb each time it gets Angry.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.AFFLICTION;

	public override string afflictionTraitName => "Hothead";

	public HotheadedData()
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

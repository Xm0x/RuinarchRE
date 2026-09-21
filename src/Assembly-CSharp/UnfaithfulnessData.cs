public class UnfaithfulnessData : AfflictData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.UNFAITHFULNESS;

	public override string name => "Unfaithfulness";

	public override string description => "This Affliction will make a Villager Unfaithful. Unfaithful Villagers may flirt and develop Affairs even if they are already in a relationship.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.AFFLICTION;

	public override string afflictionTraitName => "Unfaithful";

	public UnfaithfulnessData()
	{
		base.targetTypes = new SPELL_TARGET[2]
		{
			SPELL_TARGET.CHARACTER,
			SPELL_TARGET.TILE_OBJECT
		};
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		PlayerApplyAfflictionToTarget(targetPOI);
		OnExecutePlayerSkill();
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		if (targetCharacter.isDead || targetCharacter.race == RACE.SKELETON || targetCharacter.traitContainer.HasTrait("Beast"))
		{
			return false;
		}
		return base.CanPerformAbilityTowards(targetCharacter);
	}
}

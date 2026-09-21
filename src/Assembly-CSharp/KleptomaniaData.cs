public class KleptomaniaData : AfflictData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.KLEPTOMANIA;

	public override string name => "Kleptomania";

	public override string description => "This Affliction will turn a Villager into a Kleptomaniac. Kleptomaniacs will sometimes steal objects owned by others.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.AFFLICTION;

	public override string afflictionTraitName => "Kleptomaniac";

	public KleptomaniaData()
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

public class VampirismData : AfflictData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.VAMPIRISM;

	public override string name => "Vampirism";

	public override string description => "This Affliction will turn a Villager into a Vampire. A Vampire's Energy Meter no longer decreases but they have to drink other Villager's blood to survive.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.AFFLICTION;

	public override string afflictionTraitName => "Vampire";

	public VampirismData()
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
		if (targetCharacter.HasItem(TILE_OBJECT_TYPE.PHYLACTERY))
		{
			return false;
		}
		if (targetCharacter.classComponent.IsStalkerCannotBeTurned())
		{
			return false;
		}
		return base.CanPerformAbilityTowards(targetCharacter);
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
		if (targetCharacter.HasItem(TILE_OBJECT_TYPE.PHYLACTERY))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Target_Has_Phylactery", targetCharacter) + "|";
		}
		if (targetCharacter.classComponent.IsStalkerCannotBeTurned())
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Stalker_Cannot_Be_Turned", targetCharacter) + "|";
		}
		return text;
	}
}

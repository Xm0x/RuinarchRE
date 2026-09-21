using Traits;

public class LycanthropyData : AfflictData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.LYCANTHROPY;

	public override string name => "Lycanthropy";

	public override string description => "This Affliction will turn a Villager into a Werewolf. A Werewolf sometimes switches from normal form to wolf form and vice-versa whenever it sleeps.\nA Lycanthrope produces 2 Chaos Orbs whenever it sheds a Wolf Pelt. It also produces 2 Chaos Orbs each time it kills a Villager.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.AFFLICTION;

	public override string afflictionTraitName => "Lycanthrope";

	public LycanthropyData()
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

	public override void ApplyAfflictionEffects(IPointOfInterest target, int overridenDuration = 0)
	{
		new LycanthropeData(target as Character);
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

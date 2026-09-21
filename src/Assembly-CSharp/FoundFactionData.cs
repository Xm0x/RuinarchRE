public class FoundFactionData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.FOUND_FACTION;

	public override string name => "Found Faction";

	public override string description => "This Ability encourages the target to start their own Faction.";

	public FoundFactionData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		if (targetPOI is Character character)
		{
			character.interruptComponent.TriggerInterrupt(INTERRUPT.Create_Faction, character);
			base.ActivateAbility(targetPOI);
		}
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		if (targetCharacter.limiterComponent.IsIncapacitated())
		{
			return false;
		}
		if (FactionManager.Instance.GetActiveVillagerFactionCount() >= FactionManager.Instance.maxActiveVillagerFactions)
		{
			return false;
		}
		return base.CanPerformAbilityTowards(targetCharacter);
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
		if (targetCharacter.limiterComponent.IsIncapacitated())
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Target_Incapacitated") + "|";
		}
		if (FactionManager.Instance.GetActiveVillagerFactionCount() >= FactionManager.Instance.maxActiveVillagerFactions)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Max_Factions") + "|";
		}
		return text;
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		if (target is Character character && (character.isDead || !character.race.IsSapient() || !character.isVagrantOrFactionless))
		{
			return false;
		}
		return base.IsValid(target);
	}
}

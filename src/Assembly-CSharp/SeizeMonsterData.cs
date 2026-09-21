public class SeizeMonsterData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SEIZE_MONSTER;

	public override string name => "Seize Monster";

	public override string description => "This Ability can be used to take a Monster and then transfer it to an unoccupied tile.";

	public SeizeMonsterData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		PlayerManager.Instance.player.seizeComponent.SeizePOI(targetPOI);
		base.ActivateAbility(targetPOI);
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		bool flag = base.CanPerformAbilityTowards(targetCharacter);
		if (flag)
		{
			if (targetCharacter.race == RACE.TRITON || targetCharacter.race == RACE.UNICORN)
			{
				return false;
			}
			if (PlayerManager.Instance.player.underlingsComponent.IsInPersistentDefendParty(targetCharacter))
			{
				return false;
			}
			if (!PlayerManager.Instance.player.seizeComponent.hasSeizedPOI && !targetCharacter.traitContainer.HasTrait("Hibernating") && !targetCharacter.traitContainer.HasTrait("Being Drained"))
			{
				return !targetCharacter.traitContainer.HasTrait("Heavy");
			}
			return false;
		}
		return flag;
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
		if (targetCharacter.race == RACE.TRITON)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Seize_Monster_Triton") + "|";
		}
		else if (targetCharacter.race == RACE.UNICORN)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Cannot_Seize_Unicorn") + "|";
		}
		else if (PlayerManager.Instance.player.underlingsComponent.IsInPersistentDefendParty(targetCharacter))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Seize_Monster_In_Defend_Party") + "|";
		}
		else if (targetCharacter.traitContainer.HasTrait("Hibernating"))
		{
			text = ((!(targetCharacter is Golem)) ? (text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Seize_Monster_Hibernating") + "|") : (text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Seize_Monster_Hibernating_Golem") + "|"));
		}
		else if (targetCharacter.traitContainer.HasTrait("Being Drained"))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Seize_Being_Drained") + "|";
		}
		else if (targetCharacter.traitContainer.HasTrait("Heavy"))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Seize_Heavy") + "|";
		}
		return text;
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		if (target is Character character)
		{
			if (base.IsValid(target))
			{
				return !(character is Dragon);
			}
			return false;
		}
		return false;
	}
}

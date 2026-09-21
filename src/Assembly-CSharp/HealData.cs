public class HealData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.HEAL;

	public override string name => "Heal";

	public override string description => "This Ability replenishes a portion of the character's HP.";

	public HealData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		if (targetPOI is Character character)
		{
			int amount = (int)((float)character.maxHP * PlayerSkillManager.Instance.GetAdditionalHpPercentagePerLevelBaseOnLevel(PLAYER_SKILL_TYPE.HEAL) / 100f);
			character.AdjustHP(amount, ELEMENTAL_TYPE.Normal, triggerDeath: false, null, null, showHPBar: true, PlayerSkillManager.Instance.GetPierceBasedOnCurrentLevel(PLAYER_SKILL_TYPE.HEAL));
			Messenger.Broadcast(UISignals.UPDATE_CHARACTER_INFO, character);
		}
		base.ActivateAbility(targetPOI);
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		if (targetCharacter.isDead)
		{
			return false;
		}
		if (targetCharacter.IsHealthFull())
		{
			return false;
		}
		if (targetCharacter.traitContainer.HasTrait("Being Drained"))
		{
			return false;
		}
		return base.CanPerformAbilityTowards(targetCharacter);
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
		if (targetCharacter.isDead)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Already_Dead", targetCharacter) + "|";
		}
		if (targetCharacter.IsHealthFull())
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Already_Full_HP", targetCharacter) + "|";
		}
		if (targetCharacter.traitContainer.HasTrait("Being Drained"))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Being_Drained_Cannot_Heal") + "|";
		}
		return text;
	}
}

public class CultistTransformData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.CULTIST_TRANSFORM;

	public override string name => "Transform";

	public override string description => "This Action forces the character to transform into an abomination.";

	public override bool canBeCastOnBlessed => true;

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.PLAYER_ACTION;

	public CultistTransformData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		if (targetPOI is Character character && UIManager.Instance.characterInfoUI.isShowing && UIManager.Instance.characterInfoUI.activeCharacter == character)
		{
			UIManager.Instance.characterInfoUI.CloseMenu();
		}
		base.ActivateAbility(targetPOI);
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		if (base.CanPerformAbilityTowards(targetCharacter))
		{
			if (!targetCharacter.limiterComponent.canPerform)
			{
				return false;
			}
			return !targetCharacter.isDead;
		}
		return false;
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
		if (!targetCharacter.limiterComponent.canPerform)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Target_Incapacitated") + "|";
		}
		return text;
	}
}

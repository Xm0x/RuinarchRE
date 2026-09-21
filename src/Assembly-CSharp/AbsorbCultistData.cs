public class AbsorbCultistData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.ABSORB_CULTIST;

	public override string name => "Absorb Cultist";

	public override string description => "This Action forces the character to sacrifice itself and give you a bonus charge of its Resonance Power.";

	public override bool canBeCastOnBlessed => true;

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.PLAYER_ACTION;

	public AbsorbCultistData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		if (targetPOI is Character character)
		{
			character.jobComponent.TriggerCultistSacrifice();
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
			if ((targetCharacter.currentActionNode == null || targetCharacter.currentActionNode.action.goapType != INTERACTION_TYPE.SACRIFICE_SELF) && targetCharacter.jobQueue.HasJob(JOB_TYPE.SACRIFICE_SELF))
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
		if ((targetCharacter.currentActionNode == null || targetCharacter.currentActionNode.action.goapType != INTERACTION_TYPE.SACRIFICE_SELF) && targetCharacter.jobQueue.HasJob(JOB_TYPE.SACRIFICE_SELF))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Cultist_Absorb_Already_Instructed", targetCharacter) + "|";
		}
		return text;
	}
}

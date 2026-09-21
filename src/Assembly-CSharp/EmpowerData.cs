public class EmpowerData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.EMPOWER;

	public override string name => "Empower";

	public override string description => "This Ability will significantly increase a character's combat prowess for 12 hours.";

	public EmpowerData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		if (targetPOI is Character character)
		{
			if (character.traitContainer.AddTrait(character, "Empowered"))
			{
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "InterventionAbility", "PlayerPowerAlerts_Table", "Empower activated", LOG_TAG.Player);
				log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log.AddLogToDatabase();
				PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
			}
			base.ActivateAbility(targetPOI);
		}
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		if (targetCharacter.isDead || targetCharacter.traitContainer.HasTrait("Empowered"))
		{
			return false;
		}
		return base.CanPerformAbilityTowards(targetCharacter);
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
		if (targetCharacter.traitContainer.HasTrait("Empowered"))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Already_Empowered", targetCharacter) + "|";
		}
		return text;
	}
}

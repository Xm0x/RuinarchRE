public class SeizeCharacterData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SEIZE_CHARACTER;

	public override string name => "Seize Villager";

	public override string description => "This Action can be used to take a Villager and then transfer it to an unoccupied tile.";

	public SeizeCharacterData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		AkSoundEngine.PostEvent("Play_Seize_Villager", InnerMapCameraMove.Instance.gameObject);
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
			if (targetCharacter.traitContainer.HasTrait("Being Drained"))
			{
				return false;
			}
			if (targetCharacter.traitContainer.HasTrait("Heavy"))
			{
				return false;
			}
			if (targetCharacter.interruptComponent.isInterrupted && (targetCharacter.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Being_Brainwashed || targetCharacter.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Being_Tortured))
			{
				return false;
			}
			if (!PlayerManager.Instance.player.seizeComponent.hasSeizedPOI && targetCharacter.hasMarker)
			{
				return targetCharacter.grave == null;
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
		if (targetCharacter.traitContainer.HasTrait("Being Drained"))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Seize_Being_Drained") + "|";
		}
		if (targetCharacter.traitContainer.HasTrait("Heavy"))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Seize_Heavy") + "|";
		}
		return text;
	}
}

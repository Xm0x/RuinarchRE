using Inner_Maps.Location_Structures;

public class ReleaseData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.RELEASE;

	public override string name => "Release";

	public override string description => "This Ability releases a character that has been Restrained, Ensnared, Frozen or Enslaved.";

	public ReleaseData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		if (targetPOI is Character character)
		{
			character.traitContainer.RemoveRestrainAndImprison(character);
			character.traitContainer.RemoveStatusAndStacks(character, "Frozen");
			character.traitContainer.RemoveStatusAndStacks(character, "Ensnared");
			character.traitContainer.RemoveTrait(character, "Enslaved");
		}
		base.ActivateAbility(targetPOI);
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		bool flag = base.CanPerformAbilityTowards(targetCharacter);
		if (flag && targetCharacter.currentStructure is TortureChambers)
		{
			return false;
		}
		return flag;
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
		if (targetCharacter.currentStructure is TortureChambers)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Prison_Cannot_Release") + "|";
		}
		return text;
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		if (target is Character character)
		{
			if (base.IsValid(target))
			{
				return character.traitContainer.HasTrait("Restrained", "Ensnared", "Frozen", "Enslaved");
			}
			return false;
		}
		return false;
	}
}

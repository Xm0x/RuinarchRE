public class MusicHaterData : AfflictData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.MUSIC_HATER;

	public override string name => "Music Hater";

	public override string description => "This Affliction will make a Villager hate anything related to Music.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.AFFLICTION;

	public override string afflictionTraitName => "Music Hater";

	public MusicHaterData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		PlayerApplyAfflictionToTarget(targetPOI);
		OnExecutePlayerSkill();
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		if (targetCharacter.isDead || targetCharacter.traitContainer.HasTrait("Music Lover"))
		{
			return false;
		}
		return base.CanPerformAbilityTowards(targetCharacter);
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
		if (targetCharacter.traitContainer.HasTrait("Music Lover"))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Already_Music_Lover", targetCharacter) + "|";
		}
		return text;
	}
}

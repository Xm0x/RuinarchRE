using Traits;
using UtilityScripts;

public class GloomData : PlayerAction
{
	private string _bonusUIText = string.Empty;

	private string _bonusLevelUpUIText = string.Empty;

	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.GLOOM;

	public override string name => "Gloom";

	public override string description => "This Ability reduces the target’s Mood.";

	public GloomData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		if (targetPOI is Character character)
		{
			if (base.currentLevel == 0)
			{
				character.traitContainer.RemoveStatusAndStacks(character, "Gloomy2");
				character.traitContainer.RemoveStatusAndStacks(character, "Gloomy3");
				character.traitContainer.RemoveStatusAndStacks(character, "Gloomy4");
				character.traitContainer.AddTrait(character, "Gloomy1");
			}
			else if (base.currentLevel == 1)
			{
				character.traitContainer.RemoveStatusAndStacks(character, "Gloomy1");
				character.traitContainer.RemoveStatusAndStacks(character, "Gloomy3");
				character.traitContainer.RemoveStatusAndStacks(character, "Gloomy4");
				character.traitContainer.AddTrait(character, "Gloomy2");
			}
			else if (base.currentLevel == 2)
			{
				character.traitContainer.RemoveStatusAndStacks(character, "Gloomy1");
				character.traitContainer.RemoveStatusAndStacks(character, "Gloomy2");
				character.traitContainer.RemoveStatusAndStacks(character, "Gloomy4");
				character.traitContainer.AddTrait(character, "Gloomy3");
			}
			else
			{
				character.traitContainer.RemoveStatusAndStacks(character, "Gloomy1");
				character.traitContainer.RemoveStatusAndStacks(character, "Gloomy2");
				character.traitContainer.RemoveStatusAndStacks(character, "Gloomy3");
				character.traitContainer.AddTrait(character, "Gloomy4");
			}
			base.ActivateAbility(targetPOI);
		}
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		if (target is Character character && (character.isDead || !character.race.IsSapient()))
		{
			return false;
		}
		return base.IsValid(target);
	}

	protected override void OnLevelUp()
	{
		base.OnLevelUp();
		ResetBonusUIText();
		ResetBonusLevelUpUIText();
	}

	private void ResetBonusUIText()
	{
		_bonusUIText = string.Empty;
	}

	private void ResetBonusLevelUpUIText()
	{
		_bonusLevelUpUIText = string.Empty;
	}

	public override string GetBonusUIText()
	{
		if (string.IsNullOrEmpty(_bonusUIText))
		{
			Status statusByLevel = GetStatusByLevel(base.currentLevel);
			_bonusUIText = string.Format("{0} {1}", Utilities.ColorizeSpellTitle(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Mood_Reduction") + ":"), statusByLevel.moodEffect);
		}
		return _bonusUIText;
	}

	public override string GetBonusLevelUpUIText()
	{
		if (string.IsNullOrEmpty(_bonusLevelUpUIText))
		{
			Status statusByLevel = GetStatusByLevel(base.currentLevel);
			if (base.isMaxLevel)
			{
				_bonusLevelUpUIText = string.Format("{0} {1}", Utilities.ColorizeSpellTitle(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Mood_Reduction") + ":"), statusByLevel.moodEffect);
			}
			else
			{
				Status statusByLevel2 = GetStatusByLevel(base.currentLevel + 1);
				_bonusLevelUpUIText = string.Format("{0} {1} {2} {3}", Utilities.ColorizeSpellTitle(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Mood_Reduction") + ":"), statusByLevel.moodEffect, Utilities.UpgradeArrowIcon(), Utilities.ColorizeUpgradeText($"{statusByLevel2.moodEffect}"));
			}
		}
		return _bonusLevelUpUIText;
	}

	private Status GetStatusByLevel(int p_level)
	{
		string p_traitName = "Gloomy1";
		switch (p_level)
		{
		case 0:
			p_traitName = "Gloomy1";
			break;
		case 1:
			p_traitName = "Gloomy2";
			break;
		case 2:
			p_traitName = "Gloomy3";
			break;
		case 3:
			p_traitName = "Gloomy4";
			break;
		}
		if (TraitManager.Instance != null)
		{
			return TraitManager.Instance.GetTrait(p_traitName) as Status;
		}
		return null;
	}
}

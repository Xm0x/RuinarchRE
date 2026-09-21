using System;
using Settings;
using UnityEngine;

[Serializable]
public struct PortraitSettings
{
	public RACE race;

	public GENDER gender;

	public HAIR_COLOR hairColorType;

	public int portraitIndex;

	public string className;

	public Color wholeImageColor
	{
		get
		{
			if (SettingsManager.Instance.settings.arachnophobiaToggle && race == RACE.SPIDER)
			{
				return CharacterVisuals.ColorToUseForArachnophobia;
			}
			return Color.white;
		}
	}

	public PortraitSettings(RACE p_race, GENDER p_gender, HAIR_COLOR p_hairColorType, string p_className)
	{
		race = p_race;
		gender = p_gender;
		hairColorType = p_hairColorType;
		className = p_className;
		portraitIndex = CharacterManager.Instance.portraitCollection.GetRandomAvailablePortraitIndex(gender, race, hairColorType);
	}

	public PortraitSettings(RACE p_race, string p_className)
	{
		race = p_race;
		gender = GENDER.MALE;
		hairColorType = HAIR_COLOR.Brunette;
		className = p_className;
		portraitIndex = CharacterManager.Instance.portraitCollection.GetRandomAvailablePortraitIndex(gender, race, hairColorType);
	}

	public PortraitSettings(RACE p_race, GENDER p_gender, HAIR_COLOR p_hairColorType, string p_className, int p_portraitIndex)
	{
		race = p_race;
		gender = p_gender;
		hairColorType = p_hairColorType;
		className = p_className;
		portraitIndex = p_portraitIndex;
	}

	public string GetClassToUseForPortrait()
	{
		if (SettingsManager.Instance.settings.arachnophobiaToggle && race == RACE.SPIDER)
		{
			return CharacterVisuals.ClassToUseForArachnophobia;
		}
		return className;
	}
}

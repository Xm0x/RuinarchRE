using UtilityScripts;

namespace Character_Talents;

public abstract class CharacterTalentData
{
	public const int MaxTalentLevel = 5;

	private string _localizedName;

	private string _localizedDescription;

	private string[] _localizedBonusDescriptions;

	public CHARACTER_TALENT type { get; private set; }

	public string name { get; private set; }

	public virtual bool hasReevaluation => false;

	public string localizedName => _localizedName;

	public string localizedDescription => _localizedDescription;

	public string[] localizedBonusDescriptions => _localizedBonusDescriptions;

	public CharacterTalentData(CHARACTER_TALENT p_talentType)
	{
		type = p_talentType;
		name = Utilities.NotNormalizedConversionEnumToString(p_talentType.ToString());
		_localizedName = LocalizationManager.Instance.GetLocalizedValue("Talents_Table", name);
		_localizedDescription = LocalizationManager.Instance.GetLocalizedValue("Talents_Table", name + "_Description");
		_localizedBonusDescriptions = new string[5];
		for (int i = 0; i < _localizedBonusDescriptions.Length; i++)
		{
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("Talents_Table", $"{name}_Level_{i}_Description");
			_localizedBonusDescriptions[i] = localizedValue;
		}
	}

	public string GetBonusDescription(int p_level)
	{
		return localizedBonusDescriptions[p_level - 1];
	}

	public abstract void OnLevelUp(Character p_character, int level);

	public abstract void OnLevelUpAsAWhole(Character p_character);

	public abstract void OnReevaluateTalentPerLevel(Character p_character, int level);

	public abstract void OnReevaluateTalentAsAWhole(Character p_character);

	public abstract string GetAdditionalBonusDescription(Character p_character, int p_level);
}

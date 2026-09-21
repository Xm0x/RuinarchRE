using System;
using System.Collections.Generic;
using Character_Talents;

public class CharacterTalentManager : BaseMonoBehaviour
{
	public const int CHARACTER_TALENT_MAX_EXP = 100;

	public const int CHARACTER_TALENT_MAX_LEVEL = 5;

	public CHARACTER_TALENT[] allTalentEnums;

	private Dictionary<CHARACTER_TALENT, CharacterTalentData> _characterTalentDataDictionary;

	public void Initialize()
	{
		_characterTalentDataDictionary = new Dictionary<CHARACTER_TALENT, CharacterTalentData>();
	}

	public CharacterTalentData GetOrCreateCharacterTalentData(CHARACTER_TALENT p_talentType)
	{
		if (_characterTalentDataDictionary.ContainsKey(p_talentType))
		{
			return _characterTalentDataDictionary[p_talentType];
		}
		CharacterTalentData characterTalentData = CreateCharacterTalentData(p_talentType);
		if (characterTalentData == null)
		{
			throw new Exception($"There are no talent scriptable object for {p_talentType}");
		}
		_characterTalentDataDictionary.Add(p_talentType, characterTalentData);
		return characterTalentData;
	}

	private CharacterTalentData CreateCharacterTalentData(CHARACTER_TALENT p_talentType)
	{
		CharacterTalentData result = null;
		Type type = Type.GetType("Character_Talents." + p_talentType.ToStringEnumNoSpace() + "Data, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
		if (type != null)
		{
			result = Activator.CreateInstance(type) as CharacterTalentData;
		}
		return result;
	}
}

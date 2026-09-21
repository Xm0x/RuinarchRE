using UnityEngine;

[CreateAssetMenu(fileName = "New Character Portrait Data", menuName = "Scriptable Objects/Character Portrait Data")]
public class CharacterPortraitData : ScriptableObject
{
	[SerializeField]
	private CharacterPortraitDictionary _humanMalePortraitDictionary;

	[SerializeField]
	private CharacterPortraitDictionary _humanFemalePortraitDictionary;

	[SerializeField]
	private CharacterPortraitDictionary _elfMalePortraitDictionary;

	[SerializeField]
	private CharacterPortraitDictionary _elfFemalePortraitDictionary;

	[SerializeField]
	private CharacterPortraitAssetsData[] _allPortraitAssetsData;

	public CharacterPortraitAssetsData[] allPortraitAssetsData => _allPortraitAssetsData;

	public Sprite GetPortraitAsset(PortraitSettings p_settings)
	{
		return GetPortraitAsset(p_settings.gender, p_settings.race, p_settings.hairColorType, p_settings.portraitIndex);
	}

	public Sprite GetPortraitAsset(GENDER p_genderType, RACE p_raceType, HAIR_COLOR p_colorType, int p_index)
	{
		CharacterPortraitAssetsData portraitAssetData = GetPortraitAssetData(p_genderType, p_raceType, p_colorType);
		if (portraitAssetData != null)
		{
			return portraitAssetData.portraits[p_index];
		}
		return null;
	}

	public int GetRandomAvailablePortraitIndex(GENDER p_genderType, RACE p_raceType, HAIR_COLOR p_colorType, bool p_tagAsUnavailable = true)
	{
		CharacterPortraitAssetsData portraitAssetData = GetPortraitAssetData(p_genderType, p_raceType, p_colorType);
		if (portraitAssetData != null)
		{
			return portraitAssetData.GetRandomAvailablePortraitIndex(p_tagAsUnavailable);
		}
		return -1;
	}

	private CharacterPortraitAssetsData GetPortraitAssetData(GENDER p_genderType, RACE p_raceType, HAIR_COLOR p_colorType)
	{
		return p_raceType switch
		{
			RACE.HUMANS => GetHumanPortraitAssetData(p_genderType, p_colorType), 
			RACE.ELVES => GetElfPortraitAssetData(p_genderType, p_colorType), 
			_ => null, 
		};
	}

	private CharacterPortraitAssetsData GetHumanPortraitAssetData(GENDER p_genderType, HAIR_COLOR p_colorType)
	{
		if (p_genderType == GENDER.FEMALE)
		{
			if (_humanFemalePortraitDictionary.ContainsKey(p_colorType))
			{
				return _humanFemalePortraitDictionary[p_colorType];
			}
		}
		else if (_humanMalePortraitDictionary.ContainsKey(p_colorType))
		{
			return _humanMalePortraitDictionary[p_colorType];
		}
		return null;
	}

	private CharacterPortraitAssetsData GetElfPortraitAssetData(GENDER p_genderType, HAIR_COLOR p_colorType)
	{
		if (p_genderType == GENDER.FEMALE)
		{
			if (_elfFemalePortraitDictionary.ContainsKey(p_colorType))
			{
				return _elfFemalePortraitDictionary[p_colorType];
			}
		}
		else if (_elfMalePortraitDictionary.ContainsKey(p_colorType))
		{
			return _elfMalePortraitDictionary[p_colorType];
		}
		return null;
	}
}

using System.Collections.Generic;
using UnityEngine;

public class CharacterPortraitRaceCollection
{
	private Dictionary<RACE, CharacterPortraitGenderCollection> _collection;

	public CharacterPortraitRaceCollection()
	{
		_collection = new Dictionary<RACE, CharacterPortraitGenderCollection>();
	}

	public CharacterPortraitGenderCollection GetGenderCollection(RACE p_race)
	{
		if (_collection.ContainsKey(p_race))
		{
			return _collection[p_race];
		}
		return null;
	}

	public void SetToCollection(RACE p_race, GENDER p_gender, HAIR_COLOR p_hairColor, string p_folderPath)
	{
		CharacterPortraitGenderCollection characterPortraitGenderCollection;
		if (_collection.ContainsKey(p_race))
		{
			characterPortraitGenderCollection = _collection[p_race];
		}
		else
		{
			characterPortraitGenderCollection = new CharacterPortraitGenderCollection();
			_collection.Add(p_race, characterPortraitGenderCollection);
		}
		characterPortraitGenderCollection.SetToCollection(p_gender, p_hairColor, p_folderPath);
	}

	public void LoadAllSpriteKeys()
	{
		foreach (KeyValuePair<RACE, CharacterPortraitGenderCollection> item in _collection)
		{
			item.Value.LoadAllSpriteKeys(item.Key);
		}
	}

	public Sprite GetPortraitAsset(PortraitSettings p_settings)
	{
		return GetPortraitAsset(p_settings.gender, p_settings.race, p_settings.hairColorType, p_settings.portraitIndex);
	}

	public Sprite GetPortraitAsset(GENDER p_genderType, RACE p_raceType, HAIR_COLOR p_colorType, int p_index)
	{
		return GetPortraitSpriteData(p_genderType, p_raceType, p_colorType)?.GetPortraitByIndex(p_index);
	}

	public int GetRandomAvailablePortraitIndex(GENDER p_genderType, RACE p_raceType, HAIR_COLOR p_colorType, bool p_tagAsUnavailable = true)
	{
		return GetPortraitSpriteData(p_genderType, p_raceType, p_colorType)?.GetRandomAvailablePortraitIndex(p_tagAsUnavailable) ?? (-1);
	}

	private CharacterPortraitSpriteCollection GetPortraitSpriteData(GENDER p_genderType, RACE p_raceType, HAIR_COLOR p_colorType)
	{
		if (_collection.ContainsKey(p_raceType))
		{
			return _collection[p_raceType].GetPortraitSpriteData(p_genderType, p_colorType);
		}
		return null;
	}

	public void PopulateAllSpriteCollection(List<CharacterPortraitSpriteCollection> p_collection)
	{
		foreach (CharacterPortraitGenderCollection value in _collection.Values)
		{
			value.PopulateAllSpriteCollection(p_collection);
		}
	}
}

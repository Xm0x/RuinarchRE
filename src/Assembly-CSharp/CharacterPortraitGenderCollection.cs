using System.Collections.Generic;

public class CharacterPortraitGenderCollection
{
	private Dictionary<GENDER, CharacterPortraitHairColorCollection> _collection;

	public CharacterPortraitGenderCollection()
	{
		_collection = new Dictionary<GENDER, CharacterPortraitHairColorCollection>();
	}

	public void SetToCollection(GENDER p_gender, HAIR_COLOR p_hairColor, string p_folderPath)
	{
		CharacterPortraitHairColorCollection characterPortraitHairColorCollection;
		if (_collection.ContainsKey(p_gender))
		{
			characterPortraitHairColorCollection = _collection[p_gender];
		}
		else
		{
			characterPortraitHairColorCollection = new CharacterPortraitHairColorCollection();
			_collection.Add(p_gender, characterPortraitHairColorCollection);
		}
		characterPortraitHairColorCollection.SetToCollection(p_hairColor, p_folderPath);
	}

	public void LoadAllSpriteKeys(RACE p_race)
	{
		foreach (KeyValuePair<GENDER, CharacterPortraitHairColorCollection> item in _collection)
		{
			item.Value.LoadAllSpriteKeys(p_race, item.Key);
		}
	}

	public CharacterPortraitSpriteCollection GetPortraitSpriteData(GENDER p_genderType, HAIR_COLOR p_colorType)
	{
		if (_collection.ContainsKey(p_genderType))
		{
			return _collection[p_genderType].GetPortraitSpriteData(p_colorType);
		}
		return null;
	}

	public void PopulateAllSpriteCollection(List<CharacterPortraitSpriteCollection> p_collection)
	{
		foreach (CharacterPortraitHairColorCollection value in _collection.Values)
		{
			value.PopulateAllSpriteCollection(p_collection);
		}
	}
}

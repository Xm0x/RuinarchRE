using System.Collections.Generic;

public class CharacterPortraitHairColorCollection
{
	private Dictionary<HAIR_COLOR, CharacterPortraitSpriteCollection> _collection;

	public CharacterPortraitHairColorCollection()
	{
		_collection = new Dictionary<HAIR_COLOR, CharacterPortraitSpriteCollection>();
	}

	public void SetToCollection(HAIR_COLOR p_hairColor, string p_folderPath)
	{
		CharacterPortraitSpriteCollection characterPortraitSpriteCollection;
		if (_collection.ContainsKey(p_hairColor))
		{
			characterPortraitSpriteCollection = _collection[p_hairColor];
		}
		else
		{
			characterPortraitSpriteCollection = new CharacterPortraitSpriteCollection();
			_collection.Add(p_hairColor, characterPortraitSpriteCollection);
		}
		characterPortraitSpriteCollection.SetFolderPath(p_folderPath);
	}

	public void LoadAllSpriteKeys(RACE p_race, GENDER p_gender)
	{
		foreach (KeyValuePair<HAIR_COLOR, CharacterPortraitSpriteCollection> item in _collection)
		{
			item.Value.LoadAllSpriteKeys(p_race, p_gender, item.Key);
		}
	}

	public CharacterPortraitSpriteCollection GetPortraitSpriteData(HAIR_COLOR p_colorType)
	{
		if (_collection.ContainsKey(p_colorType))
		{
			return _collection[p_colorType];
		}
		return null;
	}

	public void PopulateAllSpriteCollection(List<CharacterPortraitSpriteCollection> p_collection)
	{
		foreach (CharacterPortraitSpriteCollection value in _collection.Values)
		{
			p_collection.Add(value);
		}
	}
}

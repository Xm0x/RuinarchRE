using System.IO;
using UnityEngine;

public class CharacterPortraitSpriteCollection
{
	private string _folderPath;

	private string[] _collectionKeys;

	private bool[] _portraitAvailability;

	public bool[] portraitAvailability => _portraitAvailability;

	public Sprite GetPortraitByIndex(int p_index)
	{
		return GetPortraitSprite(_collectionKeys[p_index]);
	}

	public Sprite GetPortraitSprite(string p_key)
	{
		return TextureManager.Instance.characterSpritesAtlas.GetSpriteByKey(p_key);
	}

	public int GetRandomAvailablePortraitIndex(bool p_tagAsUnavailable = true)
	{
		int randomAvailablePortraitIndexBase = GetRandomAvailablePortraitIndexBase();
		if (p_tagAsUnavailable)
		{
			_portraitAvailability[randomAvailablePortraitIndexBase] = true;
		}
		return randomAvailablePortraitIndexBase;
	}

	public void SetPortraitAvailability(bool[] p_arg)
	{
		_portraitAvailability = p_arg;
	}

	private int GetRandomAvailablePortraitIndexBase()
	{
		if (_portraitAvailability == null)
		{
			_portraitAvailability = new bool[_collectionKeys.Length];
		}
		else if (!HasAvailablePortraitIndex())
		{
			ResetPortraitAvailability();
		}
		int num = Random.Range(0, _portraitAvailability.Length);
		if (_portraitAvailability[num])
		{
			num = GetNearestAvailablePortraitIndex(num);
		}
		return num;
	}

	private bool HasAvailablePortraitIndex()
	{
		for (int i = 0; i < _portraitAvailability.Length; i++)
		{
			if (!_portraitAvailability[i])
			{
				return true;
			}
		}
		return false;
	}

	private void ResetPortraitAvailability()
	{
		for (int i = 0; i < _portraitAvailability.Length; i++)
		{
			_portraitAvailability[i] = false;
		}
	}

	private int GetNearestAvailablePortraitIndex(int p_startingIndex)
	{
		int num = -1;
		if (Random.Range(0, 2) == 0)
		{
			num = GetNearestAvailablePortraitIndexToTheLeft(p_startingIndex);
			if (num == -1)
			{
				num = GetNearestAvailablePortraitIndexToTheRight(p_startingIndex);
			}
		}
		else
		{
			num = GetNearestAvailablePortraitIndexToTheRight(p_startingIndex);
			if (num == -1)
			{
				num = GetNearestAvailablePortraitIndexToTheLeft(p_startingIndex);
			}
		}
		return num;
	}

	private int GetNearestAvailablePortraitIndexToTheLeft(int p_startingIndex)
	{
		for (int num = p_startingIndex; num >= 0; num--)
		{
			if (!_portraitAvailability[num])
			{
				return num;
			}
		}
		return -1;
	}

	private int GetNearestAvailablePortraitIndexToTheRight(int p_startingIndex)
	{
		for (int i = p_startingIndex; i < _portraitAvailability.Length; i++)
		{
			if (!_portraitAvailability[i])
			{
				return i;
			}
		}
		return -1;
	}

	public void SetFolderPath(string p_folderPath)
	{
		_folderPath = p_folderPath;
	}

	public void LoadAllSpriteKeys(RACE p_race, GENDER p_gender, HAIR_COLOR p_color)
	{
		string[] files = Directory.GetFiles(_folderPath, "*.png");
		if (files == null || files.Length == 0)
		{
			return;
		}
		_collectionKeys = new string[files.Length];
		TextureAtlas2D characterSpritesAtlas = TextureManager.Instance.characterSpritesAtlas;
		for (int i = 0; i < files.Length; i++)
		{
			string text = files[i];
			string text2 = p_race.ToStringEnum() + "_" + p_gender.ToStringEnum() + "_" + p_color.ToStringEnum() + "_" + i;
			_collectionKeys[i] = text2;
			if (characterSpritesAtlas.HasTextureID(text2))
			{
				characterSpritesAtlas.GetPackedTextureByKey(text2).SetPath(text);
			}
			else
			{
				characterSpritesAtlas.AddTexturePathToAtlas(text2, text, 100);
			}
		}
	}
}

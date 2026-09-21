using System.Collections.Generic;
using UnityEngine;

public class CharacterSpriteCollection
{
	private string _defaultSpriteAtlasKey;

	private Dictionary<string, string> _spriteCollection = new Dictionary<string, string>();

	public Dictionary<string, string> spriteCollection => _spriteCollection;

	public void Add(string p_spriteID, string p_spriteAtlasKey)
	{
		if (!_spriteCollection.ContainsKey(p_spriteID))
		{
			_spriteCollection.Add(p_spriteID, p_spriteAtlasKey);
			if (_defaultSpriteAtlasKey == null)
			{
				_defaultSpriteAtlasKey = p_spriteAtlasKey;
			}
		}
	}

	public Sprite GetSprite(string p_spriteID)
	{
		string text = _defaultSpriteAtlasKey;
		if (_spriteCollection.ContainsKey(p_spriteID))
		{
			text = _spriteCollection[p_spriteID];
		}
		if (!string.IsNullOrEmpty(text))
		{
			return TextureManager.Instance.characterSpritesAtlas.GetSpriteByKey(text);
		}
		return null;
	}
}

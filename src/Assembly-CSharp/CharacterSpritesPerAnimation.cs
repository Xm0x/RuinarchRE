using System.Collections.Generic;
using UnityEngine;

public class CharacterSpritesPerAnimation
{
	private Dictionary<string, CharacterSpriteCollection> _animationSpriteCollection = new Dictionary<string, CharacterSpriteCollection>();

	public Dictionary<string, CharacterSpriteCollection> animationSpriteCollection => _animationSpriteCollection;

	public void Add(string p_animationName, string p_spriteID, string p_spriteAtlasKey)
	{
		if (!Contains(p_animationName))
		{
			_animationSpriteCollection.Add(p_animationName, new CharacterSpriteCollection());
		}
		_animationSpriteCollection[p_animationName].Add(p_spriteID, p_spriteAtlasKey);
	}

	public bool Contains(string p_animationName)
	{
		return _animationSpriteCollection.ContainsKey(p_animationName);
	}

	public Sprite GetSprite(string p_animationName, string p_spriteID)
	{
		if (_animationSpriteCollection.ContainsKey(p_animationName))
		{
			return _animationSpriteCollection[p_animationName].GetSprite(p_spriteID);
		}
		return null;
	}
}

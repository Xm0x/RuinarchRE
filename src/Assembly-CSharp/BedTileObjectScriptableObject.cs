using System.Collections.Generic;
using UnityEngine;

public class BedTileObjectScriptableObject : TileObjectScriptableObject
{
	[Header("Bed Specific")]
	[SerializeField]
	private BedSpriteDictionary _bedSpriteDictionary;

	[SerializeField]
	private Sprite defaultBedSprite;

	[SerializeField]
	private Sprite elvenBedSprite;

	public BedSpriteSetting GetBedSpriteSettings(Sprite p_sprite)
	{
		if (_bedSpriteDictionary.ContainsKey(p_sprite))
		{
			return _bedSpriteDictionary[p_sprite];
		}
		foreach (KeyValuePair<Sprite, BedSpriteSetting> item in _bedSpriteDictionary)
		{
			if (item.Value.sprite1Sleeping == p_sprite || item.Value.sprite2Sleeping == p_sprite || item.Value.spriteUnoccupied == p_sprite)
			{
				return item.Value;
			}
		}
		return null;
	}

	public Sprite GetBedSpriteToUseForRace(RACE race)
	{
		if (race == RACE.ELVES)
		{
			return elvenBedSprite;
		}
		return defaultBedSprite;
	}
}

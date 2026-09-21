using System.Collections.Generic;
using AK.Wwise;
using UnityEngine;
using UnityEngine.Tilemaps;
using UtilityScripts;

[CreateAssetMenu(fileName = "New Tile Object Data", menuName = "Scriptable Objects/Tile Object Data")]
public class TileObjectScriptableObject : ScriptableObject
{
	[Header("Assets")]
	public TileBase defaultTileMapAsset;

	public Sprite defaultSprite;

	public TileObjectTileSetting tileObjectAssets;

	public TileObjectTileSetting corruptedTileObjectAssets;

	public List<Sprite> allTileObjectSprites;

	[Header("SFX")]
	public AK.Wwise.Event uiSFX;

	public AK.Wwise.Event seizeSFX;

	public AK.Wwise.Event unseizeSFX;

	public TileObjectScriptableObject()
	{
		tileObjectAssets = new TileObjectTileSetting();
		corruptedTileObjectAssets = new TileObjectTileSetting();
	}

	public TileBase GetTileBaseToUse(BIOMES p_biome)
	{
		if (!tileObjectAssets.biomeAssets.ContainsKey(p_biome))
		{
			p_biome = BIOMES.NONE;
		}
		return CollectionUtilities.GetRandomElement(tileObjectAssets.biomeAssets[p_biome].tileBase);
	}

	public int GetIndexBySprite(Sprite p_sprite)
	{
		for (int i = 0; i < allTileObjectSprites.Count; i++)
		{
			if (allTileObjectSprites[i] == p_sprite)
			{
				return i;
			}
		}
		return -1;
	}

	public Sprite GetSpriteByIndex(int p_index)
	{
		if (p_index >= 0 && p_index < allTileObjectSprites.Count)
		{
			return allTileObjectSprites[p_index];
		}
		return null;
	}
}

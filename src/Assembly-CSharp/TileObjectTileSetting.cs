using System;

[Serializable]
public class TileObjectTileSetting
{
	public TileObjectBiomeAssetDictionary biomeAssets;

	public BiomeTileObjectTileSetting GetAsset(BIOMES biome)
	{
		if (biomeAssets.ContainsKey(biome))
		{
			return biomeAssets[biome];
		}
		return biomeAssets[BIOMES.NONE];
	}
}

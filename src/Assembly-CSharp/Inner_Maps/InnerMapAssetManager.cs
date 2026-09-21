using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Inner_Maps;

public class InnerMapAssetManager : BaseMonoBehaviour
{
	public TileTypeAssetDictionary tileTypeAssets;

	[Header("Grassland Tiles")]
	public TileBase outsideTile;

	public TileBase grassTile;

	public TileBase soilTile;

	public TileBase stoneTile;

	public TileBase shrubTile;

	public TileBase herbPlantTile;

	public TileBase flowerTile;

	public TileBase rockTile;

	public TileBase randomGarbTile;

	public TileBase dirtTile;

	[Header("Snow Tiles")]
	public TileBase snowOutsideTile;

	public TileBase snowTile;

	public TileBase tundraTile;

	public TileBase snowDirt;

	public TileBase snowFlowerTile;

	public TileBase snowGarbTile;

	[Header("Desert Tiles")]
	public TileBase desertOutsideTile;

	public TileBase desertGrassTile;

	public TileBase desertSandTile;

	public TileBase desertStoneGroundTile;

	public TileBase desertFlowerTile;

	public TileBase desertGarbTile;

	public TileBase desertRockTile;

	[Header("Inside Detail Tiles")]
	public TileBase crateBarrelTile;

	public TileBase structureStoneFloor;

	public TileBase ruinedStoneFloorTile;

	[Header("Seamless Edges")]
	public SeamlessEdgeAssetsDictionary edgeAssets;

	[Header("Water Tiles")]
	public TileBase wetTile;

	public TileBase deepestWaterTile;

	public TileBase midWaterTile;

	public TileBase shallowWaterTile;

	public TileBase shoreWaterTile;

	[Header("Cave Tiles")]
	public TileBase caveWallTile;

	public TileBase caveGroundTile;

	[Header("Monster Lair Tiles")]
	public TileBase monsterLairWallTile;

	public TileBase monsterLairGroundTile;

	[Header("Corrupted Tiles")]
	public TileBase corruptedTile;

	[Header("Demon Tiles")]
	public TileBase demonicWallTile;

	[Header("Structure Floor Tiles")]
	public TileBase woodFloorTile;

	public TileBase stoneFloorTile;

	[Header("Other Tiles")]
	public TileBase poisonRuleTile;

	public TileBase boneFloorRuleTile;

	public TileBase noxiousFloorRuleTile;

	public TileBase fieryCaveFloorRuleTile;

	public TileBase elvenFloorTile;

	public TileBase elvenFloorTile2;

	public TileBase magmaFloorTile;

	[Header("Materials")]
	public Material burntMaterial;

	public Material defaultObjectMaterial;

	[Header("Demon")]
	public TileBase demonStoneRuleTile;

	[Header("Fiery Cave")]
	public TileBase fieryCaveWallTile;

	[Header("Minimap")]
	public TileBase minimapTile;

	public TileTypeColorDictionary tileTypeColorDictionary;

	[Header("Divine Church")]
	public TileBase[] divineCultFloor;

	[Header("Wiccans")]
	public TileBase[] natureCultFloor;

	public TileBase GetOutsideFloorTile(BIOMES p_biomeType)
	{
		switch (p_biomeType)
		{
		case BIOMES.SNOW:
		case BIOMES.TUNDRA:
			return snowOutsideTile;
		case BIOMES.DESERT:
			return desertOutsideTile;
		default:
			return outsideTile;
		}
	}

	public TileBase GetWallAssetBasedOnWallType(WALL_TYPE wallType)
	{
		return wallType switch
		{
			WALL_TYPE.Stone => caveWallTile, 
			WALL_TYPE.Flesh => monsterLairWallTile, 
			WALL_TYPE.Demon_Stone => demonicWallTile, 
			WALL_TYPE.Fiery_Cave => fieryCaveWallTile, 
			_ => null, 
		};
	}

	public TileBase GetFlowerTile(BIOMES p_biomeType)
	{
		switch (p_biomeType)
		{
		case BIOMES.SNOW:
		case BIOMES.TUNDRA:
			return snowFlowerTile;
		case BIOMES.DESERT:
			return desertFlowerTile;
		default:
			return flowerTile;
		}
	}

	public TileBase GetGarbTile(BIOMES p_biomeType)
	{
		switch (p_biomeType)
		{
		case BIOMES.SNOW:
		case BIOMES.TUNDRA:
			return snowGarbTile;
		case BIOMES.DESERT:
			return desertGarbTile;
		default:
			return randomGarbTile;
		}
	}

	public TileBase GetRockTile(BIOMES p_biomeType)
	{
		if (p_biomeType == BIOMES.DESERT)
		{
			return desertRockTile;
		}
		return rockTile;
	}

	public Dictionary<string, TileBase> GetFloorAndWallTileAssetDB()
	{
		Dictionary<string, TileBase> dictionary = new Dictionary<string, TileBase>();
		dictionary.Add(outsideTile.name, outsideTile);
		dictionary.Add(dirtTile.name, dirtTile);
		dictionary.Add(grassTile.name, grassTile);
		dictionary.Add(soilTile.name, soilTile);
		dictionary.Add(stoneTile.name, stoneTile);
		dictionary.Add(snowOutsideTile.name, snowOutsideTile);
		dictionary.Add(snowTile.name, snowTile);
		dictionary.Add(tundraTile.name, tundraTile);
		dictionary.Add(snowDirt.name, snowDirt);
		dictionary.Add(desertOutsideTile.name, desertOutsideTile);
		dictionary.Add(desertGrassTile.name, desertGrassTile);
		dictionary.Add(desertSandTile.name, desertSandTile);
		dictionary.Add(desertStoneGroundTile.name, desertStoneGroundTile);
		dictionary.Add(caveGroundTile.name, caveGroundTile);
		dictionary.Add(caveWallTile.name, caveWallTile);
		dictionary.Add(monsterLairWallTile.name, monsterLairWallTile);
		dictionary.Add(monsterLairGroundTile.name, monsterLairGroundTile);
		dictionary.Add(corruptedTile.name, corruptedTile);
		dictionary.Add(demonicWallTile.name, demonicWallTile);
		dictionary.Add(woodFloorTile.name, woodFloorTile);
		dictionary.Add(stoneFloorTile.name, stoneFloorTile);
		dictionary.Add(structureStoneFloor.name, structureStoneFloor);
		dictionary.Add(ruinedStoneFloorTile.name, ruinedStoneFloorTile);
		dictionary.Add(demonStoneRuleTile.name, demonStoneRuleTile);
		dictionary.Add(fieryCaveWallTile.name, fieryCaveWallTile);
		dictionary.Add(wetTile.name, wetTile);
		dictionary.Add(deepestWaterTile.name, deepestWaterTile);
		dictionary.Add(midWaterTile.name, midWaterTile);
		dictionary.Add(shoreWaterTile.name, shoreWaterTile);
		dictionary.Add(shallowWaterTile.name, shallowWaterTile);
		dictionary.Add(boneFloorRuleTile.name, boneFloorRuleTile);
		dictionary.Add(noxiousFloorRuleTile.name, noxiousFloorRuleTile);
		dictionary.Add(fieryCaveFloorRuleTile.name, fieryCaveFloorRuleTile);
		dictionary.Add(elvenFloorTile.name, elvenFloorTile);
		dictionary.Add(elvenFloorTile2.name, elvenFloorTile2);
		dictionary.Add(magmaFloorTile.name, magmaFloorTile);
		for (int i = 0; i < divineCultFloor.Length; i++)
		{
			dictionary.Add(divineCultFloor[i].name, divineCultFloor[i]);
		}
		for (int j = 0; j < natureCultFloor.Length; j++)
		{
			dictionary.Add(natureCultFloor[j].name, natureCultFloor[j]);
		}
		foreach (KeyValuePair<Biome_Tile_Type, TileBase> tileTypeAsset in tileTypeAssets)
		{
			TileBase value = tileTypeAsset.Value;
			if (value != null && !dictionary.ContainsKey(value.name))
			{
				dictionary.Add(value.name, value);
			}
		}
		return dictionary;
	}

	public TileBase TryGetTileAsset(string assetName, Dictionary<string, TileBase> tileAssetDB)
	{
		if (tileAssetDB.ContainsKey(assetName))
		{
			return tileAssetDB[assetName];
		}
		throw new Exception("Could not find asset with name " + assetName);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		burntMaterial = null;
		defaultObjectMaterial = null;
	}

	public TileBase GetGroundAssetForTile(LocationGridTile p_tile)
	{
		return tileTypeAssets[p_tile.specificBiomeTileType];
	}

	public Color GetColorForBiomeType(Biome_Tile_Type p_biome)
	{
		if (tileTypeColorDictionary.ContainsKey(p_biome))
		{
			return tileTypeColorDictionary[p_biome];
		}
		throw new Exception("No color for " + p_biome);
	}
}

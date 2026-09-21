using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class AreaBiomeComponent : AreaComponent
{
	public Dictionary<BIOMES, int> biomeDictionary { get; }

	public BIOMES biomeType { get; private set; }

	public AreaBiomeComponent()
	{
		biomeDictionary = new Dictionary<BIOMES, int>();
		biomeType = BIOMES.NONE;
	}

	public void OnTileAddedToArea(LocationGridTile p_tile)
	{
		AddBiomeVoteToDictionary(p_tile.mainBiomeType);
	}

	public void OnTileInAreaChangedBiome(LocationGridTile p_tile, BIOMES p_biome)
	{
		RemoveBiomeVoteFromDictionary(p_biome);
		AddBiomeVoteToDictionary(p_tile.mainBiomeType);
	}

	private void AddBiomeVoteToDictionary(BIOMES p_biome)
	{
		if (!biomeDictionary.ContainsKey(p_biome))
		{
			biomeDictionary.Add(p_biome, 0);
		}
		biomeDictionary[p_biome]++;
		UpdateBiomeBasedOnVotes();
	}

	private void RemoveBiomeVoteFromDictionary(BIOMES p_biome)
	{
		if (!biomeDictionary.ContainsKey(p_biome))
		{
			biomeDictionary.Add(p_biome, 0);
		}
		biomeDictionary[p_biome]--;
		UpdateBiomeBasedOnVotes();
	}

	private void UpdateBiomeBasedOnVotes()
	{
		BIOMES bIOMES = biomeType;
		int num = int.MinValue;
		BIOMES bIOMES2 = BIOMES.NONE;
		foreach (KeyValuePair<BIOMES, int> item in biomeDictionary)
		{
			int value = item.Value;
			if (value > num)
			{
				bIOMES2 = item.Key;
				num = value;
			}
		}
		biomeType = bIOMES2;
		if (bIOMES != biomeType)
		{
			GridMap.Instance.mainRegion.biomeDivisionComponent.GetBiomeDivision(bIOMES)?.RemoveArea(base.owner);
			GridMap.Instance.mainRegion.biomeDivisionComponent.GetBiomeDivision(biomeType)?.AddArea(base.owner);
		}
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}
}

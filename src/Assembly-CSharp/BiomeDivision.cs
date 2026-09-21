using System.Collections.Generic;
using Inner_Maps;

public class BiomeDivision
{
	public BIOMES biome { get; private set; }

	public List<LocationGridTile> tiles { get; }

	public List<Area> areas { get; }

	public int monsterMigrationChance { get; private set; }

	public BiomeDivision(BIOMES p_biome)
	{
		biome = p_biome;
		tiles = new List<LocationGridTile>();
		areas = new List<Area>();
		AddListenersBasedOnBiome();
	}

	public BiomeDivision(SaveDataRegionDivision p_data)
	{
		biome = p_data.biome;
		monsterMigrationChance = p_data.monsterMigrationChance;
		tiles = new List<LocationGridTile>();
		areas = new List<Area>();
		AddListenersBasedOnBiome();
	}

	public void AddTile(LocationGridTile p_tile)
	{
		tiles.Add(p_tile);
	}

	public void RemoveTile(LocationGridTile p_tile)
	{
		tiles.Remove(p_tile);
	}

	public void AddArea(Area p_area)
	{
		areas.Add(p_area);
	}

	public void RemoveArea(Area p_area)
	{
		areas.Remove(p_area);
	}

	private void AddListenersBasedOnBiome()
	{
		switch (biome)
		{
		case BIOMES.SNOW:
		case BIOMES.TUNDRA:
			Messenger.AddListener(Signals.HOUR_STARTED, TryFreezeWetObjects);
			break;
		case BIOMES.DESERT:
			Messenger.AddListener<Character, Area>(CharacterSignals.CHARACTER_ENTERED_AREA, TryRemoveFreezing);
			break;
		case BIOMES.GRASSLAND:
		case BIOMES.FOREST:
			break;
		}
	}

	private void TryFreezeWetObjects()
	{
		Messenger.Broadcast(AreaSignals.FREEZE_WET_OBJECTS);
	}

	private void TryRemoveFreezing(Character character, Area p_area)
	{
		if (GameManager.Instance.gameHasStarted && p_area.gridTileComponent.centerGridTile.mainBiomeType == biome)
		{
			character.traitContainer.RemoveTrait(character, "Freezing");
			character.traitContainer.RemoveTrait(character, "Frozen");
		}
	}
}

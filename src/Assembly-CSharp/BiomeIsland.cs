using Inner_Maps;

public class BiomeIsland : BaseIsland
{
	public const int MinimumTilesInIsland = 100;

	public readonly BIOMES biome;

	public BiomeIsland(BIOMES biome)
	{
		this.biome = biome;
	}

	public override void AddTile(LocationGridTile tile, MapGenerationData mapGenerationData)
	{
		base.AddTile(tile, mapGenerationData);
		if (tile.mainBiomeType != biome)
		{
			tile.SetIndividualBiomeType(biome);
		}
	}
}

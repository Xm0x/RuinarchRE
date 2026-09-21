public class SaveDataRegionDivision : SaveData<BiomeDivision>
{
	public BIOMES biome;

	public int monsterMigrationChance;

	public override void Save(BiomeDivision data)
	{
		biome = data.biome;
		monsterMigrationChance = data.monsterMigrationChance;
	}

	public override BiomeDivision Load()
	{
		return new BiomeDivision(this);
	}
}

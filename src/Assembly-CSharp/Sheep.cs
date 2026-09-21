public class Sheep : ShearableAnimal
{
	public override COMBAT_MODE defaultCombatMode => COMBAT_MODE.Passive;

	public override TILE_OBJECT_TYPE produceableMaterial => TILE_OBJECT_TYPE.WOOL;

	public Sheep()
		: base(SUMMON_TYPE.Sheep, "Sheep", RACE.SHEEP)
	{
	}

	public Sheep(string className)
		: base(SUMMON_TYPE.Sheep, className, RACE.SHEEP)
	{
	}

	public Sheep(SaveDataShearableAnimal data)
		: base(data)
	{
	}
}

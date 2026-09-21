public class Rabbit : ShearableAnimal
{
	public override COMBAT_MODE defaultCombatMode => COMBAT_MODE.Passive;

	public override TILE_OBJECT_TYPE produceableMaterial => TILE_OBJECT_TYPE.RABBIT_CLOTH;

	public Rabbit()
		: base(SUMMON_TYPE.Rabbit, "Rabbit", RACE.RABBIT)
	{
	}

	public Rabbit(string className)
		: base(SUMMON_TYPE.Rabbit, className, RACE.RABBIT)
	{
	}

	public Rabbit(SaveDataShearableAnimal data)
		: base(data)
	{
	}
}

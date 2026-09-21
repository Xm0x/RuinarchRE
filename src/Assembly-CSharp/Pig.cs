public class Pig : Animal
{
	public override COMBAT_MODE defaultCombatMode => COMBAT_MODE.Passive;

	public override TILE_OBJECT_TYPE produceableMaterial => TILE_OBJECT_TYPE.RABBIT_CLOTH;

	public Pig()
		: base(SUMMON_TYPE.Pig, "Pig", RACE.PIG)
	{
	}

	public Pig(string className)
		: base(SUMMON_TYPE.Pig, className, RACE.PIG)
	{
	}

	public Pig(SaveDataSummon data)
		: base(data)
	{
	}
}

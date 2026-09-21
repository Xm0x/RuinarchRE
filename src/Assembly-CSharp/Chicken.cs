public class Chicken : Animal
{
	public override COMBAT_MODE defaultCombatMode => COMBAT_MODE.Passive;

	public override TILE_OBJECT_TYPE produceableMaterial => TILE_OBJECT_TYPE.RABBIT_CLOTH;

	public Chicken()
		: base(SUMMON_TYPE.Chicken, "Chicken", RACE.CHICKEN)
	{
	}

	public Chicken(string className)
		: base(SUMMON_TYPE.Chicken, className, RACE.CHICKEN)
	{
	}

	public Chicken(SaveDataSummon data)
		: base(data)
	{
	}
}

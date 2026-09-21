public class Rat : Animal
{
	public override COMBAT_MODE defaultCombatMode => COMBAT_MODE.Defend;

	public Rat()
		: base(SUMMON_TYPE.Rat, "Rat", RACE.RAT)
	{
	}

	public Rat(string className)
		: base(SUMMON_TYPE.Rat, className, RACE.RAT)
	{
	}

	public Rat(SaveDataSummon data)
		: base(data)
	{
	}
}

public class Wrath : DeadlySin
{
	public Wrath()
	{
		base.assignments = new DEADLY_SIN_ACTION[3]
		{
			DEADLY_SIN_ACTION.SPELL_SOURCE,
			DEADLY_SIN_ACTION.INVADER,
			DEADLY_SIN_ACTION.FIGHTER
		};
	}
}

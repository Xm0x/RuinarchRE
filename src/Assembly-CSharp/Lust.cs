public class Lust : DeadlySin
{
	public Lust()
	{
		base.assignments = new DEADLY_SIN_ACTION[3]
		{
			DEADLY_SIN_ACTION.SPELL_SOURCE,
			DEADLY_SIN_ACTION.SABOTEUR,
			DEADLY_SIN_ACTION.RESEARCHER
		};
	}
}

public class Envy : DeadlySin
{
	public Envy()
	{
		base.assignments = new DEADLY_SIN_ACTION[3]
		{
			DEADLY_SIN_ACTION.SPELL_SOURCE,
			DEADLY_SIN_ACTION.INSTIGATOR,
			DEADLY_SIN_ACTION.BUILDER
		};
	}
}

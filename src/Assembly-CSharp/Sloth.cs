public class Sloth : DeadlySin
{
	public Sloth()
	{
		base.assignments = new DEADLY_SIN_ACTION[3]
		{
			DEADLY_SIN_ACTION.SPELL_SOURCE,
			DEADLY_SIN_ACTION.RESEARCHER,
			DEADLY_SIN_ACTION.BUILDER
		};
	}
}

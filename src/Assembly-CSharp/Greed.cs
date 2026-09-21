public class Greed : DeadlySin
{
	public Greed()
	{
		base.assignments = new DEADLY_SIN_ACTION[3]
		{
			DEADLY_SIN_ACTION.SABOTEUR,
			DEADLY_SIN_ACTION.INVADER,
			DEADLY_SIN_ACTION.FIGHTER
		};
	}
}

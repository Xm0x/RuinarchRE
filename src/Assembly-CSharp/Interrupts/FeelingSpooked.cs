namespace Interrupts;

public class FeelingSpooked : Interrupt
{
	public FeelingSpooked()
		: base(INTERRUPT.Feeling_Spooked)
	{
		base.duration = 5;
		base.doesStopCurrentAction = true;
		base.doesDropCurrentJob = true;
		base.interruptIconString = GoapActionStateDB.Cowering_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Social };
	}
}

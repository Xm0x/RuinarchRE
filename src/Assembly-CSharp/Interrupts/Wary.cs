namespace Interrupts;

public class Wary : Interrupt
{
	public Wary()
		: base(INTERRUPT.Wary)
	{
		base.interruptIconString = GoapActionStateDB.Question_Icon;
		base.duration = 2;
		base.doesStopCurrentAction = true;
		base.shouldAddLogs = false;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Social };
	}
}

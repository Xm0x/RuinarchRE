namespace Interrupts;

public class Cursing : Interrupt
{
	public Cursing()
		: base(INTERRUPT.Cursing)
	{
		base.duration = 2;
		base.doesStopCurrentAction = false;
		base.shouldStopMovement = false;
		base.interruptIconString = GoapActionStateDB.Anger_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Social };
	}
}

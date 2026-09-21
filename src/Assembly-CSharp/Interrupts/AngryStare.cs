namespace Interrupts;

public class AngryStare : Interrupt
{
	public AngryStare()
		: base(INTERRUPT.Angry_Stare)
	{
		base.duration = 3;
		base.doesStopCurrentAction = false;
		base.shouldStopMovement = false;
		base.interruptIconString = GoapActionStateDB.Anger_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Social };
	}
}

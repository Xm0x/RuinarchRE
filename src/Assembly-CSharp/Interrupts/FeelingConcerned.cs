namespace Interrupts;

public class FeelingConcerned : Interrupt
{
	public FeelingConcerned()
		: base(INTERRUPT.Feeling_Concerned)
	{
		base.interruptIconString = GoapActionStateDB.Sad_Icon;
		base.duration = 0;
		base.isSimulateneous = true;
		base.shouldAddLogs = false;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Social };
	}
}

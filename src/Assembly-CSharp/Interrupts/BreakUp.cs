namespace Interrupts;

public class BreakUp : Interrupt
{
	public BreakUp()
		: base(INTERRUPT.Break_Up)
	{
		base.duration = 0;
		base.isSimulateneous = true;
		base.interruptIconString = GoapActionStateDB.Heartbroken_Icon;
		base.logTags = new LOG_TAG[2]
		{
			LOG_TAG.Life_Changes,
			LOG_TAG.Social
		};
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		interruptHolder.actor.nonActionEventsComponent.NormalBreakUp(interruptHolder.target as Character, interruptHolder.reason);
		return true;
	}
}

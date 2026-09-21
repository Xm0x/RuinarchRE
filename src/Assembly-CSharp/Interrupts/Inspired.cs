namespace Interrupts;

public class Inspired : Interrupt
{
	public Inspired()
		: base(INTERRUPT.Inspired)
	{
		base.duration = 0;
		base.isSimulateneous = true;
		base.shouldShowNotif = false;
		base.interruptIconString = GoapActionStateDB.Happy_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Social };
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		interruptHolder.actor.needsComponent.AdjustHope(5f);
		return true;
	}
}

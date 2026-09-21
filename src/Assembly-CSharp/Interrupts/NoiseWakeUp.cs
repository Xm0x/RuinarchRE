namespace Interrupts;

public class NoiseWakeUp : Interrupt
{
	public NoiseWakeUp()
		: base(INTERRUPT.Noise_Wake_Up)
	{
		base.duration = 1;
		base.doesDropCurrentJob = true;
		base.doesStopCurrentAction = true;
		base.interruptIconString = GoapActionStateDB.Shock_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Social };
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		_ = interruptHolder.actor;
		return true;
	}
}

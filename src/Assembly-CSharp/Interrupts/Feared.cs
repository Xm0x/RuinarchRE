namespace Interrupts;

public class Feared : Interrupt
{
	public Feared()
		: base(INTERRUPT.Feared)
	{
		base.duration = 0;
		base.doesStopCurrentAction = true;
		base.doesDropCurrentJob = true;
		base.interruptIconString = GoapActionStateDB.Cowering_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Combat };
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		interruptHolder.actor.combatComponent.Flight(interruptHolder.target, "Feared");
		return base.ExecuteInterruptStartEffect(interruptHolder, ref overrideEffectLog, goapNode);
	}
}

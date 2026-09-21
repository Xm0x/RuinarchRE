namespace Interrupts;

public class Alerted : Interrupt
{
	public Alerted()
		: base(INTERRUPT.Alerted)
	{
		base.duration = 1;
		base.doesStopCurrentAction = true;
		base.doesDropCurrentJob = true;
		base.interruptIconString = GoapActionStateDB.Shock_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Social };
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		Character characterResponsible = null;
		if (interruptHolder.actor != interruptHolder.target)
		{
			characterResponsible = interruptHolder.target as Character;
		}
		interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, "Alerted", characterResponsible);
		return true;
	}
}

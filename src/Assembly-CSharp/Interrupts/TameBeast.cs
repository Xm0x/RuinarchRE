namespace Interrupts;

public class TameBeast : Interrupt
{
	public TameBeast()
		: base(INTERRUPT.Tame_Beast)
	{
		base.duration = 0;
		base.isSimulateneous = true;
		base.interruptIconString = GoapActionStateDB.Magic_Icon;
		base.shouldAddLogs = true;
		base.logTags = new LOG_TAG[1];
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		Character actor = interruptHolder.actor;
		if (interruptHolder.target is Character p_target)
		{
			actor.petComponent.TameCharacter(p_target, p_setRelationship: true);
		}
		return true;
	}
}

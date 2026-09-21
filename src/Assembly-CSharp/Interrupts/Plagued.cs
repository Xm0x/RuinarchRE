namespace Interrupts;

public class Plagued : Interrupt
{
	public Plagued()
		: base(INTERRUPT.Plagued)
	{
		base.duration = 0;
		base.isSimulateneous = true;
		base.interruptIconString = GoapActionStateDB.No_Icon;
		base.logTags = new LOG_TAG[1];
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		if (PlagueDisease.Instance.AddPlaguedStatusOnPOIWithLifespanDuration(interruptHolder.actor))
		{
			overrideEffectLog = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", base.name + " contract", base.logTags);
			overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			return true;
		}
		return base.ExecuteInterruptStartEffect(interruptHolder, ref overrideEffectLog, goapNode);
	}
}

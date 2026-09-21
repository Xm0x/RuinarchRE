namespace Interrupts;

public class Cry : Interrupt
{
	public Cry()
		: base(INTERRUPT.Cry)
	{
		base.duration = 3;
		base.doesStopCurrentAction = true;
		base.interruptIconString = GoapActionStateDB.Sad_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Social };
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("Interrupts_Reason_Table", interruptHolder.reason);
		if (interruptHolder.actor == interruptHolder.target)
		{
			overrideEffectLog = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", base.name + " cry_self", base.logTags);
			overrideEffectLog.AddToFillers(null, localizedValue, LOG_IDENTIFIER.STRING_1);
		}
		else
		{
			overrideEffectLog = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", base.name + " cry_other", base.logTags);
			overrideEffectLog.AddToFillers(interruptHolder.target, interruptHolder.target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			overrideEffectLog.AddToFillers(null, localizedValue, LOG_IDENTIFIER.APPEND);
		}
		overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		return true;
	}
}

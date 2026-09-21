namespace Interrupts;

public class PassOut : Interrupt
{
	public PassOut()
		: base(INTERRUPT.Pass_Out)
	{
		base.duration = 0;
		base.isSimulateneous = true;
		base.interruptIconString = GoapActionStateDB.No_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Social };
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("Interrupts_Reason_Table", interruptHolder.reason);
		overrideEffectLog = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", base.name + " effect", LOG_TAG.Major);
		overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		overrideEffectLog.AddToFillers(null, localizedValue, LOG_IDENTIFIER.STRING_1);
		interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, "Unconscious");
		return true;
	}
}

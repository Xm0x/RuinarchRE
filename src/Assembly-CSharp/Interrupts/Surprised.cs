namespace Interrupts;

public class Surprised : Interrupt
{
	public Surprised()
		: base(INTERRUPT.Surprised)
	{
		base.duration = 3;
		base.doesStopCurrentAction = true;
		base.interruptIconString = GoapActionStateDB.Shock_Icon;
		base.shouldShowNotif = false;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Social };
	}

	public override Log CreateEffectLog(Character actor, IPointOfInterest target)
	{
		Log log = base.CreateEffectLog(actor, target);
		if (log != null && actor.interruptComponent.currentInterrupt != null)
		{
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("Interrupts_Reason_Table", actor.interruptComponent.currentInterrupt.reason);
			log.AddToFillers(null, localizedValue, LOG_IDENTIFIER.STRING_1);
			return log;
		}
		return null;
	}

	public override void AddAdditionalFillersToThoughtLog(Log log, Character actor)
	{
		base.AddAdditionalFillersToThoughtLog(log, actor);
		if (log != null && actor.interruptComponent.currentInterrupt != null)
		{
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("Interrupts_Reason_Table", actor.interruptComponent.currentInterrupt.reason);
			log.AddToFillers(null, localizedValue, LOG_IDENTIFIER.STRING_1);
		}
	}
}

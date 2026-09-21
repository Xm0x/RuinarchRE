namespace Interrupts;

public class Shocked : Interrupt
{
	public Shocked()
		: base(INTERRUPT.Shocked)
	{
		base.duration = 2;
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
			string reason = actor.interruptComponent.currentInterrupt.reason;
			string value = LocalizationManager.Instance.GetLocalizedValue("Interrupts_Reason_Table", reason);
			if (string.IsNullOrEmpty(value))
			{
				value = reason;
			}
			log.AddToFillers(null, value, LOG_IDENTIFIER.STRING_1);
			return log;
		}
		return null;
	}

	public override void AddAdditionalFillersToThoughtLog(Log log, Character actor)
	{
		base.AddAdditionalFillersToThoughtLog(log, actor);
		if (log != null && actor.interruptComponent.currentInterrupt != null)
		{
			string reason = actor.interruptComponent.currentInterrupt.reason;
			string value = LocalizationManager.Instance.GetLocalizedValue("Interrupts_Reason_Table", reason);
			if (string.IsNullOrEmpty(value))
			{
				value = reason;
			}
			log.AddToFillers(null, value, LOG_IDENTIFIER.STRING_1);
		}
	}
}

namespace Interrupts;

public class Cowering : Interrupt
{
	public Cowering()
		: base(INTERRUPT.Cowering)
	{
		base.duration = 6;
		base.doesStopCurrentAction = true;
		base.doesDropCurrentJob = true;
		base.interruptIconString = GoapActionStateDB.Cowering_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Combat };
	}

	public override Log CreateEffectLog(Character actor, IPointOfInterest target)
	{
		Log log = base.CreateEffectLog(actor, target);
		if (log != null && actor.interruptComponent.currentInterrupt != null)
		{
			string value = LocalizationManager.Instance.GetLocalizedValue("Interrupts_Reason_Table", actor.interruptComponent.currentInterrupt.reason);
			if (string.IsNullOrEmpty(value))
			{
				value = actor.interruptComponent.currentInterrupt.reason;
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
			string value = LocalizationManager.Instance.GetLocalizedValue("Interrupts_Reason_Table", actor.interruptComponent.currentInterrupt.reason);
			if (string.IsNullOrEmpty(value))
			{
				value = actor.interruptComponent.currentInterrupt.reason;
			}
			log.AddToFillers(null, value, LOG_IDENTIFIER.STRING_1);
		}
	}
}

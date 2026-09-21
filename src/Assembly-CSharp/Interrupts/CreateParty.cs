namespace Interrupts;

public class CreateParty : Interrupt
{
	public CreateParty()
		: base(INTERRUPT.Create_Party)
	{
		base.duration = 0;
		base.isSimulateneous = true;
		base.interruptIconString = GoapActionStateDB.No_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Party };
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		PartyManager.Instance.CreateNewParty(interruptHolder.actor);
		return true;
	}

	public override Log CreateEffectLog(Character actor, IPointOfInterest target)
	{
		Log log = base.CreateEffectLog(actor, target);
		if (log != null)
		{
			log.AddToFillers(actor.partyComponent.currentParty, actor.partyComponent.currentParty.partyName, LOG_IDENTIFIER.PARTY_1);
			return log;
		}
		return null;
	}
}

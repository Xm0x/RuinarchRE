namespace Interrupts;

public class Chat : Interrupt
{
	public Chat()
		: base(INTERRUPT.Chat)
	{
		base.duration = 0;
		base.isSimulateneous = true;
		base.interruptIconString = GoapActionStateDB.Social_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Social };
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		interruptHolder.actor.nonActionEventsComponent.ForceChatCharacter(interruptHolder.target as Character, ref overrideEffectLog);
		return true;
	}
}

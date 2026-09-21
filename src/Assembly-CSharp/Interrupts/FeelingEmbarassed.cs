namespace Interrupts;

public class FeelingEmbarassed : Interrupt
{
	public FeelingEmbarassed()
		: base(INTERRUPT.Feeling_Embarassed)
	{
		base.duration = 0;
		base.doesStopCurrentAction = true;
		base.interruptIconString = GoapActionStateDB.Shock_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Social };
	}

	public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder)
	{
		interruptHolder.actor.combatComponent.Flight(interruptHolder.target, "Embarrassed");
		return true;
	}
}

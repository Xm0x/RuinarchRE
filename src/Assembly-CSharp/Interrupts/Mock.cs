namespace Interrupts;

public class Mock : Interrupt
{
	public Mock()
		: base(INTERRUPT.Mock)
	{
		base.duration = 0;
		base.isSimulateneous = true;
		base.interruptIconString = GoapActionStateDB.Mock_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Social };
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		if (interruptHolder.target is Character character && character != interruptHolder.actor && character.limiterComponent.canWitness)
		{
			character.relationshipContainer.AdjustOpinion(character, interruptHolder.actor, "Base", -3);
		}
		return true;
	}
}

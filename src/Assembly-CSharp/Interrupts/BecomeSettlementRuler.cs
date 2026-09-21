namespace Interrupts;

public class BecomeSettlementRuler : Interrupt
{
	public BecomeSettlementRuler()
		: base(INTERRUPT.Become_Settlement_Ruler)
	{
		base.duration = 0;
		base.isSimulateneous = true;
		base.interruptIconString = GoapActionStateDB.No_Icon;
		base.logTags = new LOG_TAG[1];
		base.shouldShowNotif = true;
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		interruptHolder.actor.homeSettlement.SetRuler(interruptHolder.actor);
		overrideEffectLog = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", base.name + " became_ruler", LOG_TAG.Major);
		overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		overrideEffectLog.AddToFillers(interruptHolder.actor.homeSettlement, interruptHolder.actor.homeSettlement.name, LOG_IDENTIFIER.LANDMARK_1);
		return true;
	}
}

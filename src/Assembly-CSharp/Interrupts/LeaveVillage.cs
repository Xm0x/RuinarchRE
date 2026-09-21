using Locations.Settlements;

namespace Interrupts;

public class LeaveVillage : Interrupt
{
	public LeaveVillage()
		: base(INTERRUPT.Leave_Village)
	{
		base.duration = 0;
		base.isSimulateneous = true;
		base.interruptIconString = GoapActionStateDB.No_Icon;
		base.logTags = new LOG_TAG[1];
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		Character actor = interruptHolder.actor;
		BaseSettlement homeSettlement = actor.homeSettlement;
		if (homeSettlement != null)
		{
			overrideEffectLog = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", base.name + " left", LOG_TAG.Major);
			overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			overrideEffectLog.AddToFillers(homeSettlement, homeSettlement.name, LOG_IDENTIFIER.LANDMARK_1);
			actor.MigrateHomeStructureTo(null);
		}
		return true;
	}
}

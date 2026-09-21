using Inner_Maps.Location_Structures;

namespace Interrupts;

public class LeaveHome : Interrupt
{
	public LeaveHome()
		: base(INTERRUPT.Leave_Home)
	{
		base.duration = 0;
		base.isSimulateneous = true;
		base.interruptIconString = GoapActionStateDB.No_Icon;
		base.logTags = new LOG_TAG[1];
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		Character actor = interruptHolder.actor;
		LocationStructure homeStructure = actor.homeStructure;
		if (homeStructure != null)
		{
			overrideEffectLog = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", base.name + " left", LOG_TAG.Major);
			overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			overrideEffectLog.AddToFillers(homeStructure, homeStructure.name, LOG_IDENTIFIER.LANDMARK_1);
			bool affectSettlement = false;
			if (homeStructure.settlementLocation != null && homeStructure.settlementLocation.locationType != LOCATION_TYPE.VILLAGE)
			{
				affectSettlement = true;
			}
			if (actor.necromancerTrait != null && homeStructure == actor.necromancerTrait.lairStructure)
			{
				actor.necromancerTrait.SetLairStructure(null);
			}
			actor.MigrateHomeStructureTo(null, broadcast: true, addToRegionResidents: true, affectSettlement);
		}
		return true;
	}
}

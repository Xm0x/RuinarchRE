namespace Interrupts;

public class Resign : Interrupt
{
	public Resign()
		: base(INTERRUPT.Resign)
	{
		base.duration = 0;
		base.isSimulateneous = true;
		base.interruptIconString = GoapActionStateDB.No_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Major };
		base.shouldShowNotif = true;
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		Character actor = interruptHolder.actor;
		Faction faction = actor.faction;
		NPCSettlement homeSettlement = actor.homeSettlement;
		if (actor.isFactionLeader && actor.isSettlementRuler)
		{
			faction.SetLeader(null);
			homeSettlement.SetRuler(null);
			overrideEffectLog = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", base.name + " resign_both", LOG_TAG.Major);
			overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			overrideEffectLog.AddToFillers(faction, faction.name, LOG_IDENTIFIER.FACTION_1);
			overrideEffectLog.AddToFillers(homeSettlement, homeSettlement.name, LOG_IDENTIFIER.LANDMARK_1);
		}
		else if (actor.isFactionLeader)
		{
			faction.SetLeader(null);
			overrideEffectLog = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", base.name + " resign_faction_leader", LOG_TAG.Major);
			overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			overrideEffectLog.AddToFillers(faction, faction.name, LOG_IDENTIFIER.FACTION_1);
		}
		else if (actor.isSettlementRuler)
		{
			homeSettlement.SetRuler(null);
			overrideEffectLog = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", base.name + " resign_ruler", LOG_TAG.Major);
			overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			overrideEffectLog.AddToFillers(homeSettlement, homeSettlement.name, LOG_IDENTIFIER.LANDMARK_1);
		}
		return true;
	}
}

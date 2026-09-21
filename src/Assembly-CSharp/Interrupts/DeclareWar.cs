namespace Interrupts;

public class DeclareWar : Interrupt
{
	public DeclareWar()
		: base(INTERRUPT.Declare_War)
	{
		base.duration = 0;
		base.isSimulateneous = true;
		base.interruptIconString = GoapActionStateDB.Hostile_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Major };
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		Character actor = interruptHolder.actor;
		Character character = interruptHolder.target as Character;
		if (actor.faction != null && character != null)
		{
			Faction faction = character.faction;
			actor.faction.SetRelationshipFor(faction, FACTION_RELATIONSHIP_STATUS.Hostile);
			overrideEffectLog = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", base.name + " effect", base.logTags);
			overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			overrideEffectLog.AddToFillers(actor.faction, actor.faction.name, LOG_IDENTIFIER.FACTION_1);
			overrideEffectLog.AddToFillers(faction, faction.name, LOG_IDENTIFIER.FACTION_2);
			Messenger.Broadcast(FactionSignals.WAR_DECLARED, actor.faction, faction);
		}
		return false;
	}
}

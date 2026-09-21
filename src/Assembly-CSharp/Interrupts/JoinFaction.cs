namespace Interrupts;

public class JoinFaction : Interrupt
{
	public JoinFaction()
		: base(INTERRUPT.Join_Faction)
	{
		base.duration = 0;
		base.isSimulateneous = true;
		base.interruptIconString = GoapActionStateDB.No_Icon;
		base.logTags = new LOG_TAG[1];
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		if (interruptHolder.target is Character { faction: var faction } character)
		{
			if (character == interruptHolder.actor && !string.IsNullOrEmpty(interruptHolder.reason))
			{
				faction = FactionManager.Instance.GetFactionByPersistentID(interruptHolder.reason);
			}
			string text = interruptHolder.identifier;
			bool bypassIdeologyChecking = interruptHolder.identifier == "join_faction_necro" || interruptHolder.identifier == "join_faction_lycan_prism_event";
			if (interruptHolder.identifier == "join_bandits")
			{
				faction = FactionManager.Instance.banditFaction;
			}
			if (interruptHolder.actor.ChangeFactionTo(faction, bypassIdeologyChecking))
			{
				if (interruptHolder.identifier == "join_faction_lycan_prism_event")
				{
					text = "join_faction_normal";
				}
				overrideEffectLog = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", base.name + " " + text, base.logTags);
				overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				overrideEffectLog.AddToFillers(faction, faction.name, LOG_IDENTIFIER.FACTION_1);
				overrideEffectLog.AddToFillers(character, character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				return true;
			}
			interruptHolder.actor.ChangeToDefaultFaction();
		}
		return base.ExecuteInterruptStartEffect(interruptHolder, ref overrideEffectLog, goapNode);
	}
}

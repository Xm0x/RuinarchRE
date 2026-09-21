using UtilityScripts;

namespace Interrupts;

public class LeftParty : Interrupt
{
	public LeftParty()
		: base(INTERRUPT.Left_Party)
	{
		base.duration = 0;
		base.isSimulateneous = true;
		base.interruptIconString = GoapActionStateDB.No_Icon;
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		Party currentParty = interruptHolder.actor.partyComponent.currentParty;
		if (currentParty != null)
		{
			LogFiller logFiller = ObjectPoolManager.Instance.CreateNewLogFiller(interruptHolder.actor, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			string value = Utilities.LogReplacer(LocalizationManager.Instance.GetLocalizedValue("Interrupts_Reason_Table", interruptHolder.reason), logFiller);
			overrideEffectLog = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", base.name + " effect", LOG_TAG.Party);
			overrideEffectLog.AddToFillers(logFiller);
			overrideEffectLog.AddToFillers(currentParty, currentParty.partyName, LOG_IDENTIFIER.PARTY_1);
			overrideEffectLog.AddToFillers(null, value, LOG_IDENTIFIER.STRING_2);
			ObjectPoolManager.Instance.ReturnLogFillerToPool(logFiller);
			currentParty.RemoveMember(interruptHolder.actor);
		}
		return true;
	}
}

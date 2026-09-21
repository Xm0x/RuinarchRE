namespace Interrupts;

public class Taunted : Interrupt
{
	public Taunted()
		: base(INTERRUPT.Taunted)
	{
		base.duration = 0;
		base.isSimulateneous = true;
		base.interruptIconString = GoapActionStateDB.Anger_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Combat };
		base.shouldAddLogs = false;
		base.shouldShowNotif = false;
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		Character character = interruptHolder.target as Character;
		Character actor = interruptHolder.actor;
		bool currentTargetCombatLethality = actor.combatComponent.GetCurrentTargetCombatLethality();
		actor.traitContainer.AddTrait(actor, "Taunted", character);
		actor.combatComponent.ClearHostilesInRange(processCombatBehavior: false);
		actor.combatComponent.Fight(character, "Taunted", null, currentTargetCombatLethality, willAttackBecauseOfCrime: false, bypassBannedHostiles: true);
		return true;
	}
}

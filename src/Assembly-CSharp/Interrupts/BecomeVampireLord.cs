using Traits;

namespace Interrupts;

public class BecomeVampireLord : Interrupt
{
	public BecomeVampireLord()
		: base(INTERRUPT.Become_Vampire_Lord)
	{
		base.duration = 0;
		base.isSimulateneous = true;
		base.interruptIconString = GoapActionStateDB.No_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Major };
		base.shouldShowNotif = true;
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		interruptHolder.actor.classComponent.AssignClass("Vampire Lord");
		interruptHolder.actor.traitContainer.GetTraitOrStatus<Vampire>("Vampire").SetHasBecomeVampireLord(state: true);
		if (interruptHolder.actor.faction != null && interruptHolder.actor.faction.GetCrimeSeverity(interruptHolder.actor, interruptHolder.actor, CRIME_TYPE.Vampire) != CRIME_SEVERITY.None)
		{
			interruptHolder.actor.MigrateHomeStructureTo(null);
			if (interruptHolder.actor.faction.isMajorFaction)
			{
				interruptHolder.actor.faction.AddBannedCharacter(interruptHolder.actor);
			}
			interruptHolder.actor.interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Faction, interruptHolder.actor, "left_faction_vampire");
		}
		return true;
	}
}

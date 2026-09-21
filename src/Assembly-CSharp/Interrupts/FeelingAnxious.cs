namespace Interrupts;

public class FeelingAnxious : Interrupt
{
	public FeelingAnxious()
		: base(INTERRUPT.Feeling_Anxious)
	{
		base.interruptIconString = GoapActionStateDB.Cowering_Icon;
		base.duration = 0;
		base.doesStopCurrentAction = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Social };
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		Character actor = interruptHolder.actor;
		IPointOfInterest target = interruptHolder.target;
		if (actor.hasMarker && actor.marker.IsPOIInVision(target))
		{
			if (target is GenericTileObject)
			{
				actor.combatComponent.Flight(target, "Anxiety_Attack");
			}
			else
			{
				actor.combatComponent.FightOrFlight(target, "Anxiety_Attack", null, isLethal: false);
			}
			actor.traitContainer.RemoveTrait(actor, "Anxious");
		}
		return true;
	}
}

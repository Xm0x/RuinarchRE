using System.Collections.Generic;

namespace Interrupts;

public class StumbleKnockout : Interrupt
{
	public StumbleKnockout()
		: base(INTERRUPT.Stumble_Knockout)
	{
		base.duration = 0;
		base.doesStopCurrentAction = true;
		base.interruptIconString = GoapActionStateDB.Injured_Icon;
		base.logTags = new LOG_TAG[1];
		base.shouldAddLogs = true;
	}

	public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder)
	{
		interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, "Unconscious");
		return true;
	}

	public override void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status)
	{
		base.PopulateReactionsToActor(reactions, actor, target, witness, interrupt, status);
		if (witness.relationshipContainer.IsFriendsWith(actor))
		{
			reactions.Add(EMOTION.Concern);
		}
		else if (witness.relationshipContainer.IsEnemiesWith(actor))
		{
			reactions.Add(EMOTION.Scorn);
		}
	}
}

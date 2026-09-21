using System.Collections.Generic;

namespace Interrupts;

public class ZombieDeath : Interrupt
{
	public ZombieDeath()
		: base(INTERRUPT.Zombie_Death)
	{
		base.duration = 3;
		base.doesStopCurrentAction = true;
		base.interruptIconString = GoapActionStateDB.Death_Icon;
		base.isIntel = true;
		base.logTags = new LOG_TAG[2]
		{
			LOG_TAG.Life_Changes,
			LOG_TAG.Player
		};
	}

	public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder)
	{
		interruptHolder.actor.Death("Zombie Virus", null, null, interruptHolder.effectLog, null, null, this);
		return true;
	}

	public override void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status)
	{
		base.PopulateReactionsToActor(reactions, actor, target, witness, interrupt, status);
		reactions.Add(EMOTION.Shock);
		switch (witness.relationshipContainer.GetOpinionLabel(actor))
		{
		case "Acquaintance":
		case "Friend":
		case "Close Friend":
			reactions.Add(EMOTION.Concern);
			break;
		case "Rival":
			reactions.Add(EMOTION.Scorn);
			break;
		}
		if (status == REACTION_STATUS.WITNESSED && witness.traitContainer.HasTrait("Coward"))
		{
			reactions.Add(EMOTION.Fear);
		}
	}
}

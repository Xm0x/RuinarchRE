using System.Collections.Generic;

namespace Interrupts;

public class HypothermiaDeath : Interrupt
{
	public HypothermiaDeath()
		: base(INTERRUPT.Hypothermia_Death)
	{
		base.interruptIconString = GoapActionStateDB.Death_Icon;
		base.duration = 4;
		base.doesStopCurrentAction = true;
		base.doesDropCurrentJob = true;
		base.logTags = new LOG_TAG[1];
	}

	public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder)
	{
		interruptHolder.actor.Death("normal", null, null, interruptHolder.effectLog, null, null, this);
		return true;
	}

	public override void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status)
	{
		base.PopulateReactionsToActor(reactions, actor, target, witness, interrupt, status);
		reactions.Add(EMOTION.Shock);
		string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
		switch (opinionLabel)
		{
		case "Acquaintance":
		case "Friend":
		case "Close Friend":
			reactions.Add(EMOTION.Concern);
			break;
		default:
			if (witness.relationshipContainer.IsFamilyMemberOrLoverOrAffairAndNotRival(actor))
			{
				reactions.Add(EMOTION.Concern);
			}
			else if (opinionLabel == "Rival")
			{
				reactions.Add(EMOTION.Scorn);
			}
			break;
		}
		if (status == REACTION_STATUS.WITNESSED && witness.traitContainer.HasTrait("Coward"))
		{
			reactions.Add(EMOTION.Fear);
		}
	}
}

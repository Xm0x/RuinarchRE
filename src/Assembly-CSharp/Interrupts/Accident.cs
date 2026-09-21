using System.Collections.Generic;
using UtilityScripts;

namespace Interrupts;

public class Accident : Interrupt
{
	public Accident()
		: base(INTERRUPT.Accident)
	{
		base.duration = 1;
		base.doesStopCurrentAction = true;
		base.doesDropCurrentJob = true;
		base.isIntel = true;
		base.interruptIconString = GoapActionStateDB.Injured_Icon;
		base.logTags = new LOG_TAG[1];
	}

	public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder)
	{
		if (interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, "Injured"))
		{
			return true;
		}
		return base.ExecuteInterruptEndEffect(interruptHolder);
	}

	public override void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status)
	{
		base.PopulateReactionsToActor(reactions, actor, target, witness, interrupt, status);
		string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
		if ((witness.relationshipContainer.IsFamilyMember(actor) || witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.AFFAIR)) && !witness.relationshipContainer.IsEnemiesWith(actor))
		{
			reactions.Add(EMOTION.Concern);
			return;
		}
		switch (opinionLabel)
		{
		case "Friend":
		case "Close Friend":
			reactions.Add(EMOTION.Concern);
			break;
		case "Acquaintance":
			if (GameUtilities.RollChance(50))
			{
				reactions.Add(EMOTION.Concern);
			}
			break;
		case "Enemy":
		case "Rival":
			reactions.Add(EMOTION.Scorn);
			break;
		}
	}

	public override Log CreateEffectLog(Character actor, IPointOfInterest target)
	{
		Log log = base.CreateEffectLog(actor, target);
		if (log != null && actor.interruptComponent.currentInterrupt != null)
		{
			string reason = actor.interruptComponent.currentInterrupt.reason;
			log.AddToFillers(null, reason, LOG_IDENTIFIER.STRING_1);
			return log;
		}
		return null;
	}

	public override void AddAdditionalFillersToThoughtLog(Log log, Character actor)
	{
		base.AddAdditionalFillersToThoughtLog(log, actor);
		if (log != null && actor.interruptComponent.currentInterrupt != null)
		{
			string reason = actor.interruptComponent.currentInterrupt.reason;
			log.AddToFillers(null, reason, LOG_IDENTIFIER.STRING_1);
		}
	}
}

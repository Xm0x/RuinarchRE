using System.Collections.Generic;
using UtilityScripts;

namespace Interrupts;

public class PulledDown : Interrupt
{
	public PulledDown()
		: base(INTERRUPT.Pulled_Down)
	{
		base.duration = 15;
		base.doesStopCurrentAction = true;
		base.doesDropCurrentJob = true;
		base.shouldEndOnSeize = true;
		base.interruptIconString = GoapActionStateDB.Cowering_Icon;
		base.logTags = new LOG_TAG[1];
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, "Pulled Down");
		return base.ExecuteInterruptStartEffect(interruptHolder, ref overrideEffectLog, goapNode);
	}

	public override bool PerTickInterrupt(InterruptHolder interruptHolder)
	{
		interruptHolder.actor.AdjustHP(-300, ELEMENTAL_TYPE.Poison);
		if (!interruptHolder.actor.HasHealth())
		{
			Scorpion scorpion = interruptHolder.target as Scorpion;
			scorpion.SetHeldCharacter(null);
			interruptHolder.actor.Death("pulled_down", null, scorpion, null, null, null, null, isPlayerSource: false, scorpion);
		}
		return true;
	}

	public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder)
	{
		interruptHolder.actor.traitContainer.RemoveTrait(interruptHolder.actor, "Pulled Down");
		if (!interruptHolder.actor.isDead)
		{
			interruptHolder.actor.AdjustHP(-300, ELEMENTAL_TYPE.Poison);
			if (!interruptHolder.actor.HasHealth())
			{
				Scorpion scorpion = interruptHolder.target as Scorpion;
				scorpion.SetHeldCharacter(null);
				interruptHolder.actor.Death("pulled_down", null, scorpion, null, null, null, null, isPlayerSource: false, scorpion);
			}
		}
		return true;
	}

	public override bool OnForceEndInterrupt(InterruptHolder interruptHolder)
	{
		interruptHolder.actor.traitContainer.RemoveTrait(interruptHolder.actor, "Pulled Down");
		(interruptHolder.target as Scorpion).SetHeldCharacter(null);
		return true;
	}

	public override void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status)
	{
		base.PopulateReactionsToActor(reactions, actor, target, witness, interrupt, status);
		string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
		reactions.Add(EMOTION.Shock);
		if (opinionLabel == "Enemy")
		{
			if (GameUtilities.RollChance(50))
			{
				reactions.Add(EMOTION.Scorn);
			}
		}
		else if (opinionLabel == "Rival")
		{
			reactions.Add(EMOTION.Scorn);
		}
	}
}

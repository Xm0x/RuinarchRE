using System.Collections.Generic;
using Traits;
using UtilityScripts;

namespace Interrupts;

public class NarcolepticNapMedium : Interrupt
{
	public NarcolepticNapMedium()
		: base(INTERRUPT.Narcoleptic_Nap_Medium)
	{
		base.name = "Narcoleptic Nap";
		base.duration = 15;
		base.doesStopCurrentAction = true;
		base.interruptIconString = GoapActionStateDB.Sleep_Icon;
		base.isIntel = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Needs };
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, "Resting");
		return true;
	}

	public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder)
	{
		interruptHolder.actor.traitContainer.RemoveTrait(interruptHolder.actor, "Resting");
		return true;
	}

	public override void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status)
	{
		base.PopulateReactionsToActor(reactions, actor, target, witness, interrupt, status);
		Narcoleptic traitOrStatus = actor.traitContainer.GetTraitOrStatus<Narcoleptic>("Narcoleptic");
		if (traitOrStatus == null || traitOrStatus.HasWitnessedNarcolepticAttack(witness))
		{
			return;
		}
		traitOrStatus.AddNarcolepticAttackWitness(witness);
		string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
		if (opinionLabel == "Enemy")
		{
			if (GameUtilities.RollChance(50))
			{
				reactions.Add(EMOTION.Scorn);
			}
			else
			{
				reactions.Add(EMOTION.Shock);
			}
		}
		else if (opinionLabel == "Rival")
		{
			reactions.Add(EMOTION.Scorn);
		}
		else
		{
			reactions.Add(EMOTION.Shock);
		}
	}
}

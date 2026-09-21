using System.Collections.Generic;

namespace Interrupts;

public class BurningAtStake : Interrupt
{
	public BurningAtStake()
		: base(INTERRUPT.Burning_At_Stake)
	{
		base.duration = 20;
		base.doesStopCurrentAction = true;
		base.doesDropCurrentJob = true;
		base.shouldEndOnSeize = true;
		base.interruptIconString = GoapActionStateDB.Burn_Icon;
		base.logTags = new LOG_TAG[1];
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, "Burning At Stake");
		return base.ExecuteInterruptStartEffect(interruptHolder, ref overrideEffectLog, goapNode);
	}

	public override bool PerTickInterrupt(InterruptHolder interruptHolder)
	{
		interruptHolder.actor.AdjustHP(-500, ELEMENTAL_TYPE.Fire);
		if (!interruptHolder.actor.HasHealth())
		{
			Character character = interruptHolder.target as Character;
			interruptHolder.actor.Death("burn_at_stake", null, character, null, null, null, null, isPlayerSource: false, character);
		}
		return true;
	}

	public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder)
	{
		interruptHolder.actor.traitContainer.RemoveTrait(interruptHolder.actor, "Burning At Stake");
		if (!interruptHolder.actor.isDead)
		{
			interruptHolder.actor.AdjustHP(-500, ELEMENTAL_TYPE.Fire);
			if (!interruptHolder.actor.HasHealth())
			{
				Character character = interruptHolder.target as Character;
				Character actor = interruptHolder.actor;
				object deathSource = character;
				actor.Death("burn_at_stake", null, character, null, null, null, this, isPlayerSource: false, deathSource);
			}
		}
		return true;
	}

	public override bool OnForceEndInterrupt(InterruptHolder interruptHolder)
	{
		interruptHolder.actor.traitContainer.RemoveTrait(interruptHolder.actor, "Burning At Stake");
		return true;
	}

	public override void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status)
	{
		base.PopulateReactionsToActor(reactions, actor, target, witness, interrupt, status);
		if (witness.relationshipContainer.GetOpinionLabel(actor) == "Close Friend")
		{
			reactions.Add(EMOTION.Sadness);
			reactions.Add(EMOTION.Concern);
		}
		else if (witness.relationshipContainer.IsFamilyMemberOrLoverOrAffairAndNotRival(actor))
		{
			reactions.Add(EMOTION.Concern);
			reactions.Add(EMOTION.Despair);
		}
		else
		{
			reactions.Add(EMOTION.Shock);
		}
	}
}

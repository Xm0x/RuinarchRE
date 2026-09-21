using System.Collections.Generic;
using UnityEngine;

namespace Interrupts;

public class Stumble : Interrupt
{
	public Stumble()
		: base(INTERRUPT.Stumble)
	{
		base.duration = 2;
		base.doesStopCurrentAction = true;
		base.interruptIconString = GoapActionStateDB.Injured_Icon;
		base.isIntel = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder)
	{
		float num = (float)Random.Range(1, 6) / 100f;
		int num2 = Mathf.CeilToInt((float)interruptHolder.actor.maxHP * num);
		interruptHolder.actor.AdjustHP(-num2, ELEMENTAL_TYPE.Normal, triggerDeath: false, null, null, showHPBar: true);
		if (!interruptHolder.actor.HasHealth())
		{
			interruptHolder.actor.Death("Stumble");
		}
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

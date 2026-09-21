using System.Collections.Generic;
using UtilityScripts;

namespace Interrupts;

public class Tantrum : Interrupt
{
	public Tantrum()
		: base(INTERRUPT.Tantrum)
	{
		base.duration = 5;
		base.doesStopCurrentAction = true;
		base.doesDropCurrentJob = true;
		base.interruptIconString = GoapActionStateDB.Anger_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Social };
	}

	public override void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status)
	{
		base.PopulateReactionsToActor(reactions, actor, target, witness, interrupt, status);
		string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
		if (opinionLabel == "Close Friend")
		{
			reactions.Add(EMOTION.Concern);
		}
		else
		{
			reactions.Add(EMOTION.Disappointment);
		}
		if (opinionLabel == "Enemy" || opinionLabel == "Rival")
		{
			if (GameUtilities.RollChance(50))
			{
				reactions.Add(EMOTION.Scorn);
			}
			else
			{
				reactions.Add(EMOTION.Disgust);
			}
		}
	}
}

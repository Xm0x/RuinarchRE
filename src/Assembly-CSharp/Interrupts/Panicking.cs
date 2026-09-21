using System.Collections.Generic;

namespace Interrupts;

public class Panicking : Interrupt
{
	public Panicking()
		: base(INTERRUPT.Panicking)
	{
		base.duration = 6;
		base.doesStopCurrentAction = true;
		base.doesDropCurrentJob = true;
		base.interruptIconString = GoapActionStateDB.Shock_Icon;
		base.shouldShowNotif = false;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Social };
	}

	public override Log CreateEffectLog(Character actor, IPointOfInterest target)
	{
		Log log = base.CreateEffectLog(actor, target);
		if (log != null && actor.interruptComponent.currentInterrupt != null)
		{
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("Interrupts_Reason_Table", actor.interruptComponent.currentInterrupt.reason);
			log.AddToFillers(null, localizedValue, LOG_IDENTIFIER.STRING_1);
			return log;
		}
		return null;
	}

	public override void AddAdditionalFillersToThoughtLog(Log log, Character actor)
	{
		base.AddAdditionalFillersToThoughtLog(log, actor);
		if (log != null && actor.interruptComponent.currentInterrupt != null)
		{
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("Interrupts_Reason_Table", actor.interruptComponent.currentInterrupt.reason);
			log.AddToFillers(null, localizedValue, LOG_IDENTIFIER.STRING_1);
		}
	}

	public override void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status)
	{
		base.PopulateReactionsToActor(reactions, actor, target, witness, interrupt, status);
		string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
		if (!witness.traitContainer.HasTrait("Dousing") || !actor.interruptComponent.currentInterrupt.reason.Contains("FIRE"))
		{
			reactions.Add(EMOTION.Shock);
		}
		if (witness.relationshipContainer.IsFriendsWith(actor))
		{
			reactions.Add(EMOTION.Concern);
		}
		else if (witness.relationshipContainer.IsFamilyMemberOrLoverOrAffairAndNotRival(actor))
		{
			reactions.Add(EMOTION.Concern);
		}
		else if (opinionLabel == "Acquaintance")
		{
			reactions.Add(EMOTION.Concern);
		}
	}
}

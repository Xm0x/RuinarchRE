using System.Collections.Generic;
using Locations.Settlements.Settlement_Events;

namespace Interrupts;

public class HeartAttack : Interrupt
{
	public HeartAttack()
		: base(INTERRUPT.Heart_Attack)
	{
		base.duration = 4;
		base.doesStopCurrentAction = true;
		base.doesDropCurrentJob = true;
		base.interruptIconString = GoapActionStateDB.Death_Icon;
		base.isIntel = true;
		base.logTags = new LOG_TAG[2]
		{
			LOG_TAG.Life_Changes,
			LOG_TAG.Player
		};
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		if (interruptHolder.reason != "Plagued" && !string.IsNullOrEmpty(interruptHolder.reason))
		{
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("Interrupts_Reason_Table", interruptHolder.reason);
			overrideEffectLog = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", base.name + " effect_with_reason", base.logTags);
			overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			overrideEffectLog.AddToFillers(null, localizedValue, LOG_IDENTIFIER.STRING_1);
		}
		return false;
	}

	public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder)
	{
		bool isPlayerSource = false;
		if (interruptHolder.reason == "Plagued")
		{
			isPlayerSource = true;
			if (ChanceData.RollChance(CHANCE_TYPE.Plagued_Event_Heart_Attck) && interruptHolder.actor.homeSettlement != null && PlaguedEvent.HasMinimumAmountOfPlaguedVillagersForEvent(interruptHolder.actor.homeSettlement) && !interruptHolder.actor.homeSettlement.eventManager.HasActiveEvent(SETTLEMENT_EVENT.Plagued_Event) && interruptHolder.actor.homeSettlement.eventManager.CanHaveEvents())
			{
				interruptHolder.actor.homeSettlement.eventManager.AddNewActiveEvent(SETTLEMENT_EVENT.Plagued_Event);
			}
		}
		interruptHolder.actor.Death("Heart Attack", null, null, interruptHolder.effectLog, null, null, this, isPlayerSource);
		return true;
	}

	public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status)
	{
		string result = base.ReactionToActor(actor, target, witness, interrupt, status);
		if (status == REACTION_STATUS.WITNESSED && actor.homeSettlement != null)
		{
			NPCSettlement homeSettlement = actor.homeSettlement;
			if (homeSettlement != null && interrupt.reason == "Plagued")
			{
				homeSettlement.SetIsPlagued(state: true);
			}
		}
		return result;
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
		if (witness.traitContainer.HasTrait("Coward"))
		{
			reactions.Add(EMOTION.Fear);
		}
	}
}

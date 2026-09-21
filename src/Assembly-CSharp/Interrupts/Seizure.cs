using System.Collections.Generic;
using Locations.Settlements.Settlement_Events;
using UtilityScripts;

namespace Interrupts;

public class Seizure : Interrupt
{
	public Seizure()
		: base(INTERRUPT.Seizure)
	{
		base.interruptIconString = GoapActionStateDB.Injured_Icon;
		base.duration = 6;
		base.doesStopCurrentAction = true;
		base.doesDropCurrentJob = true;
		base.isIntel = true;
		base.logTags = new LOG_TAG[1];
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		if (ChanceData.RollChance(CHANCE_TYPE.Plagued_Event_Seizure) && interruptHolder.actor.homeSettlement != null && PlaguedEvent.HasMinimumAmountOfPlaguedVillagersForEvent(interruptHolder.actor.homeSettlement) && !interruptHolder.actor.homeSettlement.eventManager.HasActiveEvent(SETTLEMENT_EVENT.Plagued_Event) && interruptHolder.actor.homeSettlement.eventManager.CanHaveEvents())
		{
			interruptHolder.actor.homeSettlement.eventManager.AddNewActiveEvent(SETTLEMENT_EVENT.Plagued_Event);
		}
		return base.ExecuteInterruptStartEffect(interruptHolder, ref overrideEffectLog, goapNode);
	}

	public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status)
	{
		string result = base.ReactionToActor(actor, target, witness, interrupt, status);
		if (GameUtilities.RollChance(25))
		{
			NPCSettlement homeSettlement = witness.homeSettlement;
			if (homeSettlement != null && homeSettlement.eventManager.HasActiveEvent(SETTLEMENT_EVENT.Plagued_Event) && !witness.relationshipContainer.IsFriendsWith(actor))
			{
				witness.assumptionComponent.CreateAndReactToNewAssumption(actor, actor, INTERACTION_TYPE.IS_PLAGUED, REACTION_STATUS.WITNESSED, !actor.traitContainer.HasTrait("Plagued"));
			}
		}
		return result;
	}

	public override void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status)
	{
		base.PopulateReactionsToActor(reactions, actor, target, witness, interrupt, status);
		reactions.Add(EMOTION.Shock);
		string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
		if (opinionLabel == "Friend" || opinionLabel == "Close Friend")
		{
			reactions.Add(EMOTION.Concern);
		}
		else if (witness.relationshipContainer.IsFamilyMemberOrLoverOrAffairAndNotRival(actor))
		{
			reactions.Add(EMOTION.Concern);
		}
		else if (opinionLabel != "Enemy" && opinionLabel != "Rival")
		{
			reactions.Add(EMOTION.Concern);
		}
	}

	public override bool PerTickInterrupt(InterruptHolder interruptHolder)
	{
		if (interruptHolder.actor.needsComponent.HasNeeds())
		{
			interruptHolder.actor.needsComponent.AdjustTiredness(-2f);
		}
		return base.PerTickInterrupt(interruptHolder);
	}
}

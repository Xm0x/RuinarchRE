using System.Collections.Generic;
using Locations.Settlements.Settlement_Events;
using UtilityScripts;

namespace Interrupts;

public class Puke : Interrupt
{
	public Puke()
		: base(INTERRUPT.Puke)
	{
		base.duration = 3;
		base.doesStopCurrentAction = true;
		base.interruptIconString = GoapActionStateDB.Sick_Icon;
		base.isIntel = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Needs };
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		interruptHolder.actor.SetPOIState(POI_STATE.INACTIVE);
		LogFiller logFiller = ObjectPoolManager.Instance.CreateNewLogFiller(interruptHolder.target, LOG_IDENTIFIER.TARGET_CHARACTER);
		string value = Utilities.LogReplacer(LocalizationManager.Instance.GetLocalizedValue("Interrupts_Reason_Table", interruptHolder.reason), logFiller);
		overrideEffectLog = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", base.name + " effect", base.logTags);
		overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		overrideEffectLog.AddToFillers(logFiller);
		overrideEffectLog.AddToFillers(null, value, LOG_IDENTIFIER.STRING_1);
		ObjectPoolManager.Instance.ReturnLogFillerToPool(logFiller);
		if (ChanceData.RollChance(CHANCE_TYPE.Plagued_Event_Puke) && interruptHolder.actor.homeSettlement != null && PlaguedEvent.HasMinimumAmountOfPlaguedVillagersForEvent(interruptHolder.actor.homeSettlement) && !interruptHolder.actor.homeSettlement.eventManager.HasActiveEvent(SETTLEMENT_EVENT.Plagued_Event) && interruptHolder.actor.homeSettlement.eventManager.CanHaveEvents())
		{
			interruptHolder.actor.homeSettlement.eventManager.AddNewActiveEvent(SETTLEMENT_EVENT.Plagued_Event);
		}
		return true;
	}

	public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder)
	{
		interruptHolder.actor.SetPOIState(POI_STATE.ACTIVE);
		return true;
	}

	public override bool PerTickInterrupt(InterruptHolder interruptHolder)
	{
		if (interruptHolder.actor.needsComponent.HasNeeds())
		{
			interruptHolder.actor.needsComponent.AdjustFullness(-1f);
		}
		return base.PerTickInterrupt(interruptHolder);
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
		if (witness.relationshipContainer.GetOpinionLabel(actor) == "Close Friend")
		{
			reactions.Add(EMOTION.Concern);
		}
		else if (witness.relationshipContainer.IsFamilyMemberOrLoverOrAffairAndNotRival(actor))
		{
			reactions.Add(EMOTION.Concern);
		}
		else
		{
			reactions.Add(EMOTION.Disgust);
		}
	}
}

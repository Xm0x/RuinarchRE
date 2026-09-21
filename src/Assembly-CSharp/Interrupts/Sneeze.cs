using Locations.Settlements.Settlement_Events;
using Plague.Transmission;
using UtilityScripts;

namespace Interrupts;

public class Sneeze : Interrupt
{
	public Sneeze()
		: base(INTERRUPT.Sneeze)
	{
		base.duration = 1;
		base.doesStopCurrentAction = true;
		base.interruptIconString = GoapActionStateDB.Sick_Icon;
		base.isIntel = true;
		base.logTags = new LOG_TAG[1];
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		Character actor = interruptHolder.actor;
		if (actor.traitContainer.HasTrait("Plagued"))
		{
			Transmission<AirborneTransmission>.Instance.Transmit(actor, null, PlagueDisease.Instance.GetTransmissionLevel(PLAGUE_TRANSMISSION.Airborne));
			return true;
		}
		if (ChanceData.RollChance(CHANCE_TYPE.Plagued_Event_Sneeze) && interruptHolder.actor.homeSettlement != null && PlaguedEvent.HasMinimumAmountOfPlaguedVillagersForEvent(interruptHolder.actor.homeSettlement) && !interruptHolder.actor.homeSettlement.eventManager.HasActiveEvent(SETTLEMENT_EVENT.Plagued_Event) && interruptHolder.actor.homeSettlement.eventManager.CanHaveEvents())
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
}

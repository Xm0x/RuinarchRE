using System.Collections.Generic;
using Locations.Settlements.Settlement_Events;

public class IsPlagued : GoapAction
{
	public override bool isTargetSelf => true;

	public IsPlagued()
		: base(INTERACTION_TYPE.IS_PLAGUED)
	{
		base.actionIconString = GoapActionStateDB.Sick_Icon;
		base.logTags = new LOG_TAG[1];
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Plague Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 0;
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		return REACTABLE_EFFECT.Negative;
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		if (witness.relationshipContainer.IsFriendsWith(actor))
		{
			reactions.Add(EMOTION.Concern);
		}
		else if (witness.relationshipContainer.IsEnemiesWith(actor))
		{
			reactions.Add(EMOTION.Disgust);
			reactions.Add(EMOTION.Scorn);
		}
		else
		{
			reactions.Add(EMOTION.Disgust);
			reactions.Add(EMOTION.Fear);
		}
	}

	public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		string result = base.ReactionToActor(actor, target, witness, node, status);
		if (witness.homeSettlement.eventManager.HasActiveEvent(out PlaguedEvent p_settlementEvent) && p_settlementEvent.rulerDecision == PLAGUE_EVENT_RESPONSE.Quarantine && !actor.traitContainer.HasTrait("Quarantined"))
		{
			witness.homeSettlement.settlementJobTriggerComponent.TriggerQuarantineJob(actor);
		}
		return result;
	}

	public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime)
	{
		return CRIME_TYPE.Plagued;
	}

	public override CRIME_TYPE GetRawCrimeType(Character actor)
	{
		return CRIME_TYPE.Plagued;
	}

	public void PrePlagueSuccess(ActualGoapNode goapNode)
	{
	}

	public void PerTickPlagueSuccess(ActualGoapNode goapNode)
	{
	}

	public void AfterPlagueSuccess(ActualGoapNode goapNode)
	{
	}
}

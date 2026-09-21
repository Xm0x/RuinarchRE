using System.Collections.Generic;

public class WatchSleep : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.INDIRECT;

	public WatchSleep()
		: base(INTERACTION_TYPE.WATCH_SLEEP)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		base.actionIconString = GoapActionStateDB.Happy_Icon;
		base.doesNotStopTargetCharacter = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Social };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Watch Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override string ReactionOfTarget(Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status)
	{
		string result = base.ReactionOfTarget(actor, target, node, status);
		if (target is Character character && !actor.relationshipContainer.IsLoverOrAffair(character) && status == REACTION_STATUS.WITNESSED)
		{
			character.interruptComponent.TriggerInterrupt(INTERRUPT.Stopped, actor);
		}
		return result;
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		Character character = target as Character;
		if (witness.traitContainer.HasTrait("Psychopath"))
		{
			reactions.Add(EMOTION.Arousal);
			reactions.Add(EMOTION.Approval);
			return;
		}
		reactions.Add(EMOTION.Disgust);
		if (witness.relationshipContainer.IsFriendsOrAcquaintancesWith(character))
		{
			if (witness.relationshipContainer.IsFriendsOrAcquaintancesWith(actor))
			{
				reactions.Add(EMOTION.Shock);
				reactions.Add(EMOTION.Disappointment);
			}
			else
			{
				reactions.Add(EMOTION.Anger);
				reactions.Add(EMOTION.Disapproval);
			}
		}
	}

	public override void PopulateEmotionReactionsToTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToTarget(reactions, actor, target, witness, node, status);
		Character character = target as Character;
		if (!witness.traitContainer.HasTrait("Psychopath") && witness.relationshipContainer.IsFriendsOrAcquaintancesWith(character))
		{
			reactions.Add(EMOTION.Shock);
			reactions.Add(EMOTION.Concern);
		}
	}

	public override void PopulateEmotionReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsOfTarget(reactions, actor, target, node, status);
		if (target is Character target2 && !actor.relationshipContainer.IsLoverOrAffair(target2))
		{
			reactions.Add(EMOTION.Shock);
			reactions.Add(EMOTION.Disgust);
			reactions.Add(EMOTION.Anger);
		}
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		return REACTABLE_EFFECT.Negative;
	}

	public void AfterWatchSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.needsComponent.AdjustHappiness(10f);
		goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Euphoric");
	}
}

using System.Collections.Generic;

public class LickTileObject : GoapAction
{
	public LickTileObject()
		: base(INTERACTION_TYPE.LICK_TILE_OBJECT)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		base.actionIconString = GoapActionStateDB.Happy_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Social };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Lick Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		Character character = null;
		if (target is TileObject tileObject)
		{
			character = tileObject.characterOwner;
		}
		if (witness.traitContainer.HasTrait("Psychopath"))
		{
			reactions.Add(EMOTION.Arousal);
			reactions.Add(EMOTION.Approval);
			return;
		}
		reactions.Add(EMOTION.Disgust);
		if (character != null)
		{
			if (character == witness)
			{
				if (!actor.relationshipContainer.IsLoverOrAffair(character))
				{
					reactions.Add(EMOTION.Shock);
					reactions.Add(EMOTION.Anger);
				}
			}
			else if (witness.relationshipContainer.IsFriendsOrAcquaintancesWith(character))
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
		else
		{
			reactions.Add(EMOTION.Shock);
			reactions.Add(EMOTION.Disapproval);
		}
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		return REACTABLE_EFFECT.Negative;
	}

	public void AfterLickSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.needsComponent.AdjustHappiness(10f);
		goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Euphoric");
	}
}

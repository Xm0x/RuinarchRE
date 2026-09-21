using System.Collections.Generic;

public class EatAlive : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.CONSUME;

	public EatAlive()
		: base(INTERACTION_TYPE.EAT_ALIVE)
	{
		base.actionIconString = GoapActionStateDB.Eat_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Needs };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Eat Alive Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override void PopulateEmotionReactionsToTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToTarget(reactions, actor, target, witness, node, status);
		if (!(target is Character character))
		{
			return;
		}
		if (witness.relationshipContainer.IsFriendsWith(character))
		{
			if (!witness.traitContainer.HasTrait("Psychopath"))
			{
				reactions.Add(EMOTION.Despair);
				reactions.Add(EMOTION.Sadness);
			}
		}
		else if (witness.relationshipContainer.IsFamilyMemberOrLoverOrAffairAndNotRival(character) && !witness.traitContainer.HasTrait("Psychopath"))
		{
			reactions.Add(EMOTION.Despair);
			reactions.Add(EMOTION.Sadness);
		}
	}

	public override bool ShouldActionBeAnIntel(ActualGoapNode node)
	{
		if (!node.actor.isNormalCharacter && node.target is Character character)
		{
			return character.isNormalCharacter;
		}
		return false;
	}

	public void PerTickEatAliveSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.actor.race == RACE.ELVES && goapNode.poiTarget is RatMeat)
		{
			goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Poor Meal");
		}
		if (goapNode.actor.hasMarker)
		{
			AkSoundEngine.PostEvent("Play_Monster_Eat", goapNode.actor.marker.gameObject);
		}
		goapNode.poiTarget.AdjustHP(-10, ELEMENTAL_TYPE.Normal, triggerDeath: true, goapNode.actor, null, showHPBar: true);
	}

	public void AfterEatAliveSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.actor.race == RACE.ELVES && goapNode.poiTarget is Character character && (character.race == RACE.RAT || character.race == RACE.RATMAN))
		{
			goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Poor Meal");
		}
	}
}

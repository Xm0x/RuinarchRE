using System.Collections.Generic;
using Traits;
using UnityEngine;

public class Whip : GoapAction
{
	public Whip()
		: base(INTERACTION_TYPE.WHIP)
	{
		base.actionIconString = GoapActionStateDB.Hostile_Icon;
		base.logTags = new LOG_TAG[3]
		{
			LOG_TAG.Crimes,
			LOG_TAG.Work,
			LOG_TAG.Life_Changes
		};
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Injured", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Whip Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		if (node.target is Character character && !character.traitContainer.HasTrait("Criminal"))
		{
			return REACTABLE_EFFECT.Negative;
		}
		return REACTABLE_EFFECT.Positive;
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		Character character = target as Character;
		if (character.crimeComponent.HasWantedCrime() && character.crimeComponent.IsTargetOfACrime(witness))
		{
			reactions.Add(EMOTION.Approval);
			return;
		}
		if (witness.relationshipContainer.IsFriendsWith(character) && !witness.traitContainer.HasTrait("Psychopath"))
		{
			reactions.Add(EMOTION.Resentment);
		}
		if (!witness.traitContainer.HasTrait("Psychopath") && ((witness.traitContainer.HasTrait("Coward") && Random.Range(0, 100) < 75) || (!witness.traitContainer.HasTrait("Coward") && Random.Range(0, 100) < 15)))
		{
			reactions.Add(EMOTION.Fear);
		}
	}

	public override void PopulateEmotionReactionsToTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToTarget(reactions, actor, target, witness, node, status);
		Character character = target as Character;
		if (witness.relationshipContainer.HasOpinionLabelWithCharacter(character, "Acquaintance"))
		{
			if (!witness.traitContainer.HasTrait("Psychopath") && Random.Range(0, 100) < 50)
			{
				reactions.Add(EMOTION.Concern);
			}
		}
		else if (witness.relationshipContainer.IsFriendsWith(character))
		{
			if (!witness.traitContainer.HasTrait("Psychopath"))
			{
				reactions.Add(EMOTION.Concern);
			}
		}
		else if (witness.relationshipContainer.IsEnemiesWith(character) && !witness.traitContainer.HasTrait("Diplomatic"))
		{
			reactions.Add(EMOTION.Scorn);
		}
	}

	public override void PopulateEmotionReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsOfTarget(reactions, actor, target, node, status);
		Character obj = target as Character;
		if (Random.Range(0, 100) < 20)
		{
			reactions.Add(EMOTION.Resentment);
		}
		if (obj.traitContainer.HasTrait("Hothead") || Random.Range(0, 100) < 20)
		{
			reactions.Add(EMOTION.Anger);
		}
	}

	public void AfterWhipSuccess(ActualGoapNode goapNode)
	{
		Character character = goapNode.target as Character;
		if (character.traitContainer.HasTrait("Criminal"))
		{
			character.traitContainer.GetTraitOrStatus<Criminal>("Criminal").SetIsImprisoned(state: false);
		}
		character.crimeComponent.SetDecisionAndJudgeToAllUnpunishedCrimesWantedBy(character.faction, CRIME_STATUS.Punished, goapNode.actor);
		OtherData[] otherData = goapNode.otherData;
		if (otherData != null && otherData[0].obj is CrimeData crimeData && crimeData.IsCrimeFabricated())
		{
			crimeData.TryTriggerGrudgeAgainstJudgeOrReporter();
		}
		character.crimeComponent.RemoveAllCrimesWantedBy(goapNode.actor.faction);
		character.traitContainer.RemoveRestrainAndImprison(character, goapNode.actor);
		character.traitContainer.AddTrait(character, "Injured", goapNode.actor);
		character.traitContainer.GetTraitOrStatus<Trait>("Injured")?.SetGainedFromDoingAction(goapNode.action.goapType, goapNode.isStealth);
	}
}

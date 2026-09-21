using System.Collections.Generic;
using Traits;
using UnityEngine;
using UtilityScripts;

public class Sing : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.VERBAL;

	public Sing()
		: base(INTERACTION_TYPE.SING)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
		base.actionIconString = GoapActionStateDB.Sing_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Needs };
	}

	public override bool ShouldActionBeAnIntel(ActualGoapNode node)
	{
		return true;
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Sing Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		int num = Utilities.Rng.Next(85, 126);
		int numOfTimesActionDone = actor.jobComponent.GetNumOfTimesActionDone(this);
		if (numOfTimesActionDone > 5)
		{
			num += 2000;
		}
		if (actor.traitContainer.HasTrait("Music Hater"))
		{
			num += 2000;
		}
		if (actor.traitContainer.HasTrait("Music Lover"))
		{
			num -= 20;
		}
		int num2 = 10 * numOfTimesActionDone;
		return num + num2;
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		Trait traitOrStatus = witness.traitContainer.GetTraitOrStatus<Trait>("Music Hater", "Music Lover");
		if (traitOrStatus == null)
		{
			return;
		}
		if (traitOrStatus.name == "Music Hater")
		{
			PLAYER_SKILL_TYPE afflictionSkillType = traitOrStatus.GetAfflictionSkillType();
			if (afflictionSkillType == PLAYER_SKILL_TYPE.NONE)
			{
				return;
			}
			SkillData skillData = PlayerSkillManager.Instance.GetSkillData(afflictionSkillType);
			if (skillData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Angry_Upon_Hear_Music))
			{
				if (skillData.currentLevel >= 2)
				{
					reactions.Add(EMOTION.Rage);
				}
				else
				{
					reactions.Add(EMOTION.Anger);
				}
			}
			return;
		}
		reactions.Add(EMOTION.Approval);
		if (RelationshipManager.Instance.GetCompatibilityBetween(witness, actor) >= 4 && RelationshipManager.IsSexuallyCompatible(witness, actor) && witness.moodComponent.moodState != MOOD_STATE.Critical)
		{
			int num = 50;
			if (actor.traitContainer.HasTrait("Unattractive"))
			{
				num = 20;
			}
			if (Random.Range(0, 100) < num)
			{
				reactions.Add(EMOTION.Arousal);
			}
		}
	}

	public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		witness.traitContainer.GetTraitOrStatus<MusicHater>("Music Hater")?.ReactToMusicPerformer(witness, actor);
		return base.ReactionToActor(actor, target, witness, node, status);
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		if (witness.traitContainer.HasTrait("Music Hater"))
		{
			return REACTABLE_EFFECT.Negative;
		}
		if (witness.traitContainer.HasTrait("Music Lover"))
		{
			return REACTABLE_EFFECT.Positive;
		}
		return REACTABLE_EFFECT.Neutral;
	}

	public override bool IsHappinessRecoveryAction()
	{
		return true;
	}

	public void PreSingSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.jobComponent.IncreaseNumOfTimesActionDone(base.goapType);
	}

	public void PerTickSingSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.needsComponent.AdjustHappiness(6f);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapStructureIsNot(poiTarget.gridTileLocation.structure))
			{
				return false;
			}
			if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapAreaIsNot(poiTarget.gridTileLocation.area))
			{
				return false;
			}
			if (actor == poiTarget && !actor.traitContainer.HasTrait("Music Hater"))
			{
				return actor.moodComponent.moodState == MOOD_STATE.Normal;
			}
			return false;
		}
		return false;
	}
}

using System.Collections.Generic;
using Traits;
using UtilityScripts;

public class SmellHair : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.INDIRECT;

	public SmellHair()
		: base(INTERACTION_TYPE.SMELL_HAIR)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		base.actionIconString = GoapActionStateDB.Happy_Icon;
		base.doesNotStopTargetCharacter = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Social };
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Smell Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return Utilities.Rng.Next(1, 10);
	}

	public override bool IsHappinessRecoveryAction()
	{
		return true;
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
			if (poiTarget is Character character)
			{
				if (!character.traitContainer.HasTrait("Resting"))
				{
					return false;
				}
				Obsessed traitOrStatus = actor.traitContainer.GetTraitOrStatus<Obsessed>("Obsessed");
				if (traitOrStatus == null)
				{
					return false;
				}
				if (traitOrStatus.targetCharacter != poiTarget)
				{
					return false;
				}
				return true;
			}
			return false;
		}
		return false;
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		if (!goapActionInvalidity.isInvalid && node.poiTarget is Character character && !character.traitContainer.HasTrait("Resting"))
		{
			goapActionInvalidity.isInvalid = true;
			goapActionInvalidity.reason = "already_awake";
		}
		return goapActionInvalidity;
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

	public void PerTickSmellSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.needsComponent.AdjustHappiness(2f);
	}

	public void AfterSmellSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Euphoric");
	}
}

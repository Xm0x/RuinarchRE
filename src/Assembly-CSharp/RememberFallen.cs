using System.Collections.Generic;
using Inner_Maps;
using UtilityScripts;

public class RememberFallen : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.INDIRECT;

	public RememberFallen()
		: base(INTERACTION_TYPE.REMEMBER_FALLEN)
	{
		base.actionIconString = GoapActionStateDB.Sad_Icon;
		validTimeOfDays = new TIME_IN_WORDS[3]
		{
			TIME_IN_WORDS.EARLY_NIGHT,
			TIME_IN_WORDS.LATE_NIGHT,
			TIME_IN_WORDS.AFTER_MIDNIGHT
		};
		base.logTags = new LOG_TAG[1] { LOG_TAG.Social };
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
		SetState("Remember Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		if (actor.traitContainer.HasTrait("Enslaved") && (target.gridTileLocation == null || !target.gridTileLocation.IsInHomeOf(actor)))
		{
			return 2000;
		}
		if (actor.partyComponent.hasParty && actor.partyComponent.currentParty.isActive && actor.partyComponent.isActiveMember && target.gridTileLocation != null && actor.gridTileLocation != null)
		{
			LocationGridTile centerGridTile = target.gridTileLocation.area.gridTileComponent.centerGridTile;
			float distanceTo = actor.gridTileLocation.area.gridTileComponent.centerGridTile.GetDistanceTo(centerGridTile);
			int num = InnerMapManager.AreaLocationGridTileSize.x * 3;
			if (distanceTo > (float)num)
			{
				return 2000;
			}
		}
		int num2 = Utilities.Rng.Next(80, 121);
		int numOfTimesActionDone = actor.jobComponent.GetNumOfTimesActionDone(this);
		if (numOfTimesActionDone > 5)
		{
			num2 += 2000;
		}
		if (actor.traitContainer.HasTrait("Psychopath") || actor.partyComponent.isActiveMember)
		{
			num2 += 2000;
		}
		if (actor.moodComponent.moodState == MOOD_STATE.Bad || actor.moodComponent.moodState == MOOD_STATE.Critical || !actor.limiterComponent.isSociable)
		{
			num2 -= 15;
		}
		int num3 = 10 * numOfTimesActionDone;
		return num2 + num3;
	}

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		Character actor = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		_ = node.targetStructure;
		log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		Tombstone tombstone = poiTarget as Tombstone;
		log.AddToFillers(tombstone.character, tombstone.character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		if (!(target is Tombstone))
		{
			return;
		}
		Character character = (target as Tombstone).character;
		string opinionLabel = witness.relationshipContainer.GetOpinionLabel(character);
		if ((opinionLabel == "Friend" || opinionLabel == "Close Friend") && !witness.traitContainer.HasTrait("Psychopath"))
		{
			reactions.Add(EMOTION.Approval);
		}
		else if (opinionLabel == "Rival")
		{
			reactions.Add(EMOTION.Resentment);
			if (witness.relationshipContainer.IsFriendsWith(actor))
			{
				reactions.Add(EMOTION.Disappointment);
			}
		}
	}

	public override bool IsHappinessRecoveryAction()
	{
		return true;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (!poiTarget.IsAvailable() || poiTarget.gridTileLocation == null)
			{
				return false;
			}
			if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapStructureIsNot(poiTarget.gridTileLocation.structure))
			{
				return false;
			}
			if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapAreaIsNot(poiTarget.gridTileLocation.area))
			{
				return false;
			}
			if (poiTarget is Tombstone)
			{
				Character character = (poiTarget as Tombstone).character;
				return actor.relationshipContainer.GetRelationshipEffectWith(character) == RELATIONSHIP_EFFECT.POSITIVE;
			}
			return false;
		}
		return false;
	}

	public void PreRememberSuccess(ActualGoapNode goapNode)
	{
		Tombstone tombstone = goapNode.poiTarget as Tombstone;
		goapNode.descriptionLog.AddToFillers(tombstone.character, tombstone.character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
		goapNode.actor.jobComponent.IncreaseNumOfTimesActionDone(base.goapType);
	}

	public void PerTickRememberSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.needsComponent.AdjustHappiness(6f);
	}
}

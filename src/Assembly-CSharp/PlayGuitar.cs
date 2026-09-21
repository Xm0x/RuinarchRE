using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UnityEngine;
using UtilityScripts;

public class PlayGuitar : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;

	public PlayGuitar()
		: base(INTERACTION_TYPE.PLAY_GUITAR)
	{
		base.actionIconString = GoapActionStateDB.Entertain_Icon;
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
		SetState("Play Success", goapNode);
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
		if (target.gridTileLocation != null)
		{
			if (actor.trapStructure.IsTrapped())
			{
				if (actor.trapStructure.IsTrapStructure(target.gridTileLocation.structure))
				{
					num2 += 2000;
				}
			}
			else if (target.gridTileLocation.structure != actor.homeStructure)
			{
				num2 += 2000;
			}
		}
		if (actor.traitContainer.HasTrait("Music Lover"))
		{
			num2 += -25;
		}
		int num3 = 10 * numOfTimesActionDone;
		return num2 + num3;
	}

	public override void OnStopWhilePerforming(ActualGoapNode node)
	{
		base.OnStopWhilePerforming(node);
		_ = node.actor;
		node.poiTarget.SetPOIState(POI_STATE.ACTIVE);
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		IPointOfInterest poiTarget = node.poiTarget;
		if (!goapActionInvalidity.isInvalid && !poiTarget.IsAvailable())
		{
			goapActionInvalidity.isInvalid = true;
			goapActionInvalidity.stateName = "Play Fail";
		}
		return goapActionInvalidity;
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

	public void PrePlaySuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.jobComponent.IncreaseNumOfTimesActionDone(base.goapType);
		goapNode.poiTarget.SetPOIState(POI_STATE.INACTIVE);
	}

	public void PerTickPlaySuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.needsComponent.AdjustHappiness(3.34f);
	}

	public void AfterPlaySuccess(ActualGoapNode goapNode)
	{
		goapNode.poiTarget.SetPOIState(POI_STATE.ACTIVE);
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
			if (actor.traitContainer.HasTrait("Music Hater"))
			{
				return false;
			}
			if (poiTarget.gridTileLocation == null)
			{
				return false;
			}
			return true;
		}
		return false;
	}
}

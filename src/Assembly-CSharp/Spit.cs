using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UtilityScripts;

public class Spit : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.VERBAL;

	public Spit()
		: base(INTERACTION_TYPE.SPIT)
	{
		base.actionIconString = GoapActionStateDB.Anger_Icon;
		validTimeOfDays = new TIME_IN_WORDS[3]
		{
			TIME_IN_WORDS.MORNING,
			TIME_IN_WORDS.LUNCH_TIME,
			TIME_IN_WORDS.AFTERNOON
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
		SetState("Spit Success", goapNode);
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
		int num2 = Utilities.Rng.Next(80, 131);
		int numOfTimesActionDone = actor.jobComponent.GetNumOfTimesActionDone(this);
		if (numOfTimesActionDone > 5)
		{
			num2 += 2000;
		}
		if (!actor.partyComponent.isActiveMember)
		{
			num2 += 2000;
		}
		if (!actor.traitContainer.HasTrait("Angry", "Annoyed", "Drunk"))
		{
			num2 += 2000;
		}
		Betrayed traitOrStatus = actor.traitContainer.GetTraitOrStatus<Betrayed>("Betrayed");
		if (target is Tombstone tombstone && traitOrStatus != null && traitOrStatus.IsResponsibleForTrait(tombstone.character))
		{
			num2 -= 25;
		}
		if (actor.traitContainer.HasTrait("Evil"))
		{
			num2 -= 10;
		}
		if (actor.traitContainer.HasTrait("Treacherous"))
		{
			num2 -= 10;
		}
		int num3 = 10 * numOfTimesActionDone;
		return num2 + num3;
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		if (target is Tombstone)
		{
			Character character = (target as Tombstone).character;
			switch (witness.relationshipContainer.GetOpinionLabel(character))
			{
			case "Friend":
			case "Close Friend":
				reactions.Add(EMOTION.Anger);
				reactions.Add(EMOTION.Disapproval);
				break;
			case "Rival":
				reactions.Add(EMOTION.Approval);
				break;
			default:
				reactions.Add(EMOTION.Disapproval);
				break;
			}
		}
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		return REACTABLE_EFFECT.Negative;
	}

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		if (node.poiTarget is Tombstone tombstone)
		{
			log.AddToFillers(tombstone.character, tombstone.character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
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
			if (poiTarget is Tombstone { character: var character })
			{
				return actor.relationshipContainer.IsEnemiesWith(character);
			}
			return false;
		}
		return false;
	}

	public void PreSpitSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.jobComponent.IncreaseNumOfTimesActionDone(base.goapType);
	}

	public void PerTickSpitSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.needsComponent.AdjustHappiness(50f);
	}
}

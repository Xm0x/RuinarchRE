using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class Strangle : GoapAction
{
	public Strangle()
		: base(INTERACTION_TYPE.STRANGLE)
	{
		base.actionIconString = GoapActionStateDB.Anger_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION;
		base.logTags = new LOG_TAG[1];
	}

	public override bool ShouldActionBeAnIntel(ActualGoapNode node)
	{
		return true;
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.DEATH, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Strangle Success", goapNode);
	}

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		if (node.otherData != null && node.otherData.Length == 1)
		{
			string key = (string)node.otherData[0].obj;
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("GoapActionsStrings_Table", key);
			log.AddToFillers(null, localizedValue, LOG_IDENTIFIER.STRING_1);
		}
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override LocationStructure GetTargetStructure(ActualGoapNode node)
	{
		Character actor = node.actor;
		if (actor.homeStructure != null)
		{
			return actor.homeStructure;
		}
		return actor.currentRegion.wilderness;
	}

	public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		string result = base.ReactionToActor(actor, target, witness, node, status);
		if (actor != target && target is Character character && actor.faction != null && witness.faction != null && witness.faction.isMajorNonPlayerOrBandits && actor.faction.isMajorNonPlayerOrBandits && actor.faction != witness.faction && witness.faction == character.faction)
		{
			if (witness.isFactionLeader || witness.isSettlementRuler)
			{
				witness.faction.FactionProcessingAbductionOrMurder(actor, character, node);
				return result;
			}
			if (witness.faction.leader is Character || (witness.homeSettlement != null && witness.homeSettlement.ruler != null))
			{
				witness.jobComponent.TryCreateReportMurderOrAbduct(node);
			}
		}
		return result;
	}

	public override LocationGridTile GetTargetTileToGoTo(ActualGoapNode goapNode)
	{
		if (goapNode.targetStructure is Wilderness)
		{
			return goapNode.GetRandomNearbyTileFromActor(goapNode.actor);
		}
		return null;
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		if (!(target is Character))
		{
			return;
		}
		Character character = target as Character;
		if (actor != character)
		{
			if (witness.traitContainer.HasTrait("Psychopath"))
			{
				reactions.Add(EMOTION.Approval);
				return;
			}
			if (witness.traitContainer.HasTrait("Coward"))
			{
				reactions.Add(EMOTION.Fear);
				return;
			}
			switch (witness.relationshipContainer.GetOpinionLabel(character))
			{
			case "Rival":
				reactions.Add(EMOTION.Approval);
				break;
			case "Friend":
			case "Close Friend":
				reactions.Add(EMOTION.Anger);
				reactions.Add(EMOTION.Threatened);
				break;
			default:
				reactions.Add(EMOTION.Shock);
				reactions.Add(EMOTION.Disapproval);
				break;
			}
		}
		else
		{
			reactions.Add(EMOTION.Disapproval);
			reactions.Add(EMOTION.Shock);
			if (witness.traitContainer.HasTrait("Psychopath") || witness.relationshipContainer.IsEnemiesWith(actor))
			{
				reactions.Add(EMOTION.Scorn);
			}
		}
	}

	public override void PopulateEmotionReactionsToTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToTarget(reactions, actor, target, witness, node, status);
		if (!(target is Character))
		{
			return;
		}
		Character character = target as Character;
		if (actor != character)
		{
			if (witness.relationshipContainer.GetOpinionLabel(character) == "Rival")
			{
				reactions.Add(EMOTION.Scorn);
			}
			else
			{
				reactions.Add(EMOTION.Concern);
			}
		}
	}

	public override void PopulateEmotionReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsOfTarget(reactions, actor, target, node, status);
		if (!(target is Character))
		{
			return;
		}
		Character character = target as Character;
		if (actor != character)
		{
			reactions.Add(EMOTION.Anger);
			if (character.relationshipContainer.IsFriendsWith(actor) && !character.traitContainer.HasTrait("Psychopath"))
			{
				reactions.Add(EMOTION.Betrayal);
			}
		}
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		return REACTABLE_EFFECT.Negative;
	}

	public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime)
	{
		if (actor == target)
		{
			return CRIME_TYPE.None;
		}
		return CRIME_TYPE.Murder;
	}

	public override CRIME_TYPE GetRawCrimeType(Character actor)
	{
		return CRIME_TYPE.Murder;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (poiTarget == actor && poiTarget.IsAvailable())
			{
				return poiTarget.gridTileLocation != null;
			}
			return false;
		}
		return false;
	}

	public void PerTickStrangleSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.AdjustHP(-(int)((float)goapNode.actor.maxHP * 0.18f), ELEMENTAL_TYPE.Normal, triggerDeath: false, null, null, showHPBar: true);
	}

	public void AfterStrangleSuccess(ActualGoapNode goapNode)
	{
		Character character = null;
		if (goapNode.actor != goapNode.poiTarget)
		{
			character = goapNode.actor;
		}
		goapNode.actor.Death("normal", goapNode, character, goapNode.descriptionLog, null, null, null, isPlayerSource: false, character);
	}
}

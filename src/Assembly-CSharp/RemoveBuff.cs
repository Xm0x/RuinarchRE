using System.Collections.Generic;
using UnityEngine;

public class RemoveBuff : GoapAction
{
	public RemoveBuff()
		: base(INTERACTION_TYPE.REMOVE_BUFF)
	{
		base.doesNotStopTargetCharacter = true;
		base.actionIconString = GoapActionStateDB.Cult_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Crimes };
	}

	public override bool ShouldActionBeAnIntel(ActualGoapNode node)
	{
		return true;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Remove Buff Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 0;
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		return REACTABLE_EFFECT.Negative;
	}

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		OtherData[] otherData = node.otherData;
		if (otherData != null && otherData.Length == 1 && otherData[0] is StringOtherData stringOtherData)
		{
			log.AddToFillers(null, TraitManager.Instance.GetLocalizedNameOfTrait(stringOtherData.str), LOG_IDENTIFIER.STRING_1);
		}
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest target, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, target, otherData, job))
		{
			if (target != actor && target.traitContainer.HasTrait("Resting", "Unconscious") && target.traitContainer.HasTraitOf(TRAIT_TYPE.BUFF))
			{
				return actor.HasItem(TILE_OBJECT_TYPE.CULTIST_KIT);
			}
			return false;
		}
		return false;
	}

	public void AfterRemoveBuffSuccess(ActualGoapNode goapNode)
	{
		OtherData[] otherData = goapNode.otherData;
		if (otherData != null && otherData.Length == 1 && otherData[0] is StringOtherData { str: var str })
		{
			goapNode.target.traitContainer.RemoveTrait(goapNode.target, str, goapNode.actor);
			goapNode.actor.UnobtainItem(TILE_OBJECT_TYPE.CULTIST_KIT);
		}
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		if (!witness.traitContainer.HasTrait("Demon Cultist"))
		{
			reactions.Add(EMOTION.Shock);
			if (witness.relationshipContainer.IsFriendsWith(actor) || witness.relationshipContainer.HasOpinionLabelWithCharacter(actor, "Acquaintance"))
			{
				reactions.Add(EMOTION.Despair);
			}
			if (witness.traitContainer.HasTrait("Coward"))
			{
				reactions.Add(EMOTION.Fear);
			}
			else if (!witness.traitContainer.HasTrait("Psychopath"))
			{
				reactions.Add(EMOTION.Threatened);
			}
			if (!witness.relationshipContainer.IsEnemiesWith(actor))
			{
				reactions.Add(EMOTION.Disapproval);
			}
			return;
		}
		reactions.Add(EMOTION.Approval);
		if (RelationshipManager.IsSexuallyCompatible(witness, actor))
		{
			int num = 10 * witness.relationshipContainer.GetCompatibility(actor);
			if (Random.Range(0, 100) < num)
			{
				reactions.Add(EMOTION.Arousal);
			}
		}
	}

	public override void PopulateEmotionReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsOfTarget(reactions, actor, target, node, status);
		if (target is Character character)
		{
			reactions.Add(EMOTION.Shock);
			if (character.traitContainer.HasTrait("Coward"))
			{
				reactions.Add(EMOTION.Fear);
			}
			else
			{
				reactions.Add(EMOTION.Threatened);
			}
			if (character.relationshipContainer.IsFriendsWith(actor))
			{
				reactions.Add(EMOTION.Betrayal);
			}
		}
	}

	public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime)
	{
		return CRIME_TYPE.Demon_Worship;
	}
}

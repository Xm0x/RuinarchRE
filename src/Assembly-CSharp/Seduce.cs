using System.Collections.Generic;
using Traits;
using UtilityScripts;

public class Seduce : GoapAction
{
	public Seduce()
		: base(INTERACTION_TYPE.SEDUCE)
	{
		base.actionIconString = GoapActionStateDB.Flirt_Icon;
		base.logTags = new LOG_TAG[1];
	}

	public override bool ShouldActionBeAnIntel(ActualGoapNode node)
	{
		return true;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Seduce Success", goapNode);
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		Character actor = node.actor;
		_ = node.poiTarget;
		if (!goapActionInvalidity.isInvalid && (actor.relationshipContainer.HasAliveOrUnspawnedRelationship(RELATIONSHIP_TYPE.LOVER) || actor.relationshipContainer.HasAliveOrUnspawnedRelationship(RELATIONSHIP_TYPE.AFFAIR)))
		{
			Character character = actor.relationshipContainer.GetFirstAliveCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER) ?? actor.relationshipContainer.GetFirstAliveCharacterWithRelationship(RELATIONSHIP_TYPE.AFFAIR);
			if (character != null && actor.hasMarker && actor.marker.IsPOIInVision(character))
			{
				goapActionInvalidity.isInvalid = true;
				goapActionInvalidity.reason = "lover_in_range";
			}
		}
		return goapActionInvalidity;
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		if (!node.hasBeenReset && node.target is Character relatable && !node.actor.relationshipContainer.HasRelationshipWith(relatable, RELATIONSHIP_TYPE.LOVER) && node.actor.relationshipContainer.HasRelationship(RELATIONSHIP_TYPE.LOVER))
		{
			return REACTABLE_EFFECT.Negative;
		}
		return base.GetReactableEffect(node, witness);
	}

	public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime)
	{
		if (target is Character relatable && !actor.relationshipContainer.HasRelationshipWith(relatable, RELATIONSHIP_TYPE.LOVER) && actor.relationshipContainer.HasRelationship(RELATIONSHIP_TYPE.LOVER))
		{
			return CRIME_TYPE.Infidelity;
		}
		return base.GetCrimeType(actor, target, crime);
	}

	public override CRIME_TYPE GetRawCrimeType(Character actor)
	{
		return CRIME_TYPE.Infidelity;
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		if (status == REACTION_STATUS.WITNESSED)
		{
			reactions.Add(EMOTION.Shock);
		}
		if (!(target is Character character))
		{
			return;
		}
		if (!actor.relationshipContainer.HasRelationshipWith(character, RELATIONSHIP_TYPE.LOVER))
		{
			Character firstCharacterWithRelationship = actor.relationshipContainer.GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER);
			if (firstCharacterWithRelationship != null && firstCharacterWithRelationship == character && firstCharacterWithRelationship == witness)
			{
				reactions.Add(EMOTION.Betrayal);
				reactions.Add(EMOTION.Disapproval);
			}
			Character firstCharacterWithRelationship2 = character.relationshipContainer.GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER);
			if (firstCharacterWithRelationship2 != null && witness == firstCharacterWithRelationship2)
			{
				reactions.Add(EMOTION.Rage);
				if (witness.relationshipContainer.IsFriendsWith(actor) || witness.relationshipContainer.IsFamilyMember(actor))
				{
					reactions.Add(EMOTION.Betrayal);
				}
				else
				{
					reactions.Add(EMOTION.Resentment);
				}
			}
		}
		else if (witness.relationshipContainer.HasRelationshipWith(character, RELATIONSHIP_TYPE.AFFAIR))
		{
			reactions.Add(EMOTION.Resentment);
		}
		else if (witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.AFFAIR))
		{
			reactions.Add(EMOTION.Resentment);
		}
	}

	public override void PopulateEmotionReactionsToTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToTarget(reactions, actor, target, witness, node, status);
		if (!(target is Character target2) || !witness.relationshipContainer.IsLoverOrAffair(target2))
		{
			return;
		}
		bool flag = false;
		if (node.otherData != null)
		{
			for (int i = 0; i < node.otherData.Length; i++)
			{
				if (node.otherData[i] is StringOtherData { str: "fail" })
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			reactions.Add(EMOTION.Approval);
		}
		else
		{
			reactions.Add(EMOTION.Betrayal);
		}
	}

	public override string ReactionOfTarget(Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status)
	{
		if (target is Character character)
		{
			bool flag = false;
			if (node.otherData != null)
			{
				for (int i = 0; i < node.otherData.Length; i++)
				{
					if (node.otherData[i] is StringOtherData { str: "fail" })
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				node.IncreaseReactionCounter();
				CrimeManager.Instance.ReactToCrime(character, actor, target, target.factionOwner, node.crimeType, node, status);
				node.DecreaseReactionCounter();
			}
			List<EMOTION> list = RuinarchListPool<EMOTION>.Claim(5);
			PopulateEmotionReactionsOfTarget(list, actor, target, node, status);
			string text = string.Empty;
			int p_totalOpinionReduction = 0;
			string p_lastStrawReasonKey = string.Empty;
			for (int j = 0; j < list.Count; j++)
			{
				text += CharacterManager.Instance.TriggerEmotion(list[j], character, actor, status, ref p_totalOpinionReduction, ref p_lastStrawReasonKey, node, "", p_triggerOpinionChangesEffect: false);
			}
			if (p_totalOpinionReduction < 0 && !character.reactionComponent.isDisguised && !actor.reactionComponent.isDisguised)
			{
				character.relationshipContainer.CreateJobsOnOpinionReduced(character, actor, p_lastStrawReasonKey, p_totalOpinionReduction);
			}
			RuinarchListPool<EMOTION>.Release(list);
			return text;
		}
		return string.Empty;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job) && poiTarget is Character character)
		{
			if (actor != poiTarget && character.hasMarker)
			{
				return character.gridTileLocation != null;
			}
			return false;
		}
		return false;
	}

	public void PreSeduceSuccess(ActualGoapNode goapNode)
	{
		Character character = goapNode.poiTarget as Character;
		Character actor = goapNode.actor;
		bool flag = false;
		if (!character.limiterComponent.isSociable)
		{
			flag = true;
		}
		else if (actor.traitContainer.HasTrait("Unattractive") && GameUtilities.RollChance(50))
		{
			flag = true;
		}
		if (!flag && (character.relationshipContainer.HasAliveOrUnspawnedRelationship(RELATIONSHIP_TYPE.LOVER) || character.relationshipContainer.HasAliveOrUnspawnedRelationship(RELATIONSHIP_TYPE.AFFAIR)))
		{
			Character character2 = character.relationshipContainer.GetFirstAliveCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER) ?? character.relationshipContainer.GetFirstAliveCharacterWithRelationship(RELATIONSHIP_TYPE.AFFAIR);
			if (character2 != null)
			{
				switch (character.relationshipContainer.GetOpinionLabel(character2))
				{
				case "Acquaintance":
				case "Friend":
				case "Close Friend":
					if (character.hasMarker && character.marker.IsPOIInVision(character2))
					{
						flag = true;
					}
					break;
				}
			}
		}
		if (!flag && character.traitContainer.HasTrait("Hemophobic") && character.traitContainer.GetTraitOrStatus<Hemophobic>("Hemophobic").IsVampireKnown(actor))
		{
			flag = true;
		}
		if (!flag && character.traitContainer.HasTrait("Lycanphobic") && character.traitContainer.GetTraitOrStatus<Lycanphobic>("Lycanphobic").IsLycanKnown(actor))
		{
			flag = true;
		}
		if (!flag && character.traitContainer.HasTrait("Angry") && GameUtilities.RollChance(70) && character.traitContainer.GetTraitOrStatus<Angry>("Angry").IsResponsibleForTrait(actor))
		{
			flag = true;
		}
		if (!flag && GameUtilities.RollChance(90) && !RelationshipManager.IsSexuallyCompatibleOneSided(character, actor))
		{
			flag = true;
		}
		if (!flag && character.traitContainer.HasTrait("Griefstricken"))
		{
			flag = true;
		}
		if (character.raceSetting.category == CHARACTER_CATEGORY.Beast || character.raceSetting.category == CHARACTER_CATEGORY.Undead)
		{
			flag = false;
		}
		string text = "success";
		if (flag)
		{
			text = "fail";
			goapNode.SetOtherData(new OtherData[1]
			{
				new StringOtherData(text)
			});
		}
		Log log = ((character.raceSetting.category != CHARACTER_CATEGORY.Beast && character.raceSetting.category != CHARACTER_CATEGORY.Undead) ? GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", base.goapName + " " + text, base.logTags) : GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", "Seduce success beast or undead", base.logTags));
		log.AddToFillers(goapNode.actor, goapNode.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddToFillers(goapNode.poiTarget, goapNode.poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
		goapNode.OverrideDescriptionLog(log);
	}

	public void AfterSeduceSuccess(ActualGoapNode goapNode)
	{
		Character character = goapNode.poiTarget as Character;
		Character actor = goapNode.actor;
		bool flag = false;
		if (goapNode.otherData != null)
		{
			for (int i = 0; i < goapNode.otherData.Length; i++)
			{
				if (goapNode.otherData[i] is StringOtherData { str: "fail" })
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			character.relationshipContainer.AdjustOpinion(character, actor, "Rebuffed_Courtship", -4);
			actor.relationshipContainer.AdjustOpinion(actor, character, "Rebuffed_Courtship", -8);
			return;
		}
		character.relationshipContainer.AdjustOpinion(character, actor, "Seduced", 4);
		actor.relationshipContainer.AdjustOpinion(actor, character, "Seduced", 8);
		int chance = 35;
		bool flag2 = false;
		switch (character.relationshipContainer.GetOpinionLabel(actor))
		{
		case "Acquaintance":
			flag2 = true;
			break;
		case "Friend":
		case "Close Friend":
			flag2 = true;
			chance = 50;
			break;
		}
		if (!flag2)
		{
			return;
		}
		Character firstCharacterWithRelationship = character.relationshipContainer.GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER);
		Character firstCharacterWithRelationship2 = actor.relationshipContainer.GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER);
		if (firstCharacterWithRelationship == null && firstCharacterWithRelationship2 == null)
		{
			if (GameUtilities.RollChance(chance))
			{
				RelationshipManager.Instance.CreateNewRelationshipBetween(character, actor, RELATIONSHIP_TYPE.LOVER);
			}
		}
		else if (firstCharacterWithRelationship != actor && firstCharacterWithRelationship2 != character && GameUtilities.RollChance(50))
		{
			RelationshipManager.Instance.CreateNewRelationshipBetween(character, actor, RELATIONSHIP_TYPE.AFFAIR);
		}
	}
}

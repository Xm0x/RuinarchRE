using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public class Evangelize : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.VERBAL;

	public Evangelize()
		: base(INTERACTION_TYPE.EVANGELIZE)
	{
		base.actionIconString = GoapActionStateDB.Cult_Icon;
		base.logTags = new LOG_TAG[2]
		{
			LOG_TAG.Player,
			LOG_TAG.Crimes
		};
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		_ = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		if (!goapActionInvalidity.isInvalid && poiTarget.traitContainer.HasTrait("Berserked"))
		{
			goapActionInvalidity.isInvalid = true;
			goapActionInvalidity.reason = "target_unavailable";
		}
		return goapActionInvalidity;
	}

	public override bool ShouldActionBeAnIntel(ActualGoapNode node)
	{
		return true;
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Evangelize Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 0;
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		return REACTABLE_EFFECT.Negative;
	}

	public override string ReactionOfTarget(Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status)
	{
		if (target is Character character)
		{
			if (status == REACTION_STATUS.INFORMED)
			{
				bool flag = false;
				if (node.otherData != null)
				{
					for (int i = 0; i < node.otherData.Length; i++)
					{
						if (node.otherData[i] is StringOtherData { str: "True" })
						{
							flag = true;
						}
					}
				}
				if (!flag)
				{
					node.IncreaseReactionCounter();
					CrimeManager.Instance.ReactToCrime(character, actor, target, target.factionOwner, node.crimeType, node, status);
					node.DecreaseReactionCounter();
				}
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

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		if (witness.religionComponent.religion != actor.religionComponent.religion)
		{
			reactions.Add(EMOTION.Shock);
			if (CrimeManager.Instance.GetCrimeSeverity(witness, actor, target, node.crimeType).IsConsideredACrime())
			{
				reactions.Add(EMOTION.Disapproval);
			}
		}
		else
		{
			reactions.Add(EMOTION.Approval);
		}
	}

	public override void PopulateEmotionReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsOfTarget(reactions, actor, target, node, status);
		bool flag = false;
		if (node.otherData != null)
		{
			for (int i = 0; i < node.otherData.Length; i++)
			{
				if (node.otherData[i] is StringOtherData { str: "True" })
				{
					flag = true;
				}
			}
		}
		if (actor != target && !flag && target is Character character && character.religionComponent.religion != actor.religionComponent.religion)
		{
			reactions.Add(EMOTION.Disapproval);
		}
	}

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		string value = node.actor.religionComponent.religion.LocalizedName();
		log.AddToFillers(null, value, LOG_IDENTIFIER.STRING_1);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest target, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, target, otherData, job))
		{
			string cultistTraitNameForReligion = actor.religionComponent.religion.GetCultistTraitNameForReligion();
			if (target != actor)
			{
				return !target.traitContainer.HasTrait(cultistTraitNameForReligion);
			}
			return false;
		}
		return false;
	}

	public void PreEvangelizeSuccess(ActualGoapNode goapNode)
	{
		Character character = goapNode.poiTarget as Character;
		_ = character.religionComponent.religion;
		RELIGION religion = goapNode.actor.religionComponent.religion;
		WeightedDictionary<bool> weightedDictionary = new WeightedDictionary<bool>();
		int num = 50;
		int num2 = 50;
		int totalOpinion = character.relationshipContainer.GetTotalOpinion(goapNode.actor);
		if (totalOpinion >= 0)
		{
			num += totalOpinion;
		}
		else if (totalOpinion < 0)
		{
			num2 += Mathf.Abs(totalOpinion);
		}
		if (goapNode.actor.traitContainer.HasTrait("Persuasive"))
		{
			num += 200;
		}
		if (character.traitContainer.HasTrait("Treacherous"))
		{
			num += 100;
		}
		if (character.traitContainer.HasTrait("Betrayed"))
		{
			num += 100;
		}
		if (character.moodComponent.moodState == MOOD_STATE.Bad)
		{
			num += 100;
		}
		else if (character.moodComponent.moodState == MOOD_STATE.Critical)
		{
			num += 200;
		}
		if (character.traitContainer.HasTrait("Blessed"))
		{
			switch (religion)
			{
			case RELIGION.Demon_Worship:
				num2 += 100;
				break;
			case RELIGION.Divine_Worship:
				num += 300;
				break;
			}
		}
		if (character.characterClass.className == "Hero" && religion == RELIGION.Demon_Worship)
		{
			num2 += 500;
		}
		if (character.traitContainer.HasTrait("Evil"))
		{
			switch (religion)
			{
			case RELIGION.Demon_Worship:
				num += 100;
				break;
			case RELIGION.Divine_Worship:
				num2 += 100;
				break;
			case RELIGION.Nature_Worship:
				num2 += 100;
				break;
			}
		}
		if (character.traitContainer.HasTrait("Chaste"))
		{
			switch (religion)
			{
			case RELIGION.Demon_Worship:
				num2 += 100;
				break;
			case RELIGION.Nature_Worship:
				num += 300;
				break;
			}
		}
		if (character.traitContainer.HasTrait("Devout"))
		{
			num2 += 200;
		}
		if (character.isSettlementRuler)
		{
			num2 += 200;
		}
		if (character.isFactionLeader)
		{
			num2 += 500;
		}
		CRIME_TYPE crimeTypeByReligion = religion.GetCrimeTypeByReligion();
		CRIME_SEVERITY p_crimeSeverity = CRIME_SEVERITY.None;
		if (character.faction != null)
		{
			p_crimeSeverity = character.faction.GetCrimeSeverity(goapNode.actor, character, crimeTypeByReligion);
		}
		if (p_crimeSeverity.IsConsideredACrime())
		{
			num2 += 200;
		}
		weightedDictionary.AddElement(newElement: true, num);
		weightedDictionary.AddElement(newElement: false, num2);
		bool flag = weightedDictionary.PickRandomElementGivenWeights();
		goapNode.SetOtherData(new OtherData[1]
		{
			new StringOtherData(flag.ToString())
		});
	}

	public void AfterEvangelizeSuccess(ActualGoapNode goapNode)
	{
		Character character = goapNode.poiTarget as Character;
		bool flag = false;
		if (goapNode.otherData != null)
		{
			for (int i = 0; i < goapNode.otherData.Length; i++)
			{
				if (goapNode.otherData[i] is StringOtherData { str: "True" })
				{
					flag = true;
				}
			}
		}
		if (flag)
		{
			RELIGION religion = goapNode.actor.religionComponent.religion;
			RELIGION religion2 = character.religionComponent.religion;
			if (religion2 != religion)
			{
				character.religionComponent.DecreaseBeliefPoints(religion2, 10);
			}
			character.religionComponent.IncreaseBeliefPointsFromReligiousActions(religion, base.goapType, GameUtilities.RandomBetweenTwoNumbers(10, 30));
			RELIGION religion3 = character.religionComponent.religion;
			Log log = null;
			if (!character.traitContainer.IsReligiousCultist(religion))
			{
				log = ((religion3 != religion) ? GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", base.goapName + " success_no_convert", LOG_TAG.Life_Changes, goapNode) : ((religion3 != religion2) ? GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", base.goapName + " success_convert", LOG_TAG.Life_Changes, goapNode) : GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", base.goapName + " success_no_convert", LOG_TAG.Life_Changes, goapNode)));
			}
			else
			{
				log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", base.goapName + " success_cultist", LOG_TAG.Life_Changes, goapNode);
				log.AddToFillers(null, goapNode.actor.religionComponent.religion.GetCultistTraitNameForReligion(), LOG_IDENTIFIER.STRING_1);
			}
			log.AddToFillers(goapNode.actor, goapNode.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(character, character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			log.AddLogToDatabase(releaseLogAfter: true);
		}
		else if (CrimeManager.Instance.IsConsideredACrimeByCharacter(character, goapNode.actor, character, GetCrimeTypeBasedOnActorReligion(goapNode.actor)))
		{
			string value = goapNode.actor.religionComponent.religion.ToStringEnumWithSpace();
			if ((character.relationshipContainer.IsFamilyMember(goapNode.actor) || character.relationshipContainer.IsLoverOrAffair(goapNode.actor) || character.relationshipContainer.IsFriendsWith(goapNode.actor) || character.relationshipContainer.HasOpinionLabelWithCharacter(goapNode.actor, "Acquaintance")) && !character.relationshipContainer.IsEnemiesWith(goapNode.actor))
			{
				Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", base.goapName + " nothing_happens_crime", LOG_TAG.Crimes, LOG_TAG.Work, LOG_TAG.Social, goapNode);
				log2.AddToFillers(goapNode.actor, goapNode.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log2.AddToFillers(character, character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				log2.AddToFillers(null, value, LOG_IDENTIFIER.STRING_1);
				log2.AddLogToDatabase(releaseLogAfter: true);
			}
			else
			{
				Log log3 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", base.goapName + " crime", LOG_TAG.Crimes, LOG_TAG.Work, LOG_TAG.Social, goapNode);
				log3.AddToFillers(goapNode.actor, goapNode.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log3.AddToFillers(character, character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				log3.AddToFillers(null, value, LOG_IDENTIFIER.STRING_1);
				character.assumptionComponent.CreateAndReactToNewAssumption(goapNode.actor, goapNode.actor, INTERACTION_TYPE.IS_CULTIST, REACTION_STATUS.WITNESSED, isFabricated: false);
				log3.AddLogToDatabase(releaseLogAfter: true);
			}
		}
		else
		{
			Log log4 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", base.goapName + " nothing_happens", LOG_TAG.Crimes, LOG_TAG.Work, LOG_TAG.Social, goapNode);
			log4.AddToFillers(goapNode.actor, goapNode.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log4.AddToFillers(character, character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			log4.AddLogToDatabase(releaseLogAfter: true);
		}
	}

	public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime)
	{
		return GetCrimeTypeBasedOnActorReligion(actor);
	}

	public override CRIME_TYPE GetRawCrimeType(Character actor)
	{
		return GetCrimeTypeBasedOnActorReligion(actor);
	}

	private CRIME_TYPE GetCrimeTypeBasedOnActorReligion(Character p_actor)
	{
		return p_actor.religionComponent.religion.GetCrimeTypeByReligion();
	}
}

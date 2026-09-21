using System.Collections.Generic;
using UtilityScripts;

public class Pray : GoapAction
{
	public Pray()
		: base(INTERACTION_TYPE.PRAY)
	{
		base.goapName = "Pray";
		base.actionLocationType = ACTION_LOCATION_TYPE.NEARBY;
		base.actionIconString = GoapActionStateDB.Pray_Icon;
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
		SetState("Pray Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		int num = Utilities.Rng.Next(90, 131);
		if (actor.religionComponent.religion != RELIGION.Demon_Worship && actor.traitContainer.HasTrait("Evil", "Psychopath"))
		{
			num += 2000;
		}
		if (actor.traitContainer.HasTrait("Chaste"))
		{
			num -= 15;
		}
		if (actor.traitContainer.HasTrait("Devout"))
		{
			return num - 30;
		}
		int numOfTimesActionDone = actor.jobComponent.GetNumOfTimesActionDone(this);
		if (numOfTimesActionDone > 5)
		{
			num += 2000;
		}
		int num2 = 10 * numOfTimesActionDone;
		return num + num2;
	}

	public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime)
	{
		CRIME_TYPE crimeTypeByReligion = actor.religionComponent.religion.GetCrimeTypeByReligion();
		if (crimeTypeByReligion != CRIME_TYPE.None)
		{
			return crimeTypeByReligion;
		}
		return base.GetCrimeType(actor, target, crime);
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		if (((node.crimeType == CRIME_TYPE.None) ? CRIME_SEVERITY.None : CrimeManager.Instance.GetCrimeSeverity(witness, actor, target, node.crimeType)).IsConsideredACrime())
		{
			reactions.Add(EMOTION.Repulsed);
			if (witness.characterClass.className != "Stalker")
			{
				string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
				if (opinionLabel == "Close Friend")
				{
					reactions.Add(EMOTION.Despair);
				}
				else if (opinionLabel == "Friend")
				{
					reactions.Add(EMOTION.Shock);
				}
			}
			if (witness.traitContainer.HasTrait("Coward"))
			{
				reactions.Add(EMOTION.Fear);
			}
			else if (!witness.traitContainer.HasTrait("Psychopath"))
			{
				reactions.Add(EMOTION.Threatened);
			}
		}
		else if (witness.religionComponent.religion == actor.religionComponent.religion)
		{
			reactions.Add(EMOTION.Approval);
		}
	}

	public override bool IsHappinessRecoveryAction()
	{
		return true;
	}

	public void PrePraySuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.jobComponent.IncreaseNumOfTimesActionDone(base.goapType);
	}

	public void PerTickPraySuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.needsComponent.AdjustHappiness(12f);
	}

	public void AfterPraySuccess(ActualGoapNode goapNode)
	{
		int num = GameUtilities.RandomBetweenTwoNumbers(2, 5);
		if (goapNode.actor.traitContainer.HasTrait("Devout"))
		{
			num *= 3;
		}
		goapNode.actor.religionComponent.IncreaseBeliefPointsFromReligiousActions(goapNode.actor.religionComponent.religion, base.goapType, num);
		if (goapNode.actor.religionComponent.religion == RELIGION.Demon_Worship)
		{
			Messenger.Broadcast(CharacterSignals.CHARACTER_PRAY_SUCCESS, goapNode.poiTarget as Character);
		}
		if (PlayerSkillManager.Instance.selectedArchetype.IsRavagerLoadout() && goapNode.actor.traitContainer.HasTrait("Demon Cultist") && GameUtilities.RollChance(10))
		{
			COMBAT_SPECIAL_SKILL_CATEGORY p_category = COMBAT_SPECIAL_SKILL_CATEGORY.Physical;
			if (goapNode.actor.characterClass.attackType == ATTACK_TYPE.MAGICAL)
			{
				p_category = COMBAT_SPECIAL_SKILL_CATEGORY.Magical;
			}
			List<COMBAT_SPECIAL_SKILL> list = RuinarchListPool<COMBAT_SPECIAL_SKILL>.Claim();
			int p_currentSkillTier = (goapNode.actor.combatComponent.specialSkillParent.HasSpecialSkill() ? goapNode.actor.combatComponent.specialSkillParent.specialSkill.tier : 0);
			CombatManager.Instance.PopulateHigherTierCombatSkillsThatCanBeLearnedByCharacter(goapNode.actor, p_currentSkillTier, p_category, list);
			if (list.Count > 0)
			{
				COMBAT_SPECIAL_SKILL randomElement = CollectionUtilities.GetRandomElement(list);
				goapNode.actor.combatComponent.specialSkillParent.SetSpecialSkill(randomElement);
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", "learn_new_skill", LOG_TAG.Life_Changes);
				log.AddToFillers(goapNode.actor, goapNode.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log.AddToFillers(null, goapNode.actor.combatComponent.specialSkillParent.specialSkill.name, LOG_IDENTIFIER.STRING_1);
				log.AddLogToDatabase();
			}
			RuinarchListPool<COMBAT_SPECIAL_SKILL>.Release(list);
		}
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
			if (actor.traitContainer.HasTrait("Evil"))
			{
				return false;
			}
			return actor == poiTarget;
		}
		return false;
	}
}

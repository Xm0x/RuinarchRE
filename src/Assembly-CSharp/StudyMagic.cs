using System.Collections.Generic;
using UtilityScripts;

public class StudyMagic : GoapAction
{
	public StudyMagic()
		: base(INTERACTION_TYPE.STUDY_MAGIC)
	{
		base.actionIconString = GoapActionStateDB.Read_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
		base.showNotification = true;
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.TRAIN_TALENT, "Magic", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Study Magic Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return Utilities.Rng.Next(1, 50);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (actor.characterClass.attackType != ATTACK_TYPE.MAGICAL)
			{
				return false;
			}
			if (poiTarget is TileObject { mapObjectState: not MAP_OBJECT_STATE.BUILT })
			{
				return false;
			}
			return poiTarget.gridTileLocation != null;
		}
		return false;
	}

	public void PerTickStudyMagicSuccess(ActualGoapNode goapNode)
	{
		if (GameUtilities.RollChance(50) && goapNode.actor.TryGetTalentLevel(CHARACTER_TALENT.Combat_Magic) < 5)
		{
			goapNode.actor.talentComponent.GetTalent(CHARACTER_TALENT.Combat_Magic).AdjustExperience(GameUtilities.RandomBetweenTwoNumbers(2, 5), goapNode.actor);
		}
		else
		{
			goapNode.actor.talentComponent.GetTalent(CHARACTER_TALENT.Healing_Magic).AdjustExperience(GameUtilities.RandomBetweenTwoNumbers(2, 5), goapNode.actor);
		}
	}

	public void AfterStudyMagicSuccess(ActualGoapNode goapNode)
	{
		if (ChanceData.RollChance(CHANCE_TYPE.Learn_New_Combat_Skill_Magic_Acad))
		{
			List<COMBAT_SPECIAL_SKILL> list = RuinarchListPool<COMBAT_SPECIAL_SKILL>.Claim();
			int p_currentSkillTier = (goapNode.actor.combatComponent.specialSkillParent.HasSpecialSkill() ? goapNode.actor.combatComponent.specialSkillParent.specialSkill.tier : 0);
			CombatManager.Instance.PopulateHigherTierCombatSkillsThatCanBeLearnedByCharacter(goapNode.actor, p_currentSkillTier, COMBAT_SPECIAL_SKILL_CATEGORY.Magical, list);
			CombatManager.Instance.PopulateHigherTierCombatSkillsThatCanBeLearnedByCharacter(goapNode.actor, p_currentSkillTier, COMBAT_SPECIAL_SKILL_CATEGORY.Healing, list);
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
}

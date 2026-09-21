using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;

public class TrainCombatMagic : GoapAction
{
	public TrainCombatMagic()
		: base(INTERACTION_TYPE.TRAIN_COMBAT_MAGIC)
	{
		base.actionIconString = GoapActionStateDB.Train_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.OVERRIDE;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
		base.showNotification = true;
		base.canBeAdvertisedEvenIfTargetIsUnavailable = true;
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.TRAIN_TALENT, "Magic", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		if (!goapActionInvalidity.isInvalid && node.poiTarget is TrainingDummy { currentUser: not null } trainingDummy && trainingDummy.currentUser != node.actor)
		{
			goapActionInvalidity.isInvalid = true;
			goapActionInvalidity.reason = "dummy_being_used";
		}
		return goapActionInvalidity;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Train Combat Magic Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return Utilities.Rng.Next(1, 50);
	}

	public override LocationGridTile GetOverrideTargetTile(ActualGoapNode goapNode)
	{
		if (goapNode.poiTarget.gridTileLocation != null)
		{
			Vector3Int localPlace = goapNode.poiTarget.gridTileLocation.localPlace;
			if (localPlace.x + 3 < goapNode.poiTarget.gridTileLocation.parentMap.width)
			{
				Vector3Int p_vector = localPlace;
				p_vector.x += 3;
				return goapNode.poiTarget.gridTileLocation.parentMap.GetTileFromMapCoordinates(p_vector);
			}
			if (localPlace.x - 3 > 0)
			{
				Vector3Int p_vector2 = localPlace;
				p_vector2.x -= 3;
				return goapNode.poiTarget.gridTileLocation.parentMap.GetTileFromMapCoordinates(p_vector2);
			}
			if (localPlace.y - 3 > 0)
			{
				Vector3Int p_vector3 = localPlace;
				p_vector3.y -= 3;
				return goapNode.poiTarget.gridTileLocation.parentMap.GetTileFromMapCoordinates(p_vector3);
			}
			if (localPlace.y + 3 < goapNode.poiTarget.gridTileLocation.parentMap.height)
			{
				Vector3Int p_vector4 = localPlace;
				p_vector4.y += 3;
				return goapNode.poiTarget.gridTileLocation.parentMap.GetTileFromMapCoordinates(p_vector4);
			}
		}
		return null;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (actor.characterClass.attackType != ATTACK_TYPE.MAGICAL)
			{
				return false;
			}
			if (poiTarget is TrainingDummy trainingDummy)
			{
				if (trainingDummy.currentUser != null)
				{
					return false;
				}
				if (trainingDummy.mapObjectState != MAP_OBJECT_STATE.BUILT)
				{
					return false;
				}
			}
			if (poiTarget.gridTileLocation != null)
			{
				return actor.TryGetTalentLevel(CHARACTER_TALENT.Combat_Magic) < 5;
			}
			return false;
		}
		return false;
	}

	public void PerTickTrainCombatMagicSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.actor.hasMarker)
		{
			goapNode.actor.marker.DoDummyAttack(goapNode.poiTarget);
			goapNode.actor.talentComponent.GetTalent(CHARACTER_TALENT.Combat_Magic).AdjustExperience(GameUtilities.RandomBetweenTwoNumbers(1, 2), goapNode.actor);
		}
	}

	public void AfterTrainCombatMagicSuccess(ActualGoapNode goapNode)
	{
		if (ChanceData.RollChance(CHANCE_TYPE.Learn_New_Combat_Skill_Magic_Acad))
		{
			List<COMBAT_SPECIAL_SKILL> list = RuinarchListPool<COMBAT_SPECIAL_SKILL>.Claim();
			int p_currentSkillTier = (goapNode.actor.combatComponent.specialSkillParent.HasSpecialSkill() ? goapNode.actor.combatComponent.specialSkillParent.specialSkill.tier : 0);
			CombatManager.Instance.PopulateHigherTierCombatSkillsThatCanBeLearnedByCharacter(goapNode.actor, p_currentSkillTier, COMBAT_SPECIAL_SKILL_CATEGORY.Magical, list);
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

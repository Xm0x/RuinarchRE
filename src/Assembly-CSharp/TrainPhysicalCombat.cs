using System;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;

public class TrainPhysicalCombat : GoapAction
{
	public TrainPhysicalCombat()
		: base(INTERACTION_TYPE.TRAIN_PHYSICAL_COMBAT)
	{
		base.actionIconString = GoapActionStateDB.Train_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.OVERRIDE;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
		base.showNotification = true;
		base.canBeAdvertisedEvenIfTargetIsUnavailable = true;
		base.animationName = "Idle";
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.TRAIN_TALENT, "Martial Arts", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		if (!goapActionInvalidity.isInvalid)
		{
			if (node.poiTarget is TrainingDummy { currentUser: not null } trainingDummy && trainingDummy.currentUser != node.actor)
			{
				goapActionInvalidity.isInvalid = true;
				goapActionInvalidity.reason = "dummy_being_used";
			}
			else if (node.poiTarget is ArcheryTarget { currentUser: not null } archeryTarget && archeryTarget.currentUser != node.actor)
			{
				goapActionInvalidity.isInvalid = true;
				goapActionInvalidity.reason = "dummy_being_used";
			}
		}
		return goapActionInvalidity;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Train Physical Combat Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return Utilities.Rng.Next(1, 50);
	}

	public override LocationGridTile GetOverrideTargetTile(ActualGoapNode goapNode)
	{
		int num = 0;
		num = ((!(goapNode.poiTarget is ArcheryTarget)) ? 1 : 2);
		LocationGridTile targetTile = null;
		if (goapNode.poiTarget.gridTileLocation != null && goapNode.poiTarget.mapObjectVisual != null)
		{
			Vector3Int localPlace = goapNode.poiTarget.gridTileLocation.localPlace;
			float z = goapNode.poiTarget.mapObjectVisual.rotation.eulerAngles.z;
			if (Mathf.Approximately(z, 0f))
			{
				if (TryGetTileLocation(localPlace, num, Cardinal_Direction.South, goapNode, out targetTile))
				{
					return targetTile;
				}
			}
			else if (Mathf.Approximately(z, 90f) || Mathf.Approximately(z, -270f))
			{
				if (TryGetTileLocation(localPlace, num, Cardinal_Direction.East, goapNode, out targetTile))
				{
					return targetTile;
				}
			}
			else if (Mathf.Approximately(z, 180f) || Mathf.Approximately(z, -180f))
			{
				if (TryGetTileLocation(localPlace, num, Cardinal_Direction.North, goapNode, out targetTile))
				{
					return targetTile;
				}
			}
			else if ((Mathf.Approximately(z, 270f) || Mathf.Approximately(z, -90f)) && TryGetTileLocation(localPlace, num, Cardinal_Direction.West, goapNode, out targetTile))
			{
				return targetTile;
			}
			if (TryGetTileLocation(localPlace, num, Cardinal_Direction.East, goapNode, out targetTile))
			{
				return targetTile;
			}
			if (TryGetTileLocation(localPlace, num, Cardinal_Direction.West, goapNode, out targetTile))
			{
				return targetTile;
			}
			if (TryGetTileLocation(localPlace, num, Cardinal_Direction.South, goapNode, out targetTile))
			{
				return targetTile;
			}
			TryGetTileLocation(localPlace, num, Cardinal_Direction.North, goapNode, out targetTile);
			return targetTile;
		}
		return targetTile;
	}

	private bool TryGetTileLocation(Vector3Int dummyLocation, int distanceFromTarget, Cardinal_Direction direction, ActualGoapNode goapNode, out LocationGridTile targetTile)
	{
		switch (direction)
		{
		case Cardinal_Direction.North:
			if (dummyLocation.y + distanceFromTarget < goapNode.poiTarget.gridTileLocation.parentMap.height)
			{
				Vector3Int p_vector3 = dummyLocation;
				p_vector3.y += distanceFromTarget;
				targetTile = goapNode.poiTarget.gridTileLocation.parentMap.GetTileFromMapCoordinates(p_vector3);
				return true;
			}
			targetTile = null;
			return false;
		case Cardinal_Direction.South:
			if (dummyLocation.y - distanceFromTarget > 0)
			{
				Vector3Int p_vector2 = dummyLocation;
				p_vector2.y -= distanceFromTarget;
				targetTile = goapNode.poiTarget.gridTileLocation.parentMap.GetTileFromMapCoordinates(p_vector2);
				return true;
			}
			targetTile = null;
			return false;
		case Cardinal_Direction.East:
			if (dummyLocation.x + distanceFromTarget < goapNode.poiTarget.gridTileLocation.parentMap.width)
			{
				Vector3Int p_vector4 = dummyLocation;
				p_vector4.x += distanceFromTarget;
				targetTile = goapNode.poiTarget.gridTileLocation.parentMap.GetTileFromMapCoordinates(p_vector4);
				return true;
			}
			targetTile = null;
			return false;
		case Cardinal_Direction.West:
			if (dummyLocation.x - distanceFromTarget > 0)
			{
				Vector3Int p_vector = dummyLocation;
				p_vector.x -= distanceFromTarget;
				targetTile = goapNode.poiTarget.gridTileLocation.parentMap.GetTileFromMapCoordinates(p_vector);
				return true;
			}
			targetTile = null;
			return false;
		default:
			throw new ArgumentOutOfRangeException("direction", direction, null);
		}
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (actor.characterClass.attackType != ATTACK_TYPE.PHYSICAL)
			{
				return false;
			}
			if (poiTarget is ArcheryTarget archeryTarget)
			{
				if (actor.combatComponent.rangeType == RANGE_TYPE.MELEE)
				{
					return false;
				}
				if (archeryTarget.currentUser != null)
				{
					return false;
				}
				if (archeryTarget.mapObjectState != MAP_OBJECT_STATE.BUILT)
				{
					return false;
				}
			}
			else if (poiTarget is TrainingDummy trainingDummy)
			{
				if (actor.combatComponent.rangeType == RANGE_TYPE.RANGED)
				{
					return false;
				}
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
				return actor.TryGetTalentLevel(CHARACTER_TALENT.Martial_Arts) < 5;
			}
			return false;
		}
		return false;
	}

	public void PerTickTrainPhysicalCombatSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.actor.hasMarker)
		{
			goapNode.actor.marker.DoDummyAttack(goapNode.poiTarget);
			goapNode.actor.talentComponent.GetTalent(CHARACTER_TALENT.Martial_Arts).AdjustExperience(GameUtilities.RandomBetweenTwoNumbers(1, 2), goapNode.actor);
		}
	}

	public void AfterTrainPhysicalCombatSuccess(ActualGoapNode goapNode)
	{
		if (ChanceData.RollChance(CHANCE_TYPE.Learn_New_Combat_Skill_Barracks))
		{
			List<COMBAT_SPECIAL_SKILL> list = RuinarchListPool<COMBAT_SPECIAL_SKILL>.Claim();
			int p_currentSkillTier = (goapNode.actor.combatComponent.specialSkillParent.HasSpecialSkill() ? goapNode.actor.combatComponent.specialSkillParent.specialSkill.tier : 0);
			CombatManager.Instance.PopulateHigherTierCombatSkillsThatCanBeLearnedByCharacter(goapNode.actor, p_currentSkillTier, COMBAT_SPECIAL_SKILL_CATEGORY.Physical, list);
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

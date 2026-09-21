using System.Collections.Generic;
using Inner_Maps;
using Object_Pools;
using Traits;
using UnityEngine;
using UtilityScripts;

public class DrinkBlood : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.CONSUME;

	public DrinkBlood()
		: base(INTERACTION_TYPE.DRINK_BLOOD)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		base.actionIconString = GoapActionStateDB.Drink_Blood_Icon;
		base.doesNotStopTargetCharacter = true;
		base.logTags = new LOG_TAG[2]
		{
			LOG_TAG.Crimes,
			LOG_TAG.Needs
		};
		validTimeOfDays = new TIME_IN_WORDS[1];
	}

	public override bool ShouldActionBeAnIntel(ActualGoapNode node)
	{
		return true;
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		SetPrecondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Unconscious", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET), HasUnconsciousOrRestingTarget);
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Lethargic", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
		AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, GOAP_EFFECT_TARGET.ACTOR));
	}

	protected override List<GoapEffect> GetExpectedEffects(Character actor, IPointOfInterest target, OtherData[] otherData, out bool isOverridden)
	{
		if (actor.traitContainer.HasTrait("Vampire"))
		{
			List<GoapEffect> list = RuinarchListPool<GoapEffect>.Claim(4);
			AddBaseExpectedEffectsToList(list);
			list.Add(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
			isOverridden = true;
			return list;
		}
		return base.GetExpectedEffects(actor, target, otherData, out isOverridden);
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Drink Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		int num = 0;
		if (job.jobType == JOB_TYPE.TRIGGER_FLAW)
		{
			if (actor.traitContainer.HasTrait("Cannibal"))
			{
				return Utilities.Rng.Next(450, 551);
			}
			if (!target.traitContainer.HasTrait("Vampire"))
			{
				return Utilities.Rng.Next(450, 551);
			}
			return 2000;
		}
		if (actor.traitContainer.HasTrait("Enslaved") && (target.gridTileLocation == null || !target.gridTileLocation.IsInHomeOf(actor)))
		{
			return 2000;
		}
		if (actor.partyComponent.hasParty && actor.partyComponent.currentParty.isActive && actor.partyComponent.isActiveMember)
		{
			if (!(target is Animal))
			{
				if (actor.partyComponent.currentParty.partyFaction.GetCrimeSeverity(actor, actor, CRIME_TYPE.Vampire).IsConsideredACrime())
				{
					return 2000;
				}
			}
			else if (!actor.needsComponent.isStarving)
			{
				return 2000;
			}
			if (target.gridTileLocation != null && actor.gridTileLocation != null)
			{
				LocationGridTile centerGridTile = target.gridTileLocation.area.gridTileComponent.centerGridTile;
				float distanceTo = actor.gridTileLocation.area.gridTileComponent.centerGridTile.GetDistanceTo(centerGridTile);
				int num2 = InnerMapManager.AreaLocationGridTileSize.x * 3;
				if (distanceTo > (float)num2)
				{
					return 2000;
				}
			}
		}
		if (target is Character character)
		{
			if (character.traitContainer.HasTrait("Vampire") && !actor.traitContainer.HasTrait("Cannibal"))
			{
				return num + 2000;
			}
			if (!character.traitContainer.HasTrait("Vampire") && actor.traitContainer.HasTrait("Cannibal"))
			{
				return num + 2000;
			}
			if (!actor.isVagrant)
			{
				AWARENESS_STATE awarenessState = actor.relationshipContainer.GetAwarenessState(actor, character);
				if (actor.currentRegion != character.currentRegion || awarenessState == AWARENESS_STATE.Missing || awarenessState == AWARENESS_STATE.Presumed_Dead || character.partyComponent.isMemberThatJoinedQuest)
				{
					return num + 2000;
				}
			}
			if (character.limiterComponent.canPerform && character.limiterComponent.canMove)
			{
				num += 80;
			}
			if (!character.race.IsSapient())
			{
				num += 200;
			}
			if (actor.needsComponent.isHungry || (!actor.needsComponent.isHungry && !actor.needsComponent.isStarving))
			{
				string opinionLabel = actor.relationshipContainer.GetOpinionLabel(character);
				if (opinionLabel == "Friend" || opinionLabel == "Close Friend")
				{
					return num + 2000;
				}
				num = ((actor.homeStructure != null && character.currentStructure == actor.homeStructure && character.traitContainer.HasTrait("Prisoner")) ? num : (opinionLabel switch
				{
					"Rival" => num + 20, 
					"Enemy" => num + 40, 
					"Acquaintance" => num + 75, 
					_ => num + 40, 
				}));
			}
			else if (actor.needsComponent.isStarving)
			{
				string opinionLabel2 = actor.relationshipContainer.GetOpinionLabel(character);
				num = ((actor.homeStructure != null && character.currentStructure == actor.homeStructure && character.traitContainer.HasTrait("Prisoner")) ? num : (opinionLabel2 switch
				{
					"Close Friend" => num + 400, 
					"Friend" => num + 300, 
					"Rival" => num + 20, 
					"Enemy" => num + 40, 
					"Acquaintance" => num + 75, 
					_ => num + 40, 
				}));
			}
		}
		return num;
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		_ = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		if (!goapActionInvalidity.isInvalid)
		{
			Character character = poiTarget as Character;
			if (character.limiterComponent.canMove && character.limiterComponent.canPerform)
			{
				goapActionInvalidity.isInvalid = true;
				goapActionInvalidity.stateName = "Drink Fail";
			}
		}
		return goapActionInvalidity;
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		if (CrimeManager.Instance.GetCrimeSeverity(witness, actor, target, CRIME_TYPE.Vampire).IsConsideredACrime())
		{
			if (witness.characterClass.className == "Stalker")
			{
				reactions.Add(EMOTION.Disapproval);
				return;
			}
			if (witness.traitContainer.IsReligiousCultist(RELIGION.Demon_Worship) && actor.traitContainer.IsReligiousCultist(RELIGION.Demon_Worship))
			{
				reactions.Add(EMOTION.Approval);
				if (RelationshipManager.IsSexuallyCompatibleOneSided(witness, actor) && GameUtilities.RollChance(10 * witness.relationshipContainer.GetCompatibility(actor)))
				{
					reactions.Add(EMOTION.Arousal);
				}
				return;
			}
			if (witness.traitContainer.HasTrait("Coward", "Hemophobic"))
			{
				reactions.Add(EMOTION.Fear);
			}
			else if (!witness.traitContainer.HasTrait("Psychopath"))
			{
				reactions.Add(EMOTION.Threatened);
				if (witness.relationshipContainer.GetOpinionLabel(actor) == "Close Friend")
				{
					reactions.Add(EMOTION.Despair);
				}
				else
				{
					reactions.Add(EMOTION.Shock);
				}
			}
			if (target is Character character)
			{
				if (witness.relationshipContainer.IsFriendsWith(character) || witness.relationshipContainer.IsFamilyMemberOrLoverOrAffairAndNotRival(character))
				{
					reactions.Add(EMOTION.Anger);
				}
				else if (!witness.relationshipContainer.IsEnemiesWith(character) && (witness.relationshipContainer.GetOpinionLabel(character) == "Acquaintance" || (witness.faction != null && witness.faction == character.faction) || (witness.homeSettlement != null && witness.homeSettlement == character.homeSettlement)))
				{
					reactions.Add(EMOTION.Anger);
				}
			}
		}
		else if (witness.traitContainer.HasTrait("Hemophiliac"))
		{
			if (RelationshipManager.IsSexuallyCompatibleOneSided(witness, actor))
			{
				reactions.Add(EMOTION.Arousal);
			}
			else
			{
				reactions.Add(EMOTION.Approval);
			}
		}
		else if (witness.traitContainer.HasTrait("Hemophobic"))
		{
			reactions.Add(EMOTION.Threatened);
		}
	}

	public override void PopulateEmotionReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsOfTarget(reactions, actor, target, node, status);
		if (!(target is Character character))
		{
			return;
		}
		if (CrimeManager.Instance.GetCrimeSeverity(character, actor, target, CRIME_TYPE.Vampire).IsConsideredACrime())
		{
			reactions.Add(EMOTION.Shock);
			if (character.traitContainer.HasTrait("Coward", "Hemophobic"))
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
		else if (character.traitContainer.HasTrait("Hemophiliac"))
		{
			if (RelationshipManager.IsSexuallyCompatible(actor, character))
			{
				reactions.Add(EMOTION.Arousal);
			}
			else
			{
				reactions.Add(EMOTION.Approval);
			}
		}
		else if (character.traitContainer.HasTrait("Hemophobic"))
		{
			reactions.Add(EMOTION.Threatened);
		}
	}

	public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		actor.traitContainer.GetTraitOrStatus<Vampire>("Vampire")?.AddAwareCharacter(witness);
		return base.ReactionToActor(actor, target, witness, node, status);
	}

	public override string ReactionOfTarget(Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status)
	{
		if (target is Character character)
		{
			actor.traitContainer.GetTraitOrStatus<Vampire>("Vampire")?.AddAwareCharacter(character);
		}
		return base.ReactionOfTarget(actor, target, node, status);
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		return REACTABLE_EFFECT.Negative;
	}

	public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime)
	{
		return CRIME_TYPE.Vampire;
	}

	public override CRIME_TYPE GetRawCrimeType(Character actor)
	{
		return CRIME_TYPE.Vampire;
	}

	public override bool IsHappinessRecoveryAction()
	{
		return true;
	}

	public override bool IsFullnessRecoveryAction()
	{
		return true;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (poiTarget is Character character)
			{
				if (actor != character && actor.traitContainer.HasTrait("Vampire") && !character.isDead)
				{
					return character.carryComponent.IsNotBeingCarried();
				}
				return false;
			}
			if (actor != poiTarget)
			{
				return actor.traitContainer.HasTrait("Vampire");
			}
			return false;
		}
		return false;
	}

	private bool HasUnconsciousOrRestingTarget(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType)
	{
		return (poiTarget as Character).traitContainer.HasTrait("Unconscious", "Resting");
	}

	public void PerTickDrinkSuccess(ActualGoapNode goapNode)
	{
		Character actor = goapNode.actor;
		if (actor.needsComponent.HasNeeds())
		{
			actor.needsComponent.AdjustFullness(20f, 0.25f);
			actor.needsComponent.AdjustHappiness(20f);
		}
		if (!actor.IsHealthFull())
		{
			int num = Mathf.CeilToInt((float)actor.maxHP * 0.05f);
			if (num > 0)
			{
				actor.AdjustHP(num, ELEMENTAL_TYPE.Normal);
			}
		}
	}

	public void AfterDrinkSuccess(ActualGoapNode goapNode)
	{
		Character actor = goapNode.actor;
		if (!(goapNode.poiTarget is Character character))
		{
			return;
		}
		if (character.HasItem("Phylactery"))
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", base.goapName + " activate_phylactery", LOG_TAG.Social, goapNode);
			log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(character, character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			log.AddLogToDatabase();
			character.UnobtainItem("Phylactery");
			actor.AdjustHP(-500, ELEMENTAL_TYPE.Normal);
			if (!actor.HasHealth())
			{
				actor.Death("normal", goapNode, character, log, null, null, null, isPlayerSource: false, character);
			}
			else
			{
				actor.traitContainer.AddTrait(actor, "Unconscious", character);
				actor.traitContainer.GetTraitOrStatus<Trait>("Unconscious")?.SetGainedFromDoingAction(goapNode.action.goapType, goapNode.isStealth);
			}
			LogPool.Release(log);
			return;
		}
		if (actor.currentSettlement is NPCSettlement nPCSettlement && nPCSettlement.eventManager.CanHaveEvents() && nPCSettlement.owner != null && ChanceData.RollChance(CHANCE_TYPE.Vampire_Hunt_Drink_Blood_Chance) && nPCSettlement.owner.GetCrimeSeverity(actor, goapNode.poiTarget, CRIME_TYPE.Vampire).IsConsideredACrime() && !nPCSettlement.eventManager.HasActiveEvent(SETTLEMENT_EVENT.Vampire_Hunt))
		{
			nPCSettlement.eventManager.AddNewActiveEvent(SETTLEMENT_EVENT.Vampire_Hunt);
		}
		if (!character.race.IsSapient() || (character.traitContainer.HasTrait("Vampire") && !actor.traitContainer.HasTrait("Cannibal")))
		{
			actor.traitContainer.AddTrait(actor, "Poor Meal", character);
		}
		if (GameUtilities.RollChance(2))
		{
			if (!character.classComponent.IsStalkerCannotBeTurned() && character.traitContainer.AddTrait(character, "Vampire", actor))
			{
				Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", base.goapName + " contracted", LOG_TAG.Life_Changes, goapNode);
				log2.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				log2.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log2.AddLogToDatabase();
				PlayerManager.Instance.player.ShowNotificationFrom(actor, log2, releaseLogAfter: true);
			}
			if (character.isNormalCharacter)
			{
				actor.traitContainer.GetTraitOrStatus<Vampire>("Vampire")?.AdjustNumOfConvertedVillagers(1);
			}
		}
	}
}

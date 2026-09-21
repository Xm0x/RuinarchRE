using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;

public class Butcher : GoapAction
{
	public int m_amountProducedPerTick = 4;

	private const float _coinGainMultiplier = 0.344f;

	public Butcher()
		: base(INTERACTION_TYPE.BUTCHER)
	{
		base.actionIconString = GoapActionStateDB.Butcher_Icon;
		base.canBeAdvertisedEvenIfTargetIsUnavailable = true;
		base.logTags = new LOG_TAG[2]
		{
			LOG_TAG.Work,
			LOG_TAG.Needs
		};
		base.shouldAddLogs = false;
	}

	public override bool ShouldActionBeAnIntel(ActualGoapNode node)
	{
		return true;
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		SetPrecondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.DEATH, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET), IsTargetDead);
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.PRODUCE_FOOD, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Transform Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		if (actor.movementComponent.ShouldAvoidStructureLocationOfTarget(target) && !actor.partyComponent.hasParty)
		{
			return 2000;
		}
		if (job.jobType == JOB_TYPE.PRODUCE_FOOD_FOR_CAMP)
		{
			if (target.gridTileLocation != null && actor.gridTileLocation != null)
			{
				LocationGridTile centerGridTile = target.gridTileLocation.area.gridTileComponent.centerGridTile;
				float distanceTo = actor.gridTileLocation.area.gridTileComponent.centerGridTile.GetDistanceTo(centerGridTile);
				int num = InnerMapManager.AreaLocationGridTileSize.x * 3;
				if (distanceTo > (float)num)
				{
					return 2000;
				}
			}
			if (target is Character character && actor.partyComponent.hasParty && actor.partyComponent.currentParty.IsMember(character))
			{
				return 2000;
			}
		}
		Character deadCharacter = GetDeadCharacter(target);
		int num2 = 0;
		if (job.jobType == JOB_TYPE.MONSTER_BUTCHER)
		{
			return num2 + 10;
		}
		if (deadCharacter is Animal)
		{
			return num2 + 10;
		}
		bool flag = actor.traitContainer.HasTrait("Cannibal");
		if (job.jobType == JOB_TYPE.TRIGGER_FLAW && flag && !actor.traitContainer.HasTrait("Vampire"))
		{
			return Utilities.Rng.Next(450, 551);
		}
		if (deadCharacter != null)
		{
			if (actor == deadCharacter)
			{
				num2 += 2000;
			}
			else
			{
				if (actor.traitContainer.HasTrait("Enslaved"))
				{
					if (job.jobType == JOB_TYPE.PRODUCE_FOOD && job.originalOwner.ownerType == JOB_OWNER.SETTLEMENT && deadCharacter.faction == actor.faction)
					{
						return num2 + 2000;
					}
					if (deadCharacter.faction == actor.faction)
					{
						return num2 + 2000;
					}
				}
				if (!actor.isNormalCharacter)
				{
					num2 += 10;
				}
				else if (flag && !actor.traitContainer.HasTrait("Vampire"))
				{
					if (actor.traitContainer.HasTrait("Malnourished"))
					{
						if (actor.relationshipContainer.IsFriendsWith(deadCharacter))
						{
							int num3 = Utilities.Rng.Next(100, 151);
							num2 += num3;
						}
						if (deadCharacter.race.IsSapient() || deadCharacter.IsRatmanThatIsPartOfMajorFaction())
						{
							num2 += 100;
						}
					}
					else
					{
						if (actor.relationshipContainer.IsFriendsWith(deadCharacter))
						{
							num2 += 2000;
						}
						if ((deadCharacter.race.IsSapient() || deadCharacter.IsRatmanThatIsPartOfMajorFaction()) && !actor.needsComponent.isStarving)
						{
							num2 += 200;
						}
					}
				}
				else if (actor.traitContainer.HasTrait("Malnourished"))
				{
					if (actor.relationshipContainer.IsFriendsWith(deadCharacter))
					{
						int num4 = Utilities.Rng.Next(100, 151);
						num2 += num4;
					}
					if (deadCharacter.race.IsSapient() || deadCharacter.IsRatmanThatIsPartOfMajorFaction())
					{
						num2 += 200;
					}
				}
				else if (deadCharacter.race.IsSapient() || deadCharacter.IsRatmanThatIsPartOfMajorFaction())
				{
					num2 += 2000;
				}
			}
			if (deadCharacter.race.IsSapient())
			{
				int num5 = Utilities.Rng.Next(80, 91);
				num2 += num5;
			}
			else if (deadCharacter.race == RACE.RATMAN)
			{
				int num6 = Utilities.Rng.Next(80, 91);
				num2 += num6;
			}
			else if (deadCharacter.race == RACE.DEMON || deadCharacter.race == RACE.LESSER_DEMON)
			{
				int num7 = Utilities.Rng.Next(90, 111);
				num2 += num7;
			}
			if (actor.race == RACE.ELVES && (deadCharacter.race == RACE.RATMAN || deadCharacter.race == RACE.RAT))
			{
				num2 += 150;
			}
			if (deadCharacter is Animal || deadCharacter.race == RACE.WOLF || deadCharacter.race == RACE.SPIDER)
			{
				if (!actor.characterClass.IsCombatant() && !deadCharacter.isDead && (deadCharacter.race == RACE.WOLF || deadCharacter.race == RACE.SPIDER))
				{
					num2 += 2000;
				}
				CRIME_SEVERITY crimeSeverity = CrimeManager.Instance.GetCrimeSeverity(actor, actor, deadCharacter, CRIME_TYPE.Animal_Killing);
				int num8 = 0;
				switch (crimeSeverity)
				{
				case CRIME_SEVERITY.Infraction:
					num8 += Utilities.Rng.Next(80, 91);
					break;
				case CRIME_SEVERITY.Misdemeanor:
				case CRIME_SEVERITY.Serious:
				case CRIME_SEVERITY.Heinous:
					if (actor.traitContainer.HasTrait("Malnourished"))
					{
						if (actor.relationshipContainer.IsFriendsWith(deadCharacter))
						{
							num8 += 200;
						}
						num8 += Utilities.Rng.Next(100, 111);
					}
					else
					{
						num8 += 2000;
					}
					break;
				default:
					num8 += Utilities.Rng.Next(40, 51);
					break;
				}
				num2 += num8;
			}
			if (!deadCharacter.isDead)
			{
				num2 *= 2;
			}
		}
		return num2;
	}

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		IPointOfInterest pointOfInterest = node.poiTarget;
		if (node.poiTarget is Tombstone tombstone)
		{
			pointOfInterest = tombstone.character;
		}
		log.AddToFillers(pointOfInterest, pointOfInterest.name, LOG_IDENTIFIER.TARGET_CHARACTER);
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		Character actor = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		string stateName = "Target Missing";
		bool isInvalid = IsTargetMissing(actor, poiTarget);
		GoapActionInvalidity invalidity = node.invalidity;
		invalidity.isInvalid = isInvalid;
		invalidity.stateName = stateName;
		invalidity.reason = "target_unreachable";
		return invalidity;
	}

	private bool IsTargetMissing(Character actor, IPointOfInterest poiTarget)
	{
		if (poiTarget.gridTileLocation == null || actor.currentRegion != poiTarget.currentRegion || poiTarget is Character { isDead: false } || poiTarget.numOfNonSecretActionsBeingPerformedOnThis > 0 || poiTarget.isBeingCarriedBy != null || (poiTarget is Character character2 && character2.grave?.isBeingCarriedBy != null))
		{
			return true;
		}
		if (actor.gridTileLocation != poiTarget.gridTileLocation && !actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation, sameStructureOnly: true))
		{
			if (actor.hasMarker && actor.marker.IsCharacterInLineOfSightWith(poiTarget))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		Character deadCharacter = GetDeadCharacter(target);
		if (deadCharacter == null || (witness.race == RACE.RATMAN && actor.race == RACE.RATMAN && deadCharacter.race != RACE.RATMAN) || witness.traitContainer.HasTrait("Cannibal") || !deadCharacter.race.IsSapient())
		{
			return;
		}
		reactions.Add(EMOTION.Shock);
		reactions.Add(EMOTION.Repulsed);
		if (witness.relationshipContainer.IsFriendsOrAcquaintancesWith(actor))
		{
			reactions.Add(EMOTION.Disappointment);
		}
		if (!witness.traitContainer.HasTrait("Psychopath"))
		{
			if (!witness.characterClass.IsCombatant())
			{
				reactions.Add(EMOTION.Fear);
			}
			else if (!witness.relationshipContainer.IsEnemiesWith(deadCharacter))
			{
				reactions.Add(EMOTION.Rage);
			}
		}
	}

	public override void PopulateEmotionReactionsToTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToTarget(reactions, actor, target, witness, node, status);
		Character deadCharacter = GetDeadCharacter(target);
		if (deadCharacter != null && (witness.relationshipContainer.IsFriendsWith(deadCharacter) || witness.relationshipContainer.IsFamilyMemberOrLoverOrAffairAndNotRival(deadCharacter)) && !witness.traitContainer.HasTrait("Psychopath"))
		{
			reactions.Add(EMOTION.Despair);
			reactions.Add(EMOTION.Sadness);
		}
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		if (node.poiTarget is Character character && character.race.IsSapient())
		{
			return REACTABLE_EFFECT.Negative;
		}
		return REACTABLE_EFFECT.Positive;
	}

	public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime)
	{
		if (target is Character character)
		{
			if (actor.race.IsSapient() && character.race.IsSapient())
			{
				return CRIME_TYPE.Cannibalism;
			}
			if (actor.race.IsSapient() && character.raceSetting.category == CHARACTER_CATEGORY.Beast)
			{
				return CRIME_TYPE.Animal_Killing;
			}
		}
		return base.GetCrimeType(actor, target, crime);
	}

	public override CRIME_TYPE GetRawCrimeType(Character actor)
	{
		return CRIME_TYPE.Cannibalism;
	}

	public override void OnStopWhilePerforming(ActualGoapNode node)
	{
		base.OnStopWhilePerforming(node);
		if (!node.poiTarget.isBeingSeized && node.ticksPerformingCurrentState > 0)
		{
			ProduceMats(node);
		}
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (poiTarget.gridTileLocation == null)
			{
				return false;
			}
			if (poiTarget.isBeingCarriedBy != null)
			{
				return false;
			}
			if (poiTarget is Character { grave: not null } character && character.grave.isBeingCarriedBy != null)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	private bool IsTargetDead(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType)
	{
		if (poiTarget is Character character)
		{
			return character.isDead;
		}
		return true;
	}

	private Character GetDeadCharacter(IPointOfInterest poiTarget)
	{
		if (poiTarget is Character result)
		{
			return result;
		}
		if (poiTarget is Tombstone tombstone)
		{
			return tombstone.character;
		}
		return null;
	}

	public void PreTransformSuccess(ActualGoapNode goapNode)
	{
		Character deadCharacter = GetDeadCharacter(goapNode.poiTarget);
		int foodAmountTakenFromPOI = CharacterManager.Instance.GetFoodAmountTakenFromPOI(deadCharacter);
		goapNode.descriptionLog.AddToFillers(goapNode.poiTarget, goapNode.poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
		goapNode.descriptionLog.AddToFillers(null, foodAmountTakenFromPOI.ToString(), LOG_IDENTIFIER.STRING_1);
	}

	public void AfterTransformSuccess(ActualGoapNode goapNode)
	{
		ProduceMats(goapNode);
	}

	private void ProduceMats(ActualGoapNode p_node)
	{
		IPointOfInterest poiTarget = p_node.poiTarget;
		LocationGridTile gridTileLocation = poiTarget.gridTileLocation;
		if (gridTileLocation == null)
		{
			return;
		}
		FoodPile foodPile = CharacterManager.Instance.CreateFoodPileForPOI(poiTarget, gridTileLocation, createLog: false);
		int num = p_node.ticksPerformingCurrentState * m_amountProducedPerTick;
		if (p_node.associatedJobType == JOB_TYPE.BUTCHER)
		{
			p_node.actor.moneyComponent.AdjustCoins(Mathf.CeilToInt((float)num * 0.344f));
		}
		foodPile.SetResourceInPile(num);
		if (p_node.associatedJobType == JOB_TYPE.PRODUCE_FOOD_FOR_CAMP)
		{
			if (p_node.actor.partyComponent.hasParty && p_node.actor.partyComponent.currentParty.targetCamp != null)
			{
				p_node.actor.partyComponent.currentParty.jobComponent.CreateHaulForCampJob(foodPile, p_node.actor.partyComponent.currentParty.targetCamp);
				p_node.actor.marker.AddPOIAsInVisionRange(foodPile);
			}
		}
		else if (foodPile != null && p_node.actor.homeSettlement != null && ((foodPile.tileObjectType != TILE_OBJECT_TYPE.ELF_MEAT && foodPile.tileObjectType != TILE_OBJECT_TYPE.HUMAN_MEAT) || p_node.actor.faction == null || !p_node.actor.faction.isMajorNonPlayer))
		{
			p_node.actor.jobComponent.TryCreateHaulToWorkplaceJob(foodPile);
			p_node.actor.marker.AddPOIAsInVisionRange(foodPile);
		}
		if (foodPile != null)
		{
			p_node.descriptionLog.AddInvolvedObjectManual(foodPile.persistentID);
			if ((foodPile.tileObjectType == TILE_OBJECT_TYPE.HUMAN_MEAT || foodPile.tileObjectType == TILE_OBJECT_TYPE.ELF_MEAT) && !p_node.actor.traitContainer.HasTrait("Cannibal") && p_node.actor.isNormalCharacter && poiTarget is Character characterResponsible)
			{
				p_node.actor.traitContainer.AddTrait(p_node.actor, "Traumatized", characterResponsible);
			}
		}
		p_node.actor.talentComponent?.GetTalent(CHARACTER_TALENT.Food).AdjustExperience(8, p_node.actor);
		gridTileLocation.structure.RemoveCharacterAtLocation(poiTarget as Character);
		if (poiTarget is Character character)
		{
			if (character.grave != null && character.grave.gridTileLocation != null)
			{
				character.grave.SetRespawnCorpseOnDestroy(state: false);
				character.grave.gridTileLocation.structure.RemovePOI(character.grave);
			}
			else
			{
				character.DestroyMarker();
			}
		}
		ProduceLogs(p_node, foodPile);
	}

	public void ProduceLogs(ActualGoapNode p_node, FoodPile foodPile)
	{
		string value = p_node.ticksPerformingCurrentState * m_amountProducedPerTick + " " + foodPile.tileObjectType.LocalizedName();
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", p_node.action.goapName + " produced_resources", LOG_TAG.Work, null);
		log.AddToFillers(p_node.actor, p_node.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddToFillers(null, value, LOG_IDENTIFIER.STRING_1);
		p_node.descriptionLog.AddLogToDatabase();
		p_node.LogAction(log, ignoreShouldAddLog: true);
	}
}

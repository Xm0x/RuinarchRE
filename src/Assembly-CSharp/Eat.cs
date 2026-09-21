using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UnityEngine.Localization.Settings;
using UtilityScripts;

public class Eat : GoapAction
{
	private Precondition _foodPrecondition;

	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.CONSUME;

	public Eat()
		: base(INTERACTION_TYPE.EAT)
	{
		base.actionIconString = GoapActionStateDB.Eat_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Needs };
		_foodPrecondition = new Precondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_POI, "Food Pile", p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET), HasFood);
	}

	public override bool ShouldActionBeAnIntel(ActualGoapNode node)
	{
		if (node.crimeType != CRIME_TYPE.None && node.crimeType != CRIME_TYPE.Unset)
		{
			return true;
		}
		return base.ShouldActionBeAnIntel(node);
	}

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		if (node.target is Table)
		{
			if (LocalizationSettings.SelectedLocale.LocaleName.Equals("Polish (pl)") || LocalizationSettings.SelectedLocale.LocaleName.Equals("English (en)"))
			{
				log.AddToFillers(node.target, node.target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				return;
			}
			log.AddToFillers(node.target, LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Small_At") + " " + LocalizationManager.Instance.GetLocalizedValue("Articles_Table", "Lower_A") + " " + node.target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
		}
		else
		{
			log.AddToFillers(node.target, node.target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
		}
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.STAMINA_RECOVERY, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
	}

	public override Precondition GetPrecondition(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType, out bool isOverridden)
	{
		if (target is Table && !(actor is Summon) && jobType != JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT)
		{
			Precondition foodPrecondition = _foodPrecondition;
			isOverridden = true;
			return foodPrecondition;
		}
		return base.GetPrecondition(actor, target, otherData, jobType, out isOverridden);
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Eat Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		string log = string.Empty;
		if (actor.traitContainer.HasTrait("Enslaved"))
		{
			if (target.gridTileLocation == null)
			{
				return 2000;
			}
			if (actor.HasHome() && !actor.HasTerritory() && !target.gridTileLocation.IsInHomeOf(actor))
			{
				return 2000;
			}
		}
		int num = 0;
		if (actor.movementComponent.ShouldAvoidStructureLocationOfTarget(target) && !actor.partyComponent.hasParty)
		{
			return 2000;
		}
		if (job.jobType == JOB_TYPE.MANIFEST_FOOD_EAT)
		{
			return 10;
		}
		if (actor is Rat)
		{
			if (target is FoodPile)
			{
				num += Utilities.Rng.Next(20, 51);
			}
			else
			{
				if (!(target is Table))
				{
					return 2000;
				}
				num += Utilities.Rng.Next(10, 61);
			}
		}
		else
		{
			if (actor.race == RACE.RATMAN)
			{
				Faction faction = actor.faction;
				if (faction != null && faction.factionType.type == FACTION_TYPE.Ratmen)
				{
					BaseSettlement settlement = null;
					if (target.gridTileLocation != null && target.gridTileLocation.IsPartOfSettlement(out settlement))
					{
						Faction owner = settlement.owner;
						if (owner != null && actor.faction != owner)
						{
							return 2000;
						}
						num = ((owner != null && owner.factionType.type == FACTION_TYPE.Ratmen) ? (num + Utilities.Rng.Next(800, 851)) : (num + Utilities.Rng.Next(850, 951)));
					}
					else
					{
						num += Utilities.Rng.Next(800, 851);
					}
					goto IL_0c12;
				}
			}
			BaseSettlement settlement3;
			bool needsToPay;
			int buyerOpinionOfWorker;
			if (target is Table table)
			{
				bool flag = actor.trapStructure.IsTrapStructure(table.gridTileLocation.structure) || actor.trapStructure.IsTrapArea(table.gridTileLocation.area);
				BaseSettlement settlement2 = null;
				if (table.gridTileLocation != null && table.gridTileLocation.IsPartOfSettlement(out settlement2) && actor.faction != null && settlement2.owner != null && settlement2.owner.IsHostileWith(actor.faction))
				{
					num += 2000;
				}
				else if (flag)
				{
					num = Utilities.Rng.Next(50, 71);
				}
				else if (!actor.traitContainer.HasTrait("Travelling"))
				{
					num = ((settlement2 == actor.homeSettlement) ? ((table.structureLocation == actor.homeStructure) ? Utilities.Rng.Next(20, 36) : (actor.needsComponent.isStarving ? ((!actor.traitContainer.HasTrait("Psychopath", "Treacherous", "Evil") && !actor.trapStructure.IsTrapStructure(target.gridTileLocation?.structure) && actor.moodComponent.moodState == MOOD_STATE.Normal) ? ((actor.traitContainer.HasTrait("Diplomatic") && actor.characterClass.className == "Hero") ? (num + 2000) : ((job.jobType == JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT) ? ((!GameUtilities.RollChanceThreadSafe(25, ref log)) ? (num + 2000) : Utilities.Rng.Next(800, 851)) : ((!actor.traitContainer.HasTrait("Malnourished")) ? (num + 2000) : ((!GameUtilities.RollChanceThreadSafe(50, ref log)) ? (num + 2000) : Utilities.Rng.Next(800, 851))))) : Utilities.Rng.Next(800, 851)) : ((table.characterOwner == null || table.IsOwnedBy(actor) || table.characterOwner.relationshipContainer.IsLoverOrAffair(actor) || table.characterOwner.relationshipContainer.IsFamilyMember(actor)) ? Utilities.Rng.Next(50, 71) : (num + 2000)))) : ((table.characterOwner == null || table.IsOwnedBy(actor) || !actor.relationshipContainer.IsFriendsWith(table.characterOwner)) ? 996 : 988));
				}
				else if (table.structureLocation.structureType == STRUCTURE_TYPE.TAVERN || table.structureLocation == actor.homeStructure)
				{
					num = Utilities.Rng.Next(400, 451);
				}
				else if (actor.needsComponent.isStarving)
				{
					Character characterOwner = table.characterOwner;
					num = ((characterOwner == null) ? 994 : ((characterOwner == actor) ? Utilities.Rng.Next(400, 451) : ((!actor.traitContainer.HasTrait("Psychopath", "Treacherous", "Evil") && !actor.trapStructure.IsTrapStructure(table.structureLocation) && actor.moodComponent.moodState == MOOD_STATE.Normal) ? ((!actor.traitContainer.HasTrait("Diplomatic") && !(actor.characterClass.className == "Hero")) ? ((job.jobType == JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT) ? ((!GameUtilities.RollChanceThreadSafe(25, ref log)) ? (num + 2000) : Utilities.Rng.Next(800, 851)) : ((!actor.traitContainer.HasTrait("Malnourished")) ? (num + 2000) : ((!GameUtilities.RollChanceThreadSafe(50, ref log)) ? (num + 2000) : Utilities.Rng.Next(800, 851)))) : (num + 2000)) : Utilities.Rng.Next(800, 851))));
				}
				else
				{
					num += 2000;
				}
			}
			else if (target is ElfMeat || target is HumanMeat)
			{
				num = (actor.traitContainer.HasTrait("Cannibal") ? Utilities.Rng.Next(350, 451) : ((!actor.needsComponent.isStarving) ? 2000 : ((!actor.traitContainer.HasTrait("Psychopath", "Treacherous", "Evil") && !actor.trapStructure.IsTrapStructure(target.gridTileLocation?.structure) && actor.moodComponent.moodState == MOOD_STATE.Normal) ? ((actor.characterClass.className == "Hero") ? (num + 2000) : ((job.jobType == JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT) ? ((!GameUtilities.RollChanceThreadSafe(25, ref log)) ? (num + 2000) : Utilities.Rng.Next(800, 851)) : ((!actor.traitContainer.HasTrait("Malnourished")) ? (num + 2000) : ((!GameUtilities.RollChanceThreadSafe(50, ref log)) ? (num + 2000) : Utilities.Rng.Next(800, 851))))) : Utilities.Rng.Next(800, 851))));
			}
			else if (target.gridTileLocation == null || !target.gridTileLocation.IsPartOfSettlement(out settlement3) || settlement3.owner == null || actor.faction == null)
			{
				num = ((target.gridTileLocation == null || !target.gridTileLocation.structure.structureType.IsForageStructure()) ? Utilities.Rng.Next(850, 861) : Utilities.Rng.Next(350, 451));
			}
			else if (actor.faction.IsHostileWith(settlement3.owner))
			{
				num = (actor.needsComponent.isStarving ? Utilities.Rng.Next(850, 861) : (num + 2000));
			}
			else if (target.gridTileLocation.structure.structureType == STRUCTURE_TYPE.TAVERN)
			{
				num = Utilities.Rng.Next(600, 651);
			}
			else if (target.gridTileLocation.structure.structureType.IsSpecialStructure())
			{
				num = Utilities.Rng.Next(700, 751);
			}
			else if (!(target.gridTileLocation.structure is ManMadeStructure manMadeStructure) || !(target is FoodPile))
			{
				num = ((!actor.needsComponent.isStarving) ? (num + 2000) : Utilities.Rng.Next(770, 781));
			}
			else if (manMadeStructure == actor.homeStructure)
			{
				num = Utilities.Rng.Next(100, 151);
			}
			else if (manMadeStructure.CanPurchaseFromHere(actor, out needsToPay, out buyerOpinionOfWorker))
			{
				num = ((!needsToPay) ? Utilities.Rng.Next(100, 151) : ((!actor.moneyComponent.CanAfford(10) && !actor.needsComponent.isStarving) ? 2000 : Utilities.Rng.Next(600, 651)));
			}
			else if (job.jobType.IsFullnessRecoveryTypeJob() && actor.isNormalCharacter)
			{
				LocationStructure locationStructure = target.gridTileLocation?.structure;
				LocationStructure locationStructure2 = actor.gridTileLocation?.structure;
				if (actor.needsComponent.isStarving)
				{
					num = ((!actor.traitContainer.HasTrait("Psychopath", "Treacherous", "Evil") && !actor.trapStructure.IsTrapStructure(target.gridTileLocation?.structure) && actor.moodComponent.moodState == MOOD_STATE.Normal) ? ((job.jobType == JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT) ? ((!GameUtilities.RollChanceThreadSafe(25, ref log)) ? (num + 2000) : Utilities.Rng.Next(800, 851)) : ((!actor.traitContainer.HasTrait("Malnourished")) ? (num + 2000) : ((!GameUtilities.RollChanceThreadSafe(50, ref log)) ? (num + 2000) : Utilities.Rng.Next(800, 851)))) : Utilities.Rng.Next(800, 851));
				}
				else if (locationStructure is Dwelling)
				{
					if (!locationStructure.IsResident(actor))
					{
						bool flag2 = false;
						if (locationStructure2 == locationStructure)
						{
							LocationGridTile randomPassableTile = GridMap.Instance.mainRegion.wilderness.GetRandomPassableTile();
							if (randomPassableTile == null || !actor.movementComponent.HasPathToEvenIfDiffRegion(randomPassableTile))
							{
								flag2 = true;
							}
						}
						num = ((!flag2) ? (num + 2000) : ((!actor.traitContainer.HasTrait("Psychopath", "Treacherous", "Evil") && !actor.trapStructure.IsTrapStructure(target.gridTileLocation?.structure) && actor.moodComponent.moodState == MOOD_STATE.Normal) ? ((job.jobType == JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT) ? ((!GameUtilities.RollChanceThreadSafe(25, ref log)) ? (num + 2000) : Utilities.Rng.Next(800, 851)) : ((!actor.traitContainer.HasTrait("Malnourished")) ? (num + 2000) : ((!GameUtilities.RollChanceThreadSafe(50, ref log)) ? (num + 2000) : Utilities.Rng.Next(800, 851)))) : Utilities.Rng.Next(800, 851)));
					}
				}
				else
				{
					num += 2000;
				}
			}
			else
			{
				num += 2000;
			}
		}
		goto IL_0c12;
		IL_0c12:
		return num;
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		IPointOfInterest poiTarget = node.poiTarget;
		if (!goapActionInvalidity.isInvalid)
		{
			if (!poiTarget.IsAvailable())
			{
				goapActionInvalidity.isInvalid = true;
				goapActionInvalidity.reason = "target_unavailable";
			}
			else if (poiTarget is FoodPile { structureLocation: not null, structureLocation: ManMadeStructure structureLocation } && structureLocation.structureType.IsFoodProducingStructure() && node.actor.isNormalCharacter)
			{
				if (structureLocation.CanPurchaseFromHere(node.actor, out var needsToPay, out var _))
				{
					if (needsToPay && !node.actor.moneyComponent.CanAfford(10))
					{
						goapActionInvalidity.isInvalid = true;
						goapActionInvalidity.reason = "not_enough_money";
					}
				}
				else
				{
					goapActionInvalidity.isInvalid = true;
					goapActionInvalidity.reason = "cannot_buy";
				}
			}
		}
		return goapActionInvalidity;
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		if (node.poiTarget is TileObject { characterOwner: not null } tileObject && !tileObject.IsOwnedBy(node.actor) && !tileObject.characterOwner.relationshipContainer.IsLoverOrAffair(node.actor) && !tileObject.characterOwner.relationshipContainer.IsFamilyMember(node.actor))
		{
			return REACTABLE_EFFECT.Negative;
		}
		return REACTABLE_EFFECT.Neutral;
	}

	public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime)
	{
		if (actor.race.IsSapient())
		{
			if (target is Character character && character.race.IsSapient())
			{
				return CRIME_TYPE.Cannibalism;
			}
			if (target is HumanMeat || target is ElfMeat)
			{
				return CRIME_TYPE.Cannibalism;
			}
			if (target is TileObject { characterOwner: not null } tileObject && !tileObject.IsOwnedBy(actor))
			{
				bool num = tileObject.characterOwner.relationshipContainer.IsLoverOrAffair(actor) || tileObject.characterOwner.relationshipContainer.IsFamilyMember(actor);
				bool flag = tileObject.characterOwner.relationshipContainer.GetOpinionLabel(actor) == "Close Friend";
				if (!num && !flag)
				{
					LocationStructure structureLocation = tileObject.structureLocation;
					if (structureLocation is Dwelling && structureLocation != actor.homeStructure && !actor.trapStructure.IsTrapStructure(structureLocation))
					{
						return CRIME_TYPE.Theft;
					}
				}
			}
		}
		return base.GetCrimeType(actor, target, crime);
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		Character character = target as Character;
		if (!witness.traitContainer.HasTrait("Cannibal") && ((character != null && character.race.IsSapient()) || target is HumanMeat || target is ElfMeat))
		{
			reactions.Add(EMOTION.Repulsed);
			if (witness.relationshipContainer.IsFriendsOrAcquaintancesWith(actor))
			{
				reactions.Add(EMOTION.Disappointment);
			}
			if (!witness.traitContainer.HasTrait("Psychopath"))
			{
				if (witness.characterClass.IsCombatant())
				{
					reactions.Add(EMOTION.Threatened);
				}
				else
				{
					reactions.Add(EMOTION.Fear);
				}
			}
		}
		if (node.crimeType == CRIME_TYPE.Theft && target is TileObject tileObject)
		{
			if (tileObject.characterOwner == witness)
			{
				reactions.Add(EMOTION.Anger);
			}
			else if (!reactions.Contains(EMOTION.Disappointment))
			{
				reactions.Add(EMOTION.Disappointment);
			}
		}
	}

	public override bool IsFullnessRecoveryAction()
	{
		return true;
	}

	public void PreEatSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.actor.isNormalCharacter)
		{
			if (goapNode.poiTarget is FoodPile { structureLocation: not null, structureLocation: ManMadeStructure structureLocation } && structureLocation.structureType.IsFoodProducingStructure() && structureLocation.CanPurchaseFromHere(goapNode.actor, out var needsToPay, out var _) && needsToPay && goapNode.actor.moneyComponent.CanAfford(10))
			{
				goapNode.actor.moneyComponent.AdjustCoins(-10);
			}
			LocationStructure locationStructure = goapNode.poiTarget.gridTileLocation?.structure;
			if (locationStructure != null && locationStructure.structureType == STRUCTURE_TYPE.TAVERN && locationStructure is ManMadeStructure manMadeStructure && manMadeStructure.HasAssignedWorker())
			{
				string id = manMadeStructure.assignedWorkerIDs[0];
				DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(id).moneyComponent.AdjustCoins(33);
			}
		}
	}

	public void AfterEatSuccess(ActualGoapNode goapNode)
	{
		if (!goapNode.actor.traitContainer.HasTrait("Cannibal") && (goapNode.poiTarget is ElfMeat || goapNode.poiTarget is HumanMeat) && goapNode.actor.isNotSummonAndDemon)
		{
			goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Cannibal");
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "became_cannibal", LOG_TAG.Life_Changes, LOG_TAG.Needs, LOG_TAG.Crimes, goapNode);
			log.AddToFillers(goapNode.actor, goapNode.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(null, goapNode.poiTarget.name, LOG_IDENTIFIER.STRING_1);
			log.AddLogToDatabase(releaseLogAfter: true);
		}
		if (goapNode.actor.race == RACE.ELVES && goapNode.poiTarget is RatMeat)
		{
			goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Poor Meal");
		}
		if (goapNode.poiTarget is Table table)
		{
			table.ApplyFoodEffectsToConsumer(goapNode.actor);
		}
		else
		{
			if (!(goapNode.poiTarget is FoodPile foodPile))
			{
				return;
			}
			foodPile.ApplyFoodEffectsToConsumer(goapNode.actor);
			if (foodPile.isFromManifestFood)
			{
				Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "PlayerAlerts_Table", "manifest_food_forget", LOG_TAG.Life_Changes, LOG_TAG.Needs, LOG_TAG.Player, goapNode);
				log2.AddToFillers(goapNode.actor, goapNode.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log2.AddToFillers(goapNode.poiTarget, goapNode.poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				log2.AddLogToDatabase(releaseLogAfter: true);
				goapNode.actor.CancelAllJobs();
				switch (foodPile.infusedType)
				{
				case FOOD_INFUSE_TYPE.Bloated:
					goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Bloated");
					break;
				case FOOD_INFUSE_TYPE.Food_Coma:
					goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Food Coma");
					break;
				case FOOD_INFUSE_TYPE.Rabid:
					goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Rabid");
					break;
				}
			}
		}
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (!poiTarget.IsAvailable())
			{
				return false;
			}
			if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapStructureIsNot(poiTarget.gridTileLocation.structure))
			{
				return false;
			}
			if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapAreaIsNot(poiTarget.gridTileLocation.area))
			{
				return false;
			}
			if (actor.traitContainer.HasTrait("Vampire"))
			{
				return false;
			}
			if (poiTarget is BerryShrub && !actor.needsComponent.isStarving && actor.homeStructure != null)
			{
				return false;
			}
			if (poiTarget is Table)
			{
				if (poiTarget.resourceStorageComponent.GetResourceValue(RESOURCE.FOOD) < 10 && job.jobType == JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT)
				{
					return false;
				}
				if (poiTarget.gridTileLocation != null && poiTarget.gridTileLocation.structure is Tavern && poiTarget.resourceStorageComponent.GetResourceValue(RESOURCE.FOOD) <= 0)
				{
					return false;
				}
				if (!(actor is Rat) && (GameUtilities.IsRaceBeast(actor.race) || !actor.isNormalCharacter))
				{
					return false;
				}
			}
			else if (poiTarget is FoodPile && poiTarget.resourceStorageComponent.GetResourceValue(RESOURCE.FOOD) < 10 && job.jobType == JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT)
			{
				return false;
			}
			if (poiTarget.gridTileLocation != null)
			{
				if (job.jobType.IsFullnessRecoveryTypeJob() && actor.isNormalCharacter)
				{
					LocationStructure locationStructure = poiTarget.gridTileLocation?.structure;
					LocationStructure locationStructure2 = actor.gridTileLocation?.structure;
					if (locationStructure == null)
					{
						return false;
					}
					if (locationStructure is Dwelling)
					{
						if (!locationStructure.IsResident(actor))
						{
							if (actor.needsComponent.isStarving)
							{
								return true;
							}
							bool flag = false;
							if (locationStructure2 == locationStructure)
							{
								LocationGridTile randomPassableTile = GridMap.Instance.mainRegion.wilderness.GetRandomPassableTile();
								if (randomPassableTile == null || !actor.movementComponent.HasPathToEvenIfDiffRegion(randomPassableTile))
								{
									flag = true;
								}
							}
							if (!flag)
							{
								return false;
							}
						}
					}
					else if (locationStructure.structureType.IsFoodProducingStructure() && locationStructure is ManMadeStructure manMadeStructure && actor.homeStructure is Dwelling && !manMadeStructure.DoesCharacterWorkHere(actor))
					{
						if (actor.needsComponent.isStarving)
						{
							return true;
						}
						if (actor.moneyComponent.CanAfford(10))
						{
							return true;
						}
						bool flag2 = false;
						if (locationStructure2 == locationStructure)
						{
							LocationGridTile randomPassableTile2 = GridMap.Instance.mainRegion.wilderness.GetRandomPassableTile();
							if (randomPassableTile2 == null || !actor.movementComponent.HasPathToEvenIfDiffRegion(randomPassableTile2))
							{
								flag2 = true;
							}
						}
						if (!flag2)
						{
							return false;
						}
					}
				}
				return true;
			}
		}
		return false;
	}

	private bool HasFood(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType)
	{
		return poiTarget.resourceStorageComponent.HasResourceAmount(RESOURCE.FOOD, 10);
	}
}

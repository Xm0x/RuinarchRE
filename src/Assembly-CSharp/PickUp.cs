using System.Collections.Generic;
using UtilityScripts;

public class PickUp : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;

	public PickUp()
		: base(INTERACTION_TYPE.PICK_UP)
	{
		base.actionIconString = GoapActionStateDB.Haul_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override bool ShouldActionBeAnIntel(ActualGoapNode node)
	{
		if (node.crimeType != CRIME_TYPE.None && node.crimeType != CRIME_TYPE.Unset)
		{
			return true;
		}
		return base.ShouldActionBeAnIntel(node);
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_POI, GOAP_EFFECT_TARGET.TARGET));
		AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_POI, GOAP_EFFECT_TARGET.ACTOR));
	}

	protected override List<GoapEffect> GetExpectedEffects(Character actor, IPointOfInterest target, OtherData[] otherData, out bool isOverridden)
	{
		List<GoapEffect> list = RuinarchListPool<GoapEffect>.Claim(4);
		AddBaseExpectedEffectsToList(list);
		TileObject tileObject = target as TileObject;
		if (tileObject is ResourcePile)
		{
			list.Add(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_POI, tileObject.internalName, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET));
		}
		list.Add(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_POI, tileObject.internalName, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
		isOverridden = true;
		return list;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Take Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		if (actor.movementComponent.ShouldAvoidStructureLocationOfTarget(target) && !actor.partyComponent.hasParty)
		{
			return 2000;
		}
		if (job != null && job.jobType == JOB_TYPE.TAKE_ITEM_ON_SIGHT && target.gridTileLocation != null && target.gridTileLocation.IsPartOfSettlement(out var settlement) && settlement.locationType == LOCATION_TYPE.VILLAGE && settlement.owner != null && settlement.owner != actor.faction && actor.faction != null)
		{
			FactionRelationship relationshipWith = actor.faction.GetRelationshipWith(settlement.owner);
			if (relationshipWith != null && relationshipWith.relationshipStatus != FACTION_RELATIONSHIP_STATUS.Hostile)
			{
				return 2000;
			}
		}
		int num = 0;
		if (job != null && job.jobType == JOB_TYPE.OBTAIN_PERSONAL_ITEM && !target.gridTileLocation.IsPartOfSettlement(actor.homeSettlement))
		{
			num += 2000;
		}
		if (target is TileObject tileObject)
		{
			if (tileObject is Heirloom && job != null && job.jobType == JOB_TYPE.DROP_ITEM_PARTY)
			{
				num += 10;
			}
			else if (tileObject is FoodPile && job != null && job.jobType == JOB_TYPE.DISPOSE_FOOD_PILE)
			{
				num += 10;
			}
			else if (tileObject.characterOwner != null)
			{
				num = (tileObject.IsOwnedBy(actor) ? ((tileObject.gridTileLocation == null || tileObject.gridTileLocation.structure != actor.homeStructure) ? (num + Utilities.Rng.Next(40, 81)) : (num + Utilities.Rng.Next(10, 31))) : ((!(tileObject is LunchPack)) ? (num + 2000) : (num + 10)));
			}
			else if (job != null && (job.jobType == JOB_TYPE.TAKE_ITEM_ON_SIGHT || job.jobType == JOB_TYPE.HAUL || job.jobType == JOB_TYPE.COMBINE_STOCKPILE || job.jobType == JOB_TYPE.STOCKPILE_FOOD || job.jobType == JOB_TYPE.OBTAIN_WANTED_ITEM || job.jobType == JOB_TYPE.FORAGE_FOOD || job.jobType == JOB_TYPE.DOUSE_FIRE || job.jobType == JOB_TYPE.HAUL_ON_SIGHT))
			{
				num += 10;
			}
			else if (job != null && job.jobType == JOB_TYPE.REMOVE_STATUS)
			{
				if (target.gridTileLocation != null && actor.homeSettlement != null && actor.movementComponent.HasPathTo(tileObject.gridTileLocation) && (tileObject.gridTileLocation.IsPartOfSettlement(actor.homeSettlement) || tileObject.gridTileLocation.IsNextToSettlementArea(actor.homeSettlement)))
				{
					int num2 = Utilities.Rng.Next(40, 81);
					num += num2;
				}
				else
				{
					num += 2000;
				}
			}
			else if (actor.homeSettlement != null && tileObject.gridTileLocation != null && tileObject.gridTileLocation.area.HasSettlementOnArea(actor.homeSettlement))
			{
				int num3 = Utilities.Rng.Next(80, 91);
				num += num3;
			}
			else if (!actor.isFactionless && !actor.isVagrantOrFactionless && tileObject.gridTileLocation != null && tileObject.gridTileLocation.area.HasSettlementWithFactionOwnerOnArea(actor.faction))
			{
				int num4 = Utilities.Rng.Next(100, 121);
				num += num4;
			}
			else
			{
				num += 2000;
			}
		}
		if (actor is Troll && job != null && job.jobType == JOB_TYPE.DROP_ITEM)
		{
			num = 10;
		}
		return num;
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		if (node.poiTarget is TileObject { characterOwner: not null } tileObject && !tileObject.IsOwnedBy(node.actor))
		{
			return REACTABLE_EFFECT.Negative;
		}
		return REACTABLE_EFFECT.Neutral;
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		if (!(target is TileObject { characterOwner: not null } tileObject) || tileObject.IsOwnedBy(node.actor))
		{
			return;
		}
		reactions.Add(EMOTION.Disapproval);
		if (witness.relationshipContainer.IsFriendsWith(actor))
		{
			reactions.Add(EMOTION.Disappointment);
			reactions.Add(EMOTION.Shock);
			if (tileObject.IsOwnedBy(witness))
			{
				reactions.Add(EMOTION.Betrayal);
			}
		}
		else if (tileObject.IsOwnedBy(witness))
		{
			reactions.Add(EMOTION.Anger);
		}
	}

	public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime)
	{
		if (target is TileObject { characterOwner: not null } tileObject)
		{
			Character firstCharacterWithRelationship = tileObject.characterOwner.relationshipContainer.GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER);
			if (actor != tileObject.characterOwner && actor != firstCharacterWithRelationship)
			{
				return CRIME_TYPE.Theft;
			}
		}
		return base.GetCrimeType(actor, target, crime);
	}

	public override void OnStopWhileStarted(ActualGoapNode node)
	{
		base.OnStopWhileStarted(node);
		if (node.associatedJobType != JOB_TYPE.COMBINE_STOCKPILE)
		{
			return;
		}
		Character actor = node.actor;
		if (actor.HasItem<ResourcePile>())
		{
			List<ResourcePile> list = RuinarchListPool<ResourcePile>.Claim();
			actor.PopulateItemsOfType(list);
			for (int i = 0; i < list.Count; i++)
			{
				ResourcePile item = list[i];
				actor.DropItem(item);
			}
			RuinarchListPool<ResourcePile>.Release(list);
		}
	}

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		log.AddToFillers(node.poiTarget, node.poiTarget.name, LOG_IDENTIFIER.ITEM_1);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			TileObject item = poiTarget as TileObject;
			if (poiTarget.gridTileLocation != null && !actor.HasItemOrEquipment(item))
			{
				return poiTarget.numOfNonSecretActionsBeingPerformedOnThis <= 0;
			}
			return false;
		}
		return false;
	}

	public void PreTakeSuccess(ActualGoapNode goapNode)
	{
	}

	public void AfterTakeSuccess(ActualGoapNode goapNode)
	{
		bool setOwnership = !goapNode.actor.isNormalCharacter || goapNode.actor.isConsideredRatman || (goapNode.associatedJobType != JOB_TYPE.HAUL && goapNode.associatedJobType != JOB_TYPE.FULLNESS_RECOVERY_NORMAL && goapNode.associatedJobType != JOB_TYPE.FULLNESS_RECOVERY_URGENT && goapNode.associatedJobType != JOB_TYPE.OBTAIN_PERSONAL_FOOD && goapNode.associatedJobType != JOB_TYPE.HAUL_ON_SIGHT);
		Faction faction = goapNode.actor.faction;
		bool changeCharacterOwnership = (faction != null && faction.factionType.type == FACTION_TYPE.Bandits && (goapNode.associatedJobType == JOB_TYPE.TAKE_ITEM_ON_SIGHT || goapNode.associatedJobType == JOB_TYPE.KLEPTOMANIAC_STEAL)) || (goapNode.associatedJobType == JOB_TYPE.STOCKPILE_FOOD && goapNode.poiTarget is LunchPack);
		bool equipItem = goapNode.associatedJobType != JOB_TYPE.DROP_ITEM_TO_WORKPLACE;
		goapNode.actor.PickUpItem(goapNode.poiTarget as TileObject, changeCharacterOwnership, setOwnership, equipItem);
	}
}

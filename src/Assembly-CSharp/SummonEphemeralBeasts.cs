using System.Collections.Generic;
using Inner_Maps;
using UtilityScripts;

public class SummonEphemeralBeasts : GoapAction
{
	public SummonEphemeralBeasts()
		: base(INTERACTION_TYPE.SUMMON_EPHEMERAL_BEASTS)
	{
		base.actionIconString = GoapActionStateDB.Magic_Icon;
		base.actionLocationType = ACTION_LOCATION_TYPE.TARGET_IN_VISION;
		base.doesNotStopTargetCharacter = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Combat };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Summon Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		return REACTABLE_EFFECT.Negative;
	}

	public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime)
	{
		return CRIME_TYPE.Assault;
	}

	public override CRIME_TYPE GetRawCrimeType(Character actor)
	{
		return CRIME_TYPE.Assault;
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

	public void AfterSummonSuccess(ActualGoapNode goapNode)
	{
		Character character = goapNode.poiTarget as Character;
		Character actor = goapNode.actor;
		LocationGridTile gridTileLocation = character.gridTileLocation;
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		gridTileLocation.PopulateTilesInRadius(list, 2, 0, includeCenterTile: false, includeTilesInDifferentStructure: false, includeImpassable: false);
		MonsterMigrationBiomeAtomizedData randomMonsterForSummoning = (PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.TRIGGER_GRUDGE) as TriggerGrudgeData).GetRandomMonsterForSummoning();
		int num = GameUtilities.RandomBetweenTwoNumbers(randomMonsterForSummoning.minRange, randomMonsterForSummoning.maxRange);
		for (int i = 0; i < num; i++)
		{
			LocationGridTile locationTile = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
			Summon summon = CharacterManager.Instance.CreateNewSummon(randomMonsterForSummoning.monsterType, actor.faction, actor.homeSettlement, GridMap.Instance.mainRegion, null, "", bypassIdeologyChecking: true);
			summon.traitContainer.AddTrait(summon, "Ephemeral", null, bypassElementalChance: false, 40);
			summon.combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
			CharacterManager.Instance.PlaceSummonInitially(summon, locationTile);
			actor.petComponent.AddPet(summon, p_setRelationship: false);
			summon.combatComponent.Fight(character, "Pet_Owner_Grudge");
		}
		if (actor.faction != null)
		{
			Messenger.Broadcast(FactionSignals.UPDATE_FACTION_COUNT, actor.faction);
		}
	}
}

using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UnityEngine;
using UtilityScripts;

public class DemonRaidBehaviour : CharacterBehaviour
{
	public DemonRaidBehaviour()
	{
		base.priority = 200;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		bool flag = false;
		Party currentParty = character.partyComponent.currentParty;
		if (currentParty.isActive && currentParty.partyState == PARTY_STATE.Working && currentParty.targetDestination.IsAtTargetDestination(character))
		{
			DemonRaidPartyQuest demonRaidPartyQuest = currentParty.currentQuest as DemonRaidPartyQuest;
			demonRaidPartyQuest.SetIsSuccessful(state: true);
			if (demonRaidPartyQuest.target == null)
			{
				currentParty.GoBackHomeAndEndQuest();
				return true;
			}
			BaseSettlement targetSettlement = demonRaidPartyQuest.targetSettlement;
			if (targetSettlement.locationType == LOCATION_TYPE.DUNGEON && targetSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.HALLOWED_GROUND) is Inner_Maps.Location_Structures.HallowedGround hallowedGround)
			{
				if (hallowedGround.claimedByReligion == RELIGION.None)
				{
					currentParty.GoBackHomeAndEndQuest();
					return true;
				}
				bool flag2 = false;
				for (int i = 0; i < currentParty.members.Count; i++)
				{
					if (currentParty.members[i].jobQueue.HasJob(JOB_TYPE.CLEANSE_HALLOWED_GROUND))
					{
						flag2 = true;
					}
				}
				if (!flag2)
				{
					HallowedGround firstTileObjectOfType = targetSettlement.GetFirstTileObjectOfType<HallowedGround>(TILE_OBJECT_TYPE.HALLOWED_GROUND);
					if (firstTileObjectOfType != null)
					{
						flag = character.jobComponent.CreateCleanseHallowedGroundJob(firstTileObjectOfType, out producedJob);
					}
				}
				else
				{
					flag = character.jobComponent.TriggerRoamAroundStructure(out producedJob);
				}
			}
			if (!flag)
			{
				bool flag3 = false;
				if (demonRaidPartyQuest.raidType == DEMON_RAID_TYPE.Harass_Villagers)
				{
					flag3 = HarassVillagers(character, demonRaidPartyQuest, ref log);
				}
				else if (demonRaidPartyQuest.raidType == DEMON_RAID_TYPE.Destroy_Structures)
				{
					flag3 = DestroyStructures(character, demonRaidPartyQuest, ref log);
				}
				else if (demonRaidPartyQuest.raidType == DEMON_RAID_TYPE.Destroy_Supplies)
				{
					flag3 = DestroySupplies(character, demonRaidPartyQuest, ref log);
				}
				else if (demonRaidPartyQuest.raidType == DEMON_RAID_TYPE.Destroy_Defenses)
				{
					flag3 = DestroyDefenses(character, demonRaidPartyQuest, ref log);
				}
				if (!flag3)
				{
					LocationStructure randomStructure = targetSettlement.GetRandomStructure();
					if (randomStructure != null)
					{
						LocationGridTile randomPassableTile = randomStructure.GetRandomPassableTile();
						flag = ((randomPassableTile == null) ? character.jobComponent.TriggerRoamAroundStructure(out producedJob) : character.jobComponent.CreateGoToSpecificTileJob(randomPassableTile, out producedJob));
					}
					else
					{
						flag = character.jobComponent.TriggerRoamAroundStructure(out producedJob);
					}
				}
			}
		}
		if (producedJob != null)
		{
			producedJob.SetIsThisAPartyJob(state: true);
		}
		return flag;
	}

	private bool HarassVillagers(Character p_actor, DemonRaidPartyQuest p_quest, ref string p_log)
	{
		BaseSettlement targetSettlement = p_quest.targetSettlement;
		Character randomAliveResidentInsideSettlementThatIsHostileWith = GetRandomAliveResidentInsideSettlementThatIsHostileWith(p_actor, targetSettlement);
		if (randomAliveResidentInsideSettlementThatIsHostileWith != null)
		{
			p_actor.combatComponent.Fight(randomAliveResidentInsideSettlementThatIsHostileWith, "Raid", null, isLethal: false);
			return true;
		}
		return false;
	}

	private Character GetRandomAliveResidentInsideSettlementThatIsHostileWith(Character p_actor, BaseSettlement p_targetSettlement)
	{
		List<Character> list = RuinarchListPool<Character>.Claim();
		Character result = null;
		for (int i = 0; i < p_targetSettlement.residents.Count; i++)
		{
			Character character = p_targetSettlement.residents[i];
			if (p_actor != character && !character.isDead && !character.isBeingSeized && character.gridTileLocation != null && character.gridTileLocation.IsPartOfSettlement(p_targetSettlement) && (character.faction == null || p_actor.faction == null || p_actor.faction.IsHostileWith(character.faction)) && character.CanBeDamaged() && !character.traitContainer.HasTrait("Unconscious", "Ensnared"))
			{
				list.Add(character);
			}
		}
		if (list.Count > 0)
		{
			result = list[Random.Range(0, list.Count)];
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	private bool DestroyStructures(Character p_actor, DemonRaidPartyQuest p_quest, ref string p_log)
	{
		BaseSettlement targetSettlement = p_quest.targetSettlement;
		ManMadeStructure firstStructureToDestroy = GetFirstStructureToDestroy(p_actor, targetSettlement);
		if (firstStructureToDestroy != null)
		{
			TileObject tileObjectToAttackToDestroyStructure = GetTileObjectToAttackToDestroyStructure(firstStructureToDestroy);
			if (tileObjectToAttackToDestroyStructure != null)
			{
				p_actor.combatComponent.Fight(tileObjectToAttackToDestroyStructure, "Raid");
				return true;
			}
		}
		return false;
	}

	private ManMadeStructure GetFirstStructureToDestroy(Character p_actor, BaseSettlement p_targetSettlement)
	{
		for (int i = 0; i < p_targetSettlement.allStructures.Count; i++)
		{
			LocationStructure locationStructure = p_targetSettlement.allStructures[i];
			if (locationStructure.structureType != STRUCTURE_TYPE.CITY_CENTER && locationStructure is ManMadeStructure result && !locationStructure.hasBeenDestroyed)
			{
				return result;
			}
		}
		return null;
	}

	private TileObject GetTileObjectToAttackToDestroyStructure(LocationStructure p_structure)
	{
		TileObject result = null;
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		for (int i = 0; i < p_structure.tiles.Count; i++)
		{
			LocationGridTile locationGridTile = p_structure.tiles.ElementAt(i);
			if (locationGridTile.IsPassable() && locationGridTile.tileObjectComponent.genericTileObject.currentHP > 0 && p_structure.DoesTileContributeToDamage(locationGridTile))
			{
				list.Add(locationGridTile.tileObjectComponent.genericTileObject);
			}
		}
		if (list.Count > 0)
		{
			result = list[Random.Range(0, list.Count)];
		}
		RuinarchListPool<TileObject>.Release(list);
		return result;
	}

	private bool DestroySupplies(Character p_actor, DemonRaidPartyQuest p_quest, ref string p_log)
	{
		BaseSettlement targetSettlement = p_quest.targetSettlement;
		if (p_actor.gridTileLocation != null)
		{
			ResourcePile nearestResourcePileFrom = targetSettlement.SettlementResources.GetNearestResourcePileFrom(p_actor.gridTileLocation, targetSettlement);
			if (nearestResourcePileFrom != null)
			{
				p_actor.combatComponent.Fight(nearestResourcePileFrom, "Raid");
				return true;
			}
		}
		return false;
	}

	private bool DestroyDefenses(Character p_actor, DemonRaidPartyQuest p_quest, ref string p_log)
	{
		BaseSettlement targetSettlement = p_quest.targetSettlement;
		if (p_actor.gridTileLocation != null)
		{
			TileObject randomTileObjectOfType = targetSettlement.GetRandomTileObjectOfType(TILE_OBJECT_TYPE.WARD_LIGHT);
			if (randomTileObjectOfType != null)
			{
				p_actor.combatComponent.Fight(randomTileObjectOfType, "Destroy_Defenses");
				return true;
			}
			List<LocationStructure> list = RuinarchListPool<LocationStructure>.Claim();
			if (targetSettlement.HasStructure(STRUCTURE_TYPE.ARROW_TOWER))
			{
				list.AddRange(targetSettlement.GetStructuresOfType(STRUCTURE_TYPE.ARROW_TOWER));
			}
			if (targetSettlement.HasStructure(STRUCTURE_TYPE.LIGHTNING_TOWER))
			{
				list.AddRange(targetSettlement.GetStructuresOfType(STRUCTURE_TYPE.LIGHTNING_TOWER));
			}
			LocationStructure locationStructure = null;
			if (list.Count > 0)
			{
				locationStructure = CollectionUtilities.GetRandomElement(list);
			}
			RuinarchListPool<LocationStructure>.Release(list);
			if (locationStructure != null)
			{
				TileObject tileObjectToAttackToDestroyStructure = GetTileObjectToAttackToDestroyStructure(locationStructure);
				if (tileObjectToAttackToDestroyStructure != null)
				{
					p_actor.combatComponent.Fight(tileObjectToAttackToDestroyStructure, "Destroy_Defenses");
					return true;
				}
			}
			ManMadeStructure firstStructureToDestroy = GetFirstStructureToDestroy(p_actor, targetSettlement);
			if (firstStructureToDestroy != null)
			{
				TileObject tileObjectToAttackToDestroyStructure2 = GetTileObjectToAttackToDestroyStructure(firstStructureToDestroy);
				if (tileObjectToAttackToDestroyStructure2 != null)
				{
					p_actor.combatComponent.Fight(tileObjectToAttackToDestroyStructure2, "Destroy_Defenses");
					return true;
				}
			}
		}
		return false;
	}
}

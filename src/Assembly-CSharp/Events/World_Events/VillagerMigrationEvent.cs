using System.Collections.Generic;
using Generator.Map_Generation.Components;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

namespace Events.World_Events;

public abstract class VillagerMigrationEvent : WorldEvent
{
	private WeightedDictionary<FACTION_TYPE> _factionTypeWeights;

	protected bool TryTriggerGenericMigrationEvent(int p_migrationChance, int p_migrationChanceIfNoHostileFactions)
	{
		if (FactionManager.Instance.GetActiveVillagerFactionCount() >= FactionManager.Instance.maxActiveVillagerFactions)
		{
			return false;
		}
		if (FactionManager.Instance.HasMajorFactionHostileWithPlayer())
		{
			if (GameUtilities.RollChance(p_migrationChance))
			{
				VillageSpot randomVillageSpotForVillagerMigration = GetRandomVillageSpotForVillagerMigration();
				if (randomVillageSpotForVillagerMigration != null)
				{
					FACTION_TYPE factionTypeToCreateBasedOnVillageSpot = GetFactionTypeToCreateBasedOnVillageSpot(randomVillageSpotForVillagerMigration);
					if (factionTypeToCreateBasedOnVillageSpot == FACTION_TYPE.None)
					{
						return false;
					}
					int migrantCount = GetMigrantCount();
					List<PreCharacterData> list = RuinarchListPool<PreCharacterData>.Claim();
					RACE raceForFactionType = factionTypeToCreateBasedOnVillageSpot.GetRaceForFactionType(randomizeDefault: true);
					DatabaseManager.Instance.familyTreeDatabase.ForcePopulateAllUnspawnedCharactersThatFitRace(list, raceForFactionType);
					if (list.Count > 0)
					{
						Faction faction = FactionManager.Instance.CreateNewFaction(factionTypeToCreateBasedOnVillageSpot, "", null, raceForFactionType);
						faction.factionType.SetAsDefault(faction);
						if (randomVillageSpotForVillagerMigration.IsOccupiedByVillage(out var p_settlement))
						{
							LandmarkManager.Instance.OwnSettlement(faction, p_settlement);
							Migrate(list, faction, migrantCount, p_settlement, randomVillageSpotForVillagerMigration);
							faction.successionComponent.UpdateSuccessors();
							faction.DesignateNewLeader(willLog: false);
						}
						else
						{
							Migrate(list, faction, migrantCount, null, randomVillageSpotForVillagerMigration);
							faction.successionComponent.UpdateSuccessors();
							faction.DesignateNewLeader(willLog: false);
							for (int i = 0; i < faction.characters.Count; i++)
							{
								Character character = faction.characters[i];
								character.SetTerritory(randomVillageSpotForVillagerMigration.coreSpot, returnHome: false);
								if (character == faction.leader)
								{
									character.behaviourComponent.MakeBuildVillagePriority(randomVillageSpotForVillagerMigration);
									Area coreSpot = randomVillageSpotForVillagerMigration.coreSpot;
									StructureSetting structureSetting = new StructureSetting(STRUCTURE_TYPE.CITY_CENTER, faction.factionType.mainResource);
									GameObject randomElement = CollectionUtilities.GetRandomElement(InnerMapManager.Instance.GetStructurePrefabsForStructure(faction.factionType.type, structureSetting));
									if (LandmarkManager.Instance.HasEnoughSpaceForStructure(randomElement.name, coreSpot.gridTileComponent.centerGridTile))
									{
										character.jobComponent.TriggerFindNewVillage(coreSpot.gridTileComponent.centerGridTile, randomElement.name);
									}
								}
								else
								{
									LocationGridTile randomPassableTile = randomVillageSpotForVillagerMigration.coreSpot.GetRandomPassableTile();
									JobQueueItem producedJob;
									if (randomPassableTile != null)
									{
										character.jobComponent.CreateGoToJob(randomPassableTile);
									}
									else if (character.jobComponent.TriggerMoveToArea(JOB_TYPE.RETURN_HOME_URGENT, out producedJob, randomVillageSpotForVillagerMigration.coreSpot))
									{
										character.jobQueue.AddJobInQueue(producedJob);
									}
								}
								CharacterFinalization.ApplyFactionTypeRelatedEffectToMember(faction, character);
							}
						}
						Messenger.Broadcast(FactionSignals.FORCE_FACTION_UI_RELOAD);
						RuinarchListPool<PreCharacterData>.Release(list);
						return true;
					}
					RuinarchListPool<PreCharacterData>.Release(list);
					return false;
				}
			}
		}
		else if (GameUtilities.RollChance(p_migrationChanceIfNoHostileFactions))
		{
			NPCSettlement nPCSettlement = null;
			if (GameUtilities.RollChance(50))
			{
				nPCSettlement = GetRandomUnoccupiedVillage();
			}
			int migrantCount2 = GetMigrantCount();
			List<PreCharacterData> list2 = RuinarchListPool<PreCharacterData>.Claim();
			RACE race = RACE.HUMANS;
			if (GameUtilities.RollChance(50))
			{
				race = RACE.ELVES;
			}
			DatabaseManager.Instance.familyTreeDatabase.ForcePopulateAllUnspawnedCharactersThatFitRace(list2, race);
			if (list2.Count > 0)
			{
				Faction faction2 = FactionManager.Instance.CreateNewFaction(FactionManager.Instance.GetFactionTypeForRace(race), "", null, race);
				faction2.factionType.SetAsDefault(faction2);
				faction2.SetLeader(null);
				if (nPCSettlement != null)
				{
					LandmarkManager.Instance.OwnSettlement(faction2, nPCSettlement);
				}
				Migrate(list2, faction2, migrantCount2, nPCSettlement, nPCSettlement?.occupiedVillageSpot);
				Messenger.Broadcast(FactionSignals.FORCE_FACTION_UI_RELOAD);
				RuinarchListPool<PreCharacterData>.Release(list2);
				return true;
			}
			RuinarchListPool<PreCharacterData>.Release(list2);
			return false;
		}
		return false;
	}

	protected VillageSpot GetRandomVillageSpotForVillagerMigration()
	{
		List<VillageSpot> list = RuinarchListPool<VillageSpot>.Claim();
		if (GameUtilities.RollChance(50))
		{
			for (int i = 0; i < GridMap.Instance.mainRegion.villageSpots.Count; i++)
			{
				VillageSpot villageSpot = GridMap.Instance.mainRegion.villageSpots[i];
				if (!villageSpot.isDisabled && !villageSpot.IsOccupiedByVillage(out var _))
				{
					list.Add(villageSpot);
				}
			}
		}
		if (list.Count <= 0)
		{
			for (int j = 0; j < GridMap.Instance.mainRegion.villageSpots.Count; j++)
			{
				VillageSpot villageSpot2 = GridMap.Instance.mainRegion.villageSpots[j];
				if (villageSpot2.isDisabled)
				{
					continue;
				}
				if (villageSpot2.IsOccupiedByVillage(out var p_settlement2))
				{
					if (p_settlement2.locationType == LOCATION_TYPE.VILLAGE && !p_settlement2.HasResidents())
					{
						list.Add(villageSpot2);
					}
				}
				else
				{
					list.Add(villageSpot2);
				}
			}
		}
		VillageSpot result = null;
		if (list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<VillageSpot>.Release(list);
		return result;
	}

	protected NPCSettlement GetRandomUnoccupiedVillage()
	{
		List<NPCSettlement> list = RuinarchListPool<NPCSettlement>.Claim();
		for (int i = 0; i < DatabaseManager.Instance.settlementDatabase.allNonPlayerSettlements.Count; i++)
		{
			NPCSettlement nPCSettlement = DatabaseManager.Instance.settlementDatabase.allNonPlayerSettlements[i];
			if (nPCSettlement.locationType == LOCATION_TYPE.VILLAGE && nPCSettlement.residents.Count <= 0)
			{
				list.Add(nPCSettlement);
			}
		}
		NPCSettlement result = null;
		if (list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<NPCSettlement>.Release(list);
		return result;
	}

	protected FACTION_TYPE GetFactionTypeToCreateBasedOnVillageSpot(VillageSpot p_villageSpot)
	{
		if (_factionTypeWeights == null)
		{
			_factionTypeWeights = new WeightedDictionary<FACTION_TYPE>();
		}
		else
		{
			_factionTypeWeights.Clear();
		}
		if (p_villageSpot.CanAccommodateFaction(FACTION_TYPE.Elven_Kingdom))
		{
			_factionTypeWeights.AddElement(FACTION_TYPE.Elven_Kingdom, 100);
		}
		if (p_villageSpot.CanAccommodateFaction(FACTION_TYPE.Human_Empire))
		{
			_factionTypeWeights.AddElement(FACTION_TYPE.Human_Empire, 100);
		}
		return _factionTypeWeights.PickRandomElementGivenWeights();
	}

	protected int GetMigrantCount()
	{
		if (WorldSettings.Instance.worldSettingsData.victoryCondition == VICTORY_CONDITION.Eradication)
		{
			return 3;
		}
		ThePortal thePortal = PlayerManager.Instance.player.playerSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal;
		switch ((WorldSettings.Instance.worldSettingsData.playerSkillSettings.omnipotentMode == OMNIPOTENT_MODE.Enabled) ? 4 : thePortal.level)
		{
		case 1:
		case 2:
		case 3:
			return 5;
		case 4:
		case 5:
			return 8;
		case 6:
		case 7:
		case 8:
			return 12;
		default:
			return 0;
		}
	}

	protected void Migrate(List<PreCharacterData> unspawnedCharacters, Faction faction, int randomAmount, NPCSettlement homeSettlement, VillageSpot villageSpot)
	{
		LocationGridTile tile = ((villageSpot == null) ? GridMap.Instance.mainRegion.innerMap.GetRandomPassableEdgeTile() : villageSpot.GetRandomMigrationSpawningTile());
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < randomAmount; i++)
		{
			if (unspawnedCharacters.Count <= 0)
			{
				break;
			}
			PreCharacterData randomElement = CollectionUtilities.GetRandomElement(unspawnedCharacters);
			randomElement.hasBeenSpawned = true;
			unspawnedCharacters.Remove(randomElement);
			string className = "Farmer";
			Character character = CharacterManager.Instance.CreateNewCharacter(randomElement, className, faction, homeSettlement);
			if (ChanceData.RollChance(CHANCE_TYPE.Noble_Chance))
			{
				character.classComponent.AssignClass("Noble", isInitial: true);
				character.classComponent.OnUpdateCharacterClass();
			}
			else
			{
				character.classComponent.RandomizeCurrentClassBasedOnAbleClasses();
			}
			RelationshipManager.Instance.ApplyPreGeneratedRelationships(randomElement, character);
			character.CreateRandomInitialTraits();
			if (WorldSettings.Instance.worldSettingsData.villageSettings.blessedMigrants)
			{
				character.traitContainer.AddTrait(character, "Blessed");
			}
			character.CreateMarker();
			character.InitialCharacterPlacement(tile);
			if (character.homeSettlement != null)
			{
				character.jobComponent.PlanReturnToVillageCenter(JOB_TYPE.RETURN_HOME_URGENT);
			}
			list.Add(character);
			Messenger.Broadcast(WorldEventSignals.NEW_VILLAGER_ARRIVED, character);
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "WorldEvents", "WorldEvents_Table", "new_villager", LOG_TAG.Major);
			log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(GridMap.Instance.mainRegion, GridMap.Instance.mainRegion.name, LOG_IDENTIFIER.LANDMARK_1);
			log.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
		}
		if (ChanceData.RollChance(CHANCE_TYPE.Nullchild_Migrant))
		{
			Character randomElement2 = CollectionUtilities.GetRandomElement(list);
			randomElement2?.traitContainer.AddTrait(randomElement2, "Nullchild");
		}
		if (ChanceData.RollChance(CHANCE_TYPE.Jinxed_Migrant))
		{
			Character randomElement3 = CollectionUtilities.GetRandomElement(list);
			randomElement3?.traitContainer.AddTrait(randomElement3, "Jinxed");
		}
		RuinarchListPool<Character>.Release(list);
		AkSoundEngine.PostEvent("Play_New_Immigrants", InnerMapCameraMove.Instance.gameObject);
	}
}

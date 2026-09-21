using System.Collections.Generic;
using System.Linq;
using Factions.Faction_Types;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Traits;
using UtilityScripts;

public class SettlementPartyComponent : NPCSettlementComponent
{
	public GameDate scheduleDateForProcessingOfPartyQuests { get; private set; }

	public SettlementPartyComponent()
	{
	}

	public SettlementPartyComponent(SaveDataSettlementPartyComponent data)
	{
		scheduleDateForProcessingOfPartyQuests = data.scheduleDateForProcessingOfPartyQuests;
	}

	public void InitialScheduleProcessingOfPartyQuests()
	{
		if (base.owner.locationType == LOCATION_TYPE.VILLAGE || base.owner.locationType == LOCATION_TYPE.PSEUDO_VILLAGE)
		{
			int ticksBasedOnHour = GameManager.Instance.GetTicksBasedOnHour(2);
			int ticksBasedOnHour2 = GameManager.Instance.GetTicksBasedOnHour(5);
			int ticks = GameUtilities.RandomBetweenTwoNumbers(ticksBasedOnHour, ticksBasedOnHour2);
			GameDate gameDate = GameManager.Instance.Today().AddDays(1);
			gameDate.SetTicks(ticks);
			scheduleDateForProcessingOfPartyQuests = gameDate;
			SchedulingManager.Instance.AddEntry(scheduleDateForProcessingOfPartyQuests, ProcessingOfPartyQuests, base.owner);
		}
	}

	private void ProcessingOfPartyQuests()
	{
		if (base.owner.HasResidentThatIsNotDead())
		{
			ProcessPartyQuests();
		}
		scheduleDateForProcessingOfPartyQuests = GameManager.Instance.Today().AddDays(1);
		SchedulingManager.Instance.AddEntry(scheduleDateForProcessingOfPartyQuests, ProcessingOfPartyQuests, base.owner);
	}

	private void ProcessPartyQuests()
	{
		if (base.owner.owner != null && base.owner.owner.factionType.HasIdeology(FACTION_IDEOLOGY.Raiders))
		{
			ProcessPartyQuestsForRaiderFaction();
		}
		else
		{
			ProcessPartyQuestsForNormalFaction();
		}
		if (base.owner.owner != null)
		{
			if (base.owner.owner.factionType.type == FACTION_TYPE.Vampire_Clan)
			{
				ProcessPartyQuestsForVampireClan();
			}
			else if (base.owner.owner.factionType is CultFaction)
			{
				ProcessPartyQuestsForReligiousCult();
			}
		}
	}

	private void ProcessPartyQuestsForVampireClan()
	{
		if (ChanceData.RollChance(CHANCE_TYPE.Spawn_Blood_Hunt))
		{
			base.owner.owner.partyQuestBoard.CreateBloodHuntPartyQuest(base.owner, base.owner.region);
		}
		if (ChanceData.RollChance(CHANCE_TYPE.Spawn_Recruit_Vampires))
		{
			base.owner.owner.partyQuestBoard.CreateRecruitVampiresPartyQuest(base.owner, base.owner.region);
		}
	}

	private void ProcessPartyQuestsForReligiousCult()
	{
		if (base.owner.region.GetRandomStructureOfType(STRUCTURE_TYPE.HALLOWED_GROUND) is Inner_Maps.Location_Structures.HallowedGround hallowedGround && hallowedGround.IsNearHallowedGrounds(base.owner))
		{
			if (ChanceData.RollChance(CHANCE_TYPE.Spawn_Claim_Hallowed_Ground) && !base.owner.owner.partyQuestBoard.HasPartyQuest(PARTY_QUEST_TYPE.Claim_Hallowed_Ground))
			{
				base.owner.owner.partyQuestBoard.CreateClaimHallowedGroundQuest(base.owner, base.owner.region, hallowedGround);
			}
			if (ChanceData.RollChance(CHANCE_TYPE.Spawn_Defend_Hallowed_Ground) && !base.owner.owner.partyQuestBoard.HasPartyQuest(PARTY_QUEST_TYPE.Defend_Hallowed_Ground))
			{
				base.owner.owner.partyQuestBoard.CreateDefendHallowedGroundQuest(base.owner, base.owner.region, hallowedGround);
			}
		}
		if (base.owner.owner == null || !ChanceData.RollChance(CHANCE_TYPE.Religious_Cult_Raid) || base.owner.owner.partyQuestBoard.HasPartyQuest(PARTY_QUEST_TYPE.Raid))
		{
			return;
		}
		List<BaseSettlement> list = RuinarchListPool<BaseSettlement>.Claim();
		foreach (KeyValuePair<Faction, FactionRelationship> relationship in base.owner.owner.relationships)
		{
			if (!(relationship.Key.factionType is CultFaction) || relationship.Key.ownedSettlements.Count <= 0 || relationship.Value.relationshipStatus != FACTION_RELATIONSHIP_STATUS.Hostile)
			{
				continue;
			}
			for (int i = 0; i < relationship.Key.ownedSettlements.Count; i++)
			{
				BaseSettlement baseSettlement = relationship.Key.ownedSettlements[i];
				if (baseSettlement.residents.Count((Character r) => !r.traitContainer.HasTrait("Restrained") && !r.traitContainer.HasTrait("Prisoner")) > 0)
				{
					list.Add(baseSettlement);
				}
			}
		}
		if (list.Count > 0)
		{
			BaseSettlement randomElement = CollectionUtilities.GetRandomElement(list);
			base.owner.owner.partyQuestBoard.CreateRaidPartyQuest(null, null, randomElement);
		}
	}

	private void ProcessPartyQuestsForNormalFaction()
	{
		string log = string.Empty;
		int partyCount = base.owner.GetPartyCount();
		Faction faction = base.owner.owner;
		if (faction != null)
		{
			TryCreateExploreQuest(faction, partyCount, ref log);
			TryCreateMorningPatrolQuest(faction, partyCount, ref log);
			TryCreateRaidQuest(faction, ref log);
			TryCreateRescueQuest(faction, partyCount, ref log);
			TryCreateExterminateQuest(faction, ref log);
			TryCreateHuntBeastQuest(faction, ref log);
			TryCreateBountyHuntQuest(faction, ref log);
			TryCreateCounterattackQuest(faction, ref log);
		}
	}

	private void ProcessPartyQuestsForRaiderFaction()
	{
		string log = string.Empty;
		int partyCount = base.owner.GetPartyCount();
		Faction faction = base.owner.owner;
		if (faction != null)
		{
			int numberOfFoodInBanditCamp = base.owner.GetNumberOfFoodInBanditCamp();
			TryCreateRaidersRaidQuest(faction, numberOfFoodInBanditCamp, ref log);
			TryCreateRescueQuest(faction, partyCount, ref log);
			TryCreateRaidersHuntBeastQuest(faction, numberOfFoodInBanditCamp, ref log);
			TryCreateCounterattackQuest(faction, ref log);
		}
	}

	private void TryCreateExploreQuest(Faction p_faction, int p_villagePartyCount, ref string log)
	{
		int chance = 50;
		if (p_villagePartyCount >= 3)
		{
			chance = 100;
		}
		if (GameUtilities.RollChance(chance, ref log) && !p_faction.partyQuestBoard.HasPartyQuest(PARTY_QUEST_TYPE.Exploration))
		{
			p_faction.partyQuestBoard.CreateExplorationPartyQuest(null, base.owner, base.owner.region);
		}
	}

	private void TryCreateMorningPatrolQuest(Faction p_faction, int p_villagePartyCount, ref string log)
	{
		int chance = 50;
		if (p_villagePartyCount >= 3)
		{
			chance = 100;
		}
		if (GameUtilities.RollChance(chance, ref log) && !p_faction.partyQuestBoard.HasPartyQuest(PARTY_QUEST_TYPE.Morning_Patrol))
		{
			p_faction.partyQuestBoard.CreateMorningPatrolPartyQuest(null, base.owner);
		}
	}

	private void TryCreateRaidQuest(Faction p_faction, ref string log)
	{
		if (!p_faction.factionType.HasIdeology(FACTION_IDEOLOGY.Warmonger) || !p_faction.IsAtWar())
		{
			return;
		}
		Faction randomAtWarMajorNonPlayerFaction = p_faction.GetRandomAtWarMajorNonPlayerFaction();
		if (randomAtWarMajorNonPlayerFaction == null)
		{
			return;
		}
		BaseSettlement randomOwnedVillage = randomAtWarMajorNonPlayerFaction.GetRandomOwnedVillage();
		if (randomOwnedVillage != null)
		{
			int chance = ChanceData.GetChance(CHANCE_TYPE.Raid_Chance);
			if (base.owner.GetNumberOfFoodInAllFoodProducingStructures() < 50)
			{
				chance = 100;
			}
			else if (base.owner.GetNumberOfMainBasicResourceInAllResourceProducingStructures() < 50)
			{
				chance = 100;
			}
			if (GameUtilities.RollChance(chance, ref log) && !p_faction.partyQuestBoard.HasPartyQuest(PARTY_QUEST_TYPE.Raid))
			{
				p_faction.partyQuestBoard.CreateRaidPartyQuest(null, base.owner, randomOwnedVillage);
			}
		}
	}

	private void TryCreateRaidersRaidQuest(Faction p_faction, int p_numberOfFood, ref string log)
	{
		Faction randomNotFriendlyMajorNonPlayerFaction = p_faction.GetRandomNotFriendlyMajorNonPlayerFaction();
		if (randomNotFriendlyMajorNonPlayerFaction == null)
		{
			return;
		}
		BaseSettlement randomOwnedVillage = randomNotFriendlyMajorNonPlayerFaction.GetRandomOwnedVillage();
		if (randomOwnedVillage != null)
		{
			int chance = ChanceData.GetChance(CHANCE_TYPE.Raid_Chance);
			if (p_numberOfFood < 50)
			{
				chance = 100;
			}
			else if (base.owner.GetNumberOfMainBasicResourceInBanditCamp() < 50)
			{
				chance = 100;
			}
			if (GameUtilities.RollChance(chance, ref log) && !p_faction.partyQuestBoard.HasPartyQuest(PARTY_QUEST_TYPE.Raid))
			{
				p_faction.partyQuestBoard.CreateRaidPartyQuest(null, base.owner, randomOwnedVillage);
			}
		}
	}

	private void TryCreateRescueQuest(Faction p_faction, int p_villagePartyCount, ref string log)
	{
		int chance = 50;
		if (p_villagePartyCount >= 2)
		{
			chance = 100;
		}
		if (GameUtilities.RollChance(chance, ref log) && !p_faction.partyQuestBoard.HasPartyQuest(PARTY_QUEST_TYPE.Rescue) && !p_faction.partyQuestBoard.HasPartyQuest(PARTY_QUEST_TYPE.Demon_Rescue))
		{
			Character randomResidentForRescue = base.owner.GetRandomResidentForRescue();
			if (randomResidentForRescue != null)
			{
				p_faction.partyQuestBoard.CreateRescuePartyQuest(null, base.owner, randomResidentForRescue);
			}
		}
	}

	private void TryCreateExterminateQuest(Faction p_faction, ref string log)
	{
		int chance = 50;
		if (base.owner.structureComponent.HasLinkedStructureWithMonsterSpawner())
		{
			chance = 85;
		}
		if (GameUtilities.RollChance(chance, ref log) && !p_faction.partyQuestBoard.HasPartyQuest(PARTY_QUEST_TYPE.Extermination))
		{
			LocationStructure randomLinkedStructureForExtermination = base.owner.structureComponent.GetRandomLinkedStructureForExtermination();
			if (randomLinkedStructureForExtermination != null)
			{
				p_faction.partyQuestBoard.CreateExterminatePartyQuest(null, base.owner, randomLinkedStructureForExtermination);
			}
		}
	}

	private void TryCreateBountyHuntQuest(Faction p_faction, ref string log)
	{
		List<Character> list = RuinarchListPool<Character>.Claim();
		Character character = null;
		for (int i = 0; i < p_faction.crimeComponent.wantedCharacters.Count; i++)
		{
			Character character2 = p_faction.crimeComponent.wantedCharacters[i];
			if (!character2.crimeComponent.IsWantedBy(p_faction) || character2.isDead || character2.faction == p_faction)
			{
				continue;
			}
			LocationStructure currentStructure = character2.currentStructure;
			if ((currentStructure == null || !currentStructure.structureType.IsPlayerStructure() || p_faction.isAwareOfPlayer) && !p_faction.partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Bounty_Hunt, character2))
			{
				Prisoner traitOrStatus = character2.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
				if (traitOrStatus == null || (!traitOrStatus.IsFactionPrisonerOf(p_faction) && !traitOrStatus.IsFactionPrisonerOf(character2.faction)))
				{
					list.Add(character2);
				}
			}
		}
		if (list.Count > 0)
		{
			character = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
		}
		RuinarchListPool<Character>.Release(list);
		if (character != null)
		{
			p_faction.partyQuestBoard.CreateBountyHuntPartyQuest(null, base.owner, character);
		}
	}

	private void TryCreateHuntBeastQuest(Faction p_faction, ref string log)
	{
		if ((base.owner.HasStructure(STRUCTURE_TYPE.BUTCHERS_SHOP) || base.owner.HasBlueprintOnTileForStructure(STRUCTURE_TYPE.BUTCHERS_SHOP) || base.owner.HasStructure(STRUCTURE_TYPE.HUNTER_LODGE) || base.owner.HasBlueprintOnTileForStructure(STRUCTURE_TYPE.HUNTER_LODGE)) && base.owner.occupiedVillageSpot != null && !p_faction.partyQuestBoard.HasPartyQuest(PARTY_QUEST_TYPE.Hunt_Beast))
		{
			LocationStructure randomLinkedAliveBeastDen = base.owner.occupiedVillageSpot.GetRandomLinkedAliveBeastDen();
			if (randomLinkedAliveBeastDen != null)
			{
				p_faction.partyQuestBoard.CreateHuntBeastPartyQuest(null, base.owner, randomLinkedAliveBeastDen);
			}
		}
	}

	private void TryCreateRaidersHuntBeastQuest(Faction p_faction, int p_numberOfFood, ref string log)
	{
		if (p_numberOfFood >= 50 || p_faction.partyQuestBoard.HasPartyQuest(PARTY_QUEST_TYPE.Hunt_Beast))
		{
			return;
		}
		Area area = base.owner.GetFirstStructureOfType(STRUCTURE_TYPE.BANDIT_CAMP)?.occupiedArea;
		if (area == null)
		{
			return;
		}
		List<Area> list = RuinarchListPool<Area>.Claim();
		List<LocationStructure> list2 = RuinarchListPool<LocationStructure>.Claim();
		area.PopulateAreasInRange(list, 6, includeCenterTile: true);
		LocationStructure locationStructure = null;
		for (int i = 0; i < list.Count; i++)
		{
			AnimalBurrow firstTileObject = list[i].tileObjectComponent.GetFirstTileObject<AnimalBurrow>();
			if (firstTileObject != null && firstTileObject.structureLocation != null && firstTileObject.HasAliveSpawnedMonster())
			{
				list2.Add(firstTileObject.structureLocation);
			}
		}
		if (list2.Count > 0)
		{
			locationStructure = list2[GameUtilities.RandomBetweenTwoNumbers(0, list2.Count - 1)];
		}
		RuinarchListPool<Area>.Release(list);
		RuinarchListPool<LocationStructure>.Release(list2);
		if (locationStructure != null)
		{
			p_faction.partyQuestBoard.CreateHuntBeastPartyQuest(null, base.owner, locationStructure);
		}
	}

	private void TryCreateCounterattackQuest(Faction p_faction, ref string log)
	{
		if (!p_faction.IsHostileWith(PlayerManager.Instance.player.playerFaction) || p_faction.partyQuestBoard.HasPartyQuest(PARTY_QUEST_TYPE.Counterattack))
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < base.owner.areas.Count; i++)
		{
			Area area = base.owner.areas[i];
			for (int j = 0; j < area.neighbourComponent.neighbours.Count; j++)
			{
				if (area.neighbourComponent.neighbours[j].HasPlayerSettlement())
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				break;
			}
		}
		if (flag)
		{
			if (!p_faction.isAwareOfPlayer)
			{
				p_faction.SetIsAwareOfPlayer(p_state: true);
			}
			p_faction.partyQuestBoard.CreateCounterattackPartyQuest(null, base.owner);
		}
	}

	public void LoadReferences(SaveDataSettlementPartyComponent data)
	{
		if (base.owner.locationType == LOCATION_TYPE.VILLAGE || base.owner.locationType == LOCATION_TYPE.PSEUDO_VILLAGE)
		{
			SchedulingManager.Instance.AddEntry(scheduleDateForProcessingOfPartyQuests, ProcessingOfPartyQuests, base.owner);
		}
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}
}

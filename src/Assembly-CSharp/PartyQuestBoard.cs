using System.Collections.Generic;
using System.Linq;
using Factions.Faction_Types;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UtilityScripts;

public class PartyQuestBoard
{
	public Faction owner { get; private set; }

	public List<PartyQuest> availablePartyQuests { get; protected set; }

	public PartyQuestBoard(Faction owner)
	{
		this.owner = owner;
		availablePartyQuests = new List<PartyQuest>();
	}

	public PartyQuestBoard(SaveDataPartyQuestBoard data)
	{
		availablePartyQuests = new List<PartyQuest>();
	}

	public PartyQuest GetFirstPriorityUnassignedPartyQuestFor(Party party, Character memberThatWillAcceptQuest)
	{
		PartyQuest partyQuest = null;
		int num = 0;
		for (int i = 0; i < availablePartyQuests.Count; i++)
		{
			PartyQuest partyQuest2 = availablePartyQuests[i];
			if (!partyQuest2.isAssigned && partyQuest2.IsInterestedInJoiningQuest(memberThatWillAcceptQuest))
			{
				bool num2 = party.members.Count >= partyQuest2.minimumPartySize;
				bool flag = partyQuest2.madeInLocation != null && partyQuest2.madeInLocation == party.partySettlement;
				if (((num2 && (flag || partyQuest2.isFactionWideQuest)) || party.isPlayerParty) && (partyQuest == null || num < partyQuest2.priority))
				{
					partyQuest = partyQuest2;
					num = partyQuest2.priority;
				}
			}
		}
		return partyQuest;
	}

	public void RemoveAllNoLongerFitQuests()
	{
		for (int i = 0; i < availablePartyQuests.Count; i++)
		{
			PartyQuest partyQuest = availablePartyQuests[i];
			if (!partyQuest.IsStillEligibleFor(owner) && partyQuest.assignedParty == null)
			{
				RemovePartyQuest(partyQuest);
				i--;
			}
		}
	}

	public void RemoveAllQuestsThatAreMadeInLocation(BaseSettlement p_settlement)
	{
		List<PartyQuest> list = RuinarchListPool<PartyQuest>.Claim();
		list.AddRange(availablePartyQuests);
		for (int i = 0; i < list.Count; i++)
		{
			PartyQuest partyQuest = list[i];
			if (!partyQuest.isFactionWideQuest && partyQuest.madeInLocation == p_settlement && partyQuest.assignedParty == null)
			{
				RemovePartyQuest(partyQuest);
			}
		}
		RuinarchListPool<PartyQuest>.Release(list);
	}

	public bool HasPartyQuest(PARTY_QUEST_TYPE questType)
	{
		return GetPartyQuest(questType) != null;
	}

	public bool HasPartyQuestWithTarget(PARTY_QUEST_TYPE questType, IPartyQuestTarget target)
	{
		return GetPartyQuestWithTarget(questType, target) != null;
	}

	public PartyQuest GetPartyQuestWithTarget(PARTY_QUEST_TYPE questType, IPartyQuestTarget target)
	{
		for (int i = 0; i < availablePartyQuests.Count; i++)
		{
			PartyQuest partyQuest = availablePartyQuests[i];
			if (partyQuest.partyQuestType == questType && partyQuest.target == target)
			{
				return partyQuest;
			}
		}
		return null;
	}

	public PartyQuest GetPartyQuest(PARTY_QUEST_TYPE questType)
	{
		for (int i = 0; i < availablePartyQuests.Count; i++)
		{
			PartyQuest partyQuest = availablePartyQuests[i];
			if (partyQuest.partyQuestType == questType)
			{
				return partyQuest;
			}
		}
		return null;
	}

	public void AddPartyQuest(PartyQuest quest, Character questCreator)
	{
		if (!availablePartyQuests.Contains(quest))
		{
			availablePartyQuests.Add(quest);
			quest.OnPostQuestToBoard(owner);
			Messenger.Broadcast(FactionSignals.PARTY_QUEST_ADDED, quest, owner);
		}
	}

	public bool RemovePartyQuest(PartyQuest quest)
	{
		if (availablePartyQuests.Remove(quest))
		{
			quest.OnQuestRemovedFromBoard();
			Messenger.Broadcast(FactionSignals.PARTY_QUEST_REMOVED, quest, owner);
			return true;
		}
		return false;
	}

	public void CreateExplorationPartyQuest(Character questCreator, BaseSettlement madeInLocation, Region region)
	{
		if (owner.isMajorOrBandits)
		{
			LocationStructure randomSpecialStructure = region.GetRandomSpecialStructure();
			if (randomSpecialStructure != null)
			{
				ExplorationPartyQuest explorationPartyQuest = PartyManager.Instance.CreateNewPartyQuest(PARTY_QUEST_TYPE.Exploration) as ExplorationPartyQuest;
				explorationPartyQuest.SetMadeInLocation(madeInLocation);
				explorationPartyQuest.SetTargetStructure(randomSpecialStructure);
				explorationPartyQuest.SetQuestCreator(questCreator);
				AddPartyQuest(explorationPartyQuest, questCreator);
			}
		}
	}

	public void CreateRescuePartyQuest(Character questCreator, BaseSettlement madeInLocation, Character targetCharacter)
	{
		if (!owner.isMajorOrBandits)
		{
			return;
		}
		LocationStructure currentStructure = targetCharacter.currentStructure;
		if (currentStructure != null && currentStructure.structureType.IsPlayerStructure())
		{
			if (owner.factionType.type != FACTION_TYPE.Demons && owner.factionType.type != FACTION_TYPE.Demon_Cult)
			{
				if (owner.isAwareOfPlayer)
				{
					CreateDemonRescuePartyQuest(questCreator, madeInLocation, targetCharacter, currentStructure as DemonicStructure);
				}
				else if (madeInLocation is NPCSettlement nPCSettlement)
				{
					nPCSettlement.settlementJobTriggerComponent.CreateSearchForDemonicAreaJob();
				}
			}
		}
		else
		{
			RescuePartyQuest rescuePartyQuest = PartyManager.Instance.CreateNewPartyQuest(PARTY_QUEST_TYPE.Rescue) as RescuePartyQuest;
			rescuePartyQuest.SetMadeInLocation(madeInLocation);
			rescuePartyQuest.SetTargetCharacter(targetCharacter);
			rescuePartyQuest.SetQuestCreator(questCreator);
			AddPartyQuest(rescuePartyQuest, questCreator);
		}
	}

	public void CreateDemonRescuePartyQuest(Character questCreator, BaseSettlement madeInLocation, Character targetCharacter, DemonicStructure targetDemonicStructure)
	{
		if (owner.isMajorOrBandits)
		{
			DemonRescuePartyQuest demonRescuePartyQuest = PartyManager.Instance.CreateNewPartyQuest(PARTY_QUEST_TYPE.Demon_Rescue) as DemonRescuePartyQuest;
			demonRescuePartyQuest.SetMadeInLocation(madeInLocation);
			demonRescuePartyQuest.SetTargetCharacter(targetCharacter);
			demonRescuePartyQuest.SetTargetDemonicStructure(targetDemonicStructure);
			demonRescuePartyQuest.SetQuestCreator(questCreator);
			AddPartyQuest(demonRescuePartyQuest, questCreator);
		}
	}

	public void CreateExterminatePartyQuest(Character questCreator, BaseSettlement madeInLocation, LocationStructure targetStructure)
	{
		if (owner.isMajorOrBandits)
		{
			ExterminationPartyQuest exterminationPartyQuest = PartyManager.Instance.CreateNewPartyQuest(PARTY_QUEST_TYPE.Extermination) as ExterminationPartyQuest;
			exterminationPartyQuest.SetMadeInLocation(madeInLocation);
			exterminationPartyQuest.SetTargetStructure(targetStructure);
			exterminationPartyQuest.SetQuestCreator(questCreator);
			AddPartyQuest(exterminationPartyQuest, questCreator);
		}
	}

	public void CreateCounterattackPartyQuest(Character questCreator, BaseSettlement madeInLocation)
	{
		if (owner.isMajorOrBandits)
		{
			CounterattackPartyQuest counterattackPartyQuest = PartyManager.Instance.CreateNewPartyQuest(PARTY_QUEST_TYPE.Counterattack) as CounterattackPartyQuest;
			counterattackPartyQuest.SetMadeInLocation(madeInLocation);
			counterattackPartyQuest.SetQuestCreator(questCreator);
			int num = 1;
			int num2 = ((PlayerManager.Instance.player.playerSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal).level - 1) / 2;
			if (num2 > 0)
			{
				num += num2;
			}
			counterattackPartyQuest.SetNumberOfStructuresToBeDestroyed(num);
			AddPartyQuest(counterattackPartyQuest, questCreator);
		}
	}

	public void CreateRaidPartyQuest(Character questCreator, BaseSettlement madeInLocation, BaseSettlement targetSettlement)
	{
		if (owner.isMajorOrBandits)
		{
			RaidPartyQuest raidPartyQuest = PartyManager.Instance.CreateNewPartyQuest(PARTY_QUEST_TYPE.Raid) as RaidPartyQuest;
			raidPartyQuest.SetMadeInLocation(madeInLocation);
			raidPartyQuest.SetTargetSettlement(targetSettlement);
			raidPartyQuest.SetQuestCreator(questCreator);
			AddPartyQuest(raidPartyQuest, questCreator);
		}
	}

	public PartyQuest CreateDemonRaidPartyQuest(Character questCreator, BaseSettlement madeInLocation, BaseSettlement targetSettlement)
	{
		if (!owner.isMajorOrBandits)
		{
			return null;
		}
		DemonRaidPartyQuest demonRaidPartyQuest = PartyManager.Instance.CreateNewPartyQuest(PARTY_QUEST_TYPE.Demon_Raid) as DemonRaidPartyQuest;
		demonRaidPartyQuest.SetMadeInLocation(madeInLocation);
		demonRaidPartyQuest.SetTargetSettlement(targetSettlement);
		demonRaidPartyQuest.SetQuestCreator(questCreator);
		AddPartyQuest(demonRaidPartyQuest, questCreator);
		return demonRaidPartyQuest;
	}

	public PartyQuest CreateDemonDefendPartyQuest(Character questCreator, BaseSettlement madeInLocation)
	{
		if (!owner.isMajorOrBandits)
		{
			return null;
		}
		DemonDefendPartyQuest demonDefendPartyQuest = PartyManager.Instance.CreateNewPartyQuest(PARTY_QUEST_TYPE.Demon_Defend) as DemonDefendPartyQuest;
		demonDefendPartyQuest.SetMadeInLocation(madeInLocation);
		demonDefendPartyQuest.SetQuestCreator(questCreator);
		AddPartyQuest(demonDefendPartyQuest, questCreator);
		return demonDefendPartyQuest;
	}

	public PartyQuest CreateDemonSnatchPartyQuest(Character questCreator, BaseSettlement madeInLocation, Character targetCharacter, LocationStructure dropStructure)
	{
		if (!owner.isMajorOrBandits)
		{
			return null;
		}
		DemonSnatchPartyQuest demonSnatchPartyQuest = PartyManager.Instance.CreateNewPartyQuest(PARTY_QUEST_TYPE.Demon_Snatch) as DemonSnatchPartyQuest;
		demonSnatchPartyQuest.SetMadeInLocation(madeInLocation);
		demonSnatchPartyQuest.SetTargetCharacter(targetCharacter);
		demonSnatchPartyQuest.SetDropStructure(dropStructure);
		demonSnatchPartyQuest.SetQuestCreator(questCreator);
		AddPartyQuest(demonSnatchPartyQuest, questCreator);
		return demonSnatchPartyQuest;
	}

	public PartyQuest CreateKillVillagerPartyQuest(Character questCreator, BaseSettlement madeInLocation, Character targetCharacter)
	{
		if (!owner.isMajorOrBandits)
		{
			return null;
		}
		KillVillagerPartyQuest killVillagerPartyQuest = PartyManager.Instance.CreateNewPartyQuest(PARTY_QUEST_TYPE.Kill_Villager) as KillVillagerPartyQuest;
		killVillagerPartyQuest.SetMadeInLocation(madeInLocation);
		killVillagerPartyQuest.SetTargetCharacter(targetCharacter);
		killVillagerPartyQuest.SetQuestCreator(questCreator);
		AddPartyQuest(killVillagerPartyQuest, questCreator);
		return killVillagerPartyQuest;
	}

	public PartyQuest CreateDemonStealPartyQuest(Character questCreator, BaseSettlement madeInLocation, TileObject targetItem, LocationStructure dropStructure)
	{
		if (!owner.isMajorOrBandits)
		{
			return null;
		}
		DemonStealPartyQuest demonStealPartyQuest = PartyManager.Instance.CreateNewPartyQuest(PARTY_QUEST_TYPE.Demon_Steal) as DemonStealPartyQuest;
		demonStealPartyQuest.SetMadeInLocation(madeInLocation);
		demonStealPartyQuest.SetTargetItem(targetItem);
		demonStealPartyQuest.SetDropStructure(dropStructure);
		demonStealPartyQuest.SetQuestCreator(questCreator);
		AddPartyQuest(demonStealPartyQuest, questCreator);
		return demonStealPartyQuest;
	}

	public void CreateMorningPatrolPartyQuest(Character questCreator, BaseSettlement madeInLocation)
	{
		if (owner.isMajorOrBandits)
		{
			MorningPatrolPartyQuest morningPatrolPartyQuest = PartyManager.Instance.CreateNewPartyQuest(PARTY_QUEST_TYPE.Morning_Patrol) as MorningPatrolPartyQuest;
			morningPatrolPartyQuest.SetMadeInLocation(madeInLocation);
			morningPatrolPartyQuest.SetQuestCreator(questCreator);
			AddPartyQuest(morningPatrolPartyQuest, questCreator);
		}
	}

	public void CreateNightPatrolPartyQuest(Character questCreator, BaseSettlement madeInLocation)
	{
		if (owner.isMajorOrBandits)
		{
			NightPatrolPartyQuest nightPatrolPartyQuest = PartyManager.Instance.CreateNewPartyQuest(PARTY_QUEST_TYPE.Night_Patrol) as NightPatrolPartyQuest;
			nightPatrolPartyQuest.SetMadeInLocation(madeInLocation);
			nightPatrolPartyQuest.SetQuestCreator(questCreator);
			AddPartyQuest(nightPatrolPartyQuest, questCreator);
		}
	}

	public void CreateHuntBeastPartyQuest(Character questCreator, BaseSettlement madeInLocation, LocationStructure targetStructure)
	{
		if (owner.isMajorOrBandits)
		{
			HuntBeastPartyQuest huntBeastPartyQuest = PartyManager.Instance.CreateNewPartyQuest(PARTY_QUEST_TYPE.Hunt_Beast) as HuntBeastPartyQuest;
			huntBeastPartyQuest.SetMadeInLocation(madeInLocation);
			huntBeastPartyQuest.SetTargetStructure(targetStructure);
			huntBeastPartyQuest.SetQuestCreator(questCreator);
			AddPartyQuest(huntBeastPartyQuest, questCreator);
		}
	}

	public void CreateBloodHuntPartyQuest(BaseSettlement madeInLocation, Region region)
	{
		if (!owner.isMajorOrBandits)
		{
			return;
		}
		List<BaseSettlement> list = RuinarchListPool<BaseSettlement>.Claim();
		for (int i = 0; i < region.settlementsInRegion.Count; i++)
		{
			BaseSettlement baseSettlement = region.settlementsInRegion[i];
			if (baseSettlement != madeInLocation && (baseSettlement.owner == null || (!baseSettlement.owner.IsHostileWith(madeInLocation.owner) && baseSettlement.owner.factionType.type != FACTION_TYPE.Vampire_Clan)) && baseSettlement.residents.Count((Character c) => !c.isDead && c.isNormalCharacter && !c.traitContainer.HasTrait("Vampire")) > 0)
			{
				list.Add(baseSettlement);
			}
		}
		BaseSettlement randomElement = CollectionUtilities.GetRandomElement(list);
		if (randomElement != null)
		{
			BloodHuntPartyQuest bloodHuntPartyQuest = PartyManager.Instance.CreateNewPartyQuest(PARTY_QUEST_TYPE.Blood_Hunt) as BloodHuntPartyQuest;
			bloodHuntPartyQuest.SetMadeInLocation(madeInLocation);
			bloodHuntPartyQuest.SetTargetSettlement(randomElement);
			AddPartyQuest(bloodHuntPartyQuest, null);
		}
	}

	public void CreateRecruitVampiresPartyQuest(BaseSettlement madeInLocation, Region region)
	{
		if (!owner.isMajorOrBandits)
		{
			return;
		}
		List<BaseSettlement> list = RuinarchListPool<BaseSettlement>.Claim();
		for (int i = 0; i < region.settlementsInRegion.Count; i++)
		{
			BaseSettlement baseSettlement = region.settlementsInRegion[i];
			if (baseSettlement != madeInLocation && (baseSettlement.owner == null || (!baseSettlement.owner.IsHostileWith(madeInLocation.owner) && baseSettlement.owner.factionType.type != FACTION_TYPE.Vampire_Clan)) && baseSettlement.residents.Count((Character c) => !c.isDead && c.isNormalCharacter && !c.traitContainer.HasTrait("Vampire")) > 0)
			{
				list.Add(baseSettlement);
			}
		}
		BaseSettlement randomElement = CollectionUtilities.GetRandomElement(list);
		if (randomElement != null)
		{
			RecruitVampiresPartyQuest recruitVampiresPartyQuest = PartyManager.Instance.CreateNewPartyQuest(PARTY_QUEST_TYPE.Recruit_Vampires) as RecruitVampiresPartyQuest;
			recruitVampiresPartyQuest.SetMadeInLocation(madeInLocation);
			recruitVampiresPartyQuest.SetTargetSettlement(randomElement);
			AddPartyQuest(recruitVampiresPartyQuest, null);
		}
	}

	public void CreateBountyHuntPartyQuest(Character questCreator, BaseSettlement madeInLocation, Character targetCharacter)
	{
		if (owner.isMajorOrBandits)
		{
			BountyHuntPartyQuest bountyHuntPartyQuest = PartyManager.Instance.CreateNewPartyQuest(PARTY_QUEST_TYPE.Bounty_Hunt) as BountyHuntPartyQuest;
			bountyHuntPartyQuest.SetMadeInLocation(madeInLocation);
			bountyHuntPartyQuest.SetTargetCharacter(targetCharacter);
			bountyHuntPartyQuest.SetQuestCreator(questCreator);
			AddPartyQuest(bountyHuntPartyQuest, questCreator);
		}
	}

	public void CreateClaimHallowedGroundQuest(BaseSettlement madeInLocation, Region region, Inner_Maps.Location_Structures.HallowedGround hallowedGround)
	{
		if (owner.isMajorOrBandits && owner.factionType is CultFaction cultFaction && hallowedGround.claimedByReligion != cultFaction.cultReligion)
		{
			ClaimHallowedGroundPartyQuest claimHallowedGroundPartyQuest = PartyManager.Instance.CreateNewPartyQuest(PARTY_QUEST_TYPE.Claim_Hallowed_Ground) as ClaimHallowedGroundPartyQuest;
			claimHallowedGroundPartyQuest.SetMadeInLocation(madeInLocation);
			claimHallowedGroundPartyQuest.SetTargetStructure(hallowedGround);
			AddPartyQuest(claimHallowedGroundPartyQuest, null);
		}
	}

	public void CreateDefendHallowedGroundQuest(BaseSettlement madeInLocation, Region region, Inner_Maps.Location_Structures.HallowedGround hallowedGround)
	{
		if (owner.isMajorOrBandits && owner.factionType is CultFaction cultFaction && hallowedGround.claimedByReligion == cultFaction.cultReligion)
		{
			DefendHallowedGroundPartyQuest defendHallowedGroundPartyQuest = PartyManager.Instance.CreateNewPartyQuest(PARTY_QUEST_TYPE.Defend_Hallowed_Ground) as DefendHallowedGroundPartyQuest;
			defendHallowedGroundPartyQuest.SetMadeInLocation(madeInLocation);
			defendHallowedGroundPartyQuest.SetTargetStructure(hallowedGround);
			AddPartyQuest(defendHallowedGroundPartyQuest, null);
		}
	}

	public void LoadReferences(SaveDataPartyQuestBoard data)
	{
		owner = FactionManager.Instance.GetFactionByPersistentID(data.owner);
		if (data.availablePartyQuests == null)
		{
			return;
		}
		for (int i = 0; i < data.availablePartyQuests.Count; i++)
		{
			PartyQuest partyQuestByPersistentID = DatabaseManager.Instance.partyQuestDatabase.GetPartyQuestByPersistentID(data.availablePartyQuests[i]);
			if (partyQuestByPersistentID.target != null)
			{
				availablePartyQuests.Add(partyQuestByPersistentID);
				partyQuestByPersistentID.OnLoadQuestOnBoard(owner);
			}
		}
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		for (int i = 0; i < availablePartyQuests.Count; i++)
		{
			availablePartyQuests[i].CheckIfStructureIsStillReferenced(p_structure);
		}
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		for (int i = 0; i < availablePartyQuests.Count; i++)
		{
			availablePartyQuests[i].CheckIfCharacterIsStillReferenced(p_character);
		}
	}

	public void OnDisbandFaction()
	{
	}
}

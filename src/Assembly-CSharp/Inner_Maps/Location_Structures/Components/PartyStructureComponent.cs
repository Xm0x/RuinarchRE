using System;
using System.Collections.Generic;
using System.Linq;
using Locations.Settlements;
using UtilityScripts;

namespace Inner_Maps.Location_Structures.Components;

public class PartyStructureComponent : Party.PartyEventsIListener
{
	public const int MAX_SUMMON_COUNT = 5;

	private bool m_isInitialized;

	private bool m_isUndeployUserAction;

	public LocationStructure owner { get; private set; }

	public Party party { get; protected set; }

	public PartyStructureData partyData { get; private set; }

	public STRUCTURE_PARTY_TYPE structurePartyType { get; private set; }

	public SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR[] availableBehaviours { get; private set; }

	public virtual Type serializedData => typeof(SaveDataPartyStructureComponent);

	public PartyStructureComponent(LocationStructure p_owner)
	{
		owner = p_owner;
		partyData = new PartyStructureData();
		SubscribeListeners();
		StructureData structureData = LandmarkManager.Instance.GetStructureData(owner.structureType);
		structurePartyType = structureData.structurePartyType;
		PopulateAvailableBehaviours(structureData);
	}

	public PartyStructureComponent(SaveDataPartyStructureComponent p_data)
	{
		partyData = new PartyStructureData();
		SubscribeListeners();
	}

	public virtual bool IsAvailable()
	{
		return true;
	}

	protected virtual void OnAfterGameLoaded()
	{
	}

	public void DeployParty(SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR p_behaviour)
	{
		m_isUndeployUserAction = false;
		switch (p_behaviour)
		{
		case SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Snatch_Villager:
			DeploySnatchVillagerParty();
			break;
		case SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Snatch_Monster:
			DeploySnatchMonsterParty();
			break;
		case SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Kill_Villager:
			DeployKillVillagerParty();
			break;
		case SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Destroy_Supplies:
		case SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Destroy_Structures:
		case SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Harass_Villagers:
		case SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Destroy_Defenses:
			DeployRaidParty(p_behaviour);
			break;
		default:
			throw new ArgumentOutOfRangeException("p_behaviour", p_behaviour, null);
		}
	}

	private void DeploySnatchVillagerParty()
	{
		Character partyLeader = partyData.GetPartyLeader();
		IStoredTarget target = partyData.target;
		if (target is Character targetCharacter)
		{
			party = PartyManager.Instance.CreateNewParty(partyLeader, PARTY_QUEST_TYPE.Demon_Snatch);
			partyLeader.combatComponent.SetCombatMode(COMBAT_MODE.Defend);
			partyLeader.reactionComponent.SetIsHidden(state: false);
			partyData.deployedSummons.ForEach(delegate(Character eachSummon)
			{
				party.AddMember(eachSummon);
			});
			partyData.deployedSummons.ForEach(delegate(Character eachSummon)
			{
				eachSummon.combatComponent.SetCombatMode(COMBAT_MODE.Defend);
			});
			partyData.deployedSummons.ForEach(delegate(Character eachSummon)
			{
				eachSummon.reactionComponent.SetIsHidden(state: false);
			});
			PartyQuest partyQuest = PlayerManager.Instance.player.playerFaction.partyQuestBoard.CreateDemonSnatchPartyQuest(partyLeader, partyLeader.homeSettlement, targetCharacter, partyData.targetStructure);
			target.isTargetted = true;
			partyQuest.SetIsDemonicQuest(p_state: true);
			party.TryAcceptQuest(partyQuest, partyLeader);
			party.AddMemberThatJoinedQuest(partyLeader);
			partyData.deployedSummons.ForEach(delegate(Character eachSummon)
			{
				party.AddMemberThatJoinedQuest(eachSummon);
			});
			ListenToParty();
			PlayerManager.Instance.player.bookmarkComponent.AddBookmark(party, BOOKMARK_CATEGORY.Player_Parties);
		}
	}

	private void DeploySnatchMonsterParty()
	{
		Character partyLeader = partyData.GetPartyLeader();
		IStoredTarget target = partyData.target;
		if (target is Character targetCharacter)
		{
			party = PartyManager.Instance.CreateNewParty(partyLeader, PARTY_QUEST_TYPE.Demon_Snatch);
			partyLeader.combatComponent.SetCombatMode(COMBAT_MODE.Defend);
			partyLeader.reactionComponent.SetIsHidden(state: false);
			partyData.deployedSummons.ForEach(delegate(Character eachSummon)
			{
				party.AddMember(eachSummon);
			});
			partyData.deployedSummons.ForEach(delegate(Character eachSummon)
			{
				eachSummon.combatComponent.SetCombatMode(COMBAT_MODE.Defend);
			});
			partyData.deployedSummons.ForEach(delegate(Character eachSummon)
			{
				eachSummon.reactionComponent.SetIsHidden(state: false);
			});
			PartyQuest partyQuest = PlayerManager.Instance.player.playerFaction.partyQuestBoard.CreateDemonSnatchPartyQuest(partyLeader, partyLeader.homeSettlement, targetCharacter, partyData.targetStructure);
			target.isTargetted = true;
			partyQuest.SetIsDemonicQuest(p_state: true);
			party.TryAcceptQuest(partyQuest, partyLeader);
			party.AddMemberThatJoinedQuest(partyLeader);
			partyData.deployedSummons.ForEach(delegate(Character eachSummon)
			{
				party.AddMemberThatJoinedQuest(eachSummon);
			});
			ListenToParty();
			PlayerManager.Instance.player.bookmarkComponent.AddBookmark(party, BOOKMARK_CATEGORY.Player_Parties);
		}
	}

	private void DeployKillVillagerParty()
	{
		Character partyLeader = partyData.GetPartyLeader();
		IStoredTarget target = partyData.target;
		if (target is Character targetCharacter)
		{
			party = PartyManager.Instance.CreateNewParty(partyLeader, PARTY_QUEST_TYPE.Kill_Villager);
			partyLeader.combatComponent.SetCombatMode(COMBAT_MODE.Defend);
			partyLeader.reactionComponent.SetIsHidden(state: false);
			partyData.deployedSummons.ForEach(delegate(Character eachSummon)
			{
				party.AddMember(eachSummon);
			});
			partyData.deployedSummons.ForEach(delegate(Character eachSummon)
			{
				eachSummon.combatComponent.SetCombatMode(COMBAT_MODE.Defend);
			});
			partyData.deployedSummons.ForEach(delegate(Character eachSummon)
			{
				eachSummon.reactionComponent.SetIsHidden(state: false);
			});
			PartyQuest partyQuest = PlayerManager.Instance.player.playerFaction.partyQuestBoard.CreateKillVillagerPartyQuest(partyLeader, partyLeader.homeSettlement, targetCharacter);
			target.isTargetted = true;
			partyQuest.SetIsDemonicQuest(p_state: true);
			party.TryAcceptQuest(partyQuest, partyLeader);
			party.AddMemberThatJoinedQuest(partyLeader);
			partyData.deployedSummons.ForEach(delegate(Character eachSummon)
			{
				party.AddMemberThatJoinedQuest(eachSummon);
			});
			ListenToParty();
			PlayerManager.Instance.player.bookmarkComponent.AddBookmark(party, BOOKMARK_CATEGORY.Player_Parties);
		}
	}

	private void DeployRaidParty(SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR p_raidBehaviour)
	{
		Character partyLeader = partyData.GetPartyLeader();
		party = PartyManager.Instance.CreateNewParty(partyLeader, PARTY_QUEST_TYPE.Demon_Raid);
		partyLeader.combatComponent.SetCombatMode(COMBAT_MODE.Defend);
		partyLeader.reactionComponent.SetIsHidden(state: false);
		partyData.deployedSummons.ForEach(delegate(Character eachSummon)
		{
			party.AddMember(eachSummon);
		});
		partyData.deployedSummons.ForEach(delegate(Character eachSummon)
		{
			eachSummon.combatComponent.SetCombatMode(COMBAT_MODE.Defend);
		});
		partyData.deployedSummons.ForEach(delegate(Character eachSummon)
		{
			eachSummon.reactionComponent.SetIsHidden(state: false);
		});
		PartyQuest partyQuest = PlayerManager.Instance.player.playerFaction.partyQuestBoard.CreateDemonRaidPartyQuest(partyLeader, partyLeader.homeSettlement, partyData.target as BaseSettlement);
		partyQuest.SetIsDemonicQuest(p_state: true);
		party.TryAcceptQuest(partyQuest);
		party.AddMemberThatJoinedQuest(partyLeader);
		partyData.deployedSummons.ForEach(delegate(Character eachSummon)
		{
			party.AddMemberThatJoinedQuest(eachSummon);
		});
		if (partyQuest is DemonRaidPartyQuest demonRaidPartyQuest)
		{
			switch (p_raidBehaviour)
			{
			case SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Destroy_Supplies:
				demonRaidPartyQuest.SetRaidType(DEMON_RAID_TYPE.Destroy_Supplies);
				break;
			case SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Destroy_Structures:
				demonRaidPartyQuest.SetRaidType(DEMON_RAID_TYPE.Destroy_Structures);
				break;
			case SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Harass_Villagers:
				demonRaidPartyQuest.SetRaidType(DEMON_RAID_TYPE.Harass_Villagers);
				break;
			case SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Destroy_Defenses:
				demonRaidPartyQuest.SetRaidType(DEMON_RAID_TYPE.Destroy_Defenses);
				break;
			}
		}
		ListenToParty();
		PlayerManager.Instance.player.bookmarkComponent.AddBookmark(party, BOOKMARK_CATEGORY.Player_Parties);
	}

	public virtual void LoadReferences(LocationStructure p_owner, SaveDataPartyStructureComponent p_saveDataPartyStructureComponent)
	{
		owner = p_owner;
		StructureData structureData = LandmarkManager.Instance.GetStructureData(owner.structureType);
		structurePartyType = structureData.structurePartyType;
		PopulateAvailableBehaviours(structureData);
		if (p_saveDataPartyStructureComponent != null && !string.IsNullOrEmpty(p_saveDataPartyStructureComponent.partyID))
		{
			party = DatabaseManager.Instance.partyDatabase.GetPartyByPersistentIDSafe(p_saveDataPartyStructureComponent.partyID);
		}
	}

	public void AfterStructureDestruction(Character p_responsibleCharacter = null)
	{
		Messenger.RemoveListener(Signals.GAME_LOADED, OnGameLoaded);
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
		Messenger.RemoveListener<Party>(PartySignals.PARTY_DESTROYED, OnPartyDestroyed);
		Messenger.RemoveListener<Party>(PartySignals.UNDEPLOY_PLAYER_PARTY, UndeployPlayerParty);
		if (party != null && partyData != null)
		{
			UnDeployAll();
		}
	}

	public void ProcessOnSetAsInactiveInStructureInfo()
	{
	}

	public void OnResidentArrived(Character p_character)
	{
		if (party != null && !party.IsMember(p_character) && IsResidentValidForParty(p_character))
		{
			party.AddMember(p_character);
			p_character.combatComponent.SetCombatMode(COMBAT_MODE.Defend);
			p_character.reactionComponent.SetIsHidden(state: false);
			party.AddMemberThatJoinedQuest(p_character);
		}
	}

	public void OnResidentAdded(Character p_character)
	{
		if (party != null && !party.IsMember(p_character) && IsResidentValidForParty(p_character))
		{
			party.AddMember(p_character);
			p_character.combatComponent.SetCombatMode(COMBAT_MODE.Defend);
			p_character.reactionComponent.SetIsHidden(state: false);
			party.AddMemberThatJoinedQuest(p_character);
		}
	}

	public void OnResidentCanPerformAgain(Character p_character)
	{
		if (party != null && !party.IsMember(p_character) && IsResidentValidForParty(p_character))
		{
			party.AddMember(p_character);
			p_character.combatComponent.SetCombatMode(COMBAT_MODE.Defend);
			p_character.reactionComponent.SetIsHidden(state: false);
			party.AddMemberThatJoinedQuest(p_character);
		}
	}

	private void SubscribeListeners()
	{
		Messenger.AddListener(Signals.GAME_LOADED, OnGameLoaded);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
		Messenger.AddListener<Party>(PartySignals.PARTY_DESTROYED, OnPartyDestroyed);
		Messenger.AddListener<Party>(PartySignals.UNDEPLOY_PLAYER_PARTY, UndeployPlayerParty);
	}

	private void UnsubscribeListeners()
	{
		Messenger.RemoveListener(Signals.GAME_LOADED, OnGameLoaded);
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
		Messenger.RemoveListener<Party>(PartySignals.PARTY_DESTROYED, OnPartyDestroyed);
		Messenger.RemoveListener<Party>(PartySignals.UNDEPLOY_PLAYER_PARTY, UndeployPlayerParty);
	}

	protected virtual void OnCharacterDied(Character p_deadCharacter)
	{
		if (m_isUndeployUserAction)
		{
			return;
		}
		for (int i = 0; i < partyData.deployedSummons.Count; i++)
		{
			if (p_deadCharacter == partyData.deployedSummons[i])
			{
				partyData.deployedSummons.RemoveAt(i);
				break;
			}
		}
		if (p_deadCharacter == partyData.leader)
		{
			partyData.SetMinionLeader(null);
		}
		if (partyData.deployedSummonCount <= 0 && partyData.leader == null && partyData.target != null)
		{
			partyData.target.isTargetted = false;
			partyData.SetTarget(null);
		}
	}

	private void OnGameLoaded()
	{
		InitializeTeam();
		OnAfterGameLoaded();
	}

	private void OnPartyDestroyed(Party p_party)
	{
		if (party == p_party)
		{
			partyData.ClearAllData();
			party = null;
		}
	}

	private void UndeployPlayerParty(Party p_party)
	{
		if (party == p_party)
		{
			UnDeployAll();
		}
	}

	public bool HasValidStoredTarget(SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR p_behaviour)
	{
		List<IStoredTarget> allPossibleTargets = GetAllPossibleTargets(p_behaviour);
		for (int i = 0; i < allPossibleTargets.Count; i++)
		{
			if (allPossibleTargets[i].IsValidTargetForPartyStructure(owner))
			{
				return true;
			}
		}
		return false;
	}

	private void PopulateAvailableBehaviours(StructureData structureData)
	{
		if (structureData.structurePartyBehaviours.Length != 0)
		{
			availableBehaviours = new SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR[structureData.structurePartyBehaviours.Length];
			for (int i = 0; i < structureData.structurePartyBehaviours.Length; i++)
			{
				SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR sPECIFIC_STRUCTURE_PARTY_BEHAVIOUR = structureData.structurePartyBehaviours[i];
				availableBehaviours[i] = sPECIFIC_STRUCTURE_PARTY_BEHAVIOUR;
			}
		}
	}

	public bool IsBehaviourAvailable(SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR p_behaviour)
	{
		return availableBehaviours.Contains(p_behaviour);
	}

	private void InitializeTeam()
	{
		m_isUndeployUserAction = false;
		if (m_isInitialized)
		{
			return;
		}
		m_isInitialized = true;
		if (party == null)
		{
			return;
		}
		for (int i = 0; i < party.members.Count; i++)
		{
			Character character = party.members[i];
			if (character is Summon)
			{
				partyData.deployedSummons.Add(character);
			}
			else if (character.minion != null)
			{
				partyData.SetMinionLeader(character);
			}
		}
		if (party.currentQuest != null)
		{
			if (party.currentQuest.target != null)
			{
				partyData.SetTarget(party.currentQuest.target as IStoredTarget);
			}
			if (party.currentQuest is DemonStealPartyQuest demonStealPartyQuest)
			{
				partyData.SetTargetStructure(demonStealPartyQuest.dropStructure);
			}
		}
	}

	public void AddDeployedItem(Character p_character)
	{
		if (p_character.minion != null)
		{
			partyData.SetMinionLeader(p_character);
		}
		else
		{
			AddSummonOnCharacterList(p_character);
		}
		p_character.SetDestroyMarkerOnDeath(state: true);
	}

	public void AddMonsterFromMonsterSpawner(Character p_character)
	{
		AddSummonOnCharacterList(p_character);
	}

	private void AddSummonOnCharacterList(Character p_newSummon)
	{
		partyData.deployedSummons.Add(p_newSummon);
	}

	public void SetTarget(IStoredTarget p_newTarget)
	{
		partyData.SetTarget(p_newTarget);
	}

	public void ListenToParty()
	{
		party.Subscribe(this);
	}

	private void UnDeployAll()
	{
		m_isUndeployUserAction = true;
		Party prevParty = party;
		party = null;
		partyData.deployedSummons.ForEach(delegate(Character eachSummon)
		{
			prevParty.RemoveMember(eachSummon);
		});
		List<Character> list = RuinarchListPool<Character>.Claim();
		if (partyData.deployedSummons.Count > 0)
		{
			list.AddRange(partyData.deployedSummons);
		}
		for (int num = 0; num < list.Count; num++)
		{
			Character character = list[num];
			if (character.faction != null && character.faction.isPlayerFaction)
			{
				character.Death();
				continue;
			}
			character.CancelAllJobs();
			if (character is Summon summon)
			{
				character.combatComponent.SetCombatMode(summon.defaultCombatMode);
			}
			else
			{
				character.combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
			}
		}
		if (partyData.leader != null)
		{
			Character leader = partyData.leader;
			prevParty.RemoveMember(leader);
			if (leader.faction != null && leader.faction.isPlayerFaction)
			{
				leader.Death();
			}
			else
			{
				leader.CancelAllJobs();
				if (leader is Summon summon2)
				{
					leader.combatComponent.SetCombatMode(summon2.defaultCombatMode);
				}
				else
				{
					leader.combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
				}
			}
		}
		RuinarchListPool<Character>.Release(list);
		if (partyData != null)
		{
			if (partyData.target != null)
			{
				partyData.target.isTargetted = false;
			}
			partyData.ClearAllData();
		}
		Messenger.Broadcast(PartySignals.PARTY_UNDEPLOYED, prevParty);
		Messenger.Broadcast(PlayerSkillSignals.FORCE_RELOAD_PLAYER_ACTIONS);
	}

	public bool IsResidentValidForParty(Character p_character)
	{
		if (!owner.IsResident(p_character))
		{
			return false;
		}
		if (!p_character.limiterComponent.canMove)
		{
			return false;
		}
		if (p_character.movementComponent.isStationary)
		{
			return false;
		}
		if (!p_character.race.IsSapient() && p_character.gridTileLocation != null)
		{
			return p_character.gridTileLocation.structure == owner;
		}
		return false;
	}

	public bool HasValidResidents()
	{
		for (int i = 0; i < owner.residents.Count; i++)
		{
			Character p_character = owner.residents[i];
			if (IsResidentValidForParty(p_character))
			{
				return true;
			}
		}
		return false;
	}

	public void OnQuestSucceed()
	{
		if (!m_isUndeployUserAction)
		{
			if (partyData != null && partyData.target != null)
			{
				partyData.target.isTargetted = false;
			}
			party.Unsubscribe(this);
			UnDeployAll();
		}
		if (owner.hasBeenDestroyed)
		{
			owner.MarkForCleanup();
		}
	}

	public void OnQuestFailed()
	{
		if (!m_isUndeployUserAction)
		{
			if (partyData != null && partyData.target != null)
			{
				partyData.target.isTargetted = false;
			}
			party.Unsubscribe(this);
			UnDeployAll();
		}
		if (owner.hasBeenDestroyed)
		{
			owner.MarkForCleanup();
		}
	}

	public List<IStoredTarget> GetAllPossibleTargets(SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR p_behaviour)
	{
		switch (p_behaviour)
		{
		case SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Snatch_Villager:
		case SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Kill_Villager:
			return PlayerManager.Instance.player.storedTargetsComponent.storedVillagers;
		case SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Snatch_Monster:
			return PlayerManager.Instance.player.storedTargetsComponent.storedMonsters;
		case SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Destroy_Supplies:
		case SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Destroy_Structures:
		case SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Harass_Villagers:
		case SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Destroy_Defenses:
			return PlayerManager.Instance.player.storedTargetsComponent.validRaidTargets;
		default:
			throw new ArgumentOutOfRangeException("p_behaviour", p_behaviour, null);
		}
	}

	public string GetTestingInfo()
	{
		return "Party Data:\n " + partyData.GetTestingData();
	}

	public virtual void CleanUp()
	{
		if (party != null && partyData != null)
		{
			UnDeployAll();
		}
		partyData?.ClearAllData();
		partyData = null;
		party = null;
		owner = null;
		UnsubscribeListeners();
	}

	public void DisconnectFromStructure(LocationStructure p_structure)
	{
		if (partyData != null && (partyData.target == p_structure || partyData.targetStructure == p_structure))
		{
			UnDeployAll();
		}
	}

	public void DisconnectFromCharacter(Character p_character)
	{
		if (partyData != null && partyData.target == p_character)
		{
			UnDeployAll();
		}
	}

	public virtual void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		_ = owner;
		partyData?.CheckIfStructureIsStillReferenced(p_structure);
	}

	public virtual void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		partyData?.CheckIfCharacterIsStillReferenced(p_character);
	}
}

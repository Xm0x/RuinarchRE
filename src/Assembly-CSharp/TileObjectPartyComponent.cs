using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class TileObjectPartyComponent : TileObjectComponent
{
	private TileObject _owner;

	public Party currentSnatchObjectParty { get; private set; }

	public TileObjectPartyComponent()
	{
	}

	public TileObjectPartyComponent(SaveDataTileObjectPartyComponent data)
	{
	}

	public void SetOwner(TileObject p_owner)
	{
		_owner = p_owner;
	}

	private void SetCurrentSnatchObjectParty(Party p_party)
	{
		if (currentSnatchObjectParty != p_party)
		{
			currentSnatchObjectParty = p_party;
			if (currentSnatchObjectParty != null)
			{
				Messenger.AddListener<Party>(PartySignals.UNDEPLOY_PLAYER_PARTY, OnUndeployParty);
				Messenger.AddListener<Party, PartyQuest>(FactionSignals.PARTY_QUEST_DROPPED, OnPartyQuestDropped);
			}
			else
			{
				Messenger.RemoveListener<Party>(PartySignals.UNDEPLOY_PLAYER_PARTY, OnUndeployParty);
				Messenger.RemoveListener<Party, PartyQuest>(FactionSignals.PARTY_QUEST_DROPPED, OnPartyQuestDropped);
			}
		}
	}

	public bool HasSnatchPartyTargetingThis()
	{
		return currentSnatchObjectParty != null;
	}

	private void OnUndeployParty(Party p_party)
	{
		if (currentSnatchObjectParty == p_party)
		{
			UndeployCurrentSnatchObjectParty(_owner);
		}
	}

	public Party CreateSnatchObjectParty(Character p_leader)
	{
		Party result = PartyManager.Instance.CreateNewParty(p_leader, PARTY_QUEST_TYPE.Demon_Steal);
		p_leader.combatComponent.SetCombatMode(COMBAT_MODE.Defend);
		return result;
	}

	public void DeployParty(Party p_party, TileObject p_targetTileObject, LocationStructure p_targetDropStructure)
	{
		Character partyLeader = p_party.partyLeader;
		PartyQuest p_quest = partyLeader.faction.partyQuestBoard.CreateDemonStealPartyQuest(partyLeader, partyLeader.homeSettlement, p_targetTileObject, p_targetDropStructure);
		p_targetTileObject.isTargetted = true;
		p_party.TryAcceptQuest(p_quest, partyLeader);
		for (int i = 0; i < p_party.members.Count; i++)
		{
			Character character = p_party.members[i];
			p_party.AddMemberThatJoinedQuest(character);
		}
		PlayerManager.Instance.player.bookmarkComponent.AddBookmark(p_party, BOOKMARK_CATEGORY.Player_Parties);
		SetCurrentSnatchObjectParty(p_party);
	}

	public void UndeployCurrentSnatchObjectParty(TileObject p_targetTileObject)
	{
		Party party = currentSnatchObjectParty;
		if (party == null)
		{
			return;
		}
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < party.members.Count; i++)
		{
			Character character = party.members[i];
			if (character != party.partyLeader && party.RemoveMember(character))
			{
				list.Add(character);
				i--;
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			Character character2 = list[j];
			if (character2.faction != null && character2.faction.isPlayerFaction)
			{
				character2.Death();
			}
		}
		Character partyLeader = party.partyLeader;
		party.RemoveMember(partyLeader);
		if (partyLeader.faction != null && partyLeader.faction.isPlayerFaction)
		{
			partyLeader.Death();
		}
		RuinarchListPool<Character>.Release(list);
		p_targetTileObject.isTargetted = false;
		SetCurrentSnatchObjectParty(null);
		Messenger.Broadcast(PartySignals.PARTY_UNDEPLOYED, party);
	}

	private void OnPartyQuestDropped(Party p_party, PartyQuest p_quest)
	{
		if (p_party == currentSnatchObjectParty)
		{
			UndeployCurrentSnatchObjectParty(_owner);
		}
	}

	public void LoadSecondWave(TileObject owner, SaveDataTileObjectPartyComponent p_partyComponent)
	{
		if (!string.IsNullOrEmpty(p_partyComponent.currentSnatchObjectParty))
		{
			currentSnatchObjectParty = DatabaseManager.Instance.partyDatabase.GetPartyByPersistentIDSafe(p_partyComponent.currentSnatchObjectParty);
		}
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}
}

using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Object_Pools;
using UtilityScripts;

public class PartyQuest : ISavable
{
	private string _localizedPartialQuestName;

	public string persistentID { get; private set; }

	public PARTY_QUEST_TYPE partyQuestType { get; protected set; }

	public int minimumPartySize { get; protected set; }

	public bool isWaitTimeOver { get; protected set; }

	public Type relatedBehaviour { get; protected set; }

	public Party assignedParty { get; protected set; }

	public BaseSettlement madeInLocation { get; protected set; }

	public bool isSuccessful { get; protected set; }

	public Character questCreator { get; private set; }

	public Faction postedFaction { get; private set; }

	public int priority { get; protected set; }

	public bool isFactionWideQuest { get; protected set; }

	public bool isDemonicQuest { get; protected set; }

	public virtual IPartyQuestTarget target => null;

	public virtual Type serializedData => typeof(SaveDataPartyQuest);

	public virtual bool waitingToWorkingStateImmediately => false;

	public OBJECT_TYPE objectType => OBJECT_TYPE.Party_Quest;

	public bool isAssigned => assignedParty != null;

	public virtual bool shouldAssignedPartyRetreatUponKnockoutOrKill => false;

	public virtual bool canStillJoinQuestAnytime => false;

	public virtual bool workingStateImmediately => false;

	protected string localizedPartialQuestName
	{
		get
		{
			if (string.IsNullOrEmpty(_localizedPartialQuestName))
			{
				_localizedPartialQuestName = LocalizationManager.Instance.GetLocalizedValue("PartyQuest_Table", partyQuestType.ToStringEnum() + "_Name");
			}
			return _localizedPartialQuestName;
		}
	}

	public PartyQuest(PARTY_QUEST_TYPE partyType)
	{
		persistentID = Utilities.GetNewUniqueID();
		partyQuestType = partyType;
	}

	public PartyQuest(SaveDataPartyQuest data)
	{
		persistentID = data.persistentID;
		partyQuestType = data.partyQuestType;
		minimumPartySize = data.minimumPartySize;
		isWaitTimeOver = data.isWaitTimeOver;
		isSuccessful = data.isSuccessful;
		priority = data.priority;
		isFactionWideQuest = data.isFactionWideQuest;
		isDemonicQuest = data.isDemonicQuest;
		relatedBehaviour = Type.GetType(data.relatedBehaviour);
	}

	protected virtual bool IsConnectedToSettlement(NPCSettlement p_settlement)
	{
		if (madeInLocation == p_settlement)
		{
			return true;
		}
		if (target == p_settlement)
		{
			return true;
		}
		if (GetTargetDestination() == p_settlement)
		{
			return true;
		}
		return false;
	}

	protected virtual bool IsConnectedToStructure(LocationStructure p_structure)
	{
		if (target == p_structure)
		{
			return true;
		}
		if (GetTargetDestination() == p_structure)
		{
			return true;
		}
		return false;
	}

	protected virtual bool IsConnectedToCharacter(Character p_character)
	{
		if (target == p_character)
		{
			return true;
		}
		return false;
	}

	public virtual void OnAcceptQuest(Party partyThatAcceptedQuest)
	{
		if (shouldAssignedPartyRetreatUponKnockoutOrKill)
		{
			Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterNoLongerPerform);
		}
	}

	public virtual void OnWaitTimeOver()
	{
		isWaitTimeOver = true;
	}

	protected virtual void OnEndQuest()
	{
		if (shouldAssignedPartyRetreatUponKnockoutOrKill)
		{
			Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterNoLongerPerform);
		}
		if (madeInLocation != null && madeInLocation is NPCSettlement nPCSettlement)
		{
			nPCSettlement.OnFinishedQuest(this);
		}
	}

	protected virtual void AfterEndQuest()
	{
	}

	public virtual void OnAssignedPartySwitchedState(PARTY_STATE fromState, PARTY_STATE toState)
	{
		if (fromState == PARTY_STATE.Waiting && toState == PARTY_STATE.Moving && !waitingToWorkingStateImmediately)
		{
			OnWaitTimeOver();
		}
		else if (fromState == PARTY_STATE.Waiting && toState == PARTY_STATE.Working && waitingToWorkingStateImmediately)
		{
			OnWaitTimeOver();
		}
	}

	public virtual IPartyTargetDestination GetTargetDestination()
	{
		return null;
	}

	public virtual void OnRemoveMemberThatJoinedQuest(Character character)
	{
	}

	public virtual string GetPartyQuestName()
	{
		return string.Empty;
	}

	public virtual void OnCharacterDeath(Character p_character)
	{
		if (shouldAssignedPartyRetreatUponKnockoutOrKill && assignedParty != null && !assignedParty.isPlayerParty && assignedParty.membersThatJoinedQuest.Contains(p_character))
		{
			if (GameUtilities.RollChance(assignedParty.chanceToRetreatUponKnockoutOrDeath))
			{
				EndQuest(GetLocalizedEndQuestReason("Character_Dead", p_character));
			}
			else
			{
				assignedParty.SetChanceToRetreatUponKnockoutOrDeath(100);
			}
		}
	}

	public virtual bool IsInterestedInJoiningQuest(Character p_character)
	{
		return true;
	}

	public virtual bool IsStillEligibleFor(Faction p_faction)
	{
		return true;
	}

	public void SetAssignedParty(Party party)
	{
		if (assignedParty == null)
		{
			assignedParty = party;
		}
		else if (!assignedParty.IsPartyTheSameAsThisParty(party))
		{
			assignedParty = party;
		}
	}

	public void SetMadeInLocation(BaseSettlement settlement)
	{
		madeInLocation = settlement;
	}

	public void SetIsSuccessful(bool state)
	{
		isSuccessful = state;
	}

	public void SetPriority(int p_priority)
	{
		priority = p_priority;
	}

	public void EndQuest(string reason)
	{
		OnEndQuest();
		if (postedFaction != null)
		{
			if (assignedParty != null)
			{
				assignedParty.DropQuest(postedFaction, reason);
			}
			else
			{
				postedFaction.partyQuestBoard.RemovePartyQuest(this);
			}
		}
		else if (assignedParty != null)
		{
			assignedParty.DropQuest(null, reason);
		}
		AfterEndQuest();
	}

	private void OnCharacterNoLongerPerform(Character character)
	{
		if (character.traitContainer.HasTrait("Unconscious") && assignedParty != null && !assignedParty.isPlayerParty && assignedParty.membersThatJoinedQuest.Contains(character))
		{
			if (GameUtilities.RollChance(assignedParty.chanceToRetreatUponKnockoutOrDeath))
			{
				EndQuest(GetLocalizedEndQuestReason("Character_Incapacitated", character));
			}
			else
			{
				assignedParty.SetChanceToRetreatUponKnockoutOrDeath(100);
			}
		}
	}

	public bool TryTriggerRetreat(string endQuestReason)
	{
		if (assignedParty != null && !assignedParty.isPlayerParty)
		{
			if (GameUtilities.RollChance(assignedParty.chanceToRetreatUponKnockoutOrDeath))
			{
				EndQuest(endQuestReason);
				return true;
			}
			assignedParty.SetChanceToRetreatUponKnockoutOrDeath(100);
		}
		return false;
	}

	public void SetQuestCreator(Character p_creator)
	{
		questCreator = p_creator;
	}

	public void SetIsDemonicQuest(bool p_state)
	{
		isDemonicQuest = p_state;
	}

	public void CultistBetrayalProcessing(ref bool hasEndQuest)
	{
		if (assignedParty == null)
		{
			return;
		}
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < assignedParty.membersThatJoinedQuest.Count; i++)
		{
			Character character = assignedParty.membersThatJoinedQuest[i];
			if (character.isAlliedWithPlayer)
			{
				list.Add(character);
			}
		}
		if (list.Count == assignedParty.membersThatJoinedQuest.Count)
		{
			hasEndQuest = true;
			EndQuest(GetLocalizedEndQuestReason("Allied_With_Ruinarch"));
		}
		else
		{
			for (int j = 0; j < list.Count; j++)
			{
				Character character2 = list[j];
				MembersAreBetrayedByThis(character2);
				character2.interruptComponent.TriggerInterrupt(INTERRUPT.Left_Party, character2, "", null, "Left_Party_Cultist_Player");
				assignedParty.banningComponent.BanCharacter(character2);
			}
		}
		RuinarchListPool<Character>.Release(list);
	}

	private void MembersAreBetrayedByThis(Character p_betrayer)
	{
		if (assignedParty == null)
		{
			return;
		}
		for (int i = 0; i < assignedParty.membersThatJoinedQuest.Count; i++)
		{
			Character character = assignedParty.membersThatJoinedQuest[i];
			if (character != p_betrayer && !character.isAlliedWithPlayer)
			{
				CharacterManager.Instance.TriggerEmotion(EMOTION.Betrayal, character, p_betrayer, REACTION_STATUS.WITNESSED);
			}
		}
	}

	public void OnPostQuestToBoard(Faction p_faction)
	{
		postedFaction = p_faction;
		Messenger.AddListener<NPCSettlement>(SettlementSignals.DISCONNECT_FROM_SETTLEMENT, DisconnectFromSettlement);
		Messenger.AddListener<LocationStructure>(StructureSignals.DISCONNECT_FROM_STRUCTURE, DisconnectFromStructure);
		Messenger.AddListener<Character>(CharacterSignals.DISCONNECT_FROM_CHARACTER, DisconnectFromCharacter);
	}

	public void OnQuestRemovedFromBoard()
	{
		Messenger.RemoveListener<NPCSettlement>(SettlementSignals.DISCONNECT_FROM_SETTLEMENT, DisconnectFromSettlement);
		Messenger.RemoveListener<LocationStructure>(StructureSignals.DISCONNECT_FROM_STRUCTURE, DisconnectFromStructure);
		Messenger.RemoveListener<Character>(CharacterSignals.DISCONNECT_FROM_CHARACTER, DisconnectFromCharacter);
	}

	private void DisconnectFromSettlement(NPCSettlement p_settlement)
	{
		if (IsConnectedToSettlement(p_settlement))
		{
			EndQuest(GetLocalizedEndQuestReason("Specific_Settlement_Destroyed", p_settlement));
		}
	}

	private void DisconnectFromStructure(LocationStructure p_structure)
	{
		if (IsConnectedToStructure(p_structure))
		{
			EndQuest(GetLocalizedEndQuestReason("Specific_Structure_Destroyed", p_structure));
		}
	}

	private void DisconnectFromCharacter(Character p_character)
	{
		if (IsConnectedToCharacter(p_character))
		{
			EndQuest(GetLocalizedEndQuestReason("Target_Disappeared"));
		}
		else if (questCreator == p_character)
		{
			SetQuestCreator(null);
		}
	}

	public void OnLoadQuestOnBoard(Faction p_faction)
	{
		postedFaction = p_faction;
		Messenger.AddListener<NPCSettlement>(SettlementSignals.DISCONNECT_FROM_SETTLEMENT, DisconnectFromSettlement);
		Messenger.AddListener<LocationStructure>(StructureSignals.DISCONNECT_FROM_STRUCTURE, DisconnectFromStructure);
		Messenger.AddListener<Character>(CharacterSignals.DISCONNECT_FROM_CHARACTER, DisconnectFromCharacter);
	}

	public virtual void LoadReferences(SaveDataPartyQuest data)
	{
		if (!string.IsNullOrEmpty(data.assignedParty))
		{
			assignedParty = DatabaseManager.Instance.partyDatabase.GetPartyByPersistentID(data.assignedParty);
		}
		if (!string.IsNullOrEmpty(data.madeInLocation))
		{
			madeInLocation = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentIDSafe(data.madeInLocation);
		}
		if (!string.IsNullOrEmpty(data.questCreator))
		{
			questCreator = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(data.questCreator);
		}
	}

	public virtual void LoadReferencesInMainThread(SaveDataPartyQuest data)
	{
		if (shouldAssignedPartyRetreatUponKnockoutOrKill)
		{
			Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterNoLongerPerform);
		}
	}

	public int GetCoinRewardPerMember(Faction partyFaction)
	{
		int result = 83;
		if (this is BountyHuntPartyQuest bountyHuntPartyQuest)
		{
			CrimeData firstCrimeWantedBy = bountyHuntPartyQuest.targetCharacter.crimeComponent.GetFirstCrimeWantedBy(partyFaction);
			if (firstCrimeWantedBy != null)
			{
				if (firstCrimeWantedBy.crimeSeverity == CRIME_SEVERITY.Misdemeanor)
				{
					result = 50;
				}
				else if (firstCrimeWantedBy.crimeSeverity == CRIME_SEVERITY.Serious)
				{
					result = 80;
				}
				else if (firstCrimeWantedBy.crimeSeverity == CRIME_SEVERITY.Heinous)
				{
					result = 120;
				}
			}
		}
		return result;
	}

	public static string GetLocalizedEndQuestReason(string p_key)
	{
		return LocalizationManager.Instance.GetLocalizedValue("EndQuestsReasons_Table", p_key);
	}

	public static string GetLocalizedEndQuestReason(string p_key, IPointOfInterest p_poi)
	{
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Quest", "EndQuestsReasons_Table", p_key);
		log.AddToFillers(p_poi, p_poi.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		string logText = log.logText;
		LogPool.Release(log);
		return logText;
	}

	public static string GetLocalizedEndQuestReason(string p_key, BaseSettlement p_settlement)
	{
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Quest", "EndQuestsReasons_Table", p_key);
		log.AddToFillers(p_settlement, p_settlement.name, LOG_IDENTIFIER.LANDMARK_1);
		string logText = log.logText;
		LogPool.Release(log);
		return logText;
	}

	public static string GetLocalizedEndQuestReason(string p_key, LocationStructure p_structure)
	{
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Quest", "EndQuestsReasons_Table", p_key);
		log.AddToFillers(p_structure, p_structure.name, LOG_IDENTIFIER.LANDMARK_1);
		string logText = log.logText;
		LogPool.Release(log);
		return logText;
	}

	public virtual void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		_ = target;
		GetTargetDestination();
	}

	public virtual void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		_ = questCreator;
		_ = target;
	}
}

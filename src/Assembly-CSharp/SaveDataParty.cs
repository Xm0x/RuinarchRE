using System;
using System.Collections.Generic;

[Serializable]
public class SaveDataParty : SaveData<Party>, ISavableCounterpart
{
	public string partyName;

	public PARTY_STATE partyState;

	public bool startedTrueRestingState;

	public bool isDisbanded;

	public bool doNotDisband;

	public bool hasChangedTargetDestination;

	public string partyLeader;

	public string partySettlement;

	public string partyFaction;

	public string meetingPlace;

	public string targetRestingTavern;

	public string targetCamp;

	public string targetDestination;

	public PARTY_TARGET_DESTINATION_TYPE targetDestinationType;

	public string currentQuest;

	public PARTY_QUEST_TYPE prevQuestType;

	public PARTY_QUEST_TYPE plannedPartyQuestType;

	public GameDate nextQuestCheckDate;

	public GameDate nextWaitingCheckDate;

	public bool hasSetNextSwitchToWaitingStateTrigger;

	public GameDate endQuestDate;

	public bool hasSetEndQuestDate;

	public int chanceToRetreatUponKnockoutOrDeath;

	public string campSetter;

	public string foodProducer;

	public SaveDataJobBoard jobBoard;

	public List<string> forcedCancelJobsOnTickEnded;

	public GameDate waitingEndDate;

	public List<string> members;

	public List<string> membersThatJoinedQuest;

	public List<string> deadmembers;

	public SaveDataPartyBeaconComponent beaconComponent;

	public SaveDataPartyDamageAccumulator damageAccumulator;

	public SaveDataPartyBanningComponent banningComponent;

	public string persistentID { get; set; }

	public OBJECT_TYPE objectType => OBJECT_TYPE.Party;

	public override void Save(Party data)
	{
		persistentID = data.persistentID;
		partyName = data.partyName;
		partyState = data.partyState;
		partyLeader = data.partyLeader?.persistentID;
		startedTrueRestingState = data.startedTrueRestingState;
		isDisbanded = data.isDisbanded;
		doNotDisband = data.doNotDisband;
		hasChangedTargetDestination = data.hasChangedTargetDestination;
		if (data.partySettlement != null)
		{
			partySettlement = data.partySettlement.persistentID;
		}
		partyFaction = data.partyFaction?.persistentID;
		nextQuestCheckDate = data.nextQuestCheckDate;
		hasSetNextSwitchToWaitingStateTrigger = data.hasSetNextSwitchToWaitingStateTrigger;
		nextWaitingCheckDate = data.nextWaitingCheckDate;
		hasSetEndQuestDate = data.hasSetEndQuestDate;
		endQuestDate = data.endQuestDate;
		waitingEndDate = data.waitingEndDate;
		prevQuestType = data.prevQuestType;
		plannedPartyQuestType = data.plannedPartyQuestType;
		chanceToRetreatUponKnockoutOrDeath = data.chanceToRetreatUponKnockoutOrDeath;
		members = SaveUtilities.ConvertSavableListToIDs(data.members);
		membersThatJoinedQuest = SaveUtilities.ConvertSavableListToIDs(data.membersThatJoinedQuest);
		deadmembers = SaveUtilities.ConvertSavableListToIDs(data.deadMembers);
		jobBoard = new SaveDataJobBoard();
		jobBoard.Save(data.jobBoard);
		if (data.forcedCancelJobsOnTickEnded.Count > 0)
		{
			forcedCancelJobsOnTickEnded = new List<string>();
			for (int i = 0; i < data.forcedCancelJobsOnTickEnded.Count; i++)
			{
				JobQueueItem jobQueueItem = data.forcedCancelJobsOnTickEnded[i];
				forcedCancelJobsOnTickEnded.Add(jobQueueItem.persistentID);
				SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(jobQueueItem);
			}
		}
		if (data.meetingPlace != null)
		{
			meetingPlace = data.meetingPlace.persistentID;
		}
		if (data.targetRestingTavern != null)
		{
			targetRestingTavern = data.targetRestingTavern.persistentID;
		}
		if (data.targetCamp != null)
		{
			targetCamp = data.targetCamp.persistentID;
		}
		if (data.targetDestination != null)
		{
			targetDestination = data.targetDestination.persistentID;
			targetDestinationType = data.targetDestination.partyTargetDestinationType;
		}
		if (data.currentQuest != null)
		{
			currentQuest = data.currentQuest.persistentID;
			SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data.currentQuest);
		}
		beaconComponent = new SaveDataPartyBeaconComponent();
		beaconComponent.Save(data.beaconComponent);
		damageAccumulator = new SaveDataPartyDamageAccumulator();
		damageAccumulator.Save(data.damageAccumulator);
		banningComponent = new SaveDataPartyBanningComponent();
		banningComponent.Save(data.banningComponent);
	}

	public override Party Load()
	{
		return PartyManager.Instance.CreateNewParty(this);
	}

	public override void CleanUp()
	{
	}
}

using System.Collections.Generic;
using Locations.Settlements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class PartyInfoUI : InfoUIBase
{
	[Space(10f)]
	[Header("Basic Info")]
	[SerializeField]
	private TextMeshProUGUI nameLbl;

	[Space(10f)]
	[Header("Info")]
	[SerializeField]
	private TextMeshProUGUI questLbl;

	[SerializeField]
	private TextMeshProUGUI noHomeSettlementLbl;

	[SerializeField]
	private SettlementNameplateItem homeSettlementNameplate;

	[Space(10f)]
	[Header("Characters")]
	[SerializeField]
	private Toggle membersToggle;

	[SerializeField]
	private GameObject characterItemPrefab;

	[SerializeField]
	private ScrollRect activeMembersScrollView;

	[SerializeField]
	private ScrollRect inactiveMembersScrollView;

	[SerializeField]
	private GameObject membersGO;

	[Space(10f)]
	[Header("Logs")]
	[SerializeField]
	private LogsWindow logsWindow;

	public Party activeParty { get; private set; }

	internal override void Initialize()
	{
		base.Initialize();
		Messenger.AddListener<Log>(UISignals.LOG_ADDED, UpdateLogsFromSignal);
		Messenger.AddListener<Log>(UISignals.LOG_IN_DATABASE_UPDATED, UpdateLogsFromSignal);
		Messenger.AddListener<Party, Character>(PartySignals.CHARACTER_JOINED_PARTY, UpdateMembersFromSignal);
		Messenger.AddListener<Party, Character>(PartySignals.CHARACTER_LEFT_PARTY, UpdateMembersFromSignal);
		Messenger.AddListener<Party, Character>(PartySignals.CHARACTER_JOINED_PARTY_QUEST, UpdateMembersFromSignal);
		Messenger.AddListener<Party, Character>(PartySignals.CHARACTER_LEFT_PARTY_QUEST, UpdateMembersFromSignal);
		Messenger.AddListener<Party>(PartySignals.CLEAR_MEMBERS_THAT_JOINED_QUEST, UpdateMembersFromSignal);
		Messenger.AddListener<Party>(PartySignals.DISBAND_PARTY, OnDisbandParty);
		homeSettlementNameplate.SetAsButton();
		homeSettlementNameplate.ClearAllOnClickActions();
		homeSettlementNameplate.AddOnClickAction(OnClickSettlementItem);
		logsWindow.Initialize();
	}

	public override void CloseMenu()
	{
		base.CloseMenu();
		Selector.Instance.Deselect();
		activeParty = null;
	}

	public override void OpenMenu()
	{
		activeParty = _data as Party;
		base.OpenMenu();
		UIManager.Instance.HideObjectPicker();
		UpdateTabs();
		UpdateBasicInfo();
		UpdateInfo();
		UpdateMembers();
		logsWindow.OnParentMenuOpened(activeParty.persistentID);
		UpdateLogs();
	}

	public void UpdatePartyInfo()
	{
		if (activeParty != null)
		{
			UpdateBasicInfo();
			UpdateInfo();
		}
	}

	private void UpdateTabs()
	{
		if (activeParty.members != null && activeParty.members.Count > 0)
		{
			membersToggle.interactable = true;
			return;
		}
		membersToggle.isOn = false;
		membersToggle.interactable = false;
	}

	private void UpdateBasicInfo()
	{
		string text = activeParty.partyName;
		if (activeParty.partyLeader != null)
		{
			text = text + "\n" + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Leader") + ": " + activeParty.partyLeader.name;
		}
		nameLbl.text = text;
	}

	private void UpdateInfo()
	{
		string text = "No Quest";
		if (activeParty.isActive)
		{
			text = activeParty.currentQuest.GetPartyQuestName();
		}
		questLbl.text = text ?? "";
		if (activeParty.partySettlement != null)
		{
			if (homeSettlementNameplate.obj != activeParty.partySettlement)
			{
				homeSettlementNameplate.SetObject(activeParty.partySettlement);
			}
			homeSettlementNameplate.gameObject.SetActive(value: true);
			noHomeSettlementLbl.gameObject.SetActive(value: false);
		}
		else
		{
			homeSettlementNameplate.gameObject.SetActive(value: false);
			noHomeSettlementLbl.gameObject.SetActive(value: true);
		}
	}

	private void UpdateMembers()
	{
		Utilities.DestroyChildren(activeMembersScrollView.content);
		Utilities.DestroyChildren(inactiveMembersScrollView.content);
		for (int i = 0; i < activeParty.membersThatJoinedQuest.Count; i++)
		{
			Character character = activeParty.membersThatJoinedQuest[i];
			if (character != null)
			{
				CharacterNameplateItem component = UIManager.Instance.InstantiateUIObject(characterItemPrefab.name, activeMembersScrollView.content).GetComponent<CharacterNameplateItem>();
				component.SetObject(character);
				component.SetAsDefaultBehaviour();
			}
		}
		List<Character> members = activeParty.members;
		if (members == null || members.Count <= 0)
		{
			return;
		}
		for (int j = 0; j < members.Count; j++)
		{
			Character character2 = members[j];
			if (character2 != null && !activeParty.membersThatJoinedQuest.Contains(character2))
			{
				CharacterNameplateItem component2 = UIManager.Instance.InstantiateUIObject(characterItemPrefab.name, inactiveMembersScrollView.content).GetComponent<CharacterNameplateItem>();
				component2.SetObject(character2);
				component2.SetAsDefaultBehaviour();
			}
		}
	}

	private void ShowDisbandLog(Party party)
	{
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Party", "Party_Table", "disband", LOG_TAG.Party);
		log.AddToFillers(party, party.name, LOG_IDENTIFIER.PARTY_1);
		log.AddLogToDatabase();
		PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
	}

	public void UpdateLogs()
	{
		logsWindow.UpdateAllHistoryInfo();
	}

	private void OnDisbandParty(Party p_party)
	{
		ShowDisbandLog(p_party);
		UpdateMembersFromSignal(p_party);
	}

	private void UpdateLogsFromSignal(Log log)
	{
		if (isShowing && log.IsInvolved(activeParty))
		{
			UpdateLogs();
		}
	}

	private void UpdateMembersFromSignal(Party party, Character member)
	{
		if (isShowing && activeParty.IsPartyTheSameAsThisParty(party))
		{
			UpdateMembers();
		}
	}

	private void UpdateMembersFromSignal(Party party)
	{
		if (isShowing && activeParty.IsPartyTheSameAsThisParty(party))
		{
			UpdateMembers();
		}
	}

	private void OnClickSettlementItem(BaseSettlement settlement)
	{
		UIManager.Instance.ShowSettlementInfo(settlement);
	}

	public void ShowPartyTestingInfo()
	{
	}

	public void HidePartyTestingInfo()
	{
	}
}

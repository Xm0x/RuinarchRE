using System;
using EZObjectPools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class PartyQuestItem : PooledObject
{
	[SerializeField]
	private TextMeshProUGUI lblQuestName;

	[SerializeField]
	private TextMeshProUGUI lblReward;

	[SerializeField]
	private Button btnCancelQuest;

	[SerializeField]
	private GameObject goCover;

	[SerializeField]
	private HoverHandler hoverHandler;

	private PartyQuest _quest;

	private Action<PartyQuest> _onClickCancelQuest;

	public PartyQuest quest => _quest;

	public void Initialize(PartyQuest p_quest, Action<PartyQuest> p_onClickCancelQuest, Faction p_faction)
	{
		_quest = p_quest;
		_onClickCancelQuest = p_onClickCancelQuest;
		lblQuestName.text = p_quest.GetPartyQuestName();
		lblReward.text = $"{Utilities.CoinIcon()}{p_quest.GetCoinRewardPerMember(p_faction)}";
		Messenger.AddListener<Party, PartyQuest>(FactionSignals.PARTY_QUEST_ACCEPTED, OnPartyQuestAccepted);
		Messenger.AddListener<Party, PartyQuest>(FactionSignals.PARTY_QUEST_DROPPED, OnPartyQuestDropped);
		UpdateCoverState();
	}

	private void UpdateCoverState()
	{
		goCover.SetActive(quest.isAssigned);
		if (goCover.activeSelf)
		{
			base.transform.SetAsLastSibling();
		}
	}

	private void OnClickCancelQuest()
	{
		_onClickCancelQuest?.Invoke(_quest);
	}

	private void OnHoverOverItem()
	{
		if (_quest.isAssigned)
		{
			UIManager.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Quest_Assigned") + " " + _quest.assignedParty.name);
		}
		else if (_quest.questCreator != null)
		{
			UIManager.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Quest_Posted") + " " + _quest.questCreator.bookmarkName);
		}
	}

	private void OnHoverOutItem()
	{
		UIManager.Instance.HideSmallInfo();
	}

	private void Awake()
	{
		btnCancelQuest.onClick.RemoveAllListeners();
		btnCancelQuest.onClick.AddListener(OnClickCancelQuest);
		hoverHandler.ClearHoverActions();
		hoverHandler.AddOnHoverOverAction(OnHoverOverItem);
		hoverHandler.AddOnHoverOutAction(OnHoverOutItem);
	}

	private void OnPartyQuestAccepted(Party p_party, PartyQuest p_quest)
	{
		if (p_quest == _quest)
		{
			UpdateCoverState();
		}
	}

	private void OnPartyQuestDropped(Party p_party, PartyQuest p_quest)
	{
		if (p_quest == _quest)
		{
			UpdateCoverState();
		}
	}

	public override void Reset()
	{
		base.Reset();
		_quest = null;
		goCover.SetActive(value: false);
		Messenger.RemoveListener<Party, PartyQuest>(FactionSignals.PARTY_QUEST_ACCEPTED, OnPartyQuestAccepted);
		Messenger.RemoveListener<Party, PartyQuest>(FactionSignals.PARTY_QUEST_DROPPED, OnPartyQuestDropped);
	}
}

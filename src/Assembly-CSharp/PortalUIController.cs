using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Maccima_Games.Util;
using Ruinarch;
using Ruinarch.MVCFramework;
using UnityEngine;

public class PortalUIController : MVCUIController, PortalUIView.IListener
{
	[SerializeField]
	private PortalUIModel m_portalUIModel;

	private PortalUIView m_portalUIView;

	public PurchaseSkillUIController purchaseSkillUIController;

	public UnlockMinionUIController unlockMinionUIController;

	public UnlockStructureUIController unlockStructureUIController;

	public UpgradePortalUIController upgradePortalUIController;

	private string m_tooltipCancelReleaseAbility;

	private string m_tooltipCancelUpgradePortal;

	private ThePortal _portal;

	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		PortalUIView.Create(_canvas, m_portalUIModel, delegate(PortalUIView p_ui)
		{
			m_portalUIView = p_ui;
			m_portalUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
			SubscribeListeners();
			unlockMinionUIController.InstantiateUI();
			unlockMinionUIController.HideUI();
		});
	}

	private void OnDestroy()
	{
		m_portalUIView?.Unsubscribe(this);
	}

	public void ShowUI(ThePortal p_portal)
	{
		_portal = p_portal;
		ShowUI();
		InputManager.Instance.SetAllHotkeysEnabledState(p_state: false);
		m_portalUIView.SetUpgradePortalBtnInteractable(!p_portal.IsMaxLevel());
	}

	public override void ShowUI()
	{
		m_mvcUIView.ShowUI();
		if (PlayerManager.Instance.player.playerSkillComponent.currentSpellBeingUnlocked != PLAYER_SKILL_TYPE.NONE)
		{
			m_portalUIView.ShowUnlockAbilityTimerAndHideButton(PlayerSkillManager.Instance.GetSkillData(PlayerManager.Instance.player.playerSkillComponent.currentSpellBeingUnlocked));
		}
		else
		{
			m_portalUIView.ShowUnlockAbilityButtonAndHideTimer();
		}
		if (!PlayerManager.Instance.player.playerSkillComponent.timerUpgradePortal.IsFinished())
		{
			m_portalUIView.ShowUpgradePortalTimerAndHideButton();
		}
		else
		{
			m_portalUIView.ShowUpgradePortalButtonAndHideTimer();
		}
	}

	public override void HideUI()
	{
		base.HideUI();
		InputManager.Instance.SetAllHotkeysEnabledState(p_state: true);
		UIManager.Instance.SetSpeedTogglesState(state: true);
		UIManager.Instance.ResumeLastProgressionSpeed();
	}

	private void Start()
	{
		Messenger.AddListener(Signals.GAME_LOADED, Initialize);
	}

	private void Initialize()
	{
		InstantiateUI();
		HideUI();
		int num = UIManager.Instance.structureInfoUI.transform.GetSiblingIndex() + 1;
		m_portalUIView.UIModel.transform.SetSiblingIndex(num);
		m_tooltipCancelReleaseAbility = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "player_cancel_release_ability");
		m_tooltipCancelUpgradePortal = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "player_cancel_upgrade_portal");
		Messenger.RemoveListener(Signals.GAME_LOADED, Initialize);
	}

	public void InitializeAfterLoadoutSelected()
	{
		m_portalUIView.UIModel.timerReleaseAbility.SetTimer(PlayerManager.Instance.player.playerSkillComponent.timerUnlockSpell);
		m_portalUIView.UIModel.timerReleaseAbility.SetHoverOverAction(OnHoverOverReleaseAbilityTimer);
		m_portalUIView.UIModel.timerReleaseAbility.SetHoverOutAction(OnHoverOutReleaseAbilityTimer);
		m_portalUIView.UIModel.timerUpgradePortal.SetTimer(PlayerManager.Instance.player.playerSkillComponent.timerUpgradePortal);
		m_portalUIView.UIModel.timerUpgradePortal.SetHoverOverAction(OnHoverOverUpgradePortalTimer);
		m_portalUIView.UIModel.timerUpgradePortal.SetHoverOutAction(OnHoverOutUpgradePortalTimer);
	}

	private void SubscribeListeners()
	{
		Messenger.AddListener<SkillData, int>(PlayerSignals.PLAYER_CHOSE_SKILL_TO_UNLOCK, OnPlayerChoseSkillToUnlock);
		Messenger.AddListener<PLAYER_SKILL_TYPE, int>(PlayerSignals.PLAYER_FINISHED_SKILL_UNLOCK, OnPlayerFinishedSkillUnlock);
		Messenger.AddListener(PlayerSignals.PLAYER_SKILL_UNLOCK_CANCELLED, OnPlayerCancelledSkillUnlock);
		Messenger.AddListener(PlayerSignals.PLAYER_STARTED_PORTAL_UPGRADE, OnPlayerChosePortalUpgrade);
		Messenger.AddListener<int>(PlayerSignals.PLAYER_FINISHED_PORTAL_UPGRADE, OnPlayerFinishedPortalUpgrade);
		Messenger.AddListener(PlayerSignals.PORTAL_UPGRADE_CANCELLED, OnPlayerCancelledPortalUpgrade);
	}

	private void OnPlayerChoseSkillToUnlock(SkillData p_skill, int p_unlockCost)
	{
		m_portalUIView.ShowUnlockAbilityTimerAndHideButton(p_skill);
	}

	private void OnPlayerFinishedSkillUnlock(PLAYER_SKILL_TYPE p_skill, int p_unlockCost)
	{
		m_portalUIView.ShowUnlockAbilityButtonAndHideTimer();
	}

	private void OnPlayerCancelledSkillUnlock()
	{
		m_portalUIView.ShowUnlockAbilityButtonAndHideTimer();
	}

	private void OnPlayerChosePortalUpgrade()
	{
		m_portalUIView.ShowUpgradePortalTimerAndHideButton();
	}

	private void OnPlayerFinishedPortalUpgrade(int p_currentPortal)
	{
		m_portalUIView.ShowUpgradePortalButtonAndHideTimer();
	}

	private void OnPlayerCancelledPortalUpgrade()
	{
		m_portalUIView.ShowUpgradePortalButtonAndHideTimer();
	}

	public void OnClickReleaseAbility()
	{
		purchaseSkillUIController.Init(purchaseSkillUIController.skillCountPerDraw, playShowAnimation: true);
	}

	public void OnClickUpgradePortal()
	{
		ThePortal portal = PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal;
		upgradePortalUIController.ShowPortalUpgradeTier(portal);
	}

	public void OnClickCancelReleaseAbility()
	{
		SkillData skillData = PlayerSkillManager.Instance.GetSkillData(PlayerManager.Instance.player.playerSkillComponent.currentSpellBeingUnlocked);
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Cancel_Release_Ability");
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
		dictionary.Add("powerName", skillData.localizedName);
		string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Cancel_Release_Ability_Description", dictionary);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
		UIManager.Instance.ShowYesNoConfirmation(localizedValue, localizedValue2, OnConfirmCancelRelease, null, showCover: true, 30);
	}

	private void OnConfirmCancelRelease()
	{
		PlayerManager.Instance.player.playerSkillComponent.CancelCurrentPlayerSkillUnlock();
	}

	public void OnClickCancelUpgradePortal()
	{
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Cancel_Portal_Upgrade");
		string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Cancel_Portal_Upgrade_Description");
		UIManager.Instance.ShowYesNoConfirmation(localizedValue, localizedValue2, OnConfirmCancelUpgradePortal, null, showCover: true, 30);
	}

	private void OnConfirmCancelUpgradePortal()
	{
		PlayerManager.Instance.player.playerSkillComponent.CancelPortalUpgrade();
	}

	public void OnHoverOverCancelReleaseAbility()
	{
		UIManager.Instance.ShowSmallInfo(m_tooltipCancelReleaseAbility);
	}

	public void OnHoverOutCancelReleaseAbility()
	{
		UIManager.Instance.HideSmallInfo();
	}

	public void OnHoverOverCancelUpgradePortal()
	{
		UIManager.Instance.ShowSmallInfo(m_tooltipCancelUpgradePortal);
	}

	public void OnHoverOutCancelUpgradePortal()
	{
		UIManager.Instance.HideSmallInfo();
	}

	public void OnHoverOverUpgradePortal()
	{
		if (_portal != null && _portal.IsMaxLevel())
		{
			UIManager.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Portal_Max_Level"));
		}
	}

	public void OnHoverOutUpgradePortal()
	{
		if (_portal != null && _portal.IsMaxLevel())
		{
			UIManager.Instance.HideSmallInfo();
		}
	}

	public void OnClickClose()
	{
		HideUI();
	}

	private void OnHoverOverReleaseAbilityTimer()
	{
		string info = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Remaining_Time") + ": " + PlayerManager.Instance.player.playerSkillComponent.timerUnlockSpell.GetRemainingTimeString();
		UIManager.Instance.ShowSmallInfo(info, "", autoReplaceText: false);
	}

	private void OnHoverOutReleaseAbilityTimer()
	{
		UIManager.Instance.HideSmallInfo();
	}

	private void OnHoverOverUpgradePortalTimer()
	{
		string info = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Remaining_Time") + ": " + PlayerManager.Instance.player.playerSkillComponent.timerUpgradePortal.GetRemainingTimeString();
		UIManager.Instance.ShowSmallInfo(info, "", autoReplaceText: false);
	}

	private void OnHoverOutUpgradePortalTimer()
	{
		UIManager.Instance.HideSmallInfo();
	}
}

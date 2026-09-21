using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using Maccima_Games.Util;
using Ruinarch;
using Ruinarch.MVCFramework;
using UnityEngine;

public class UpgradePortalUIController : MVCUIController, UpgradePortalUIView.IListener
{
	[SerializeField]
	private UpgradePortalUIModel m_upgradePortalUIModel;

	private UpgradePortalUIView m_upgradePortalUIView;

	private string m_tooltipCancelUpgradePortal;

	private UpgradePortalPowerItemUI _currentlyClickedUpgradeItem;

	public bool isShowing { get; private set; }

	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		UpgradePortalUIView.Create(_canvas, m_upgradePortalUIModel, delegate(UpgradePortalUIView p_ui)
		{
			m_upgradePortalUIView = p_ui;
			m_upgradePortalUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
		});
	}

	private void Start()
	{
		InstantiateUI();
		HideUI();
		m_upgradePortalUIView.AssignChooseItemActions(OnClickChoosePower, OnHoverOverChoosePower, OnHoverOutChoosePower);
		m_upgradePortalUIView.SetOnClickLeveLUpTierAction(OnClickLevelUpTier);
		SubscribeListeners();
	}

	private void OnDestroy()
	{
		m_upgradePortalUIView?.Unsubscribe(this);
	}

	private void SubscribeListeners()
	{
		Messenger.AddListener(PlayerSignals.PORTAL_UPGRADE_CANCELLED, OnPlayerCancelledPortalUpgrade);
		Messenger.AddListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_SPIRIT_ENERGY, OnSpiritEnergyAdjusted);
	}

	private void OnSpiritEnergyAdjusted(int p_amount, int p_spiritEnergy)
	{
		m_upgradePortalUIView.SetCurrentSpiritEnergyText(PlayerManager.Instance.player.currenciesComponent.spiritEnergy);
	}

	public void InitializeAfterLoadoutSelected()
	{
		m_tooltipCancelUpgradePortal = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "player_cancel_upgrade_portal");
		m_upgradePortalUIView.UIModel.timerUpgradePortal.SetTimer(PlayerManager.Instance.player.playerSkillComponent.timerUpgradePortal);
		m_upgradePortalUIView.UIModel.timerUpgradePortal.SetHoverOverAction(OnHoverOverUpgradePortalTimer);
		m_upgradePortalUIView.UIModel.timerUpgradePortal.SetHoverOutAction(OnHoverOutUpgradePortalTimer);
	}

	public void ShowPortalUpgradeTier(ThePortal portal)
	{
		ShowUI();
		m_upgradePortalUIView.UIModel.goChooseSkill.SetActive(value: false);
		m_upgradePortalUIView.UpdateItems(portal);
		m_upgradePortalUIView.SetHeader(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "player_upgrade_portal"));
		m_upgradePortalUIView.SetUpgradeTimerState(!PlayerManager.Instance.player.playerSkillComponent.timerUpgradePortal.IsFinished());
		m_upgradePortalUIView.PlayShowAnimation();
	}

	private void AnimatedHideUI()
	{
		m_upgradePortalUIView.PlayHideAnimation(HideUI);
	}

	public void HideViaShortcutKey()
	{
		AnimatedHideUI();
	}

	public override void ShowUI()
	{
		base.ShowUI();
		isShowing = true;
		m_upgradePortalUIView.SetCurrentSpiritEnergyText(PlayerManager.Instance.player.currenciesComponent.spiritEnergy);
		UIManager.Instance.Pause();
		UIManager.Instance.SetSpeedTogglesState(state: false);
		InputManager.Instance.SetAllHotkeysEnabledState(p_state: false);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Cancel, p_state: true);
		InnerMapCameraMove.Instance.DisableMovement();
	}

	public override void HideUI()
	{
		base.HideUI();
		isShowing = false;
		UIManager.Instance.ResumeLastProgressionSpeed();
		InputManager.Instance.SetAllHotkeysEnabledState(p_state: true);
		InnerMapCameraMove.Instance.EnableMovement();
	}

	public void OnClickClose()
	{
		AnimatedHideUI();
	}

	private void OnClickLevelUpTier(UpgradePortalTierItem p_item)
	{
		PortalUpgradeTier portalUpgradeTier = p_item.portalUpgradeTier;
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Portal_Upgrade");
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
		dictionary.Add("cost", portalUpgradeTier.GetUpgradeCostString() ?? "");
		string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Portal_Upgrade_Description", dictionary);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
		UIManager.Instance.ShowYesNoConfirmation(localizedValue, localizedValue2, OnClickConfirmUpgrade, null, showCover: true, 150);
	}

	private void OnClickConfirmUpgrade()
	{
		ThePortal obj = PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal;
		PortalUpgradeTier nextTier = obj.nextTier;
		obj.PayForUpgrade(nextTier);
		PlayerManager.Instance.player.playerSkillComponent.PlayerStartedPortalUpgrade(nextTier.upgradeCost, nextTier);
		AnimatedHideUI();
	}

	public void OnClickCancelUpgrade()
	{
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Cancel_Portal_Upgrade");
		string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Cancel_Portal_Upgrade_Description");
		UIManager.Instance.ShowYesNoConfirmation(localizedValue, localizedValue2, OnConfirmCancelUpgradePortal, null, showCover: true, 100);
	}

	public void OnHoverOverCancelUpgrade()
	{
		UIManager.Instance.ShowSmallInfo(m_tooltipCancelUpgradePortal);
	}

	public void OnHoverOutCancelUpgrade()
	{
		UIManager.Instance.HideSmallInfo();
	}

	public void OnClickCloseChooseSkill()
	{
		m_upgradePortalUIView.PlayHideChooseSkillAnimation();
	}

	private void OnConfirmCancelUpgradePortal()
	{
		PlayerManager.Instance.player.playerSkillComponent.CancelPortalUpgrade();
	}

	private void OnPlayerCancelledPortalUpgrade()
	{
		m_upgradePortalUIView.SetUpgradeTimerState(p_state: false);
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

	public bool IsChoosePowersWindowOpen()
	{
		return m_upgradePortalUIView.UIModel.goChooseSkill.activeInHierarchy;
	}

	private void OnClickChoosePower(PLAYER_SKILL_TYPE p_skillType)
	{
		ThePortal obj = PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal;
		bool p_addChargesIfLearned = PlayerSkillLoadout.All_Common_Structures.Contains(p_skillType);
		obj.GainPowerFromPortalUpgrade(p_skillType, p_addChargesIfLearned);
		PlayerManager.Instance.player.playerSkillComponent.SetChosenPowerForUpgradeItem(_currentlyClickedUpgradeItem.parentTier, _currentlyClickedUpgradeItem.upgradeItem, p_skillType);
		m_upgradePortalUIView.HideChooseSkillUI();
	}

	private void OnHoverOverChoosePower(PlayerSkillData p_skillData, ChooseSkillItemUI p_itemUI)
	{
		if (!p_itemUI.btnSkill.interactable)
		{
			UIManager.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Already_Learned_Power"));
		}
	}

	private void OnHoverOutChoosePower(PlayerSkillData p_skillData, ChooseSkillItemUI p_itemUI)
	{
		if (!p_itemUI.btnSkill.interactable)
		{
			UIManager.Instance.HideSmallInfo();
		}
	}

	public void HideChooseSkillsWindow()
	{
		OnClickCloseChooseSkill();
	}
}

using System;
using System.Collections.Generic;
using Maccima_Games.Util;
using Ruinarch;
using Ruinarch.MVCFramework;
using UnityEngine;
using UtilityScripts;

public class PurchaseSkillUIController : MVCUIController, PurchaseSkillUIView.IListener
{
	public bool isTestScene;

	public int skillCountPerDraw = 3;

	[SerializeField]
	private PurchaseSkillUIModel m_purchaseSkillUIModel;

	private PurchaseSkillUIView m_purchaseSkillUIView;

	[SerializeField]
	private PurchaseSkillItemUI m_purchaseSkillItemUI;

	private WeightedDictionary<SkillData> m_weightedList;

	private int m_numberOfSkills;

	private SkillProgressionManager m_skillProgressionManager = new SkillProgressionManager();

	public FakePlayer fakePlayer;

	private GameDate m_nextPurchased;

	private bool m_firstRun = true;

	private bool m_isDrawn;

	private string m_tooltipCancelReleaseAbility;

	private PLAYER_SKILL_TYPE m_selectedSkill;

	private int m_unlockCost;

	public bool isShowing { get; private set; }

	private PlayerSkillComponent skillComponentToUse
	{
		get
		{
			if (isTestScene)
			{
				return fakePlayer.skillComponent;
			}
			return PlayerManager.Instance.player.playerSkillComponent;
		}
	}

	private void Start()
	{
		SubscribeListeners();
	}

	private void OnDestroy()
	{
		if (m_purchaseSkillUIView != null)
		{
			m_purchaseSkillUIView.Unsubscribe(this);
			if (m_purchaseSkillUIView.UIModel != null)
			{
				m_purchaseSkillUIView.UIModel.skillItems.ForEach(CleanupItem);
			}
		}
	}

	private void CleanupItem(PurchaseSkillItemUI eachItem)
	{
		eachItem.onButtonClick = (Action<PLAYER_SKILL_TYPE>)Delegate.Remove(eachItem.onButtonClick, new Action<PLAYER_SKILL_TYPE>(OnSkillClick));
		eachItem.onHoverOver = (Action<PlayerSkillData, PurchaseSkillItemUI>)Delegate.Remove(eachItem.onHoverOver, new Action<PlayerSkillData, PurchaseSkillItemUI>(OnHoverOverSkill));
		eachItem.onHoverOut = (Action<PlayerSkillData, PurchaseSkillItemUI>)Delegate.Remove(eachItem.onHoverOut, new Action<PlayerSkillData, PurchaseSkillItemUI>(OnHoverOutSkill));
	}

	public void InitializeAfterLoadoutSelected()
	{
		m_tooltipCancelReleaseAbility = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "player_cancel_release_ability");
		Init(skillCountPerDraw, playShowAnimation: false);
		HideUI();
		m_purchaseSkillUIView.UIModel.timerReleaseAbility.SetTimer(PlayerManager.Instance.player.playerSkillComponent.timerUnlockSpell);
		m_purchaseSkillUIView.UIModel.timerReleaseAbility.SetHoverOverAction(OnHoverOverReleaseAbilityTimer);
		m_purchaseSkillUIView.UIModel.timerReleaseAbility.SetHoverOutAction(OnHoverOutReleaseAbilityTimer);
	}

	private void SubscribeListeners()
	{
		Messenger.AddListener<PLAYER_SKILL_TYPE, int>(PlayerSignals.PLAYER_FINISHED_SKILL_UNLOCK, OnPlayerFinishedSkillUnlock);
		Messenger.AddListener<SkillData, int>(PlayerSignals.PLAYER_CHOSE_SKILL_TO_UNLOCK, OnPlayerChoseSkillToUnlock);
		Messenger.AddListener(PlayerSignals.PLAYER_SKILL_UNLOCK_CANCELLED, OnPlayerCancelledSkillUnlock);
		Messenger.AddListener<int, int>(PlayerSignals.CHAOTIC_ENERGY_ADJUSTED, OnPlaguePointsAdjusted);
	}

	private void OnPlaguePointsAdjusted(int p_amount, int p_plaguePoints)
	{
		m_purchaseSkillUIView.SetCurrentChaoticEnergyText(PlayerManager.Instance.player.chaoticEnergy);
	}

	private void OnPlayerFinishedSkillUnlock(PLAYER_SKILL_TYPE p_skill, int p_unlockCost)
	{
		OnFinishSkillUnlock();
	}

	private void OnPlayerChoseSkillToUnlock(SkillData p_skill, int p_unlockCost)
	{
		UpdateTimerState();
		UpdateWindowCoverState();
		m_purchaseSkillUIView.UIModel.timerReleaseAbility.RefreshName();
	}

	private void OnPlayerCancelledSkillUnlock()
	{
		UpdateTimerState();
		UpdateWindowCoverState();
		UpdateRerollBtn();
		UpdateItems();
	}

	public override void HideUI()
	{
		base.HideUI();
		isShowing = false;
		UIManager.Instance.SetSpeedTogglesState(state: true);
		UIManager.Instance.ResumeLastProgressionSpeed();
		InputManager.Instance.SetAllHotkeysEnabledState(p_state: true);
		InnerMapCameraMove.Instance.EnableMovement();
	}

	public override void ShowUI()
	{
		m_mvcUIView.ShowUI();
		isShowing = true;
	}

	private bool GetIsAvailable()
	{
		return GameManager.Instance.Today().GetTickDifferenceNonAbsoluteOrZeroIfReached(m_nextPurchased) <= 0;
	}

	public void Init(int numberOfSkills, bool playShowAnimation)
	{
		m_numberOfSkills = numberOfSkills;
		InstantiateUI();
		m_purchaseSkillUIView.SetRerollCooldownFill(0f);
		UpdateWindowCoverState();
		UpdateTimerState();
		if (playShowAnimation)
		{
			m_purchaseSkillUIView.PlayShowAnimation();
		}
	}

	private void UpdateWindowCoverState()
	{
		if (PlayerManager.Instance.player.playerSkillComponent.timerUnlockSpell.IsFinished())
		{
			m_purchaseSkillUIView.SetWindowCoverState(p_state: false);
		}
		else
		{
			m_purchaseSkillUIView.SetWindowCoverState(p_state: true);
		}
	}

	private void UpdateTimerState()
	{
		m_purchaseSkillUIView.SetTimerState(!PlayerManager.Instance.player.playerSkillComponent.timerUnlockSpell.IsFinished());
	}

	private void SpawnItems()
	{
		if (m_weightedList.Count <= 0 && PlayerManager.Instance.player.playerSkillComponent.currentSpellChoices.Count <= 0)
		{
			m_purchaseSkillUIView.HideSkills();
			m_purchaseSkillUIView.DisableRerollButton();
			m_purchaseSkillUIView.SetMessage("All Skills Unlocked");
			return;
		}
		m_purchaseSkillUIView.UIModel.skillItems.ForEach(delegate(PurchaseSkillItemUI eachSkill)
		{
			eachSkill.gameObject.SetActive(value: false);
		});
		int num = m_numberOfSkills;
		if (m_weightedList.Count < num)
		{
			num = m_weightedList.Count;
		}
		if (PlayerManager.Instance.player.playerSkillComponent.currentSpellChoices.Count <= 0)
		{
			for (int num2 = 0; num2 < num; num2++)
			{
				SkillData skillData = m_weightedList.PickRandomElementGivenWeights();
				m_weightedList.RemoveElement(skillData);
				PlayerManager.Instance.player.playerSkillComponent.AddCurrentPlayerSpellChoice(skillData.type);
			}
		}
		int num3 = PlayerManager.Instance.player.playerSkillComponent.currentSpellChoices.Count - m_purchaseSkillUIView.UIModel.skillItems.Count;
		if (num3 > 0)
		{
			for (int num4 = 0; num4 < num3; num4++)
			{
				PurchaseSkillItemUI purchaseSkillItemUI = UnityEngine.Object.Instantiate(m_purchaseSkillItemUI, m_purchaseSkillUIView.GetSkillsParent(), worldPositionStays: true);
				purchaseSkillItemUI.onButtonClick = (Action<PLAYER_SKILL_TYPE>)Delegate.Combine(purchaseSkillItemUI.onButtonClick, new Action<PLAYER_SKILL_TYPE>(OnSkillClick));
				purchaseSkillItemUI.onHoverOver = (Action<PlayerSkillData, PurchaseSkillItemUI>)Delegate.Combine(purchaseSkillItemUI.onHoverOver, new Action<PlayerSkillData, PurchaseSkillItemUI>(OnHoverOverSkill));
				purchaseSkillItemUI.onHoverOut = (Action<PlayerSkillData, PurchaseSkillItemUI>)Delegate.Combine(purchaseSkillItemUI.onHoverOut, new Action<PlayerSkillData, PurchaseSkillItemUI>(OnHoverOutSkill));
				m_purchaseSkillUIView.UIModel.skillItems.Add(purchaseSkillItemUI);
			}
		}
		for (int num5 = 0; num5 < PlayerManager.Instance.player.playerSkillComponent.currentSpellChoices.Count; num5++)
		{
			PLAYER_SKILL_TYPE type = PlayerManager.Instance.player.playerSkillComponent.currentSpellChoices[num5];
			SkillData skillData2 = PlayerSkillManager.Instance.GetSkillData(type);
			PurchaseSkillItemUI purchaseSkillItemUI2 = m_purchaseSkillUIView.UIModel.skillItems[num5];
			purchaseSkillItemUI2.gameObject.SetActive(value: true);
			purchaseSkillItemUI2.InitItem(skillData2.type, PlayerManager.Instance.player.currenciesComponent.chaoticEnergy);
		}
		m_weightedList.Clear();
	}

	private void UpdateItems()
	{
		if (m_purchaseSkillUIView.UIModel.skillItems.Count > 0)
		{
			m_purchaseSkillUIView.UIModel.skillItems.ForEach(delegate(PurchaseSkillItemUI eachItems)
			{
				eachItems.UpdateItem(PlayerManager.Instance.player.currenciesComponent.chaoticEnergy);
			});
		}
	}

	private void MakeListForAvailableSkills()
	{
		m_weightedList = TryGetWeightedSpellChoicesList();
		m_isDrawn = true;
	}

	private WeightedDictionary<SkillData> TryGetWeightedSpellChoicesList()
	{
		WeightedDictionary<SkillData> weightedDictionary = new WeightedDictionary<SkillData>();
		foreach (KeyValuePair<PLAYER_SKILL_TYPE, SkillData> allPlayerSkillsDatum in PlayerSkillManager.Instance.allPlayerSkillsData)
		{
			if (allPlayerSkillsDatum.Value.category != PLAYER_SKILL_CATEGORY.AFFLICTION && allPlayerSkillsDatum.Value.category != PLAYER_SKILL_CATEGORY.PLAYER_ACTION && allPlayerSkillsDatum.Value.category != PLAYER_SKILL_CATEGORY.SPELL && allPlayerSkillsDatum.Value.category != PLAYER_SKILL_CATEGORY.DEMONIC_STRUCTURE)
			{
				continue;
			}
			PlayerSkillData scriptableObjPlayerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(allPlayerSkillsDatum.Value.type);
			if ((!allPlayerSkillsDatum.Value.isInUse || scriptableObjPlayerSkillData.canBeReleasedEvenIfLearned) && scriptableObjPlayerSkillData != null && m_skillProgressionManager.CheckRequirementsAndGetUnlockCost(PlayerManager.Instance.player.playerSkillComponent, PlayerManager.Instance.player.currenciesComponent.chaoticEnergy, allPlayerSkillsDatum.Value.type) != -1)
			{
				int num = scriptableObjPlayerSkillData.baseLoadoutWeight;
				if (PlayerSkillManager.Instance.selectedArchetype.IsSameBaseArchetype(scriptableObjPlayerSkillData.archetypeWeightedBonus))
				{
					num /= 2;
				}
				if (num > 0)
				{
					weightedDictionary.AddElement(allPlayerSkillsDatum.Value, num);
				}
			}
		}
		return weightedDictionary;
	}

	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		if (GetIsAvailable() || m_firstRun)
		{
			if (m_purchaseSkillUIView == null)
			{
				MakeListForAvailableSkills();
				DisplayMenuFirstTime();
			}
			else
			{
				DisplayMenu();
				ShowUI();
			}
		}
		else
		{
			ShowUI();
			DisplayMenu();
		}
	}

	private void DisplayMenuFirstTime()
	{
		PurchaseSkillUIView.Create(_canvas, m_purchaseSkillUIModel, delegate(PurchaseSkillUIView p_ui)
		{
			m_purchaseSkillUIView = p_ui;
			m_purchaseSkillUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
			int num = UIManager.Instance.structureInfoUI.transform.GetSiblingIndex() + 2;
			m_purchaseSkillUIView.UIModel.transform.SetSiblingIndex(num);
			ShowUI();
			m_purchaseSkillUIView.ShowSkills();
			UpdateRerollBtn();
			SpawnItems();
			m_purchaseSkillUIView.SetCurrentChaoticEnergyText(PlayerManager.Instance.player.chaoticEnergy);
			UIManager.Instance.Pause();
			UIManager.Instance.SetSpeedTogglesState(state: false);
			InputManager.Instance.SetAllHotkeysEnabledState(p_state: false);
			InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Cancel, p_state: true);
			InnerMapCameraMove.Instance.DisableMovement();
		});
	}

	private void DisplayMenu()
	{
		if (GetIsAvailable())
		{
			if (!m_isDrawn)
			{
				MakeListForAvailableSkills();
			}
			if (m_weightedList.Count > 0 || PlayerManager.Instance.player.playerSkillComponent.currentSpellChoices.Count > 0)
			{
				UpdateRerollBtn();
				SpawnItems();
				m_purchaseSkillUIView.ShowSkills();
				UpdateItems();
			}
			else
			{
				m_purchaseSkillUIView.HideSkills();
				m_purchaseSkillUIView.DisableRerollButton();
				m_purchaseSkillUIView.SetMessage("All Skills Unlocked");
			}
		}
		else
		{
			m_purchaseSkillUIView.HideSkills();
			m_purchaseSkillUIView.SetMessage("New Abilities will be available after " + GameManager.Instance.Today().GetTickDifferenceNonAbsoluteOrZeroIfReached(m_nextPurchased) + " ticks");
		}
		m_purchaseSkillUIView.SetCurrentChaoticEnergyText(PlayerManager.Instance.player.chaoticEnergy);
		UIManager.Instance.Pause();
		UIManager.Instance.SetSpeedTogglesState(state: false);
		InputManager.Instance.SetAllHotkeysEnabledState(p_state: false);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Cancel, p_state: true);
		InnerMapCameraMove.Instance.DisableMovement();
	}

	private void UpdateRerollBtn()
	{
		Cost releaseAbilitiesRerollCost = EditableValuesManager.Instance.GetReleaseAbilitiesRerollCost();
		if (PlayerManager.Instance.player.currenciesComponent.CanAfford(releaseAbilitiesRerollCost) && skillComponentToUse.timerUnlockSpell.IsFinished())
		{
			m_purchaseSkillUIView.EnableRerollButton();
		}
		else
		{
			m_purchaseSkillUIView.DisableRerollButton();
		}
	}

	public void OnRerollClicked()
	{
		m_firstRun = false;
		AudioManager.Instance.TryPlayUISFX("Play_Release_Powers_Reroll");
		Cost releaseAbilitiesRerollCost = EditableValuesManager.Instance.GetReleaseAbilitiesRerollCost();
		PlayerManager.Instance.player.currenciesComponent.ReduceCurrency(releaseAbilitiesRerollCost);
		PlayerManager.Instance.player.playerSkillComponent.OnRerollUsed();
		MakeListForAvailableSkills();
		UpdateRerollBtn();
		SpawnItems();
		m_purchaseSkillUIView.ShowSkills();
		m_purchaseSkillUIView.PlayItemsAnimation();
	}

	public void OnCloseClicked()
	{
		m_purchaseSkillUIView.PlayHideAnimation(HideUI);
	}

	public void OnHoverOverReroll()
	{
		Cost releaseAbilitiesRerollCost = EditableValuesManager.Instance.GetReleaseAbilitiesRerollCost();
		if (!m_purchaseSkillUIView.UIModel.btnReroll.IsInteractable())
		{
			bool flag = PlayerManager.Instance.player.currenciesComponent.CanAfford(releaseAbilitiesRerollCost);
			if (!skillComponentToUse.timerUnlockSpell.IsFinished())
			{
				string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "player_reroll_tooltip_releasing");
				UIManager.Instance.ShowSmallInfo(localizedValue, "", autoReplaceText: false);
			}
			else if (!flag)
			{
				ShowRerollTooltip(releaseAbilitiesRerollCost, "player_reroll_tooltip_cooldown");
			}
		}
		else
		{
			ShowRerollTooltip(releaseAbilitiesRerollCost, "player_reroll_tooltip_default");
		}
	}

	private void ShowRerollTooltip(Cost p_rerollCost, string p_key)
	{
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
		dictionary.Add("text1", p_rerollCost.GetCostStringWithIcon());
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", p_key, dictionary);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
		UIManager.Instance.ShowSmallInfo(localizedValue, "", autoReplaceText: false);
	}

	public void OnHoverOutReroll()
	{
		UIManager.Instance.HideSmallInfo();
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

	private void OnHoverOverSkill(PlayerSkillData p_skillData, PurchaseSkillItemUI p_item)
	{
		if (PlayerManager.Instance.player.playerSkillComponent.currentSpellBeingUnlocked == PLAYER_SKILL_TYPE.NONE)
		{
			if (PlayerManager.Instance.player.currenciesComponent.chaoticEnergy < p_skillData.GetUnlockCost())
			{
				UIManager.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "No_Mana"));
			}
			else
			{
				p_item.borderShineEffect.Play();
			}
		}
	}

	private void OnHoverOutSkill(PlayerSkillData p_skillData, PurchaseSkillItemUI p_item)
	{
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null && PlayerManager.Instance.player.playerSkillComponent.currentSpellBeingUnlocked == PLAYER_SKILL_TYPE.NONE)
		{
			if (PlayerManager.Instance.player.currenciesComponent.chaoticEnergy < p_skillData.GetUnlockCost())
			{
				UIManager.Instance.HideSmallInfo();
			}
			else
			{
				p_item.borderShineEffect.Stop();
			}
		}
	}

	private void OnSkillClick(PLAYER_SKILL_TYPE p_type)
	{
		SkillData skillData = PlayerSkillManager.Instance.GetSkillData(p_type);
		m_selectedSkill = p_type;
		int num = (m_unlockCost = (isTestScene ? m_skillProgressionManager.CheckRequirementsAndGetUnlockCost(fakePlayer.skillComponent, fakePlayer.fakeCurrenciesComponent, p_type) : m_skillProgressionManager.CheckRequirementsAndGetUnlockCost(PlayerManager.Instance.player.playerSkillComponent, PlayerManager.Instance.player.currenciesComponent.chaoticEnergy, p_type)));
		if (num != -1)
		{
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Portal_Upgrade");
			Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
			dictionary.Add("cost", num + Utilities.ChaoticEnergyIcon());
			dictionary.Add("powerName", skillData.localizedName);
			string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Portal_Unlock_Power_Description", dictionary);
			MaccimaDictionaryPool<string, string>.Release(dictionary);
			UIManager.Instance.ShowYesNoConfirmation(localizedValue, localizedValue2, OnYesUnlockSkill, null, showCover: true, 150);
		}
	}

	private void OnYesUnlockSkill()
	{
		SkillData skillData = PlayerSkillManager.Instance.GetSkillData(m_selectedSkill);
		m_firstRun = false;
		PlayerManager.Instance.player.currenciesComponent.AdjustChaoticEnergy(-m_unlockCost);
		PlayerManager.Instance.player.playerSkillComponent.PlayerChoseSkillToAddBonusCharge(skillData, m_unlockCost);
		UpdateRerollBtn();
		UpdateItems();
		OnCloseClicked();
	}

	public void OnFinishSkillUnlock()
	{
		MakeListForAvailableSkills();
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

	public void OnHoverOverCancelReleaseAbility()
	{
		UIManager.Instance.ShowSmallInfo(m_tooltipCancelReleaseAbility);
	}

	public void OnHoverOutCancelReleaseAbility()
	{
		UIManager.Instance.HideSmallInfo();
	}

	public void HideViaShortcutKey()
	{
		m_purchaseSkillUIView.PlayHideAnimation(HideUI);
	}
}

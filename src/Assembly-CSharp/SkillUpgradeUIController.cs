using System;
using System.Collections;
using System.Collections.Generic;
using Ruinarch;
using Ruinarch.MVCFramework;
using UnityEngine;
using UtilityScripts;

public class SkillUpgradeUIController : MVCUIController, SkillUpgradeUIView.IListener
{
	private enum SKILL_VIEW
	{
		AFFLICTIONS,
		SPELLS,
		PLAYER_ACTION
	}

	[SerializeField]
	private SkillUpgradeUIModel m_skillUpgradeUIModel;

	private SkillUpgradeUIView m_skillUpgradeUIView;

	[SerializeField]
	private SkillUpgradeItemUI m_purchaseSkillItemUI;

	private readonly List<SkillUpgradeItemUI> m_skillItems = new List<SkillUpgradeItemUI>();

	private SKILL_VIEW m_currentView = SKILL_VIEW.SPELLS;

	public FakePlayer fakePlayer;

	public bool isTestScene;

	public bool isShowing;

	private bool m_isInitialized;

	private PlayerSkillComponent m_skillComponent;

	private void Start()
	{
		if (!isTestScene)
		{
			if (UIManager.Instance != null)
			{
				UIManager instance = UIManager.Instance;
				instance.onSpireClicked = (Action)Delegate.Combine(instance.onSpireClicked, new Action(OnSpireClicked));
			}
		}
		else
		{
			Init();
		}
		SkillUpgradeItemUI.onHoverOverUpgradeItem = (Action<PLAYER_SKILL_TYPE>)Delegate.Combine(SkillUpgradeItemUI.onHoverOverUpgradeItem, new Action<PLAYER_SKILL_TYPE>(OnHoverOverUpgradeItem));
		SkillUpgradeItemUI.onHoverOutUpgradeItem = (Action<PLAYER_SKILL_TYPE>)Delegate.Combine(SkillUpgradeItemUI.onHoverOutUpgradeItem, new Action<PLAYER_SKILL_TYPE>(OnHoverOutUpgradeItem));
	}

	private void OnDestroy()
	{
		m_skillUpgradeUIView?.Unsubscribe(this);
		if (UIManager.Instance != null)
		{
			UIManager instance = UIManager.Instance;
			instance.onSpireClicked = (Action)Delegate.Remove(instance.onSpireClicked, new Action(OnSpireClicked));
		}
		SkillUpgradeItemUI.onHoverOverUpgradeItem = (Action<PLAYER_SKILL_TYPE>)Delegate.Remove(SkillUpgradeItemUI.onHoverOverUpgradeItem, new Action<PLAYER_SKILL_TYPE>(OnHoverOverUpgradeItem));
		SkillUpgradeItemUI.onHoverOutUpgradeItem = (Action<PLAYER_SKILL_TYPE>)Delegate.Remove(SkillUpgradeItemUI.onHoverOutUpgradeItem, new Action<PLAYER_SKILL_TYPE>(OnHoverOutUpgradeItem));
	}

	private void OnSpireClicked()
	{
		if (GameManager.Instance.gameHasStarted)
		{
			if (!m_isInitialized)
			{
				Init();
				m_isInitialized = true;
			}
			else
			{
				InstantiateUI();
			}
		}
	}

	private void Init()
	{
		if (isTestScene)
		{
			fakePlayer.Initialize();
			m_skillComponent = fakePlayer.skillComponent;
			InstantiateUI();
		}
		else
		{
			m_skillComponent = PlayerManager.Instance.player.playerSkillComponent;
			InstantiateUI();
		}
	}

	public void Open()
	{
		ShowUI();
		Messenger.AddListener<int>(PlayerSignals.UPDATED_CHAOTIC_ENERGY, OnPlaguePointsUpdated);
	}

	public void HideViaShortcutKey()
	{
		HideUI();
	}

	private void OnPlaguePointsUpdated(int p_plaguePoints)
	{
		UpdateTopMenuSummary();
	}

	public override void ShowUI()
	{
		base.ShowUI();
		isShowing = true;
		UpdateTopMenuSummary();
		OnShowUI();
		InputManager.Instance.SetAllHotkeysEnabledState(p_state: false);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Cancel, p_state: true);
		GameManager.Instance.SetPausedState(isPaused: true);
	}

	public override void HideUI()
	{
		isShowing = false;
		UIManager.Instance.ResumeLastProgressionSpeed();
		InputManager.Instance.SetAllHotkeysEnabledState(p_state: true);
		base.HideUI();
		Messenger.RemoveListener<int>(PlayerSignals.UPDATED_CHAOTIC_ENERGY, OnPlaguePointsUpdated);
	}

	private void OnShowUI()
	{
		if (m_currentView == SKILL_VIEW.SPELLS)
		{
			OnSpellTabClicked(isOn: true);
		}
		else if (m_currentView == SKILL_VIEW.AFFLICTIONS)
		{
			OnAfflictionTabClicked(isOn: true);
		}
		else if (m_currentView == SKILL_VIEW.PLAYER_ACTION)
		{
			OnPlayerActionTabClicked(isOn: true);
		}
	}

	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		if (m_skillUpgradeUIView == null)
		{
			SkillUpgradeUIView.Create(_canvas, m_skillUpgradeUIModel, delegate(SkillUpgradeUIView p_ui)
			{
				m_skillUpgradeUIView = p_ui;
				m_skillUpgradeUIView.Subscribe(this);
				InitUI(p_ui.UIModel, p_ui);
				ShowUI();
			});
		}
		else
		{
			ShowUI();
		}
	}

	private void DisplaySkills(List<PLAYER_SKILL_TYPE> p_skillSets)
	{
		SpawnSkillItems(p_skillSets);
		UpdateTopMenuSummary();
	}

	private void UpdateTopMenuSummary()
	{
		int num = m_skillComponent.GetNumberOfUpgradeableAfflictions() + m_skillComponent.GetNumberOfUpgradeableSpells() + m_skillComponent.GetNumberOfUpgradeableAbilities();
		m_skillUpgradeUIView.SetUnlockSkillCount(num.ToString());
		m_skillUpgradeUIView.SetChaticEnergyCount(isTestScene ? fakePlayer.fakeCurrenciesComponent.Spirits.ToString() : PlayerManager.Instance.player.currenciesComponent.chaoticEnergy.ToString());
	}

	private void ClearListFirst()
	{
		if (m_skillItems != null && m_skillItems.Count > 0)
		{
			for (int i = 0; i < m_skillItems.Count; i++)
			{
				SkillUpgradeItemUI skillUpgradeItemUI = m_skillItems[i];
				skillUpgradeItemUI.onButtonClick = (Action<PLAYER_SKILL_TYPE>)Delegate.Remove(skillUpgradeItemUI.onButtonClick, new Action<PLAYER_SKILL_TYPE>(OnSkillClick));
				skillUpgradeItemUI.gameObject.SetActive(value: false);
			}
		}
	}

	private void SpawnSkillItems(List<PLAYER_SKILL_TYPE> listOfSkills)
	{
		if (m_skillItems.Count < listOfSkills.Count)
		{
			int num = listOfSkills.Count - m_skillItems.Count;
			for (int i = 0; i < num; i++)
			{
				SkillUpgradeItemUI skillUpgradeItemUI = UnityEngine.Object.Instantiate(m_purchaseSkillItemUI, m_skillUpgradeUIView.GetSkillParent());
				skillUpgradeItemUI.transform.localScale = new Vector3(1f, 1f, 1f);
				skillUpgradeItemUI.onButtonClick = (Action<PLAYER_SKILL_TYPE>)Delegate.Combine(skillUpgradeItemUI.onButtonClick, new Action<PLAYER_SKILL_TYPE>(OnSkillClick));
				skillUpgradeItemUI.gameObject.SetActive(value: false);
				m_skillItems.Add(skillUpgradeItemUI);
			}
		}
		for (int j = 0; j < m_skillItems.Count; j++)
		{
			SkillUpgradeItemUI skillUpgradeItemUI2 = m_skillItems[j];
			if (j < listOfSkills.Count)
			{
				PLAYER_SKILL_TYPE type = listOfSkills[j];
				SkillData skillData = PlayerSkillManager.Instance.GetSkillData(type);
				skillUpgradeItemUI2.InitItem(skillData.type, isTestScene ? fakePlayer.fakeCurrenciesComponent.Spirits : PlayerManager.Instance.player.currenciesComponent.chaoticEnergy);
				skillUpgradeItemUI2.gameObject.SetActive(value: true);
			}
			else
			{
				skillUpgradeItemUI2.gameObject.SetActive(value: false);
			}
		}
	}

	private void OnSkillClick(PLAYER_SKILL_TYPE p_type)
	{
		PlayerSkillData scriptableObjPlayerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(p_type);
		SkillData skillData = PlayerSkillManager.Instance.GetSkillData(p_type);
		if (isTestScene)
		{
			fakePlayer.fakeCurrenciesComponent.AdjustPlaguePoints(-1 * scriptableObjPlayerSkillData.skillUpgradeData.GetUpgradeCostBaseOnLevel(skillData.currentLevel));
		}
		else
		{
			PlayerManager.Instance.player.currenciesComponent.AdjustChaoticEnergy(-1 * scriptableObjPlayerSkillData.skillUpgradeData.GetUpgradeCostBaseOnLevel(skillData.currentLevel));
		}
		skillData.LevelUp();
		switch (skillData.currentLevel)
		{
		case 1:
			AudioManager.Instance.TryPlayUISFX("Play_Upgrade_Power_1");
			break;
		case 2:
			AudioManager.Instance.TryPlayUISFX("Play_Upgrade_Power_2");
			break;
		case 3:
			AudioManager.Instance.TryPlayUISFX("Play_Upgrade_Power_3");
			break;
		}
		List<PLAYER_SKILL_TYPE> list = RuinarchListPool<PLAYER_SKILL_TYPE>.Claim();
		switch (m_currentView)
		{
		case SKILL_VIEW.AFFLICTIONS:
			PopulateUpgradeableAfflictions(list);
			DisplaySkills(list);
			break;
		case SKILL_VIEW.SPELLS:
			PopulateUpgradeableSpells(list);
			DisplaySkills(list);
			break;
		case SKILL_VIEW.PLAYER_ACTION:
			PopulateUpgradeablePlayerActions(list);
			DisplaySkills(list);
			break;
		}
		RuinarchListPool<PLAYER_SKILL_TYPE>.Release(list);
		UpdateTopMenuSummary();
	}

	public void OnAfflictionTabClicked(bool isOn)
	{
		if (isOn)
		{
			StartCoroutine(IShowAfflictionTab());
		}
	}

	public void OnSpellTabClicked(bool isOn)
	{
		if (isOn)
		{
			StartCoroutine(IShowSpellsTab());
		}
	}

	public void OnPlayerActionTabClicked(bool isOn)
	{
		if (isOn)
		{
			StartCoroutine(IShowAbilitiesTab());
		}
	}

	private IEnumerator IShowAfflictionTab()
	{
		m_currentView = SKILL_VIEW.AFFLICTIONS;
		List<PLAYER_SKILL_TYPE> list = RuinarchListPool<PLAYER_SKILL_TYPE>.Claim();
		PopulateUpgradeableAfflictions(list);
		DisplaySkills(list);
		UpdateTopMenuSummary();
		RuinarchListPool<PLAYER_SKILL_TYPE>.Release(list);
		yield return null;
		m_skillUpgradeUIView.UIModel.skillScrollRect.verticalScrollbar.value = 1f;
	}

	private IEnumerator IShowSpellsTab()
	{
		m_currentView = SKILL_VIEW.SPELLS;
		List<PLAYER_SKILL_TYPE> list = RuinarchListPool<PLAYER_SKILL_TYPE>.Claim();
		PopulateUpgradeableSpells(list);
		DisplaySkills(list);
		UpdateTopMenuSummary();
		RuinarchListPool<PLAYER_SKILL_TYPE>.Release(list);
		yield return null;
		m_skillUpgradeUIView.UIModel.skillScrollRect.verticalScrollbar.value = 1f;
	}

	private IEnumerator IShowAbilitiesTab()
	{
		m_currentView = SKILL_VIEW.PLAYER_ACTION;
		List<PLAYER_SKILL_TYPE> list = RuinarchListPool<PLAYER_SKILL_TYPE>.Claim();
		PopulateUpgradeablePlayerActions(list);
		DisplaySkills(list);
		UpdateTopMenuSummary();
		RuinarchListPool<PLAYER_SKILL_TYPE>.Release(list);
		yield return null;
		m_skillUpgradeUIView.UIModel.skillScrollRect.verticalScrollbar.value = 1f;
	}

	private void PopulateUpgradeableAfflictions(List<PLAYER_SKILL_TYPE> skills)
	{
		for (int i = 0; i < m_skillComponent.afflictions.Count; i++)
		{
			PLAYER_SKILL_TYPE pLAYER_SKILL_TYPE = m_skillComponent.afflictions[i];
			if (!PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(pLAYER_SKILL_TYPE).isNonUpgradeable)
			{
				skills.Add(pLAYER_SKILL_TYPE);
			}
		}
	}

	private void PopulateUpgradeablePlayerActions(List<PLAYER_SKILL_TYPE> skills)
	{
		for (int i = 0; i < m_skillComponent.playerActions.Count; i++)
		{
			PLAYER_SKILL_TYPE pLAYER_SKILL_TYPE = m_skillComponent.playerActions[i];
			if (!PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(pLAYER_SKILL_TYPE).isNonUpgradeable)
			{
				skills.Add(pLAYER_SKILL_TYPE);
			}
		}
	}

	private void PopulateUpgradeableSpells(List<PLAYER_SKILL_TYPE> skills)
	{
		for (int i = 0; i < m_skillComponent.spells.Count; i++)
		{
			PLAYER_SKILL_TYPE pLAYER_SKILL_TYPE = m_skillComponent.spells[i];
			if (!PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(pLAYER_SKILL_TYPE).isNonUpgradeable)
			{
				skills.Add(pLAYER_SKILL_TYPE);
			}
		}
	}

	public void OnCloseClicked()
	{
		HideUI();
	}

	private void OnHoverOverUpgradeItem(PLAYER_SKILL_TYPE p_skillType)
	{
		SkillData skillData = PlayerSkillManager.Instance.GetSkillData(p_skillType);
		if (skillData.isMaxLevel)
		{
			PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(skillData, m_skillUpgradeUIView.UIModel.tooltipPosition, p_dontShowAdditionalText: true);
			return;
		}
		PlayerSkillData scriptableObjPlayerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skillType);
		bool p_isChaoticEnergyEnough = PlayerManager.Instance.player.chaoticEnergy >= scriptableObjPlayerSkillData.skillUpgradeData.GetUpgradeCostBaseOnLevel(skillData.currentLevel);
		PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillWithLevelUpDetails(skillData, m_skillUpgradeUIView.UIModel.tooltipPosition, p_isChaoticEnergyEnough);
	}

	private void OnHoverOutUpgradeItem(PLAYER_SKILL_TYPE p_skillType)
	{
		PlayerUI.Instance.skillDetailsTooltip.HidePlayerSkillDetails();
	}
}

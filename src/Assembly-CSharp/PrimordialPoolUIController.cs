using System;
using Ruinarch;
using Ruinarch.MVCFramework;
using UnityEngine;

public class PrimordialPoolUIController : MVCUIController, PrimordialPoolUIView.IListener
{
	[SerializeField]
	private PrimordialPoolUIModel m_primordialPoolUIModel;

	private PrimordialPoolUIView m_primordialPoolUIView;

	public PrimordialPoolStatsUpgradeUIController primordialPoolUpgradeStatusUIController;

	private Action onPrimordialUIClosed;

	private CHARACTER_CATEGORY m_activeTab;

	public bool isShowing { get; private set; }

	private void OnEnable()
	{
		Messenger.AddListener<int>(PlayerSignals.UPDATED_CHAOTIC_ENERGY, OnPlaguePointsUpdated);
	}

	private void OnDisable()
	{
		Messenger.RemoveListener<int>(PlayerSignals.UPDATED_CHAOTIC_ENERGY, OnPlaguePointsUpdated);
	}

	private void Start()
	{
		InstantiateUI();
	}

	public void Init(Action p_onCloseBiolabUI = null)
	{
		InstantiateUI();
		HideUI();
		if (p_onCloseBiolabUI != null)
		{
			onPrimordialUIClosed = (Action)Delegate.Combine(onPrimordialUIClosed, p_onCloseBiolabUI);
		}
	}

	public void Open()
	{
		ShowUI();
	}

	private void OnPlaguePointsUpdated(int p_plaguePoints)
	{
		UpdateTopMenuSummary();
	}

	public override void ShowUI()
	{
		base.ShowUI();
		isShowing = true;
		UIManager.Instance.Pause();
		UIManager.Instance.SetSpeedTogglesState(state: false);
		InputManager.Instance.SetAllHotkeysEnabledState(p_state: false);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Cancel, p_state: true);
	}

	public override void HideUI()
	{
		base.HideUI();
		isShowing = false;
		UIManager.Instance.SetSpeedTogglesState(state: true);
		UIManager.Instance.ResumeLastProgressionSpeed();
		InputManager.Instance.SetAllHotkeysEnabledState(p_state: true);
		onPrimordialUIClosed?.Invoke();
	}

	private void OnDestroy()
	{
		m_primordialPoolUIView?.Unsubscribe(this);
		if (UIManager.Instance != null)
		{
			UIManager instance = UIManager.Instance;
			instance.onPrimordialPoolClicked = (Action)Delegate.Remove(instance.onPrimordialPoolClicked, new Action(OnPrimordialStructureClicked));
		}
	}

	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		PrimordialPoolUIView.Create(_canvas, m_primordialPoolUIModel, delegate(PrimordialPoolUIView p_ui)
		{
			m_primordialPoolUIView = p_ui;
			m_primordialPoolUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
			m_activeTab = CHARACTER_CATEGORY.Humanoid;
			primordialPoolUpgradeStatusUIController.InstantiateUI();
			primordialPoolUpgradeStatusUIController.SetParent(m_primordialPoolUIView.UIModel.tabPrent);
			HideUI();
			UIManager instance = UIManager.Instance;
			instance.onPrimordialPoolClicked = (Action)Delegate.Combine(instance.onPrimordialPoolClicked, new Action(OnPrimordialStructureClicked));
		});
	}

	private void OnPrimordialStructureClicked()
	{
		ShowUI();
		UpdateTopMenuSummary();
		primordialPoolUpgradeStatusUIController.SetDisplayTexts(m_activeTab);
	}

	private void UpdateTopMenuSummary()
	{
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null)
		{
			m_primordialPoolUIView.SetChaoticEnergy(PlayerManager.Instance.player.currenciesComponent.chaoticEnergy.ToString());
		}
	}

	public void OnHumanoidTabClicked(bool isOn)
	{
		if (isOn)
		{
			m_activeTab = CHARACTER_CATEGORY.Humanoid;
			primordialPoolUpgradeStatusUIController.SetDisplayTexts(m_activeTab);
		}
	}

	public void OnDemonicTabClicked(bool isOn)
	{
		if (isOn)
		{
			m_activeTab = CHARACTER_CATEGORY.Demonic;
			primordialPoolUpgradeStatusUIController.SetDisplayTexts(m_activeTab);
		}
	}

	public void OnUndeadTabClicked(bool isOn)
	{
		if (isOn)
		{
			m_activeTab = CHARACTER_CATEGORY.Undead;
			primordialPoolUpgradeStatusUIController.SetDisplayTexts(m_activeTab);
		}
	}

	public void OnBeastTabClicked(bool isOn)
	{
		if (isOn)
		{
			m_activeTab = CHARACTER_CATEGORY.Beast;
			primordialPoolUpgradeStatusUIController.SetDisplayTexts(m_activeTab);
		}
	}

	public void OnCloseClicked()
	{
		HideUI();
	}

	public void HideViaShortcutKey()
	{
		HideUI();
	}
}

using System;
using Ruinarch.MVCFramework;
using UnityEngine;

public class BiolabUIController : MVCUIController, BiolabUIView.IListener
{
	[SerializeField]
	private BiolabUIModel m_biolabUIModel;

	private BiolabUIView m_biolabUIView;

	public TransmissionUIController transmissionUIController;

	public LifeSpanUIController lifeSpanUIController;

	public FatalityUIController fatalityUIController;

	public SymptomsUIController symptomsUIController;

	public OnDeathUIController onDeathUIController;

	private Action onCloseBiolabUI;

	public bool isShowing { get; private set; }

	public void Init(Action p_onCloseBiolabUI = null)
	{
		InstantiateUI();
		HideUI();
		if (p_onCloseBiolabUI != null)
		{
			onCloseBiolabUI = (Action)Delegate.Combine(onCloseBiolabUI, p_onCloseBiolabUI);
		}
	}

	public void Open()
	{
		ShowUI();
		Messenger.AddListener<int>(PlayerSignals.UPDATED_CHAOTIC_ENERGY, OnPlaguePointsUpdated);
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
		ShowUI(transmissionUIController);
		m_biolabUIView.SetTransmissionTabIsOnWithoutNotify(p_isOn: true);
	}

	public override void HideUI()
	{
		base.HideUI();
		isShowing = false;
		onCloseBiolabUI?.Invoke();
		Messenger.RemoveListener<int>(PlayerSignals.UPDATED_CHAOTIC_ENERGY, OnPlaguePointsUpdated);
	}

	private void OnDisable()
	{
		OnDeathUIController obj = onDeathUIController;
		obj.onUIINstantiated = (Action)Delegate.Remove(obj.onUIINstantiated, new Action(LastUIInstantiated));
	}

	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		BiolabUIView.Create(_canvas, m_biolabUIModel, delegate(BiolabUIView p_ui)
		{
			m_biolabUIView = p_ui;
			m_biolabUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
			OnDeathUIController obj = onDeathUIController;
			obj.onUIINstantiated = (Action)Delegate.Combine(obj.onUIINstantiated, new Action(LastUIInstantiated));
			transmissionUIController.InstantiateUI();
			lifeSpanUIController.InstantiateUI();
			fatalityUIController.InstantiateUI();
			symptomsUIController.InstantiateUI();
			onDeathUIController.InstantiateUI();
		});
	}

	private void OnDestroy()
	{
		m_biolabUIView?.Unsubscribe(this);
	}

	private void LastUIInstantiated()
	{
		transmissionUIController.SetParent(m_biolabUIView.GetTabParentTransform());
		lifeSpanUIController.SetParent(m_biolabUIView.GetTabParentTransform());
		fatalityUIController.SetParent(m_biolabUIView.GetTabParentTransform());
		symptomsUIController.SetParent(m_biolabUIView.GetTabParentTransform());
		onDeathUIController.SetParent(m_biolabUIView.GetTabParentTransform());
		ShowUI(transmissionUIController);
		UpdateTopMenuSummary();
	}

	private void ShowUI(MVCUIController p_targetUIToShow)
	{
		transmissionUIController.HideUI();
		lifeSpanUIController.HideUI();
		fatalityUIController.HideUI();
		symptomsUIController.HideUI();
		onDeathUIController.HideUI();
		p_targetUIToShow.ShowUI();
	}

	private void UpdateTopMenuSummary()
	{
		m_biolabUIView.SetActiveCases(PlagueDisease.Instance.activeCases.ToString());
		m_biolabUIView.SetDeathCases(PlagueDisease.Instance.deaths.ToString());
		m_biolabUIView.SetRecoveriesCases(PlagueDisease.Instance.recoveries.ToString());
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null)
		{
			m_biolabUIView.SetPlaguePoints(PlayerManager.Instance.player.currenciesComponent.chaoticEnergy.ToString());
		}
	}

	public void OnTransmissionTabClicked(bool isOn)
	{
		if (isOn)
		{
			ShowUI(transmissionUIController);
		}
	}

	public void OnLifeSpanTabClicked(bool isOn)
	{
		if (isOn)
		{
			ShowUI(lifeSpanUIController);
		}
	}

	public void OnFatalityTabClicked(bool isOn)
	{
		if (isOn)
		{
			ShowUI(fatalityUIController);
		}
	}

	public void OnSymptomsTabClicked(bool isOn)
	{
		if (isOn)
		{
			ShowUI(symptomsUIController);
		}
	}

	public void OnOnDeathClicked(bool isOn)
	{
		if (isOn)
		{
			ShowUI(onDeathUIController);
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

using System;
using System.Collections.Generic;
using Ruinarch;
using Ruinarch.MVCFramework;
using UnityEngine;
using UtilityScripts;

public class GrudgeUIController : MVCUIController, GrudgeUIView.IListener
{
	[SerializeField]
	private GrudgeUIModel m_grudgeUIModel;

	private GrudgeUIView m_grudgeUIView;

	[SerializeField]
	private GrudgeItemUI m_grudgeItemUI;

	private Character _actor;

	private Character _target;

	private TRIGGER_GRUDGE_ACTION m_selectedGrudgeAction;

	public bool isShowing { get; private set; }

	private void Start()
	{
		Initialize();
		SubscribeListeners();
	}

	private void OnDestroy()
	{
		m_grudgeUIView?.Unsubscribe(this);
		UnsubscribeListeners();
		if (m_grudgeUIView != null && m_grudgeUIView.UIModel != null)
		{
			CleanUp();
		}
	}

	private void CleanUp()
	{
		Utilities.DestroyChildren(m_grudgeUIView.GetContainerParent());
		m_grudgeUIView.UIModel.items.Clear();
	}

	public void Initialize()
	{
		Init(playShowAnimation: false);
		HideUI();
	}

	private void SubscribeListeners()
	{
		Messenger.AddListener<List<TRIGGER_GRUDGE_ACTION>, Character, Character>(UISignals.SHOW_GRUDGE_UI, OnShowGrudgeUI);
	}

	private void UnsubscribeListeners()
	{
		Messenger.RemoveListener<List<TRIGGER_GRUDGE_ACTION>, Character, Character>(UISignals.SHOW_GRUDGE_UI, OnShowGrudgeUI);
	}

	private void OnShowGrudgeUI(List<TRIGGER_GRUDGE_ACTION> p_grudgeActionTypes, Character p_actor, Character p_target)
	{
		SetActorAndTarget(p_actor, p_target);
		SpawnItems(p_grudgeActionTypes);
		ShowUI();
		m_grudgeUIView.PlayShowAnimation();
	}

	public override void HideUI()
	{
		base.HideUI();
		isShowing = false;
		UIManager.Instance.Unpause();
		UIManager.Instance.SetSpeedTogglesState(state: true);
		UIManager.Instance.ResumeLastProgressionSpeed();
		InputManager.Instance.SetAllHotkeysEnabledState(p_state: true);
		InnerMapCameraMove.Instance.EnableMovement();
	}

	public override void ShowUI()
	{
		m_mvcUIView.ShowUI();
		isShowing = true;
		UIManager.Instance.Pause();
		UIManager.Instance.SetSpeedTogglesState(state: false);
		InputManager.Instance.SetAllHotkeysEnabledState(p_state: false);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Cancel, p_state: true);
		InnerMapCameraMove.Instance.DisableMovement();
	}

	public void Init(bool playShowAnimation)
	{
		InstantiateUI();
		if (playShowAnimation)
		{
			m_grudgeUIView.PlayShowAnimation();
		}
	}

	private void SetActorAndTarget(Character p_actor, Character p_target)
	{
		_actor = p_actor;
		_target = p_target;
	}

	private void SpawnItems(List<TRIGGER_GRUDGE_ACTION> p_grudgeActionTypes)
	{
		CleanUp();
		for (int i = 0; i < p_grudgeActionTypes.Count; i++)
		{
			GrudgeItemUI grudgeItemUI = UnityEngine.Object.Instantiate(m_grudgeItemUI, m_grudgeUIView.GetContainerParent(), worldPositionStays: true);
			grudgeItemUI.InitItem(p_grudgeActionTypes[i], _actor, _target);
			grudgeItemUI.onButtonClick = (Action<TRIGGER_GRUDGE_ACTION>)Delegate.Combine(grudgeItemUI.onButtonClick, new Action<TRIGGER_GRUDGE_ACTION>(OnGrudgeActionClick));
			m_grudgeUIView.UIModel.items.Add(grudgeItemUI);
		}
	}

	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		if (m_grudgeUIView == null)
		{
			DisplayMenuFirstTime();
		}
	}

	private void DisplayMenuFirstTime()
	{
		GrudgeUIView.Create(_canvas, m_grudgeUIModel, delegate(GrudgeUIView p_ui)
		{
			m_grudgeUIView = p_ui;
			m_grudgeUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
			int num = UIManager.Instance.contextMenuUIController.transform.GetSiblingIndex() + 2;
			m_grudgeUIView.UIModel.transform.SetSiblingIndex(num);
			ShowUI();
		});
	}

	public void OnCloseClicked()
	{
		m_grudgeUIView.PlayHideAnimation(HideUI);
	}

	private void OnGrudgeActionClick(TRIGGER_GRUDGE_ACTION p_type)
	{
		m_selectedGrudgeAction = p_type;
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Trigger_Grudge");
		string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Trigger_Grudge_Description");
		UIManager.Instance.ShowYesNoConfirmation(localizedValue, localizedValue2, OnYesDoGrudgeAction, null, showCover: true, 150);
	}

	private void OnYesDoGrudgeAction()
	{
		OnCloseClicked();
		(PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.TRIGGER_GRUDGE) as TriggerGrudgeData).TryActivateGrudge(m_selectedGrudgeAction, _actor, _target);
	}

	public void HideViaShortcutKey()
	{
		m_grudgeUIView.PlayHideAnimation(HideUI);
	}
}

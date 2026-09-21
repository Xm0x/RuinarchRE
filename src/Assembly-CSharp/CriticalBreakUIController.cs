using System;
using System.Collections.Generic;
using Ruinarch;
using Ruinarch.MVCFramework;
using UnityEngine;
using UtilityScripts;

public class CriticalBreakUIController : MVCUIController, CriticalBreakUIView.IListener
{
	[SerializeField]
	private CriticalBreakUIModel m_grudgeUIModel;

	private CriticalBreakUIView m_criticalBreakUIView;

	[SerializeField]
	private CriticalBreakItemUI m_criticalBreakItemUI;

	private Character _actor;

	private Dictionary<CRITICAL_BREAK_ACTION, object> _targets;

	private CRITICAL_BREAK_ACTION m_selectedGrudgeAction;

	public bool isShowing { get; private set; }

	private void Start()
	{
		Initialize();
		SubscribeListeners();
		_targets = new Dictionary<CRITICAL_BREAK_ACTION, object>();
		CRITICAL_BREAK_ACTION[] array = (CRITICAL_BREAK_ACTION[])Enum.GetValues(typeof(CRITICAL_BREAK_ACTION));
		for (int i = 0; i < array.Length; i++)
		{
			_targets.Add(array[i], null);
		}
	}

	private void OnDestroy()
	{
		m_criticalBreakUIView?.Unsubscribe(this);
		UnsubscribeListeners();
		if (m_criticalBreakUIView != null && m_criticalBreakUIView.UIModel != null)
		{
			CleanUp();
		}
		_targets?.Clear();
		_targets = null;
	}

	private void CleanUp()
	{
		Utilities.DestroyChildren(m_criticalBreakUIView.GetContainerParent());
		m_criticalBreakUIView.UIModel.items.Clear();
	}

	private void Initialize()
	{
		Init(playShowAnimation: false);
		HideUI();
	}

	private void SubscribeListeners()
	{
		Messenger.AddListener<List<CRITICAL_BREAK_ACTION>, Character>(UISignals.SHOW_CRITICAL_BREAK_UI, OnShowCriticalBreakUI);
	}

	private void UnsubscribeListeners()
	{
		Messenger.RemoveListener<List<CRITICAL_BREAK_ACTION>, Character>(UISignals.SHOW_CRITICAL_BREAK_UI, OnShowCriticalBreakUI);
	}

	private void OnShowCriticalBreakUI(List<CRITICAL_BREAK_ACTION> p_grudgeActionTypes, Character p_actor)
	{
		_actor = p_actor;
		SpawnItems(p_grudgeActionTypes);
		ShowUI();
		m_criticalBreakUIView.PlayShowAnimation();
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
			m_criticalBreakUIView.PlayShowAnimation();
		}
	}

	private void SpawnItems(List<CRITICAL_BREAK_ACTION> p_grudgeActionTypes)
	{
		CleanUp();
		CriticalBreakData criticalBreakData = PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.CRITICAL_BREAK) as CriticalBreakData;
		for (int i = 0; i < p_grudgeActionTypes.Count; i++)
		{
			CriticalBreakItemUI criticalBreakItemUI = UnityEngine.Object.Instantiate(m_criticalBreakItemUI, m_criticalBreakUIView.GetContainerParent(), worldPositionStays: true);
			CRITICAL_BREAK_ACTION cRITICAL_BREAK_ACTION = p_grudgeActionTypes[i];
			object criticalBreakTarget = criticalBreakData.GetCriticalBreakTarget(cRITICAL_BREAK_ACTION, _actor);
			_targets[cRITICAL_BREAK_ACTION] = criticalBreakTarget;
			criticalBreakItemUI.InitItem(cRITICAL_BREAK_ACTION, _actor, criticalBreakTarget);
			criticalBreakItemUI.onButtonClick = (Action<CriticalBreakItemUI>)Delegate.Combine(criticalBreakItemUI.onButtonClick, new Action<CriticalBreakItemUI>(OnCriticalBreakActionClick));
			m_criticalBreakUIView.UIModel.items.Add(criticalBreakItemUI);
		}
	}

	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		if (m_criticalBreakUIView == null)
		{
			DisplayMenuFirstTime();
		}
	}

	private void DisplayMenuFirstTime()
	{
		CriticalBreakUIView.Create(_canvas, m_grudgeUIModel, delegate(CriticalBreakUIView p_ui)
		{
			m_criticalBreakUIView = p_ui;
			m_criticalBreakUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
			int num = UIManager.Instance.contextMenuUIController.transform.GetSiblingIndex() + 2;
			m_criticalBreakUIView.UIModel.transform.SetSiblingIndex(num);
			ShowUI();
		});
	}

	public void OnCloseClicked()
	{
		m_criticalBreakUIView.PlayHideAnimation(HideUI);
	}

	private void OnCriticalBreakActionClick(CriticalBreakItemUI p_itemUI)
	{
		m_selectedGrudgeAction = p_itemUI.criticalBreakActionType;
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Trigger_Critical_Break");
		string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Trigger_Critical_Break_Description");
		UIManager.Instance.ShowYesNoConfirmation(localizedValue, localizedValue2, delegate
		{
			OnYesDoCriticalBreakAction(p_itemUI);
		}, null, showCover: true, 150);
	}

	private void OnYesDoCriticalBreakAction(CriticalBreakItemUI p_itemUI)
	{
		OnCloseClicked();
		(PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.CRITICAL_BREAK) as CriticalBreakData).TryActivateCriticalBreak(p_target: _targets[m_selectedGrudgeAction], p_actionType: m_selectedGrudgeAction, p_actor: _actor, p_actionDescription: p_itemUI.txtDescription.text);
	}

	public void HideViaShortcutKey()
	{
		m_criticalBreakUIView.PlayHideAnimation(HideUI);
	}
}

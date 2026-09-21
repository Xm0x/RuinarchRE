using System;
using Ruinarch.MVCFramework;
using UnityEngine;

public class UnlockMinionUIController : MVCUIController, UnlockMinionUIView.IListener
{
	[SerializeField]
	private UnlockMinionUIModel m_unlockMinionUIModel;

	private UnlockMinionUIView m_unlockMinionUIView;

	private void OnEnable()
	{
		UnlockMinionItemUI.onClickUnlockMinion = (Action<PLAYER_SKILL_TYPE, int>)Delegate.Combine(UnlockMinionItemUI.onClickUnlockMinion, new Action<PLAYER_SKILL_TYPE, int>(OnChooseMinionToUnlock));
	}

	private void OnDisable()
	{
		UnlockMinionItemUI.onClickUnlockMinion = (Action<PLAYER_SKILL_TYPE, int>)Delegate.Remove(UnlockMinionItemUI.onClickUnlockMinion, new Action<PLAYER_SKILL_TYPE, int>(OnChooseMinionToUnlock));
	}

	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		UnlockMinionUIView.Create(_canvas, m_unlockMinionUIModel, delegate(UnlockMinionUIView p_ui)
		{
			m_unlockMinionUIView = p_ui;
			m_unlockMinionUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
			int num = UIManager.Instance.structureInfoUI.transform.GetSiblingIndex() + 1;
			m_unlockMinionUIView.UIModel.transform.SetSiblingIndex(num);
			Initialize();
		});
	}

	private void OnDestroy()
	{
		m_unlockMinionUIView?.Unsubscribe(this);
	}

	private void Initialize()
	{
		for (int i = 0; i < PlayerSkillManager.Instance.allMinionPlayerSkills.Length; i++)
		{
			PLAYER_SKILL_TYPE minionType = PlayerSkillManager.Instance.allMinionPlayerSkills[i];
			UnlockMinionItemUI obj = m_unlockMinionUIView.UIModel.minionItems[i];
			obj.SetMinionType(minionType);
			obj.SetCoverState(p_state: false);
			obj.SetCheckmarkState(p_state: false);
		}
		Messenger.AddListener(UISignals.START_GAME_AFTER_LOADOUT_SELECT, InitializeAfterLoadoutSelected);
	}

	private void InitializeAfterLoadoutSelected()
	{
		Messenger.RemoveListener(UISignals.START_GAME_AFTER_LOADOUT_SELECT, InitializeAfterLoadoutSelected);
		m_unlockMinionUIView.UpdateMinionItemsSelectableStates();
	}

	public override void ShowUI()
	{
		m_mvcUIView.ShowUI();
		m_unlockMinionUIView.UpdateMinionItemsSelectableStates();
		UIManager.Instance.Pause();
		UIManager.Instance.SetSpeedTogglesState(state: false);
	}

	public override void HideUI()
	{
		base.HideUI();
		UIManager.Instance.SetSpeedTogglesState(state: true);
		UIManager.Instance.ResumeLastProgressionSpeed();
	}

	public void OnClickClose()
	{
		HideUI();
	}

	private void OnChooseMinionToUnlock(PLAYER_SKILL_TYPE p_minionType, int p_unlockCost)
	{
		PlayerManager.Instance.player.currenciesComponent.AdjustMana(-p_unlockCost);
		HideUI();
	}
}

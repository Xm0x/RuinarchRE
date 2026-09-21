using System;
using Ruinarch.MVCFramework;
using UnityEngine;

public class UnlockStructureUIController : MVCUIController, UnlockStructureUIView.IListener
{
	[SerializeField]
	private UnlockStructureUIModel m_unlockStructureUIModel;

	private UnlockStructureUIView m_unlockStructureUIView;

	private void OnEnable()
	{
		UnlockStructureItemUI.onClickUnlockStructure = (Action<PLAYER_SKILL_TYPE, int>)Delegate.Combine(UnlockStructureItemUI.onClickUnlockStructure, new Action<PLAYER_SKILL_TYPE, int>(OnChooseStructureToUnlock));
	}

	private void OnDisable()
	{
		UnlockStructureItemUI.onClickUnlockStructure = (Action<PLAYER_SKILL_TYPE, int>)Delegate.Remove(UnlockStructureItemUI.onClickUnlockStructure, new Action<PLAYER_SKILL_TYPE, int>(OnChooseStructureToUnlock));
	}

	public override void ShowUI()
	{
		m_mvcUIView.ShowUI();
		m_unlockStructureUIView.UpdateStructureItemsSelectableStates();
	}

	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		UnlockStructureUIView.Create(_canvas, m_unlockStructureUIModel, delegate(UnlockStructureUIView p_ui)
		{
			m_unlockStructureUIView = p_ui;
			m_unlockStructureUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
			int num = UIManager.Instance.structureInfoUI.transform.GetSiblingIndex() + 1;
			m_unlockStructureUIView.UIModel.transform.SetSiblingIndex(num);
			Initialize();
		});
	}

	private void OnDestroy()
	{
		m_unlockStructureUIView?.Unsubscribe(this);
	}

	private void Initialize()
	{
		for (int i = 0; i < PlayerSkillManager.Instance.allDemonicStructureSkills.Length; i++)
		{
			PLAYER_SKILL_TYPE structureType = PlayerSkillManager.Instance.allDemonicStructureSkills[i];
			UnlockStructureItemUI obj = m_unlockStructureUIView.UIModel.structureItems[i];
			obj.SetStructureType(structureType);
			obj.SetCoverState(p_state: false);
		}
		Messenger.AddListener(UISignals.START_GAME_AFTER_LOADOUT_SELECT, InitializeAfterLoadoutSelected);
	}

	private void InitializeAfterLoadoutSelected()
	{
		Messenger.RemoveListener(UISignals.START_GAME_AFTER_LOADOUT_SELECT, InitializeAfterLoadoutSelected);
		m_unlockStructureUIView.UpdateStructureItemsSelectableStates();
	}

	public void OnClickClose()
	{
		HideUI();
	}

	private void OnChooseStructureToUnlock(PLAYER_SKILL_TYPE p_structureType, int p_unlockCost)
	{
		PlayerManager.Instance.player.currenciesComponent.AdjustMana(-p_unlockCost);
		HideUI();
	}
}

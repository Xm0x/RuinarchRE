using System;
using System.Collections.Generic;
using DG.Tweening;
using Ruinarch;
using UnityEngine;

public class GainedPowersPopup : PopupMenuBase
{
	[SerializeField]
	private GainedPowersPopupItem[] items;

	[SerializeField]
	private CanvasGroup cgMainPanel;

	[SerializeField]
	private GameObject mainPanel;

	private Action _onCloseAction;

	public void Show(List<PLAYER_SKILL_TYPE> p_skills, Action p_onCloseAction)
	{
		OnGameObjectEnabled();
		_onCloseAction = p_onCloseAction;
		UIManager.Instance.Pause();
		UIManager.Instance.SetSpeedTogglesState(state: false);
		InputManager.Instance.SetAllHotkeysEnabledState(p_state: false);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Cancel, p_state: true);
		InnerMapCameraMove.Instance.DisableMovement();
		for (int i = 0; i < items.Length; i++)
		{
			GainedPowersPopupItem gainedPowersPopupItem = items[i];
			if (p_skills.IsIndexInList(i))
			{
				PLAYER_SKILL_TYPE p_type = p_skills[i];
				gainedPowersPopupItem.InitItem(p_type);
				gainedPowersPopupItem.gameObject.SetActive(value: true);
			}
			else
			{
				gainedPowersPopupItem.gameObject.SetActive(value: false);
			}
		}
		cgMainPanel.alpha = 0f;
		mainPanel.SetActive(value: true);
		Sequence sequence = DOTween.Sequence();
		sequence.Join(cgMainPanel.DOFade(1f, 0.5f));
		sequence.AppendInterval(0.02f);
		for (int j = 0; j < items.Length; j++)
		{
			GainedPowersPopupItem gainedPowersPopupItem2 = items[j];
			if (gainedPowersPopupItem2.gameObject.activeSelf)
			{
				sequence.Join(gainedPowersPopupItem2.PrepareAnimation().SetDelay((float)j / 5f));
			}
		}
		sequence.Play();
	}

	public override void Close()
	{
		OnGameObjectDisabled();
		UIManager.Instance.ResumeLastProgressionSpeed();
		InputManager.Instance.SetAllHotkeysEnabledState(p_state: true);
		InnerMapCameraMove.Instance.EnableMovement();
		cgMainPanel.alpha = 1f;
		Sequence sequence = DOTween.Sequence();
		sequence.Join(cgMainPanel.DOFade(0f, 0.5f));
		sequence.OnComplete(delegate
		{
			mainPanel.SetActive(value: false);
		});
		sequence.Play();
		_onCloseAction?.Invoke();
		_onCloseAction = null;
	}
}

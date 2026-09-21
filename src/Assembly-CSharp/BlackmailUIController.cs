using System;
using System.Collections.Generic;
using Ruinarch.MVCFramework;
using UnityEngine;

public class BlackmailUIController : MVCUIController, BlackmailUIView.IListener
{
	[SerializeField]
	private BlackmailUIModel m_blackmailUIModel;

	private BlackmailUIView m_blackmailUIView;

	private List<IIntel> _chosenBlackmail;

	private Action<List<IIntel>> _onConfirmAction;

	private void OnEnable()
	{
		BlackmailUIItem.onChooseBlackmail = (Action<IIntel, bool>)Delegate.Combine(BlackmailUIItem.onChooseBlackmail, new Action<IIntel, bool>(OnChooseIntel));
		BlackmailUIItem.onHoverOverBlackmail = (Action<IIntel, UIHoverPosition>)Delegate.Combine(BlackmailUIItem.onHoverOverBlackmail, new Action<IIntel, UIHoverPosition>(OnHoverOverBlackmail));
		BlackmailUIItem.onHoverOutBlackmail = (Action<IIntel>)Delegate.Combine(BlackmailUIItem.onHoverOutBlackmail, new Action<IIntel>(OnHoverOutBlackmail));
	}

	private void OnDisable()
	{
		BlackmailUIItem.onChooseBlackmail = (Action<IIntel, bool>)Delegate.Remove(BlackmailUIItem.onChooseBlackmail, new Action<IIntel, bool>(OnChooseIntel));
		BlackmailUIItem.onHoverOverBlackmail = (Action<IIntel, UIHoverPosition>)Delegate.Remove(BlackmailUIItem.onHoverOverBlackmail, new Action<IIntel, UIHoverPosition>(OnHoverOverBlackmail));
		BlackmailUIItem.onHoverOutBlackmail = (Action<IIntel>)Delegate.Remove(BlackmailUIItem.onHoverOutBlackmail, new Action<IIntel>(OnHoverOutBlackmail));
	}

	private void Awake()
	{
		_chosenBlackmail = new List<IIntel>();
	}

	private void OnDestroy()
	{
		m_blackmailUIView?.Unsubscribe(this);
	}

	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		BlackmailUIView.Create(_canvas, m_blackmailUIModel, delegate(BlackmailUIView p_ui)
		{
			m_blackmailUIView = p_ui;
			m_blackmailUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
		});
	}

	public void ShowBlackmailUI(List<IIntel> p_blackmail, List<IIntel> p_alreadyChosenBlackmail, Action<List<IIntel>> p_onConfirmAction)
	{
		ShowUI();
		_chosenBlackmail.Clear();
		m_blackmailUIView.DisplayBlackmailItems(p_blackmail, p_alreadyChosenBlackmail);
		_onConfirmAction = p_onConfirmAction;
	}

	private void OnChooseIntel(IIntel p_intel, bool p_isOn)
	{
		if (p_isOn)
		{
			_chosenBlackmail.Add(p_intel);
			Debug.Log("Chosen intel " + p_intel?.log.logText);
		}
		else
		{
			_chosenBlackmail.Remove(p_intel);
			Debug.Log("Remove intel " + p_intel?.log.logText);
		}
	}

	private void OnHoverOverBlackmail(IIntel p_blackmail, UIHoverPosition p_hoverPosition)
	{
		string fullIntelTooltip = p_blackmail.GetFullIntelTooltip();
		UIManager.Instance.ShowSmallInfo(fullIntelTooltip, p_hoverPosition, "", autoReplaceText: false, relayout: true);
	}

	private void OnHoverOutBlackmail(IIntel p_blackmail)
	{
		UIManager.Instance.HideSmallInfo();
	}

	public void OnClickClose()
	{
		HideUI();
	}

	public void OnClickConfirm()
	{
		HideUI();
		_onConfirmAction?.Invoke(_chosenBlackmail);
	}
}

using System;
using System.Collections.Generic;
using Ruinarch.MVCFramework;
using UnityEngine;
using UtilityScripts;

public class ContextMenuUIController : MVCUIController, ContextMenuUIView.IListener
{
	[SerializeField]
	private ContextMenuUIModel m_contextMenuUIModel;

	private ContextMenuUIView m_contextMenuUIView;

	private bool m_hasVectorPositionToFollow;

	private Action<IContextMenuItem, UIHoverPosition> _onHoverOverAction;

	private Action<IContextMenuItem> _onHoverOutAction;

	public IContextMenuItem currentlyOpenedParentContextItem { get; private set; }

	private void OnEnable()
	{
		ContextMenuUIObject.onMenuPress = (Action<IContextMenuItem, bool, int>)Delegate.Combine(ContextMenuUIObject.onMenuPress, new Action<IContextMenuItem, bool, int>(OnMenuClicked));
		ContextMenuUIObject.onHoverOverItem = (Action<IContextMenuItem, bool, int>)Delegate.Combine(ContextMenuUIObject.onHoverOverItem, new Action<IContextMenuItem, bool, int>(OnMenuHoveredOver));
		ContextMenuUIObject.onHoverOutItem = (Action<IContextMenuItem, bool, int>)Delegate.Combine(ContextMenuUIObject.onHoverOutItem, new Action<IContextMenuItem, bool, int>(OnMenuHoveredOut));
	}

	private void OnDisable()
	{
		ContextMenuUIObject.onMenuPress = (Action<IContextMenuItem, bool, int>)Delegate.Remove(ContextMenuUIObject.onMenuPress, new Action<IContextMenuItem, bool, int>(OnMenuClicked));
		ContextMenuUIObject.onHoverOverItem = (Action<IContextMenuItem, bool, int>)Delegate.Remove(ContextMenuUIObject.onHoverOverItem, new Action<IContextMenuItem, bool, int>(OnMenuHoveredOver));
		ContextMenuUIObject.onHoverOutItem = (Action<IContextMenuItem, bool, int>)Delegate.Remove(ContextMenuUIObject.onHoverOutItem, new Action<IContextMenuItem, bool, int>(OnMenuHoveredOut));
	}

	private void Awake()
	{
		InstantiateUI();
		HideUI();
	}

	private void OnDestroy()
	{
		m_contextMenuUIView?.Unsubscribe(this);
	}

	private void Start()
	{
		Messenger.AddListener<KeyCode>(ControlsSignals.KEY_DOWN_EMPTY_SPACE, OnReceiveKeyCodeSignal);
	}

	public void SetOnHoverOverAction(Action<IContextMenuItem, UIHoverPosition> p_onHoverOverAction)
	{
		_onHoverOverAction = p_onHoverOverAction;
	}

	public void SetOnHoverOutAction(Action<IContextMenuItem> p_onHoverOutAction)
	{
		_onHoverOutAction = p_onHoverOutAction;
	}

	public override void HideUI()
	{
		base.HideUI();
		m_contextMenuUIView.HideColumn(1);
	}

	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		ContextMenuUIView.Create(_canvas, m_contextMenuUIModel, delegate(ContextMenuUIView p_ui)
		{
			m_contextMenuUIView = p_ui;
			m_contextMenuUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
		});
	}

	private void OnMenuClicked(IContextMenuItem p_UIMenu, bool p_isAction, int p_currentColumn)
	{
		if (!p_isAction)
		{
			if (p_UIMenu.CanBePickedRegardlessOfCooldown())
			{
				currentlyOpenedParentContextItem = p_UIMenu;
				bool dontShowName = false;
				if (PlayerManager.Instance.player.currentlySelectedPlayerActionTarget is Character { isInfoUnlocked: false } && (p_UIMenu is TriggerFlawData || p_UIMenu is RemoveFlawData || p_UIMenu is RemoveBuffData || p_UIMenu is TriggerGrudgeData))
				{
					dontShowName = true;
				}
				m_contextMenuUIView.DisplaySubMenu(p_UIMenu.subMenus, p_currentColumn + 1, _canvas, dontShowName);
			}
		}
		else
		{
			p_UIMenu.OnPickAction();
		}
	}

	private void OnMenuHoveredOver(IContextMenuItem p_UIMenu, bool p_isAction, int p_currentColumn)
	{
		_onHoverOverAction?.Invoke(p_UIMenu, m_contextMenuUIView.GetTooltipHoverPositionToUse());
	}

	private void OnMenuHoveredOut(IContextMenuItem p_UIMenu, bool p_isAction, int p_currentColumn)
	{
		_onHoverOutAction?.Invoke(p_UIMenu);
	}

	public void ShowContextMenu(List<IContextMenuItem> p_initialItems, Vector3 p_screenPos, string p_title, Cursor_Type p_cursorType)
	{
		m_contextMenuUIView.HideColumn(1);
		ShowUI();
		m_contextMenuUIView.InitializeUI(p_initialItems, _canvas);
		m_contextMenuUIView.SetPosition(p_screenPos, _canvas);
		RectTransform rectTransform = m_contextMenuUIView.UIModel.parentDisplay.transform as RectTransform;
		GameUtilities.PositionTooltip(p_screenPos, m_contextMenuUIView.UIModel.parentDisplay.gameObject, rectTransform, rectTransform, p_cursorType, _canvas.transform as RectTransform);
		m_contextMenuUIView.SetTitleName(p_title);
	}

	public void ShowContextMenu(List<IContextMenuItem> p_initialItems, string p_title)
	{
		m_contextMenuUIView.HideColumn(1);
		ShowUI();
		m_contextMenuUIView.InitializeUI(p_initialItems, _canvas);
		m_contextMenuUIView.SetTitleName(p_title);
	}

	public void UpdateContextMenuItems(List<IContextMenuItem> p_initialItems)
	{
		m_contextMenuUIView.InitializeUI(p_initialItems, _canvas);
	}

	public void SetFollowPosition(Vector3 p_pos, bool p_isScreenPosition)
	{
		m_hasVectorPositionToFollow = true;
	}

	public void SetFollowPosition(Transform p_transformToFollow)
	{
		m_hasVectorPositionToFollow = false;
	}

	public bool IsShowing()
	{
		return m_contextMenuUIView.UIModel.parentDisplay.gameObject.activeSelf;
	}

	public void OnHoverOverParentDisplay()
	{
	}

	public void OnHoverOutParentDisplay()
	{
	}

	private void OnReceiveKeyCodeSignal(KeyCode p_key)
	{
		if (p_key == KeyCode.Mouse1)
		{
			HideUI();
		}
	}
}

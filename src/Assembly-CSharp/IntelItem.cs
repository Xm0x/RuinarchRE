using System;
using System.Collections.Generic;
using Ruinarch;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UtilityScripts;

public class IntelItem : MonoBehaviour
{
	public delegate void OnClickAction(IIntel intel);

	private OnClickAction onClickAction;

	private List<Action> otherClickActions;

	private Action onHoverEnterAction;

	private Action onHoverExitAction;

	[SerializeField]
	private TextMeshProUGUI infoLbl;

	[SerializeField]
	private Toggle shareToggle;

	[SerializeField]
	private LogItem logItem;

	public IIntel intel { get; private set; }

	private void Awake()
	{
		Messenger.AddListener<IIntel>(UISignals.INTEL_LOG_UPDATED, OnIntelLogUpdated);
		UpdateIntelText();
	}

	private void OnDestroy()
	{
		Messenger.RemoveListener<IIntel>(UISignals.INTEL_LOG_UPDATED, OnIntelLogUpdated);
	}

	public void SetIntel(IIntel intel)
	{
		this.intel = intel;
		otherClickActions = new List<Action>();
		ClearClickActions();
		SetClickedState(isClicked: false);
		UpdateIntelText();
		if (intel != null)
		{
			shareToggle.interactable = true;
			shareToggle.gameObject.SetActive(value: true);
		}
		else
		{
			shareToggle.interactable = false;
			shareToggle.gameObject.SetActive(value: false);
		}
	}

	private void OnIntelLogUpdated(IIntel p_intel)
	{
		if (intel == p_intel)
		{
			UpdateIntelText();
		}
	}

	private void UpdateIntelText()
	{
		if (intel != null)
		{
			infoLbl.text = intel.log.logText;
		}
		else
		{
			infoLbl.text = "";
		}
	}

	public void SetOnHoverEnterAction(Action action)
	{
		onHoverEnterAction = action;
	}

	public void SetOnHoverExitAction(Action action)
	{
		onHoverExitAction = action;
	}

	public void OnHoverEnter()
	{
		onHoverEnterAction?.Invoke();
	}

	public void OnHoverExit()
	{
		onHoverExitAction?.Invoke();
	}

	public void SetClickAction(OnClickAction clickAction)
	{
		onClickAction = clickAction;
	}

	public void AddOtherClickAction(Action clickAction)
	{
		if (otherClickActions != null)
		{
			otherClickActions.Add(clickAction);
		}
	}

	public void OnClick()
	{
		onClickAction?.Invoke(intel);
		for (int i = 0; i < otherClickActions.Count; i++)
		{
			otherClickActions[i]();
		}
	}

	public void ClearClickActions()
	{
		onClickAction = null;
		otherClickActions.Clear();
	}

	public void SetClickedState(bool isClicked)
	{
		shareToggle.SetIsOnWithoutNotify(isClicked);
	}

	public void OnPointerClick(BaseEventData eventData)
	{
		if (eventData is PointerEventData { button: PointerEventData.InputButton.Right })
		{
			PlayerManager.Instance.shareIntelContextMenuItem.SetCurrentIntel(intel);
			List<IContextMenuItem> list = RuinarchListPool<IContextMenuItem>.Claim();
			list.Add(PlayerManager.Instance.shareIntelContextMenuItem);
			UIManager.Instance.ShowContextMenu(list, InputManager.Instance.mousePosition, p_isScreenPosition: true, "Share Intel");
			RuinarchListPool<IContextMenuItem>.Release(list);
		}
	}
}

using Ruinarch;
using UnityEngine;

public class PrismEventsUI : PopupMenuBase
{
	[SerializeField]
	private PrismEventItem[] items;

	[SerializeField]
	private CanvasGroup cgMainPanel;

	[SerializeField]
	private GameObject mainPanel;

	public void Initialize(PrismEvent[] p_events)
	{
		for (int i = 0; i < p_events.Length; i++)
		{
			PrismEvent p_event = p_events[i];
			items[i].Initialize(p_event);
		}
	}

	public void Show()
	{
		UIManager.Instance.Pause();
		UIManager.Instance.SetSpeedTogglesState(state: false);
		InputManager.Instance.SetAllHotkeysEnabledState(p_state: false);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Cancel, p_state: true);
		InnerMapCameraMove.Instance.DisableMovement();
		for (int i = 0; i < items.Length; i++)
		{
			PrismEventItem obj = items[i];
			obj.Show();
			obj.SetOnClickAction(OnClickPrismEvent);
			obj.SetInteractableState(PlayerManager.Instance.player.playerSkillComponent.prismEvents[i].AreRequirementsMet());
		}
		mainPanel.SetActive(value: true);
	}

	public override void Close()
	{
		UIManager.Instance.ResumeLastProgressionSpeed();
		InputManager.Instance.SetAllHotkeysEnabledState(p_state: true);
		InnerMapCameraMove.Instance.EnableMovement();
		mainPanel.SetActive(value: false);
	}

	private void UpdateInteractableStates()
	{
		for (int i = 0; i < items.Length; i++)
		{
			items[i].SetInteractableState(PlayerManager.Instance.player.playerSkillComponent.prismEvents[i].AreRequirementsMet());
		}
	}

	private void OnClickPrismEvent(PrismEvent p_event)
	{
		p_event.TriggerEvent();
		UpdateInteractableStates();
	}

	private PrismEventItem GetPrismEventItem(PrismEvent p_event)
	{
		for (int i = 0; i < items.Length; i++)
		{
			PrismEventItem prismEventItem = items[i];
			if (prismEventItem.prismEvent == p_event)
			{
				return prismEventItem;
			}
		}
		return null;
	}

	public void UpdateRequirement(PrismEvent p_event)
	{
		PrismEventItem prismEventItem = GetPrismEventItem(p_event);
		if (prismEventItem != null)
		{
			prismEventItem.UpdateAllRequirements();
		}
	}
}

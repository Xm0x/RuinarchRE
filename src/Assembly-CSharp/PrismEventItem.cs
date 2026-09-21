using System.Collections.Generic;
using Ruinarch.Custom_UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UtilityScripts;

public class PrismEventItem : MonoBehaviour
{
	[SerializeField]
	private RuinarchButton _triggerButton;

	[SerializeField]
	private RuinarchText _triggerButtonLbl;

	[SerializeField]
	private RuinarchText _eventNameLbl;

	[SerializeField]
	private Image _activatedEventVisualEffectImage;

	[SerializeField]
	private Transform _eventRequirementPanel;

	[SerializeField]
	private GameObject _eventRequirementPrefab;

	[SerializeField]
	private List<PrismEventRequirementItem> _eventRequirements;

	private PrismEvent _event;

	private UnityAction<PrismEvent> _onClickAction;

	private string _eventDescription;

	public PrismEvent prismEvent => _event;

	public void Initialize(PrismEvent p_event)
	{
		_event = p_event;
		_eventNameLbl.text = _event.data.eventName;
		_eventDescription = _event.data.eventDescription;
		ConstructRequirements();
	}

	public void Show()
	{
		_triggerButtonLbl.text = string.Format("{0} {1}{2}", LocalizationManager.Instance.GetLocalizedValue("PrismEvents_Table", "Trigger"), Utilities.ManaIcon(), _event.GetManaCost());
		UpdateAllRequirements();
		UpdateEvent();
	}

	public void SetInteractableState(bool p_state)
	{
		_triggerButton.interactable = p_state;
	}

	public void SetOnClickAction(UnityAction<PrismEvent> p_action)
	{
		_onClickAction = p_action;
	}

	public void OnClick()
	{
		_onClickAction?.Invoke(_event);
	}

	private void ConstructRequirements()
	{
		for (int i = 0; i < _event.requirements.Length; i++)
		{
			PrismEventRequirement p_requirement = _event.requirements[i];
			if (i >= _eventRequirements.Count)
			{
				CreateNewPrismRequirementPrefab();
			}
			_eventRequirements[i].Initialize(p_requirement);
		}
		for (int j = 0; j < _eventRequirements.Count; j++)
		{
			PrismEventRequirementItem prismEventRequirementItem = _eventRequirements[j];
			prismEventRequirementItem.gameObject.SetActive(prismEventRequirementItem.IsUsed());
		}
	}

	private void CreateNewPrismRequirementPrefab()
	{
		PrismEventRequirementItem component = Object.Instantiate(_eventRequirementPrefab, _eventRequirementPanel).GetComponent<PrismEventRequirementItem>();
		_eventRequirements.Add(component);
	}

	public void UpdateAllRequirements()
	{
		for (int i = 0; i < _eventRequirements.Count; i++)
		{
			PrismEventRequirementItem prismEventRequirementItem = _eventRequirements[i];
			if (prismEventRequirementItem.IsUsed())
			{
				prismEventRequirementItem.UpdateRequirementToggle();
			}
		}
	}

	private void UpdateEvent()
	{
		if (!_event.isActivated)
		{
			_activatedEventVisualEffectImage.color = Color.green;
		}
		else
		{
			_activatedEventVisualEffectImage.color = Color.red;
		}
	}

	public void OnHoverEnterTriggerButton()
	{
		string text = string.Empty;
		if (_event.isActivated)
		{
			text = LocalizationManager.Instance.GetLocalizedValue("PrismEvents_Table", "Already_Activated");
		}
		else if (PlayerManager.Instance.player.currenciesComponent.mana < _event.GetManaCost())
		{
			text = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "No_Mana");
		}
		else if (!_event.AreRequirementsMet())
		{
			text = LocalizationManager.Instance.GetLocalizedValue("PrismEvents_Table", "Requirements_Not_Met");
		}
		if (!string.IsNullOrEmpty(text))
		{
			text = Utilities.ColorizeInvalidText(text);
			UIManager.Instance.ShowSmallInfo(text);
		}
	}

	public void OnHoverExitTriggerButton()
	{
		UIManager.Instance.HideSmallInfo();
	}

	public void OnHoverEnterEventName()
	{
		UIManager.Instance.ShowSmallInfo(_eventDescription);
	}

	public void OnHoverExitEventName()
	{
		UIManager.Instance.HideSmallInfo();
	}
}

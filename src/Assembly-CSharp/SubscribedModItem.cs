using System;
using LapinerTools.Steam;
using Ruinarch.Custom_UI;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

public class SubscribedModItem : MonoBehaviour
{
	[SerializeField]
	private RuinarchText _nameText;

	[SerializeField]
	private Toggle _toggle;

	[SerializeField]
	private Toggle _installTogggle;

	[SerializeField]
	private RuinarchButton _addButton;

	[SerializeField]
	private RuinarchButton _minusButton;

	[SerializeField]
	private GameObject _needsUpdateGO;

	private Action<SubscribedModItem, bool> _onToggleSMI;

	private Action<SubscribedModItem> _onIncreasedPriority;

	private Action<SubscribedModItem> _onDecreasedPriority;

	private InstalledModData _modData;

	public InstalledModData modData => _modData;

	public bool isUpdateNeeded { get; private set; }

	public void Initialize(InstalledModData modData)
	{
		_modData = modData;
		UpdateSMI();
	}

	public void SetOnToggleSMI(Action<SubscribedModItem, bool> p_toggleAction)
	{
		_onToggleSMI = p_toggleAction;
	}

	public void SetToggleGroup(ToggleGroup p_group)
	{
		_toggle.group = p_group;
	}

	public void UpdateSMI()
	{
		ResetSteamDetails();
		if (_modData.isLocal)
		{
			_nameText.text = "[L] " + _modData.itemInfo.Name;
		}
		else
		{
			_nameText.text = _modData.itemInfo.Name;
			CheckSteamDetails();
		}
		_installTogggle.isOn = _modData.applyMod;
	}

	private void CheckSteamDetails()
	{
		if (SteamManager.Initialized)
		{
			EItemState itemState = (EItemState)SteamUGC.GetItemState(_modData.publishedField);
			isUpdateNeeded = SteamWorkshopMain.Instance.IsUpdateNeeded(itemState);
			if (isUpdateNeeded)
			{
				_needsUpdateGO.SetActive(value: true);
			}
		}
	}

	private void ResetSteamDetails()
	{
		_needsUpdateGO.SetActive(value: false);
	}

	public bool ShouldInstall()
	{
		return _installTogggle.isOn;
	}

	public void ForceToggleSMI(bool p_state)
	{
		_toggle.isOn = p_state;
	}

	public void OnToggleSMI(bool p_state)
	{
		_onToggleSMI?.Invoke(this, p_state);
	}

	public void IncreasePriority()
	{
		_onIncreasedPriority?.Invoke(this);
	}

	public void DecreasePriority()
	{
		_onDecreasedPriority?.Invoke(this);
	}

	public void SetOnIncreasedPriorityAction(Action<SubscribedModItem> p_action)
	{
		_onIncreasedPriority = p_action;
	}

	public void SetOnDecreasedPriorityAction(Action<SubscribedModItem> p_action)
	{
		_onDecreasedPriority = p_action;
	}

	public void ResetItem()
	{
		_modData = null;
		_installTogggle.isOn = false;
		_onIncreasedPriority = null;
		_onDecreasedPriority = null;
		_toggle.isOn = false;
	}
}

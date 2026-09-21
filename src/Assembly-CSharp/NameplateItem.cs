using System;
using System.Collections.Generic;
using EZObjectPools;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class NameplateItem<T> : PooledObject, INameplateItem
{
	public delegate void OnHoverEnterNameplate(T obj);

	public delegate void OnHoverExitNameplate(T obj);

	public delegate void OnClickNameplate(T obj);

	public delegate void OnRightClickNameplate(T obj);

	public delegate void OnToggleNameplate(T obj, bool isOn);

	[Header("Main Content")]
	[SerializeField]
	protected TextMeshProUGUI mainLbl;

	[SerializeField]
	protected TextMeshProUGUI subLbl;

	[SerializeField]
	protected TextMeshProUGUI supportingLbl;

	[SerializeField]
	protected NameplateButton nameplateButton;

	[Header("Button")]
	[SerializeField]
	protected Button button;

	[FormerlySerializedAs("toggle")]
	[Header("Toggle")]
	[SerializeField]
	protected Toggle _toggle;

	[Header("Cover")]
	[SerializeField]
	protected GameObject coverGO;

	protected int m_displayRemainingChargeText;

	protected int m_displayMaxChrageText;

	private OnHoverEnterNameplate onHoverEnterNameplate;

	private OnHoverExitNameplate onHoverExitNameplate;

	private OnClickNameplate onClickNameplate;

	private OnRightClickNameplate onRightClickNameplate;

	private OnToggleNameplate onToggleNameplate;

	private Dictionary<string, Delegate> signals;

	public virtual T obj { get; private set; }

	public bool coverState => coverGO.activeSelf;

	public Toggle toggle => _toggle;

	public bool isInteractable
	{
		get
		{
			if (button.interactable)
			{
				return toggle.interactable;
			}
			return false;
		}
	}

	public virtual void SetObject(T o)
	{
		obj = o;
		nameplateButton.SetNameplateItem(this);
	}

	public virtual void UpdateObject(T o)
	{
		obj = o;
		nameplateButton.SetNameplateItem(this);
	}

	public override void Reset()
	{
		base.Reset();
		onHoverEnterNameplate = null;
		onHoverExitNameplate = null;
		onClickNameplate = null;
		onRightClickNameplate = null;
		onToggleNameplate = null;
		toggle.SetIsOnWithoutNotify(value: false);
		obj = default(T);
		SetToggleGroup(null);
		SetInteractableState(state: true);
	}

	public void AddHoverEnterAction(OnHoverEnterNameplate action)
	{
		onHoverEnterNameplate = (OnHoverEnterNameplate)Delegate.Combine(onHoverEnterNameplate, action);
	}

	public void RemoveHoverEnterAction(OnHoverEnterNameplate action)
	{
		onHoverEnterNameplate = (OnHoverEnterNameplate)Delegate.Remove(onHoverEnterNameplate, action);
	}

	public void ClearAllHoverEnterActions()
	{
		onHoverEnterNameplate = null;
	}

	public virtual void OnHoverEnter()
	{
		onHoverEnterNameplate?.Invoke(obj);
	}

	private void ProcessButtonBaseOnChargeDisplay()
	{
		if (m_displayRemainingChargeText <= 0)
		{
			SetInteractableState(state: false);
		}
		else
		{
			SetInteractableState(state: true);
		}
	}

	public virtual void IncreaseOneChargeForDisplayPurpose()
	{
		m_displayRemainingChargeText = Mathf.Clamp(m_displayRemainingChargeText + 1, 0, m_displayMaxChrageText);
		ProcessButtonBaseOnChargeDisplay();
		subLbl.text = m_displayRemainingChargeText + "/" + m_displayMaxChrageText;
	}

	public virtual void DeductOneChargeForDisplayPurpose()
	{
		m_displayRemainingChargeText = Mathf.Clamp(m_displayRemainingChargeText - 1, 0, m_displayMaxChrageText);
		ProcessButtonBaseOnChargeDisplay();
		subLbl.text = m_displayRemainingChargeText + "/" + m_displayMaxChrageText;
	}

	public void AddHoverExitAction(OnHoverExitNameplate action)
	{
		onHoverExitNameplate = (OnHoverExitNameplate)Delegate.Combine(onHoverExitNameplate, action);
	}

	public void RemoveHoverExitAction(OnHoverExitNameplate action)
	{
		onHoverExitNameplate = (OnHoverExitNameplate)Delegate.Remove(onHoverExitNameplate, action);
	}

	public void ClearAllHoverExitActions()
	{
		onHoverExitNameplate = null;
	}

	public virtual void OnHoverExit()
	{
		onHoverExitNameplate?.Invoke(obj);
	}

	public void SetAsButton()
	{
		button.gameObject.SetActive(value: true);
		toggle.gameObject.SetActive(value: false);
	}

	public void AddOnClickAction(OnClickNameplate action)
	{
		onClickNameplate = (OnClickNameplate)Delegate.Combine(onClickNameplate, action);
	}

	public void AddOnRightClickAction(OnRightClickNameplate action)
	{
		onRightClickNameplate = (OnRightClickNameplate)Delegate.Combine(onRightClickNameplate, action);
	}

	public void RemoveOnClickAction(OnClickNameplate action)
	{
		onClickNameplate = (OnClickNameplate)Delegate.Remove(onClickNameplate, action);
	}

	public void RemoveOnRightClickAction(OnRightClickNameplate action)
	{
		onRightClickNameplate = (OnRightClickNameplate)Delegate.Remove(onRightClickNameplate, action);
	}

	public void ClearAllOnClickActions()
	{
		onClickNameplate = null;
		onRightClickNameplate = null;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (button.gameObject.activeSelf)
		{
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				onClickNameplate?.Invoke(obj);
				Messenger.Broadcast(UISignals.NAMEPLATE_CLICKED, mainLbl.text);
			}
			else if (eventData.button == PointerEventData.InputButton.Right)
			{
				onRightClickNameplate?.Invoke(obj);
			}
		}
	}

	public void SetAsToggle()
	{
		button.gameObject.SetActive(value: false);
		toggle.gameObject.SetActive(value: true);
		toggle.isOn = false;
	}

	public void AddOnToggleAction(OnToggleNameplate action)
	{
		onToggleNameplate = (OnToggleNameplate)Delegate.Combine(onToggleNameplate, action);
	}

	public void RemoveOnToggleAction(OnToggleNameplate action)
	{
		onToggleNameplate = (OnToggleNameplate)Delegate.Remove(onToggleNameplate, action);
	}

	public void ClearAllOnToggleActions()
	{
		onToggleNameplate = null;
	}

	public void OnToggle(bool isOn)
	{
		onToggleNameplate?.Invoke(obj, isOn);
	}

	public void SetToggleState(bool isOn)
	{
		toggle.isOn = isOn;
	}

	public void SetToggleGroup(ToggleGroup group)
	{
		ToggleGroup toggleGroup = toggle.group;
		toggle.group = group;
		if (toggleGroup != null)
		{
			toggleGroup.UnregisterToggle(toggle);
		}
		if (group != null)
		{
			group.RegisterToggle(toggle);
			if (!group.allowSwitchOff && !group.AnyTogglesOn())
			{
				toggle.isOn = true;
			}
		}
	}

	public virtual void SetInteractableState(bool state)
	{
		button.interactable = state;
		toggle.interactable = state;
		coverGO.SetActive(!state);
	}

	public void SetAsDisplayOnly()
	{
		button.gameObject.SetActive(value: false);
		toggle.gameObject.SetActive(value: false);
	}

	public void SetSupportingLabelState(bool state)
	{
		supportingLbl.gameObject.SetActive(state);
	}
}

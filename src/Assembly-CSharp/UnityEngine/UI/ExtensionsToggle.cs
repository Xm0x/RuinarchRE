using System;
using System.Runtime.CompilerServices;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace UnityEngine.UI;

[AddComponentMenu("UI/Extensions/Extensions Toggle", 31)]
[RequireComponent(typeof(RectTransform))]
public class ExtensionsToggle : Selectable, IPointerClickHandler, IEventSystemHandler, ISubmitHandler, ICanvasElement
{
	public enum ToggleTransition
	{
		None,
		Fade
	}

	[Serializable]
	public class ToggleEvent : UnityEvent<bool>
	{
	}

	[Serializable]
	public class ToggleEventObject : UnityEvent<ExtensionsToggle>
	{
	}

	public string UniqueID;

	public ToggleTransition toggleTransition = ToggleTransition.Fade;

	public Graphic graphic;

	[SerializeField]
	private ExtensionsToggleGroup m_Group;

	[Tooltip("Use this event if you only need the bool state of the toggle that was changed")]
	public ToggleEvent onValueChanged = new ToggleEvent();

	[Tooltip("Use this event if you need access to the toggle that was changed")]
	public ToggleEventObject onToggleChanged = new ToggleEventObject();

	[FormerlySerializedAs("m_IsActive")]
	[Tooltip("Is the toggle currently on or off?")]
	[SerializeField]
	private bool m_IsOn;

	public ExtensionsToggleGroup Group
	{
		get
		{
			return m_Group;
		}
		set
		{
			m_Group = value;
			SetToggleGroup(m_Group, setMemberValue: true);
			PlayEffect(instant: true);
		}
	}

	public bool IsOn
	{
		get
		{
			return m_IsOn;
		}
		set
		{
			Set(value);
		}
	}

	protected ExtensionsToggle()
	{
	}

	public virtual void Rebuild(CanvasUpdate executing)
	{
	}

	public virtual void LayoutComplete()
	{
	}

	public virtual void GraphicUpdateComplete()
	{
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		SetToggleGroup(m_Group, setMemberValue: false);
		PlayEffect(instant: true);
	}

	protected override void OnDisable()
	{
		SetToggleGroup(null, setMemberValue: false);
		base.OnDisable();
	}

	protected override void OnDidApplyAnimationProperties()
	{
		if (graphic != null)
		{
			bool flag = !Mathf.Approximately(graphic.canvasRenderer.GetColor().a, 0f);
			if (m_IsOn != flag)
			{
				m_IsOn = flag;
				Set(!flag);
			}
		}
		base.OnDidApplyAnimationProperties();
	}

	private void SetToggleGroup(ExtensionsToggleGroup newGroup, bool setMemberValue)
	{
		ExtensionsToggleGroup extensionsToggleGroup = m_Group;
		if (m_Group != null)
		{
			m_Group.UnregisterToggle(this);
		}
		if (setMemberValue)
		{
			m_Group = newGroup;
		}
		if (m_Group != null && IsActive())
		{
			m_Group.RegisterToggle(this);
		}
		if (newGroup != null && newGroup != extensionsToggleGroup && IsOn && IsActive())
		{
			m_Group.NotifyToggleOn(this);
		}
	}

	private void Set(bool value)
	{
		Set(value, sendCallback: true);
	}

	private void Set(bool value, bool sendCallback)
	{
		if (m_IsOn != value)
		{
			m_IsOn = value;
			if (m_Group != null && IsActive() && (m_IsOn || (!m_Group.AnyTogglesOn() && !m_Group.AllowSwitchOff)))
			{
				m_IsOn = true;
				m_Group.NotifyToggleOn(this);
			}
			PlayEffect(toggleTransition == ToggleTransition.None);
			if (sendCallback)
			{
				onValueChanged.Invoke(m_IsOn);
				onToggleChanged.Invoke(this);
			}
		}
	}

	private void PlayEffect(bool instant)
	{
		if (!(graphic == null))
		{
			graphic.CrossFadeAlpha(m_IsOn ? 1f : 0f, instant ? 0f : 0.1f, ignoreTimeScale: true);
		}
	}

	protected override void Start()
	{
		PlayEffect(instant: true);
	}

	private void InternalToggle()
	{
		if (IsActive() && IsInteractable())
		{
			IsOn = !IsOn;
		}
	}

	public virtual void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			InternalToggle();
		}
	}

	public virtual void OnSubmit(BaseEventData eventData)
	{
		InternalToggle();
	}

	Transform ICanvasElement.transform => base.transform;
}

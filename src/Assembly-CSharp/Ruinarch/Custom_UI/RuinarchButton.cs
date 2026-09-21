using System;
using Coffee.UIExtensions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Ruinarch.Custom_UI;

public class RuinarchButton : Button
{
	public bool playClickAudio = true;

	private UIShiny shineEffect;

	private Action _onHoverOverAction;

	private Action _onHoverOutAction;

	private Action _onRightClickAction;

	private TextMeshProUGUI _lblBtnName;

	private bool _isShineEffectPlaying;

	private bool _isUnavailable;

	private RuinarchText _text;

	public RuinarchText text
	{
		get
		{
			if (_text == null)
			{
				_text = GetComponentInChildren<RuinarchText>();
			}
			return _text;
		}
	}

	public bool isShineEffectPlaying => _isShineEffectPlaying;

	protected override void Awake()
	{
		base.Awake();
		if (!Application.isPlaying)
		{
			return;
		}
		shineEffect = GetComponent<UIShiny>();
		if (shineEffect == null)
		{
			shineEffect = base.targetGraphic.gameObject.GetComponent<UIShiny>();
		}
		if (shineEffect != null)
		{
			if (base.targetGraphic.gameObject.GetComponent<PlayShineEffectOnAwake>() == null)
			{
				_isShineEffectPlaying = false;
				shineEffect.Stop();
			}
			else
			{
				_isShineEffectPlaying = true;
				shineEffect.Play();
			}
		}
		_lblBtnName = GetComponentInChildren<TextMeshProUGUI>();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (Application.isPlaying)
		{
			Messenger.AddListener<string>(UISignals.SHOW_SELECTABLE_GLOW, OnReceiveShowGlowSignal);
			Messenger.AddListener<string>(UISignals.HIDE_SELECTABLE_GLOW, OnReceiveHideGlowSignal);
			Messenger.Broadcast(UISignals.BUTTON_SHOWN, this);
			if (InputManager.Instance != null && InputManager.Instance.ShouldBeHighlighted(this))
			{
				StartGlow();
			}
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (Application.isPlaying)
		{
			Messenger.RemoveListener<string>(UISignals.SHOW_SELECTABLE_GLOW, OnReceiveShowGlowSignal);
			Messenger.RemoveListener<string>(UISignals.HIDE_SELECTABLE_GLOW, OnReceiveHideGlowSignal);
			HideGlow();
		}
	}

	public void MakeAvailable()
	{
		_isUnavailable = false;
	}

	public void MakeUnavailable()
	{
		_isUnavailable = true;
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		if (_isUnavailable)
		{
			AudioManager.Instance.OnErrorSoundPlay();
		}
		else if (IsInteractable())
		{
			Messenger.Broadcast(UISignals.BUTTON_CLICKED, this);
			base.OnPointerClick(eventData);
			if (eventData.button == PointerEventData.InputButton.Right)
			{
				_onRightClickAction?.Invoke();
			}
			if (IsActive() && IsInteractable())
			{
				_onHoverOverAction?.Invoke();
			}
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		if (IsActive() && IsInteractable())
		{
			_onHoverOverAction?.Invoke();
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		if (IsActive() && IsInteractable())
		{
			_onHoverOutAction?.Invoke();
		}
	}

	public void AddHoverOverAction(Action p_hoverOverAction)
	{
		_onHoverOverAction = (Action)Delegate.Combine(_onHoverOverAction, p_hoverOverAction);
	}

	public void AddHoverOutAction(Action p_hoverOutAction)
	{
		_onHoverOutAction = (Action)Delegate.Combine(_onHoverOutAction, p_hoverOutAction);
	}

	public void RemoveHoverOverAction(Action p_hoverOverAction)
	{
		_onHoverOverAction = (Action)Delegate.Remove(_onHoverOverAction, p_hoverOverAction);
	}

	public void RemoveHoverOutAction(Action p_hoverOutAction)
	{
		_onHoverOutAction = (Action)Delegate.Remove(_onHoverOutAction, p_hoverOutAction);
	}

	public void AddRightClickAction(Action p_action)
	{
		_onRightClickAction = (Action)Delegate.Combine(_onRightClickAction, p_action);
	}

	public void RemoveRightClickAction(Action p_action)
	{
		_onRightClickAction = (Action)Delegate.Remove(_onRightClickAction, p_action);
	}

	public void StartGlow()
	{
		if (shineEffect != null && !_isShineEffectPlaying)
		{
			_isShineEffectPlaying = true;
			shineEffect.Play();
		}
	}

	private void HideGlow()
	{
		if (shineEffect != null)
		{
			_isShineEffectPlaying = false;
			shineEffect.Stop();
		}
	}

	private void OnReceiveShowGlowSignal(string buttonName)
	{
		if (base.name == buttonName)
		{
			StartGlow();
		}
	}

	private void OnReceiveHideGlowSignal(string buttonName)
	{
		if (base.name == buttonName)
		{
			HideGlow();
		}
	}

	public void ForceUpdateGlow()
	{
		if (InputManager.Instance != null && InputManager.Instance.ShouldBeHighlighted(this))
		{
			StartGlow();
		}
	}

	public void SetButtonLabelName(string p_name)
	{
		if (_lblBtnName != null)
		{
			_lblBtnName.text = p_name;
		}
	}

	public void OnReceiveHotKeyClick()
	{
		if (IsInteractable())
		{
			base.onClick?.Invoke();
			Messenger.Broadcast(UISignals.BUTTON_CLICKED, this);
		}
	}
}

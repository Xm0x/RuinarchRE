using Coffee.UIExtensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Ruinarch.Custom_UI;

public class RuinarchToggle : Toggle
{
	private UIShiny _shineEffect;

	private bool _isShineEffectPlaying;

	public bool isShineEffectPlaying => _isShineEffectPlaying;

	protected override void Awake()
	{
		base.Awake();
		if (Application.isPlaying)
		{
			_shineEffect = GetComponent<UIShiny>();
			if (_shineEffect == null)
			{
				_shineEffect = base.targetGraphic.gameObject.GetComponent<UIShiny>();
			}
			if (_shineEffect != null)
			{
				_isShineEffectPlaying = false;
				_shineEffect.Stop();
			}
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (Application.isPlaying)
		{
			if (_shineEffect != null)
			{
				Messenger.AddListener<string>(UISignals.SHOW_SELECTABLE_GLOW, OnReceiveShowGlowSignal);
				Messenger.AddListener<string>(UISignals.HIDE_SELECTABLE_GLOW, OnReceiveHideGlowSignal);
			}
			FireToggleShownSignal();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (Application.isPlaying && _shineEffect != null)
		{
			Messenger.RemoveListener<string>(UISignals.SHOW_SELECTABLE_GLOW, OnReceiveShowGlowSignal);
			Messenger.RemoveListener<string>(UISignals.HIDE_SELECTABLE_GLOW, OnReceiveHideGlowSignal);
			HideGlow();
		}
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		base.OnPointerClick(eventData);
		Messenger.Broadcast(UISignals.TOGGLE_CLICKED, this);
	}

	public void StartGlow()
	{
		if (_shineEffect != null && !_isShineEffectPlaying)
		{
			_isShineEffectPlaying = true;
			_shineEffect.Play();
		}
	}

	private void HideGlow()
	{
		if (_shineEffect != null)
		{
			_isShineEffectPlaying = false;
			_shineEffect.Stop();
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

	public void FireToggleShownSignal()
	{
		Messenger.Broadcast(UISignals.TOGGLE_SHOWN, this);
	}

	public void OnReceiveHotKeyClick()
	{
		if (base.interactable)
		{
			base.isOn = !base.isOn;
			Messenger.Broadcast(UISignals.TOGGLE_CLICKED, this);
		}
	}
}

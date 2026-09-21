using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class TimerItemUI : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI lblTimerName;

	[SerializeField]
	private Slider barProgress;

	[SerializeField]
	private HoverHandler hoverHandler;

	private RuinarchTimer _timer;

	private Action _onHoverOverAction;

	private Action _onHoverOutAction;

	private void OnEnable()
	{
		Messenger.AddListener(Signals.TICK_ENDED, OnTickEnded);
		ForceUpdateOnEnable();
	}

	private void OnDisable()
	{
		Messenger.RemoveListener(Signals.TICK_ENDED, OnTickEnded);
	}

	private void Awake()
	{
		hoverHandler.AddOnHoverOverAction(OnHoverOver);
		hoverHandler.AddOnHoverOutAction(OnHoverOut);
	}

	private void OnTickEnded()
	{
		if (_timer != null)
		{
			SetCurrentProgress(_timer.GetCurrentTimerProgressPercent());
		}
	}

	public void SetTimer(RuinarchTimer p_timer)
	{
		_timer = p_timer;
	}

	public void SetName(string p_name)
	{
		lblTimerName.text = p_name;
	}

	private void SetCurrentProgress(float p_progress)
	{
		barProgress.DOValue(p_progress, 0.1f);
	}

	private void ForceUpdateOnEnable()
	{
		barProgress.value = _timer?.GetCurrentTimerProgressPercent() ?? 0f;
	}

	public void RefreshName()
	{
		if (_timer != null)
		{
			SetName(_timer.timerName);
		}
	}

	private void OnDestroy()
	{
		_timer = null;
	}

	public void SetHoverOverAction(Action p_action)
	{
		_onHoverOverAction = (Action)Delegate.Combine(_onHoverOverAction, p_action);
	}

	public void SetHoverOutAction(Action p_action)
	{
		_onHoverOutAction = (Action)Delegate.Combine(_onHoverOutAction, p_action);
	}

	private void OnHoverOver()
	{
		_onHoverOverAction?.Invoke();
	}

	private void OnHoverOut()
	{
		_onHoverOutAction?.Invoke();
	}
}

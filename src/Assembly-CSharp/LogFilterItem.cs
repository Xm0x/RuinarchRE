using System;
using UnityEngine;
using UnityEngine.UI;

public class LogFilterItem : MonoBehaviour
{
	[SerializeField]
	private LOG_TAG logTag;

	[SerializeField]
	private Toggle toggle;

	private Action<bool, LOG_TAG> onToggleAction;

	public bool isOn => toggle.isOn;

	public LOG_TAG filterType => logTag;

	public void SetOnToggleAction(Action<bool, LOG_TAG> onToggleAction)
	{
		this.onToggleAction = onToggleAction;
	}

	public void AddOnToggleAction(Action<bool, LOG_TAG> onToggleAction)
	{
		this.onToggleAction = (Action<bool, LOG_TAG>)Delegate.Combine(this.onToggleAction, onToggleAction);
	}

	public void OnToggleFilter(bool state)
	{
		onToggleAction(state, logTag);
	}

	public void SetIsOnWithoutNotify(bool state)
	{
		toggle.SetIsOnWithoutNotify(state);
	}

	public void SetIsOn(bool state)
	{
		toggle.isOn = state;
	}
}

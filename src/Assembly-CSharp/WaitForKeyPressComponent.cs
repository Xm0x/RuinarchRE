using System;
using UnityEngine;

public class WaitForKeyPressComponent : MonoBehaviour
{
	private static readonly Array keyCodes = Enum.GetValues(typeof(KeyCode));

	private Action<KeyCode> _keypressCallback;

	public void StartWaitingForKeyPress(Action<KeyCode> p_keypressCallback)
	{
		_keypressCallback = p_keypressCallback;
		base.enabled = true;
	}

	private void OnDisable()
	{
		base.enabled = false;
	}

	private void Update()
	{
		if (!Input.anyKeyDown)
		{
			return;
		}
		foreach (KeyCode keyCode in keyCodes)
		{
			if (Input.GetKey(keyCode))
			{
				_keypressCallback?.Invoke(keyCode);
				base.enabled = false;
				break;
			}
		}
	}
}

using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UtilityScripts;

namespace Ruinarch.Custom_UI;

[RequireComponent(typeof(Selectable))]
public class SelectableHotkeyReceiver : MonoBehaviour
{
	[SerializeField]
	private RuinarchButton _btn;

	[SerializeField]
	private RuinarchToggle _toggle;

	[SerializeField]
	private SHORTCUT_ACTION _action;

	[SerializeField]
	private InputActionReference _inputAction;

	[Header("UI")]
	[SerializeField]
	private bool appendShortcutKey;

	[SerializeField]
	private TextMeshProUGUI buttonLbl;

	[SerializeField]
	private CustomLocalizeStringEvent localizeStringEvent;

	private string _mainStr;

	private void OnEnable()
	{
		if (buttonLbl != null)
		{
			_mainStr = buttonLbl.text;
		}
		Messenger.AddListener<SHORTCUT_ACTION>(ControlsSignals.PLAYER_INPUT_ACTION, OnReceivePlayerInputAction);
		Messenger.AddListener<InputAction>(ControlsSignals.UPDATED_KEYBIND, OnKeyBindUpdated);
		Messenger.AddListener(Signals.GAME_LOADED, OnGameLoaded);
		Messenger.AddListener<string>(ControlsSignals.CONTROL_DEVICE_CHANGED, OnControlDeviceChanged);
		if (_inputAction != null)
		{
			UpdateLabel(_inputAction.action);
		}
	}

	private void OnDisable()
	{
		_mainStr = string.Empty;
		Messenger.RemoveListener<SHORTCUT_ACTION>(ControlsSignals.PLAYER_INPUT_ACTION, OnReceivePlayerInputAction);
		Messenger.RemoveListener<InputAction>(ControlsSignals.UPDATED_KEYBIND, OnKeyBindUpdated);
		Messenger.RemoveListener(Signals.GAME_LOADED, OnGameLoaded);
		Messenger.RemoveListener<string>(ControlsSignals.CONTROL_DEVICE_CHANGED, OnControlDeviceChanged);
	}

	private void OnReceivePlayerInputAction(SHORTCUT_ACTION p_action)
	{
		if (_action == p_action)
		{
			if (_btn != null)
			{
				_btn.OnReceiveHotKeyClick();
			}
			else if (_toggle != null)
			{
				_toggle.OnReceiveHotKeyClick();
			}
		}
	}

	private void OnKeyBindUpdated(InputAction p_keybinding)
	{
		if (p_keybinding == _inputAction.action)
		{
			UpdateLabel(p_keybinding);
		}
	}

	private void OnGameLoaded()
	{
		if (_inputAction != null)
		{
			UpdateLabel(_inputAction.action);
		}
	}

	private void OnControlDeviceChanged(string p_deviceName)
	{
		if (_inputAction != null)
		{
			UpdateLabel(_inputAction.action);
		}
	}

	private void UpdateLabel(InputAction p_keybinding)
	{
		if (!appendShortcutKey)
		{
			return;
		}
		if (localizeStringEvent != null)
		{
			localizeStringEvent?.RefreshString();
		}
		else
		{
			buttonLbl.text = _mainStr;
		}
		string bindingDisplayString = p_keybinding.GetBindingDisplayString();
		string shortcutIcon = bindingDisplayString.GetShortcutIcon();
		if (!string.IsNullOrEmpty(bindingDisplayString))
		{
			if (!string.IsNullOrEmpty(shortcutIcon))
			{
				buttonLbl.text = buttonLbl.text + " " + shortcutIcon;
			}
			else
			{
				buttonLbl.text = buttonLbl.text + " <size=60%>(" + bindingDisplayString + ")";
			}
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(buttonLbl.rectTransform);
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform.parent as RectTransform);
	}
}

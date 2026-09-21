using System;
using UnityEngine;

namespace Settings;

[Serializable]
public class KeyBinding
{
	public static KeyCode[] reservedKeyCodes = new KeyCode[5]
	{
		KeyCode.Escape,
		KeyCode.BackQuote,
		KeyCode.Mouse0,
		KeyCode.Mouse1,
		KeyCode.Mouse2
	};

	public SHORTCUT_ACTION shortcutAction;

	public string main;

	public string alternative;

	public bool hasValue;

	public KeyBinding(string p_main, string p_alternative)
	{
		main = p_main;
		alternative = p_alternative;
		hasValue = true;
	}

	public KeyBinding(KeyBinding p_copy)
	{
		shortcutAction = p_copy.shortcutAction;
		main = p_copy.main;
		alternative = p_copy.alternative;
		hasValue = true;
	}

	public void Copy(KeyBinding p_copy)
	{
		main = p_copy.main;
		alternative = p_copy.alternative;
	}
}

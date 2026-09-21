using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PsychopathPickerItem : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI mainLbl;

	[SerializeField]
	private Toggle toggle;

	private Action<string, string, bool> onToggleNameplate;

	private Action<string> onHoverEnter;

	private Action<string> onHoverExit;

	private string str;

	private string localizedStr;

	public void SetObject(string o)
	{
		str = o;
		base.name = o;
	}

	public void SetLocalizedName(string p_name)
	{
		mainLbl.text = p_name;
		localizedStr = p_name;
	}

	public void SetToggleAction(Action<string, string, bool> onToggleNameplate)
	{
		this.onToggleNameplate = onToggleNameplate;
	}

	public void SetOnHoverEnterAction(Action<string> onHoverEnter)
	{
		this.onHoverEnter = onHoverEnter;
	}

	public void SetOnHoverExitAction(Action<string> onHoverExit)
	{
		this.onHoverExit = onHoverExit;
	}

	public void SetToggleGroup(ToggleGroup group)
	{
		toggle.group = group;
	}

	public void OnToggle(bool isOn)
	{
		onToggleNameplate(str, localizedStr, isOn);
	}

	public void OnHoverEnter()
	{
		if (onHoverEnter != null)
		{
			onHoverEnter(str);
		}
	}

	public void OnHoverExit()
	{
		if (onHoverExit != null)
		{
			onHoverExit(str);
		}
	}
}

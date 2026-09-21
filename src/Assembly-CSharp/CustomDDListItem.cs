using System;
using EZObjectPools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomDDListItem : PooledObject
{
	public TextMeshProUGUI itemText;

	[SerializeField]
	private GameObject _cover;

	[SerializeField]
	private Button _button;

	private Action<CustomDDListItem> onClick;

	public void SetText(string text)
	{
		itemText.text = text;
	}

	public void SetClickAction(Action<CustomDDListItem> action)
	{
		onClick = action;
	}

	public void SetCoverState(bool state)
	{
		_cover.SetActive(state);
		_button.interactable = !state;
	}

	public void OnClickThis()
	{
		if (onClick != null)
		{
			onClick(this);
		}
	}

	public override void Reset()
	{
		base.Reset();
		onClick = null;
		itemText.text = string.Empty;
		SetCoverState(state: false);
	}
}

using System;
using EZObjectPools;
using Ruinarch.Custom_UI;
using UnityEngine;
using UnityEngine.UI;

public class POITestingItemUI : PooledObject
{
	[SerializeField]
	private RuinarchButton btn;

	[SerializeField]
	private Text btnTitle;

	private Action _onClickAction;

	private void Awake()
	{
		btn.onClick.AddListener(OnClick);
	}

	public void Initialize(string p_title, Action p_onClick)
	{
		_onClickAction = p_onClick;
		btnTitle.text = p_title;
	}

	private void OnClick()
	{
		_onClickAction?.Invoke();
	}

	public override void Reset()
	{
		base.Reset();
		_onClickAction = null;
	}
}

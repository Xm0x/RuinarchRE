using System;
using Ruinarch.MVCFramework;
using UnityEngine;
using UnityEngine.UI;

public class TipsUIModel : MVCUIModel
{
	public Action onCloseClicked;

	public Button btnClose;

	public Transform scrollViewContent;

	public RectTransform window;

	private void OnEnable()
	{
		btnClose.onClick.AddListener(OnClickClose);
	}

	private void OnDisable()
	{
		btnClose.onClick.RemoveListener(OnClickClose);
	}

	private void OnClickClose()
	{
		onCloseClicked?.Invoke();
	}
}

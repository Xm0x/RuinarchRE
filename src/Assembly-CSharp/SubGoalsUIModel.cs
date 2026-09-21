using System;
using Ruinarch.MVCFramework;
using UnityEngine;
using UnityEngine.UI;

public class SubGoalsUIModel : MVCUIModel
{
	public Button btnClose;

	public ScrollRect scrollRectSubGoals;

	public GameObject prefabSubGoalItem;

	public Action onClickClose;

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
		onClickClose?.Invoke();
	}
}

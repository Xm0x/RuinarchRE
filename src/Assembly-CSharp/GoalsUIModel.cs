using System;
using Ruinarch.MVCFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoalsUIModel : MVCUIModel
{
	public ScrollRect scrollRectGoals;

	public Button btnClose;

	public GameObject prefabGoalItem;

	public TextMeshProUGUI lblInstructions;

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

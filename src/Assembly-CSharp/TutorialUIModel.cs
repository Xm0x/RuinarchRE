using System;
using Ruinarch.Custom_UI;
using Ruinarch.MVCFramework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class TutorialUIModel : MVCUIModel
{
	public ScrollRect scrollRectTutorialItems;

	public RuinarchButton btnClose;

	public ToggleGroup toggleGroupTutorialItems;

	public HorizontalScrollSnap tutorialPagesScrollSnap;

	public RectTransform tutorialPagesScrollSnapContent;

	public RectTransform tutorialPaginationParent;

	public GameObject goTutorialPages;

	public Button btnPreviousPage;

	public Button btnNextPage;

	public Action onClickClose;

	public Action onClickPreviousPage;

	public Action onClickNextPage;

	private void OnEnable()
	{
		btnClose.onClick.AddListener(OnClickClose);
		btnPreviousPage.onClick.AddListener(OnClickPreviousPage);
		btnNextPage.onClick.AddListener(OnClickNextPage);
	}

	private void OnDisable()
	{
		btnClose.onClick.RemoveListener(OnClickClose);
		btnPreviousPage.onClick.RemoveListener(OnClickPreviousPage);
		btnNextPage.onClick.RemoveListener(OnClickNextPage);
	}

	private void OnClickClose()
	{
		onClickClose?.Invoke();
	}

	private void OnClickPreviousPage()
	{
		onClickPreviousPage?.Invoke();
	}

	private void OnClickNextPage()
	{
		onClickNextPage?.Invoke();
	}
}

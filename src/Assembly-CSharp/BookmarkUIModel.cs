using System;
using Ruinarch.Custom_UI;
using Ruinarch.MVCFramework;
using UnityEngine;
using UnityEngine.UI;

public class BookmarkUIModel : MVCUIModel
{
	public RuinarchButton btnHide;

	public RuinarchButton btnShow;

	public ScrollRect scrollRectBookmarks;

	public RectTransform rtWindow;

	public Vector2 posHidden;

	public Vector2 posShowing;

	public Action onClickHide;

	public Action onClickShow;

	public GameObject goBookmarkCategoryPrefab;

	public UIHoverPosition tooltipHoverPosition;

	private void OnEnable()
	{
		btnHide.onClick.AddListener(ClickHide);
		btnShow.onClick.AddListener(ClickShow);
	}

	private void OnDisable()
	{
		btnHide.onClick.RemoveListener(ClickHide);
		btnShow.onClick.RemoveListener(ClickShow);
	}

	private void ClickHide()
	{
		onClickHide?.Invoke();
	}

	private void ClickShow()
	{
		onClickShow?.Invoke();
	}
}

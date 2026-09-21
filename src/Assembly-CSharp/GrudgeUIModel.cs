using System;
using System.Collections.Generic;
using Ruinarch.Custom_UI;
using Ruinarch.MVCFramework;
using UnityEngine;

public class GrudgeUIModel : MVCUIModel
{
	public Action onCloseClicked;

	public RuinarchButton btnClose;

	public Transform containerParent;

	[Header("Cover")]
	public CanvasGroup canvasGroupCover;

	[Header("Window")]
	public CanvasGroup canvasGroupMainWindow;

	public RectTransform rectTransformMainWindow;

	[Header("Frame")]
	public CanvasGroup canvasGroupFrameGlow;

	public CanvasGroup canvasGroupFrame;

	public RectTransform rectTransformFrame;

	[Header("Items")]
	public List<GrudgeItemUI> items = new List<GrudgeItemUI>();

	public Vector2 defaultFrameSize { get; private set; }

	private void Awake()
	{
		defaultFrameSize = rectTransformFrame.sizeDelta;
	}

	private void OnEnable()
	{
		btnClose.onClick.AddListener(ClickClose);
	}

	private void OnDisable()
	{
		btnClose.onClick.RemoveListener(ClickClose);
	}

	private void ClickClose()
	{
		onCloseClicked?.Invoke();
	}
}

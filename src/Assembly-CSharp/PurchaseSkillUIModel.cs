using System;
using System.Collections.Generic;
using Ruinarch.Custom_UI;
using Ruinarch.MVCFramework;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseSkillUIModel : MVCUIModel
{
	public Action onCloseClicked;

	public Action onRerollClicked;

	public Action onHoverOverReroll;

	public Action onHoverOutReroll;

	public Action onClickCancelReleaseAbility;

	public RuinarchButton btnClose;

	public RuinarchButton btnReroll;

	public HoverHandler hoverHandlerReroll;

	public RuinarchText txtMessageDisplay;

	public Transform skillsParent;

	public Image imgCooldown;

	public GameObject goCover;

	public RuinarchText lblChaoticEnergy;

	[Header("Timer")]
	public GameObject goReleaseAbilityTimer;

	public TimerItemUI timerReleaseAbility;

	public RuinarchButton btnCancelReleaseAbility;

	public HoverHandler hoverHandlerBtnCancelReleaseAbility;

	public Action onHoverOverCancelReleaseAbility;

	public Action onHoverOutCancelReleaseAbility;

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
	public List<PurchaseSkillItemUI> skillItems = new List<PurchaseSkillItemUI>();

	public Vector2 defaultFrameSize { get; private set; }

	private void Awake()
	{
		defaultFrameSize = rectTransformFrame.sizeDelta;
	}

	private void OnEnable()
	{
		btnClose.onClick.AddListener(ClickClose);
		btnReroll.onClick.AddListener(ClickReroll);
		hoverHandlerReroll.AddOnHoverOverAction(OnHoverOverReroll);
		hoverHandlerReroll.AddOnHoverOutAction(OnHoverOutReroll);
		btnCancelReleaseAbility.onClick.AddListener(OnClickCancelReleaseAbility);
		hoverHandlerBtnCancelReleaseAbility.AddOnHoverOverAction(OnHoverOverCancelReleaseAbility);
		hoverHandlerBtnCancelReleaseAbility.AddOnHoverOutAction(OnHoverOutCancelReleaseAbility);
	}

	private void OnDisable()
	{
		btnClose.onClick.RemoveListener(ClickClose);
		btnReroll.onClick.RemoveListener(ClickReroll);
		hoverHandlerReroll.RemoveOnHoverOverAction(OnHoverOverReroll);
		hoverHandlerReroll.RemoveOnHoverOutAction(OnHoverOutReroll);
		btnCancelReleaseAbility.onClick.RemoveListener(OnClickCancelReleaseAbility);
		hoverHandlerBtnCancelReleaseAbility.RemoveOnHoverOverAction(OnHoverOverCancelReleaseAbility);
		hoverHandlerBtnCancelReleaseAbility.RemoveOnHoverOutAction(OnHoverOutCancelReleaseAbility);
	}

	private void ClickClose()
	{
		onCloseClicked?.Invoke();
	}

	private void ClickReroll()
	{
		onRerollClicked?.Invoke();
	}

	private void OnHoverOverReroll()
	{
		onHoverOverReroll?.Invoke();
	}

	private void OnHoverOutReroll()
	{
		onHoverOutReroll?.Invoke();
	}

	private void OnClickCancelReleaseAbility()
	{
		onClickCancelReleaseAbility?.Invoke();
	}

	private void OnHoverOverCancelReleaseAbility()
	{
		onHoverOverCancelReleaseAbility?.Invoke();
	}

	private void OnHoverOutCancelReleaseAbility()
	{
		onHoverOutCancelReleaseAbility?.Invoke();
	}
}

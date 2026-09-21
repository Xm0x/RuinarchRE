using System;
using Ruinarch.Custom_UI;
using Ruinarch.MVCFramework;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class UpgradePortalUIModel : MVCUIModel
{
	public TextMeshProUGUI lblTitle;

	public RuinarchButton btnClose;

	public CanvasGroup canvasGroupCover;

	public RectTransform rectFrame;

	public CanvasGroup canvasGroupFrameGlow;

	public CanvasGroup canvasGroupFrame;

	public CanvasGroup canvasGroupSkillTree;

	[FormerlySerializedAs("lblChaoticEnergy")]
	public RuinarchText lblSpiritEnergy;

	[Header("Upgrade Tiers")]
	public UpgradePortalTierItem[] tierItems;

	[Header("Choose skills")]
	public GameObject goChooseSkill;

	public TextMeshProUGUI lblChooseSkill;

	public CanvasGroup canvasGroupChooseSkill;

	public ChooseSkillItemUI[] chooseSkillItems;

	public RuinarchButton chooseSkillBtnClose;

	[Header("Upgrade Timer")]
	public GameObject goUpgradeTimer;

	public TimerItemUI timerUpgradePortal;

	public RuinarchButton btnCancelUpgrade;

	public HoverHandler hoverHandlerBtnCancelUpgradePortal;

	public Action onHoverOverCancelUpgradePortal;

	public Action onHoverOutCancelUpgradePortal;

	public Action onClickClose;

	public Action onClickCancelUpgrade;

	public Action onClickCloseChooseSkill;

	public Vector2 defaultFrameSize { get; private set; }

	private void Awake()
	{
		defaultFrameSize = rectFrame.sizeDelta;
	}

	private void OnEnable()
	{
		btnClose.onClick.AddListener(OnClickClose);
		btnCancelUpgrade.onClick.AddListener(OnClickCancelUpgrade);
		hoverHandlerBtnCancelUpgradePortal.AddOnHoverOverAction(OnHoverOverCancelUpgradePortal);
		hoverHandlerBtnCancelUpgradePortal.AddOnHoverOutAction(OnHoverOutCancelUpgradePortal);
		chooseSkillBtnClose.onClick.AddListener(OnClickCloseChooseSkill);
	}

	private void OnDisable()
	{
		btnClose.onClick.RemoveListener(OnClickClose);
		btnCancelUpgrade.onClick.RemoveListener(OnClickCancelUpgrade);
		hoverHandlerBtnCancelUpgradePortal.RemoveOnHoverOverAction(OnHoverOverCancelUpgradePortal);
		hoverHandlerBtnCancelUpgradePortal.RemoveOnHoverOutAction(OnHoverOutCancelUpgradePortal);
		chooseSkillBtnClose.onClick.RemoveListener(OnClickCloseChooseSkill);
	}

	private void OnClickClose()
	{
		onClickClose?.Invoke();
	}

	private void OnClickCancelUpgrade()
	{
		onClickCancelUpgrade?.Invoke();
	}

	private void OnHoverOverCancelUpgradePortal()
	{
		onHoverOverCancelUpgradePortal?.Invoke();
	}

	private void OnHoverOutCancelUpgradePortal()
	{
		onHoverOutCancelUpgradePortal?.Invoke();
	}

	private void OnClickCloseChooseSkill()
	{
		onClickCloseChooseSkill?.Invoke();
	}
}

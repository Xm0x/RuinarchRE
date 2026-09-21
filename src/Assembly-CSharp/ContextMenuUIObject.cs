using System;
using DG.Tweening;
using EZObjectPools;
using Ruinarch.Custom_UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class ContextMenuUIObject : PooledObject
{
	public static Action<IContextMenuItem, bool, int> onMenuPress;

	public static Action<IContextMenuItem, bool, int> onHoverOverItem;

	public static Action<IContextMenuItem, bool, int> onHoverOutItem;

	public Image ImgIcon;

	public TextMeshProUGUI txtMenuName;

	public TextMeshProUGUI txtMenuFullName;

	public RuinarchButton btnActivate;

	public GameObject goArrow;

	public Image coverImg;

	public Image imgCooldownRadial;

	public Image imgCooldown;

	public TextMeshProUGUI txtCooldownTimer;

	public TextMeshProUGUI txtCharges;

	public HoverHandler hoverHandler;

	private IContextMenuItem m_parentUIMenu;

	private bool m_isAction;

	private int m_menuColumn;

	private void OnEnable()
	{
		btnActivate.onClick.AddListener(ButtonClicked);
		hoverHandler.AddOnHoverOverAction(HoverOver);
		hoverHandler.AddOnHoverOutAction(HoverOut);
	}

	private void OnDisable()
	{
		btnActivate.onClick.RemoveListener(ButtonClicked);
		hoverHandler.RemoveOnHoverOverAction(HoverOver);
		hoverHandler.RemoveOnHoverOverAction(HoverOut);
	}

	private void Update()
	{
		if (m_parentUIMenu == null)
		{
			return;
		}
		bool flag = m_parentUIMenu.CanBePickedRegardlessOfCooldown();
		bool active = m_parentUIMenu.IsInCooldown();
		coverImg.gameObject.SetActive(!flag);
		imgCooldown.gameObject.SetActive(active);
		txtCooldownTimer.gameObject.SetActive(active);
		btnActivate.interactable = flag;
		if (imgCooldown.gameObject.activeSelf)
		{
			imgCooldownRadial.DOFillAmount(m_parentUIMenu.GetCoverFillAmount(), 0.4f);
		}
		if (txtCooldownTimer.gameObject.activeSelf)
		{
			txtCooldownTimer.text = m_parentUIMenu.GetCurrentRemainingCooldownTicks().ToString();
		}
		if (m_parentUIMenu is PlayerAction playerAction)
		{
			string chargesCombinedUIText = playerAction.GetChargesCombinedUIText();
			if (!string.IsNullOrEmpty(chargesCombinedUIText))
			{
				txtCharges.text = chargesCombinedUIText;
				txtCharges.gameObject.SetActive(value: true);
			}
			else
			{
				txtCharges.gameObject.SetActive(value: false);
			}
		}
		else
		{
			txtCharges.gameObject.SetActive(value: false);
		}
	}

	public void SetMenuDetails(IContextMenuItem p_parentUIMenu, bool dontShowName = false)
	{
		ImgIcon.gameObject.SetActive(p_parentUIMenu.contextMenuIcon != null);
		if (ImgIcon.gameObject.activeSelf)
		{
			txtMenuName.text = p_parentUIMenu.contextMenuName;
			txtMenuFullName.gameObject.SetActive(value: false);
			txtMenuName.gameObject.SetActive(value: true);
		}
		else
		{
			string text = string.Empty;
			if (p_parentUIMenu.GetManaCost() > 0)
			{
				text = $"{p_parentUIMenu.GetManaCost()}{Utilities.ManaIcon()}  ";
			}
			text += p_parentUIMenu.contextMenuName;
			if (dontShowName)
			{
				txtMenuFullName.text = "?????";
			}
			else
			{
				txtMenuFullName.text = text;
			}
			txtMenuFullName.gameObject.SetActive(value: true);
			txtMenuName.gameObject.SetActive(value: false);
		}
		btnActivate.name = p_parentUIMenu.contextMenuName;
		ImgIcon.sprite = p_parentUIMenu.contextMenuIcon;
		m_parentUIMenu = p_parentUIMenu;
		bool flag = p_parentUIMenu.CanBePickedRegardlessOfCooldown();
		bool active = m_parentUIMenu.IsInCooldown();
		coverImg.gameObject.SetActive(!flag);
		imgCooldown.gameObject.SetActive(active);
		txtCooldownTimer.gameObject.SetActive(active);
		btnActivate.interactable = flag;
		if (imgCooldown.gameObject.activeSelf)
		{
			imgCooldownRadial.fillAmount = p_parentUIMenu.GetCoverFillAmount();
		}
		if (txtCooldownTimer.gameObject.activeSelf)
		{
			txtCooldownTimer.text = p_parentUIMenu.GetCurrentRemainingCooldownTicks().ToString();
		}
		if (m_parentUIMenu is PlayerAction { charges: >0 } playerAction)
		{
			txtCharges.text = $"{Utilities.ChargesIcon()}{playerAction.charges}";
			txtCharges.gameObject.SetActive(value: true);
		}
		else
		{
			txtCharges.gameObject.SetActive(value: false);
		}
		bool flag2 = m_parentUIMenu.subMenus != null && m_parentUIMenu.subMenus.Count > 0;
		goArrow.gameObject.SetActive(flag2);
		if (!flag2)
		{
			m_isAction = true;
		}
		m_menuColumn = p_parentUIMenu.contextMenuColumn;
	}

	private void ButtonClicked()
	{
		onMenuPress?.Invoke(m_parentUIMenu, m_isAction, m_menuColumn);
	}

	private void HoverOver()
	{
		onHoverOverItem?.Invoke(m_parentUIMenu, m_isAction, m_menuColumn);
	}

	private void HoverOut()
	{
		onHoverOutItem?.Invoke(m_parentUIMenu, m_isAction, m_menuColumn);
	}

	public override void Reset()
	{
		base.Reset();
		btnActivate.name = "ClickableButton";
		m_isAction = false;
	}
}

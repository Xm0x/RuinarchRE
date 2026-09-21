using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Clickable Data", menuName = "Scriptable Objects/UI/ClickableData")]
public class ClickableMenuData : ScriptableObject, IContextMenuItem
{
	public Sprite sprtIcon;

	public string strMenuName;

	public int column;

	public List<ClickableMenuData> subMenu;

	public Sprite contextMenuIcon => sprtIcon;

	public string contextMenuName => strMenuName;

	public int contextMenuColumn => column;

	public List<IContextMenuItem> subMenus => ((IEnumerable<ClickableMenuData>)subMenu).Select((Func<ClickableMenuData, IContextMenuItem>)((ClickableMenuData x) => x)).ToList();

	public void OnPickAction()
	{
		Debug.Log("Picked " + strMenuName);
	}

	public bool CanBePickedRegardlessOfCooldown()
	{
		return true;
	}

	public bool IsInCooldown()
	{
		return false;
	}

	public float GetCoverFillAmount()
	{
		return 1f;
	}

	public int GetCurrentRemainingCooldownTicks()
	{
		return 0;
	}

	public int GetManaCost()
	{
		return -1;
	}
}

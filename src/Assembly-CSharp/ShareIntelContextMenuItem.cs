using System.Collections.Generic;
using UnityEngine;

public class ShareIntelContextMenuItem : IContextMenuItem
{
	private List<IContextMenuItem> _subMenuCachedList;

	public Sprite contextMenuIcon => null;

	public string contextMenuName => "Share with";

	public int contextMenuColumn => 0;

	public List<IContextMenuItem> subMenus => GetSubMenus();

	public IIntel currentIntelForContextMenu { get; private set; }

	public void OnPickAction()
	{
	}

	public bool CanBePickedRegardlessOfCooldown()
	{
		for (int i = 0; i < PlayerManager.Instance.player.storedTargetsComponent.allStoredTargets.Count; i++)
		{
			if (PlayerManager.Instance.player.storedTargetsComponent.allStoredTargets[i] is Character)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsInCooldown()
	{
		return false;
	}

	public float GetCoverFillAmount()
	{
		return 0f;
	}

	public int GetCurrentRemainingCooldownTicks()
	{
		return 0;
	}

	public int GetManaCost()
	{
		return 0;
	}

	private List<IContextMenuItem> GetSubMenus()
	{
		if (_subMenuCachedList == null)
		{
			_subMenuCachedList = new List<IContextMenuItem>();
		}
		else
		{
			_subMenuCachedList.Clear();
		}
		for (int i = 0; i < PlayerManager.Instance.player.storedTargetsComponent.allStoredTargets.Count; i++)
		{
			if (PlayerManager.Instance.player.storedTargetsComponent.allStoredTargets[i] is Character { isNormalCharacter: not false } character)
			{
				_subMenuCachedList.Add(character);
			}
		}
		return _subMenuCachedList;
	}

	public void SetCurrentIntel(IIntel p_intel)
	{
		currentIntelForContextMenu = p_intel;
	}

	public void Cleanup()
	{
		currentIntelForContextMenu = null;
	}
}

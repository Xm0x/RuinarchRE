using System.Collections.Generic;
using UnityEngine;

public abstract class InfuseChoice : IContextMenuItem
{
	public string name { get; protected set; }

	public string localizedName { get; protected set; }

	public Sprite contextMenuIcon => null;

	public string contextMenuName => localizedName;

	public int contextMenuColumn => 1;

	public List<IContextMenuItem> subMenus => null;

	public abstract void Infuse(FoodPile p_foodPile);

	public bool CanInfuse(FoodPile p_foodPile)
	{
		return p_foodPile.infusedType == FOOD_INFUSE_TYPE.None;
	}

	public void OnPickAction()
	{
		if (PlayerManager.Instance.player.currentlySelectedPlayerActionTarget is FoodPile p_foodPile)
		{
			Infuse(p_foodPile);
			UIManager.Instance.HideContextMenu();
		}
	}

	public bool CanBePickedRegardlessOfCooldown()
	{
		if (PlayerManager.Instance.player.currentlySelectedPlayerActionTarget is FoodPile p_foodPile)
		{
			return CanInfuse(p_foodPile);
		}
		return false;
	}

	public abstract bool IsValid();

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

	public virtual string GetReasonsWhyCannotPerformAbilityTowards(TileObject p_tileObject)
	{
		return string.Empty;
	}
}

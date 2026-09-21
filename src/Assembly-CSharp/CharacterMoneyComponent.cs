using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

public class CharacterMoneyComponent : CharacterComponent
{
	public int coins { get; private set; }

	public CharacterMoneyComponent()
	{
		coins = 50;
	}

	public CharacterMoneyComponent(SaveDataCharacterMoneyComponent data)
	{
		coins = data.coins;
	}

	public void Initialize()
	{
		if (!base.owner.isNormalCharacter || base.owner.isConsideredRatman)
		{
			coins = 0;
		}
	}

	public void AdjustCoins(int amount)
	{
		coins += amount;
		if (coins < 0)
		{
			coins = 0;
		}
		if (base.owner.hasMarker)
		{
			Color p_color = Color.green;
			string empty = string.Empty;
			if (amount < 0)
			{
				p_color = Color.red;
				empty = $"{Utilities.CoinIcon()}{amount}";
			}
			else
			{
				empty = $"+{Utilities.CoinIcon()}{amount}";
			}
			if (empty != string.Empty)
			{
				InnerMapManager.Instance.ShowAreaMapTextPopup(empty, base.owner.worldPosition, p_color);
			}
		}
	}

	public bool HasCoins()
	{
		return coins > 0;
	}

	public bool CanAfford(int p_amount)
	{
		return coins >= p_amount;
	}

	public void GainCoinsAfterDoingAction(ActualGoapNode p_action)
	{
	}

	public void GainCoinsAfterDoingJob(JobQueueItem p_job)
	{
	}

	public void LoadReferences(SaveDataCharacterMoneyComponent data)
	{
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}
}

using System;
using UnityEngine;

public class CurrenciesComponent
{
	public int chaoticEnergy { get; private set; }

	public int maxChaoticEnergy { get; private set; }

	public int mana { get; private set; }

	public int spiritEnergy { get; private set; }

	public CurrenciesComponent()
	{
		mana = EditableValuesManager.Instance.startingMana;
		spiritEnergy = EditableValuesManager.Instance.startingSpiritEnergy;
		chaoticEnergy = EditableValuesManager.Instance.GetInitialChaoticEnergyBaseOnGameMode();
		maxChaoticEnergy = EditableValuesManager.Instance.GetMaxChaoticEnergyPerPortalLevel(1);
	}

	public CurrenciesComponent(SaveDataCurrenciesComponent p_component)
	{
	}

	public void LoadReferences(SaveDataCurrenciesComponent p_data)
	{
		chaoticEnergy = p_data.chaoticEnergy;
		maxChaoticEnergy = p_data.maxChaoticEnergy;
		AdjustMana(p_data.mana, shouldBroadcastSignal: false);
		AdjustSpiritEnergy(p_data.spiritEnergy, shouldBroadcastSignal: false);
	}

	public void SubscribeListeners()
	{
		Messenger.AddListener<int>(PlayerSignals.PLAYER_FINISHED_PORTAL_UPGRADE, OnPortalUpgraded);
	}

	private void OnPortalUpgraded(int p_currentPortalLevel)
	{
		int maxChaoticEnergyPerPortalLevel = EditableValuesManager.Instance.GetMaxChaoticEnergyPerPortalLevel(p_currentPortalLevel);
		if (maxChaoticEnergyPerPortalLevel != -1)
		{
			maxChaoticEnergy = maxChaoticEnergyPerPortalLevel;
		}
	}

	public void AdjustChaoticEnergy(int amount)
	{
		chaoticEnergy = Mathf.Clamp(chaoticEnergy + amount, 0, maxChaoticEnergy);
		Messenger.Broadcast(PlayerSignals.UPDATED_CHAOTIC_ENERGY, chaoticEnergy);
		Messenger.Broadcast(PlayerSignals.CHAOTIC_ENERGY_ADJUSTED, amount, chaoticEnergy);
		if (amount > 0)
		{
			AdjustSpiritEnergy(10);
		}
	}

	public void AdjustChaoticEnergyWithoutAffectingSpiritEnergy(int amount)
	{
		chaoticEnergy = Mathf.Clamp(chaoticEnergy + amount, 0, maxChaoticEnergy);
		Messenger.Broadcast(PlayerSignals.UPDATED_CHAOTIC_ENERGY, chaoticEnergy);
		Messenger.Broadcast(PlayerSignals.CHAOTIC_ENERGY_ADJUSTED, amount, chaoticEnergy);
	}

	public void AdjustChaoticEnergyNoLimit(int amount)
	{
		chaoticEnergy += amount;
		chaoticEnergy = Mathf.Max(0, amount);
		Messenger.Broadcast(PlayerSignals.UPDATED_CHAOTIC_ENERGY, chaoticEnergy);
		Messenger.Broadcast(PlayerSignals.CHAOTIC_ENERGY_ADJUSTED, amount, chaoticEnergy);
	}

	public void GainPlaguePointFromCharacter(int amount, Character p_character)
	{
	}

	public bool CanGainPlaguePoints()
	{
		return false;
	}

	public void AdjustSpiritEnergy(int amount, bool shouldBroadcastSignal = true)
	{
		if (WorldSettings.Instance.worldSettingsData.IsSpiritEnergyEnabledBasedOnVictoryCondition())
		{
			spiritEnergy += amount;
			spiritEnergy = Mathf.Clamp(spiritEnergy, 0, 100000);
			if (shouldBroadcastSignal)
			{
				Messenger.Broadcast(PlayerSignals.PLAYER_ADJUSTED_SPIRIT_ENERGY, amount, spiritEnergy);
				Messenger.Broadcast(PlayerSkillSignals.FORCE_RELOAD_PLAYER_ACTIONS);
			}
		}
	}

	public void AdjustMana(int amount, bool shouldBroadcastSignal = true)
	{
		mana += amount;
		mana = Mathf.Clamp(mana, 0, EditableValuesManager.Instance.maximumMana);
		if (shouldBroadcastSignal)
		{
			Messenger.Broadcast(PlayerSignals.PLAYER_ADJUSTED_MANA, amount, mana);
			Messenger.Broadcast(PlayerSkillSignals.FORCE_RELOAD_PLAYER_ACTIONS);
		}
	}

	public void AdjustManaNoLimit(int amount, bool shouldBroadcastSignal = true)
	{
		mana += amount;
		mana = Mathf.Max(0, mana);
		if (shouldBroadcastSignal)
		{
			Messenger.Broadcast(PlayerSignals.PLAYER_ADJUSTED_MANA, amount, mana);
			Messenger.Broadcast(PlayerSkillSignals.FORCE_RELOAD_PLAYER_ACTIONS);
		}
	}

	public int GetManaCostForInterventionAbility(PLAYER_SKILL_TYPE ability)
	{
		int spellTier = PlayerManager.Instance.GetSpellTier(ability);
		return PlayerManager.Instance.GetManaCostForSpell(spellTier);
	}

	private void AdjustCurrency(CURRENCY p_currency, int p_amount, bool affectSpiritEnergy = true)
	{
		switch (p_currency)
		{
		case CURRENCY.Mana:
			AdjustMana(p_amount);
			break;
		case CURRENCY.Chaotic_Energy:
			AdjustChaoticEnergy(p_amount);
			break;
		case CURRENCY.Spirit_Energy:
			AdjustSpiritEnergy(p_amount);
			break;
		default:
			throw new ArgumentOutOfRangeException("p_currency", p_currency, null);
		}
	}

	public void AddCurrency(Cost p_cost)
	{
		AdjustCurrency(p_cost.currency, p_cost.processedAmount);
	}

	public void ReduceCurrency(Cost p_cost)
	{
		AdjustCurrency(p_cost.currency, -p_cost.processedAmount);
	}

	public void AddCurrency(Reward p_reward)
	{
		AdjustCurrency(p_reward.currency, p_reward.amount);
	}

	public void ReduceCurrency(Reward p_reward)
	{
		AdjustCurrency(p_reward.currency, -p_reward.amount);
	}

	public bool CanAfford(CURRENCY p_currency, int p_amount)
	{
		return p_currency switch
		{
			CURRENCY.Mana => mana >= p_amount, 
			CURRENCY.Chaotic_Energy => chaoticEnergy >= p_amount, 
			CURRENCY.Spirit_Energy => spiritEnergy >= p_amount, 
			_ => throw new ArgumentOutOfRangeException("p_currency", p_currency, null), 
		};
	}

	public bool CanAfford(Cost p_cost)
	{
		return CanAfford(p_cost.currency, p_cost.processedAmount);
	}

	public bool CanAfford(Cost[] p_cost)
	{
		bool result = true;
		foreach (Cost p_cost2 in p_cost)
		{
			if (!CanAfford(p_cost2))
			{
				result = false;
				break;
			}
		}
		return result;
	}
}

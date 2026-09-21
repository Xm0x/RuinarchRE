using System.Collections.Generic;
using Maccima_Games.Util;
using Plague.Death_Effect;
using Ruinarch.MVCFramework;
using UnityEngine;
using UtilityScripts;

public class OnDeathUIController : MVCUIController, OnDeathUIView.IListener
{
	[SerializeField]
	private OnDeathUIModel m_onDeathUIModel;

	private OnDeathUIView m_onDeathUIView;

	private string _invalidTextMaxActiveOnDeath;

	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		OnDeathUIView.Create(_canvas, m_onDeathUIModel, delegate(OnDeathUIView p_ui)
		{
			m_onDeathUIView = p_ui;
			m_onDeathUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
		});
	}

	private void OnDestroy()
	{
		m_onDeathUIView?.Unsubscribe(this);
	}

	public override void ShowUI()
	{
		base.ShowUI();
		UpdateAllDeathEffects();
	}

	private void UpdateAllDeathEffects()
	{
		UpdateDeathEffectData(PLAGUE_DEATH_EFFECT.Explosion);
		UpdateDeathEffectData(PLAGUE_DEATH_EFFECT.Zombie);
		UpdateDeathEffectData(PLAGUE_DEATH_EFFECT.Chaos_Generator);
		UpdateDeathEffectData(PLAGUE_DEATH_EFFECT.Haunted_Spirits);
	}

	private void UpdateDeathEffectData(PLAGUE_DEATH_EFFECT p_deathEffect)
	{
		if (PlagueDisease.Instance.HasMaxActiveDeathEffect() && PlagueDisease.Instance.IsDeathEffectActive(p_deathEffect, out var deathEffect))
		{
			int finalNextLevelUpgradeCost = deathEffect.GetFinalNextLevelUpgradeCost();
			bool flag = finalNextLevelUpgradeCost == -1;
			m_onDeathUIView.UpdateDeathEffectCost(p_deathEffect, flag ? LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "MAX") : (finalNextLevelUpgradeCost + Utilities.ChaoticEnergyIcon()));
			m_onDeathUIView.UpdateDeathEffectDescription(p_deathEffect, deathEffect.GetCurrentEffectDescription());
			m_onDeathUIView.UpdateDeathEffectUpgradeButtonInteractable(p_deathEffect, !flag && CanAffordUnlockOrUpgrade(p_deathEffect));
		}
		else
		{
			m_onDeathUIView.UpdateDeathEffectCost(p_deathEffect, p_deathEffect.GetUnlockCost() + Utilities.ChaoticEnergyIcon());
			m_onDeathUIView.UpdateDeathEffectDescription(p_deathEffect, string.Empty);
			m_onDeathUIView.UpdateDeathEffectUpgradeButtonInteractable(p_deathEffect, !PlagueDisease.Instance.HasMaxActiveDeathEffect() && CanAffordUnlockOrUpgrade(p_deathEffect));
		}
	}

	public void OnIgniteUpgradeClicked()
	{
		PayForUnlockOrUpgrade(PLAGUE_DEATH_EFFECT.Explosion);
		SetOrUpgradeDeathEffect(PLAGUE_DEATH_EFFECT.Explosion);
		UpdateAllDeathEffects();
		PlayUpgradeSFX();
	}

	public void OnWalkerZombieUpgradeClicked()
	{
		PayForUnlockOrUpgrade(PLAGUE_DEATH_EFFECT.Zombie);
		SetOrUpgradeDeathEffect(PLAGUE_DEATH_EFFECT.Zombie);
		UpdateAllDeathEffects();
		PlayUpgradeSFX();
	}

	public void OnMana2_3UpgradeClicked()
	{
		PayForUnlockOrUpgrade(PLAGUE_DEATH_EFFECT.Chaos_Generator);
		SetOrUpgradeDeathEffect(PLAGUE_DEATH_EFFECT.Chaos_Generator);
		UpdateAllDeathEffects();
		PlayUpgradeSFX();
	}

	public void OnRandomSpirit_1UpgradeClicked()
	{
		PayForUnlockOrUpgrade(PLAGUE_DEATH_EFFECT.Haunted_Spirits);
		SetOrUpgradeDeathEffect(PLAGUE_DEATH_EFFECT.Haunted_Spirits);
		UpdateAllDeathEffects();
		PlayUpgradeSFX();
	}

	public void OnIgniteHoveredOver(UIHoverPosition hoverPosition)
	{
		ShowTooltip(PLAGUE_DEATH_EFFECT.Explosion, hoverPosition);
	}

	public void OnWalkerZombieHoveredOver(UIHoverPosition hoverPosition)
	{
		ShowTooltip(PLAGUE_DEATH_EFFECT.Zombie, hoverPosition);
	}

	public void OnManaHoveredOver(UIHoverPosition hoverPosition)
	{
		ShowTooltip(PLAGUE_DEATH_EFFECT.Chaos_Generator, hoverPosition);
	}

	public void OnSpiritHoveredOver(UIHoverPosition hoverPosition)
	{
		ShowTooltip(PLAGUE_DEATH_EFFECT.Haunted_Spirits, hoverPosition);
	}

	public void OnIgniteHoveredOut()
	{
		HideTooltip();
	}

	public void OnWalkerZombieHoveredOut()
	{
		HideTooltip();
	}

	public void OnManaHoveredOut()
	{
		HideTooltip();
	}

	public void OnSpiritHoveredOut()
	{
		HideTooltip();
	}

	private void ShowTooltip(PLAGUE_DEATH_EFFECT p_deathEffectType, UIHoverPosition p_hoverPosition)
	{
		if (!(UIManager.Instance != null))
		{
			return;
		}
		int num = 0;
		if (PlagueDisease.Instance.IsDeathEffectActive(p_deathEffectType, out var deathEffect))
		{
			num = deathEffect.level;
		}
		string text = "<b><size=18>" + LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Current Effect") + "</b>";
		text = text + "\n<line-height=70%><size=16>" + p_deathEffectType.GetEffectTooltip(num);
		text = text + "\n\n<color=\"green\"><b><size=18>" + LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "On Upgrade") + "</b>";
		text = text + "\n<line-height=70%><size=16>" + p_deathEffectType.GetEffectTooltip(num + 1);
		if (!PlagueDisease.Instance.IsDeathEffectActive(p_deathEffectType) && PlagueDisease.Instance.HasMaxActiveDeathEffect())
		{
			if (_invalidTextMaxActiveOnDeath == null)
			{
				Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
				dictionary.Add("amount", PlagueDisease.Instance.activeMaxDeathEffect.ToString());
				dictionary.Add("plagueCategory", LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "On Death"));
				_invalidTextMaxActiveOnDeath = Utilities.ColorizeInvalidText(LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Max_Choice_Tooltip", dictionary));
				MaccimaDictionaryPool<string, string>.Release(dictionary);
			}
			text = text + "\n" + _invalidTextMaxActiveOnDeath;
		}
		UIManager.Instance.ShowSmallInfo(text, p_hoverPosition);
	}

	private void HideTooltip()
	{
		if (UIManager.Instance != null)
		{
			UIManager.Instance.HideSmallInfo();
		}
	}

	private void SetOrUpgradeDeathEffect(PLAGUE_DEATH_EFFECT p_deathEffect)
	{
		if (PlagueDisease.Instance.activeDeathEffect != null)
		{
			PlagueDisease.Instance.activeDeathEffect.AdjustLevel(1);
		}
		else
		{
			PlagueDisease.Instance.SetNewPlagueDeathEffectAndUnsetPrev(p_deathEffect);
		}
	}

	private void PayForUnlockOrUpgrade(PLAGUE_DEATH_EFFECT p_deathEffect)
	{
		int unlockOrUpgradeCost = GetUnlockOrUpgradeCost(p_deathEffect);
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null)
		{
			PlayerManager.Instance.player.currenciesComponent.AdjustChaoticEnergy(-unlockOrUpgradeCost);
		}
	}

	private bool CanAffordUnlockOrUpgrade(PLAGUE_DEATH_EFFECT p_deathEffect)
	{
		int unlockOrUpgradeCost = GetUnlockOrUpgradeCost(p_deathEffect);
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null)
		{
			if (PlayerManager.Instance.player.currenciesComponent.chaoticEnergy < unlockOrUpgradeCost)
			{
				if (WorldSettings.Instance != null)
				{
					return WorldSettings.Instance.worldSettingsData.playerSkillSettings.costAmount == SKILL_COST_AMOUNT.None;
				}
				return false;
			}
			return true;
		}
		return true;
	}

	private int GetUnlockOrUpgradeCost(PLAGUE_DEATH_EFFECT p_deathEffect)
	{
		if (PlagueDisease.Instance.activeDeathEffect == null)
		{
			return p_deathEffect.GetUnlockCost();
		}
		return PlagueDisease.Instance.activeDeathEffect.GetFinalNextLevelUpgradeCost();
	}

	private void PlayUpgradeSFX()
	{
		switch (PlagueDisease.Instance.activeDeathEffect.level)
		{
		case 1:
			AudioManager.Instance.TryPlayUISFX("Play_Upgrade_Power_1");
			break;
		case 2:
			AudioManager.Instance.TryPlayUISFX("Play_Upgrade_Power_2");
			break;
		case 3:
			AudioManager.Instance.TryPlayUISFX("Play_Upgrade_Power_3");
			break;
		}
	}
}

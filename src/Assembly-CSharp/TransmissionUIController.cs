using System;
using System.Collections.Generic;
using Maccima_Games.Util;
using Plague.Transmission;
using Ruinarch.MVCFramework;
using UnityEngine;
using UtilityScripts;

public class TransmissionUIController : MVCUIController, TransmissionUIView.IListener
{
	[SerializeField]
	private TransmissionUIModel m_transmissionUIModel;

	private TransmissionUIView m_transmissionUIView;

	private string _invalidTextMaxActiveTransmission;

	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		TransmissionUIView.Create(_canvas, m_transmissionUIModel, delegate(TransmissionUIView p_ui)
		{
			m_transmissionUIView = p_ui;
			m_transmissionUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
		});
	}

	private void OnDestroy()
	{
		m_transmissionUIView?.Unsubscribe(this);
	}

	public override void ShowUI()
	{
		base.ShowUI();
		UpdateAllTransmissionData();
	}

	public void OnAirBorneUpgradeClicked()
	{
		PayForUpgrade(PLAGUE_TRANSMISSION.Airborne);
		int num = PlagueDisease.Instance.UpgradeTransmissionLevel(PLAGUE_TRANSMISSION.Airborne);
		UpdateAllTransmissionData();
		switch (num)
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

	public void OnConsumptionUpgradeClicked()
	{
		PayForUpgrade(PLAGUE_TRANSMISSION.Consumption);
		int num = PlagueDisease.Instance.UpgradeTransmissionLevel(PLAGUE_TRANSMISSION.Consumption);
		UpdateAllTransmissionData();
		switch (num)
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

	public void OnPhysicalContactUpgradeClicked()
	{
		PayForUpgrade(PLAGUE_TRANSMISSION.Physical_Contact);
		int num = PlagueDisease.Instance.UpgradeTransmissionLevel(PLAGUE_TRANSMISSION.Physical_Contact);
		UpdateAllTransmissionData();
		switch (num)
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

	public void OnCombatUpgradeClicked()
	{
		PayForUpgrade(PLAGUE_TRANSMISSION.Combat);
		int num = PlagueDisease.Instance.UpgradeTransmissionLevel(PLAGUE_TRANSMISSION.Combat);
		UpdateAllTransmissionData();
		switch (num)
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

	public void OnAirBorneHoveredOver(UIHoverPosition p_hoverPosition)
	{
		ShowTooltip(PLAGUE_TRANSMISSION.Airborne, p_hoverPosition);
	}

	public void OnConsumptionHoveredOver(UIHoverPosition p_hoverPosition)
	{
		ShowTooltip(PLAGUE_TRANSMISSION.Consumption, p_hoverPosition);
	}

	public void OnPhysicalContactHoveredOver(UIHoverPosition p_hoverPosition)
	{
		ShowTooltip(PLAGUE_TRANSMISSION.Physical_Contact, p_hoverPosition);
	}

	public void OnCombatHoveredOver(UIHoverPosition p_hoverPosition)
	{
		ShowTooltip(PLAGUE_TRANSMISSION.Combat, p_hoverPosition);
	}

	public void OnAirBorneHoveredOut()
	{
		HideTooltip();
	}

	public void OnConsumptionHoveredOut()
	{
		HideTooltip();
	}

	public void OnPhysicalContactHoveredOut()
	{
		HideTooltip();
	}

	public void OnCombatHoveredOut()
	{
		HideTooltip();
	}

	private void ShowTooltip(PLAGUE_TRANSMISSION p_transmissionType, UIHoverPosition p_hoverPosition)
	{
		if (!(UIManager.Instance != null))
		{
			return;
		}
		string header = string.Empty;
		switch (p_transmissionType)
		{
		case PLAGUE_TRANSMISSION.Airborne:
			header = LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Airborne Rate");
			break;
		case PLAGUE_TRANSMISSION.Consumption:
			header = LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Consumption Rate");
			break;
		case PLAGUE_TRANSMISSION.Physical_Contact:
			header = LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Direct Contact Rate");
			break;
		case PLAGUE_TRANSMISSION.Combat:
			header = LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Combat Rate");
			break;
		}
		string text = p_transmissionType.GetTransmissionTooltip();
		if (!PlagueDisease.Instance.IsTransmissionActive(p_transmissionType) && PlagueDisease.Instance.HasMaxActiveTransmissions())
		{
			if (_invalidTextMaxActiveTransmission == null)
			{
				Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
				dictionary.Add("amount", PlagueDisease.Instance.activeMaxTransmissions.ToString());
				dictionary.Add("plagueCategory", LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Transmission"));
				_invalidTextMaxActiveTransmission = Utilities.ColorizeInvalidText(LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Max_Choice_Tooltip", dictionary));
				MaccimaDictionaryPool<string, string>.Release(dictionary);
			}
			text = text + "\n" + _invalidTextMaxActiveTransmission;
		}
		UIManager.Instance.ShowSmallInfo(text, p_hoverPosition, header);
	}

	private void HideTooltip()
	{
		if (UIManager.Instance != null)
		{
			UIManager.Instance.HideSmallInfo();
		}
	}

	private void UpdateAllTransmissionData()
	{
		UpdateTransmissionData(PLAGUE_TRANSMISSION.Airborne);
		UpdateTransmissionData(PLAGUE_TRANSMISSION.Consumption);
		UpdateTransmissionData(PLAGUE_TRANSMISSION.Physical_Contact);
		UpdateTransmissionData(PLAGUE_TRANSMISSION.Combat);
	}

	private void UpdateTransmissionData(PLAGUE_TRANSMISSION p_transmissionType)
	{
		int transmissionUpgradeCost = GetTransmissionUpgradeCost(p_transmissionType);
		m_transmissionUIView.UpdateTransmissionRate(p_transmissionType, PlagueDisease.Instance.GetTransmissionRateDescription(PlagueDisease.Instance.GetTransmissionLevel(p_transmissionType)));
		bool flag = PlagueDisease.Instance.IsMaxLevel(p_transmissionType);
		bool flag2 = !flag && (!PlagueDisease.Instance.HasMaxActiveTransmissions() || PlagueDisease.Instance.IsTransmissionActive(p_transmissionType));
		m_transmissionUIView.UpdateTransmissionUpgradeButtonInteractable(p_transmissionType, flag2 && CanAffordUpgrade(p_transmissionType));
		m_transmissionUIView.UpdateTransmissionCost(p_transmissionType, flag ? LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "MAX") : (transmissionUpgradeCost + Utilities.ChaoticEnergyIcon()));
	}

	private int GetTransmissionUpgradeCost(PLAGUE_TRANSMISSION p_transmissionType)
	{
		return p_transmissionType switch
		{
			PLAGUE_TRANSMISSION.Airborne => Transmission<AirborneTransmission>.Instance.GetFinalTransmissionNextLevelCost(PlagueDisease.Instance.GetTransmissionLevel(PLAGUE_TRANSMISSION.Airborne)), 
			PLAGUE_TRANSMISSION.Consumption => Transmission<ConsumptionTransmission>.Instance.GetFinalTransmissionNextLevelCost(PlagueDisease.Instance.GetTransmissionLevel(PLAGUE_TRANSMISSION.Consumption)), 
			PLAGUE_TRANSMISSION.Physical_Contact => Transmission<PhysicalContactTransmission>.Instance.GetFinalTransmissionNextLevelCost(PlagueDisease.Instance.GetTransmissionLevel(PLAGUE_TRANSMISSION.Physical_Contact)), 
			PLAGUE_TRANSMISSION.Combat => Transmission<CombatRateTransmission>.Instance.GetFinalTransmissionNextLevelCost(PlagueDisease.Instance.GetTransmissionLevel(PLAGUE_TRANSMISSION.Combat)), 
			_ => throw new ArgumentOutOfRangeException("p_transmissionType", p_transmissionType, null), 
		};
	}

	private void PayForUpgrade(PLAGUE_TRANSMISSION p_transmissionType)
	{
		int num = p_transmissionType switch
		{
			PLAGUE_TRANSMISSION.Airborne => Transmission<AirborneTransmission>.Instance.GetFinalTransmissionNextLevelCost(PlagueDisease.Instance.GetTransmissionLevel(p_transmissionType)), 
			PLAGUE_TRANSMISSION.Consumption => Transmission<ConsumptionTransmission>.Instance.GetFinalTransmissionNextLevelCost(PlagueDisease.Instance.GetTransmissionLevel(p_transmissionType)), 
			PLAGUE_TRANSMISSION.Physical_Contact => Transmission<PhysicalContactTransmission>.Instance.GetFinalTransmissionNextLevelCost(PlagueDisease.Instance.GetTransmissionLevel(p_transmissionType)), 
			PLAGUE_TRANSMISSION.Combat => Transmission<CombatRateTransmission>.Instance.GetFinalTransmissionNextLevelCost(PlagueDisease.Instance.GetTransmissionLevel(p_transmissionType)), 
			_ => throw new ArgumentOutOfRangeException("p_transmissionType", p_transmissionType, null), 
		};
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null)
		{
			PlayerManager.Instance.player.currenciesComponent.AdjustChaoticEnergy(-num);
		}
	}

	private bool CanAffordUpgrade(PLAGUE_TRANSMISSION p_transmissionType)
	{
		int num = p_transmissionType switch
		{
			PLAGUE_TRANSMISSION.Airborne => Transmission<AirborneTransmission>.Instance.GetFinalTransmissionNextLevelCost(PlagueDisease.Instance.GetTransmissionLevel(p_transmissionType)), 
			PLAGUE_TRANSMISSION.Consumption => Transmission<ConsumptionTransmission>.Instance.GetFinalTransmissionNextLevelCost(PlagueDisease.Instance.GetTransmissionLevel(p_transmissionType)), 
			PLAGUE_TRANSMISSION.Physical_Contact => Transmission<PhysicalContactTransmission>.Instance.GetFinalTransmissionNextLevelCost(PlagueDisease.Instance.GetTransmissionLevel(p_transmissionType)), 
			PLAGUE_TRANSMISSION.Combat => Transmission<CombatRateTransmission>.Instance.GetFinalTransmissionNextLevelCost(PlagueDisease.Instance.GetTransmissionLevel(p_transmissionType)), 
			_ => throw new ArgumentOutOfRangeException("p_transmissionType", p_transmissionType, null), 
		};
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null)
		{
			if (PlayerManager.Instance.player.currenciesComponent.chaoticEnergy < num)
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
}

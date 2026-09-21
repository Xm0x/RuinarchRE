using System.Collections.Generic;
using Maccima_Games.Util;
using Plague.Fatality;
using Ruinarch.MVCFramework;
using UnityEngine;
using UtilityScripts;

public class FatalityUIController : MVCUIController, FatalityUIView.IListener
{
	[SerializeField]
	private FatalityUIModel m_fatalityUIModel;

	private FatalityUIView m_fatalityUIView;

	private string _invalidTextMaxActiveFatality;

	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		FatalityUIView.Create(_canvas, m_fatalityUIModel, delegate(FatalityUIView p_ui)
		{
			m_fatalityUIView = p_ui;
			m_fatalityUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
		});
	}

	private void OnDestroy()
	{
		m_fatalityUIView?.Unsubscribe(this);
	}

	public override void ShowUI()
	{
		base.ShowUI();
		UpdateAllFatalityData();
	}

	private void UpdateAllFatalityData()
	{
		UpdateSepticShockData();
		UpdateHeartAttackData();
		UpdateStrokeData();
		UpdateTotalOrganFailureData();
		UpdatePneumoniaData();
	}

	public void OnSepticShockUpgradeClicked()
	{
		PayForFatality(PLAGUE_FATALITY.Septic_Shock);
		PlagueDisease.Instance.AddAndInitializeFatality(PLAGUE_FATALITY.Septic_Shock);
		UpdateAllFatalityData();
		AudioManager.Instance.TryPlayUISFX("Play_Upgrade_Power_3");
	}

	public void OnHeartAttackUpgradeClicked()
	{
		PayForFatality(PLAGUE_FATALITY.Heart_Attack);
		PlagueDisease.Instance.AddAndInitializeFatality(PLAGUE_FATALITY.Heart_Attack);
		UpdateAllFatalityData();
		AudioManager.Instance.TryPlayUISFX("Play_Upgrade_Power_3");
	}

	public void OnStrokeUpgradeClicked()
	{
		PayForFatality(PLAGUE_FATALITY.Stroke);
		PlagueDisease.Instance.AddAndInitializeFatality(PLAGUE_FATALITY.Stroke);
		UpdateAllFatalityData();
		AudioManager.Instance.TryPlayUISFX("Play_Upgrade_Power_3");
	}

	public void OnTotalOrganFailureUpgradeClicked()
	{
		PayForFatality(PLAGUE_FATALITY.Total_Organ_Failure);
		PlagueDisease.Instance.AddAndInitializeFatality(PLAGUE_FATALITY.Total_Organ_Failure);
		UpdateAllFatalityData();
		AudioManager.Instance.TryPlayUISFX("Play_Upgrade_Power_3");
	}

	public void OnPneumoniaUpgradeClicked()
	{
		PayForFatality(PLAGUE_FATALITY.Pneumonia);
		PlagueDisease.Instance.AddAndInitializeFatality(PLAGUE_FATALITY.Pneumonia);
		UpdateAllFatalityData();
		AudioManager.Instance.TryPlayUISFX("Play_Upgrade_Power_3");
	}

	public void OnSepticShockHoveredOver(UIHoverPosition hoverPosition)
	{
		ShowTooltip(PLAGUE_FATALITY.Septic_Shock, hoverPosition);
	}

	public void OnSepticShockHoveredOut()
	{
		HideTooltip();
	}

	public void OnHeartAttackHoveredOver(UIHoverPosition hoverPosition)
	{
		ShowTooltip(PLAGUE_FATALITY.Heart_Attack, hoverPosition);
	}

	public void OnHeartAttackHoveredOut()
	{
		HideTooltip();
	}

	public void OnStrokeHoveredOver(UIHoverPosition hoverPosition)
	{
		ShowTooltip(PLAGUE_FATALITY.Stroke, hoverPosition);
	}

	public void OnStrokeHoveredOut()
	{
		HideTooltip();
	}

	public void OnTotalOrganFailureHoveredOver(UIHoverPosition hoverPosition)
	{
		ShowTooltip(PLAGUE_FATALITY.Total_Organ_Failure, hoverPosition);
	}

	public void OnTotalOrganFailureHoveredOut()
	{
		HideTooltip();
	}

	public void OnPneumoniaHoveredOver(UIHoverPosition hoverPosition)
	{
		ShowTooltip(PLAGUE_FATALITY.Pneumonia, hoverPosition);
	}

	public void OnPneumoniaHoveredOut()
	{
		HideTooltip();
	}

	private void ShowTooltip(PLAGUE_FATALITY p_fatalityType, UIHoverPosition p_hoverPosition)
	{
		if (!(UIManager.Instance != null))
		{
			return;
		}
		string key = p_fatalityType.ToStringEnumWithSpace();
		string text = p_fatalityType.GetFatalityTooltip();
		if (!PlagueDisease.Instance.IsFatalityActive(p_fatalityType) && PlagueDisease.Instance.HasMaxActiveFatalities())
		{
			if (_invalidTextMaxActiveFatality == null)
			{
				Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
				dictionary.Add("amount", PlagueDisease.Instance.activeMaxFatalities.ToString());
				dictionary.Add("plagueCategory", LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Fatality"));
				_invalidTextMaxActiveFatality = Utilities.ColorizeInvalidText(LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Max_Choice_Tooltip", dictionary));
				MaccimaDictionaryPool<string, string>.Release(dictionary);
			}
			text = text + "\n" + _invalidTextMaxActiveFatality;
		}
		UIManager.Instance.ShowSmallInfo(text, p_hoverPosition, LocalizationManager.Instance.GetLocalizedValue("Plague_Table", key));
	}

	private void HideTooltip()
	{
		if (UIManager.Instance != null)
		{
			UIManager.Instance.HideSmallInfo();
		}
	}

	private void UpdateSepticShockData()
	{
		bool flag = PlagueDisease.Instance.IsFatalityActive(PLAGUE_FATALITY.Septic_Shock);
		m_fatalityUIView.UpdateSepticShockCost(PLAGUE_FATALITY.Septic_Shock.GetFatalityCost() + Utilities.ChaoticEnergyIcon());
		m_fatalityUIView.UpdateSepticShockCostState(!flag);
		m_fatalityUIView.UpdateSepticShockUpgradeButtonInteractable(!flag && !PlagueDisease.Instance.HasMaxActiveFatalities() && CanAffordSymptom(PLAGUE_FATALITY.Septic_Shock));
		m_fatalityUIView.UpdateSepticShockCheckmarkState(flag);
	}

	private void UpdateHeartAttackData()
	{
		bool flag = PlagueDisease.Instance.IsFatalityActive(PLAGUE_FATALITY.Heart_Attack);
		m_fatalityUIView.UpdateHeartAttackCost(PLAGUE_FATALITY.Heart_Attack.GetFatalityCost() + Utilities.ChaoticEnergyIcon());
		m_fatalityUIView.UpdateHeartAttackCostState(!flag);
		m_fatalityUIView.UpdateHeartAttackUpgradeButtonInteractable(!flag && !PlagueDisease.Instance.HasMaxActiveFatalities() && CanAffordSymptom(PLAGUE_FATALITY.Heart_Attack));
		m_fatalityUIView.UpdateHeartAttackCheckmarkState(flag);
	}

	private void UpdateStrokeData()
	{
		bool flag = PlagueDisease.Instance.IsFatalityActive(PLAGUE_FATALITY.Stroke);
		m_fatalityUIView.UpdateStrokeCost(PLAGUE_FATALITY.Stroke.GetFatalityCost() + Utilities.ChaoticEnergyIcon());
		m_fatalityUIView.UpdateStrokeCostState(!flag);
		m_fatalityUIView.UpdateStrokeUpgradeButtonInteractable(!flag && !PlagueDisease.Instance.HasMaxActiveFatalities() && CanAffordSymptom(PLAGUE_FATALITY.Stroke));
		m_fatalityUIView.UpdateStrokeCheckmarkState(flag);
	}

	private void UpdateTotalOrganFailureData()
	{
		bool flag = PlagueDisease.Instance.IsFatalityActive(PLAGUE_FATALITY.Total_Organ_Failure);
		m_fatalityUIView.UpdateTotalOrganFailureCost(PLAGUE_FATALITY.Total_Organ_Failure.GetFatalityCost() + Utilities.ChaoticEnergyIcon());
		m_fatalityUIView.UpdateTotalOrganFailureCostState(!flag);
		m_fatalityUIView.UpdateTotalOrganFailureUpgradeButtonInteractable(!flag && !PlagueDisease.Instance.HasMaxActiveFatalities() && CanAffordSymptom(PLAGUE_FATALITY.Total_Organ_Failure));
		m_fatalityUIView.UpdateTotalOrganFailureCheckmarkState(flag);
	}

	private void UpdatePneumoniaData()
	{
		bool flag = PlagueDisease.Instance.IsFatalityActive(PLAGUE_FATALITY.Pneumonia);
		m_fatalityUIView.UpdatePneumoniaCost(PLAGUE_FATALITY.Pneumonia.GetFatalityCost() + Utilities.ChaoticEnergyIcon());
		m_fatalityUIView.UpdatePneumoniaCostState(!flag);
		m_fatalityUIView.UpdatePneumoniaUpgradeButtonInteractable(!flag && !PlagueDisease.Instance.HasMaxActiveFatalities() && CanAffordSymptom(PLAGUE_FATALITY.Pneumonia));
		m_fatalityUIView.UpdatePneumoniaCheckmarkState(flag);
	}

	private void PayForFatality(PLAGUE_FATALITY p_fatalityType)
	{
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null)
		{
			PlayerManager.Instance.player.currenciesComponent.AdjustChaoticEnergy(-p_fatalityType.GetFatalityCost());
		}
	}

	private bool CanAffordSymptom(PLAGUE_FATALITY p_fatalityType)
	{
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null)
		{
			if (PlayerManager.Instance.player.currenciesComponent.chaoticEnergy < p_fatalityType.GetFatalityCost())
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

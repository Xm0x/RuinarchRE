using System.Collections.Generic;
using Maccima_Games.Util;
using Plague.Symptom;
using Ruinarch.MVCFramework;
using UnityEngine;
using UtilityScripts;

public class SymptomsUIController : MVCUIController, SymptomsUIView.IListener
{
	[SerializeField]
	private SymptomsUIModel m_symptomsUIModel;

	private SymptomsUIView m_symptomsUIView;

	private string _invalidTextMaxActiveSymptom;

	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		SymptomsUIView.Create(_canvas, m_symptomsUIModel, delegate(SymptomsUIView p_ui)
		{
			m_symptomsUIView = p_ui;
			m_symptomsUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
		});
	}

	private void OnDestroy()
	{
		m_symptomsUIView?.Unsubscribe(this);
	}

	public override void ShowUI()
	{
		base.ShowUI();
		UpdateAllSymptomsData();
	}

	private void UpdateAllSymptomsData()
	{
		UpdateSymptomData(PLAGUE_SYMPTOM.Paralysis);
		UpdateSymptomData(PLAGUE_SYMPTOM.Vomiting);
		UpdateSymptomData(PLAGUE_SYMPTOM.Lethargy);
		UpdateSymptomData(PLAGUE_SYMPTOM.Seizure);
		UpdateSymptomData(PLAGUE_SYMPTOM.Insomnia);
		UpdateSymptomData(PLAGUE_SYMPTOM.Poison_Cloud);
		UpdateSymptomData(PLAGUE_SYMPTOM.Monster_Scent);
		UpdateSymptomData(PLAGUE_SYMPTOM.Sneezing);
		UpdateSymptomData(PLAGUE_SYMPTOM.Depression);
		UpdateSymptomData(PLAGUE_SYMPTOM.Hunger_Pangs);
	}

	private void UpdateSymptomData(PLAGUE_SYMPTOM p_symptomType)
	{
		bool flag = PlagueDisease.Instance.IsSymptomActive(p_symptomType);
		m_symptomsUIView.UpdateSymptomCost(p_symptomType, p_symptomType.GetSymptomCost() + Utilities.ChaoticEnergyIcon());
		m_symptomsUIView.UpdateSymptomCostState(p_symptomType, !flag);
		m_symptomsUIView.UpdateSymptomUpgradeButtonInteractable(p_symptomType, !flag && !PlagueDisease.Instance.HasMaxActiveSymptoms() && CanAffordSymptom(p_symptomType));
		m_symptomsUIView.UpdateSymptomCheckmarkState(p_symptomType, flag);
	}

	public void OnParalysisUpgradeClicked()
	{
		PayForSymptom(PLAGUE_SYMPTOM.Paralysis);
		PlagueDisease.Instance.AddAndInitializeSymptom(PLAGUE_SYMPTOM.Paralysis);
		UpdateAllSymptomsData();
		AudioManager.Instance.TryPlayUISFX("Play_Upgrade_Power_3");
	}

	public void OnVomitingUpgradeClicked()
	{
		PayForSymptom(PLAGUE_SYMPTOM.Vomiting);
		PlagueDisease.Instance.AddAndInitializeSymptom(PLAGUE_SYMPTOM.Vomiting);
		UpdateAllSymptomsData();
		AudioManager.Instance.TryPlayUISFX("Play_Upgrade_Power_3");
	}

	public void OnLethargyUpgradeClicked()
	{
		PayForSymptom(PLAGUE_SYMPTOM.Lethargy);
		PlagueDisease.Instance.AddAndInitializeSymptom(PLAGUE_SYMPTOM.Lethargy);
		UpdateAllSymptomsData();
		AudioManager.Instance.TryPlayUISFX("Play_Upgrade_Power_3");
	}

	public void OnSeizuresUpgradeClicked()
	{
		PayForSymptom(PLAGUE_SYMPTOM.Seizure);
		PlagueDisease.Instance.AddAndInitializeSymptom(PLAGUE_SYMPTOM.Seizure);
		UpdateAllSymptomsData();
		AudioManager.Instance.TryPlayUISFX("Play_Upgrade_Power_3");
	}

	public void OnInsomniaUpgradeClicked()
	{
		PayForSymptom(PLAGUE_SYMPTOM.Insomnia);
		PlagueDisease.Instance.AddAndInitializeSymptom(PLAGUE_SYMPTOM.Insomnia);
		UpdateAllSymptomsData();
		AudioManager.Instance.TryPlayUISFX("Play_Upgrade_Power_3");
	}

	public void OnPoisonCloudUpgradeClicked()
	{
		PayForSymptom(PLAGUE_SYMPTOM.Poison_Cloud);
		PlagueDisease.Instance.AddAndInitializeSymptom(PLAGUE_SYMPTOM.Poison_Cloud);
		UpdateAllSymptomsData();
		AudioManager.Instance.TryPlayUISFX("Play_Upgrade_Power_3");
	}

	public void OnMonsterScentUpgradeClicked()
	{
		PayForSymptom(PLAGUE_SYMPTOM.Monster_Scent);
		PlagueDisease.Instance.AddAndInitializeSymptom(PLAGUE_SYMPTOM.Monster_Scent);
		UpdateAllSymptomsData();
		AudioManager.Instance.TryPlayUISFX("Play_Upgrade_Power_3");
	}

	public void OnSneezingUpgradeClicked()
	{
		PayForSymptom(PLAGUE_SYMPTOM.Sneezing);
		PlagueDisease.Instance.AddAndInitializeSymptom(PLAGUE_SYMPTOM.Sneezing);
		UpdateAllSymptomsData();
		AudioManager.Instance.TryPlayUISFX("Play_Upgrade_Power_3");
	}

	public void OnDepressionUpgradeClicked()
	{
		PayForSymptom(PLAGUE_SYMPTOM.Depression);
		PlagueDisease.Instance.AddAndInitializeSymptom(PLAGUE_SYMPTOM.Depression);
		UpdateAllSymptomsData();
		AudioManager.Instance.TryPlayUISFX("Play_Upgrade_Power_3");
	}

	public void OnHungerPangsUpgradeClicked()
	{
		PayForSymptom(PLAGUE_SYMPTOM.Hunger_Pangs);
		PlagueDisease.Instance.AddAndInitializeSymptom(PLAGUE_SYMPTOM.Hunger_Pangs);
		UpdateAllSymptomsData();
		AudioManager.Instance.TryPlayUISFX("Play_Upgrade_Power_3");
	}

	public void OnHoverOverParalysis(UIHoverPosition p_hoverPosition)
	{
		ShowTooltip(PLAGUE_SYMPTOM.Paralysis, p_hoverPosition);
	}

	public void OnHoverOverVomiting(UIHoverPosition p_hoverPosition)
	{
		ShowTooltip(PLAGUE_SYMPTOM.Vomiting, p_hoverPosition);
	}

	public void OnHoverOverLethargy(UIHoverPosition p_hoverPosition)
	{
		ShowTooltip(PLAGUE_SYMPTOM.Lethargy, p_hoverPosition);
	}

	public void OnHoverOverSeizures(UIHoverPosition p_hoverPosition)
	{
		ShowTooltip(PLAGUE_SYMPTOM.Seizure, p_hoverPosition);
	}

	public void OnHoverOverInsomnia(UIHoverPosition p_hoverPosition)
	{
		ShowTooltip(PLAGUE_SYMPTOM.Insomnia, p_hoverPosition);
	}

	public void OnHoverOverPoisonCloud(UIHoverPosition p_hoverPosition)
	{
		ShowTooltip(PLAGUE_SYMPTOM.Poison_Cloud, p_hoverPosition);
	}

	public void OnHoverOverMonsterScent(UIHoverPosition p_hoverPosition)
	{
		ShowTooltip(PLAGUE_SYMPTOM.Monster_Scent, p_hoverPosition);
	}

	public void OnHoverOverSneezing(UIHoverPosition p_hoverPosition)
	{
		ShowTooltip(PLAGUE_SYMPTOM.Sneezing, p_hoverPosition);
	}

	public void OnHoverOverDepression(UIHoverPosition p_hoverPosition)
	{
		ShowTooltip(PLAGUE_SYMPTOM.Depression, p_hoverPosition);
	}

	public void OnHoverOverHungerPangs(UIHoverPosition p_hoverPosition)
	{
		ShowTooltip(PLAGUE_SYMPTOM.Hunger_Pangs, p_hoverPosition);
	}

	public void OnHoverOutParalysis()
	{
		HideTooltip();
	}

	public void OnHoverOutVomiting()
	{
		HideTooltip();
	}

	public void OnHoverOutLethargy()
	{
		HideTooltip();
	}

	public void OnHoverOutSeizures()
	{
		HideTooltip();
	}

	public void OnHoverOutInsomnia()
	{
		HideTooltip();
	}

	public void OnHoverOutPoisonCloud()
	{
		HideTooltip();
	}

	public void OnHoverOutMonsterScent()
	{
		HideTooltip();
	}

	public void OnHoverOutSneezing()
	{
		HideTooltip();
	}

	public void OnHoverOutDepression()
	{
		HideTooltip();
	}

	public void OnHoverOutHungerPangs()
	{
		HideTooltip();
	}

	private void PayForSymptom(PLAGUE_SYMPTOM p_symptomType)
	{
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null)
		{
			PlayerManager.Instance.player.currenciesComponent.AdjustChaoticEnergy(-p_symptomType.GetSymptomCost());
		}
	}

	private bool CanAffordSymptom(PLAGUE_SYMPTOM p_symptomType)
	{
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null)
		{
			if (PlayerManager.Instance.player.currenciesComponent.chaoticEnergy < p_symptomType.GetSymptomCost())
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

	private void ShowTooltip(PLAGUE_SYMPTOM p_symptomType, UIHoverPosition p_hoverPosition)
	{
		if (!(UIManager.Instance != null))
		{
			return;
		}
		string key = p_symptomType.ToStringEnumWithSpace();
		string text = p_symptomType.GetSymptomTooltip();
		if (!PlagueDisease.Instance.IsSymptomActive(p_symptomType) && PlagueDisease.Instance.HasMaxActiveSymptoms())
		{
			if (_invalidTextMaxActiveSymptom == null)
			{
				Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
				dictionary.Add("amount", PlagueDisease.Instance.activeMaxSymptoms.ToString());
				dictionary.Add("plagueCategory", LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Symptoms"));
				_invalidTextMaxActiveSymptom = Utilities.ColorizeInvalidText(LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Max_Choice_Tooltip", dictionary));
				MaccimaDictionaryPool<string, string>.Release(dictionary);
			}
			text = text + "\n" + _invalidTextMaxActiveSymptom;
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
}

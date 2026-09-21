using Ruinarch.MVCFramework;
using UnityEngine;
using UtilityScripts;

public class LifeSpanUIController : MVCUIController, LifeSpanUIView.IListener
{
	[SerializeField]
	private LifeSpanUIModel m_lifeSpanUIModel;

	private LifeSpanUIView m_lifeSpanUIView;

	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		LifeSpanUIView.Create(_canvas, m_lifeSpanUIModel, delegate(LifeSpanUIView p_ui)
		{
			m_lifeSpanUIView = p_ui;
			m_lifeSpanUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
		});
	}

	private void OnDestroy()
	{
		m_lifeSpanUIView?.Unsubscribe(this);
	}

	public override void ShowUI()
	{
		base.ShowUI();
		UpdateAllLifespanData();
	}

	private void UpdateAllLifespanData()
	{
		UpdateTileObjectInfectionTimeData();
		UpdateElvesInfectionTimeData();
		UpdateHumansInfectionTimeData();
		UpdateMonstersInfectionTimeData();
		UpdateUndeadInfectionTimeData();
	}

	public void OnObjectsUpgradeClicked()
	{
		int tileObjectInfectionTimeUpgradeCost = PlagueDisease.Instance.lifespan.GetTileObjectInfectionTimeUpgradeCost();
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null)
		{
			PlayerManager.Instance.player.currenciesComponent.AdjustChaoticEnergy(-tileObjectInfectionTimeUpgradeCost);
		}
		PlagueDisease.Instance.lifespan.UpgradeTileObjectInfectionTime();
		UpdateAllLifespanData();
		switch (PlagueDisease.Instance.lifespan.GetTileObjectLifespanCurrentLevel())
		{
		case 2:
			AudioManager.Instance.TryPlayUISFX("Play_Upgrade_Power_1");
			break;
		case 3:
			AudioManager.Instance.TryPlayUISFX("Play_Upgrade_Power_2");
			break;
		case 4:
			AudioManager.Instance.TryPlayUISFX("Play_Upgrade_Power_3");
			break;
		}
	}

	public void OnElvesUpgradeClicked()
	{
		int sapientInfectionTimeUpgradeCost = PlagueDisease.Instance.lifespan.GetSapientInfectionTimeUpgradeCost(RACE.ELVES);
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null)
		{
			PlayerManager.Instance.player.currenciesComponent.AdjustChaoticEnergy(-sapientInfectionTimeUpgradeCost);
		}
		PlagueDisease.Instance.lifespan.UpgradeSapientInfectionTime(RACE.ELVES);
		UpdateAllLifespanData();
		switch (PlagueDisease.Instance.lifespan.GetSapientLifespanCurrentLevel(RACE.ELVES))
		{
		case 2:
			AudioManager.Instance.TryPlayUISFX("Play_Upgrade_Power_1");
			break;
		case 3:
			AudioManager.Instance.TryPlayUISFX("Play_Upgrade_Power_2");
			break;
		case 4:
			AudioManager.Instance.TryPlayUISFX("Play_Upgrade_Power_3");
			break;
		}
	}

	public void OnHumansUpgradeClicked()
	{
		int sapientInfectionTimeUpgradeCost = PlagueDisease.Instance.lifespan.GetSapientInfectionTimeUpgradeCost(RACE.HUMANS);
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null)
		{
			PlayerManager.Instance.player.currenciesComponent.AdjustChaoticEnergy(-sapientInfectionTimeUpgradeCost);
		}
		PlagueDisease.Instance.lifespan.UpgradeSapientInfectionTime(RACE.HUMANS);
		UpdateAllLifespanData();
		switch (PlagueDisease.Instance.lifespan.GetSapientLifespanCurrentLevel(RACE.HUMANS))
		{
		case 2:
			AudioManager.Instance.TryPlayUISFX("Play_Upgrade_Power_1");
			break;
		case 3:
			AudioManager.Instance.TryPlayUISFX("Play_Upgrade_Power_2");
			break;
		case 4:
			AudioManager.Instance.TryPlayUISFX("Play_Upgrade_Power_3");
			break;
		}
	}

	public void OnMonstersUpgradeClicked()
	{
		int monsterInfectionTimeUpgradeCost = PlagueDisease.Instance.lifespan.GetMonsterInfectionTimeUpgradeCost();
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null)
		{
			PlayerManager.Instance.player.currenciesComponent.AdjustChaoticEnergy(-monsterInfectionTimeUpgradeCost);
		}
		PlagueDisease.Instance.lifespan.UpgradeMonsterInfectionTime();
		UpdateAllLifespanData();
		switch (PlagueDisease.Instance.lifespan.GetMonstersLifespanCurrentLevel())
		{
		case 2:
			AudioManager.Instance.TryPlayUISFX("Play_Upgrade_Power_1");
			break;
		case 3:
			AudioManager.Instance.TryPlayUISFX("Play_Upgrade_Power_2");
			break;
		case 4:
			AudioManager.Instance.TryPlayUISFX("Play_Upgrade_Power_3");
			break;
		}
	}

	public void OnUndeadUpgradeClicked()
	{
		int undeadInfectionTimeUpgradeCost = PlagueDisease.Instance.lifespan.GetUndeadInfectionTimeUpgradeCost();
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null)
		{
			PlayerManager.Instance.player.currenciesComponent.AdjustChaoticEnergy(-undeadInfectionTimeUpgradeCost);
		}
		PlagueDisease.Instance.lifespan.UpgradeUndeadInfectionTime();
		UpdateAllLifespanData();
		switch (PlagueDisease.Instance.lifespan.GetUndeadLifespanCurrentLevel())
		{
		case 2:
			AudioManager.Instance.TryPlayUISFX("Play_Upgrade_Power_1");
			break;
		case 3:
			AudioManager.Instance.TryPlayUISFX("Play_Upgrade_Power_2");
			break;
		case 4:
			AudioManager.Instance.TryPlayUISFX("Play_Upgrade_Power_3");
			break;
		}
	}

	public void OnObjectsHoveredOver(UIHoverPosition hoverPosition)
	{
		ShowTooltip(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Objects"), LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Objects_Lifespan_Tooltip"), hoverPosition);
	}

	public void OnElvesHoveredOver(UIHoverPosition hoverPosition)
	{
		ShowTooltip(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Elves"), LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Elves_Lifespan_Tooltip"), hoverPosition);
	}

	public void OnHumansHoveredOver(UIHoverPosition hoverPosition)
	{
		ShowTooltip(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Humans"), LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Humans_Lifespan_Tooltip"), hoverPosition);
	}

	public void OnMonstersHoveredOver(UIHoverPosition hoverPosition)
	{
		ShowTooltip(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Monsters"), LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Monsters_Lifespan_Tooltip"), hoverPosition);
	}

	public void OnUndeadHoveredOver(UIHoverPosition hoverPosition)
	{
		ShowTooltip(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Undead"), LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Undead_Lifespan_Tooltip"), hoverPosition);
	}

	public void OnObjectsHoveredOut()
	{
		HideTooltip();
	}

	public void OnElvesHoveredOut()
	{
		HideTooltip();
	}

	public void OnHumansHoveredOut()
	{
		HideTooltip();
	}

	public void OnMonstersHoveredOut()
	{
		HideTooltip();
	}

	public void OnUndeadHoveredOut()
	{
		HideTooltip();
	}

	public void OnUpgradeBtnObjectsHoveredOver()
	{
		m_lifeSpanUIView.UpdateTileObjectInfectionTime("<color=\"green\">" + PlagueDisease.Instance.lifespan.GetInfectionTimeString(PlagueDisease.Instance.lifespan.GetUpgradedTileObjectInfectionTime()) + "</color>");
	}

	public void OnUpgradeBtnObjectsHoveredOut()
	{
		m_lifeSpanUIView.UpdateTileObjectInfectionTime(PlagueDisease.Instance.lifespan.GetInfectionTimeString(PlagueDisease.Instance.lifespan.tileObjectInfectionTimeInHours));
	}

	public void OnUpgradeBtnElvesHoveredOver()
	{
		m_lifeSpanUIView.UpdateElvesInfectionTime("<color=\"green\">" + PlagueDisease.Instance.lifespan.GetInfectionTimeString(PlagueDisease.Instance.lifespan.GetUpgradedSapientInfectionTime(RACE.ELVES)) + "</color>");
	}

	public void OnUpgradeBtnElvesHoveredOut()
	{
		m_lifeSpanUIView.UpdateElvesInfectionTime(PlagueDisease.Instance.lifespan.GetInfectionTimeString(PlagueDisease.Instance.lifespan.GetSapientLifespanOfPlagueInHours(RACE.ELVES)));
	}

	public void OnUpgradeBtnHumansHoveredOver()
	{
		m_lifeSpanUIView.UpdateHumansInfectionTime("<color=\"green\">" + PlagueDisease.Instance.lifespan.GetInfectionTimeString(PlagueDisease.Instance.lifespan.GetUpgradedSapientInfectionTime(RACE.HUMANS)) + "</color>");
	}

	public void OnUpgradeBtnHumansHoveredOut()
	{
		m_lifeSpanUIView.UpdateHumansInfectionTime(PlagueDisease.Instance.lifespan.GetInfectionTimeString(PlagueDisease.Instance.lifespan.GetSapientLifespanOfPlagueInHours(RACE.HUMANS)));
	}

	public void OnUpgradeBtnMonstersHoveredOver()
	{
		m_lifeSpanUIView.UpdateMonstersInfectionTime("<color=\"green\">" + PlagueDisease.Instance.lifespan.GetInfectionTimeString(PlagueDisease.Instance.lifespan.GetUpgradedMonsterInfectionTime()) + "</color>");
	}

	public void OnUpgradeBtnMonstersHoveredOut()
	{
		m_lifeSpanUIView.UpdateMonstersInfectionTime(PlagueDisease.Instance.lifespan.GetInfectionTimeString(PlagueDisease.Instance.lifespan.monsterInfectionTimeInHours));
	}

	public void OnUpgradeBtnUndeadHoveredOver()
	{
		m_lifeSpanUIView.UpdateUndeadInfectionTime("<color=\"green\">" + PlagueDisease.Instance.lifespan.GetInfectionTimeString(PlagueDisease.Instance.lifespan.GetUpgradedUndeadInfectionTime()) + "</color>");
	}

	public void OnUpgradeBtnUndeadHoveredOut()
	{
		m_lifeSpanUIView.UpdateUndeadInfectionTime(PlagueDisease.Instance.lifespan.GetInfectionTimeString(PlagueDisease.Instance.lifespan.undeadInfectionTimeInHours));
	}

	private void ShowTooltip(string p_lifespanHeader, string p_lifespanDescription, UIHoverPosition p_hoverPosition)
	{
		if (UIManager.Instance != null)
		{
			UIManager.Instance.ShowSmallInfo(p_lifespanDescription, p_hoverPosition, p_lifespanHeader);
		}
	}

	private void HideTooltip()
	{
		if (UIManager.Instance != null)
		{
			UIManager.Instance.HideSmallInfo();
		}
	}

	private bool CanAffordUpgrade(int cost)
	{
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null)
		{
			if (PlayerManager.Instance.player.currenciesComponent.chaoticEnergy < cost)
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

	private void UpdateTileObjectInfectionTimeData()
	{
		m_lifeSpanUIView.UpdateTileObjectInfectionTime(PlagueDisease.Instance.lifespan.GetInfectionTimeString(PlagueDisease.Instance.lifespan.tileObjectInfectionTimeInHours));
		int tileObjectInfectionTimeUpgradeCost = PlagueDisease.Instance.lifespan.GetTileObjectInfectionTimeUpgradeCost();
		bool flag = PlagueDisease.Instance.lifespan.IsTileObjectAtMaxLevel();
		m_lifeSpanUIView.UpdateTileObjectUpgradePrice(flag ? LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "MAX") : (tileObjectInfectionTimeUpgradeCost + Utilities.ChaoticEnergyIcon()));
		m_lifeSpanUIView.UpdateTileObjectUpgradeButtonInteractable(!flag && CanAffordUpgrade(tileObjectInfectionTimeUpgradeCost));
	}

	private void UpdateElvesInfectionTimeData()
	{
		m_lifeSpanUIView.UpdateElvesInfectionTime(PlagueDisease.Instance.lifespan.GetInfectionTimeString(PlagueDisease.Instance.lifespan.GetSapientLifespanOfPlagueInHours(RACE.ELVES)));
		int sapientInfectionTimeUpgradeCost = PlagueDisease.Instance.lifespan.GetSapientInfectionTimeUpgradeCost(RACE.ELVES);
		bool flag = PlagueDisease.Instance.lifespan.IsSapientAtMaxLevel(RACE.ELVES);
		m_lifeSpanUIView.UpdateElvesUpgradePrice(flag ? LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "MAX") : (sapientInfectionTimeUpgradeCost + Utilities.ChaoticEnergyIcon()));
		m_lifeSpanUIView.UpdateElvesUpgradeButtonInteractable(!flag && CanAffordUpgrade(sapientInfectionTimeUpgradeCost));
	}

	private void UpdateHumansInfectionTimeData()
	{
		m_lifeSpanUIView.UpdateHumansInfectionTime(PlagueDisease.Instance.lifespan.GetInfectionTimeString(PlagueDisease.Instance.lifespan.GetSapientLifespanOfPlagueInHours(RACE.HUMANS)));
		int sapientInfectionTimeUpgradeCost = PlagueDisease.Instance.lifespan.GetSapientInfectionTimeUpgradeCost(RACE.HUMANS);
		bool flag = PlagueDisease.Instance.lifespan.IsSapientAtMaxLevel(RACE.HUMANS);
		m_lifeSpanUIView.UpdateHumansUpgradePrice(flag ? LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "MAX") : (sapientInfectionTimeUpgradeCost + Utilities.ChaoticEnergyIcon()));
		m_lifeSpanUIView.UpdateHumansUpgradeButtonInteractable(!flag && CanAffordUpgrade(sapientInfectionTimeUpgradeCost));
	}

	private void UpdateMonstersInfectionTimeData()
	{
		m_lifeSpanUIView.UpdateMonstersInfectionTime(PlagueDisease.Instance.lifespan.GetInfectionTimeString(PlagueDisease.Instance.lifespan.monsterInfectionTimeInHours));
		int monsterInfectionTimeUpgradeCost = PlagueDisease.Instance.lifespan.GetMonsterInfectionTimeUpgradeCost();
		bool flag = PlagueDisease.Instance.lifespan.IsMonstersAtMaxLevel();
		m_lifeSpanUIView.UpdateMonstersUpgradePrice(flag ? LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "MAX") : (monsterInfectionTimeUpgradeCost + Utilities.ChaoticEnergyIcon()));
		m_lifeSpanUIView.UpdateMonstersUpgradeButtonInteractable(!flag && CanAffordUpgrade(monsterInfectionTimeUpgradeCost));
	}

	private void UpdateUndeadInfectionTimeData()
	{
		m_lifeSpanUIView.UpdateUndeadInfectionTime(PlagueDisease.Instance.lifespan.GetInfectionTimeString(PlagueDisease.Instance.lifespan.undeadInfectionTimeInHours));
		int undeadInfectionTimeUpgradeCost = PlagueDisease.Instance.lifespan.GetUndeadInfectionTimeUpgradeCost();
		bool flag = PlagueDisease.Instance.lifespan.IsUndeadAtMaxLevel();
		m_lifeSpanUIView.UpdateUndeadUpgradePrice(flag ? LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "MAX") : (undeadInfectionTimeUpgradeCost + Utilities.ChaoticEnergyIcon()));
		m_lifeSpanUIView.UpdateUndeadUpgradeButtonInteractable(!flag && CanAffordUpgrade(undeadInfectionTimeUpgradeCost));
	}
}

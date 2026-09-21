using System.Collections.Generic;
using Maccima_Games.Util;
using Ruinarch.MVCFramework;
using UnityEngine;

public class PrimordialPoolStatsUpgradeUIController : MVCUIController, PrimordialPoolStatsUpgradeUIView.IListener
{
	[SerializeField]
	private PrimordialPoolStatsUpgradeUIModel m_primordialPoolStatusUpgradeUIModel;

	private PrimordialPoolStatsUpgradeUIView m_primordialPoolStatusUpgradeUIView;

	private CHARACTER_CATEGORY m_activeTab;

	private string _localizedCharacterCategory;

	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		PrimordialPoolStatsUpgradeUIView.Create(_canvas, m_primordialPoolStatusUpgradeUIModel, delegate(PrimordialPoolStatsUpgradeUIView p_ui)
		{
			m_primordialPoolStatusUpgradeUIView = p_ui;
			m_primordialPoolStatusUpgradeUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
		});
	}

	private void OnDestroy()
	{
		m_primordialPoolStatusUpgradeUIView?.Unsubscribe(this);
	}

	private void OnBonusUpgradeClicked(PRIMORDIAL_STATS_BONUS p_bonus)
	{
		PrimordialStatsData primordialStatsData = PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[m_activeTab];
		int num = (int)primordialStatsData.GetCurrentUpgradeCost(p_bonus);
		if (PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[m_activeTab].CheckIfUpgradeable(PlayerManager.Instance.player.currenciesComponent.chaoticEnergy, p_bonus))
		{
			PlayerManager.Instance.player.currenciesComponent.AdjustChaoticEnergy(-num);
			primordialStatsData.LevelUp(p_bonus);
			m_primordialPoolStatusUpgradeUIView.DisplayCategoryInfo(m_activeTab);
			Messenger.Broadcast(PlayerSkillSignals.ON_PRIMORDIAL_POOL_UPGRADE, m_activeTab, p_bonus);
		}
	}

	public void OnStrengthUpgradeClicked()
	{
		OnBonusUpgradeClicked(PRIMORDIAL_STATS_BONUS.Str);
	}

	public void OnIntelligenceUpgradeClicked()
	{
		OnBonusUpgradeClicked(PRIMORDIAL_STATS_BONUS.Int);
	}

	public void OnPiercingUpgradeClicked()
	{
		OnBonusUpgradeClicked(PRIMORDIAL_STATS_BONUS.Piercing);
	}

	public void OnMentalResistanceUpgradeClicked()
	{
		OnBonusUpgradeClicked(PRIMORDIAL_STATS_BONUS.Mental_Res);
	}

	public void OnPhysicalResistanceUpgradeClicked()
	{
		OnBonusUpgradeClicked(PRIMORDIAL_STATS_BONUS.Physical_Res);
	}

	public void OnElementalResistanceUpgradeClicked()
	{
		OnBonusUpgradeClicked(PRIMORDIAL_STATS_BONUS.Elemental_Res);
	}

	public void OnSecondaryResistanceUpgradeClicked()
	{
		OnBonusUpgradeClicked(PRIMORDIAL_STATS_BONUS.Secondary_Res);
	}

	public void OnStrengthHoveredOver(UIHoverPosition p_hoverPosition)
	{
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
		dictionary.Add("type", _localizedCharacterCategory);
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Primordial_Pool_Strength", dictionary);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
		ShowTooltip(localizedValue, p_hoverPosition);
	}

	public void OnIntelligenceHoveredOver(UIHoverPosition p_hoverPosition)
	{
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
		dictionary.Add("type", _localizedCharacterCategory);
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Primordial_Pool_Intelligence", dictionary);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
		ShowTooltip(localizedValue, p_hoverPosition);
	}

	public void OnPiercingHoveredOver(UIHoverPosition p_hoverPosition)
	{
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
		dictionary.Add("type", _localizedCharacterCategory);
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Primordial_Pool_Piercing", dictionary);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
		ShowTooltip(localizedValue, p_hoverPosition);
	}

	public void OnMentalResistanceHoveredOver(UIHoverPosition p_hoverPosition)
	{
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
		dictionary.Add("type", _localizedCharacterCategory);
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Primordial_Pool_Mental", dictionary);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
		ShowTooltip(localizedValue, p_hoverPosition);
	}

	public void OnPhysicalResistanceHoveredOver(UIHoverPosition p_hoverPosition)
	{
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
		dictionary.Add("type", _localizedCharacterCategory);
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Primordial_Pool_Physical", dictionary);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
		ShowTooltip(localizedValue, p_hoverPosition);
	}

	public void OnElementalResistanceHoveredOver(UIHoverPosition p_hoverPosition)
	{
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
		dictionary.Add("type", _localizedCharacterCategory);
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Primordial_Pool_Elemental", dictionary);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
		ShowTooltip(localizedValue, p_hoverPosition);
	}

	public void OnSecondaryResistanceHoveredOver(UIHoverPosition p_hoverPosition)
	{
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
		dictionary.Add("type", _localizedCharacterCategory);
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Primordial_Pool_Secondary", dictionary);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
		ShowTooltip(localizedValue, p_hoverPosition);
	}

	public void OnStrengthHoveredOut()
	{
		HideTooltip();
	}

	public void OnIntelligenceHoveredOut()
	{
		HideTooltip();
	}

	public void OnPiercingHoveredOut()
	{
		HideTooltip();
	}

	public void OnMentalResistanceHoveredOut()
	{
		HideTooltip();
	}

	public void OnPhysicalResistanceHoveredOut()
	{
		HideTooltip();
	}

	public void OnElementalResistanceHoveredOut()
	{
		HideTooltip();
	}

	public void OnSecondaryResistanceHoveredOut()
	{
		HideTooltip();
	}

	private void OnButtonUpgradeHoveredOver(int level, PRIMORDIAL_STATS_BONUS p_bonus, RuinarchText p_textUI)
	{
		if (level < 5)
		{
			string empty = string.Empty;
			empty = (((uint)p_bonus > 1u) ? ("<color=\"green\">+" + PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[m_activeTab].GetNextBonusForTextDisplay(p_bonus) + "</color>") : ("<color=\"green\">" + PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[m_activeTab].GetNextBonusForTextDisplay(p_bonus) + "%</color>"));
			m_primordialPoolStatusUpgradeUIView.ShowNextUpgradeValue(p_textUI, empty);
		}
		else
		{
			SetCurrentDisplayText();
		}
	}

	public void OnBtnUpgradeStrengthHoveredOver()
	{
		int lvlStr = PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[m_activeTab].lvlStr;
		OnButtonUpgradeHoveredOver(lvlStr, PRIMORDIAL_STATS_BONUS.Str, m_primordialPoolStatusUpgradeUIView.UIModel.txtStrengthRate);
	}

	public void OnBtnUpgradeStrengthHoveredOut()
	{
		SetCurrentDisplayText();
	}

	public void OnBtnUpgradeIntelligenceHoveredOver()
	{
		int lvlInt = PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[m_activeTab].lvlInt;
		OnButtonUpgradeHoveredOver(lvlInt, PRIMORDIAL_STATS_BONUS.Int, m_primordialPoolStatusUpgradeUIView.UIModel.txtIntelligenceRate);
	}

	public void OnBtnUpgradeIntelligenceHoveredOut()
	{
		SetCurrentDisplayText();
	}

	public void OnBtnUpgradePiercingHoveredOver()
	{
		int lvlPiercing = PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[m_activeTab].lvlPiercing;
		OnButtonUpgradeHoveredOver(lvlPiercing, PRIMORDIAL_STATS_BONUS.Piercing, m_primordialPoolStatusUpgradeUIView.UIModel.txtPiercingRate);
	}

	public void OnBtnUpgradePiercingHoveredOut()
	{
		SetCurrentDisplayText();
	}

	public void OnBtnUpgradeMentalResistanceHoveredOver()
	{
		int lvlMentalResistance = PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[m_activeTab].lvlMentalResistance;
		OnButtonUpgradeHoveredOver(lvlMentalResistance, PRIMORDIAL_STATS_BONUS.Mental_Res, m_primordialPoolStatusUpgradeUIView.UIModel.txtMentalResistanceRate);
	}

	public void OnBtnUpgradeMentalResistanceHoveredOut()
	{
		SetCurrentDisplayText();
	}

	public void OnBtnUpgradePhysicalResistanceHoveredOver()
	{
		int lvlPhysicalResistance = PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[m_activeTab].lvlPhysicalResistance;
		OnButtonUpgradeHoveredOver(lvlPhysicalResistance, PRIMORDIAL_STATS_BONUS.Physical_Res, m_primordialPoolStatusUpgradeUIView.UIModel.txtPhysicalResistanceRate);
	}

	public void OnBtnUpgradePhysicalResistanceHoveredOut()
	{
		SetCurrentDisplayText();
	}

	public void OnBtnUpgradeElementalResistanceHoveredOver()
	{
		int lvlElementalResistance = PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[m_activeTab].lvlElementalResistance;
		OnButtonUpgradeHoveredOver(lvlElementalResistance, PRIMORDIAL_STATS_BONUS.Elemental_Res, m_primordialPoolStatusUpgradeUIView.UIModel.txtElementalResistanceRate);
	}

	public void OnBtnUpgradeElementalResistanceHoveredOut()
	{
		SetCurrentDisplayText();
	}

	public void OnBtnUpgradeSecondaryResistanceHoveredOver()
	{
		int lvlSecondaryResistance = PlayerManager.Instance.player.primordialPoolDataHandler.bonusPerCategory[m_activeTab].lvlSecondaryResistance;
		OnButtonUpgradeHoveredOver(lvlSecondaryResistance, PRIMORDIAL_STATS_BONUS.Secondary_Res, m_primordialPoolStatusUpgradeUIView.UIModel.txtSecondaryResistanceRate);
	}

	public void OnBtnUpgradeSecondaryResistanceHoveredOut()
	{
		SetCurrentDisplayText();
	}

	private void ShowTooltip(string p_message, UIHoverPosition p_hoverPosition)
	{
		if (UIManager.Instance != null)
		{
			UIManager.Instance.ShowSmallInfo(p_message, p_hoverPosition);
		}
	}

	private void HideTooltip()
	{
		if (UIManager.Instance != null)
		{
			UIManager.Instance.HideSmallInfo();
			SetCurrentDisplayText();
		}
	}

	public void SetCurrentDisplayText()
	{
		if (GameManager.Instance.gameHasStarted)
		{
			SetDisplayTexts(m_activeTab);
		}
	}

	public void SetDisplayTexts(CHARACTER_CATEGORY p_displayCategory)
	{
		m_activeTab = p_displayCategory;
		_localizedCharacterCategory = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", m_activeTab.ToStringEnum());
		m_primordialPoolStatusUpgradeUIView.DisplayCategoryInfo(p_displayCategory);
	}
}

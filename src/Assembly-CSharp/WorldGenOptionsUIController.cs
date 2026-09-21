using System;
using Ruinarch.MVCFramework;
using UnityEngine;
using UnityEngine.Localization;

public class WorldGenOptionsUIController : MVCUIController, WorldGenOptionsUIView.IListener
{
	[SerializeField]
	private WorldGenOptionsUIModel m_worldGenOptionsUIModel;

	private WorldGenOptionsUIView m_worldGenOptionsUIView;

	public FactionSettingVillageEditorUIController factionSettingVillageEditorUIController;

	private Action onUpdateVillageCountAction;

	private string _victoryConditionTooltip;

	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		WorldGenOptionsUIView.Create(_canvas, m_worldGenOptionsUIModel, delegate(WorldGenOptionsUIView p_ui)
		{
			m_worldGenOptionsUIView = p_ui;
			m_worldGenOptionsUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
			FactionSettingVillageEditorUIController obj = factionSettingVillageEditorUIController;
			obj.onUIINstantiated = (Action)Delegate.Combine(obj.onUIINstantiated, new Action(OnFactionSettingVillageEditorInstantiated));
			factionSettingVillageEditorUIController.InstantiateUI();
		});
	}

	private void OnFactionSettingVillageEditorInstantiated()
	{
		factionSettingVillageEditorUIController.SetParent(m_worldGenOptionsUIView.UIModel.parentDisplay);
		factionSettingVillageEditorUIController.HideUI();
		factionSettingVillageEditorUIController.SetOnHideAction(OnDoneEditingVillages);
	}

	private void OnEnable()
	{
		FactionSettingUIItem.onClickMinus = (Action<FactionTemplate, FactionSettingUIItem>)Delegate.Combine(FactionSettingUIItem.onClickMinus, new Action<FactionTemplate, FactionSettingUIItem>(OnDeleteFactionSetting));
		FactionSettingUIItem.onChangeName = (Action<FactionTemplate, string>)Delegate.Combine(FactionSettingUIItem.onChangeName, new Action<FactionTemplate, string>(OnChangeFactionSettingName));
		FactionSettingUIItem.onClickRandomizeName = (Action<FactionTemplate, FactionSettingUIItem>)Delegate.Combine(FactionSettingUIItem.onClickRandomizeName, new Action<FactionTemplate, FactionSettingUIItem>(OnClickRandomizeFactionName));
		FactionSettingUIItem.onChangeFactionType = (Action<FactionTemplate, int>)Delegate.Combine(FactionSettingUIItem.onChangeFactionType, new Action<FactionTemplate, int>(OnChangeFactionSettingFactionType));
		FactionSettingUIItem.onClickEditVillages = (Action<FactionTemplate>)Delegate.Combine(FactionSettingUIItem.onClickEditVillages, new Action<FactionTemplate>(OnClickEditFactionVillages));
		FactionSettingUIItem.onHoverOverEditVillages = (Action<FactionTemplate>)Delegate.Combine(FactionSettingUIItem.onHoverOverEditVillages, new Action<FactionTemplate>(OnHoverOverEditFactionVillages));
		FactionSettingUIItem.onHoverOutEditVillages = (Action<FactionTemplate>)Delegate.Combine(FactionSettingUIItem.onHoverOutEditVillages, new Action<FactionTemplate>(OnHoverOutEditFactionVillages));
	}

	private void OnDisable()
	{
		FactionSettingUIItem.onClickMinus = (Action<FactionTemplate, FactionSettingUIItem>)Delegate.Remove(FactionSettingUIItem.onClickMinus, new Action<FactionTemplate, FactionSettingUIItem>(OnDeleteFactionSetting));
		FactionSettingUIItem.onChangeName = (Action<FactionTemplate, string>)Delegate.Remove(FactionSettingUIItem.onChangeName, new Action<FactionTemplate, string>(OnChangeFactionSettingName));
		FactionSettingUIItem.onClickRandomizeName = (Action<FactionTemplate, FactionSettingUIItem>)Delegate.Remove(FactionSettingUIItem.onClickRandomizeName, new Action<FactionTemplate, FactionSettingUIItem>(OnClickRandomizeFactionName));
		FactionSettingUIItem.onChangeFactionType = (Action<FactionTemplate, int>)Delegate.Remove(FactionSettingUIItem.onChangeFactionType, new Action<FactionTemplate, int>(OnChangeFactionSettingFactionType));
		FactionSettingUIItem.onClickEditVillages = (Action<FactionTemplate>)Delegate.Remove(FactionSettingUIItem.onClickEditVillages, new Action<FactionTemplate>(OnClickEditFactionVillages));
		FactionSettingUIItem.onHoverOverEditVillages = (Action<FactionTemplate>)Delegate.Remove(FactionSettingUIItem.onHoverOverEditVillages, new Action<FactionTemplate>(OnHoverOverEditFactionVillages));
		FactionSettingUIItem.onHoverOutEditVillages = (Action<FactionTemplate>)Delegate.Remove(FactionSettingUIItem.onHoverOutEditVillages, new Action<FactionTemplate>(OnHoverOutEditFactionVillages));
		FactionSettingVillageEditorUIController obj = factionSettingVillageEditorUIController;
		obj.onUIINstantiated = (Action)Delegate.Remove(obj.onUIINstantiated, new Action(OnFactionSettingVillageEditorInstantiated));
	}

	private void OnDestroy()
	{
		m_worldGenOptionsUIView?.Unsubscribe(this);
	}

	public void InitUI(Action p_onUpdateVillageCountAction)
	{
		InstantiateUI();
		onUpdateVillageCountAction = p_onUpdateVillageCountAction;
		m_worldGenOptionsUIView.InitializeMapSizeDropdown();
		m_worldGenOptionsUIView.InitializeFactionItems();
		m_worldGenOptionsUIView.InitializeMigrationDropdown();
		m_worldGenOptionsUIView.InitializeVictoryConditionDropdown();
		m_worldGenOptionsUIView.InitializeCooldownDropdown();
		m_worldGenOptionsUIView.InitializeCostsDropdown();
		m_worldGenOptionsUIView.InitializeChargesDropdown();
		m_worldGenOptionsUIView.InitializeThreatDropdown();
		m_worldGenOptionsUIView.InitializeOmnipotentModeDropdown();
		m_worldGenOptionsUIView.InitializeCorruptionChargesDropdown();
		OnChangeMapSize(MAP_SIZE.Small);
		UpdateVictoryConditionTooltip();
	}

	public void OnLocaleChanged(Locale p_newLang)
	{
		m_worldGenOptionsUIView.InitializeMapSizeDropdown();
		m_worldGenOptionsUIView.InitializeFactionItems();
		m_worldGenOptionsUIView.InitializeMigrationDropdown();
		m_worldGenOptionsUIView.InitializeVictoryConditionDropdown();
		m_worldGenOptionsUIView.InitializeCooldownDropdown();
		m_worldGenOptionsUIView.InitializeCostsDropdown();
		m_worldGenOptionsUIView.InitializeChargesDropdown();
		m_worldGenOptionsUIView.InitializeThreatDropdown();
		m_worldGenOptionsUIView.InitializeOmnipotentModeDropdown();
		m_worldGenOptionsUIView.InitializeCorruptionChargesDropdown();
		OnChangeMapSize(MAP_SIZE.Small);
		UpdateVictoryConditionTooltip();
		factionSettingVillageEditorUIController.OnLocaleChanged(p_newLang);
	}

	public bool IsUIShowing()
	{
		return m_worldGenOptionsUIView.UIModel.parentDisplay.gameObject.activeInHierarchy;
	}

	public override void HideUI()
	{
		base.HideUI();
		factionSettingVillageEditorUIController.HideUI();
	}

	public override void ShowUI()
	{
		base.ShowUI();
		UpdateUIBasedOnCurrentSettings(WorldSettings.Instance.worldSettingsData);
		UpdateOtherSettingsGivenChosenVictoryCondition();
	}

	public void ApplyCurrentSettingsToData()
	{
		WorldSettings.Instance.worldSettingsData.factionSettings.FinalizeFactionTemplates();
	}

	private void UpdateUIBasedOnCurrentSettings(WorldSettingsData p_settings)
	{
		m_worldGenOptionsUIView.SetMapSizeDropdownValue((int)p_settings.mapSettings.mapSize);
		OnChangeMapSize(p_settings.mapSettings.mapSize);
		m_worldGenOptionsUIView.SetMigrationDropdownValue((int)p_settings.villageSettings.migrationSpeed);
		m_worldGenOptionsUIView.SetCooldownDropdownValue((int)p_settings.playerSkillSettings.cooldownSpeed);
		m_worldGenOptionsUIView.SetCostsDropdownValue((int)p_settings.playerSkillSettings.costAmount);
		m_worldGenOptionsUIView.SetChargesDropdownValue((int)p_settings.playerSkillSettings.chargeAmount);
		m_worldGenOptionsUIView.SetThreatDropdownValue((int)p_settings.playerSkillSettings.retaliation);
		m_worldGenOptionsUIView.SetOmnipotentDropdownValue((int)p_settings.playerSkillSettings.omnipotentMode);
		m_worldGenOptionsUIView.SetCorruptionChargesDropdownValue((int)p_settings.playerSkillSettings.corruptionChargeAmount);
	}

	private void OnDeleteFactionSetting(FactionTemplate p_FactionTemplate, FactionSettingUIItem p_uiItem)
	{
		WorldSettings.Instance.worldSettingsData.factionSettings.RemoveFactionSetting(p_FactionTemplate);
		m_worldGenOptionsUIView.HideFactionItem(p_uiItem);
		FactionEmblemRandomizer.SetEmblemAsUnUsed(p_FactionTemplate.factionEmblem);
		UpdateAddFactionBtn();
		UpdateVillageCount();
		UpdateFactionCount();
	}

	private void OnChangeFactionSettingName(FactionTemplate p_FactionTemplate, string p_newName)
	{
		p_FactionTemplate.ChangeName(p_newName);
	}

	private void OnClickRandomizeFactionName(FactionTemplate p_FactionTemplate, FactionSettingUIItem p_uiItem)
	{
		p_FactionTemplate.ChangeName(RandomNameGenerator.GenerateFactionName());
		p_uiItem.UpdateName(p_FactionTemplate.name);
	}

	private void OnChangeFactionSettingFactionType(FactionTemplate p_FactionTemplate, int p_index)
	{
		p_FactionTemplate.ChangeFactionType(p_index);
	}

	private void OnClickEditFactionVillages(FactionTemplate p_FactionTemplate)
	{
		factionSettingVillageEditorUIController.EditVillageSettings(p_FactionTemplate);
	}

	private void OnHoverOverEditFactionVillages(FactionTemplate p_FactionTemplate)
	{
		Tooltip.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Edit Villages"));
	}

	private void OnHoverOutEditFactionVillages(FactionTemplate p_FactionTemplate)
	{
		Tooltip.Instance.HideSmallInfo();
	}

	private void UpdateAddFactionBtn()
	{
		m_worldGenOptionsUIView.SetAddFactionBtnState(!WorldSettings.Instance.worldSettingsData.HasReachedMaxStartingFactionCount());
	}

	private void ResetFactions()
	{
		WorldSettings.Instance.worldSettingsData.factionSettings.ClearFactionSettings();
		m_worldGenOptionsUIView.ResetFactionItems();
		FactionEmblemRandomizer.Reset();
	}

	private void AddDefaultFactionSetting()
	{
		int maxStartingFactions = WorldSettings.Instance.worldSettingsData.mapSettings.GetMaxStartingFactions();
		int p_villageCount = WorldSettings.Instance.worldSettingsData.mapSettings.GetMaxStartingVillages() / maxStartingFactions;
		FactionTemplate factionTemplate = WorldSettings.Instance.worldSettingsData.factionSettings.AddFactionSetting(p_villageCount);
		Sprite unusedFactionEmblem = FactionEmblemRandomizer.GetUnusedFactionEmblem();
		factionTemplate.SetFactionEmblem(unusedFactionEmblem);
		FactionEmblemRandomizer.SetEmblemAsUsed(unusedFactionEmblem);
		m_worldGenOptionsUIView.UpdateFactionItems(WorldSettings.Instance.worldSettingsData.factionSettings.factionTemplates);
		UpdateAddFactionBtn();
		UpdateVillageCount();
		UpdateFactionCount();
	}

	private void UpdateVillageCount()
	{
		int maxStartingVillages = WorldSettings.Instance.worldSettingsData.mapSettings.GetMaxStartingVillages();
		int currentTotalVillageCountBasedOnFactions = WorldSettings.Instance.worldSettingsData.factionSettings.GetCurrentTotalVillageCountBasedOnFactions();
		m_worldGenOptionsUIView.UpdateVillageCount(currentTotalVillageCountBasedOnFactions, maxStartingVillages);
		onUpdateVillageCountAction?.Invoke();
	}

	private void OnDoneEditingVillages()
	{
		UpdateVillageCount();
		m_worldGenOptionsUIView.UpdateFactionItems(WorldSettings.Instance.worldSettingsData.factionSettings.factionTemplates);
	}

	private void UpdateFactionCount()
	{
		int maxStartingFactions = WorldSettings.Instance.worldSettingsData.mapSettings.GetMaxStartingFactions();
		int count = WorldSettings.Instance.worldSettingsData.factionSettings.factionTemplates.Count;
		m_worldGenOptionsUIView.UpdateFactionCount(count, maxStartingFactions);
	}

	public void OnChangeMapSize(MAP_SIZE p_value)
	{
		ResetFactions();
		WorldSettings.Instance.worldSettingsData.mapSettings.SetMapSize(p_value);
		int maxStartingFactions = WorldSettings.Instance.worldSettingsData.mapSettings.GetMaxStartingFactions();
		for (int i = 0; i < maxStartingFactions; i++)
		{
			AddDefaultFactionSetting();
		}
		m_worldGenOptionsUIView.UpdateFactionItems(WorldSettings.Instance.worldSettingsData.factionSettings.factionTemplates);
		UpdateAddFactionBtn();
	}

	public void OnChangeMigrationSpeed(MIGRATION_SPEED p_value)
	{
		WorldSettings.Instance.worldSettingsData.villageSettings.SetMigrationSpeed(p_value);
	}

	public void OnChangeVictoryCondition(VICTORY_CONDITION p_value)
	{
		WorldSettings.Instance.worldSettingsData.SetVictoryCondition(p_value);
		UpdateOtherSettingsGivenChosenVictoryCondition();
	}

	public void OnChangeSkillCooldownSpeed(SKILL_COOLDOWN_SPEED p_value)
	{
		WorldSettings.Instance.worldSettingsData.playerSkillSettings.SetCooldownSpeed(p_value);
	}

	public void OnChangeSkillCostAmount(SKILL_COST_AMOUNT p_value)
	{
		WorldSettings.Instance.worldSettingsData.playerSkillSettings.SetManaCostAmount(p_value);
	}

	public void OnChangeSkillChargeAmount(SKILL_CHARGE_AMOUNT p_value)
	{
		WorldSettings.Instance.worldSettingsData.playerSkillSettings.SetChargeAmount(p_value);
	}

	public void OnChangeThreatAmount(RETALIATION p_value)
	{
		WorldSettings.Instance.worldSettingsData.playerSkillSettings.SetRetaliationState(p_value);
	}

	public void OnChangeOmnipotentMode(OMNIPOTENT_MODE p_value)
	{
		WorldSettings.Instance.worldSettingsData.playerSkillSettings.SetOmnipotentMode(p_value);
	}

	public void OnChangeCorruptionCharges(CORRUPTION_CHARGE_AMOUNT p_value)
	{
		WorldSettings.Instance.worldSettingsData.playerSkillSettings.SetCorruptionChargeAmount(p_value);
	}

	public void OnUpdatePortalLevel(int p_level)
	{
	}

	public void OnClickAddFaction()
	{
		AddDefaultFactionSetting();
	}

	public void OnHoverOverMapSize(UIHoverPosition p_pos)
	{
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Map_Size_Tooltip");
		Tooltip.Instance.ShowSmallInfo(localizedValue, p_pos, LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Map_Size"), autoReplaceText: false);
	}

	public void OnHoverOutMapSize()
	{
		Tooltip.Instance.HideSmallInfo();
	}

	public void OnHoverOverMigration(UIHoverPosition p_pos)
	{
		if (WorldSettings.Instance.worldSettingsData.victoryCondition == VICTORY_CONDITION.Eradication)
		{
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Eradication_Migration_Speed_Tooltip");
			Tooltip.Instance.ShowSmallInfo(localizedValue, p_pos, LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Migration_Speed"), autoReplaceText: false);
		}
		else
		{
			string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Migration_Speed_Tooltip");
			Tooltip.Instance.ShowSmallInfo(localizedValue2, p_pos, LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Migration_Speed"), autoReplaceText: false);
		}
	}

	public void OnHoverOutMigration()
	{
		Tooltip.Instance.HideSmallInfo();
	}

	public void OnHoverOverVictory(UIHoverPosition p_pos)
	{
		Tooltip.Instance.ShowSmallInfo(_victoryConditionTooltip, p_pos, LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Victory_Condition"), autoReplaceText: false);
	}

	private void UpdateVictoryConditionTooltip()
	{
		string text = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Victory_Condition_Tooltip") + "\n\n";
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("Victory_Conditions", "Eradication_Description");
		int length = localizedValue.IndexOf("\n", StringComparison.InvariantCulture);
		localizedValue = localizedValue.Substring(0, length);
		text = text + "<b>" + LocalizationManager.Instance.GetLocalizedValue("Victory_Conditions", "Eradication") + "</b> - " + localizedValue + "\n";
		string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("Victory_Conditions", "Progression_Description");
		length = localizedValue2.IndexOf("\n", StringComparison.InvariantCulture);
		localizedValue2 = localizedValue2.Substring(0, length);
		text = text + "<b>" + LocalizationManager.Instance.GetLocalizedValue("Victory_Conditions", "Progression") + "</b> - " + localizedValue2 + "\n";
		string localizedValue3 = LocalizationManager.Instance.GetLocalizedValue("Victory_Conditions", "Attainment_Description");
		length = localizedValue3.IndexOf("\n", StringComparison.InvariantCulture);
		localizedValue3 = localizedValue3.Substring(0, length);
		text = text + "<b>" + LocalizationManager.Instance.GetLocalizedValue("Victory_Conditions", "Attainment") + "</b> - " + localizedValue3 + "\n";
		_victoryConditionTooltip = text;
	}

	public void OnHoverOutVictory()
	{
		Tooltip.Instance.HideSmallInfo();
	}

	public void OnHoverOverCooldown(UIHoverPosition p_pos)
	{
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Cooldown_Tooltip");
		Tooltip.Instance.ShowSmallInfo(localizedValue, p_pos, LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Cooldown_No_Icon"), autoReplaceText: false);
	}

	public void OnHoverOutCooldown()
	{
		Tooltip.Instance.HideSmallInfo();
	}

	public void OnHoverOverCosts(UIHoverPosition p_pos)
	{
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Costs_Tooltip");
		Tooltip.Instance.ShowSmallInfo(localizedValue, p_pos, LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Costs"), autoReplaceText: false);
	}

	public void OnHoverOutCosts()
	{
		Tooltip.Instance.HideSmallInfo();
	}

	public void OnHoverOverCharges(UIHoverPosition p_pos)
	{
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Charges_Tooltip");
		Tooltip.Instance.ShowSmallInfo(localizedValue, p_pos, LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Charges_No_Icon"), autoReplaceText: false);
	}

	public void OnHoverOutCharges()
	{
		Tooltip.Instance.HideSmallInfo();
	}

	public void OnHoverOverThreat(UIHoverPosition p_pos)
	{
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Retaliation_Tooltip");
		Tooltip.Instance.ShowSmallInfo(localizedValue, p_pos, LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Retaliation"), autoReplaceText: false);
	}

	public void OnHoverOutThreat()
	{
		Tooltip.Instance.HideSmallInfo();
	}

	public void OnHoverOverOmnipotent(UIHoverPosition p_pos)
	{
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Omnipotent_Tooltip");
		Tooltip.Instance.ShowSmallInfo(localizedValue, p_pos, LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Omnipotent"), autoReplaceText: false);
	}

	public void OnHoverOutOmnipotent()
	{
		Tooltip.Instance.HideSmallInfo();
	}

	public void OnHoverOverCorruptionCharges(UIHoverPosition p_pos)
	{
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Corruption_Charges_Tooltip");
		Tooltip.Instance.ShowSmallInfo(localizedValue, p_pos, LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Corruption_Title"), autoReplaceText: false);
	}

	public void OnHoverOutCorruptionCharges()
	{
		Tooltip.Instance.HideSmallInfo();
	}

	private void UpdateOtherSettingsGivenChosenVictoryCondition()
	{
		m_worldGenOptionsUIView.EnableAllGeneralSettings();
		WorldSettings.Instance.worldSettingsData.SetDefaultSettings();
		WorldSettings.Instance.worldSettingsData.SetPresetSettingsForVictoryCondition(WorldSettings.Instance.worldSettingsData.victoryCondition);
		switch (WorldSettings.Instance.worldSettingsData.victoryCondition)
		{
		case VICTORY_CONDITION.Progression:
			m_worldGenOptionsUIView.UIModel.goOmnipotent.SetActive(value: false);
			break;
		case VICTORY_CONDITION.Eradication:
			m_worldGenOptionsUIView.UIModel.goThreat.SetActive(value: false);
			break;
		}
		UpdateUIBasedOnCurrentSettings(WorldSettings.Instance.worldSettingsData);
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("Victory_Conditions", $"{WorldSettings.Instance.worldSettingsData.victoryCondition}_Description");
		m_worldGenOptionsUIView.UIModel.txtVictoryConditionDescription.text = localizedValue;
	}
}

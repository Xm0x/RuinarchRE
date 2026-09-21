using System;
using System.Collections.Generic;
using System.Linq;
using Ruinarch.MVCFramework;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UtilityScripts;

public class WorldGenOptionsUIView : MVCUIView
{
	public interface IListener
	{
		void OnChangeMapSize(MAP_SIZE p_value);

		void OnChangeMigrationSpeed(MIGRATION_SPEED p_value);

		void OnChangeVictoryCondition(VICTORY_CONDITION p_value);

		void OnChangeSkillCooldownSpeed(SKILL_COOLDOWN_SPEED p_value);

		void OnChangeSkillCostAmount(SKILL_COST_AMOUNT p_value);

		void OnChangeSkillChargeAmount(SKILL_CHARGE_AMOUNT p_value);

		void OnChangeThreatAmount(RETALIATION p_value);

		void OnChangeOmnipotentMode(OMNIPOTENT_MODE p_value);

		void OnChangeCorruptionCharges(CORRUPTION_CHARGE_AMOUNT p_value);

		void OnUpdatePortalLevel(int p_level);

		void OnClickAddFaction();

		void OnHoverOverMapSize(UIHoverPosition p_pos);

		void OnHoverOutMapSize();

		void OnHoverOverMigration(UIHoverPosition p_pos);

		void OnHoverOutMigration();

		void OnHoverOverVictory(UIHoverPosition p_pos);

		void OnHoverOutVictory();

		void OnHoverOverCooldown(UIHoverPosition p_pos);

		void OnHoverOutCooldown();

		void OnHoverOverCosts(UIHoverPosition p_pos);

		void OnHoverOutCosts();

		void OnHoverOverCharges(UIHoverPosition p_pos);

		void OnHoverOutCharges();

		void OnHoverOverThreat(UIHoverPosition p_pos);

		void OnHoverOutThreat();

		void OnHoverOverOmnipotent(UIHoverPosition p_pos);

		void OnHoverOutOmnipotent();

		void OnHoverOverCorruptionCharges(UIHoverPosition p_pos);

		void OnHoverOutCorruptionCharges();
	}

	public WorldGenOptionsUIModel UIModel => _baseAssetModel as WorldGenOptionsUIModel;

	public static void Create(Canvas p_canvas, WorldGenOptionsUIModel p_assets, Action<WorldGenOptionsUIView> p_onCreate)
	{
		WorldGenOptionsUIView worldGenOptionsUIView = new GameObject(typeof(WorldGenOptionsUIView).ToString()).AddComponent<WorldGenOptionsUIView>();
		WorldGenOptionsUIModel assets = UnityEngine.Object.Instantiate(p_assets);
		worldGenOptionsUIView.Init(p_canvas, assets);
		p_onCreate?.Invoke(worldGenOptionsUIView);
	}

	public void Subscribe(IListener p_listener)
	{
		WorldGenOptionsUIModel uIModel = UIModel;
		uIModel.onChangeMapSize = (Action<MAP_SIZE>)Delegate.Combine(uIModel.onChangeMapSize, new Action<MAP_SIZE>(p_listener.OnChangeMapSize));
		WorldGenOptionsUIModel uIModel2 = UIModel;
		uIModel2.onChangeMigrationSpeed = (Action<MIGRATION_SPEED>)Delegate.Combine(uIModel2.onChangeMigrationSpeed, new Action<MIGRATION_SPEED>(p_listener.OnChangeMigrationSpeed));
		WorldGenOptionsUIModel uIModel3 = UIModel;
		uIModel3.onChangeVictoryCondition = (Action<VICTORY_CONDITION>)Delegate.Combine(uIModel3.onChangeVictoryCondition, new Action<VICTORY_CONDITION>(p_listener.OnChangeVictoryCondition));
		WorldGenOptionsUIModel uIModel4 = UIModel;
		uIModel4.onChangeSkillCooldownSpeed = (Action<SKILL_COOLDOWN_SPEED>)Delegate.Combine(uIModel4.onChangeSkillCooldownSpeed, new Action<SKILL_COOLDOWN_SPEED>(p_listener.OnChangeSkillCooldownSpeed));
		WorldGenOptionsUIModel uIModel5 = UIModel;
		uIModel5.onChangeSkillCostAmount = (Action<SKILL_COST_AMOUNT>)Delegate.Combine(uIModel5.onChangeSkillCostAmount, new Action<SKILL_COST_AMOUNT>(p_listener.OnChangeSkillCostAmount));
		WorldGenOptionsUIModel uIModel6 = UIModel;
		uIModel6.onChangeSkillChargeAmount = (Action<SKILL_CHARGE_AMOUNT>)Delegate.Combine(uIModel6.onChangeSkillChargeAmount, new Action<SKILL_CHARGE_AMOUNT>(p_listener.OnChangeSkillChargeAmount));
		WorldGenOptionsUIModel uIModel7 = UIModel;
		uIModel7.onChangeThreatAmount = (Action<RETALIATION>)Delegate.Combine(uIModel7.onChangeThreatAmount, new Action<RETALIATION>(p_listener.OnChangeThreatAmount));
		WorldGenOptionsUIModel uIModel8 = UIModel;
		uIModel8.onChangeCorruptionCharges = (Action<CORRUPTION_CHARGE_AMOUNT>)Delegate.Combine(uIModel8.onChangeCorruptionCharges, new Action<CORRUPTION_CHARGE_AMOUNT>(p_listener.OnChangeCorruptionCharges));
		WorldGenOptionsUIModel uIModel9 = UIModel;
		uIModel9.onClickAddFaction = (Action)Delegate.Combine(uIModel9.onClickAddFaction, new Action(p_listener.OnClickAddFaction));
		WorldGenOptionsUIModel uIModel10 = UIModel;
		uIModel10.onHoverOverMapSizeDropdown = (Action<UIHoverPosition>)Delegate.Combine(uIModel10.onHoverOverMapSizeDropdown, new Action<UIHoverPosition>(p_listener.OnHoverOverMapSize));
		WorldGenOptionsUIModel uIModel11 = UIModel;
		uIModel11.onHoverOutMapSizeDropdown = (Action)Delegate.Combine(uIModel11.onHoverOutMapSizeDropdown, new Action(p_listener.OnHoverOutMapSize));
		WorldGenOptionsUIModel uIModel12 = UIModel;
		uIModel12.onHoverOverMigrationDropdown = (Action<UIHoverPosition>)Delegate.Combine(uIModel12.onHoverOverMigrationDropdown, new Action<UIHoverPosition>(p_listener.OnHoverOverMigration));
		WorldGenOptionsUIModel uIModel13 = UIModel;
		uIModel13.onHoverOutMigrationDropdown = (Action)Delegate.Combine(uIModel13.onHoverOutMigrationDropdown, new Action(p_listener.OnHoverOutMigration));
		WorldGenOptionsUIModel uIModel14 = UIModel;
		uIModel14.onHoverOverVictoryDropdown = (Action<UIHoverPosition>)Delegate.Combine(uIModel14.onHoverOverVictoryDropdown, new Action<UIHoverPosition>(p_listener.OnHoverOverVictory));
		WorldGenOptionsUIModel uIModel15 = UIModel;
		uIModel15.onHoverOutVictoryDropdown = (Action)Delegate.Combine(uIModel15.onHoverOutVictoryDropdown, new Action(p_listener.OnHoverOutVictory));
		WorldGenOptionsUIModel uIModel16 = UIModel;
		uIModel16.onHoverOverCooldownDropdown = (Action<UIHoverPosition>)Delegate.Combine(uIModel16.onHoverOverCooldownDropdown, new Action<UIHoverPosition>(p_listener.OnHoverOverCooldown));
		WorldGenOptionsUIModel uIModel17 = UIModel;
		uIModel17.onHoverOutCooldownDropdown = (Action)Delegate.Combine(uIModel17.onHoverOutCooldownDropdown, new Action(p_listener.OnHoverOutCooldown));
		WorldGenOptionsUIModel uIModel18 = UIModel;
		uIModel18.onHoverOverCostsDropdown = (Action<UIHoverPosition>)Delegate.Combine(uIModel18.onHoverOverCostsDropdown, new Action<UIHoverPosition>(p_listener.OnHoverOverCosts));
		WorldGenOptionsUIModel uIModel19 = UIModel;
		uIModel19.onHoverOutCostsDropdown = (Action)Delegate.Combine(uIModel19.onHoverOutCostsDropdown, new Action(p_listener.OnHoverOutCosts));
		WorldGenOptionsUIModel uIModel20 = UIModel;
		uIModel20.onHoverOverChargesDropdown = (Action<UIHoverPosition>)Delegate.Combine(uIModel20.onHoverOverChargesDropdown, new Action<UIHoverPosition>(p_listener.OnHoverOverCharges));
		WorldGenOptionsUIModel uIModel21 = UIModel;
		uIModel21.onHoverOutChargesDropdown = (Action)Delegate.Combine(uIModel21.onHoverOutChargesDropdown, new Action(p_listener.OnHoverOutCharges));
		WorldGenOptionsUIModel uIModel22 = UIModel;
		uIModel22.onHoverOverThreatDropdown = (Action<UIHoverPosition>)Delegate.Combine(uIModel22.onHoverOverThreatDropdown, new Action<UIHoverPosition>(p_listener.OnHoverOverThreat));
		WorldGenOptionsUIModel uIModel23 = UIModel;
		uIModel23.onHoverOutThreatDropdown = (Action)Delegate.Combine(uIModel23.onHoverOutThreatDropdown, new Action(p_listener.OnHoverOutThreat));
		WorldGenOptionsUIModel uIModel24 = UIModel;
		uIModel24.onChangeOmnipotent = (Action<OMNIPOTENT_MODE>)Delegate.Combine(uIModel24.onChangeOmnipotent, new Action<OMNIPOTENT_MODE>(p_listener.OnChangeOmnipotentMode));
		WorldGenOptionsUIModel uIModel25 = UIModel;
		uIModel25.onHoverOverOmnipotentDropdown = (Action<UIHoverPosition>)Delegate.Combine(uIModel25.onHoverOverOmnipotentDropdown, new Action<UIHoverPosition>(p_listener.OnHoverOverOmnipotent));
		WorldGenOptionsUIModel uIModel26 = UIModel;
		uIModel26.onHoverOutOmnipotentDropdown = (Action)Delegate.Combine(uIModel26.onHoverOutOmnipotentDropdown, new Action(p_listener.OnHoverOutOmnipotent));
		WorldGenOptionsUIModel uIModel27 = UIModel;
		uIModel27.onHoverOverCorruptionChargesDropdown = (Action<UIHoverPosition>)Delegate.Combine(uIModel27.onHoverOverCorruptionChargesDropdown, new Action<UIHoverPosition>(p_listener.OnHoverOverCorruptionCharges));
		WorldGenOptionsUIModel uIModel28 = UIModel;
		uIModel28.onHoverOutCorruptionChargesDropdown = (Action)Delegate.Combine(uIModel28.onHoverOutCorruptionChargesDropdown, new Action(p_listener.OnHoverOutCorruptionCharges));
	}

	public void Unsubscribe(IListener p_listener)
	{
		WorldGenOptionsUIModel uIModel = UIModel;
		uIModel.onChangeMapSize = (Action<MAP_SIZE>)Delegate.Remove(uIModel.onChangeMapSize, new Action<MAP_SIZE>(p_listener.OnChangeMapSize));
		WorldGenOptionsUIModel uIModel2 = UIModel;
		uIModel2.onChangeMigrationSpeed = (Action<MIGRATION_SPEED>)Delegate.Remove(uIModel2.onChangeMigrationSpeed, new Action<MIGRATION_SPEED>(p_listener.OnChangeMigrationSpeed));
		WorldGenOptionsUIModel uIModel3 = UIModel;
		uIModel3.onChangeVictoryCondition = (Action<VICTORY_CONDITION>)Delegate.Remove(uIModel3.onChangeVictoryCondition, new Action<VICTORY_CONDITION>(p_listener.OnChangeVictoryCondition));
		WorldGenOptionsUIModel uIModel4 = UIModel;
		uIModel4.onChangeSkillCooldownSpeed = (Action<SKILL_COOLDOWN_SPEED>)Delegate.Remove(uIModel4.onChangeSkillCooldownSpeed, new Action<SKILL_COOLDOWN_SPEED>(p_listener.OnChangeSkillCooldownSpeed));
		WorldGenOptionsUIModel uIModel5 = UIModel;
		uIModel5.onChangeSkillCostAmount = (Action<SKILL_COST_AMOUNT>)Delegate.Remove(uIModel5.onChangeSkillCostAmount, new Action<SKILL_COST_AMOUNT>(p_listener.OnChangeSkillCostAmount));
		WorldGenOptionsUIModel uIModel6 = UIModel;
		uIModel6.onChangeSkillChargeAmount = (Action<SKILL_CHARGE_AMOUNT>)Delegate.Remove(uIModel6.onChangeSkillChargeAmount, new Action<SKILL_CHARGE_AMOUNT>(p_listener.OnChangeSkillChargeAmount));
		WorldGenOptionsUIModel uIModel7 = UIModel;
		uIModel7.onChangeThreatAmount = (Action<RETALIATION>)Delegate.Remove(uIModel7.onChangeThreatAmount, new Action<RETALIATION>(p_listener.OnChangeThreatAmount));
		WorldGenOptionsUIModel uIModel8 = UIModel;
		uIModel8.onChangeCorruptionCharges = (Action<CORRUPTION_CHARGE_AMOUNT>)Delegate.Remove(uIModel8.onChangeCorruptionCharges, new Action<CORRUPTION_CHARGE_AMOUNT>(p_listener.OnChangeCorruptionCharges));
		WorldGenOptionsUIModel uIModel9 = UIModel;
		uIModel9.onClickAddFaction = (Action)Delegate.Remove(uIModel9.onClickAddFaction, new Action(p_listener.OnClickAddFaction));
		WorldGenOptionsUIModel uIModel10 = UIModel;
		uIModel10.onHoverOverMapSizeDropdown = (Action<UIHoverPosition>)Delegate.Remove(uIModel10.onHoverOverMapSizeDropdown, new Action<UIHoverPosition>(p_listener.OnHoverOverMapSize));
		WorldGenOptionsUIModel uIModel11 = UIModel;
		uIModel11.onHoverOutMapSizeDropdown = (Action)Delegate.Remove(uIModel11.onHoverOutMapSizeDropdown, new Action(p_listener.OnHoverOutMapSize));
		WorldGenOptionsUIModel uIModel12 = UIModel;
		uIModel12.onHoverOverMigrationDropdown = (Action<UIHoverPosition>)Delegate.Remove(uIModel12.onHoverOverMigrationDropdown, new Action<UIHoverPosition>(p_listener.OnHoverOverMigration));
		WorldGenOptionsUIModel uIModel13 = UIModel;
		uIModel13.onHoverOutMigrationDropdown = (Action)Delegate.Remove(uIModel13.onHoverOutMigrationDropdown, new Action(p_listener.OnHoverOutMigration));
		WorldGenOptionsUIModel uIModel14 = UIModel;
		uIModel14.onHoverOverVictoryDropdown = (Action<UIHoverPosition>)Delegate.Remove(uIModel14.onHoverOverVictoryDropdown, new Action<UIHoverPosition>(p_listener.OnHoverOverVictory));
		WorldGenOptionsUIModel uIModel15 = UIModel;
		uIModel15.onHoverOutVictoryDropdown = (Action)Delegate.Remove(uIModel15.onHoverOutVictoryDropdown, new Action(p_listener.OnHoverOutVictory));
		WorldGenOptionsUIModel uIModel16 = UIModel;
		uIModel16.onHoverOverCooldownDropdown = (Action<UIHoverPosition>)Delegate.Remove(uIModel16.onHoverOverCooldownDropdown, new Action<UIHoverPosition>(p_listener.OnHoverOverCooldown));
		WorldGenOptionsUIModel uIModel17 = UIModel;
		uIModel17.onHoverOutCooldownDropdown = (Action)Delegate.Remove(uIModel17.onHoverOutCooldownDropdown, new Action(p_listener.OnHoverOutCooldown));
		WorldGenOptionsUIModel uIModel18 = UIModel;
		uIModel18.onHoverOverCostsDropdown = (Action<UIHoverPosition>)Delegate.Remove(uIModel18.onHoverOverCostsDropdown, new Action<UIHoverPosition>(p_listener.OnHoverOverCosts));
		WorldGenOptionsUIModel uIModel19 = UIModel;
		uIModel19.onHoverOutCostsDropdown = (Action)Delegate.Remove(uIModel19.onHoverOutCostsDropdown, new Action(p_listener.OnHoverOutCosts));
		WorldGenOptionsUIModel uIModel20 = UIModel;
		uIModel20.onHoverOverChargesDropdown = (Action<UIHoverPosition>)Delegate.Remove(uIModel20.onHoverOverChargesDropdown, new Action<UIHoverPosition>(p_listener.OnHoverOverCharges));
		WorldGenOptionsUIModel uIModel21 = UIModel;
		uIModel21.onHoverOutChargesDropdown = (Action)Delegate.Remove(uIModel21.onHoverOutChargesDropdown, new Action(p_listener.OnHoverOutCharges));
		WorldGenOptionsUIModel uIModel22 = UIModel;
		uIModel22.onHoverOverThreatDropdown = (Action<UIHoverPosition>)Delegate.Remove(uIModel22.onHoverOverThreatDropdown, new Action<UIHoverPosition>(p_listener.OnHoverOverThreat));
		WorldGenOptionsUIModel uIModel23 = UIModel;
		uIModel23.onHoverOutThreatDropdown = (Action)Delegate.Remove(uIModel23.onHoverOutThreatDropdown, new Action(p_listener.OnHoverOutThreat));
		WorldGenOptionsUIModel uIModel24 = UIModel;
		uIModel24.onChangeOmnipotent = (Action<OMNIPOTENT_MODE>)Delegate.Remove(uIModel24.onChangeOmnipotent, new Action<OMNIPOTENT_MODE>(p_listener.OnChangeOmnipotentMode));
		WorldGenOptionsUIModel uIModel25 = UIModel;
		uIModel25.onHoverOverOmnipotentDropdown = (Action<UIHoverPosition>)Delegate.Remove(uIModel25.onHoverOverOmnipotentDropdown, new Action<UIHoverPosition>(p_listener.OnHoverOverOmnipotent));
		WorldGenOptionsUIModel uIModel26 = UIModel;
		uIModel26.onHoverOutOmnipotentDropdown = (Action)Delegate.Remove(uIModel26.onHoverOutOmnipotentDropdown, new Action(p_listener.OnHoverOutOmnipotent));
		WorldGenOptionsUIModel uIModel27 = UIModel;
		uIModel27.onHoverOverCorruptionChargesDropdown = (Action<UIHoverPosition>)Delegate.Remove(uIModel27.onHoverOverCorruptionChargesDropdown, new Action<UIHoverPosition>(p_listener.OnHoverOverCorruptionCharges));
		WorldGenOptionsUIModel uIModel28 = UIModel;
		uIModel28.onHoverOutCorruptionChargesDropdown = (Action)Delegate.Remove(uIModel28.onHoverOutCorruptionChargesDropdown, new Action(p_listener.OnHoverOutCorruptionCharges));
	}

	public void InitializeFactionItems()
	{
		List<string> list = RuinarchListPool<string>.Claim();
		for (int i = 0; i < GameUtilities.customWorldFactionTypeChoices.Length; i++)
		{
			FACTION_TYPE p_type = GameUtilities.customWorldFactionTypeChoices[i];
			list.Add(LocalizationManager.Instance.GetLocalizedValue("Faction_Table", p_type.ToStringEnum()));
		}
		list.Add(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Random"));
		for (int j = 0; j < UIModel.factionSettingUIItems.Length; j++)
		{
			FactionSettingUIItem obj = UIModel.factionSettingUIItems[j];
			obj.Initialize(list);
			obj.SetMinusBtnState(j != 0);
		}
	}

	public void InitializeMapSizeDropdown()
	{
		UIModel.dropDownMapSize.ClearOptions();
		List<string> list = RuinarchListPool<string>.Claim();
		Utilities.PopulateLocalizedEnumChoices<MAP_SIZE>(list);
		UIModel.dropDownMapSize.AddOptions(list);
		RuinarchListPool<string>.Release(list);
		UIModel.dropDownMapSize.value = 0;
	}

	public void SetMapSizeDropdownValue(int p_value)
	{
		UIModel.dropDownMapSize.value = p_value;
	}

	public void InitializeMigrationDropdown()
	{
		UIModel.dropDownMigration.ClearOptions();
		List<string> list = RuinarchListPool<string>.Claim();
		if (LocalizationSettings.SelectedLocale.LocaleName.Equals("Polish (pl)"))
		{
			Utilities.PolishPopulateLocalizedEnumChoices(list, p_useMaleVersion: false, Array.Empty<MIGRATION_SPEED>());
		}
		else
		{
			Utilities.PopulateLocalizedEnumChoices<MIGRATION_SPEED>(list);
		}
		UIModel.dropDownMigration.AddOptions(list);
		RuinarchListPool<string>.Release(list);
		UIModel.dropDownMigration.value = 2;
	}

	public void SetMigrationDropdownValue(int p_value)
	{
		UIModel.dropDownMigration.value = p_value;
	}

	public void InitializeVictoryConditionDropdown()
	{
		UIModel.dropDownVictory.ClearOptions();
		List<string> list = RuinarchListPool<string>.Claim();
		list.Add(LocalizationManager.Instance.GetLocalizedValue("Victory_Conditions", "Eradication"));
		list.Add(LocalizationManager.Instance.GetLocalizedValue("Victory_Conditions", "Progression"));
		list.Add(LocalizationManager.Instance.GetLocalizedValue("Victory_Conditions", "Attainment"));
		UIModel.dropDownVictory.AddOptions(list);
		RuinarchListPool<string>.Release(list);
		UIModel.dropDownVictory.value = 0;
	}

	public void SetVictoryDropdownValue(int p_value)
	{
		UIModel.dropDownVictory.value = p_value;
	}

	public void InitializeCooldownDropdown()
	{
		UIModel.dropDownCooldown.ClearOptions();
		List<string> list = RuinarchListPool<string>.Claim();
		if (LocalizationSettings.SelectedLocale.LocaleName.Equals("Polish (pl)"))
		{
			Utilities.PolishPopulateLocalizedEnumChoices(list, p_useMaleVersion: false, Array.Empty<SKILL_COOLDOWN_SPEED>());
		}
		else
		{
			Utilities.PopulateLocalizedEnumChoices<SKILL_COOLDOWN_SPEED>(list);
		}
		UIModel.dropDownCooldown.AddOptions(list);
		RuinarchListPool<string>.Release(list);
		UIModel.dropDownCooldown.value = 2;
	}

	public void SetCooldownDropdownValue(int p_value)
	{
		UIModel.dropDownCooldown.value = p_value;
	}

	public void InitializeCostsDropdown()
	{
		UIModel.dropDownCosts.ClearOptions();
		List<string> list = RuinarchListPool<string>.Claim();
		if (LocalizationSettings.SelectedLocale.LocaleName.Equals("Polish (pl)"))
		{
			Utilities.PolishPopulateLocalizedEnumChoices(list, p_useMaleVersion: true, Array.Empty<SKILL_COST_AMOUNT>());
		}
		else
		{
			Utilities.PopulateLocalizedEnumChoices<SKILL_COST_AMOUNT>(list);
		}
		UIModel.dropDownCosts.AddOptions(list);
		RuinarchListPool<string>.Release(list);
		UIModel.dropDownCosts.value = 2;
	}

	public void SetCostsDropdownValue(int p_value)
	{
		UIModel.dropDownCosts.value = p_value;
	}

	public void InitializeChargesDropdown()
	{
		UIModel.dropDownCharges.ClearOptions();
		List<string> list = RuinarchListPool<string>.Claim();
		if (LocalizationSettings.SelectedLocale.LocaleName.Equals("Polish (pl)"))
		{
			Utilities.PolishPopulateLocalizedEnumChoices(list, p_useMaleVersion: false, Array.Empty<SKILL_CHARGE_AMOUNT>());
		}
		else
		{
			Utilities.PopulateLocalizedEnumChoices<SKILL_CHARGE_AMOUNT>(list);
		}
		UIModel.dropDownCharges.AddOptions(list);
		RuinarchListPool<string>.Release(list);
		UIModel.dropDownCharges.value = 2;
	}

	public void SetChargesDropdownValue(int p_value)
	{
		UIModel.dropDownCharges.value = p_value;
	}

	public void InitializeThreatDropdown()
	{
		UIModel.dropDownThreat.ClearOptions();
		List<string> list = RuinarchListPool<string>.Claim();
		if (LocalizationSettings.SelectedLocale.LocaleName.Equals("Polish (pl)"))
		{
			Utilities.PolishPopulateLocalizedEnumChoices(list, p_useMaleVersion: true, Array.Empty<RETALIATION>());
		}
		else
		{
			Utilities.PopulateLocalizedEnumChoices<RETALIATION>(list);
		}
		UIModel.dropDownThreat.AddOptions(list);
		RuinarchListPool<string>.Release(list);
		UIModel.dropDownThreat.value = 0;
	}

	public void SetThreatDropdownValue(int p_value)
	{
		UIModel.dropDownThreat.value = p_value;
	}

	public void InitializeOmnipotentModeDropdown()
	{
		UIModel.dropDownOmnipotent.ClearOptions();
		List<string> list = RuinarchListPool<string>.Claim();
		Utilities.PopulateLocalizedEnumChoices<OMNIPOTENT_MODE>(list);
		UIModel.dropDownOmnipotent.AddOptions(list);
		RuinarchListPool<string>.Release(list);
		UIModel.dropDownOmnipotent.value = 0;
	}

	public void SetOmnipotentDropdownValue(int p_value)
	{
		UIModel.dropDownOmnipotent.value = p_value;
	}

	public void InitializeCorruptionChargesDropdown()
	{
		UIModel.dropDownCorruptionCharges.ClearOptions();
		List<string> list = RuinarchListPool<string>.Claim();
		if (LocalizationSettings.SelectedLocale.LocaleName.Equals("Polish (pl)"))
		{
			Utilities.PolishPopulateLocalizedEnumChoices(list, p_useMaleVersion: true, Array.Empty<CORRUPTION_CHARGE_AMOUNT>());
		}
		else
		{
			Utilities.PopulateLocalizedEnumChoices<CORRUPTION_CHARGE_AMOUNT>(list);
		}
		UIModel.dropDownCorruptionCharges.AddOptions(list);
		RuinarchListPool<string>.Release(list);
		UIModel.dropDownCorruptionCharges.value = 1;
	}

	public void SetCorruptionChargesDropdownValue(int p_value)
	{
		UIModel.dropDownCorruptionCharges.value = p_value;
	}

	public void HideFactionItem(FactionSettingUIItem p_item)
	{
		p_item.gameObject.SetActive(value: false);
	}

	public void ShowFactionItem(FactionSettingUIItem p_item)
	{
		p_item.gameObject.SetActive(value: true);
	}

	public void SetAddFactionBtnState(bool p_state)
	{
		UIModel.btnAddFaction.gameObject.SetActive(p_state);
	}

	public FactionSettingUIItem GetInactiveFactionSettingUIItem()
	{
		for (int i = 0; i < UIModel.factionSettingUIItems.Length; i++)
		{
			FactionSettingUIItem factionSettingUIItem = UIModel.factionSettingUIItems[i];
			if (!factionSettingUIItem.gameObject.activeSelf)
			{
				return factionSettingUIItem;
			}
		}
		return null;
	}

	public void ResetFactionItems()
	{
		for (int i = 0; i < UIModel.factionSettingUIItems.Length; i++)
		{
			FactionSettingUIItem factionSettingUIItem = UIModel.factionSettingUIItems[i];
			HideFactionItem(factionSettingUIItem);
			factionSettingUIItem.Reset();
		}
	}

	public void UpdateFactionItems(List<FactionTemplate> p_factionSettings)
	{
		for (int i = 0; i < UIModel.factionSettingUIItems.Length; i++)
		{
			FactionSettingUIItem factionSettingUIItem = UIModel.factionSettingUIItems[i];
			FactionTemplate factionTemplate = p_factionSettings.ElementAtOrDefault(i);
			factionSettingUIItem.SetMinusBtnState(i != 0);
			if (factionTemplate != null)
			{
				ShowFactionItem(factionSettingUIItem);
				factionSettingUIItem.SetItemDetails(factionTemplate);
			}
			else
			{
				HideFactionItem(factionSettingUIItem);
			}
		}
	}

	public void UpdateVillageCount(int p_count, int p_max)
	{
		if (p_count > p_max)
		{
			UIModel.txtVillages.text = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Starting Villages") + " " + Utilities.ColorizeInvalidText(p_count.ToString()) + "/" + p_max;
		}
		else
		{
			UIModel.txtVillages.text = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Starting Villages") + " " + p_count + "/" + p_max;
		}
	}

	public void UpdateFactionCount(int p_count, int p_max)
	{
		if (p_count > p_max)
		{
			UIModel.txtFactionsCount.text = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Factions") + ": " + Utilities.ColorizeInvalidText(p_count.ToString()) + "/" + p_max;
		}
		else
		{
			UIModel.txtFactionsCount.text = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Factions") + ": " + p_count + "/" + p_max;
		}
	}

	public void EnableAllGeneralSettings()
	{
		UIModel.goMapSize.SetActive(value: true);
		UIModel.goCharges.SetActive(value: true);
		UIModel.goCooldown.SetActive(value: true);
		UIModel.goCorruptionCharges.SetActive(value: true);
		UIModel.goCosts.SetActive(value: true);
		UIModel.goThreat.SetActive(value: true);
		UIModel.goMigration.SetActive(value: true);
		UIModel.goOmnipotent.SetActive(value: true);
	}
}

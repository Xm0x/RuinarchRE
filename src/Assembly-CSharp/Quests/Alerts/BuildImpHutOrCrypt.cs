using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Maccima_Games.Util;
using Ruinarch;
using Ruinarch.Custom_UI;
using Tutorial;
using UnityEngine.Localization;
using UtilityScripts;

namespace Quests.Alerts;

public class BuildImpHutOrCrypt : GameAlert
{
	private STRUCTURE_TYPE _neededStructureType;

	private PLAYER_SKILL_TYPE _neededPlayerSkillType;

	private string _strNeededStructure;

	private string _strNeededStructurePlural;

	private string _strProducedMonsters;

	private int _currentStep;

	private string _strMainStepTitle;

	private string _strTooltip1;

	private string _strTooltip2;

	private string _strTooltip3;

	public override Type serializedData => typeof(SaveDataBuildImpHutOrCrypt);

	public int currentStep => _currentStep;

	public STRUCTURE_TYPE neededStructureType => _neededStructureType;

	public PLAYER_SKILL_TYPE neededPlayerSkillType => _neededPlayerSkillType;

	public string strNeededStructure => _strNeededStructure;

	public string strProducedMonsters => _strProducedMonsters;

	public string strNeededStructurePlural => _strNeededStructurePlural;

	public BuildImpHutOrCrypt()
		: base(Game_Alert.Build_Imp_Hut_Or_Crypt)
	{
	}

	public BuildImpHutOrCrypt(SaveDataGameAlert p_data)
		: base(p_data, Game_Alert.Build_Imp_Hut_Or_Crypt)
	{
		SaveDataBuildImpHutOrCrypt saveDataBuildImpHutOrCrypt = p_data as SaveDataBuildImpHutOrCrypt;
		_currentStep = saveDataBuildImpHutOrCrypt.currentStep;
		_neededStructureType = saveDataBuildImpHutOrCrypt.neededStructureType;
		_neededPlayerSkillType = saveDataBuildImpHutOrCrypt.neededPlayerSkillType;
		_strNeededStructure = saveDataBuildImpHutOrCrypt.strNeededStructure;
		_strNeededStructurePlural = saveDataBuildImpHutOrCrypt.strNeededStructurePlural;
		_strProducedMonsters = saveDataBuildImpHutOrCrypt.strProducedMonsters;
		ConstructLocalizedTexts();
	}

	public override void SetAsSpawned()
	{
		_neededPlayerSkillType = GetNeededPlayerSkillType(out _neededStructureType);
		if (neededPlayerSkillType != PLAYER_SKILL_TYPE.NONE)
		{
			SkillData skillData = PlayerSkillManager.Instance.GetSkillData(neededPlayerSkillType);
			ConstructLocalizedTexts();
			if (skillData.isInUse && skillData.charges > 0)
			{
				AlertValid();
			}
			if (PlayerManager.Instance.player.playerSettlement.HasStructure(neededStructureType))
			{
				SetAsCleared();
				return;
			}
			Messenger.AddListener<SkillData, int>(PlayerSkillSignals.CHARGES_UPDATED, OnChargesAdjusted);
			Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_OBJECT_PLACED, OnStructurePlacedForSpawnedAlert);
			Messenger.AddListener<PLAYER_SKILL_TYPE>(PlayerSkillSignals.PLAYER_GAINED_DEMONIC_STRUCTURE, OnPlayerGainedDemonicStructure);
		}
	}

	public override void SetAsActive()
	{
		base.SetAsActive();
		SetActiveStepAs1();
		Messenger.RemoveListener<SkillData, int>(PlayerSkillSignals.CHARGES_UPDATED, OnChargesAdjusted);
		Messenger.RemoveListener<LocationStructure>(StructureSignals.STRUCTURE_OBJECT_PLACED, OnStructurePlacedForSpawnedAlert);
		Messenger.RemoveListener<PLAYER_SKILL_TYPE>(PlayerSkillSignals.PLAYER_GAINED_DEMONIC_STRUCTURE, OnPlayerGainedDemonicStructure);
	}

	protected override void SetAsCleared()
	{
		base.SetAsCleared();
		Messenger.RemoveListener<SkillData, int>(PlayerSkillSignals.CHARGES_UPDATED, OnChargesAdjusted);
		Messenger.RemoveListener<LocationStructure>(StructureSignals.STRUCTURE_OBJECT_PLACED, OnStructurePlacedForSpawnedAlert);
		Messenger.RemoveListener<PLAYER_SKILL_TYPE>(PlayerSkillSignals.PLAYER_GAINED_DEMONIC_STRUCTURE, OnPlayerGainedDemonicStructure);
		Messenger.RemoveListener<RuinarchToggle>(UISignals.TOGGLE_CLICKED, OnToggleClickedForStep1);
		Messenger.RemoveListener<SkillData>(PlayerSkillSignals.ON_EXECUTE_PLAYER_SKILL, OnStructurePlacedForStep2);
	}

	public override void LoadReferences(SaveDataGameAlert data)
	{
		base.LoadReferences(data);
		if (_isActive)
		{
			switch (_currentStep)
			{
			case 1:
				SetActiveStepAs1();
				break;
			case 2:
				SetActiveStepAs2();
				break;
			case 3:
				SetActiveStepAs3();
				break;
			}
		}
	}

	private void OnPlayerGainedDemonicStructure(PLAYER_SKILL_TYPE p_spell)
	{
		if (_neededPlayerSkillType == PLAYER_SKILL_TYPE.NONE)
		{
			_neededPlayerSkillType = GetNeededPlayerSkillType(out _neededStructureType);
			if (p_spell == _neededPlayerSkillType)
			{
				AlertValid();
			}
		}
	}

	private void OnChargesAdjusted(SkillData p_skillData, int p_amount)
	{
		if (p_skillData.type == neededPlayerSkillType)
		{
			if (p_skillData.charges > 0)
			{
				AlertValid();
			}
			else
			{
				AlertInvalid();
			}
		}
	}

	private void OnStructurePlacedForSpawnedAlert(LocationStructure p_structure)
	{
		if (p_structure.structureType == neededStructureType)
		{
			SetAsCleared();
			SaveManager.Instance.currentSaveDataPlayer.SetTutorialAlertAsDone(base.alertType);
		}
	}

	private void OnToggleClickedForStep1(RuinarchToggle p_toggle)
	{
		if (p_toggle.name == "Structures Tab" && p_toggle.isOn)
		{
			SetActiveStepAs2();
		}
	}

	private void OnStructurePlacedForStep2(SkillData p_skillData)
	{
		if (p_skillData.type == neededPlayerSkillType)
		{
			SetActiveStepAs3();
		}
	}

	private void OnSummonListOpenedForStep3()
	{
		RemoveBookmark();
	}

	private PLAYER_SKILL_TYPE GetNeededPlayerSkillType(out STRUCTURE_TYPE p_structureType)
	{
		if (PlayerManager.Instance.player.playerSkillComponent.CanBuildDemonicStructure(PLAYER_SKILL_TYPE.IMP_HUT))
		{
			p_structureType = STRUCTURE_TYPE.IMP_HUT;
			return PLAYER_SKILL_TYPE.IMP_HUT;
		}
		if (PlayerManager.Instance.player.playerSkillComponent.CanBuildDemonicStructure(PLAYER_SKILL_TYPE.CRYPT))
		{
			p_structureType = STRUCTURE_TYPE.CRYPT;
			return PLAYER_SKILL_TYPE.CRYPT;
		}
		p_structureType = STRUCTURE_TYPE.NONE;
		return PLAYER_SKILL_TYPE.NONE;
	}

	private void AlertValid()
	{
		TutorialManager.Instance.AddAlertToBuildingAlertPool(this);
	}

	private void AlertInvalid()
	{
		TutorialManager.Instance.RemoveFromBuildingAlertPool(this);
	}

	private void SetActiveStepAs1()
	{
		OnHoverOutBookmarkItem();
		_currentStep = 1;
		_displayName = _strMainStepTitle + " (1/3)";
		base.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(this);
		Messenger.AddListener<RuinarchToggle>(UISignals.TOGGLE_CLICKED, OnToggleClickedForStep1);
		if (PlayerUI.Instance.IsTopMenuToggleOn("Structures Tab"))
		{
			SetActiveStepAs2();
		}
	}

	private void SetActiveStepAs2()
	{
		OnHoverOutBookmarkItem();
		_currentStep = 2;
		_displayName = _strMainStepTitle + " (2/3)";
		base.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(this);
		Messenger.RemoveListener<RuinarchToggle>(UISignals.TOGGLE_CLICKED, OnToggleClickedForStep1);
		Messenger.AddListener<SkillData>(PlayerSkillSignals.ON_EXECUTE_PLAYER_SKILL, OnStructurePlacedForStep2);
	}

	private void SetActiveStepAs3()
	{
		OnHoverOutBookmarkItem();
		_currentStep = 3;
		_displayName = _strMainStepTitle + " (3/3)";
		base.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(this);
		Messenger.RemoveListener<SkillData>(PlayerSkillSignals.ON_EXECUTE_PLAYER_SKILL, OnStructurePlacedForStep2);
		Messenger.AddListener(UISignals.SUMMONS_LIST_OPENED, OnSummonListOpenedForStep3);
	}

	public override void OnHoverOverBookmarkItem(UIHoverPosition p_pos)
	{
		base.OnHoverOverBookmarkItem(p_pos);
		string info = string.Empty;
		if (_currentStep == 1)
		{
			info = _strTooltip1;
			Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "Structures Tab");
		}
		else if (_currentStep == 2)
		{
			info = _strTooltip2;
			Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, strNeededStructure);
		}
		else if (_currentStep == 3)
		{
			info = _strTooltip3;
			Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "Monsters Tab");
		}
		UIManager.Instance.ShowSmallInfo(info, p_pos, "", autoReplaceText: false);
	}

	public override void OnHoverOutBookmarkItem()
	{
		base.OnHoverOutBookmarkItem();
		if (_currentStep == 1)
		{
			Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, "Structures Tab");
		}
		else if (_currentStep == 2)
		{
			Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, strNeededStructure);
		}
		else if (_currentStep == 3)
		{
			Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, "Monsters Tab");
		}
		UIManager.Instance.HideSmallInfo();
	}

	public override void CleanUp()
	{
		base.CleanUp();
		Messenger.RemoveListener<RuinarchToggle>(UISignals.TOGGLE_CLICKED, OnToggleClickedForStep1);
	}

	public override void OnLocaleChanged(Locale p_newLocale)
	{
		ConstructLocalizedTexts();
		base.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(this);
	}

	protected override void OnControlDeviceChanged(string p_device)
	{
		ConstructLocalizedTexts();
	}

	private void ConstructLocalizedTexts()
	{
		SkillData skillData = PlayerSkillManager.Instance.GetSkillData(neededPlayerSkillType);
		_strNeededStructure = skillData.localizedName;
		_strNeededStructurePlural = Utilities.PluralizeString(skillData.localizedName);
		_strProducedMonsters = ((neededStructureType == STRUCTURE_TYPE.IMP_HUT) ? Utilities.PluralizeString(LocalizationManager.Instance.GetLocalizedValue("CharacterClasses_Table", "Imp")) : Utilities.PluralizeString(LocalizationManager.Instance.GetLocalizedValue("CharacterClasses_Table", "Skeleton")));
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
		dictionary.Add("text1", _strNeededStructure);
		_strMainStepTitle = GetLocalizedString("Build_Imp_Hut_Or_Crypt_Title", dictionary);
		dictionary.Clear();
		dictionary.Add("text1", _strProducedMonsters);
		dictionary.Add("text2", _strNeededStructurePlural);
		dictionary.Add("shortcutKey", InputManager.Instance.GetShortcutDisplayStringForAction(SHORTCUT_ACTION.Structures));
		_strTooltip1 = GetLocalizedString("Build_Imp_Hut_Or_Crypt_Tooltip_1", dictionary);
		dictionary.Clear();
		dictionary.Add("text1", _strNeededStructure);
		_strTooltip2 = GetLocalizedString("Build_Imp_Hut_Or_Crypt_Tooltip_2", dictionary);
		dictionary.Clear();
		dictionary.Add("text1", _strNeededStructure);
		dictionary.Add("text2", _strProducedMonsters);
		dictionary.Add("shortcutKey", InputManager.Instance.GetShortcutDisplayStringForAction(SHORTCUT_ACTION.Monsters));
		_strTooltip3 = GetLocalizedString("Build_Imp_Hut_Or_Crypt_Tooltip_3", dictionary);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
	}
}

using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Maccima_Games.Util;
using Ruinarch;
using Ruinarch.Custom_UI;
using Tutorial;

namespace Quests.Alerts;

public class BuildWatcher : GameAlert
{
	private int _currentStep;

	public override Type serializedData => typeof(SaveDataBuildWatcher);

	public int currentStep => _currentStep;

	public BuildWatcher()
		: base(Game_Alert.Build_Watcher)
	{
		_currentStep = 1;
	}

	public BuildWatcher(SaveDataGameAlert p_data)
		: base(p_data, Game_Alert.Build_Watcher)
	{
		SaveDataBuildWatcher saveDataBuildWatcher = p_data as SaveDataBuildWatcher;
		_currentStep = saveDataBuildWatcher.currentStep;
	}

	public override void SetAsSpawned()
	{
		SkillData skillData = PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.WATCHER);
		if (skillData.isInUse && skillData.charges > 0)
		{
			AlertValid();
		}
		if (PlayerManager.Instance.player.playerSettlement.HasStructure(STRUCTURE_TYPE.WATCHER))
		{
			SetAsCleared();
			return;
		}
		Messenger.AddListener<SkillData, int>(PlayerSkillSignals.CHARGES_UPDATED, OnChargesAdjusted);
		Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_OBJECT_PLACED, OnStructurePlacedForSpawnedAlert);
		Messenger.AddListener<PLAYER_SKILL_TYPE>(PlayerSkillSignals.PLAYER_GAINED_DEMONIC_STRUCTURE, OnPlayerGainedDemonicStructure);
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
		Messenger.RemoveListener<PlayerAction>(PlayerSkillSignals.PLAYER_ACTION_ACTIVATED, OnEyeSpawnedForStep3);
		Messenger.RemoveListener<IIntel>(PlayerSignals.PLAYER_OBTAINED_INTEL, OnPlayerObtainedIntelForStep4);
		Messenger.RemoveListener(UISignals.ON_SHARE_INTEL, OnShareIntel);
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
			case 4:
				SetActiveStepAs4();
				break;
			case 5:
				SetActiveStepAs5();
				break;
			}
		}
	}

	private void AlertValid()
	{
		TutorialManager.Instance.AddAlertToBuildingAlertPool(this);
	}

	private void AlertInvalid()
	{
		TutorialManager.Instance.RemoveFromBuildingAlertPool(this);
	}

	public override void OnHoverOverBookmarkItem(UIHoverPosition p_pos)
	{
		base.OnHoverOverBookmarkItem(p_pos);
		string info = string.Empty;
		if (_currentStep == 1)
		{
			Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim(1);
			dictionary.Add("shortcutKey", InputManager.Instance.GetShortcutDisplayStringForAction(SHORTCUT_ACTION.Structures));
			info = GetLocalizedString(_strGameAlertType + "_Tooltip_1", dictionary);
			MaccimaDictionaryPool<string, string>.Release(dictionary);
			Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "Structures Tab");
		}
		else if (_currentStep == 2)
		{
			info = GetLocalizedString(_strGameAlertType + "_Tooltip_2");
			Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "Watcher");
		}
		else if (_currentStep == 3)
		{
			info = GetLocalizedString(_strGameAlertType + "_Tooltip_3");
			Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "Spawn Eye");
		}
		else if (_currentStep == 4)
		{
			info = GetLocalizedString(_strGameAlertType + "_Tooltip_4");
			Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "Store Intel Button");
		}
		else if (_currentStep == 5)
		{
			Dictionary<string, string> dictionary2 = MaccimaDictionaryPool<string, string>.Claim(1);
			dictionary2.Add("shortcutKey", InputManager.Instance.GetShortcutDisplayStringForAction(SHORTCUT_ACTION.Intel));
			info = GetLocalizedString(_strGameAlertType + "_Tooltip_5", dictionary2);
			MaccimaDictionaryPool<string, string>.Release(dictionary2);
			Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "Intel Tab");
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
			Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, "Watcher");
		}
		else if (_currentStep == 3)
		{
			Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, "Spawn Eye");
		}
		else if (_currentStep == 4)
		{
			Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, "Store Intel Button");
		}
		else if (_currentStep == 5)
		{
			Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, "Intel Tab");
		}
		UIManager.Instance.HideSmallInfo();
	}

	private void SetActiveStepAs1()
	{
		OnHoverOutBookmarkItem();
		_currentStep = 1;
		_displayName = GetLocalizedString(_strGameAlertType + "_Title") + " (1/5)";
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
		_displayName = GetLocalizedString(_strGameAlertType + "_Title") + " (2/5)";
		base.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(this);
		Messenger.RemoveListener<RuinarchToggle>(UISignals.TOGGLE_CLICKED, OnToggleClickedForStep1);
		Messenger.AddListener<SkillData>(PlayerSkillSignals.ON_EXECUTE_PLAYER_SKILL, OnStructurePlacedForStep2);
	}

	private void SetActiveStepAs3()
	{
		OnHoverOutBookmarkItem();
		_currentStep = 3;
		_displayName = GetLocalizedString(_strGameAlertType + "_Title") + " (3/5)";
		base.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(this);
		Messenger.RemoveListener<SkillData>(PlayerSkillSignals.ON_EXECUTE_PLAYER_SKILL, OnStructurePlacedForStep2);
		Messenger.AddListener<PlayerAction>(PlayerSkillSignals.PLAYER_ACTION_ACTIVATED, OnEyeSpawnedForStep3);
	}

	private void SetActiveStepAs4()
	{
		OnHoverOutBookmarkItem();
		_currentStep = 4;
		_displayName = GetLocalizedString(_strGameAlertType + "_Title") + " (4/5)";
		base.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(this);
		Messenger.RemoveListener<PlayerAction>(PlayerSkillSignals.PLAYER_ACTION_ACTIVATED, OnEyeSpawnedForStep3);
		Messenger.AddListener<IIntel>(PlayerSignals.PLAYER_OBTAINED_INTEL, OnPlayerObtainedIntelForStep4);
	}

	private void SetActiveStepAs5()
	{
		OnHoverOutBookmarkItem();
		_currentStep = 5;
		_displayName = GetLocalizedString(_strGameAlertType + "_Title") + " (5/5)";
		base.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(this);
		Messenger.RemoveListener<IIntel>(PlayerSignals.PLAYER_OBTAINED_INTEL, OnPlayerObtainedIntelForStep4);
		Messenger.AddListener(UISignals.ON_SHARE_INTEL, OnShareIntel);
	}

	private void OnPlayerGainedDemonicStructure(PLAYER_SKILL_TYPE p_spell)
	{
		if (p_spell == PLAYER_SKILL_TYPE.WATCHER)
		{
			AlertValid();
		}
	}

	private void OnChargesAdjusted(SkillData p_skillData, int p_amount)
	{
		if (p_skillData.type == PLAYER_SKILL_TYPE.WATCHER)
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

	private void OnToggleClickedForStep1(RuinarchToggle p_toggle)
	{
		if (p_toggle.name == "Structures Tab" && p_toggle.isOn)
		{
			SetActiveStepAs2();
		}
	}

	private void OnStructurePlacedForStep2(SkillData p_skillData)
	{
		if (p_skillData.type == PLAYER_SKILL_TYPE.WATCHER)
		{
			SetActiveStepAs3();
		}
	}

	private void OnEyeSpawnedForStep3(PlayerAction p_playerAction)
	{
		if (p_playerAction.type == PLAYER_SKILL_TYPE.SPAWN_EYE_WARD)
		{
			SetActiveStepAs4();
		}
	}

	private void OnPlayerObtainedIntelForStep4(IIntel p_intel)
	{
		SetActiveStepAs5();
	}

	private void OnShareIntel()
	{
		RemoveBookmark();
	}

	private void OnStructurePlacedForSpawnedAlert(LocationStructure p_structure)
	{
		if (p_structure.structureType == STRUCTURE_TYPE.WATCHER)
		{
			SetAsCleared();
			SaveManager.Instance.currentSaveDataPlayer.SetTutorialAlertAsDone(base.alertType);
		}
	}

	public override void CleanUp()
	{
		base.CleanUp();
		Messenger.RemoveListener<RuinarchToggle>(UISignals.TOGGLE_CLICKED, OnToggleClickedForStep1);
	}
}

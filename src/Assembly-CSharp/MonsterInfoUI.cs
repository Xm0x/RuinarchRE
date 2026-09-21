using System.Linq;
using Ruinarch;
using TMPro;
using Traits;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class MonsterInfoUI : BaseCharacterInfoUI
{
	[Space(10f)]
	[Header("Piercing And Resistances")]
	[SerializeField]
	private PiercingAndResistancesInfo piercingAndResistancesInfo;

	[Space(10f)]
	[Header("Basic Info")]
	[SerializeField]
	private CharacterPortrait characterPortrait;

	[SerializeField]
	private TextMeshProUGUI nameLbl;

	[SerializeField]
	private TextMeshProUGUI roleLbl;

	[SerializeField]
	private TextMeshProUGUI categoryLbl;

	[SerializeField]
	private TextMeshProUGUI subLbl;

	[SerializeField]
	private TextMeshProUGUI plansLbl;

	[SerializeField]
	private EventLabel plansEventLabel;

	[SerializeField]
	private TextMeshProUGUI partyLbl;

	[SerializeField]
	private EventLabel partyEventLbl;

	[SerializeField]
	private LogItem plansLblLogItem;

	[SerializeField]
	private Image raceIcon;

	[SerializeField]
	private TextMeshProUGUI quickHPLbl;

	[SerializeField]
	private TextMeshProUGUI quickEnergyLbl;

	[SerializeField]
	private TextMeshProUGUI quickFullnessLbl;

	[SerializeField]
	private TextMeshProUGUI quickHappinessLbl;

	[SerializeField]
	private HoverHandler quickHPHoverHandler;

	[SerializeField]
	private HoverHandler quickEnergyHoverHandler;

	[SerializeField]
	private HoverHandler quickFullnessHoverHandler;

	[SerializeField]
	private HoverHandler quickHappinessHoverHandler;

	[SerializeField]
	private HoverHandler classNameHoverHandler;

	[Space(10f)]
	[Header("Location")]
	[SerializeField]
	private TextMeshProUGUI factionLbl;

	[SerializeField]
	private EventLabel factionEventLbl;

	[SerializeField]
	private TextMeshProUGUI workLocationLbl;

	[SerializeField]
	private EventLabel workLocationEventLbl;

	[SerializeField]
	private TextMeshProUGUI homeSettlementLbl;

	[SerializeField]
	private EventLabel homeSettlementEventLbl;

	[SerializeField]
	private TextMeshProUGUI houseLbl;

	[SerializeField]
	private EventLabel houseEventLbl;

	[Space(10f)]
	[Header("Logs")]
	[SerializeField]
	private LogsWindow logsWindow;

	[Space(10f)]
	[Header("Stats")]
	[SerializeField]
	private TextMeshProUGUI hpLbl;

	[SerializeField]
	private HoverHandler hpHoverHandler;

	[SerializeField]
	private TextMeshProUGUI attackLbl;

	[SerializeField]
	private HoverHandler attackHoverHandler;

	[SerializeField]
	private TextMeshProUGUI speedLbl;

	[SerializeField]
	private TextMeshProUGUI piercingLbl;

	[SerializeField]
	private HoverHandler piercingHoverHandler;

	[SerializeField]
	private TextMeshProUGUI raceLbl;

	[SerializeField]
	private TextMeshProUGUI elementLbl;

	[SerializeField]
	private HoverHandler elementHoverHandler;

	[SerializeField]
	private TextMeshProUGUI intLbl;

	[SerializeField]
	private TextMeshProUGUI critRateLbl;

	[SerializeField]
	private HoverHandler critRateHoverHandler;

	[SerializeField]
	private HoverHandler resistancesHoverHandler;

	[SerializeField]
	private TextMeshProUGUI behaviourLbl;

	[Space(10f)]
	[Header("Traits")]
	[SerializeField]
	private TextMeshProUGUI statusTraitsLbl;

	[SerializeField]
	private TextMeshProUGUI normalTraitsLbl;

	[SerializeField]
	private EventLabel statusTraitsEventLbl;

	[SerializeField]
	private EventLabel normalTraitsEventLbl;

	[Space(10f)]
	[Header("Items")]
	[SerializeField]
	private GameObject itemsParentObject;

	[SerializeField]
	private TextMeshProUGUI itemsLbl;

	[SerializeField]
	private EventLabel itemsEventLbl;

	[Space(10f)]
	[Header("Store Target")]
	[SerializeField]
	private StoreTargetButton btnStoreTarget;

	private HoverText m_roleHoverText;

	private HoverHandler m_hoverHandler;

	private Character _activeMonster;

	public Character activeMonster => _activeMonster;

	internal override void Initialize()
	{
		base.Initialize();
		Messenger.AddListener<Log>(UISignals.LOG_ADDED, UpdateHistory);
		Messenger.AddListener<Log>(UISignals.LOG_IN_DATABASE_UPDATED, UpdateHistory);
		Messenger.AddListener<Character>(UISignals.LOG_MENTIONING_CHARACTER_UPDATED, OnLogMentioningCharacterUpdated);
		Messenger.AddListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_ADDED, UpdateTraitsFromSignal);
		Messenger.AddListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_REMOVED, UpdateTraitsFromSignal);
		Messenger.AddListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_STACKED, UpdateTraitsFromSignal);
		Messenger.AddListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_UNSTACKED, UpdateTraitsFromSignal);
		Messenger.AddListener(UISignals.ON_OPEN_CONVERSATION_MENU, OnOpenConversationMenu);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
		Messenger.AddListener<TileObject, Character>(CharacterSignals.CHARACTER_OBTAINED_ITEM, UpdateInventoryInfoFromSignal);
		Messenger.AddListener<TileObject, Character>(CharacterSignals.CHARACTER_LOST_ITEM, UpdateInventoryInfoFromSignal);
		Messenger.AddListener<Character>(UISignals.UPDATE_THOUGHT_BUBBLE, UpdateThoughtBubbleFromSignal);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CHANGED_NAME, OnCharacterChangedName);
		Messenger.AddListener<Character>(UISignals.UPDATE_CHARACTER_INFO, CharacterRequestedForUpdate);
		Messenger.AddListener<KeyCode>(ControlsSignals.KEY_DOWN_EMPTY_SPACE, OnReceiveKeyCodeSignal);
		Messenger.AddListener<Character, int, object>(CharacterSignals.CHARACTER_ADJUSTED_HP, OnCharacterAdjustedHP);
		ListenToPlayerActionSignals();
		statusTraitsEventLbl.SetShouldColorHighlight(state: false);
		normalTraitsEventLbl.SetShouldColorHighlight(state: false);
		plansEventLabel.SetOnRightClickAction(OnRightClickThoughtBubble);
		itemsEventLbl.SetOnLeftClickAction(OnLeftClickItem);
		itemsEventLbl.SetOnRightClickAction(OnRightClickItem);
		hpHoverHandler.AddOnHoverOverAction(OnHoverOverHP);
		attackHoverHandler.AddOnHoverOverAction(OnHoverOverPower);
		piercingHoverHandler.AddOnHoverOverAction(OnHoverOverPiercing);
		elementHoverHandler.AddOnHoverOverAction(OnHoverOverElement);
		critRateHoverHandler.AddOnHoverOverAction(OnHoverOverCritRate);
		resistancesHoverHandler.AddOnHoverOverAction(OnHoverOverResistances);
		hpHoverHandler.AddOnHoverOutAction(OnHoverOutHP);
		attackHoverHandler.AddOnHoverOutAction(OnHoverOutPower);
		piercingHoverHandler.AddOnHoverOutAction(OnHoverOutPiercing);
		elementHoverHandler.AddOnHoverOutAction(OnHoverOutElement);
		critRateHoverHandler.AddOnHoverOutAction(OnHoverOutCritRate);
		resistancesHoverHandler.AddOnHoverOutAction(OnHoverOutResistances);
		factionEventLbl.SetOnLeftClickAction(OnClickFaction);
		homeSettlementEventLbl.SetOnLeftClickAction(OnLeftClickHomeVillage);
		homeSettlementEventLbl.SetOnRightClickAction(OnRightClickHomeVillage);
		houseEventLbl.SetOnLeftClickAction(OnLeftClickHomeStructure);
		houseEventLbl.SetOnRightClickAction(OnRightClickHomeStructure);
		workLocationEventLbl.SetOnLeftClickAction(OnLeftClickWorkStructure);
		workLocationEventLbl.SetOnRightClickAction(OnRightClickWorkStructure);
		partyEventLbl.SetOnLeftClickAction(OnClickParty);
		logsWindow.Initialize();
		m_hoverHandler = roleLbl.GetComponent<HoverHandler>();
		m_roleHoverText = roleLbl.GetComponent<HoverText>();
		piercingAndResistancesInfo.Initialize();
		quickHPHoverHandler.AddOnHoverOverAction(OnHoverOverQuickHP);
		quickEnergyHoverHandler.AddOnHoverOverAction(OnHoverOverQuickEnergy);
		quickFullnessHoverHandler.AddOnHoverOverAction(OnHoverOverQuickFullness);
		quickHappinessHoverHandler.AddOnHoverOverAction(OnHoverOverQuickHappiness);
		quickHPHoverHandler.AddOnHoverOutAction(UIManager.Instance.HideSmallInfo);
		quickEnergyHoverHandler.AddOnHoverOutAction(UIManager.Instance.HideSmallInfo);
		quickFullnessHoverHandler.AddOnHoverOutAction(UIManager.Instance.HideSmallInfo);
		quickHappinessHoverHandler.AddOnHoverOutAction(UIManager.Instance.HideSmallInfo);
	}

	private void OnCharacterAdjustedHP(Character p_character, int p_amount, object p_source)
	{
		if (isShowing && p_character == _activeCharacter)
		{
			UpdateStatInfo();
			UpdateQuickInfoMenu();
		}
	}

	private void OnRightClickThoughtBubble(object obj)
	{
		IPlayerActionTarget playerActionTarget = obj as IPlayerActionTarget;
		if (playerActionTarget != null)
		{
			if (playerActionTarget is Character { isLycanthrope: not false } character)
			{
				playerActionTarget = character.lycanData.activeForm;
			}
			UIManager.Instance.ShowPlayerActionContextMenu(playerActionTarget, InputManager.Instance.mousePosition, p_isScreenPosition: true);
		}
	}

	private void CharacterRequestedForUpdate(Character p_character)
	{
		if (isShowing && activeMonster == p_character)
		{
			UpdateCharacterInfo();
		}
	}

	public override void CloseMenu()
	{
		base.CloseMenu();
		_activeMonster = null;
	}

	public override void OpenMenu()
	{
		_ = _activeMonster;
		_activeMonster = _data as Character;
		base.OpenMenu();
		piercingAndResistancesInfo.UpdatePierceUI(_activeCharacter);
		btnStoreTarget.SetTarget(activeMonster);
		UpdateCharacterInfo();
		UpdateTraits();
		UpdateInventoryInfo();
		logsWindow.OnParentMenuOpened(activeMonster.persistentID);
		UpdateAllHistoryInfo();
		ResetAllScrollPositions();
		LoadActions(_activeMonster);
		if (!activeMonster.isDead)
		{
			CharacterClass characterClass = base.activeCharacter.characterClass;
			if (characterClass.uiSFX.IsValid())
			{
				characterClass.uiSFX.Post(InnerMapCameraMove.Instance.gameObject);
			}
		}
	}

	protected override bool ShouldCreateActionItem(SkillData p_skill, IPlayerActionTarget p_target)
	{
		if (base.ShouldCreateActionItem(p_skill, p_target))
		{
			if (p_skill.type != PLAYER_SKILL_TYPE.UNSUMMON)
			{
				return p_skill.type == PLAYER_SKILL_TYPE.UNDEPLOY_PARTY;
			}
			return true;
		}
		return false;
	}

	private void ResetAllScrollPositions()
	{
		logsWindow.ResetScrollPosition();
	}

	protected override void UpdateCharacterInfo()
	{
		base.UpdateCharacterInfo();
		UpdatePortrait();
		UpdateBasicInfo();
		UpdateStatInfo();
		UpdateQuickInfoMenu();
		UpdatePartyInfo();
		UpdateLocationInfo();
	}

	private void UpdatePortrait()
	{
		characterPortrait.GeneratePortrait(_activeMonster);
	}

	private void OnCharacterChangedName(Character p_character)
	{
		if (isShowing)
		{
			UpdateBasicInfo();
		}
	}

	public void UpdateBasicInfo()
	{
		CharacterClass characterClass = _activeMonster.characterClass;
		nameLbl.text = "<b>" + _activeMonster.firstNameWithColor + "</b>";
		if (_activeMonster.combatComponent.combatBehaviourParent.currentCombatBehaviour != null)
		{
			roleLbl.text = "<b>" + _activeMonster.combatComponent.combatBehaviourParent.currentCombatBehaviour.displayName + "</b>";
			m_roleHoverText.SetText(_activeMonster.combatComponent.combatBehaviourParent.currentCombatBehaviour.description);
			m_hoverHandler.enabled = true;
		}
		else
		{
			roleLbl.text = "<b>None</b>";
			m_hoverHandler.enabled = false;
			m_roleHoverText.SetText("");
		}
		RaceData raceData = RaceManager.Instance.GetRaceData(_activeMonster.race);
		categoryLbl.text = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", raceData.category.ToStringEnum());
		subLbl.text = characterClass.displayName;
		UpdateThoughtBubble();
	}

	private void UpdateThoughtBubble()
	{
		plansLbl.text = activeMonster.visuals.GetThoughtBubble();
	}

	private void UpdateQuickInfoMenu()
	{
		quickHPLbl.text = base.activeCharacter.currentHP.ToString();
		if (base.activeCharacter.needsComponent.HasNeeds())
		{
			quickEnergyLbl.text = base.activeCharacter.needsComponent.tiredness.ToString("N0");
			quickFullnessLbl.text = base.activeCharacter.needsComponent.fullness.ToString("N0");
			quickHappinessLbl.text = base.activeCharacter.needsComponent.happiness.ToString("N0");
		}
		else
		{
			quickEnergyLbl.text = "--";
			quickFullnessLbl.text = "--";
			quickHappinessLbl.text = "--";
		}
	}

	private void OnHoverOverQuickHP()
	{
		UIManager.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Health"));
	}

	private void OnHoverOverQuickEnergy()
	{
		UIManager.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Energy"));
	}

	private void OnHoverOverQuickFullness()
	{
		UIManager.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Fullness"));
	}

	private void OnHoverOverQuickHappiness()
	{
		UIManager.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Fun"));
	}

	private void OnHoverOverClassName()
	{
		if (isShowing)
		{
			CharacterClass characterClass = base.activeCharacter.characterClass;
			if (!string.IsNullOrEmpty(characterClass.displayDescription))
			{
				UIManager.Instance.ShowSmallInfo(characterClass.displayDescription);
			}
		}
	}

	private void OnHoverOutClassName()
	{
		if (isShowing)
		{
			UIManager.Instance.HideSmallInfo();
		}
	}

	private void OnReceiveKeyCodeSignal(KeyCode p_key)
	{
		if (p_key == KeyCode.Mouse1)
		{
			CloseMenu();
		}
	}

	private void UpdateStatInfo()
	{
		if (_activeMonster is Summon summon)
		{
			hpLbl.text = summon.currentHP + "/" + summon.maxHP;
			attackLbl.text = $"{_activeCharacter.combatComponent.GetDPS():F0}";
			speedLbl.text = $"{summon.combatComponent.GetAttackSpeedInSeconds()}s";
			raceLbl.text = GameUtilities.GetNormalizedSingularRace(summon.race) ?? "";
			critRateLbl.text = _activeCharacter.combatComponent.critRate + "%";
			elementLbl.text = "<size=\"20\">" + Utilities.GetRichTextIconForElement(_activeMonster.combatComponent.currentElement.type, elementLbl) + "</size>";
			piercingLbl.text = $"<size=\"20\">{Utilities.PiercingIcon()}</size>{_activeCharacter.piercingAndResistancesComponent.piercingPower}";
			behaviourLbl.gameObject.SetActive(value: false);
		}
		else
		{
			hpLbl.text = _activeMonster.currentHP + "/" + _activeMonster.maxHP;
			attackLbl.text = $"{_activeCharacter.combatComponent.GetDPS():F0}";
			speedLbl.text = $"{_activeMonster.combatComponent.GetAttackSpeedInSeconds()}s";
			raceLbl.text = GameUtilities.GetNormalizedSingularRace(_activeMonster.race) ?? "";
			elementLbl.text = "<size=\"20\">" + Utilities.GetRichTextIconForElement(_activeMonster.combatComponent.currentElement.type, elementLbl) + "</size>";
			critRateLbl.text = _activeCharacter.combatComponent.critRate + "%";
			piercingLbl.text = $"<size=\"20\">{Utilities.PiercingIcon()}</size>{_activeCharacter.piercingAndResistancesComponent.piercingPower}";
			behaviourLbl.gameObject.SetActive(value: false);
		}
	}

	public void OnHoverBehaviour(object obj)
	{
	}

	public void OnHoverOutBehaviour()
	{
		UIManager.Instance.HideSmallInfo();
	}

	private void OnHoverOverHP()
	{
		UIManager.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("OtherUI_Table", "Hitpoints_Tooltip"));
	}

	private void OnHoverOutHP()
	{
		UIManager.Instance.HideSmallInfo();
	}

	private void OnHoverOverCritRate()
	{
		UIManager.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("OtherUI_Table", "CritRate_Tooltip"));
	}

	private void OnHoverOutCritRate()
	{
		UIManager.Instance.HideSmallInfo();
	}

	private void OnHoverOverPower()
	{
		UIManager.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("OtherUI_Table", "Power_Tooltip"));
	}

	private void OnHoverOutPower()
	{
		UIManager.Instance.HideSmallInfo();
	}

	private void OnHoverOverPiercing()
	{
		UIManager.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("OtherUI_Table", "Piercing_Tooltip"));
	}

	private void OnHoverOutPiercing()
	{
		UIManager.Instance.HideSmallInfo();
	}

	private void OnHoverOverResistances()
	{
		UIManager.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("OtherUI_Table", "Resistances_Tooltip"));
	}

	private void OnHoverOutResistances()
	{
		UIManager.Instance.HideSmallInfo();
	}

	private void OnHoverOverElement()
	{
		UIManager.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("OtherUI_Table", "Element_Tooltip"));
	}

	private void OnHoverOutElement()
	{
		UIManager.Instance.HideSmallInfo();
	}

	private void UpdateTraitsFromSignal(Character character, Trait trait)
	{
		if (isShowing && _activeMonster == character)
		{
			UpdateTraits();
			UpdateThoughtBubble();
		}
	}

	private void UpdateThoughtBubbleFromSignal(Character character)
	{
		if (isShowing && _activeMonster == character)
		{
			UpdateThoughtBubble();
		}
	}

	private void UpdateTraits()
	{
		string text = string.Empty;
		string text2 = string.Empty;
		for (int i = 0; i < _activeMonster.traitContainer.statuses.Count; i++)
		{
			Status status = _activeMonster.traitContainer.statuses[i];
			if (!status.isHidden)
			{
				string text3 = "#CEB67C";
				if (!string.IsNullOrEmpty(text))
				{
					text += ", ";
				}
				text = $"{text}<b><color={text3}><link=\"{i}\">{status.GetNameInUI(_activeMonster)}</link></color></b>";
			}
		}
		for (int j = 0; j < _activeMonster.traitContainer.traits.Count; j++)
		{
			Trait trait = _activeMonster.traitContainer.traits[j];
			if (!trait.isHidden)
			{
				string text4 = "#CEB67C";
				if (trait.type == TRAIT_TYPE.BUFF)
				{
					text4 = "#39FF14";
				}
				else if (trait.type == TRAIT_TYPE.FLAW)
				{
					text4 = "#FF073A";
				}
				if (!string.IsNullOrEmpty(text2))
				{
					text2 += ", ";
				}
				text2 = $"{text2}<b><color={text4}><link=\"{j}\">{trait.GetNameInUI(_activeMonster)}</link></color></b>";
			}
		}
		statusTraitsLbl.text = string.Empty;
		if (!string.IsNullOrEmpty(text))
		{
			statusTraitsLbl.text = text;
		}
		normalTraitsLbl.text = string.Empty;
		if (!string.IsNullOrEmpty(text2))
		{
			normalTraitsLbl.text = text2;
		}
	}

	public void OnHoverTrait(object obj)
	{
		if (obj is string s)
		{
			int num = int.Parse(s);
			if (num < activeMonster.traitContainer.traits.Count)
			{
				string descriptionInUI = activeMonster.traitContainer.traits[num].descriptionInUI;
				UIManager.Instance.ShowSmallInfo(descriptionInUI, "", autoReplaceText: false);
			}
		}
	}

	public void OnHoverStatus(object obj)
	{
		if (obj is string s)
		{
			int num = int.Parse(s);
			if (num < activeMonster.traitContainer.statuses.Count)
			{
				string descriptionInUI = activeMonster.traitContainer.statuses[num].descriptionInUI;
				UIManager.Instance.ShowSmallInfo(descriptionInUI, "", autoReplaceText: false);
			}
		}
	}

	public void OnHoverOutTrait()
	{
		UIManager.Instance.HideSmallInfo();
	}

	public void ToggleItems(bool p_isOn)
	{
		itemsParentObject.SetActive(p_isOn);
	}

	private void UpdateInventoryInfoFromSignal(TileObject item, Character character)
	{
		if (isShowing && _activeMonster == character)
		{
			UpdateInventoryInfo();
		}
	}

	private void OnLeftClickItem(object obj)
	{
		if (obj is string s)
		{
			int index = int.Parse(s);
			TileObject tileObject = _activeMonster.items.ElementAtOrDefault(index);
			if (tileObject != null)
			{
				UIManager.Instance.ShowTileObjectInfo(tileObject);
			}
		}
	}

	private void OnRightClickItem(object obj)
	{
		if (obj is string s)
		{
			int index = int.Parse(s);
			TileObject tileObject = _activeMonster.items.ElementAtOrDefault(index);
			if (tileObject != null)
			{
				UIManager.Instance.ShowPlayerActionContextMenu(tileObject, InputManager.Instance.mousePosition, p_isScreenPosition: true);
			}
		}
	}

	private void UpdateInventoryInfo()
	{
		itemsLbl.text = string.Empty;
		for (int i = 0; i < _activeMonster.items.Count; i++)
		{
			TileObject tileObject = _activeMonster.items[i];
			itemsLbl.text = itemsLbl.text + "<link=\"" + i + "\">" + Utilities.ColorizeAndBoldName(tileObject.name) + "</link>";
			if (i < _activeMonster.items.Count - 1)
			{
				itemsLbl.text += ", ";
			}
		}
	}

	private void UpdateHistory(Log log)
	{
		if (isShowing && log.IsInvolved(activeMonster))
		{
			UpdateAllHistoryInfo();
		}
	}

	private void UpdateAllHistoryInfo()
	{
		logsWindow.UpdateAllHistoryInfo();
	}

	private void OnLogMentioningCharacterUpdated(Character character)
	{
		if (isShowing)
		{
			UpdateAllHistoryInfo();
		}
	}

	private void OnOpenConversationMenu()
	{
		backButton.interactable = false;
	}

	private void OnCharacterDied(Character character)
	{
		if (isShowing && activeMonster == character)
		{
			InnerMapCameraMove.Instance.CenterCameraOn(null);
		}
	}

	public void ShowCharacterTestingInfo()
	{
		TestingUtilities.ShowCharacterTestingInfo(activeMonster);
	}

	public void HideCharacterTestingInfo()
	{
		TestingUtilities.HideCharacterTestingInfo();
	}

	public void OnClickRenameButton()
	{
		Messenger.Broadcast(UISignals.EDIT_CHARACTER_NAME, activeMonster.persistentID, activeMonster.name);
	}

	private void UpdateLocationInfo()
	{
		if (_activeCharacter.faction != null)
		{
			factionLbl.text = (_activeCharacter.faction.isMajorNonPlayer ? ("<link=\"faction\">" + Utilities.ColorizeAndBoldName(_activeCharacter.faction.name, FactionManager.Instance.GetFactionNameColorHex()) + "</link>") : Utilities.ColorizeName(_activeCharacter.faction.name, FactionManager.Instance.GetFactionNameColorHex()));
		}
		else
		{
			factionLbl.text = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "None");
		}
		workLocationLbl.text = ((_activeCharacter.structureComponent.workPlaceStructure != null) ? ("<link=\"work\">" + Utilities.ColorizeAndBoldName(_activeCharacter.structureComponent.workPlaceStructure.name) + "</link>") : LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "None"));
		homeSettlementLbl.text = ((_activeCharacter.homeSettlement != null && _activeCharacter.homeSettlement.locationType == LOCATION_TYPE.VILLAGE) ? ("<link=\"home\">" + Utilities.ColorizeAndBoldName(_activeCharacter.homeSettlement.name) + "</link>") : LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "None"));
		houseLbl.text = ((_activeCharacter.homeStructure != null) ? ("<link=\"house\">" + Utilities.ColorizeAndBoldName(_activeCharacter.homeStructure.name) + "</link>") : LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "None"));
	}

	private void OnClickFaction(object obj)
	{
		if (base.activeCharacter.faction.isMajorNonPlayer)
		{
			UIManager.Instance.ShowFactionInfo(base.activeCharacter.faction);
		}
	}

	private void OnLeftClickHomeVillage(object obj)
	{
		if (_activeCharacter.homeSettlement != null)
		{
			UIManager.Instance.ShowSettlementInfo(_activeCharacter.homeSettlement);
		}
	}

	private void OnRightClickHomeVillage(object obj)
	{
		if (_activeCharacter.homeSettlement != null)
		{
			UIManager.Instance.ShowPlayerActionContextMenu(_activeCharacter.homeSettlement, Input.mousePosition, p_isScreenPosition: true);
		}
	}

	private void OnLeftClickHomeStructure(object obj)
	{
		base.activeCharacter.homeStructure?.CenterOnStructure();
	}

	private void OnRightClickHomeStructure(object obj)
	{
		if (obj is IPlayerActionTarget p_target)
		{
			UIManager.Instance.ShowPlayerActionContextMenu(p_target, Input.mousePosition, p_isScreenPosition: true);
		}
	}

	private void OnLeftClickWorkStructure(object obj)
	{
		base.activeCharacter.structureComponent.workPlaceStructure?.CenterOnStructure();
	}

	private void OnRightClickWorkStructure(object obj)
	{
		if (base.activeCharacter.structureComponent.workPlaceStructure != null)
		{
			UIManager.Instance.ShowPlayerActionContextMenu(base.activeCharacter.structureComponent.workPlaceStructure, Input.mousePosition, p_isScreenPosition: true);
		}
	}

	private void UpdatePartyInfo()
	{
		string text = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "None");
		if (base.activeCharacter.partyComponent.hasParty)
		{
			text = "<link=\"party\">" + Utilities.ColorizeAndBoldName(base.activeCharacter.partyComponent.currentParty.partyName) + "</link>";
		}
		partyLbl.text = text;
	}

	private void OnClickParty(object obj)
	{
		if (base.activeCharacter.partyComponent.hasParty)
		{
			UIManager.Instance.ShowPartyInfo(base.activeCharacter.partyComponent.currentParty);
		}
	}
}

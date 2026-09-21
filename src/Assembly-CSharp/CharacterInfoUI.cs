using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using Object_Pools;
using Ruinarch;
using Ruinarch.Custom_UI;
using TMPro;
using Traits;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UtilityScripts;

public class CharacterInfoUI : BaseCharacterInfoUI
{
	private enum VIEW_MODE
	{
		None,
		Info,
		Mood,
		Relationship,
		Logs
	}

	private struct MoodSummaryEntry
	{
		public int amount;

		public GameDate expiryDate;
	}

	private VIEW_MODE m_currentViewMode;

	[Header("Basic Info")]
	[SerializeField]
	private CharacterPortrait characterPortrait;

	[SerializeField]
	private TextMeshProUGUI nameLbl;

	[SerializeField]
	private TextMeshProUGUI subLbl;

	[SerializeField]
	private TextMeshProUGUI actionLbl;

	[SerializeField]
	private EventLabel actionEventLabel;

	[SerializeField]
	private TextMeshProUGUI partyLbl;

	[SerializeField]
	private EventLabel partyEventLbl;

	[SerializeField]
	private Image raceIcon;

	[SerializeField]
	private TextMeshProUGUI coinsLbl;

	[SerializeField]
	private TextMeshProUGUI resonancePowerLbl;

	[SerializeField]
	private HoverHandler resonancePowerHoverHandler;

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

	[FormerlySerializedAs("currentLocationLbl")]
	[SerializeField]
	private TextMeshProUGUI workLocationLbl;

	[FormerlySerializedAs("currentLocationEventLbl")]
	[SerializeField]
	private EventLabel workLocationEventLbl;

	[FormerlySerializedAs("homeRegionLbl")]
	[SerializeField]
	private TextMeshProUGUI homeSettlementLbl;

	[FormerlySerializedAs("homeRegionEventLbl")]
	[SerializeField]
	private EventLabel homeSettlementEventLbl;

	[SerializeField]
	private TextMeshProUGUI houseLbl;

	[SerializeField]
	private EventLabel houseEventLbl;

	[Space(10f)]
	[Header("Logs")]
	[SerializeField]
	private LogsWindow _logsWindow;

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
	private HoverHandler coinsHoverHandler;

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
	private TextMeshProUGUI itemsHeaderLbl;

	[SerializeField]
	private TextMeshProUGUI itemsLbl;

	[SerializeField]
	private EventLabel itemsEventLbl;

	[Space(10f)]
	[Header("Talents")]
	[SerializeField]
	private GameObject talentsParentObject;

	[SerializeField]
	private TextMeshProUGUI martialArtsLbl;

	[SerializeField]
	private TextMeshProUGUI combatMagicLbl;

	[SerializeField]
	private TextMeshProUGUI healingMagicLbl;

	[SerializeField]
	private TextMeshProUGUI craftingLbl;

	[SerializeField]
	private TextMeshProUGUI resourcesLbl;

	[SerializeField]
	private TextMeshProUGUI foodLbl;

	[SerializeField]
	private TextMeshProUGUI socialLbl;

	[SerializeField]
	private HoverHandler martialArtsHoverText;

	[SerializeField]
	private HoverHandler combatMagicHoverText;

	[SerializeField]
	private HoverHandler healingMagicHoverText;

	[SerializeField]
	private HoverHandler craftingHoverText;

	[SerializeField]
	private HoverHandler resourcesHoverText;

	[SerializeField]
	private HoverHandler foodHoverText;

	[SerializeField]
	private HoverHandler socialHoverText;

	[SerializeField]
	private CharacterTalentTooltip talentToolTip;

	[Space(10f)]
	[Header("Relationships")]
	[SerializeField]
	private EventLabel relationshipNamesEventLbl;

	[SerializeField]
	private TextMeshProUGUI relationshipTypesLbl;

	[SerializeField]
	private TextMeshProUGUI relationshipNamesLbl;

	[SerializeField]
	private TextMeshProUGUI relationshipValuesLbl;

	[SerializeField]
	private UIHoverPosition relationshipNameplateItemPosition;

	[SerializeField]
	private RelationshipFilterItem[] relationFilterItems;

	[SerializeField]
	private GameObject relationFiltersGO;

	[SerializeField]
	private Toggle allRelationshipFiltersToggle;

	[SerializeField]
	private EventLabel opinionsEventLabel;

	[SerializeField]
	private ScrollRect relationshipsScrollView;

	[Space(10f)]
	[Header("Mood")]
	[SerializeField]
	private MarkedMeter moodMeter;

	[SerializeField]
	private TextMeshProUGUI moodSummary;

	[SerializeField]
	private ScrollRect scrollViewMoodSummary;

	[SerializeField]
	private GameObject prefabMoodThought;

	[Space(10f)]
	[Header("Needs")]
	[SerializeField]
	private MarkedMeter energyMeter;

	[SerializeField]
	private MarkedMeter fullnessMeter;

	[SerializeField]
	private MarkedMeter happinessMeter;

	[SerializeField]
	private MarkedMeter hopeMeter;

	[SerializeField]
	private MarkedMeter staminaMeter;

	[Space(10f)]
	[Header("Piercing And Resistances")]
	[SerializeField]
	private PiercingAndResistancesInfo piercingAndResistancesInfo;

	[Space(10f)]
	[Header("Store Target")]
	[SerializeField]
	private StoreTargetButton btnStoreTarget;

	private List<SkillData> afflictions;

	private bool aliveRelationsOnly;

	private List<RELATIONS_FILTER> filters;

	private RELATIONS_FILTER[] allFilters;

	private Dictionary<string, MoodSummaryEntry> _dictMoodSummary;

	public GameObject infoContent;

	public GameObject btnRevealInfo;

	public GameObject moodContent;

	public GameObject btnRevealMood;

	public GameObject relationshipContent;

	public GameObject btnRevealRelationship;

	public GameObject logContent;

	public GameObject btnRevealLogs;

	public PiercingAndResistancesInfo pierceUI;

	internal override void Initialize()
	{
		base.Initialize();
		Messenger.AddListener(Signals.GAME_LOADED, OnGameLoaded);
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
		Messenger.AddListener<Relatable, Relatable>(CharacterSignals.RELATIONSHIP_CREATED, OnRelationshipChanged);
		Messenger.AddListener<Relatable, Relatable>(CharacterSignals.RELATIONSHIP_TYPE_ADDED, OnRelationshipChanged);
		Messenger.AddListener<Character, Character>(CharacterSignals.OPINION_ADDED, OnOpinionChanged);
		Messenger.AddListener<Character, Character>(CharacterSignals.OPINION_REMOVED, OnOpinionChanged);
		Messenger.AddListener<Character, Character, string>(CharacterSignals.OPINION_INCREASED, OnOpinionChanged);
		Messenger.AddListener<Character, Character, string>(CharacterSignals.OPINION_DECREASED, OnOpinionChanged);
		Messenger.AddListener<Character>(UISignals.UPDATE_THOUGHT_BUBBLE, UpdateThoughtBubbleFromSignal);
		Messenger.AddListener<MoodComponent>(CharacterSignals.MOOD_SUMMARY_MODIFIED, OnMoodModified);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CHANGED_NAME, OnCharacterChangedName);
		Messenger.AddListener<Character, CharacterClass, CharacterClass>(CharacterSignals.CHARACTER_CLASS_CHANGE, OnCharacterChangedClass);
		Messenger.AddListener<Character>(UISignals.UPDATE_CHARACTER_INFO, CharacterRequestedForUpdate);
		Messenger.AddListener<KeyCode>(ControlsSignals.KEY_DOWN_EMPTY_SPACE, OnReceiveKeyCodeSignal);
		Messenger.AddListener<int, int>(PlayerSignals.CHAOTIC_ENERGY_ADJUSTED, OnPlaguePointsAdjusted);
		Messenger.AddListener<Character, int, object>(CharacterSignals.CHARACTER_ADJUSTED_HP, OnCharacterAdjustedHP);
		Messenger.AddListener<SharedOpinionModifier>(CharacterSignals.SHARED_OPINION_MODIFIER_DECREASED, OnSharedOpinionModified);
		Messenger.AddListener<SharedOpinionModifier>(CharacterSignals.SHARED_OPINION_MODIFIER_INCREASED, OnSharedOpinionModified);
		Messenger.AddListener<RELIGION>(CharacterSignals.ACTIVE_RELIGIOUS_CULTISTS_UPDATED, OnActiveReligiousCultistsUpdated);
		actionEventLabel.SetOnRightClickAction(OnRightClickThoughtBubble);
		relationshipNamesEventLbl.SetOnLeftClickAction(OnLeftClickRelationship);
		relationshipNamesEventLbl.SetOnRightClickAction(OnRightClickRelationship);
		hpHoverHandler.AddOnHoverOverAction(OnHoverOverHP);
		attackHoverHandler.AddOnHoverOverAction(OnHoverOverPower);
		piercingHoverHandler.AddOnHoverOverAction(OnHoverOverPiercing);
		elementHoverHandler.AddOnHoverOverAction(OnHoverOverElement);
		critRateHoverHandler.AddOnHoverOverAction(OnHoverOverCritRate);
		resistancesHoverHandler.AddOnHoverOverAction(OnHoverOverResistances);
		coinsHoverHandler.AddOnHoverOverAction(OnHoverOverCoins);
		hpHoverHandler.AddOnHoverOutAction(OnHoverOutHP);
		attackHoverHandler.AddOnHoverOutAction(OnHoverOutPower);
		piercingHoverHandler.AddOnHoverOutAction(OnHoverOutPiercing);
		elementHoverHandler.AddOnHoverOutAction(OnHoverOutElement);
		critRateHoverHandler.AddOnHoverOutAction(OnHoverOutCritRate);
		resistancesHoverHandler.AddOnHoverOutAction(OnHoverOutResistances);
		coinsHoverHandler.AddOnHoverOutAction(OnHoverOutCoins);
		factionEventLbl.SetOnLeftClickAction(OnClickFaction);
		homeSettlementEventLbl.SetOnLeftClickAction(OnLeftClickHomeVillage);
		homeSettlementEventLbl.SetOnRightClickAction(OnRightClickHomeVillage);
		houseEventLbl.SetOnLeftClickAction(OnLeftClickHomeStructure);
		houseEventLbl.SetOnRightClickAction(OnRightClickHomeStructure);
		workLocationEventLbl.SetOnLeftClickAction(OnLeftClickWorkStructure);
		workLocationEventLbl.SetOnRightClickAction(OnRightClickWorkStructure);
		partyEventLbl.SetOnLeftClickAction(OnClickParty);
		itemsEventLbl.SetOnLeftClickAction(OnLeftClickItem);
		itemsEventLbl.SetOnRightClickAction(OnRightClickItem);
		opinionsEventLabel.SetShouldColorHighlight(state: false);
		statusTraitsEventLbl.SetShouldColorHighlight(state: false);
		normalTraitsEventLbl.SetShouldColorHighlight(state: false);
		moodMeter.ResetMarks();
		moodMeter.AddMark((float)EditableValuesManager.Instance.criticalMoodHighThreshold / 100f, Color.red);
		moodMeter.AddMark((float)EditableValuesManager.Instance.lowMoodHighThreshold / 100f, Color.yellow);
		Color green = Color.green;
		energyMeter.ResetMarks();
		energyMeter.AddMark(0.91f, green);
		energyMeter.AddMark(0.5f, Color.yellow);
		energyMeter.AddMark(0.2f, Color.red);
		fullnessMeter.ResetMarks();
		fullnessMeter.AddMark(0.91f, green);
		fullnessMeter.AddMark(0.5f, Color.yellow);
		fullnessMeter.AddMark(0.2f, Color.red);
		happinessMeter.ResetMarks();
		happinessMeter.AddMark(0.91f, green);
		happinessMeter.AddMark(0.5f, Color.yellow);
		happinessMeter.AddMark(0.2f, Color.red);
		_logsWindow.Initialize();
		InitializeRelationships();
		afflictions = new List<SkillData>();
		_dictMoodSummary = new Dictionary<string, MoodSummaryEntry>(10);
		piercingAndResistancesInfo.Initialize();
		SetButtonRevealPriceDisplay();
		ListenTalentHoverListener();
		resonancePowerHoverHandler.AddOnHoverOverAction(OnHoverOverResonancePower);
		resonancePowerHoverHandler.AddOnHoverOutAction(OnHoverOutResonancePower);
		quickHPHoverHandler.AddOnHoverOverAction(OnHoverOverQuickHP);
		quickEnergyHoverHandler.AddOnHoverOverAction(OnHoverOverQuickEnergy);
		quickFullnessHoverHandler.AddOnHoverOverAction(OnHoverOverQuickFullness);
		quickHappinessHoverHandler.AddOnHoverOverAction(OnHoverOverQuickHappiness);
		quickHPHoverHandler.AddOnHoverOutAction(UIManager.Instance.HideSmallInfo);
		quickEnergyHoverHandler.AddOnHoverOutAction(UIManager.Instance.HideSmallInfo);
		quickFullnessHoverHandler.AddOnHoverOutAction(UIManager.Instance.HideSmallInfo);
		quickHappinessHoverHandler.AddOnHoverOutAction(UIManager.Instance.HideSmallInfo);
		classNameHoverHandler.AddOnHoverOverAction(OnHoverOverClassName);
		classNameHoverHandler.AddOnHoverOutAction(OnHoverOutClassName);
	}

	private void OnGameLoaded()
	{
		SetButtonRevealPriceDisplay();
	}

	private void OnCharacterAdjustedHP(Character p_character, int p_amount, object p_source)
	{
		if (isShowing && p_character == _activeCharacter)
		{
			UpdateStatInfo();
			UpdateQuickInfoMenu();
		}
	}

	private void OnPlaguePointsAdjusted(int p_amount, int p_plaguePoints)
	{
		InitializeRevealHoverText();
	}

	private void SetButtonRevealPriceDisplay()
	{
		btnRevealInfo.transform.Find("Chaotic").GetComponentInChildren<RuinarchText>().text = EditableValuesManager.Instance.GetRevealCharacterInfoCost().ToString();
		btnRevealLogs.transform.Find("Chaotic").GetComponentInChildren<RuinarchText>().text = EditableValuesManager.Instance.GetRevealCharacterInfoCost().ToString();
		btnRevealMood.transform.Find("Chaotic").GetComponentInChildren<RuinarchText>().text = EditableValuesManager.Instance.GetRevealCharacterInfoCost().ToString();
		btnRevealRelationship.transform.Find("Chaotic").GetComponentInChildren<RuinarchText>().text = EditableValuesManager.Instance.GetRevealCharacterInfoCost().ToString();
	}

	private void InitializeRevealHoverText()
	{
		if (PlayerManager.Instance.player.currenciesComponent.chaoticEnergy < EditableValuesManager.Instance.GetRevealCharacterInfoCost())
		{
			btnRevealInfo.GetComponent<HoverText>()?.SetText("Not_Enough_Chaotic_Energy");
			btnRevealLogs.GetComponent<HoverText>()?.SetText("Not_Enough_Chaotic_Energy");
			btnRevealMood.GetComponent<HoverText>()?.SetText("Not_Enough_Chaotic_Energy");
			btnRevealRelationship.GetComponent<HoverText>()?.SetText("Not_Enough_Chaotic_Energy");
			btnRevealInfo.GetComponent<RuinarchButton>().interactable = false;
			btnRevealLogs.GetComponent<RuinarchButton>().interactable = false;
			btnRevealMood.GetComponent<RuinarchButton>().interactable = false;
			btnRevealRelationship.GetComponent<RuinarchButton>().interactable = false;
		}
		else
		{
			btnRevealInfo.GetComponent<HoverText>()?.SetText("Reveal Character Info");
			btnRevealLogs.GetComponent<HoverText>()?.SetText("Reveal Character Info");
			btnRevealMood.GetComponent<HoverText>()?.SetText("Reveal Character Info");
			btnRevealRelationship.GetComponent<HoverText>()?.SetText("Reveal Character Info");
			btnRevealInfo.GetComponent<RuinarchButton>().interactable = true;
			btnRevealLogs.GetComponent<RuinarchButton>().interactable = true;
			btnRevealMood.GetComponent<RuinarchButton>().interactable = true;
			btnRevealRelationship.GetComponent<RuinarchButton>().interactable = true;
		}
	}

	private void CharacterRequestedForUpdate(Character p_character)
	{
		if (isShowing && _activeCharacter == p_character)
		{
			UpdateCharacterInfo();
		}
	}

	private void OnReceiveKeyCodeSignal(KeyCode p_key)
	{
		if (p_key == KeyCode.Mouse1)
		{
			CloseMenu();
		}
	}

	public override void CloseMenu()
	{
		base.CloseMenu();
		Selector.Instance.Deselect();
		Character character = _activeCharacter;
		_activeCharacter = null;
		if (character != null && (object)character.marker != null)
		{
			if (InnerMapCameraMove.Instance != null && InnerMapCameraMove.Instance.target == character.marker.gameObject.transform)
			{
				InnerMapCameraMove.Instance.CenterCameraOn(null);
			}
			character.marker.UpdateNameplateElementsState();
		}
		m_currentViewMode = VIEW_MODE.None;
	}

	public override void OpenMenu()
	{
		base.OpenMenu();
		InitializeRevealHoverText();
		piercingAndResistancesInfo.UpdatePierceUI(_activeCharacter);
		btnStoreTarget.SetTarget(_activeCharacter);
		UpdateCharacterInfo();
		UpdateTraits();
		UpdateRelationships();
		UpdateInventoryInfo();
		_logsWindow.OnParentMenuOpened(_activeCharacter.persistentID);
		UpdateAllHistoryInfo();
		ResetAllScrollPositions();
		UpdateMoodSummary();
		ProcessDisplay();
		CharacterClass characterClass = base.activeCharacter.characterClass;
		if (characterClass.uiSFX.IsValid())
		{
			characterClass.uiSFX.Post(InnerMapCameraMove.Instance.gameObject);
		}
	}

	private void ProcessDisplay()
	{
		switch (m_currentViewMode)
		{
		case VIEW_MODE.Info:
			OnToggleInfo(isOn: true);
			break;
		case VIEW_MODE.Mood:
			OnToggleMood(isOn: true);
			break;
		case VIEW_MODE.Relationship:
			OnToggleRelations(isOn: true);
			break;
		case VIEW_MODE.Logs:
			OnToggleLogs(isOn: true);
			break;
		default:
			OnToggleInfo(isOn: false);
			OnToggleMood(isOn: false);
			OnToggleRelations(isOn: false);
			OnToggleInfo(isOn: true);
			break;
		}
		InitializeRevealHoverText();
	}

	private void ResetAllScrollPositions()
	{
		_logsWindow.ResetScrollPosition();
	}

	protected override void UpdateCharacterInfo()
	{
		base.UpdateCharacterInfo();
		if (_activeCharacter != null)
		{
			UpdatePortrait();
			UpdateBasicInfo();
			UpdateStatInfo();
			UpdateLocationInfo();
			UpdateMoodMeter();
			UpdateNeedMeters();
			UpdatePartyInfo();
			UpdateQuickInfoMenu();
		}
	}

	private void UpdatePortrait()
	{
		characterPortrait.GeneratePortrait(_activeCharacter);
	}

	private void OnCharacterChangedName(Character p_character)
	{
		if (isShowing)
		{
			UpdateBasicInfo();
		}
	}

	private void OnCharacterChangedClass(Character p_character, CharacterClass p_previousClass, CharacterClass p_newClass)
	{
		if (isShowing && base.activeCharacter == p_character)
		{
			UpdateBasicInfo();
		}
	}

	public void UpdateBasicInfo()
	{
		nameLbl.text = "<b>" + _activeCharacter.firstNameWithColor + "</b>";
		UpdateSubTextAndIcon();
		UpdateThoughtBubble();
	}

	private void UpdateSubTextAndIcon()
	{
		if (!_activeCharacter.isNormalCharacter)
		{
			subLbl.gameObject.SetActive(value: false);
			return;
		}
		CharacterClass characterClass = _activeCharacter.characterClass;
		subLbl.text = characterClass.displayName;
		raceIcon.sprite = _activeCharacter.raceSetting.nameplateIcon;
		raceIcon.gameObject.SetActive(_activeCharacter.raceSetting.nameplateIcon != null);
		subLbl.gameObject.SetActive(value: true);
	}

	public void UpdateThoughtBubble()
	{
		actionLbl.text = base.activeCharacter.visuals.GetThoughtBubble();
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

	private void UpdateQuickInfoMenu()
	{
		quickHPLbl.text = (base.activeCharacter.isInfoUnlocked ? base.activeCharacter.currentHP.ToString() : "???");
		if (base.activeCharacter.needsComponent.HasNeeds())
		{
			quickEnergyLbl.text = (base.activeCharacter.isInfoUnlocked ? base.activeCharacter.needsComponent.tiredness.ToString("N0") : "??");
			quickFullnessLbl.text = (base.activeCharacter.isInfoUnlocked ? base.activeCharacter.needsComponent.fullness.ToString("N0") : "??");
			quickHappinessLbl.text = (base.activeCharacter.isInfoUnlocked ? base.activeCharacter.needsComponent.happiness.ToString("N0") : "??");
		}
		else
		{
			quickEnergyLbl.text = (base.activeCharacter.isInfoUnlocked ? "--" : "??");
			quickFullnessLbl.text = (base.activeCharacter.isInfoUnlocked ? "--" : "??");
			quickHappinessLbl.text = (base.activeCharacter.isInfoUnlocked ? "--" : "??");
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

	private void UpdateStatInfo()
	{
		hpLbl.text = _activeCharacter.currentHP + "/" + _activeCharacter.maxHP;
		attackLbl.text = $"{_activeCharacter.combatComponent.GetDPS():F0}";
		speedLbl.text = $"{_activeCharacter.combatComponent.GetAttackSpeedInSeconds()}s";
		coinsLbl.text = $"{Utilities.CoinIcon()}{_activeCharacter.moneyComponent.coins}";
		critRateLbl.text = _activeCharacter.combatComponent.critRate + "%";
		raceLbl.text = GameUtilities.GetNormalizedSingularRace(_activeCharacter.race) ?? "";
		elementLbl.text = "<size=\"20\">" + Utilities.GetRichTextIconForElement(_activeCharacter.combatComponent.currentElement.type, elementLbl) + "</size>";
		piercingLbl.text = $"<size=\"20\">{Utilities.PiercingIcon()}</size>{_activeCharacter.piercingAndResistancesComponent.piercingPower}";
		SkillData skillData = PlayerSkillManager.Instance.GetSkillData(_activeCharacter.resonancePower);
		resonancePowerLbl.text = skillData.localizedName ?? "";
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

	private void OnHoverOverCoins()
	{
		UIManager.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("OtherUI_Table", "Coins_Tooltip"));
	}

	private void OnHoverOutCoins()
	{
		UIManager.Instance.HideSmallInfo();
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
		UpdateTalentsDisplay();
	}

	private void OnHoverExitTalent()
	{
		talentToolTip.gameObject.SetActive(value: false);
	}

	private void OnHoverTalent(Character p_character, CHARACTER_TALENT p_talentType)
	{
		talentToolTip.gameObject.SetActive(value: true);
		talentToolTip.ShowCharacterTalentData(base.activeCharacter, p_talentType);
	}

	private void ListenTalentHoverListener()
	{
		martialArtsHoverText.AddOnHoverOverAction(delegate
		{
			OnHoverTalent(base.activeCharacter, CHARACTER_TALENT.Martial_Arts);
		});
		combatMagicHoverText.AddOnHoverOverAction(delegate
		{
			OnHoverTalent(base.activeCharacter, CHARACTER_TALENT.Combat_Magic);
		});
		healingMagicHoverText.AddOnHoverOverAction(delegate
		{
			OnHoverTalent(base.activeCharacter, CHARACTER_TALENT.Healing_Magic);
		});
		craftingHoverText.AddOnHoverOverAction(delegate
		{
			OnHoverTalent(base.activeCharacter, CHARACTER_TALENT.Crafting);
		});
		resourcesHoverText.AddOnHoverOverAction(delegate
		{
			OnHoverTalent(base.activeCharacter, CHARACTER_TALENT.Resources);
		});
		foodHoverText.AddOnHoverOverAction(delegate
		{
			OnHoverTalent(base.activeCharacter, CHARACTER_TALENT.Food);
		});
		socialHoverText.AddOnHoverOverAction(delegate
		{
			OnHoverTalent(base.activeCharacter, CHARACTER_TALENT.Social);
		});
		martialArtsHoverText.AddOnHoverOutAction(OnHoverExitTalent);
		combatMagicHoverText.AddOnHoverOutAction(OnHoverExitTalent);
		healingMagicHoverText.AddOnHoverOutAction(OnHoverExitTalent);
		craftingHoverText.AddOnHoverOutAction(OnHoverExitTalent);
		resourcesHoverText.AddOnHoverOutAction(OnHoverExitTalent);
		foodHoverText.AddOnHoverOutAction(OnHoverExitTalent);
		socialHoverText.AddOnHoverOutAction(OnHoverExitTalent);
	}

	private void UpdateTalentsDisplay()
	{
		martialArtsLbl.text = _activeCharacter.TryGetTalentLevel(CHARACTER_TALENT.Martial_Arts).ToString();
		combatMagicLbl.text = _activeCharacter.TryGetTalentLevel(CHARACTER_TALENT.Combat_Magic).ToString();
		healingMagicLbl.text = _activeCharacter.TryGetTalentLevel(CHARACTER_TALENT.Healing_Magic).ToString();
		craftingLbl.text = _activeCharacter.TryGetTalentLevel(CHARACTER_TALENT.Crafting).ToString();
		resourcesLbl.text = _activeCharacter.TryGetTalentLevel(CHARACTER_TALENT.Resources).ToString();
		foodLbl.text = _activeCharacter.TryGetTalentLevel(CHARACTER_TALENT.Food).ToString();
		socialLbl.text = _activeCharacter.TryGetTalentLevel(CHARACTER_TALENT.Social).ToString();
	}

	private void OnClickFaction(object obj)
	{
		UIManager.Instance.ShowFactionInfo(base.activeCharacter.faction);
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
			UIManager.Instance.ShowPlayerActionContextMenu(_activeCharacter.homeSettlement, InputManager.Instance.mousePosition, p_isScreenPosition: true);
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
			UIManager.Instance.ShowPlayerActionContextMenu(p_target, InputManager.Instance.mousePosition, p_isScreenPosition: true);
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
			UIManager.Instance.ShowPlayerActionContextMenu(base.activeCharacter.structureComponent.workPlaceStructure, InputManager.Instance.mousePosition, p_isScreenPosition: true);
		}
	}

	public void ToggleTalents(bool p_isOn)
	{
		talentsParentObject.SetActive(p_isOn);
	}

	private void UpdateTraitsFromSignal(Character character, Trait trait)
	{
		if (_activeCharacter != null && _activeCharacter == character)
		{
			UpdateTraits();
			UpdateThoughtBubble();
			UpdateStatInfo();
		}
	}

	private void UpdateThoughtBubbleFromSignal(Character character)
	{
		if (isShowing && _activeCharacter == character)
		{
			UpdateThoughtBubble();
		}
	}

	private void UpdateTraits()
	{
		string text = string.Empty;
		string text2 = string.Empty;
		bool isNormalCharacter = _activeCharacter.isNormalCharacter;
		for (int i = 0; i < _activeCharacter.traitContainer.statuses.Count; i++)
		{
			Status status = _activeCharacter.traitContainer.statuses[i];
			if (!status.isHidden && (!status.IsNeeds() || isNormalCharacter))
			{
				string text3 = "#CEB67C";
				if (status.moodEffect > 0 || status.effect == TRAIT_EFFECT.POSITIVE)
				{
					text3 = "#39FF14";
				}
				else if (status.moodEffect < 0)
				{
					text3 = "#FF073A";
				}
				if (!string.IsNullOrEmpty(text))
				{
					text += ", ";
				}
				text = $"{text}<b><color={text3}><link=\"{i}\">{status.GetNameInUI(base.activeCharacter)}</link></color></b>";
			}
		}
		for (int j = 0; j < _activeCharacter.traitContainer.traits.Count; j++)
		{
			Trait trait = _activeCharacter.traitContainer.traits[j];
			if (!trait.isHidden && (!trait.IsNeeds() || isNormalCharacter))
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
				text2 = $"{text2}<b><color={text4}><link=\"{j}\">{trait.GetNameInUI(base.activeCharacter)}</link></color></b>";
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
			int index = int.Parse(s);
			Trait trait = base.activeCharacter.traitContainer.traits.ElementAtOrDefault(index);
			if (trait != null)
			{
				string descriptionInUI = trait.descriptionInUI;
				UIManager.Instance.ShowSmallInfo(descriptionInUI, "", autoReplaceText: false);
			}
		}
	}

	public void OnHoverStatus(object obj)
	{
		if (obj is string s)
		{
			int index = int.Parse(s);
			Trait trait = base.activeCharacter.traitContainer.statuses.ElementAtOrDefault(index);
			if (trait != null)
			{
				string descriptionInUI = trait.descriptionInUI;
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
		if (isShowing && _activeCharacter == character)
		{
			UpdateInventoryInfo();
		}
	}

	private void OnLeftClickItem(object obj)
	{
		if (obj is string s)
		{
			int index = int.Parse(s);
			TileObject tileObject = _activeCharacter.items.ElementAtOrDefault(index);
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
			TileObject tileObject = _activeCharacter.items.ElementAtOrDefault(index);
			if (tileObject != null)
			{
				UIManager.Instance.ShowPlayerActionContextMenu(tileObject, InputManager.Instance.mousePosition, p_isScreenPosition: true);
			}
		}
	}

	private void UpdateInventoryInfo()
	{
		string text = "#FFFFFF";
		if (_activeCharacter.IsInventoryAtFullCapacity())
		{
			text = "#FF073A";
		}
		itemsHeaderLbl.text = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Items") + " <color=" + text + ">" + _activeCharacter.items.Count + "</color>/" + _activeCharacter.characterClass.inventoryCapacity;
		string text2 = string.Empty;
		for (int i = 0; i < _activeCharacter.items.Count; i++)
		{
			TileObject tileObject = _activeCharacter.items[i];
			text2 = text2 + "<link=\"" + i + "\">" + Utilities.ColorizeAndBoldName(tileObject.name) + "</link>";
			if (i < _activeCharacter.items.Count - 1)
			{
				text2 += ", ";
			}
		}
		itemsLbl.text = text2;
		itemsLbl.Refresh();
	}

	private void UpdateHistory(Log log)
	{
		if (isShowing && log.IsInvolved(_activeCharacter))
		{
			UpdateAllHistoryInfo();
		}
	}

	private void OnLogMentioningCharacterUpdated(Character character)
	{
		if (isShowing)
		{
			UpdateAllHistoryInfo();
		}
	}

	public void UpdateAllHistoryInfo()
	{
		_logsWindow.UpdateAllHistoryInfo();
	}

	private void OnOpenConversationMenu()
	{
		backButton.interactable = false;
	}

	private void OnCharacterDied(Character character)
	{
		if (isShowing)
		{
			if (base.activeCharacter.id == character.id)
			{
				InnerMapCameraMove.Instance.CenterCameraOn(null);
				UpdateMoodSummary();
			}
			if (base.activeCharacter.relationshipContainer.HasRelationshipWith(character))
			{
				UpdateRelationships();
			}
		}
	}

	private void OnActiveReligiousCultistsUpdated(RELIGION p_religion)
	{
		if (isShowing)
		{
			SetButtonRevealPriceDisplay();
		}
	}

	public void ShowCharacterTestingInfo()
	{
	}

	public void HideCharacterTestingInfo()
	{
	}

	private void InitializeRelationships()
	{
		for (int i = 0; i < relationFilterItems.Length; i++)
		{
			RelationshipFilterItem obj = relationFilterItems[i];
			obj.Initialize(OnToggleRelationshipFilter);
			obj.SetIsOnWithoutNotify(isOn: true);
		}
		allRelationshipFiltersToggle.SetIsOnWithoutNotify(value: true);
		allFilters = CollectionUtilities.GetEnumValues<RELATIONS_FILTER>();
		filters = new List<RELATIONS_FILTER>(allFilters);
		aliveRelationsOnly = true;
	}

	public void OnToggleShowOnlyAliveRelations(bool isOn)
	{
		aliveRelationsOnly = isOn;
		UpdateRelationships();
	}

	public void OnToggleShowAll(bool isOn)
	{
		filters.Clear();
		if (isOn)
		{
			filters.AddRange(allFilters);
		}
		for (int i = 0; i < relationFilterItems.Length; i++)
		{
			relationFilterItems[i].SetIsOnWithoutNotify(isOn);
		}
		UpdateRelationships();
	}

	private void OnToggleRelationshipFilter(bool isOn, RELATIONS_FILTER filter)
	{
		if (isOn)
		{
			filters.Add(filter);
		}
		else
		{
			filters.Remove(filter);
		}
		allRelationshipFiltersToggle.SetIsOnWithoutNotify(filters.Count == allFilters.Length);
		UpdateRelationships();
	}

	public void ToggleRelationFilters()
	{
		relationFiltersGO.SetActive(!relationFiltersGO.activeSelf);
	}

	private void UpdateRelationships()
	{
		relationshipTypesLbl.text = string.Empty;
		relationshipNamesLbl.text = string.Empty;
		relationshipValuesLbl.text = string.Empty;
		HashSet<int> hashSet = new HashSet<int>();
		foreach (KeyValuePair<int, IRelationshipData> relationship in base.activeCharacter.relationshipContainer.relationships)
		{
			if (DoesRelationshipMeetFilters(relationship.Key, relationship.Value))
			{
				hashSet.Add(relationship.Key);
			}
		}
		Dictionary<int, IRelationshipData> dictionary = _activeCharacter.relationshipContainer.relationships.OrderByDescending((KeyValuePair<int, IRelationshipData> k) => k.Value.opinions.totalOpinion).ToDictionary((KeyValuePair<int, IRelationshipData> k) => k.Key, (KeyValuePair<int, IRelationshipData> v) => v.Value);
		List<int> list = _activeCharacter.relationshipContainer.relationships.Keys.ToList();
		for (int num = 0; num < dictionary.Keys.Count; num++)
		{
			int num2 = dictionary.Keys.ElementAt(num);
			if (hashSet.Contains(num2))
			{
				int num3 = list.IndexOf(num2);
				IRelationshipData relationshipDataWith = _activeCharacter.relationshipContainer.GetRelationshipDataWith(num2);
				string localizedRelationshipNameWith = _activeCharacter.relationshipContainer.GetLocalizedRelationshipNameWith(num2);
				Character characterByID = CharacterManager.Instance.GetCharacterByID(num2);
				TextMeshProUGUI textMeshProUGUI = relationshipTypesLbl;
				textMeshProUGUI.text = textMeshProUGUI.text + localizedRelationshipNameWith + "\n";
				int number = 0;
				string text;
				if (characterByID != null && characterByID.relationshipContainer.HasRelationshipWith(base.activeCharacter))
				{
					number = characterByID.relationshipContainer.GetTotalOpinion(base.activeCharacter);
					text = GetOpinionText(number);
				}
				else
				{
					text = "???";
				}
				TextMeshProUGUI textMeshProUGUI2 = relationshipNamesLbl;
				textMeshProUGUI2.text = textMeshProUGUI2.text + "<link=\"" + num3 + "\">" + Utilities.ColorizeAndBoldName(relationshipDataWith.targetName) + "</link>";
				if (relationshipDataWith.hasGrudge)
				{
					relationshipNamesLbl.text += Utilities.GrudgeIcon();
				}
				relationshipNamesLbl.text += "\n";
				textMeshProUGUI2 = relationshipValuesLbl;
				textMeshProUGUI2.text = textMeshProUGUI2.text + "<link=\"" + num3 + "\"><color=" + BaseRelationshipContainer.OpinionColor(base.activeCharacter.relationshipContainer.GetTotalOpinion(num2)) + "> " + GetOpinionText(base.activeCharacter.relationshipContainer.GetTotalOpinion(num2)) + "</color> <color=" + BaseRelationshipContainer.OpinionColor(number) + ">(" + text + ")</color></link>\n";
			}
		}
		if (relationshipsScrollView.gameObject.activeInHierarchy)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(relationshipsScrollView.content);
		}
	}

	private bool DoesRelationshipMeetFilters(int id, IRelationshipData data)
	{
		Character characterByID = CharacterManager.Instance.GetCharacterByID(id);
		if (characterByID != null)
		{
			if (aliveRelationsOnly && characterByID.isDead)
			{
				return false;
			}
			return DoesRelationshipMeetAnyFilter(data);
		}
		return DoesRelationshipMeetAnyFilter(data);
	}

	private bool DoesRelationshipMeetAnyFilter(IRelationshipData data)
	{
		if (filters.Count == 0)
		{
			return false;
		}
		bool flag = false;
		string opinionLabel = data.opinions.GetOpinionLabel();
		for (int i = 0; i < filters.Count; i++)
		{
			switch (filters[i])
			{
			case RELATIONS_FILTER.Enemies:
				if (opinionLabel == "Enemy")
				{
					flag = true;
				}
				break;
			case RELATIONS_FILTER.Rivals:
				if (opinionLabel == "Rival")
				{
					flag = true;
				}
				break;
			case RELATIONS_FILTER.Acquaintances:
				if (opinionLabel == "Acquaintance")
				{
					flag = true;
				}
				break;
			case RELATIONS_FILTER.Friends:
				if (opinionLabel == "Friend")
				{
					flag = true;
				}
				break;
			case RELATIONS_FILTER.Close_Friends:
				if (opinionLabel == "Close Friend")
				{
					flag = true;
				}
				break;
			case RELATIONS_FILTER.Relatives:
				if (data.IsFamilyMember())
				{
					flag = true;
				}
				break;
			case RELATIONS_FILTER.Lovers:
				if (data.IsLoverOrAffair())
				{
					flag = true;
				}
				break;
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	public void OnHoverRelationshipValue(object obj)
	{
		if (obj is string)
		{
			int index = int.Parse((string)obj);
			int targetID = _activeCharacter.relationshipContainer.relationships.Keys.ElementAtOrDefault(index);
			ShowOpinionData(targetID);
		}
	}

	public void OnHoverRelationshipName(object obj)
	{
		if (obj is string s)
		{
			int index = int.Parse(s);
			int id = _activeCharacter.relationshipContainer.relationships.Keys.ElementAtOrDefault(index);
			OnHoverCharacterNameInRelationships(id);
		}
	}

	private void OnSharedOpinionModified(SharedOpinionModifier p_modifier)
	{
		if (isShowing && base.activeCharacter.relationshipContainer.HasRelationshipWith(p_modifier.targetCharacter))
		{
			UpdateRelationships();
		}
	}

	private void OnOpinionChanged(Character owner, Character target, string reason)
	{
		if (isShowing && (owner == base.activeCharacter || target == base.activeCharacter))
		{
			UpdateRelationships();
		}
	}

	private void OnOpinionChanged(Character owner, Character target)
	{
		if (isShowing && (owner == base.activeCharacter || target == base.activeCharacter))
		{
			UpdateRelationships();
		}
	}

	private void OnRelationshipChanged(Relatable owner, Relatable target)
	{
		if (isShowing && (owner == base.activeCharacter || target == base.activeCharacter))
		{
			UpdateRelationships();
		}
	}

	private void ShowOpinionData(int targetID)
	{
		IRelationshipData relationshipDataWith = _activeCharacter.relationshipContainer.GetRelationshipDataWith(targetID);
		Character characterByID = CharacterManager.Instance.GetCharacterByID(targetID);
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "UI", "UIStrings_Table", "Opinion_of");
		log.AddToFillers(base.activeCharacter, base.activeCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddToFillers(null, relationshipDataWith.targetName, LOG_IDENTIFIER.TARGET_CHARACTER);
		string text = log.logText;
		LogPool.Release(log);
		bool flag = base.activeCharacter.traitContainer.HasTrait("Psychopath");
		if (!flag)
		{
			text += "\n---------------------";
			OpinionData opinionData = base.activeCharacter.relationshipContainer.GetOpinionData(targetID);
			foreach (KeyValuePair<string, int> allOpinion in opinionData.allOpinions)
			{
				string key = allOpinion.Key;
				if (!(key != "Base") || allOpinion.Value != 0)
				{
					string text2 = LocalizationManager.Instance.GetLocalizedValue("Relationships_Table", key);
					if (string.IsNullOrEmpty(text2))
					{
						text2 = key;
					}
					text = text + "\n" + text2 + ": <color=" + BaseRelationshipContainer.OpinionColorNoGray(allOpinion.Value) + ">" + GetOpinionText(allOpinion.Value) + "</color>";
				}
			}
			for (int i = 0; i < opinionData.sharedOpinions.Count; i++)
			{
				SharedOpinionModifier sharedOpinionModifier = opinionData.sharedOpinions[i];
				if (sharedOpinionModifier.modifierValue != 0)
				{
					text = text + "\n" + sharedOpinionModifier.modifierName + ": <color=" + BaseRelationshipContainer.OpinionColorNoGray(sharedOpinionModifier.modifierValue) + ">" + GetOpinionText(sharedOpinionModifier.modifierValue) + "</color>";
				}
			}
			text += "\n---------------------";
		}
		text = text + "\n" + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Total") + ": <color=" + BaseRelationshipContainer.OpinionColorNoGray(relationshipDataWith.opinions.totalOpinion) + ">" + GetOpinionText(base.activeCharacter.relationshipContainer.GetTotalOpinion(targetID)) + "</color>";
		if (flag)
		{
			text = text + " (" + LocalizationManager.Instance.GetLocalizedValue("Traits_Table", "Psychopath") + ")";
		}
		log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "UI", "UIStrings_Table", "Opinion_of");
		log.AddToFillers(null, relationshipDataWith.targetName, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddToFillers(base.activeCharacter, base.activeCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
		if (characterByID != null)
		{
			int totalOpinion = characterByID.relationshipContainer.GetTotalOpinion(base.activeCharacter);
			text = text + "\n" + log.logText + ": <color=" + BaseRelationshipContainer.OpinionColorNoGray(totalOpinion) + ">" + GetOpinionText(totalOpinion) + "</color>";
		}
		else
		{
			text = text + "\n" + log.logText + ": ???</color>";
		}
		text = text + "\n\n" + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Compatibility") + ": " + RelationshipManager.Instance.GetCompatibilityBetween(base.activeCharacter, targetID);
		if (relationshipDataWith.hasGrudge)
		{
			text = text + "\n" + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Grudgeful") + " " + Utilities.GrudgeIcon();
		}
		UIManager.Instance.ShowSmallInfo(text);
	}

	public void HideRelationshipData()
	{
		UIManager.Instance.HideSmallInfo();
	}

	private string GetOpinionText(int number)
	{
		if (number < 0)
		{
			return number.ToString() ?? "";
		}
		return "+" + number;
	}

	private void OnLeftClickRelationship(object obj)
	{
		if (obj is string)
		{
			int index = int.Parse((string)obj);
			Character characterByID = CharacterManager.Instance.GetCharacterByID(_activeCharacter.relationshipContainer.relationships.Keys.ElementAtOrDefault(index));
			if (characterByID != null)
			{
				UIManager.Instance.ShowCharacterInfo(characterByID, centerOnCharacter: true);
			}
		}
	}

	private void OnRightClickRelationship(object obj)
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

	private void OnHoverCharacterNameInRelationships(int id)
	{
		Character characterByID = CharacterManager.Instance.GetCharacterByID(id);
		if (characterByID != null)
		{
			UIManager.Instance.HideSmallInfo();
			UIManager.Instance.ShowCharacterNameplateTooltip(characterByID, relationshipNameplateItemPosition);
		}
		else
		{
			IRelationshipData relationshipData = _activeCharacter.relationshipContainer.relationships[id];
			UIManager.Instance.ShowSmallInfo(relationshipData.targetName + " " + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Relationship_Not_Present"), relationshipNameplateItemPosition);
			UIManager.Instance.HideCharacterNameplateTooltip();
		}
	}

	public void HideRelationshipNameplate()
	{
		UIManager.Instance.HideSmallInfo();
		UIManager.Instance.HideCharacterNameplateTooltip();
	}

	private void OnMoodModified(MoodComponent moodComponent)
	{
		if (_activeCharacter != null && _activeCharacter.moodComponent == moodComponent)
		{
			UpdateMoodMeter();
			UpdateMoodSummary();
		}
	}

	private void UpdateMoodMeter()
	{
		moodMeter.SetFillAmount((float)_activeCharacter.moodComponent.moodValue / 100f);
	}

	private void UpdateMoodSummary()
	{
		Utilities.DestroyChildren(scrollViewMoodSummary.content);
		if (_activeCharacter.isDead)
		{
			return;
		}
		_dictMoodSummary.Clear();
		foreach (List<MoodModification> value2 in _activeCharacter.moodComponent.allMoodModifications.Values)
		{
			for (int i = 0; i < value2.Count; i++)
			{
				MoodModification moodModification = value2[i];
				Log flavorText = moodModification.flavorText;
				if (flavorText == null)
				{
					continue;
				}
				int modification = moodModification.modification;
				GameDate expiryDate = value2[value2.Count - 1 - i].expiryDate;
				MoodSummaryEntry value;
				if (!_dictMoodSummary.ContainsKey(flavorText.logText))
				{
					value = new MoodSummaryEntry
					{
						amount = modification,
						expiryDate = expiryDate
					};
					_dictMoodSummary.Add(flavorText.logText, value);
					continue;
				}
				value = _dictMoodSummary[flavorText.logText];
				value.amount += modification;
				if (expiryDate.IsAfter(value.expiryDate))
				{
					value.expiryDate = expiryDate;
				}
				_dictMoodSummary[flavorText.logText] = value;
			}
		}
		foreach (KeyValuePair<string, MoodSummaryEntry> item in _dictMoodSummary)
		{
			ObjectPoolManager.Instance.InstantiateObjectFromPool(prefabMoodThought.name, Vector3.zero, Quaternion.identity, scrollViewMoodSummary.content).GetComponent<MoodThoughtUIItem>().SetItemDetails(item.Key, item.Value.amount, item.Value.expiryDate, OnHoverOverMoodEffect, OnHoverOutMoodEffect);
		}
	}

	private void OnHoverOverMoodEffect(GameDate expiryDate)
	{
		string info;
		if (expiryDate.hasValue)
		{
			GameDate gameDate = GameManager.Instance.Today();
			info = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Mood_Effect_Lasts") + " " + gameDate.GetTimeDifferenceString(expiryDate);
		}
		else
		{
			info = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Mood_Effect_Lasts_Linked_To_Needs");
		}
		UIManager.Instance.ShowSmallInfo(info, "", autoReplaceText: false);
	}

	public void OnHoverMoodEffect(object obj)
	{
		if (obj is string s)
		{
			int.Parse(s);
		}
	}

	public void OnHoverOutMoodEffect()
	{
		UIManager.Instance.HideSmallInfo();
	}

	public void ShowMoodTooltip()
	{
		string info = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Mood_Tooltip") + "\n\n" + _activeCharacter.moodComponent.moodValue + "/100\n" + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Brainwash_Success_Rate") + " " + PrisonCell.GetBrainwashSuccessRate(_activeCharacter).ToString("N0") + "%";
		UIManager.Instance.ShowSmallInfo(info, LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Mood") + ": " + _activeCharacter.moodComponent.moodStateName);
	}

	public void HideSmallInfo()
	{
		UIManager.Instance.HideSmallInfo();
	}

	private void UpdateNeedMeters()
	{
		energyMeter.SetFillAmount(_activeCharacter.needsComponent.tiredness / 100f);
		fullnessMeter.SetFillAmount(_activeCharacter.needsComponent.fullness / 100f);
		happinessMeter.SetFillAmount(_activeCharacter.needsComponent.happiness / 100f);
	}

	public void ShowEnergyTooltip()
	{
		string info = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Energy_Tooltip") + ": " + _activeCharacter.needsComponent.tiredness + "/100";
		UIManager.Instance.ShowSmallInfo(info, LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Energy"));
	}

	public void ShowFullnessTooltip()
	{
		string info = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Fullness_Tooltip") + ": " + _activeCharacter.needsComponent.fullness + "/100";
		UIManager.Instance.ShowSmallInfo(info, LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Fullness"));
	}

	public void ShowHappinessTooltip()
	{
		string info = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Happiness_Tooltip") + ": " + _activeCharacter.needsComponent.happiness + "/100";
		UIManager.Instance.ShowSmallInfo(info, LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Fun"));
	}

	public void ShowHopeTooltip()
	{
		string info = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Hope_Tooltip") + ": " + _activeCharacter.needsComponent.hope + "/100";
		UIManager.Instance.ShowSmallInfo(info, "TRUST");
	}

	public void ShowStaminaTooltip()
	{
		string info = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Stamina_Tooltip") + ": " + _activeCharacter.needsComponent.stamina + "/100";
		UIManager.Instance.ShowSmallInfo(info, "STAMINA");
	}

	public void ClickPierce()
	{
		if (pierceUI.isShowing)
		{
			pierceUI.HidePiercingAndResistancesInfo();
		}
		else
		{
			pierceUI.ShowPiercingAndResistancesInfo(base.activeCharacter);
		}
	}

	public void HidePierceUI()
	{
		pierceUI.HidePiercingAndResistancesInfo();
	}

	public void OnRevealInfoclicked()
	{
		if (PlayerManager.Instance.player.currenciesComponent.chaoticEnergy >= EditableValuesManager.Instance.GetRevealCharacterInfoCost() && !base.activeCharacter.isInfoUnlocked)
		{
			AudioManager.Instance.TryPlayUISFX("Play_Reveal_Info");
			base.activeCharacter.isInfoUnlocked = true;
			PlayerManager.Instance.player.currenciesComponent.AdjustChaoticEnergy(-EditableValuesManager.Instance.GetRevealCharacterInfoCost());
			ProcessDisplay();
			UpdateQuickInfoMenu();
			Messenger.Broadcast(CharacterSignals.CHARACTER_INFO_REVEALED);
		}
	}

	private void ShowInfoHideRevealButton()
	{
		btnRevealInfo.SetActive(value: false);
		infoContent.SetActive(value: true);
	}

	private void ShowRevealButtonHideInfo()
	{
		btnRevealInfo.SetActive(value: true);
		infoContent.SetActive(value: false);
	}

	private void ShowMoodHideRevealButton()
	{
		btnRevealMood.SetActive(value: false);
		moodContent.SetActive(value: true);
	}

	private void ShowRevealButtonHideMood()
	{
		btnRevealMood.SetActive(value: true);
		moodContent.SetActive(value: false);
	}

	private void ShowRelationshipHideRevealButton()
	{
		btnRevealRelationship.SetActive(value: false);
		relationshipContent.SetActive(value: true);
	}

	private void ShowRevealButtonHideRelationship()
	{
		btnRevealRelationship.SetActive(value: true);
		relationshipContent.SetActive(value: false);
	}

	private void ShowLogsHideRevealButton()
	{
		btnRevealLogs.SetActive(value: false);
		logContent.SetActive(value: true);
	}

	private void ShowRevealButtonHideLogs()
	{
		btnRevealLogs.SetActive(value: true);
		logContent.SetActive(value: false);
	}

	private void HideAllInfo()
	{
		ShowRevealButtonHideInfo();
		ShowRevealButtonHideMood();
		ShowRevealButtonHideRelationship();
		ShowRevealButtonHideLogs();
	}

	private void ShowAllInfo()
	{
		ShowInfoHideRevealButton();
		ShowMoodHideRevealButton();
		ShowRelationshipHideRevealButton();
		ShowLogsHideRevealButton();
	}

	public void OnToggleInfo(bool isOn)
	{
		if (!isOn)
		{
			return;
		}
		m_currentViewMode = VIEW_MODE.Info;
		if (base.activeCharacter.race.IsSapient())
		{
			if (base.activeCharacter.isInfoUnlocked)
			{
				ShowAllInfo();
			}
			else
			{
				HideAllInfo();
			}
		}
		else
		{
			ShowInfoHideRevealButton();
		}
	}

	public void OnToggleMood(bool isOn)
	{
		if (!isOn)
		{
			return;
		}
		m_currentViewMode = VIEW_MODE.Mood;
		if (base.activeCharacter.race.IsSapient())
		{
			if (base.activeCharacter.isInfoUnlocked)
			{
				ShowAllInfo();
			}
			else
			{
				HideAllInfo();
			}
		}
		else
		{
			ShowMoodHideRevealButton();
		}
	}

	public void OnToggleRelations(bool isOn)
	{
		if (isOn)
		{
			m_currentViewMode = VIEW_MODE.Relationship;
			if (base.activeCharacter.race.IsSapient())
			{
				if (base.activeCharacter.isInfoUnlocked)
				{
					ShowAllInfo();
				}
				else
				{
					HideAllInfo();
				}
			}
			else
			{
				ShowRelationshipHideRevealButton();
			}
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(relationshipsScrollView.content);
	}

	public void OnToggleLogs(bool isOn)
	{
		if (isOn)
		{
			m_currentViewMode = VIEW_MODE.Logs;
			if (base.activeCharacter.race.IsSapient())
			{
				if (base.activeCharacter.isInfoUnlocked)
				{
					ShowAllInfo();
				}
				else
				{
					HideAllInfo();
				}
			}
			else
			{
				ShowLogsHideRevealButton();
			}
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(relationshipsScrollView.content);
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

	public void OnClickRenameButton()
	{
		Messenger.Broadcast(UISignals.EDIT_CHARACTER_NAME, base.activeCharacter.persistentID, base.activeCharacter.name);
	}

	public void OnHoverRaceIcon()
	{
		_ = base.activeCharacter;
	}

	public void OnHoverExitRaceIcon()
	{
	}

	public void TogglePiercingAndResistances()
	{
		if (piercingAndResistancesInfo.isShowing)
		{
			piercingAndResistancesInfo.HidePiercingAndResistancesInfo();
		}
		else
		{
			piercingAndResistancesInfo.ShowPiercingAndResistancesInfo(base.activeCharacter);
		}
	}

	private void OnHoverOverResonancePower()
	{
		UIManager.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Resonance_Tooltip"));
	}

	private void OnHoverOutResonancePower()
	{
		UIManager.Instance.HideSmallInfo();
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using DG.Tweening;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Maccima_Games.Util;
using Ruinarch;
using Ruinarch.Custom_UI;
using TMPro;
using Traits;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UtilityScripts;

public class UIManager : BaseMonoBehaviour, SubGoalEventDispatcher.ISubGoalListener
{
	public bool useRayCast;

	public Action onSpireClicked;

	public Action onPrimordialPoolClicked;

	public static UIManager Instance;

	public const string normalTextColor = "#CEB67C";

	public const string greenTextColor = "#39FF14";

	public const string redTextColor = "#FF073A";

	public Canvas canvas;

	public CanvasScaler canvasScaler;

	public RectTransform smallInfoCanvasRT;

	public RectTransform canvasRectTransform;

	private InfoUIBase[] allMenus;

	[Space(10f)]
	[Header("Date Objects")]
	[SerializeField]
	private ToggleGroup speedToggleGroup;

	public Toggle pauseBtn;

	public Toggle x1Btn;

	public Toggle x2Btn;

	public Toggle x4Btn;

	[SerializeField]
	private TextMeshProUGUI dateLbl;

	[Space(10f)]
	[Header("Small Info")]
	public GameObject smallInfoGO;

	public RectTransform smallInfoRT;

	public HorizontalLayoutGroup smallInfoBGParentLG;

	public VerticalLayoutGroup smallInfoVerticalLG;

	public RectTransform smallInfoBGRT;

	public RuinarchText smallInfoLbl;

	public LocationSmallInfo locationSmallInfo;

	public RectTransform locationSmallInfoRT;

	public GameObject characterPortraitHoverInfoGO;

	public CharacterPortrait characterPortraitHoverInfo;

	public RectTransform characterPortraitHoverInfoRT;

	public Canvas smallInfoCanvas;

	[Header("Small Info with Visual")]
	[SerializeField]
	private SmallInfoWithVisual _smallInfoWithVisual;

	[Header("Character Nameplate Tooltip")]
	[SerializeField]
	private CharacterNameplateItem _characterNameplateTooltip;

	[Header("Tile Object Tooltip")]
	[SerializeField]
	private TileObjectNameplateItem _tileObjectNameplateTooltip;

	[Header("Tile Object Tooltip")]
	[SerializeField]
	private StructureNameplateItem _structureNameplateTooltip;

	[Header("Character Marker Nameplate")]
	public Transform characterMarkerNameplateParent;

	[Space(10f)]
	[Header("Shared")]
	[SerializeField]
	private GameObject cover;

	[Space(10f)]
	[Header("Object Picker")]
	[SerializeField]
	private ObjectPicker objectPicker;

	[Space(10f)]
	[Header("Right Click Commands")]
	public POITestingUI poiTestingUI;

	[Space(10f)]
	[Header("Psychopath")]
	public PsychopathUI psychopathUI;

	[Space(10f)]
	[Header("Custom Dropdown List")]
	public CustomDropdownList customDropdownList;

	[Space(10f)]
	[Header("Logs")]
	public LogTagSpriteDictionary logTagSpriteDictionary;

	[Space(10f)]
	[Header("Achievement")]
	public AchievementNotification _achievementNotification;

	private InfoUIBase _lastOpenedInfoUI;

	private PointerEventData _pointer;

	private List<RaycastResult> _raycastResults;

	private int _uiLayer;

	private int _worldUILayer;

	public List<UnallowOverlaps> unallowOverlaps = new List<UnallowOverlaps>();

	public List<DisallowOverlapWithCharacterPointer> disallowOverlapsWithCP = new List<DisallowOverlapWithCharacterPointer>();

	[Header("Options")]
	[SerializeField]
	private OptionsMenu _optionsMenu;

	[Header("Save Game")]
	[SerializeField]
	private GameObject _saveWritingToDiskGO;

	[Space(10f)]
	[Header("Character Info")]
	[SerializeField]
	internal CharacterInfoUI characterInfoUI;

	[FormerlySerializedAs("minionInfoUI")]
	[Space(10f)]
	[Header("Monster Info")]
	[SerializeField]
	internal MonsterInfoUI monsterInfoUI;

	[Space(10f)]
	[Header("Tile Object Info")]
	[SerializeField]
	internal TileObjectInfoUI tileObjectInfoUI;

	public Sprite tileObjectPortrait;

	[Space(10f)]
	[Header("Party Info")]
	[SerializeField]
	internal PartyInfoUI partyInfoUI;

	[Space(10f)]
	[Header("Structure Info")]
	[SerializeField]
	public StructureInfoUI structureInfoUI;

	[Space(10f)]
	[Header("Unbuilt Structure Info")]
	[SerializeField]
	public UnbuiltStructureInfoUI unbuiltStructureInfoUI;

	[Space(10f)]
	[Header("Structure Room Info")]
	public StructureRoomInfoUI structureRoomInfoUI;

	[Space(10f)]
	[Header("Settlement Info")]
	[SerializeField]
	public SettlementInfoUI settlementInfoUI;

	[Space(10f)]
	[Header("Console")]
	[SerializeField]
	internal ConsoleBase consoleUI;

	[Header("Conversation Menu")]
	[SerializeField]
	private ConversationMenu conversationMenu;

	[Header("Intel Notification")]
	[SerializeField]
	private GameObject intelPrefab;

	[SerializeField]
	private GameObject defaultNotificationPrefab;

	[SerializeField]
	private UIHoverPosition notificationHoverPos;

	[SerializeField]
	private Vector2 notificationHoverPosDefaultPosition;

	[SerializeField]
	private Vector2 notificationHoverPosModifiedPosition;

	[SerializeField]
	private GameObject searchFieldsParent;

	[SerializeField]
	private TMP_InputField notificationSearchField;

	[SerializeField]
	private GameObject searchFieldClearBtn;

	[SerializeField]
	private int maxPlayerNotif;

	[SerializeField]
	private LogFiltersWindow logFiltersWindow;

	[SerializeField]
	private GameObject notificationAreaMainContent;

	[SerializeField]
	private RuinarchButton showHideBtn;

	[SerializeField]
	private Image showHideImg;

	[SerializeField]
	private Sprite showSprite;

	[SerializeField]
	private Sprite hideSprite;

	[SerializeField]
	private Vector2 hidePos;

	[SerializeField]
	private Vector2 showPos;

	public ScrollRect playerNotifScrollRect;

	public List<PlayerNotificationItem> activeNotifications = new List<PlayerNotificationItem>();

	private List<string> activeNotificationIDs = new List<string>();

	[Header("Yes or No Confirmation")]
	public YesNoConfirmation yesNoConfirmation;

	private bool _yesNoPauseAndResume;

	[Header("Trigger Flaw Confirmation")]
	public GameObject triggerFlawGO;

	[SerializeField]
	private CanvasGroup triggerFlawCanvasGroup;

	[SerializeField]
	private GameObject triggerFlawCover;

	[SerializeField]
	private TextMeshProUGUI triggerFlawDescriptionLbl;

	[SerializeField]
	private TextMeshProUGUI triggerFlawEffectLbl;

	[SerializeField]
	private TextMeshProUGUI triggerFlawManaCostLbl;

	[SerializeField]
	private Button triggerFlawYesBtn;

	[SerializeField]
	private TextMeshProUGUI triggerFlawYesLbl;

	[SerializeField]
	private Button triggerFlawNoBtn;

	[SerializeField]
	private TextMeshProUGUI triggerFlawNoLbl;

	[SerializeField]
	private Button triggerFlawCloseBtn;

	[Header("General Confirmation")]
	public GeneralConfirmationWithVisual generalConfirmationWithVisual;

	[FormerlySerializedAs("_demoUI")]
	[Header("Popup Screens")]
	[SerializeField]
	private PopUpScreensUI popUpScreensUI;

	public InitialWorldSetupMenu initialWorldSetupMenu;

	[FormerlySerializedAs("_biolabUIController")]
	[Header("Biolab")]
	public BiolabUIController biolabUIController;

	[Header("Scheme")]
	[SerializeField]
	private SchemeUIController _schemeUIController;

	[Header("Context Menu")]
	public ContextMenuUIController contextMenuUIController;

	private bool _allowContextMenuInteractions = true;

	[Space(10f)]
	[Header("Demonic Structures")]
	[SerializeField]
	private PortalUIController _portalUIController;

	public UpgradePortalUIController upgradePortalUIController;

	public PurchaseSkillUIController purchaseSkillUIController;

	public GrudgeUIController grudgeUIController;

	public CriticalBreakUIController criticalBreakUIController;

	public SpawnPartyUIController spawnPartyUIController;

	public SkillUpgradeUIController skillUpgradeUIController;

	[Header("Wait Window")]
	[SerializeField]
	private GameObject waitWindow;

	[Header("Primordial Pool")]
	public PrimordialPoolUIController primordialPoolUIController;

	[Header("Gained Powers Popup")]
	[SerializeField]
	private GainedPowersPopup _gainedPowersPopup;

	[Header("Prism Events")]
	[SerializeField]
	private PrismEventsUI _prismEventsUI;

	[Header("Snatch Object")]
	public SnatchObjectUIController snatchObjectUIController;

	[Header("Sidebar")]
	public SidebarUIController sidebarUIController;

	[Header("Choose Skills Popup")]
	[SerializeField]
	private ChooseSkillsPopup _chooseSkillsPopup;

	private Dictionary<PortalUpgradeItem, PortalUpgradeTier> _itemsToShow = new Dictionary<PortalUpgradeItem, PortalUpgradeTier>(10);

	public int currentUpgradeIndex;

	public int totalUpgradesToShow;

	[Header("Sub Goal Notifications")]
	[SerializeField]
	private SubGoalNotificationUI _subGoalNotificationUI;

	public InfoUIBase latestOpenedInfoUI { get; private set; }

	public bool tempDisableShowInfoUI { get; private set; }

	public OptionsMenu optionsMenu => _optionsMenu;

	public List<PopupMenuBase> openedPopups { get; private set; }

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		_uiLayer = LayerMask.NameToLayer("UI");
		_worldUILayer = LayerMask.NameToLayer("WorldUI");
		Messenger.AddListener<bool>(UISignals.PAUSED, UpdateSpeedToggles);
		Messenger.AddListener(UISignals.UPDATE_UI, UpdateUI);
	}

	private void Update()
	{
		if (InputManager.Instance.GetMouseButtonDown(0) && IsContextMenuShowing() && !IsMouseOnContextMenu())
		{
			HideContextMenu();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Instance = null;
	}

	internal void InitializeUI()
	{
		_pointer = new PointerEventData(EventSystem.current);
		_raycastResults = new List<RaycastResult>();
		allMenus = base.transform.GetComponentsInChildren<InfoUIBase>(includeInactive: true);
		for (int i = 0; i < allMenus.Length; i++)
		{
			allMenus[i].Initialize();
		}
		openedPopups = new List<PopupMenuBase>();
		biolabUIController.Init(OnCloseBiolabUI);
		Messenger.AddListener<PROGRESSION_SPEED>(UISignals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
		Messenger.AddListener(UISignals.INTERACTION_MENU_OPENED, OnInteractionMenuOpened);
		Messenger.AddListener(UISignals.INTERACTION_MENU_CLOSED, OnInteractionMenuClosed);
		Messenger.AddListener<IIntel>(UISignals.SHOW_INTEL_NOTIFICATION, ShowPlayerNotification);
		Messenger.AddListener<Log>(UISignals.SHOW_PLAYER_NOTIFICATION, ShowPlayerNotification);
		Messenger.AddListener(UISignals.ON_OPEN_CONVERSATION_MENU, OnOpenConversationMenu);
		Messenger.AddListener(UISignals.ON_CLOSE_CONVERSATION_MENU, OnCloseShareIntelMenu);
		Messenger.AddListener(Signals.GAME_LOADED, OnGameLoaded);
		Messenger.AddListener<InfoUIBase>(UISignals.MENU_OPENED, OnUIMenuOpened);
		Messenger.AddListener<InfoUIBase>(UISignals.MENU_CLOSED, OnUIMenuClosed);
		Messenger.AddListener<PopupMenuBase>(UISignals.POPUP_MENU_OPENED, OnPopupMenuOpened);
		Messenger.AddListener<PopupMenuBase>(UISignals.POPUP_MENU_CLOSED, OnPopupMenuClosed);
		Messenger.AddListener<IPointOfInterest>(UISignals.UPDATE_POI_LOGS_UI, TryUpdatePOILog);
		Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_DESTROYED, OnStructureDestroyed);
		Messenger.AddListener<PlayerAction>(PlayerSkillSignals.PLAYER_ACTION_ACTIVATED, OnPlayerActionActivated);
		Messenger.AddListener<PlayerAction>(PlayerSkillSignals.PLAYER_ACTION_RESISTED, OnPlayerActionResisted);
		Messenger.AddListener<SHORTCUT_ACTION>(ControlsSignals.PLAYER_INPUT_ACTION, OnReceivePlayerInputAction);
		Messenger.AddListener<string>(UISignals.OBJECT_PICKER_SHOWN, OnObjectPickerShown);
		AddPlayerActionContextMenuSignals();
		InitializePlayerNotificationArea();
		contextMenuUIController.SetOnHoverOverAction(OnHoverOverPlayerActionContextMenuItem);
		contextMenuUIController.SetOnHoverOutAction(OnHoverOutPlayerActionContextMenuItem);
		optionsMenu.SubscribeListeners();
		showHideBtn.onClick.AddListener(OnClickShowHide);
		UpdateNotificationShowHideImage();
		UpdateUI();
	}

	public void InitializeAfterLoadOutPicked()
	{
		_portalUIController.InitializeAfterLoadoutSelected();
		upgradePortalUIController.InitializeAfterLoadoutSelected();
		purchaseSkillUIController.InitializeAfterLoadoutSelected();
		searchFieldsParent.gameObject.SetActive(value: true);
		logFiltersWindow.InitializeAfterLoadoutPicked();
		ListenToSubGoals();
	}

	private void OnPlayerActionActivated(PlayerAction p_playerAction)
	{
		if (p_playerAction.type == PLAYER_SKILL_TYPE.SEIZE_CHARACTER || p_playerAction.type == PLAYER_SKILL_TYPE.SEIZE_MONSTER || p_playerAction.type == PLAYER_SKILL_TYPE.SEIZE_OBJECT || p_playerAction.type == PLAYER_SKILL_TYPE.REMOVE_BUFF || p_playerAction.type == PLAYER_SKILL_TYPE.REMOVE_FLAW || p_playerAction.type == PLAYER_SKILL_TYPE.DESTROY || p_playerAction.type == PLAYER_SKILL_TYPE.DESTROY_EYE_WARD || p_playerAction.category == PLAYER_SKILL_CATEGORY.SCHEME || p_playerAction.category == PLAYER_SKILL_CATEGORY.RAID || p_playerAction.type == PLAYER_SKILL_TYPE.PSYCHOPATHY || p_playerAction.type == PLAYER_SKILL_TYPE.SNATCH_MONSTER || p_playerAction.type == PLAYER_SKILL_TYPE.SNATCH_VILLAGER || p_playerAction.type == PLAYER_SKILL_TYPE.RAID || p_playerAction.type == PLAYER_SKILL_TYPE.DEFEND || p_playerAction.type == PLAYER_SKILL_TYPE.EVANGELIZE || p_playerAction.type == PLAYER_SKILL_TYPE.UPGRADE_BEHOLDER_EYE_LEVEL || p_playerAction.type == PLAYER_SKILL_TYPE.UPGRADE_BEHOLDER_RADIUS_LEVEL || p_playerAction.type == PLAYER_SKILL_TYPE.DESTROY_STRUCTURE || p_playerAction.type == PLAYER_SKILL_TYPE.SNATCH_OBJECT || p_playerAction.type == PLAYER_SKILL_TYPE.ATTACK_VILLAGE || p_playerAction.type == PLAYER_SKILL_TYPE.CULTIST_POISON || p_playerAction.type == PLAYER_SKILL_TYPE.JOIN_FACTION || p_playerAction.type == PLAYER_SKILL_TYPE.UNDEPLOY_PARTY || p_playerAction.type == PLAYER_SKILL_TYPE.KILL_VILLAGER || p_playerAction.type == PLAYER_SKILL_TYPE.AGITATE || p_playerAction.type == PLAYER_SKILL_TYPE.TRIGGER_FLAW || p_playerAction.type == PLAYER_SKILL_TYPE.DRAIN_SPIRIT)
		{
			HideContextMenu();
		}
		else if (IsContextMenuShowing())
		{
			ForceReloadPlayerActions();
		}
	}

	private void OnPlayerActionResisted(PlayerAction p_playerAction)
	{
		if (p_playerAction.type == PLAYER_SKILL_TYPE.REMOVE_BUFF || p_playerAction.type == PLAYER_SKILL_TYPE.REMOVE_FLAW)
		{
			HideContextMenu();
		}
	}

	private void TryUpdatePOILog(IPointOfInterest poi)
	{
		if (poi is Character)
		{
			if (characterInfoUI.isShowing)
			{
				characterInfoUI.UpdateAllHistoryInfo();
			}
		}
		else if (poi is TileObject && tileObjectInfoUI.isShowing)
		{
			tileObjectInfoUI.UpdateLogs();
		}
	}

	private void OnGameLoaded()
	{
		UpdateUI();
		searchFieldsParent.gameObject.SetActive(SaveManager.Instance.useSaveData);
	}

	private void OnReceivePlayerInputAction(SHORTCUT_ACTION p_action)
	{
		switch (p_action)
		{
		case SHORTCUT_ACTION.Quick_Load:
			OnLoadHotkeyPressed();
			break;
		case SHORTCUT_ACTION.Show_Context_Menu:
			if (GameManager.Instance.gameHasStarted)
			{
				LocationGridTile tileFromMousePosition = InnerMapManager.Instance.GetTileFromMousePosition();
				IPointOfInterest currentlySelectedPOI = GetCurrentlySelectedPOI();
				if (tileFromMousePosition != null && InnerMapManager.Instance.GetFirstSelectableOnTile(tileFromMousePosition) is IPlayerActionTarget p_target)
				{
					ShowPlayerActionContextMenu(p_target, InputManager.Instance.mousePosition, p_isScreenPosition: true);
				}
				else if (currentlySelectedPOI != null && currentlySelectedPOI is IPlayerActionTarget p_target2)
				{
					ShowPlayerActionContextMenu(p_target2, InputManager.Instance.mousePosition, p_isScreenPosition: true);
				}
			}
			break;
		}
	}

	private void OnObjectPickerShown(string p_identifier)
	{
		HideContextMenu();
	}

	private void UpdateUI()
	{
		if (LocalizationSettings.SelectedLocale.Identifier.Code == "ja")
		{
			dateLbl.SetText(GameManager.Instance.continuousDays + " " + LocalizationManager.Capitalized_Day + "\n" + GameManager.Instance.ConvertTickToTime(GameManager.Instance.Today().tick));
		}
		else
		{
			dateLbl.SetText(LocalizationManager.Capitalized_Day + " " + GameManager.Instance.continuousDays + "\n" + GameManager.Instance.ConvertTickToTime(GameManager.Instance.Today().tick));
		}
		UpdateInteractableInfoUI();
		PlayerUI.Instance.UpdateUI();
	}

	private void UpdateInteractableInfoUI()
	{
		UpdateCharacterInfo();
		UpdateMonsterInfo();
		UpdateTileObjectInfo();
		UpdateStructureInfo();
		UpdateSettlementInfo();
		UpdatePartyInfo();
		UpdateUnbuiltStructureInfo();
	}

	private void UpdateSpeedToggles(bool isPaused)
	{
		if (base.gameObject.activeInHierarchy)
		{
			if (isPaused)
			{
				pauseBtn.isOn = true;
				speedToggleGroup.NotifyToggleOn(pauseBtn);
			}
			else if (GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X1)
			{
				x1Btn.isOn = true;
				speedToggleGroup.NotifyToggleOn(x1Btn);
			}
			else if (GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X2)
			{
				x2Btn.isOn = true;
				speedToggleGroup.NotifyToggleOn(x2Btn);
			}
			else if (GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X4)
			{
				x4Btn.isOn = true;
				speedToggleGroup.NotifyToggleOn(x4Btn);
			}
		}
	}

	private void OnProgressionSpeedChanged(PROGRESSION_SPEED speed)
	{
		UpdateSpeedToggles(GameManager.Instance.isPaused);
	}

	public void SetProgressionSpeed1X()
	{
		if (!GameManager.Instance.isPaused && GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X1)
		{
			PauseByPlayer();
		}
		else if (x1Btn.IsInteractable())
		{
			Unpause();
			GameManager.Instance.SetProgressionSpeed(PROGRESSION_SPEED.X1);
		}
	}

	public void SetProgressionSpeed2X()
	{
		if (!GameManager.Instance.isPaused && GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X2)
		{
			PauseByPlayer();
		}
		else if (x2Btn.IsInteractable())
		{
			Unpause();
			GameManager.Instance.SetProgressionSpeed(PROGRESSION_SPEED.X2);
		}
	}

	public void SetProgressionSpeed4X()
	{
		if (!GameManager.Instance.isPaused && GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X4)
		{
			PauseByPlayer();
		}
		else if (x4Btn.IsInteractable())
		{
			Unpause();
			GameManager.Instance.SetProgressionSpeed(PROGRESSION_SPEED.X4);
		}
	}

	public void PauseByPlayer()
	{
		if (pauseBtn.IsInteractable())
		{
			if (GameManager.Instance.isPaused)
			{
				Unpause();
				return;
			}
			Pause();
			Messenger.Broadcast(UISignals.PAUSED_BY_PLAYER);
		}
	}

	public void Pause()
	{
		GameManager.Instance.SetPausedState(isPaused: true);
	}

	public void Unpause()
	{
		GameManager.Instance.SetPausedState(isPaused: false);
	}

	public void SetSpeedTogglesState(bool state)
	{
		pauseBtn.interactable = state;
		x1Btn.interactable = state;
		x2Btn.interactable = state;
		x4Btn.interactable = state;
	}

	public void ResumeLastProgressionSpeed()
	{
		SetSpeedTogglesState(state: true);
		if (GameManager.Instance.lastProgressionBeforePausing == "paused")
		{
			Pause();
		}
		else if (GameManager.Instance.lastProgressionBeforePausing == "1")
		{
			SetProgressionSpeed1X();
		}
		else if (GameManager.Instance.lastProgressionBeforePausing == "2")
		{
			SetProgressionSpeed2X();
		}
		else if (GameManager.Instance.lastProgressionBeforePausing == "4")
		{
			SetProgressionSpeed4X();
		}
	}

	public void ToggleOptionsMenu()
	{
		if (_optionsMenu.isShowing)
		{
			_optionsMenu.Close();
		}
		else
		{
			_optionsMenu.Open();
		}
	}

	public bool IsOptionsMenuShowing()
	{
		return _optionsMenu.isShowing;
	}

	public void OpenOptionsMenu()
	{
		_optionsMenu.Open();
	}

	public void CloseOptionsMenu()
	{
		_optionsMenu.Close();
	}

	public void ShowSaveWritingToDisk()
	{
		_saveWritingToDiskGO.SetActive(value: true);
	}

	public void HideSaveWritingToDisk()
	{
		_saveWritingToDiskGO.SetActive(value: false);
	}

	public void ShowSmallInfo(string info, string header = "", bool autoReplaceText = true, TextOverflowModes textOverflowMode = TextOverflowModes.Overflow)
	{
		smallInfoGO.transform.SetAsLastSibling();
		string text = string.Empty;
		if (!string.IsNullOrEmpty(header))
		{
			text = "<b><size=18>" + header + "</b>\n";
		}
		text = text + "<line-height=70%><size=16>" + info;
		if (smallInfoLbl.overflowMode != textOverflowMode)
		{
			smallInfoLbl.overflowMode = textOverflowMode;
			if (smallInfoLbl.overflowMode == TextOverflowModes.Page)
			{
				smallInfoLbl.pageToDisplay = 1;
			}
		}
		smallInfoLbl.text = text;
		if (!IsSmallInfoShowing())
		{
			smallInfoGO.transform.SetParent(base.transform);
			smallInfoGO.SetActive(value: true);
		}
		PositionTooltip(smallInfoGO, smallInfoRT, smallInfoBGRT);
	}

	public void ShowSmallInfo(string info, UIHoverPosition pos, string header = "", bool autoReplaceText = true, bool relayout = false)
	{
		smallInfoGO.transform.SetAsLastSibling();
		string text = string.Empty;
		if (!string.IsNullOrEmpty(header))
		{
			text = "<b><size=18>" + header + "</b>\n";
		}
		text = text + "<line-height=70%><size=16>" + info;
		smallInfoLbl.text = text;
		PositionTooltip(pos, smallInfoGO, smallInfoRT);
		if (!IsSmallInfoShowing())
		{
			if (relayout)
			{
				smallInfoGO.SetActive(value: true);
			}
			else
			{
				smallInfoGO.SetActive(value: true);
			}
		}
	}

	public void HideSmallInfo()
	{
		if (IsSmallInfoShowing())
		{
			smallInfoGO.SetActive(value: false);
			_smallInfoWithVisual.Hide();
			if (smallInfoLbl.overflowMode == TextOverflowModes.Page)
			{
				smallInfoLbl.pageToDisplay = 1;
			}
		}
	}

	public bool IsSmallInfoShowing()
	{
		if (!(smallInfoGO != null) || !smallInfoGO.activeSelf)
		{
			if (_smallInfoWithVisual != null)
			{
				return _smallInfoWithVisual.gameObject.activeSelf;
			}
			return false;
		}
		return true;
	}

	public void PositionTooltip(GameObject tooltipParent, RectTransform rtToReposition, RectTransform boundsRT)
	{
		PositionTooltip(InputManager.Instance.mousePosition, tooltipParent, rtToReposition, boundsRT);
	}

	private void PositionTooltip(Vector3 position, GameObject tooltipParent, RectTransform rtToReposition, RectTransform boundsRT)
	{
		Vector3 newPos = position;
		if (tooltipParent.transform.parent != smallInfoCanvasRT)
		{
			tooltipParent.transform.SetParent(smallInfoCanvasRT);
		}
		if (tooltipParent.transform.localScale != Vector3.one)
		{
			tooltipParent.transform.localScale = Vector3.one;
		}
		rtToReposition.pivot = new Vector2(0f, 1f);
		RectTransform obj = tooltipParent.transform as RectTransform;
		obj.pivot = new Vector2(0f, 0f);
		Utilities.GetAnchorMinMax(TextAnchor.LowerLeft, out var anchorMin, out var anchorMax);
		obj.anchorMin = anchorMin;
		obj.anchorMax = anchorMax;
		smallInfoBGParentLG.childAlignment = TextAnchor.UpperLeft;
		if (InputManager.Instance.currentCursorType == Cursor_Type.Cross || InputManager.Instance.currentCursorType == Cursor_Type.Check || InputManager.Instance.currentCursorType == Cursor_Type.Link)
		{
			newPos.x += 100f;
			newPos.y -= 32f;
		}
		else
		{
			newPos.x += 25f;
			newPos.y -= 25f;
		}
		Vector3 vector = KeepFullyOnScreen(smallInfoBGRT, newPos, smallInfoCanvas, smallInfoCanvasRT);
		RectTransformUtility.ScreenPointToLocalPointInRectangle(smallInfoCanvasRT, vector, null, out var localPoint);
		(tooltipParent.transform as RectTransform).localPosition = localPoint;
	}

	private Vector3 KeepFullyOnScreen(RectTransform rect, Vector3 newPos, Canvas canvas, RectTransform CanvasRect)
	{
		float min = 0f;
		float scaleFactor = canvas.scaleFactor;
		float max = CanvasRect.sizeDelta.x * scaleFactor - rect.sizeDelta.x * scaleFactor;
		float min2 = rect.sizeDelta.y * scaleFactor;
		float max2 = CanvasRect.sizeDelta.y * scaleFactor;
		newPos.x = Mathf.Clamp(newPos.x, min, max);
		newPos.y = Mathf.Clamp(newPos.y, min2, max2);
		return newPos;
	}

	public void PositionTooltip(UIHoverPosition position, GameObject tooltipParent, RectTransform rt)
	{
		tooltipParent.transform.SetParent(position.transform);
		RectTransform obj = tooltipParent.transform as RectTransform;
		obj.pivot = position.pivot;
		Utilities.GetAnchorMinMax(position.anchor, out var anchorMin, out var anchorMax);
		obj.anchorMin = anchorMin;
		obj.anchorMax = anchorMax;
		obj.anchoredPosition = Vector2.zero;
		smallInfoBGParentLG.childAlignment = position.anchor;
		rt.pivot = position.pivot;
	}

	private bool IsSmallLocationInfoShowing()
	{
		return locationSmallInfoRT.gameObject.activeSelf;
	}

	public void ShowCharacterNameplateTooltip(Character character, UIHoverPosition position)
	{
		_characterNameplateTooltip.SetObject(character);
		_characterNameplateTooltip.gameObject.SetActive(value: true);
		_characterNameplateTooltip.SetPosition(position);
	}

	public void HideCharacterNameplateTooltip()
	{
		_characterNameplateTooltip.gameObject.SetActive(value: false);
	}

	public void ShowTileObjectNameplateTooltip(TileObject tileObject, UIHoverPosition position)
	{
		_tileObjectNameplateTooltip.SetObject(tileObject);
		_tileObjectNameplateTooltip.gameObject.SetActive(value: true);
		_tileObjectNameplateTooltip.SetPosition(position);
	}

	public void HideTileObjectNameplateTooltip()
	{
		_tileObjectNameplateTooltip.gameObject.SetActive(value: false);
	}

	public void ShowStructureNameplateTooltip(LocationStructure structure, UIHoverPosition position)
	{
		_structureNameplateTooltip.SetObject(structure);
		_structureNameplateTooltip.gameObject.SetActive(value: true);
		_structureNameplateTooltip.SetPosition(position);
	}

	public void HideStructureNameplateTooltip()
	{
		_structureNameplateTooltip.gameObject.SetActive(value: false);
	}

	private void OnUIMenuOpened(InfoUIBase menu)
	{
		latestOpenedInfoUI = menu;
	}

	private void OnUIMenuClosed(InfoUIBase menu)
	{
		if (latestOpenedInfoUI == menu)
		{
			latestOpenedInfoUI = null;
		}
	}

	private void OnPopupMenuOpened(PopupMenuBase menu)
	{
		if (!openedPopups.Contains(menu))
		{
			openedPopups.Add(menu);
		}
	}

	private void OnPopupMenuClosed(PopupMenuBase menu)
	{
		openedPopups.Remove(menu);
	}

	public void OpenObjectUI(object obj)
	{
		if (obj is Character character)
		{
			ShowCharacterInfo(character, centerOnCharacter: true);
		}
		else if (obj is NPCSettlement settlement)
		{
			ShowSettlementInfo(settlement);
		}
		else if (obj is Faction faction)
		{
			ShowFactionInfo(faction);
		}
		else if (obj is Minion minion)
		{
			ShowCharacterInfo(minion.character, centerOnCharacter: true);
		}
		else if (obj is Party party)
		{
			ShowPartyInfo(party);
		}
		else if (obj is TileObject tileObject)
		{
			ShowTileObjectInfo(tileObject);
		}
		else if (obj is LocationStructure { hasBeenDestroyed: false } locationStructure)
		{
			ShowStructureInfo(locationStructure);
		}
	}

	public bool IsMouseOnUI()
	{
		if (_pointer != null)
		{
			_pointer.position = InputManager.Instance.mousePosition;
			_raycastResults.Clear();
			EventSystem.current.RaycastAll(_pointer, _raycastResults);
			if (_raycastResults.Count > 0)
			{
				return _raycastResults.Any((RaycastResult go) => go.gameObject.layer == LayerMask.NameToLayer("UI") || go.gameObject.layer == LayerMask.NameToLayer("WorldUI") || go.gameObject.CompareTag("Map_Click_Blocker"));
			}
			return false;
		}
		return false;
	}

	private bool IsMouseOnContextMenu()
	{
		if (_pointer != null)
		{
			_pointer.position = InputManager.Instance.mousePosition;
			_raycastResults.Clear();
			EventSystem.current.RaycastAll(_pointer, _raycastResults);
			if (_raycastResults.Count > 0)
			{
				return _raycastResults.Any((RaycastResult go) => go.gameObject.CompareTag("Context Menu"));
			}
			return false;
		}
		return false;
	}

	public void SetCoverState(bool state, bool blockClicks = true)
	{
		cover.SetActive(state);
		cover.GetComponent<Image>().raycastTarget = blockClicks;
	}

	private void OnInteractionMenuOpened()
	{
		if (characterInfoUI.isShowing)
		{
			_lastOpenedInfoUI = characterInfoUI;
		}
		if (characterInfoUI.isShowing)
		{
			characterInfoUI.gameObject.SetActive(value: false);
		}
	}

	private void OnInteractionMenuClosed()
	{
		if (_lastOpenedInfoUI != null)
		{
			_lastOpenedInfoUI.OpenMenu();
			_lastOpenedInfoUI = null;
		}
	}

	public void SetTempDisableShowInfoUI(bool state)
	{
		tempDisableShowInfoUI = state;
	}

	public Character GetCurrentlySelectedCharacter()
	{
		if (characterInfoUI.isShowing)
		{
			return characterInfoUI.activeCharacter;
		}
		if (monsterInfoUI.isShowing)
		{
			return monsterInfoUI.activeMonster;
		}
		return null;
	}

	public IPointOfInterest GetCurrentlySelectedPOI()
	{
		if (characterInfoUI.isShowing)
		{
			return characterInfoUI.activeCharacter;
		}
		if (monsterInfoUI.isShowing)
		{
			return monsterInfoUI.activeMonster;
		}
		if (tileObjectInfoUI.isShowing)
		{
			return tileObjectInfoUI.activeTileObject;
		}
		return null;
	}

	public object GetCurrentlySelectedObject()
	{
		IPointOfInterest currentlySelectedPOI = GetCurrentlySelectedPOI();
		if (currentlySelectedPOI == null)
		{
			if (settlementInfoUI.isShowing)
			{
				return settlementInfoUI.activeSettlement;
			}
			if (structureInfoUI.isShowing)
			{
				return structureInfoUI.activeStructure;
			}
			if (structureRoomInfoUI.isShowing)
			{
				return structureRoomInfoUI.activeRoom;
			}
			if (partyInfoUI.isShowing)
			{
				return partyInfoUI.activeParty;
			}
		}
		return currentlySelectedPOI;
	}

	public void CenterCurrentlySelectedObject()
	{
		if (characterInfoUI.isShowing)
		{
			characterInfoUI.activeCharacter.CenterOnCharacter();
		}
		else if (monsterInfoUI.isShowing)
		{
			monsterInfoUI.activeMonster.CenterOnCharacter();
		}
		else if (tileObjectInfoUI.isShowing)
		{
			tileObjectInfoUI.activeTileObject.CenterOnTileObject();
		}
		else if (settlementInfoUI.isShowing)
		{
			if (settlementInfoUI.activeSettlement is NPCSettlement nPCSettlement)
			{
				nPCSettlement.mainStorage.CenterOnStructure();
			}
		}
		else if (structureInfoUI.isShowing)
		{
			structureInfoUI.activeStructure.CenterOnStructure();
		}
		else if (structureRoomInfoUI.isShowing)
		{
			structureRoomInfoUI.activeRoom.parentStructure.CenterOnStructure();
		}
		else if (partyInfoUI.isShowing)
		{
			partyInfoUI.activeParty.CenterOnParty();
		}
	}

	internal GameObject InstantiateUIObject(string prefabObjName, Transform parent)
	{
		return ObjectPoolManager.Instance.InstantiateObjectFromPool(prefabObjName, Vector3.zero, Quaternion.identity, parent);
	}

	public void ShowClickableObjectPicker<T>(List<T> choices, Action<object> onClickAction, IComparer<T> comparer = null, Func<T, bool> validityChecker = null, string title = "", Action<T> onHoverAction = null, Action<T> onHoverExitAction = null, string identifier = "", bool showCover = false, int layer = 9, bool closable = true, Func<string, Sprite> portraitGetter = null, bool shouldShowConfirmationWindowOnPick = false, bool asButton = false)
	{
		objectPicker.ShowClickable(choices, onClickAction, comparer, validityChecker, title, onHoverAction, onHoverExitAction, identifier, showCover, layer, portraitGetter, asButton, shouldShowConfirmationWindowOnPick);
		Messenger.Broadcast(UISignals.OBJECT_PICKER_SHOWN, identifier);
	}

	public void HideObjectPicker()
	{
		objectPicker.Close();
	}

	public bool IsObjectPickerOpen()
	{
		return objectPicker.gameObject.activeSelf;
	}

	public void SetUIState(bool state)
	{
		base.gameObject.SetActive(state);
		Messenger.Broadcast(UISignals.UI_STATE_SET);
	}

	public void DateHover()
	{
		ShowSmallInfo("Day: " + GameManager.Instance.continuousDays + " Tick: " + GameManager.Instance.Today().tick);
	}

	[ContextMenu("Set All Scroll Rect Scroll Speed")]
	public void SetAllScrollSpeed()
	{
		ScrollRect[] componentsInChildren = base.gameObject.GetComponentsInChildren<ScrollRect>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].scrollSensitivity = 25f;
		}
	}

	public void ShowFactionInfo(Faction faction)
	{
		if (tempDisableShowInfoUI)
		{
			SetTempDisableShowInfoUI(state: false);
		}
		else
		{
			FactionInfoHubUI.Instance.ShowFaction(faction);
		}
	}

	public void ShowCharacterInfo(Character character, bool centerOnCharacter = false)
	{
		Character character2 = character;
		if (character.isLycanthrope)
		{
			character2 = character.lycanData.activeForm;
		}
		if (character2 == null || !character2.hasMarker)
		{
			return;
		}
		if (character2.isNormalCharacter)
		{
			if (tempDisableShowInfoUI)
			{
				SetTempDisableShowInfoUI(state: false);
				return;
			}
			characterInfoUI.SetData(character2);
			characterInfoUI.OpenMenu();
			if (centerOnCharacter)
			{
				character2.CenterOnCharacter();
			}
		}
		else
		{
			ShowMonsterInfo(character2, centerOnCharacter);
		}
		if (IsContextMenuShowing() && !IsContextMenuShowingForTarget(character2))
		{
			RefreshPlayerActionContextMenuWithNewTarget(character2);
		}
	}

	public void UpdateCharacterInfo()
	{
		if (characterInfoUI.isShowing)
		{
			characterInfoUI.TryUpdateCharacterInfo();
		}
	}

	private void ShowMonsterInfo(Character character, bool centerOnCharacter = false)
	{
		if (tempDisableShowInfoUI)
		{
			SetTempDisableShowInfoUI(state: false);
			return;
		}
		monsterInfoUI.SetData(character);
		monsterInfoUI.OpenMenu();
		if (centerOnCharacter)
		{
			character.CenterOnCharacter();
		}
	}

	private void UpdateMonsterInfo()
	{
		if (monsterInfoUI.isShowing)
		{
			monsterInfoUI.TryUpdateCharacterInfo();
		}
	}

	public void ShowTileObjectInfo(TileObject tileObject)
	{
		if (tempDisableShowInfoUI)
		{
			SetTempDisableShowInfoUI(state: false);
			return;
		}
		tileObjectInfoUI.SetData(tileObject);
		tileObjectInfoUI.OpenMenu();
	}

	public void UpdateTileObjectInfo()
	{
		if (tileObjectInfoUI.isShowing)
		{
			tileObjectInfoUI.UpdateTileObjectInfo();
		}
	}

	public void ShowPartyInfo(Party party)
	{
		if (tempDisableShowInfoUI)
		{
			SetTempDisableShowInfoUI(state: false);
			return;
		}
		partyInfoUI.SetData(party);
		partyInfoUI.OpenMenu();
	}

	public void UpdatePartyInfo()
	{
		if (partyInfoUI.isShowing)
		{
			partyInfoUI.UpdatePartyInfo();
		}
	}

	public void ShowStructureInfo(LocationStructure structure, bool centerOnStructure = true)
	{
		if (tempDisableShowInfoUI)
		{
			SetTempDisableShowInfoUI(state: false);
			return;
		}
		structureInfoUI.SetData(structure);
		structureInfoUI.OpenMenu();
		if (centerOnStructure)
		{
			structure.CenterOnStructure();
		}
	}

	private void UpdateStructureInfo()
	{
		if (structureInfoUI.isShowing)
		{
			structureInfoUI.UpdateStructureInfoUI();
		}
	}

	private void OnStructureDestroyed(LocationStructure structure)
	{
		CheckStructureInfoForClosure(structure);
	}

	private void CheckStructureInfoForClosure(LocationStructure structure)
	{
		if (structureInfoUI.isShowing && structureInfoUI.activeStructure == structure)
		{
			structureInfoUI.CloseMenu();
		}
	}

	public void ShowUnbuiltStructureInfo(LocationStructureObject p_structureObject)
	{
		if (tempDisableShowInfoUI)
		{
			SetTempDisableShowInfoUI(state: false);
			return;
		}
		unbuiltStructureInfoUI.SetData(p_structureObject);
		unbuiltStructureInfoUI.OpenMenu();
	}

	private void UpdateUnbuiltStructureInfo()
	{
		if (unbuiltStructureInfoUI.isShowing)
		{
			unbuiltStructureInfoUI.UpdateUnbuiltStructureInfoUI();
		}
	}

	public void ShowStructureRoomInfo(StructureRoom room)
	{
		if (tempDisableShowInfoUI)
		{
			SetTempDisableShowInfoUI(state: false);
			return;
		}
		structureRoomInfoUI.SetData(room);
		structureRoomInfoUI.OpenMenu();
	}

	public void UpdateStructureRoomInfo()
	{
		if (structureRoomInfoUI.isShowing)
		{
			structureRoomInfoUI.UpdateInfo();
		}
	}

	public void ShowSettlementInfo(BaseSettlement settlement)
	{
		if (settlement.locationType != LOCATION_TYPE.VILLAGE)
		{
			if (settlement.allStructures.Count > 0)
			{
				ShowStructureInfo(settlement.allStructures.First());
			}
		}
		else if (tempDisableShowInfoUI)
		{
			SetTempDisableShowInfoUI(state: false);
		}
		else
		{
			settlementInfoUI.SetData(settlement);
			settlementInfoUI.OpenMenu();
		}
	}

	public void UpdateSettlementInfo()
	{
		if (settlementInfoUI.isShowing)
		{
			settlementInfoUI.UpdateSettlementInfoUI();
		}
	}

	public bool IsConsoleShowing()
	{
		return consoleUI.isShowing;
	}

	public void ToggleConsole()
	{
		if (consoleUI.isShowing)
		{
			HideConsole();
		}
		else
		{
			ShowConsole();
		}
	}

	public void ShowConsole()
	{
		consoleUI.ShowConsole();
	}

	public void HideConsole()
	{
		consoleUI.HideConsole();
	}

	public void OpenConversationMenu(List<ConversationData> conversationList, string titleText)
	{
		conversationMenu.Open(conversationList, titleText);
	}

	public bool IsConversationMenuOpen()
	{
		return conversationMenu.gameObject.activeSelf;
	}

	private void OnOpenConversationMenu()
	{
		SetCoverState(state: true);
	}

	private void OnCloseShareIntelMenu()
	{
		SetCoverState(state: false);
	}

	private void InitializePlayerNotificationArea()
	{
		logFiltersWindow.Initialize();
		notificationSearchField.onValueChanged.AddListener(OnEndNotificationSearchEdit);
		logFiltersWindow.AddOnToggleActionOfTypeFilterToggles(OnToggleTypeFilter);
		logFiltersWindow.AddOnToggleActionOfShowAllToggle(OnToggleAllTypeFilters);
		logFiltersWindow.AddOnToggleVillageFilterAction(OnToggleVillageFilter);
		logFiltersWindow.AddOnToggleActionOfShowAllVillagesToggle(OnToggleAllVillageFilter);
	}

	private void ShowPlayerNotification(IIntel intel)
	{
		GameObject obj = ObjectPoolManager.Instance.InstantiateObjectFromPool(intelPrefab.name, Vector3.zero, Quaternion.identity, playerNotifScrollRect.content);
		IntelNotificationItem component = obj.GetComponent<IntelNotificationItem>();
		component.Initialize(intel, OnNotificationDestroyed);
		component.SetHoverPosition(notificationHoverPos);
		obj.transform.localScale = Vector3.one;
		PlaceNewNotification(component, intel.log);
	}

	private void ShowPlayerNotification(Log log)
	{
		GameObject obj = ObjectPoolManager.Instance.InstantiateObjectFromPool(defaultNotificationPrefab.name, Vector3.zero, Quaternion.identity, playerNotifScrollRect.content);
		PlayerNotificationItem component = obj.GetComponent<PlayerNotificationItem>();
		component.Initialize(log, OnNotificationDestroyed);
		component.SetHoverPosition(notificationHoverPos);
		obj.transform.localScale = Vector3.one;
		PlaceNewNotification(component, log);
	}

	public void ShowPlayerNotification(in Log log, int tick)
	{
		GameObject obj = ObjectPoolManager.Instance.InstantiateObjectFromPool(defaultNotificationPrefab.name, Vector3.zero, Quaternion.identity, playerNotifScrollRect.content);
		PlayerNotificationItem component = obj.GetComponent<PlayerNotificationItem>();
		component.Initialize(log, tick, OnNotificationDestroyed);
		component.SetHoverPosition(notificationHoverPos);
		obj.transform.localScale = Vector3.one;
		PlaceNewNotification(component, log);
	}

	public void ShowPlayerNotification(IIntel intel, in Log log, int tick)
	{
		GameObject obj = ObjectPoolManager.Instance.InstantiateObjectFromPool(intelPrefab.name, Vector3.zero, Quaternion.identity, playerNotifScrollRect.content);
		IntelNotificationItem component = obj.GetComponent<IntelNotificationItem>();
		component.Initialize(intel, OnNotificationDestroyed);
		component.SetHoverPosition(notificationHoverPos);
		obj.transform.localScale = Vector3.one;
		PlaceNewNotification(component, log);
	}

	private void PlaceNewNotification(PlayerNotificationItem newNotif, Log shownLog)
	{
		activeNotifications.Add(newNotif);
		activeNotificationIDs.Add(shownLog.persistentID);
		if (activeNotifications.Count > maxPlayerNotif)
		{
			activeNotifications[0].DeleteOldestNotification();
		}
		if (ShouldNotificationsBeFiltered())
		{
			List<string> list = ((!logFiltersWindow.HasUntoggledVillageFilter()) ? DatabaseManager.Instance.mainSQLDatabase.GetLogIDsThatMatchCriteria(activeNotificationIDs, notificationSearchField.text, logFiltersWindow.enabledFilters) : DatabaseManager.Instance.mainSQLDatabase.GetLogIDsThatMatchCriteria(activeNotificationIDs, logFiltersWindow.enabledVillageFilters, notificationSearchField.text, logFiltersWindow.enabledFilters));
			if (list != null && list.Contains(newNotif.logPersistentID))
			{
				newNotif.DoTweenHeight();
				newNotif.TweenIn();
			}
			else
			{
				newNotif.QueueAdjustHeightOnEnable();
			}
			FilterNotifications(list);
		}
		else
		{
			newNotif.DoTweenHeight();
			newNotif.TweenIn();
		}
	}

	private void OnNotificationDestroyed(PlayerNotificationItem item)
	{
		activeNotifications.Remove(item);
		activeNotificationIDs.Remove(item.logPersistentID);
	}

	private void FilterNotifications(List<string> filteredLogIDs = null)
	{
		if (filteredLogIDs == null)
		{
			filteredLogIDs = ((!logFiltersWindow.HasUntoggledVillageFilter()) ? DatabaseManager.Instance.mainSQLDatabase.GetLogIDsThatMatchCriteria(activeNotificationIDs, notificationSearchField.text, logFiltersWindow.enabledFilters) : DatabaseManager.Instance.mainSQLDatabase.GetLogIDsThatMatchCriteria(activeNotificationIDs, logFiltersWindow.enabledVillageFilters, notificationSearchField.text, logFiltersWindow.enabledFilters));
		}
		for (int i = 0; i < activeNotifications.Count; i++)
		{
			PlayerNotificationItem playerNotificationItem = activeNotifications[i];
			if (filteredLogIDs != null && filteredLogIDs.Contains(playerNotificationItem.logPersistentID))
			{
				playerNotificationItem.gameObject.SetActive(value: true);
				playerNotificationItem.transform.SetSiblingIndex(i);
			}
			else
			{
				playerNotificationItem.gameObject.SetActive(value: false);
			}
		}
	}

	private bool ShouldNotificationsBeFiltered()
	{
		if (string.IsNullOrEmpty(notificationSearchField.text) && (logFiltersWindow.enabledFilters.Count <= 0 || logFiltersWindow.enabledFilters.Count >= DatabaseManager.Instance.mainSQLDatabase.allLogTags.Count) && logFiltersWindow.enabledFilters.Count != 0)
		{
			return logFiltersWindow.HasUntoggledVillageFilter();
		}
		return true;
	}

	private void OnEndNotificationSearchEdit(string text)
	{
		searchFieldClearBtn.gameObject.SetActive(!string.IsNullOrEmpty(text));
		FilterNotifications();
	}

	public void ToggleFilters()
	{
		logFiltersWindow.ToggleFilters();
	}

	private void OnToggleTypeFilter(bool isOn, LOG_TAG tag)
	{
		FilterNotifications();
	}

	private void OnToggleAllTypeFilters(bool state)
	{
		FilterNotifications();
	}

	private void OnToggleVillageFilter(bool isOn, NPCSettlement p_settlement)
	{
		FilterNotifications();
	}

	private void OnToggleAllVillageFilter(bool isOn)
	{
		FilterNotifications();
	}

	private void OnClickShowHide()
	{
		RectTransform obj = notificationAreaMainContent.transform as RectTransform;
		obj.anchoredPosition = ((obj.anchoredPosition == showPos) ? hidePos : showPos);
		UpdateNotificationShowHideImage();
	}

	private void UpdateNotificationShowHideImage()
	{
		RectTransform rectTransform = notificationAreaMainContent.transform as RectTransform;
		showHideImg.sprite = ((rectTransform.anchoredPosition == showPos) ? showSprite : hideSprite);
	}

	public void ShowYesNoConfirmation(string header, string question, Action onClickYesAction = null, Action onClickNoAction = null, bool showCover = false, int layer = 21, string yesBtnText = "Yes", string noBtnText = "No", bool yesBtnInteractable = true, bool noBtnInteractable = true, bool pauseAndResume = false, bool yesBtnActive = true, bool noBtnActive = true, Action yesBtnInactiveHoverAction = null, Action yesBtnInactiveHoverExitAction = null, Action onClickCloseAction = null, Action onHideUIAction = null)
	{
		if (PlayerUI.Instance.IsMajorUIShowing())
		{
			PlayerUI.Instance.AddPendingUI(delegate
			{
				ShowYesNoConfirmation(header, question, onClickYesAction, onClickNoAction, showCover, layer, yesBtnText, noBtnText, yesBtnInteractable, noBtnInteractable, pauseAndResume, yesBtnActive, noBtnActive, yesBtnInactiveHoverAction, yesBtnInactiveHoverExitAction);
			});
			return;
		}
		_yesNoPauseAndResume = pauseAndResume;
		if (_yesNoPauseAndResume && !IsObjectPickerOpen())
		{
			Pause();
			SetSpeedTogglesState(state: false);
		}
		yesNoConfirmation.ShowYesNoConfirmation(header, question, onClickYesAction, onClickNoAction, showCover, layer, yesBtnText, noBtnText, yesBtnInteractable, noBtnInteractable, yesBtnActive, noBtnActive, yesBtnInactiveHoverAction, yesBtnInactiveHoverExitAction, onClickCloseAction, onHideUIAction);
	}

	public void HideYesNoConfirmation()
	{
		yesNoConfirmation.Close();
		if (!PlayerUI.Instance.TryShowPendingUI() && !IsObjectPickerOpen() && !optionsMenu.isShowing && _yesNoPauseAndResume)
		{
			ResumeLastProgressionSpeed();
		}
	}

	private void TweenIn(CanvasGroup canvasGroup)
	{
		canvasGroup.alpha = 0f;
		RectTransform rectTransform = canvasGroup.transform as RectTransform;
		rectTransform.anchoredPosition = new Vector2(0f, -30f);
		Sequence sequence = DOTween.Sequence();
		sequence.Append(rectTransform.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutBack));
		sequence.Join(DOTween.To(() => canvasGroup.alpha, delegate(float x)
		{
			canvasGroup.alpha = x;
		}, 1f, 0.5f).SetEase(Ease.InSine));
		sequence.PrependInterval(0.2f);
		sequence.Play();
	}

	public void ShowTriggerFlawConfirmation(string question, string effect, string manaCost, Action onClickYesAction = null, bool showCover = false, int layer = 21, bool pauseAndResume = false)
	{
		if (PlayerUI.Instance.IsMajorUIShowing())
		{
			PlayerUI.Instance.AddPendingUI(delegate
			{
				ShowTriggerFlawConfirmation(question, effect, manaCost, onClickYesAction, showCover, layer, pauseAndResume);
			});
			return;
		}
		InputManager.Instance.SetInputMapState("Confirmation Window", p_state: true);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Cancel, p_state: false);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Confirmation_Window_Confirm, p_state: true);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Confirmation_Window_Cancel, p_state: true);
		InputManager.Instance.SetGamepadCursorState(p_state: false);
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Yes");
		string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "No");
		triggerFlawYesLbl.text = localizedValue;
		triggerFlawNoLbl.text = localizedValue2;
		if (pauseAndResume && !IsObjectPickerOpen())
		{
			Pause();
			SetSpeedTogglesState(state: false);
		}
		HideContextMenu();
		triggerFlawDescriptionLbl.text = question;
		triggerFlawEffectLbl.text = effect;
		triggerFlawManaCostLbl.text = manaCost;
		triggerFlawYesBtn.onClick.RemoveAllListeners();
		triggerFlawNoBtn.onClick.RemoveAllListeners();
		triggerFlawCloseBtn.onClick.RemoveAllListeners();
		triggerFlawYesBtn.onClick.AddListener(HideTriggerFlawConfirmation);
		triggerFlawNoBtn.onClick.AddListener(HideTriggerFlawConfirmation);
		triggerFlawCloseBtn.onClick.AddListener(HideTriggerFlawConfirmation);
		if (onClickYesAction != null)
		{
			triggerFlawYesBtn.onClick.AddListener(onClickYesAction.Invoke);
		}
		triggerFlawGO.SetActive(value: true);
		triggerFlawGO.transform.SetSiblingIndex(layer);
		triggerFlawCover.SetActive(showCover);
		TweenIn(triggerFlawCanvasGroup);
	}

	private void HideTriggerFlawConfirmation()
	{
		triggerFlawGO.SetActive(value: false);
		InputManager.Instance.SetInputMapState("Confirmation Window", p_state: false);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Cancel, p_state: true);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Confirmation_Window_Confirm, p_state: false);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Confirmation_Window_Cancel, p_state: false);
		InputManager.Instance.SetGamepadCursorState(p_state: true);
		if (!PlayerUI.Instance.TryShowPendingUI() && !IsObjectPickerOpen())
		{
			ResumeLastProgressionSpeed();
		}
	}

	public void ShowStartScenario(string message)
	{
		popUpScreensUI.ShowStartScreen(message);
	}

	public void ShowEndDemoScreen(string summary, bool p_isGameWin)
	{
		popUpScreensUI.ShowSummaryThenEndScreen(summary, p_isGameWin);
	}

	public bool IsShowingEndScreen()
	{
		return popUpScreensUI.IsShowingEndScreen();
	}

	public bool IsShowingStartScreen()
	{
		return popUpScreensUI.IsShowingStartScreen();
	}

	public Sprite GetLogTagSprite(LOG_TAG tag)
	{
		if (logTagSpriteDictionary.ContainsKey(tag))
		{
			return logTagSpriteDictionary[tag];
		}
		throw new Exception("No Log tag sprite for tag " + tag);
	}

	public void AddUnallowOverlapUI(UnallowOverlaps overlap)
	{
		unallowOverlaps.Add(overlap);
	}

	public bool DoesUIOverlap(UnallowOverlaps overlap)
	{
		return GetOverlappedUI(overlap) != null;
	}

	public UnallowOverlaps GetOverlappedUI(UnallowOverlaps overlap)
	{
		for (int i = 0; i < this.unallowOverlaps.Count; i++)
		{
			UnallowOverlaps unallowOverlaps = this.unallowOverlaps[i];
			if (unallowOverlaps != overlap && unallowOverlaps.gameObject.activeInHierarchy && unallowOverlaps.rectTransform.RectOverlaps(overlap.rectTransform))
			{
				return unallowOverlaps;
			}
		}
		return null;
	}

	public void AddDisallowOverlapWithCPUI(DisallowOverlapWithCharacterPointer p_arg)
	{
		disallowOverlapsWithCP.Add(p_arg);
	}

	public DisallowOverlapWithCharacterPointer GetOverlappedUIWithCharacterPointer(RectTransform p_pointer)
	{
		for (int i = 0; i < disallowOverlapsWithCP.Count; i++)
		{
			DisallowOverlapWithCharacterPointer disallowOverlapWithCharacterPointer = disallowOverlapsWithCP[i];
			if (disallowOverlapWithCharacterPointer.rectTransform != p_pointer && disallowOverlapWithCharacterPointer.gameObject.activeInHierarchy && disallowOverlapWithCharacterPointer.rectTransform.RectOverlaps(p_pointer))
			{
				return disallowOverlapWithCharacterPointer;
			}
		}
		return null;
	}

	public void ShowBiolabUI()
	{
		Pause();
		SetSpeedTogglesState(state: false);
		biolabUIController.Open();
		InputManager.Instance.SetAllHotkeysEnabledState(p_state: false);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Cancel, p_state: true);
	}

	private void OnCloseBiolabUI()
	{
		SetSpeedTogglesState(state: true);
		ResumeLastProgressionSpeed();
		InputManager.Instance.SetAllHotkeysEnabledState(p_state: true);
	}

	public void ShowSchemeUI(Character p_targetCharacter, object p_otherTarget, SchemeData p_schemeUsed)
	{
		Pause();
		SetSpeedTogglesState(state: false);
		_schemeUIController.Show(p_targetCharacter, p_otherTarget, p_schemeUsed, OnCloseSchemeUI);
		InputManager.Instance.SetAllHotkeysEnabledState(p_state: false);
		Messenger.Broadcast(UISignals.SCHEME_UI_SHOWN);
	}

	private void OnCloseSchemeUI()
	{
		SetSpeedTogglesState(state: true);
		ResumeLastProgressionSpeed();
		InputManager.Instance.SetAllHotkeysEnabledState(p_state: true);
	}

	public void ShowPlayerActionContextMenu(IPlayerActionTarget p_target, Vector3 p_followTarget, bool p_isScreenPosition)
	{
		if (_allowContextMenuInteractions)
		{
			PlayerManager.Instance.player.SetCurrentPlayerActionTarget(p_target);
			List<IContextMenuItem> list = RuinarchListPool<IContextMenuItem>.Claim();
			PopulatePlayerActionContextMenuItems(list, p_target);
			contextMenuUIController.SetFollowPosition(p_followTarget, p_isScreenPosition);
			contextMenuUIController.ShowContextMenu(list, InputManager.Instance.mousePosition, p_target.name, InputManager.Instance.currentCursorType);
			Messenger.Broadcast(UISignals.PLAYER_ACTION_CONTEXT_MENU_SHOWN, p_target);
			RuinarchListPool<IContextMenuItem>.Release(list);
		}
	}

	public void ShowContextMenu(List<IContextMenuItem> p_target, Vector3 p_followTarget, bool p_isScreenPosition, string p_menuTitle)
	{
		if (_allowContextMenuInteractions)
		{
			contextMenuUIController.SetFollowPosition(p_followTarget, p_isScreenPosition);
			contextMenuUIController.ShowContextMenu(p_target, InputManager.Instance.mousePosition, p_menuTitle, InputManager.Instance.currentCursorType);
		}
	}

	public void RefreshPlayerActionContextMenuWithNewTarget(IPlayerActionTarget p_target)
	{
		if (_allowContextMenuInteractions)
		{
			PlayerManager.Instance.player.SetCurrentPlayerActionTarget(p_target);
			List<IContextMenuItem> list = RuinarchListPool<IContextMenuItem>.Claim();
			PopulatePlayerActionContextMenuItems(list, p_target);
			contextMenuUIController.ShowContextMenu(list, p_target.name);
			Messenger.Broadcast(UISignals.PLAYER_ACTION_CONTEXT_MENU_SHOWN, p_target);
			RuinarchListPool<IContextMenuItem>.Release(list);
		}
	}

	public void DisableContextMenuInteractions()
	{
		_allowContextMenuInteractions = false;
	}

	public void EnableContextMenuInteractions()
	{
		_allowContextMenuInteractions = true;
	}

	public void HideContextMenu()
	{
		PlayerManager.Instance.player.SetCurrentPlayerActionTarget(null);
		contextMenuUIController.HideUI();
	}

	public bool IsContextMenuShowing()
	{
		return contextMenuUIController.IsShowing();
	}

	public bool IsContextMenuShowingForTarget(IPlayerActionTarget p_target)
	{
		if (IsContextMenuShowing())
		{
			return PlayerManager.Instance.player.currentlySelectedPlayerActionTarget == p_target;
		}
		return false;
	}

	private void OnHoverOverPlayerActionContextMenuItem(IContextMenuItem p_item, UIHoverPosition p_hoverPosition)
	{
		if (p_item is PlayerAction spellData)
		{
			OnHoverPlayerAction(spellData, p_hoverPosition, PlayerManager.Instance.player.currentlySelectedPlayerActionTarget);
		}
		else if (p_item is Trait trait && PlayerManager.Instance.player.currentlySelectedPlayerActionTarget is Character p_character)
		{
			if (contextMenuUIController.currentlyOpenedParentContextItem is TriggerFlawData)
			{
				OnHoverEnterFlaw(trait.name, p_character, p_hoverPosition);
			}
			else if (contextMenuUIController.currentlyOpenedParentContextItem is RemoveBuffData || contextMenuUIController.currentlyOpenedParentContextItem is RemoveFlawData)
			{
				OnHoverEnterRemoveBuff(trait.name, p_character, p_hoverPosition);
			}
		}
		else if (p_item is ShareIntelContextMenuItem shareIntelContextMenuItem)
		{
			if (!shareIntelContextMenuItem.CanBePickedRegardlessOfCooldown())
			{
				ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "No_Stored_Characters"), p_hoverPosition);
			}
		}
		else if (p_item is InfuseChoice infuseChoice && PlayerManager.Instance.player.currentlySelectedPlayerActionTarget is TileObject p_tileObject)
		{
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("Traits_Table", infuseChoice.name + "_Description");
			string additionalText = string.Empty;
			if (!infuseChoice.CanBePickedRegardlessOfCooldown())
			{
				additionalText = Utilities.ColorizeInvalidText(infuseChoice.GetReasonsWhyCannotPerformAbilityTowards(p_tileObject));
			}
			PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(infuseChoice.localizedName, localizedValue, p_hoverPosition, autoReplaceText: true, additionalText);
		}
		else
		{
			if (!(p_item is Character character))
			{
				return;
			}
			if (contextMenuUIController.currentlyOpenedParentContextItem is TriggerGrudgeData)
			{
				OnHoverEnterGrudge(character, p_hoverPosition);
			}
			else if (contextMenuUIController.currentlyOpenedParentContextItem is LureData lureData)
			{
				if (PlayerManager.Instance.player.currentlySelectedPlayerActionTarget is TileObject tileObject)
				{
					string text = string.Empty;
					if (!lureData.CanTriggerLure(character, tileObject))
					{
						text = Utilities.ColorizeInvalidText(lureData.GetReasonsWhyCannotPerformAbilityTowards(character));
					}
					if (!string.IsNullOrEmpty(text))
					{
						ShowSmallInfo(text, p_hoverPosition);
					}
				}
			}
			else
			{
				string hoverText = string.Empty;
				if (!PlayerManager.Instance.player.CanShareIntelTo(character, ref hoverText, PlayerManager.Instance.shareIntelContextMenuItem.currentIntelForContextMenu) && !string.IsNullOrEmpty(hoverText))
				{
					ShowSmallInfo(hoverText, p_hoverPosition);
				}
			}
		}
	}

	private void OnHoverOutPlayerActionContextMenuItem(IContextMenuItem p_item)
	{
		if (p_item is PlayerAction || p_item is InfuseChoice)
		{
			PlayerUI.Instance.skillDetailsTooltip.HidePlayerSkillDetails();
		}
		else
		{
			HideSmallInfo();
		}
	}

	private void OnHoverEnterFlaw(string traitName, Character p_character, UIHoverPosition p_hoverPosition)
	{
		Trait traitOrStatus = p_character.traitContainer.GetTraitOrStatus<Trait>(traitName);
		string localizedName = traitOrStatus.localizedName;
		string text = traitOrStatus.GetTriggerFlawEffectDescription(p_character, "flaw_effect");
		if (string.IsNullOrEmpty(text))
		{
			text = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "No_Flaw_Effect");
		}
		if (p_character.isInfoUnlocked)
		{
			int manaCost = PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.TRIGGER_FLAW).manaCost;
			string text2 = string.Empty;
			if (manaCost != -1)
			{
				text2 = text2 + manaCost + Utilities.ManaIcon() + "  ";
			}
			localizedName = localizedName + "    <size=16>" + text2;
			string text3 = string.Empty;
			if (PlayerManager.Instance.player.currenciesComponent.mana < manaCost)
			{
				text3 = text3 + Utilities.ColorizeInvalidText(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "No_Mana")) + "\n";
			}
			text = text + "\n\n" + text3;
		}
		else
		{
			localizedName = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Unknown_Flaw");
			Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
			dictionary.Add("targetName", p_character.name);
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Trigger_Unknown_Flaw", dictionary);
			MaccimaDictionaryPool<string, string>.Release(dictionary);
			text = localizedValue;
		}
		ShowSmallInfo(text, p_hoverPosition, localizedName, autoReplaceText: false);
	}

	private void OnHoverEnterRemoveBuff(string traitName, Character p_character, UIHoverPosition p_hoverPosition)
	{
		Trait traitOrStatus = p_character.traitContainer.GetTraitOrStatus<Trait>(traitName);
		string localizedName = traitOrStatus.localizedName;
		if (!traitOrStatus.CanBuffOrFlawBeRemoved(p_character, out var cannotRemoveReason))
		{
			cannotRemoveReason = Utilities.ColorizeInvalidText(cannotRemoveReason);
			ShowSmallInfo(cannotRemoveReason, p_hoverPosition, localizedName, autoReplaceText: false);
		}
	}

	private void OnHoverEnterGrudge(Character p_character, UIHoverPosition p_hoverPosition)
	{
		string hoverText = string.Empty;
		p_character.CanTriggerGrudge(p_character, ref hoverText);
		if (hoverText != string.Empty)
		{
			ShowSmallInfo(hoverText, p_hoverPosition, LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Trigger_Fail"), autoReplaceText: false);
		}
	}

	private void OnHoverPlayerAction(SkillData spellData, UIHoverPosition p_hoverPosition, IPlayerActionTarget p_target)
	{
		PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(spellData, p_hoverPosition, p_dontShowAdditionalText: false, p_target);
	}

	private void PopulatePlayerActionContextMenuItems(List<IContextMenuItem> contextMenuItems, IPlayerActionTarget p_target)
	{
		for (int i = 0; i < p_target.actions.Count; i++)
		{
			PLAYER_SKILL_TYPE type = p_target.actions[i];
			if (PlayerSkillManager.Instance.GetSkillData(type) is PlayerAction { shouldShowOnContextMenu: not false } playerAction && playerAction.IsValid(p_target) && PlayerManager.Instance.player.playerSkillComponent.CanDoSkill(type))
			{
				contextMenuItems.Add(playerAction);
			}
		}
	}

	private void AddPlayerActionContextMenuSignals()
	{
		Messenger.AddListener<IPlayerActionTarget>(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, ReloadPlayerActions);
		Messenger.AddListener(PlayerSkillSignals.FORCE_RELOAD_PLAYER_ACTIONS, ForceReloadPlayerActions);
		Messenger.AddListener<PLAYER_SKILL_TYPE, IPlayerActionTarget>(PlayerSkillSignals.PLAYER_ACTION_ADDED_TO_TARGET, OnPlayerActionAddedToTarget);
		Messenger.AddListener<PLAYER_SKILL_TYPE, IPlayerActionTarget>(PlayerSkillSignals.PLAYER_ACTION_REMOVED_FROM_TARGET, OnPlayerActionRemovedFromTarget);
	}

	private void RemovePlayerActionContextMenuSignals()
	{
		Messenger.RemoveListener<IPlayerActionTarget>(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, ReloadPlayerActions);
		Messenger.RemoveListener(PlayerSkillSignals.FORCE_RELOAD_PLAYER_ACTIONS, ForceReloadPlayerActions);
		Messenger.RemoveListener<PLAYER_SKILL_TYPE, IPlayerActionTarget>(PlayerSkillSignals.PLAYER_ACTION_ADDED_TO_TARGET, OnPlayerActionAddedToTarget);
		Messenger.RemoveListener<PLAYER_SKILL_TYPE, IPlayerActionTarget>(PlayerSkillSignals.PLAYER_ACTION_REMOVED_FROM_TARGET, OnPlayerActionRemovedFromTarget);
	}

	private void OnPlayerActionRemovedFromTarget(PLAYER_SKILL_TYPE p_skillType, IPlayerActionTarget p_target)
	{
		if (IsContextMenuShowing() && p_target != null && PlayerManager.Instance.player.currentlySelectedPlayerActionTarget == p_target)
		{
			UpdatePlayerActionContextMenuItems(p_target);
		}
	}

	private void OnPlayerActionAddedToTarget(PLAYER_SKILL_TYPE p_skillType, IPlayerActionTarget p_target)
	{
		if (IsContextMenuShowing() && p_target != null && PlayerManager.Instance.player.currentlySelectedPlayerActionTarget == p_target)
		{
			UpdatePlayerActionContextMenuItems(p_target);
		}
	}

	private void ReloadPlayerActions(IPlayerActionTarget p_target)
	{
		if (IsContextMenuShowing() && p_target != null && PlayerManager.Instance.player.currentlySelectedPlayerActionTarget == p_target)
		{
			UpdatePlayerActionContextMenuItems(p_target);
		}
	}

	private void ForceReloadPlayerActions()
	{
		if (IsContextMenuShowing() && PlayerManager.Instance.player.currentlySelectedPlayerActionTarget != null)
		{
			UpdatePlayerActionContextMenuItems(PlayerManager.Instance.player.currentlySelectedPlayerActionTarget);
		}
	}

	private void UpdatePlayerActionContextMenuItems(IPlayerActionTarget p_target)
	{
		List<IContextMenuItem> list = RuinarchListPool<IContextMenuItem>.Claim();
		PopulatePlayerActionContextMenuItems(list, p_target);
		contextMenuUIController.UpdateContextMenuItems(list);
		RuinarchListPool<IContextMenuItem>.Release(list);
	}

	public void ShowUpgradeAbilitiesUI()
	{
		onSpireClicked?.Invoke();
		Messenger.Broadcast(UISignals.SHOW_UPGRADE_ABILITIES);
	}

	public void ShowMonstersUpgradeAbilitiesUI()
	{
		onPrimordialPoolClicked?.Invoke();
	}

	public void ShowRaidUI(LocationStructure structure, SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR p_behaviour)
	{
		spawnPartyUIController.Show(structure, p_behaviour, null, p_attackerDropdownInteractable: false);
	}

	public void ShowRaidUI(LocationStructure structure, SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR p_behaviour, IStoredTarget additionalTarget)
	{
		spawnPartyUIController.Show(structure, p_behaviour, additionalTarget, p_attackerDropdownInteractable: true, p_targetDropdownInteractable: false);
	}

	public void ShowSnatchVillagerUI(LocationStructure structure)
	{
		spawnPartyUIController.Show(structure, SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Snatch_Villager, null, p_attackerDropdownInteractable: false);
		Messenger.Broadcast(UISignals.SHOW_SNATCH_VILLAGER_UI);
	}

	public void ShowSnatchVillagerUI(LocationStructure structure, IStoredTarget additionalTarget)
	{
		spawnPartyUIController.Show(structure, SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Snatch_Villager, additionalTarget, p_attackerDropdownInteractable: true, p_targetDropdownInteractable: false);
		Messenger.Broadcast(UISignals.SHOW_SNATCH_VILLAGER_UI);
	}

	public void ShowKillVillagerUI(LocationStructure structure, IStoredTarget additionalTarget)
	{
		spawnPartyUIController.Show(structure, SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Kill_Villager, additionalTarget, p_attackerDropdownInteractable: true, p_targetDropdownInteractable: false);
	}

	public void ShowKillVillagerUI(LocationStructure structure)
	{
		spawnPartyUIController.Show(structure, SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Kill_Villager, null, p_attackerDropdownInteractable: false);
	}

	public void ShowSnatchMonsterUI(LocationStructure structure)
	{
		spawnPartyUIController.Show(structure, SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Snatch_Monster, null, p_attackerDropdownInteractable: false);
	}

	public void ShowSnatchMonsterUI(LocationStructure structure, IStoredTarget additionalTarget)
	{
		spawnPartyUIController.Show(structure, SPECIFIC_STRUCTURE_PARTY_BEHAVIOUR.Snatch_Monster, additionalTarget, p_attackerDropdownInteractable: true, p_targetDropdownInteractable: false);
	}

	public void ShowUpgradePortalUI(ThePortal portal)
	{
		upgradePortalUIController.ShowPortalUpgradeTier(portal);
	}

	public void ShowPurchaseSkillUI()
	{
		purchaseSkillUIController.Init(3, playShowAnimation: true);
	}

	public void OnBookmarkMenuHide()
	{
		(notificationHoverPos.transform as RectTransform).anchoredPosition = notificationHoverPosDefaultPosition;
	}

	public void OnBookmarkMenuShow()
	{
		(notificationHoverPos.transform as RectTransform).anchoredPosition = notificationHoverPosModifiedPosition;
	}

	public void ShowWaitForTileObjectGenerationToFinishWindow()
	{
		Pause();
		SetSpeedTogglesState(state: false);
		InnerMapCameraMove.Instance.DisableMovement();
		InputManager.Instance.SetAllHotkeysEnabledState(p_state: false);
		waitWindow.SetActive(value: true);
		StartCoroutine(WaitForTileObjectGenerationToFinish());
	}

	private void HideWaitForTileObjectGenerationToFinishWindow()
	{
		SetSpeedTogglesState(state: true);
		InnerMapCameraMove.Instance.EnableMovement();
		InputManager.Instance.SetAllHotkeysEnabledState(p_state: true);
		waitWindow.SetActive(value: false);
	}

	private IEnumerator WaitForTileObjectGenerationToFinish()
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		while (WorldConfigManager.Instance.mapGenerationData.isGeneratingTileObjects)
		{
			yield return null;
		}
		HideWaitForTileObjectGenerationToFinishWindow();
		GameManager.Instance.StartProgression();
		stopwatch.Stop();
		UnityEngine.Debug.Log("WaitForTileObjectGenerationToFinish took " + stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds to complete.");
	}

	public bool IsWaitingForTileObjectGenerationToComplete()
	{
		return waitWindow.activeInHierarchy;
	}

	public void OpenLoadWindow()
	{
		SaveWindowUIController.Instance.ShowLoadWindow();
	}

	private void OnLoadHotkeyPressed()
	{
		if (!SaveManager.Instance.saveCurrentProgressManager.isSaving && !SaveManager.Instance.saveCurrentProgressManager.isWritingToDisk && !SaveWindowUIController.Instance.isShowing)
		{
			OpenLoadWindow();
		}
	}

	public void CloseLoadWindow()
	{
		SaveWindowUIController.Instance.HideUI();
	}

	public void ShowGainedPowersPopup(List<PLAYER_SKILL_TYPE> p_skills, Action p_onCloseAction)
	{
		_gainedPowersPopup.Show(p_skills, p_onCloseAction);
	}

	public void ShowPrismEventsUI()
	{
		_prismEventsUI.Show();
	}

	public void InitializePrismEventsUI(PrismEvent[] p_events)
	{
		_prismEventsUI.Initialize(p_events);
	}

	public void ShowSnatchObjectUI(TileObject p_targetTileObject)
	{
		snatchObjectUIController.Show(p_targetTileObject);
	}

	public void ShowChooseSkillsPopup(Dictionary<PortalUpgradeItem, PortalUpgradeTier> p_itemsToShow)
	{
		foreach (KeyValuePair<PortalUpgradeItem, PortalUpgradeTier> item in p_itemsToShow)
		{
			_itemsToShow.Add(item.Key, item.Value);
		}
		totalUpgradesToShow = p_itemsToShow.Count;
		currentUpgradeIndex = 0;
		TryShowNextSkillPopup();
	}

	public bool TryShowNextSkillPopup()
	{
		if (_itemsToShow.Count > 0)
		{
			KeyValuePair<PortalUpgradeItem, PortalUpgradeTier> keyValuePair = _itemsToShow.First();
			_itemsToShow.Remove(keyValuePair.Key);
			currentUpgradeIndex++;
			_chooseSkillsPopup.Show(keyValuePair.Key, keyValuePair.Value);
			return true;
		}
		return false;
	}

	private void ListenToSubGoals()
	{
		for (int i = 0; i < PlayerManager.Instance.player.goalComponent.subGoals.Length; i++)
		{
			PlayerManager.Instance.player.goalComponent.subGoals[i].subGoalEventDispatcher.SubscribeToSubGoal(this);
		}
	}

	public void OnSubGoalNameUpdated(SubGoal p_task)
	{
	}

	public void OnSubGoalCompleted(SubGoal p_subGoal)
	{
		_subGoalNotificationUI.ShowNotification(p_subGoal);
	}

	public void OnSubGoalPinStateChanged(SubGoal p_subGoal, bool p_isPinned)
	{
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Ruinarch.Custom_UI;
using Settings;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UtilityScripts;

namespace Ruinarch;

public class InputManager : MonoBehaviour
{
	public static InputManager Instance;

	private const string GamepadScheme = "Gamepad";

	private const string KeyboardScheme = "Keyboard";

	[Space(10f)]
	[Header("Cursors")]
	[SerializeField]
	private CursorTextureDictionary cursors;

	[Space(10f)]
	[Header("New Input System")]
	public PlayerInput playerInput;

	public InputActionAsset inputActionAsset;

	[SerializeField]
	private GamepadCursor gamepadCursor;

	[SerializeField]
	private ShortcutInputDictionary shortcutInputDictionary;

	public Cursor_Type currentCursorType;

	public Cursor_Type previousCursorType;

	public bool isDraggingItem;

	private CursorMode cursorMode = CursorMode.ForceSoftware;

	private bool _isDemolishKeyDown;

	private Dictionary<SHORTCUT_ACTION, bool> _allowedShortcuts;

	private readonly List<DemonicStructure> allDemonicStructureOfSameType = new List<DemonicStructure>();

	private string previousControlScheme;

	private Mouse _currentMouse;

	private InputAction _moveCameraAction;

	private InputAction _cameraZoomAction;

	private InputAction _rotateDecorationAction;

	private static Action m_onUpdateEvent;

	public HashSet<string> buttonsToHighlight { get; private set; }

	public bool isDemolishKeyKeyDown => _isDemolishKeyDown;

	public Vector3 mousePosition => GetCurrentMousePosition();

	public bool isUsingGamepad => playerInput.currentControlScheme == "Gamepad";

	public InputAction rotateDecorationAction => _rotateDecorationAction;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			SetCursorTo(Cursor_Type.Default);
			previousCursorType = Cursor_Type.Default;
			SceneManager.activeSceneChanged += OnActiveSceneChanged;
			Initialize();
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			UnsubscribeCsharpEvents();
		}
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		if (!hasFocus)
		{
			_isDemolishKeyDown = false;
		}
	}

	private void TryExecuteShortcutKeyUpAction(SHORTCUT_ACTION p_action)
	{
		if (p_action == SHORTCUT_ACTION.Demolish_Hold)
		{
			_isDemolishKeyDown = false;
		}
	}

	private void TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION p_action)
	{
		if (!CanUseHotkey(p_action))
		{
			return;
		}
		switch (p_action)
		{
		case SHORTCUT_ACTION.Report_A_Bug:
			ReportABug();
			break;
		case SHORTCUT_ACTION.Cycle_Eyes:
			if (GameManager.Instance == null)
			{
				break;
			}
			allDemonicStructureOfSameType.Clear();
			if (DatabaseManager.Instance?.structureDatabase != null && DatabaseManager.Instance.structureDatabase.allStructures != null)
			{
				for (int i = 0; i < DatabaseManager.Instance.structureDatabase.allStructures.Count; i++)
				{
					if (DatabaseManager.Instance.structureDatabase.allStructures[i].structureType == STRUCTURE_TYPE.WATCHER && !DatabaseManager.Instance.structureDatabase.allStructures[i].hasBeenDestroyed)
					{
						allDemonicStructureOfSameType.Add(DatabaseManager.Instance.structureDatabase.allStructures[i] as DemonicStructure);
					}
				}
			}
			EyesCycle();
			break;
		case SHORTCUT_ACTION.Cycle_Villagers_Forward:
			if (!HasSelectedUIObject() && !(GameManager.Instance == null))
			{
				CharacterCenterCycleForward();
			}
			break;
		case SHORTCUT_ACTION.Cycle_Villagers_Backward:
			if (!HasSelectedUIObject() && !(GameManager.Instance == null))
			{
				CharacterCenterCycleBackward();
			}
			break;
		case SHORTCUT_ACTION.Toggle_Nameplate:
			if (GameManager.Instance != null && GameManager.Instance.gameHasStarted)
			{
				CharacterManager.Instance.ToggleCharacterMarkerNameplate();
			}
			break;
		case SHORTCUT_ACTION.Cancel:
			if (UIManager.Instance != null)
			{
				if (isUsingGamepad)
				{
					CancelActionsByPriority();
				}
				else if (!CancelActionsByPriority() && !UIManager.Instance.IsOptionsMenuShowing())
				{
					UIManager.Instance.OpenOptionsMenu();
				}
			}
			break;
		case SHORTCUT_ACTION.Toggle_Options_Menu:
			if (UIManager.Instance != null)
			{
				if (!UIManager.Instance.IsOptionsMenuShowing())
				{
					UIManager.Instance.OpenOptionsMenu();
				}
				else
				{
					UIManager.Instance.CloseOptionsMenu();
				}
			}
			break;
		case SHORTCUT_ACTION.Right_Click:
			if (!isUsingGamepad && InnerMapManager.Instance != null)
			{
				LocationGridTile tileFromMousePosition = InnerMapManager.Instance.GetTileFromMousePosition();
				if (!InnerMapManager.Instance.HasSelectablesOnTile(tileFromMousePosition) && !UIManager.Instance.IsMouseOnUI())
				{
					Messenger.Broadcast(ControlsSignals.KEY_DOWN_EMPTY_SPACE, KeyCode.Mouse1);
				}
				CancelSpellsByPriority();
			}
			Messenger.Broadcast(ControlsSignals.PLAYER_INPUT_ACTION, p_action);
			break;
		case SHORTCUT_ACTION.Snatch_Villager:
			if (!HasSelectedUIObject() && !(GameManager.Instance == null))
			{
				DemonicStructureCycle(STRUCTURE_TYPE.TORTURE_CHAMBERS);
				Messenger.Broadcast(ControlsSignals.PLAYER_INPUT_ACTION, p_action);
			}
			break;
		case SHORTCUT_ACTION.Snatch_Monster:
			if (!HasSelectedUIObject() && !(GameManager.Instance == null))
			{
				DemonicStructureCycle(STRUCTURE_TYPE.KENNEL);
				Messenger.Broadcast(ControlsSignals.PLAYER_INPUT_ACTION, p_action);
			}
			break;
		case SHORTCUT_ACTION.Raid:
			if (!HasSelectedUIObject() && !(GameManager.Instance == null))
			{
				DemonicStructureCycle(STRUCTURE_TYPE.MARAUD);
				Messenger.Broadcast(ControlsSignals.PLAYER_INPUT_ACTION, p_action);
			}
			break;
		case SHORTCUT_ACTION.Quick_Destroy:
		{
			if (GameManager.Instance == null)
			{
				break;
			}
			PLAYER_SKILL_TYPE pLAYER_SKILL_TYPE = PLAYER_SKILL_TYPE.NONE;
			IPlayerActionTarget playerActionTarget = null;
			if (UIManager.Instance.structureInfoUI.isShowing && UIManager.Instance.structureInfoUI.activeStructure.structureType != STRUCTURE_TYPE.THE_PORTAL)
			{
				playerActionTarget = UIManager.Instance.structureInfoUI.activeStructure;
				pLAYER_SKILL_TYPE = PLAYER_SKILL_TYPE.DESTROY_STRUCTURE;
			}
			else if (UIManager.Instance.tileObjectInfoUI.isShowing)
			{
				playerActionTarget = UIManager.Instance.tileObjectInfoUI.activeTileObject;
				pLAYER_SKILL_TYPE = PLAYER_SKILL_TYPE.DESTROY;
				if (playerActionTarget is DemonEye)
				{
					pLAYER_SKILL_TYPE = PLAYER_SKILL_TYPE.DESTROY_EYE_WARD;
				}
			}
			if (pLAYER_SKILL_TYPE != PLAYER_SKILL_TYPE.NONE && playerActionTarget != null && PlayerManager.Instance.player.playerSkillComponent.CanDoSkill(pLAYER_SKILL_TYPE))
			{
				PlayerAction playerActionData = PlayerSkillManager.Instance.GetPlayerActionData(pLAYER_SKILL_TYPE);
				if (playerActionData.IsValid(playerActionTarget) && playerActionData.CanBePickedRegardlessOfCooldown(playerActionTarget) && playerActionTarget.actions.Contains(pLAYER_SKILL_TYPE))
				{
					playerActionData.OnPickAction(playerActionTarget);
				}
			}
			break;
		}
		case SHORTCUT_ACTION.Demolish_Hold:
			_isDemolishKeyDown = true;
			break;
		case SHORTCUT_ACTION.Center_Selected_Object:
			UIManager.Instance?.CenterCurrentlySelectedObject();
			break;
		default:
			if ((p_action != SHORTCUT_ACTION.Spells && p_action != SHORTCUT_ACTION.Structures && p_action != SHORTCUT_ACTION.Demons && p_action != SHORTCUT_ACTION.Monsters && p_action != SHORTCUT_ACTION.Intel && p_action != SHORTCUT_ACTION.Targets && p_action != SHORTCUT_ACTION.Villagers && p_action != SHORTCUT_ACTION.Cultists && p_action != SHORTCUT_ACTION.Tutorials && p_action != SHORTCUT_ACTION.Goals && p_action != SHORTCUT_ACTION.Center_Portal && p_action != SHORTCUT_ACTION.Sub_Goals) || !HasSelectedUIObject())
			{
				Messenger.Broadcast(ControlsSignals.PLAYER_INPUT_ACTION, p_action);
			}
			break;
		}
	}

	private void Update()
	{
		if (LevelLoaderManager.Instance.isLoadingNewScene || LevelLoaderManager.Instance.IsLoadingScreenActive())
		{
			return;
		}
		m_onUpdateEvent?.Invoke();
		if (_moveCameraAction.phase == InputActionPhase.Started)
		{
			Vector2 vector = _moveCameraAction.ReadValue<Vector2>();
			if (vector != Vector2.zero)
			{
				if (vector.y > 0f)
				{
					TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Pan_Up);
				}
				else if (vector.y < 0f)
				{
					TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Pan_Down);
				}
				if (vector.x > 0f)
				{
					TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Pan_Right);
				}
				else if (vector.x < 0f)
				{
					TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Pan_Left);
				}
			}
		}
		if (_cameraZoomAction.phase == InputActionPhase.Started)
		{
			Vector2 normalized = _cameraZoomAction.ReadValue<Vector2>().normalized;
			if (normalized != Vector2.zero)
			{
				InnerMapCameraMove.Instance?.ScrollWheelZooming(normalized.y);
			}
		}
	}

	private bool CanUseHotkey(SHORTCUT_ACTION p_action)
	{
		if (SaveManager.Instance.saveCurrentProgressManager.isSaving)
		{
			return false;
		}
		if (LevelLoaderManager.Instance.isLoadingNewScene || LevelLoaderManager.Instance.IsLoadingScreenActive())
		{
			return false;
		}
		switch (p_action)
		{
		case SHORTCUT_ACTION.Toggle_Console:
			return true;
		default:
			if (PlayerUI.Instance != null && PlayerUI.Instance.IsMajorUIShowing())
			{
				return false;
			}
			if (UIManager.Instance != null && UIManager.Instance.IsObjectPickerOpen())
			{
				return false;
			}
			break;
		case SHORTCUT_ACTION.Cancel:
		case SHORTCUT_ACTION.Toggle_Options_Menu:
		case SHORTCUT_ACTION.Confirmation_Window_Confirm:
		case SHORTCUT_ACTION.Confirmation_Window_Cancel:
			break;
		}
		if (!_allowedShortcuts.ContainsKey(p_action))
		{
			return true;
		}
		return _allowedShortcuts[p_action];
	}

	public void SetAllHotkeysEnabledState(bool p_state)
	{
		SHORTCUT_ACTION[] array = _allowedShortcuts.Keys.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			SetSpecificHotkeyEnabledState(array[i], p_state);
		}
	}

	public void SetSpecificHotkeyEnabledState(SHORTCUT_ACTION p_action, bool p_state)
	{
		_allowedShortcuts[p_action] = p_state;
	}

	private void Initialize()
	{
		if (Utilities.DoesFileExist(SettingsManager.Instance.keybindsFileLocation))
		{
			StreamReader streamReader = new StreamReader(SettingsManager.Instance.keybindsFileLocation);
			string text = streamReader.ReadToEnd();
			streamReader.Close();
			if (!string.IsNullOrEmpty(text))
			{
				inputActionAsset.LoadBindingOverridesFromJson(text);
			}
		}
		_currentMouse = Mouse.current;
		_moveCameraAction = playerInput.actions.FindAction("Move Camera");
		_cameraZoomAction = playerInput.actions.FindAction("Camera Zoom");
		_rotateDecorationAction = playerInput.actions.FindAction("Rotate Decoration");
		SubscribeCsharpEvents();
		buttonsToHighlight = new HashSet<string>();
		Messenger.MarkAsPermanent(UISignals.SHOW_SELECTABLE_GLOW);
		Messenger.MarkAsPermanent(UISignals.HIDE_SELECTABLE_GLOW);
		Messenger.MarkAsPermanent(UISignals.TOGGLE_SHOWN);
		Messenger.AddListener<string>(UISignals.SHOW_SELECTABLE_GLOW, OnReceiveHighlightSignal);
		Messenger.AddListener<string>(UISignals.HIDE_SELECTABLE_GLOW, OnReceiveUnHighlightSignal);
		Messenger.AddListener<RuinarchToggle>(UISignals.TOGGLE_SHOWN, OnToggleShown);
		Messenger.AddListener<RuinarchButton>(UISignals.BUTTON_SHOWN, OnButtonShown);
		ConstructHotKeys();
	}

	private void ConstructHotKeys()
	{
		_allowedShortcuts = new Dictionary<SHORTCUT_ACTION, bool>();
		SHORTCUT_ACTION[] enumValues = CollectionUtilities.GetEnumValues<SHORTCUT_ACTION>();
		foreach (SHORTCUT_ACTION sHORTCUT_ACTION in enumValues)
		{
			if (sHORTCUT_ACTION != SHORTCUT_ACTION.Left_Click && sHORTCUT_ACTION != SHORTCUT_ACTION.Right_Click && sHORTCUT_ACTION != SHORTCUT_ACTION.Middle_Click)
			{
				_allowedShortcuts.Add(sHORTCUT_ACTION, value: true);
			}
		}
	}

	private void OnReceiveHighlightSignal(string name)
	{
		buttonsToHighlight.Add(name);
	}

	private void OnReceiveUnHighlightSignal(string name)
	{
		buttonsToHighlight.Remove(name);
	}

	private void OnToggleShown(RuinarchToggle toggle)
	{
		if (buttonsToHighlight.Contains(toggle.name))
		{
			toggle.StartGlow();
		}
	}

	private void OnButtonShown(RuinarchButton button)
	{
		if (buttonsToHighlight.Contains(button.name))
		{
			button.StartGlow();
		}
	}

	private void SubscribeCsharpEvents()
	{
		playerInput.onControlsChanged += OnControlsChanged;
		playerInput.actions.FindAction("Left Click").performed += OnLeftClick;
		playerInput.actions.FindAction("Left Click").canceled += OnLeftClick;
		playerInput.actions.FindAction("Middle Click").performed += MiddleClick;
		playerInput.actions.FindAction("Right Click").performed += RightClick;
		playerInput.actions.FindAction("Increase Progression Speed").performed += OnIncreaseProgressionSpeed;
		playerInput.actions.FindAction("Decrease Progression Speed").performed += OnDecreaseProgressionSpeed;
		playerInput.actions.FindAction("Toggle Spells").performed += ToggleSpells;
		playerInput.actions.FindAction("Toggle Structures").performed += ToggleStructures;
		playerInput.actions.FindAction("Toggle Monsters").performed += ToggleMonsters;
		playerInput.actions.FindAction("Toggle Intel").performed += ToggleIntel;
		playerInput.actions.FindAction("Toggle Targets").performed += ToggleTargets;
		playerInput.actions.FindAction("Toggle Villagers").performed += ToggleVillagers;
		playerInput.actions.FindAction("Toggle Cultists").performed += ToggleCultists;
		playerInput.actions.FindAction("Toggle Tutorials").performed += ToggleTutorials;
		playerInput.actions.FindAction("Toggle Goals").performed += ToggleGoals;
		playerInput.actions.FindAction("Toggle Sub Goals").performed += ToggleSubGoals;
		playerInput.actions.FindAction("Quick Save").performed += QuickSave;
		playerInput.actions.FindAction("Quick Load").performed += QuickLoad;
		playerInput.actions.FindAction("Toggle Pause").performed += TogglePause;
		playerInput.actions.FindAction("Snatch Villager").performed += SnatchVillager;
		playerInput.actions.FindAction("Snatch Monster").performed += SnatchMonster;
		playerInput.actions.FindAction("Raid").performed += Raid;
		playerInput.actions.FindAction("Center Portal").performed += CenterPortal;
		playerInput.actions.FindAction("Cycle Eyes").performed += CycleEyes;
		playerInput.actions.FindAction("Cycle Villagers Forward").performed += CycleVillagersForward;
		playerInput.actions.FindAction("Cycle Villagers Backward").performed += CycleVillagersBackward;
		playerInput.actions.FindAction("Toggle Nameplates").performed += ToggleNameplates;
		playerInput.actions.FindAction("Report A Bug").performed += ReportABug;
		playerInput.actions.FindAction("Cancel").performed += Cancel;
		playerInput.actions.FindAction("Quick Destroy").performed += QuickDestroy;
		playerInput.actions.FindAction("Demolish Hold").performed += DemolishHold;
		playerInput.actions.FindAction("Demolish Hold").canceled += DemolishHold;
		playerInput.actions.FindAction("Demolish Hold").started += DemolishHold;
		playerInput.actions.FindAction("Toggle Console").performed += ToggleConsole;
		playerInput.actions.FindAction("Rotate Decoration").performed += RotateDecoration;
		playerInput.actions.FindAction("Center On Selected Object").performed += CenterOnSelectedObject;
		playerInput.actions.FindAction("Options Menu").performed += ShowOptionsMenu;
		playerInput.actions.FindAction("Show Context Menu").performed += ShowContextMenu;
		playerInput.actions.FindAction("Confirmation Window Confirm").performed += ConfirmationWindowConfirm;
		playerInput.actions.FindAction("Confirmation Window Cancel").performed += ConfirmationWindowCancel;
		playerInput.actions.FindAction("Cycle Top Menus").performed += CycleTopMenus;
	}

	private void UnsubscribeCsharpEvents()
	{
		playerInput.onControlsChanged -= OnControlsChanged;
		playerInput.actions.FindAction("Left Click").performed -= OnLeftClick;
		playerInput.actions.FindAction("Left Click").canceled -= OnLeftClick;
		playerInput.actions.FindAction("Middle Click").performed -= MiddleClick;
		playerInput.actions.FindAction("Right Click").performed -= RightClick;
		playerInput.actions.FindAction("Increase Progression Speed").performed -= OnIncreaseProgressionSpeed;
		playerInput.actions.FindAction("Decrease Progression Speed").performed -= OnDecreaseProgressionSpeed;
		playerInput.actions.FindAction("Toggle Spells").performed -= ToggleSpells;
		playerInput.actions.FindAction("Toggle Structures").performed -= ToggleStructures;
		playerInput.actions.FindAction("Toggle Monsters").performed -= ToggleMonsters;
		playerInput.actions.FindAction("Toggle Intel").performed -= ToggleIntel;
		playerInput.actions.FindAction("Toggle Targets").performed -= ToggleTargets;
		playerInput.actions.FindAction("Toggle Villagers").performed -= ToggleVillagers;
		playerInput.actions.FindAction("Toggle Cultists").performed -= ToggleCultists;
		playerInput.actions.FindAction("Toggle Tutorials").performed -= ToggleTutorials;
		playerInput.actions.FindAction("Toggle Goals").performed -= ToggleGoals;
		playerInput.actions.FindAction("Toggle Sub Goals").performed -= ToggleSubGoals;
		playerInput.actions.FindAction("Quick Save").performed -= QuickSave;
		playerInput.actions.FindAction("Quick Load").performed -= QuickLoad;
		playerInput.actions.FindAction("Toggle Pause").performed -= TogglePause;
		playerInput.actions.FindAction("Snatch Villager").performed -= SnatchVillager;
		playerInput.actions.FindAction("Snatch Monster").performed -= SnatchMonster;
		playerInput.actions.FindAction("Raid").performed -= Raid;
		playerInput.actions.FindAction("Center Portal").performed -= CenterPortal;
		playerInput.actions.FindAction("Cycle Eyes").performed -= CycleEyes;
		playerInput.actions.FindAction("Cycle Villagers Forward").performed -= CycleVillagersForward;
		playerInput.actions.FindAction("Cycle Villagers Backward").performed -= CycleVillagersBackward;
		playerInput.actions.FindAction("Toggle Nameplates").performed -= ToggleNameplates;
		playerInput.actions.FindAction("Report A Bug").performed -= ReportABug;
		playerInput.actions.FindAction("Cancel").performed -= Cancel;
		playerInput.actions.FindAction("Quick Destroy").performed -= QuickDestroy;
		playerInput.actions.FindAction("Demolish Hold").performed -= DemolishHold;
		playerInput.actions.FindAction("Demolish Hold").canceled -= DemolishHold;
		playerInput.actions.FindAction("Demolish Hold").started -= DemolishHold;
		playerInput.actions.FindAction("Toggle Console").performed -= ToggleConsole;
		playerInput.actions.FindAction("Rotate Decoration").performed -= RotateDecoration;
		playerInput.actions.FindAction("Center On Selected Object").performed -= CenterOnSelectedObject;
		playerInput.actions.FindAction("Options Menu").performed -= ShowOptionsMenu;
		playerInput.actions.FindAction("Show Context Menu").performed -= ShowContextMenu;
		playerInput.actions.FindAction("Confirmation Window Confirm").performed -= ConfirmationWindowConfirm;
		playerInput.actions.FindAction("Confirmation Window Cancel").performed -= ConfirmationWindowCancel;
		playerInput.actions.FindAction("Cycle Top Menus").performed -= CycleTopMenus;
	}

	public void SetCursorTo(Cursor_Type type)
	{
		if (currentCursorType != type)
		{
			previousCursorType = currentCursorType;
			Vector2 hotspot = Vector2.zero;
			switch (type)
			{
			case Cursor_Type.Drag_Clicked:
				isDraggingItem = true;
				break;
			case Cursor_Type.Check:
			case Cursor_Type.Cross:
			case Cursor_Type.Link:
				hotspot = new Vector2(12f, 10f);
				break;
			case Cursor_Type.Target:
				hotspot = new Vector2(29f, 29f);
				break;
			default:
				isDraggingItem = false;
				break;
			}
			currentCursorType = type;
			Cursor.SetCursor(cursors[type], hotspot, cursorMode);
			gamepadCursor.SetCursor(type);
		}
	}

	public void RevertToPreviousCursor()
	{
		SetCursorTo(previousCursorType);
	}

	private bool CancelActionsByPriority()
	{
		if (SettingsManager.Instance.IsShowing())
		{
			SettingsManager.Instance.CloseSettings();
			return true;
		}
		if (UIManager.Instance == null)
		{
			return true;
		}
		if (SaveManager.Instance != null && SaveManager.Instance.saveCurrentProgressManager.isSaving)
		{
			return true;
		}
		UIManager.Instance.SetTempDisableShowInfoUI(state: false);
		if (!CancelSpellsByPriority())
		{
			if (UIManager.Instance.IsOptionsMenuShowing())
			{
				if (SaveWindowUIController.Instance.isShowing)
				{
					UIManager.Instance.CloseLoadWindow();
					return true;
				}
				UIManager.Instance.CloseOptionsMenu();
				return true;
			}
			if (UIManager.Instance.IsContextMenuShowing())
			{
				UIManager.Instance.HideContextMenu();
				return true;
			}
			if (UIManager.Instance.biolabUIController.isShowing)
			{
				UIManager.Instance.biolabUIController.HideViaShortcutKey();
				return true;
			}
			if (UIManager.Instance.primordialPoolUIController.isShowing)
			{
				UIManager.Instance.primordialPoolUIController.HideViaShortcutKey();
				return true;
			}
			if (UIManager.Instance.upgradePortalUIController.isShowing)
			{
				if (UIManager.Instance.yesNoConfirmation.isShowing)
				{
					UIManager.Instance.yesNoConfirmation.Close();
					return true;
				}
				if (UIManager.Instance.upgradePortalUIController.IsChoosePowersWindowOpen())
				{
					UIManager.Instance.upgradePortalUIController.HideChooseSkillsWindow();
					return true;
				}
				UIManager.Instance.upgradePortalUIController.HideViaShortcutKey();
				return true;
			}
			if (UIManager.Instance.skillUpgradeUIController.isShowing)
			{
				UIManager.Instance.skillUpgradeUIController.HideViaShortcutKey();
			}
			if (UIManager.Instance.purchaseSkillUIController.isShowing)
			{
				if (UIManager.Instance.yesNoConfirmation.isShowing)
				{
					UIManager.Instance.yesNoConfirmation.Close();
					return true;
				}
				UIManager.Instance.purchaseSkillUIController.HideViaShortcutKey();
				return true;
			}
			if (UIManager.Instance.grudgeUIController.isShowing)
			{
				if (UIManager.Instance.yesNoConfirmation.isShowing)
				{
					UIManager.Instance.yesNoConfirmation.Close();
					return true;
				}
				UIManager.Instance.grudgeUIController.HideViaShortcutKey();
				return true;
			}
			if (UIManager.Instance.criticalBreakUIController.isShowing)
			{
				if (UIManager.Instance.yesNoConfirmation.isShowing)
				{
					UIManager.Instance.yesNoConfirmation.Close();
					return true;
				}
				UIManager.Instance.criticalBreakUIController.HideViaShortcutKey();
				return true;
			}
			if (PlayerUI.Instance.tutorialUIController.isShowing)
			{
				PlayerUI.Instance.tutorialUIController.HideViaShortcutKey();
				return true;
			}
			if (PlayerUI.Instance.goalsUIController.isShowing)
			{
				PlayerUI.Instance.goalsUIController.HideViaShortcutKey();
				return true;
			}
			if (PlayerUI.Instance.subGoalsUIController.isShowing)
			{
				PlayerUI.Instance.subGoalsUIController.HideViaShortcutKey();
				return true;
			}
			if (UIManager.Instance.spawnPartyUIController.isShowing)
			{
				UIManager.Instance.spawnPartyUIController.HideViaShortcutKey();
				return true;
			}
			if (UIManager.Instance.snatchObjectUIController.isShowing)
			{
				UIManager.Instance.snatchObjectUIController.HideViaShortcutKey();
				return true;
			}
			if (UIManager.Instance.openedPopups.Count > 0)
			{
				if (PointerDirection.ActiveCount <= 0)
				{
					UIManager.Instance.openedPopups.Last().Close();
					return true;
				}
				Messenger.Broadcast(ControlsSignals.ON_CLOSE_OPEN_POP_UPS);
				return true;
			}
			if (UIManager.Instance.poiTestingUI.gameObject.activeSelf)
			{
				return true;
			}
			if (UIManager.Instance.latestOpenedInfoUI != null)
			{
				UIManager.Instance.latestOpenedInfoUI.OnClickCloseMenu();
				return true;
			}
			return false;
		}
		return true;
	}

	private bool CancelSpellsByPriority()
	{
		if (PlayerManager.Instance.player != null && PlayerManager.Instance.player.currentActivePlayerSpell != null)
		{
			PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(null);
			return true;
		}
		if (PlayerManager.Instance.player != null && PlayerManager.Instance.player.currentActiveIntel != null)
		{
			PlayerManager.Instance.player.SetCurrentActiveIntel(null);
			return true;
		}
		if (PlayerManager.Instance.player != null && PlayerManager.Instance.player.currentActiveItem != TILE_OBJECT_TYPE.NONE)
		{
			PlayerManager.Instance.player.SetCurrentlyActiveItem(TILE_OBJECT_TYPE.NONE);
			return true;
		}
		if (PlayerManager.Instance.player != null && PlayerManager.Instance.player.currentActiveArtifact != ARTIFACT_TYPE.None)
		{
			PlayerManager.Instance.player.SetCurrentlyActiveArtifact(ARTIFACT_TYPE.None);
			return true;
		}
		if (PlayerManager.Instance.player != null && PlayerManager.Instance.player.currentActiveMonsterType != SUMMON_TYPE.None)
		{
			PlayerManager.Instance.player.SetCurrentlyActiveMonster(SUMMON_TYPE.None);
			return true;
		}
		return false;
	}

	private void OnActiveSceneChanged(Scene current, Scene next)
	{
		buttonsToHighlight.Clear();
		if (next.name == "Game")
		{
			SetInputMapState("Gameplay", p_state: true);
		}
	}

	public bool ShouldBeHighlighted(RuinarchButton button)
	{
		return buttonsToHighlight.Contains(button.name);
	}

	public bool ShouldBeHighlighted(RuinarchToggle button)
	{
		return buttonsToHighlight.Contains(button.name);
	}

	public bool HasSelectedUIObject()
	{
		GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
		if (currentSelectedGameObject != null)
		{
			return currentSelectedGameObject.activeInHierarchy;
		}
		return false;
	}

	private Vector3 GetCurrentMousePosition()
	{
		if (isUsingGamepad)
		{
			return gamepadCursor.position;
		}
		return Input.mousePosition;
	}

	public string GetShortcutDisplayStringForAction(SHORTCUT_ACTION p_action)
	{
		if (isUsingGamepad && (p_action == SHORTCUT_ACTION.Spells || p_action == SHORTCUT_ACTION.Structures || p_action == SHORTCUT_ACTION.Demons || p_action == SHORTCUT_ACTION.Monsters || p_action == SHORTCUT_ACTION.Intel || p_action == SHORTCUT_ACTION.Targets || p_action == SHORTCUT_ACTION.Villagers || p_action == SHORTCUT_ACTION.Cultists || p_action == SHORTCUT_ACTION.Tutorials || p_action == SHORTCUT_ACTION.Goals || p_action == SHORTCUT_ACTION.Sub_Goals))
		{
			p_action = SHORTCUT_ACTION.Cycle_Top_Menus;
		}
		if (shortcutInputDictionary.ContainsKey(p_action))
		{
			InputActionReference inputActionReference = shortcutInputDictionary[p_action];
			return InputActionRebindingExtensions.GetBindingDisplayString(group: isUsingGamepad ? "Gamepad" : "Keyboard", action: inputActionReference.action, options: InputBinding.DisplayStringOptions.DontIncludeInteractions);
		}
		return string.Empty;
	}

	public void Select(ISelectable objToSelect)
	{
		objToSelect.LeftSelectAction();
		Messenger.Broadcast(ControlsSignals.SELECTABLE_LEFT_CLICKED, objToSelect);
	}

	private void ReportABug()
	{
		YesNoConfirmation yesNoConfirmation = null;
		if (UIManager.Instance != null)
		{
			yesNoConfirmation = UIManager.Instance.yesNoConfirmation;
		}
		else if (MainMenuUI.Instance != null)
		{
			yesNoConfirmation = MainMenuUI.Instance.yesNoConfirmation;
		}
		if (yesNoConfirmation != null && !yesNoConfirmation.isShowing)
		{
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Open_Browser");
			string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Report_Bug_Description");
			yesNoConfirmation.ShowYesNoConfirmation(localizedValue, localizedValue2, delegate
			{
				Application.OpenURL("https://forms.gle/gcoa8oHxywFLegNx7");
			}, null, showCover: true, 50);
		}
	}

	public static void AddOnUpdateEvent(Action p_event)
	{
		m_onUpdateEvent = (Action)Delegate.Combine(m_onUpdateEvent, p_event);
	}

	public static void RemoveOnUpdateEvent(Action p_event)
	{
		m_onUpdateEvent = (Action)Delegate.Remove(m_onUpdateEvent, p_event);
	}

	public void CharacterCenterCycleForward()
	{
		if (DatabaseManager.Instance.characterDatabase.aliveVillagersList != null && DatabaseManager.Instance.characterDatabase.aliveVillagersList.Count > 0)
		{
			ISelectable nextCharacterToCenter = GetNextCharacterToCenter(DatabaseManager.Instance.characterDatabase.aliveVillagersList);
			if (nextCharacterToCenter != null)
			{
				Select(nextCharacterToCenter);
			}
		}
	}

	public void CharacterCenterCycleForward(List<Character> p_charactersToCycle)
	{
		if (p_charactersToCycle.Count > 0)
		{
			ISelectable nextCharacterToCenter = GetNextCharacterToCenter(p_charactersToCycle);
			if (nextCharacterToCenter != null)
			{
				Select(nextCharacterToCenter);
			}
		}
	}

	private void CharacterCenterCycleBackward()
	{
		if (DatabaseManager.Instance.characterDatabase.aliveVillagersList != null && DatabaseManager.Instance.characterDatabase.aliveVillagersList.Count > 0)
		{
			ISelectable previousCharacterToCenter = GetPreviousCharacterToCenter(DatabaseManager.Instance.characterDatabase.aliveVillagersList);
			if (previousCharacterToCenter != null)
			{
				Select(previousCharacterToCenter);
			}
		}
	}

	private Character GetNextCharacterToCenter(List<Character> selectables)
	{
		Character character = null;
		for (int i = 0; i < selectables.Count; i++)
		{
			if (selectables[i].IsCurrentlySelected())
			{
				character = CollectionUtilities.GetNextElementCyclic(selectables, i);
				break;
			}
		}
		if (character == null)
		{
			character = selectables[0];
		}
		return character;
	}

	private Character GetPreviousCharacterToCenter(List<Character> selectables)
	{
		Character character = null;
		for (int i = 0; i < selectables.Count; i++)
		{
			if (selectables[i].IsCurrentlySelected())
			{
				character = CollectionUtilities.GetPreviousElementCyclic(selectables, i);
				break;
			}
		}
		if (character == null)
		{
			character = selectables[0];
		}
		return character;
	}

	private void DemonicStructureCycle(STRUCTURE_TYPE p_type)
	{
		List<DemonicStructure> list = new List<DemonicStructure>();
		if (DatabaseManager.Instance.structureDatabase.allStructures != null)
		{
			for (int i = 0; i < DatabaseManager.Instance.structureDatabase.allStructures.Count; i++)
			{
				if (DatabaseManager.Instance.structureDatabase.allStructures[i].structureType == p_type && !DatabaseManager.Instance.structureDatabase.allStructures[i].hasBeenDestroyed)
				{
					if (p_type != STRUCTURE_TYPE.MARAUD && DatabaseManager.Instance.structureDatabase.allStructures[i].charactersHere.Count <= 0)
					{
						list.Add(DatabaseManager.Instance.structureDatabase.allStructures[i] as DemonicStructure);
					}
					else if (p_type == STRUCTURE_TYPE.MARAUD)
					{
						list.Add(DatabaseManager.Instance.structureDatabase.allStructures[i] as DemonicStructure);
					}
				}
			}
		}
		if (list != null && list.Count > 0)
		{
			ISelectable nextDemonicStructureOfType = GetNextDemonicStructureOfType(list, p_type);
			if (nextDemonicStructureOfType != null)
			{
				Select(nextDemonicStructureOfType as LocationStructure);
			}
		}
	}

	private LocationStructure GetNextDemonicStructureOfType(List<DemonicStructure> selectables, STRUCTURE_TYPE p_type)
	{
		LocationStructure locationStructure = null;
		for (int i = 0; i < selectables.Count; i++)
		{
			if (selectables[i] != null && selectables[i].structureType == p_type && selectables[i].IsCurrentlySelected())
			{
				locationStructure = CollectionUtilities.GetNextElementCyclic(selectables, i);
				break;
			}
		}
		if (locationStructure == null)
		{
			locationStructure = selectables[0];
		}
		return locationStructure;
	}

	private void EyesCycle()
	{
		DemonEye demonEye = null;
		if (UIManager.Instance != null && UIManager.Instance.GetCurrentlySelectedPOI() != null && UIManager.Instance.GetCurrentlySelectedPOI() is DemonEye demonEye2)
		{
			demonEye = demonEye2;
		}
		if (allDemonicStructureOfSameType == null || allDemonicStructureOfSameType.Count <= 0)
		{
			return;
		}
		if (demonEye != null)
		{
			if (demonEye.GetBeholderOwner().eyeWards.IndexOf(demonEye) + 1 < demonEye.GetBeholderOwner().eyeWards.Count)
			{
				Select(demonEye.GetBeholderOwner().eyeWards[demonEye.GetBeholderOwner().eyeWards.IndexOf(demonEye) + 1]);
				return;
			}
			Watcher watcher = GetNextEye(allDemonicStructureOfSameType, demonEye.GetBeholderOwner()) as Watcher;
			if (watcher.eyeWards.Count > 0)
			{
				Select(watcher.eyeWards[0]);
			}
		}
		else if (GetNextEye(allDemonicStructureOfSameType, null) is Watcher watcher2 && watcher2.eyeWards.Count > 0)
		{
			Select(watcher2.eyeWards[0]);
		}
	}

	private LocationStructure GetNextEye(List<DemonicStructure> selectables, Watcher p_currentWatcher)
	{
		LocationStructure locationStructure = null;
		int num = selectables.IndexOf(p_currentWatcher);
		if (num + 1 < selectables.Count)
		{
			locationStructure = selectables[num + 1];
		}
		if (locationStructure == null)
		{
			locationStructure = selectables[0];
		}
		return locationStructure;
	}

	public bool GetMouseButtonDown(int p_mouseButton)
	{
		if (isUsingGamepad)
		{
			return p_mouseButton switch
			{
				0 => gamepadCursor.virtualMouse.leftButton.wasPressedThisFrame, 
				1 => gamepadCursor.virtualMouse.rightButton.wasPressedThisFrame, 
				2 => gamepadCursor.virtualMouse.middleButton.wasPressedThisFrame, 
				_ => false, 
			};
		}
		return p_mouseButton switch
		{
			0 => Mouse.current.leftButton.wasPressedThisFrame, 
			1 => Mouse.current.rightButton.wasPressedThisFrame, 
			2 => Mouse.current.middleButton.wasPressedThisFrame, 
			_ => false, 
		};
	}

	public bool GetMouseButton(int p_mouseButton)
	{
		if (isUsingGamepad)
		{
			return p_mouseButton switch
			{
				0 => gamepadCursor.virtualMouse.leftButton.isPressed, 
				1 => gamepadCursor.virtualMouse.rightButton.isPressed, 
				2 => gamepadCursor.virtualMouse.middleButton.isPressed, 
				_ => false, 
			};
		}
		return p_mouseButton switch
		{
			0 => Mouse.current.leftButton.isPressed, 
			1 => Mouse.current.rightButton.isPressed, 
			2 => Mouse.current.middleButton.isPressed, 
			_ => false, 
		};
	}

	public bool GetMouseButtonUp(int p_mouseButton)
	{
		if (isUsingGamepad)
		{
			return p_mouseButton switch
			{
				0 => gamepadCursor.virtualMouse.leftButton.wasReleasedThisFrame, 
				1 => gamepadCursor.virtualMouse.rightButton.wasReleasedThisFrame, 
				2 => gamepadCursor.virtualMouse.middleButton.wasReleasedThisFrame, 
				_ => false, 
			};
		}
		return p_mouseButton switch
		{
			0 => Mouse.current.leftButton.wasReleasedThisFrame, 
			1 => Mouse.current.rightButton.wasReleasedThisFrame, 
			2 => Mouse.current.middleButton.wasReleasedThisFrame, 
			_ => false, 
		};
	}

	public void OnControlsChanged(PlayerInput p_playerInput)
	{
		if (p_playerInput.currentControlScheme == "Keyboard" && previousControlScheme != "Keyboard")
		{
			_currentMouse.WarpCursorPosition(gamepadCursor.position);
			Cursor.visible = true;
			gamepadCursor.Disable();
			previousControlScheme = "Keyboard";
			SettingsManager.Instance.OnControlDeviceChanged(p_playerInput.currentControlScheme);
			Messenger.Broadcast(ControlsSignals.CONTROL_DEVICE_CHANGED, p_playerInput.currentControlScheme);
		}
		else if (p_playerInput.currentControlScheme == "Gamepad" && previousControlScheme != "Gamepad")
		{
			gamepadCursor.Enable(_currentMouse.position.ReadValue());
			Cursor.visible = false;
			previousControlScheme = "Gamepad";
			SettingsManager.Instance.OnControlDeviceChanged(p_playerInput.currentControlScheme);
			Messenger.Broadcast(ControlsSignals.CONTROL_DEVICE_CHANGED, p_playerInput.currentControlScheme);
		}
	}

	public void SetGamepadCursorState(bool p_state)
	{
		if (isUsingGamepad)
		{
			if (p_state)
			{
				gamepadCursor.Enable();
			}
			else
			{
				gamepadCursor.Disable();
			}
		}
	}

	private void OnIncreaseProgressionSpeed(InputAction.CallbackContext p_context)
	{
		if (p_context.performed)
		{
			TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Increase_Speed);
		}
	}

	private void OnDecreaseProgressionSpeed(InputAction.CallbackContext p_context)
	{
		if (p_context.performed)
		{
			TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Decrease_Speed);
		}
	}

	private void OnLeftClick(InputAction.CallbackContext p_context)
	{
		if (p_context.performed)
		{
			if (isUsingGamepad)
			{
				if (Gamepad.current.aButton.wasPressedThisFrame)
				{
					TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Left_Click);
				}
			}
			else
			{
				TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Left_Click);
			}
		}
		gamepadCursor.OnLeftClick(p_context.performed);
	}

	private void MiddleClick(InputAction.CallbackContext p_context)
	{
		if (p_context.performed)
		{
			TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Middle_Click);
		}
	}

	private void RightClick(InputAction.CallbackContext p_context)
	{
		if (p_context.performed)
		{
			TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Right_Click);
		}
	}

	private void ToggleSpells(InputAction.CallbackContext p_context)
	{
		if (p_context.performed)
		{
			TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Spells);
		}
	}

	private void ToggleStructures(InputAction.CallbackContext p_context)
	{
		if (p_context.performed)
		{
			TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Structures);
		}
	}

	private void ToggleMonsters(InputAction.CallbackContext p_context)
	{
		if (p_context.performed)
		{
			TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Monsters);
		}
	}

	private void ToggleIntel(InputAction.CallbackContext p_context)
	{
		if (p_context.performed)
		{
			TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Intel);
		}
	}

	private void ToggleTargets(InputAction.CallbackContext p_context)
	{
		if (p_context.performed)
		{
			TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Targets);
		}
	}

	public void ToggleVillagers(InputAction.CallbackContext p_context)
	{
		if (p_context.performed)
		{
			TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Villagers);
		}
	}

	private void ToggleCultists(InputAction.CallbackContext p_context)
	{
		if (p_context.performed)
		{
			TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Cultists);
		}
	}

	private void ToggleTutorials(InputAction.CallbackContext p_context)
	{
		if (p_context.performed)
		{
			TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Tutorials);
		}
	}

	private void ToggleGoals(InputAction.CallbackContext p_context)
	{
		if (p_context.performed)
		{
			TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Goals);
		}
	}

	private void ToggleSubGoals(InputAction.CallbackContext p_context)
	{
		if (p_context.performed)
		{
			TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Sub_Goals);
		}
	}

	private void QuickSave(InputAction.CallbackContext p_context)
	{
		if (p_context.performed)
		{
			TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Quick_Save);
		}
	}

	private void QuickLoad(InputAction.CallbackContext p_context)
	{
		if (p_context.performed)
		{
			TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Quick_Load);
		}
	}

	private void TogglePause(InputAction.CallbackContext p_context)
	{
		if (p_context.performed)
		{
			TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Toggle_Pause);
		}
	}

	private void SnatchVillager(InputAction.CallbackContext p_context)
	{
		if (p_context.performed)
		{
			TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Snatch_Villager);
		}
	}

	private void SnatchMonster(InputAction.CallbackContext p_context)
	{
		if (p_context.performed)
		{
			TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Snatch_Monster);
		}
	}

	private void Raid(InputAction.CallbackContext p_context)
	{
		if (p_context.performed)
		{
			TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Raid);
		}
	}

	private void CenterPortal(InputAction.CallbackContext p_context)
	{
		if (p_context.performed)
		{
			TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Center_Portal);
		}
	}

	private void CycleEyes(InputAction.CallbackContext p_context)
	{
		if (p_context.performed)
		{
			TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Cycle_Eyes);
		}
	}

	private void CycleVillagersForward(InputAction.CallbackContext p_context)
	{
		if (p_context.performed)
		{
			TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Cycle_Villagers_Forward);
		}
	}

	private void CycleVillagersBackward(InputAction.CallbackContext p_context)
	{
		if (p_context.performed)
		{
			TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Cycle_Villagers_Backward);
		}
	}

	private void ToggleNameplates(InputAction.CallbackContext p_context)
	{
		if (p_context.performed)
		{
			TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Toggle_Nameplate);
		}
	}

	private void ReportABug(InputAction.CallbackContext p_context)
	{
		if (p_context.performed)
		{
			TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Report_A_Bug);
		}
	}

	private void Cancel(InputAction.CallbackContext p_context)
	{
		if (p_context.performed)
		{
			TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Cancel);
		}
	}

	private void QuickDestroy(InputAction.CallbackContext p_context)
	{
		if (p_context.performed)
		{
			TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Quick_Destroy);
		}
	}

	private void DemolishHold(InputAction.CallbackContext p_context)
	{
		if (p_context.performed)
		{
			TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Demolish_Hold);
		}
		else if (p_context.canceled)
		{
			TryExecuteShortcutKeyUpAction(SHORTCUT_ACTION.Demolish_Hold);
		}
	}

	private void ToggleConsole(InputAction.CallbackContext p_context)
	{
		if (p_context.performed)
		{
			TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Toggle_Console);
		}
	}

	private void RotateDecoration(InputAction.CallbackContext p_context)
	{
		if (p_context.performed)
		{
			TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Rotate_Decoration);
		}
	}

	private void CenterOnSelectedObject(InputAction.CallbackContext p_context)
	{
		if (p_context.performed)
		{
			TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Center_Selected_Object);
		}
	}

	private void ShowOptionsMenu(InputAction.CallbackContext p_context)
	{
		if (p_context.performed)
		{
			TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Toggle_Options_Menu);
		}
	}

	private void ShowContextMenu(InputAction.CallbackContext p_context)
	{
		if (p_context.performed)
		{
			TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Show_Context_Menu);
		}
	}

	private void ConfirmationWindowConfirm(InputAction.CallbackContext p_context)
	{
		if (p_context.performed)
		{
			TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Confirmation_Window_Confirm);
		}
	}

	private void ConfirmationWindowCancel(InputAction.CallbackContext p_context)
	{
		if (p_context.performed)
		{
			TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Confirmation_Window_Cancel);
		}
	}

	private void CycleTopMenus(InputAction.CallbackContext p_context)
	{
		if (p_context.performed)
		{
			TryExecuteShortcutKeyDownAction(SHORTCUT_ACTION.Cycle_Top_Menus);
		}
	}

	public void SetInputMapState(string p_mapName, bool p_state)
	{
		InputActionMap inputActionMap = playerInput.actions.FindActionMap(p_mapName);
		if (p_state)
		{
			inputActionMap.Enable();
		}
		else
		{
			inputActionMap.Disable();
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using System.IO;
using Ruinarch;
using Ruinarch.Custom_UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UtilityScripts;

namespace Settings;

public class SettingsManager : MonoBehaviour
{
	public static SettingsManager Instance;

	private string _settingFileLocation;

	private string _keybindsFileLocation;

	public GameObject settingsGO;

	public GameObject keyBindsGo;

	[SerializeField]
	private DefaultSettingsData defaultSettingsData;

	[Header("Graphics Settings UI")]
	public TMP_Dropdown resolutionsDropdown;

	public TMP_Dropdown graphicsDropdown;

	public Toggle fullscreenToggle;

	public int targetFrameRate;

	[Header("Gameplay Settings UI")]
	[SerializeField]
	private TMP_Dropdown languageDropdown;

	[SerializeField]
	private GameObject languageGO;

	[SerializeField]
	private Toggle edgePanningToggle;

	[SerializeField]
	private Toggle confineCursorToggle;

	[SerializeField]
	private Toggle vsyncToggle;

	[SerializeField]
	private Toggle showVideosToggle;

	[SerializeField]
	private Toggle cameraShakeToggle;

	[SerializeField]
	private Toggle randomizeMonsterNamesToggle;

	[SerializeField]
	private Toggle arachnophobiaToggle;

	[SerializeField]
	private Toggle autosaveToggle;

	[SerializeField]
	private Button resetTutorialsBtn;

	[SerializeField]
	private Slider panSpeedSlider;

	[SerializeField]
	private Slider logLimitSlider;

	[SerializeField]
	private TextMeshProUGUI logLimitValueLbl;

	[SerializeField]
	private TMP_InputField logLimitInputField;

	[SerializeField]
	private Transform keybindItemsParent;

	[SerializeField]
	private GameObject keybindCover;

	[SerializeField]
	private TextMeshProUGUI keybindCoverText;

	[SerializeField]
	private RuinarchButton keybindsBtn;

	[SerializeField]
	private GameObject goControllerLayoutWindow;

	[SerializeField]
	private HoverHandler hoverHandlerEdgePanning;

	[SerializeField]
	private HoverHandler hoverHandlerConfineCursor;

	[SerializeField]
	private HoverHandler hoverHandlerCameraShake;

	[SerializeField]
	private HoverHandler hoverHandlerRandomizeMonsterNames;

	[SerializeField]
	private HoverHandler hoverHandlerLanguageDropdown;

	[Header("Audio Settings UI")]
	[SerializeField]
	private Slider masterVolumeSlider;

	[SerializeField]
	private Slider musicVolumeSlider;

	[SerializeField]
	private Slider sfxVolumeSlider;

	private List<Resolution> resolutions;

	private Settings _settings;

	private KeybindItem[] _keybindItems;

	private const int Minimum_Log_Limit = 1000;

	private const int Maximum_Log_Limit = 5000;

	public bool hasShownEarlyAccessAnnouncement { get; private set; }

	public Settings settings => _settings;

	public bool doNotShowVideos => true;

	public string keybindsFileLocation => _keybindsFileLocation;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			Object.DontDestroyOnLoad(base.gameObject);
			Application.targetFrameRate = targetFrameRate;
			SceneManager.sceneLoaded += OnSceneLoaded;
			LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
			_settingFileLocation = Application.persistentDataPath + "/Settings.ini";
			_keybindsFileLocation = Application.persistentDataPath + "/CustomKeybinds.json";
			Initialize();
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
		ConstructGraphicsQuality();
		languageGO.SetActive(LocalizationManager.Instance.allowChangeLanguage);
		_keybindItems = keybindItemsParent.GetComponentsInChildren<KeybindItem>();
		for (int i = 0; i < _keybindItems.Length; i++)
		{
			KeybindItem obj = _keybindItems[i];
			obj.Initialize(keybindCover, keybindCoverText);
			obj.UpdateKeybindDisplay();
		}
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
			SceneManager.sceneLoaded -= OnSceneLoaded;
			LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
		}
	}

	private void OnSelectedLocaleChanged(Locale obj)
	{
		ConstructGraphicsQuality();
		UpdateUI();
	}

	private void OnApplicationQuit()
	{
		SaveSettingsFile();
	}

	private void OnEditorQuit()
	{
		SaveSettingsFile();
	}

	private void Initialize()
	{
		masterVolumeSlider.minValue = AudioManager.Minimum_Volume_Level;
		masterVolumeSlider.maxValue = AudioManager.Maximum_Volume_Level;
		musicVolumeSlider.minValue = AudioManager.Minimum_Volume_Level;
		musicVolumeSlider.maxValue = AudioManager.Maximum_Volume_Level;
		sfxVolumeSlider.minValue = AudioManager.Minimum_Volume_Level;
		sfxVolumeSlider.maxValue = AudioManager.Maximum_Volume_Level;
		LoadSettings();
		ConstructResolutions();
		StartCoroutine(ConstructLanguages());
		masterVolumeSlider.value = settings.masterVolume;
		musicVolumeSlider.value = settings.musicVolume;
		sfxVolumeSlider.value = settings.sfxVolume;
		hoverHandlerRandomizeMonsterNames.AddOnHoverOverAction(OnHoverOverRandomizeMonsterNames);
		hoverHandlerRandomizeMonsterNames.AddOnHoverOutAction(OnHoverOutRandomizeMonsterNames);
		hoverHandlerCameraShake.AddOnHoverOverAction(OnHoverOverCameraShake);
		hoverHandlerCameraShake.AddOnHoverOutAction(OnHoverOutCameraShake);
		hoverHandlerEdgePanning.AddOnHoverOverAction(OnHoverOverEdgePanning);
		hoverHandlerEdgePanning.AddOnHoverOutAction(OnHoverOutEdgePanning);
		hoverHandlerConfineCursor.AddOnHoverOverAction(OnHoverOverConfineCursor);
		hoverHandlerConfineCursor.AddOnHoverOutAction(OnHoverOutConfineCursor);
		hoverHandlerLanguageDropdown.AddOnHoverOverAction(OnHoverOverLanguageDropdown);
		hoverHandlerLanguageDropdown.AddOnHoverOutAction(OnHoverOutLanguageDropdown);
		resetTutorialsBtn.onClick.AddListener(ResetAlerts);
		languageDropdown.onValueChanged.AddListener(OnChangeLanguage);
	}

	private void OnSceneLoaded(Scene p_newScene, LoadSceneMode p_mode)
	{
		if (p_newScene.name == "Game")
		{
			languageDropdown.interactable = false;
		}
		else
		{
			if (!(p_newScene.name == "MainMenu"))
			{
				return;
			}
			languageDropdown.interactable = true;
			for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; i++)
			{
				Locale locale = LocalizationSettings.AvailableLocales.Locales[i];
				if (_settings.language == locale.LocaleName)
				{
					LocalizationSettings.SelectedLocale = locale;
					break;
				}
			}
		}
	}

	private void ConstructResolutions()
	{
		resolutionsDropdown.ClearOptions();
		resolutions = new List<Resolution>();
		for (int i = 0; i < Screen.resolutions.Length; i++)
		{
			if (!HasResolution(Screen.resolutions[i]))
			{
				resolutions.Add(Screen.resolutions[i]);
			}
		}
		List<string> list = new List<string>();
		for (int j = 0; j < resolutions.Count; j++)
		{
			list.Add($"{resolutions[j].width}x{resolutions[j].height}");
		}
		resolutionsDropdown.AddOptions(list);
	}

	private IEnumerator ConstructLanguages()
	{
		yield return LocalizationSettings.InitializationOperation;
		languageDropdown.ClearOptions();
		List<string> list = new List<string>();
		Locale locale = null;
		for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; i++)
		{
			Locale locale2 = LocalizationSettings.AvailableLocales.Locales[i];
			list.Add(locale2.LocaleName);
			if (_settings.language == locale2.LocaleName)
			{
				locale = locale2;
			}
		}
		languageDropdown.AddOptions(list);
		if (locale != null)
		{
			LocalizationSettings.SelectedLocale = locale;
		}
	}

	private bool HasResolution(Resolution resolution)
	{
		for (int i = 0; i < resolutions.Count; i++)
		{
			if (resolutions[i].width == resolution.width && resolutions[i].height == resolution.height)
			{
				return true;
			}
		}
		return false;
	}

	private void ConstructGraphicsQuality()
	{
		graphicsDropdown.ClearOptions();
		List<string> list = RuinarchListPool<string>.Claim();
		for (int i = 0; i < QualitySettings.names.Length; i++)
		{
			string key = QualitySettings.names[i];
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", key);
			list.Add(localizedValue);
		}
		graphicsDropdown.AddOptions(list);
		RuinarchListPool<string>.Release(list);
	}

	public void OpenSettings()
	{
		UpdateUI();
		settingsGO.SetActive(value: true);
		InputManager.Instance.SetAllHotkeysEnabledState(p_state: false);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Cancel, p_state: true);
	}

	public void CloseSettings()
	{
		settingsGO.SetActive(value: false);
		InputManager.Instance.SetAllHotkeysEnabledState(p_state: true);
	}

	public void OpenKeyBinds()
	{
		keyBindsGo.SetActive(value: true);
		InputManager.Instance.SetInputMapState("Gameplay", p_state: false);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Cancel, p_state: false);
	}

	public void CloseKeyBinds()
	{
		keyBindsGo.SetActive(value: false);
		if (SceneManager.GetActiveScene().name == "Game")
		{
			InputManager.Instance.SetInputMapState("Gameplay", p_state: true);
		}
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Cancel, p_state: true);
	}

	public void OnControlDeviceChanged(string p_deviceName)
	{
		keybindsBtn.interactable = !InputManager.Instance.isUsingGamepad;
		if (InputManager.Instance.isUsingGamepad && keyBindsGo.activeInHierarchy)
		{
			CloseKeyBinds();
		}
	}

	public bool IsShowing()
	{
		return settingsGO.activeSelf;
	}

	private void UpdateUI()
	{
		edgePanningToggle.isOn = settings.useEdgePanning;
		confineCursorToggle.SetIsOnWithoutNotify(settings.confineCursor);
		cameraShakeToggle.SetIsOnWithoutNotify(settings.disableCameraShake);
		randomizeMonsterNamesToggle.SetIsOnWithoutNotify(settings.randomizeMonsterNames);
		arachnophobiaToggle.SetIsOnWithoutNotify(settings.arachnophobiaToggle);
		autosaveToggle.SetIsOnWithoutNotify(settings.shouldAutosave);
		languageDropdown.SetValueWithoutNotify(GameUtilities.GetOptionIndex(languageDropdown, settings.language));
		logLimitSlider.value = settings.logLimit;
		panSpeedSlider.value = settings.cameraPanSpeed;
		resolutionsDropdown.value = GameUtilities.GetOptionIndex(resolutionsDropdown, settings.resolution);
		graphicsDropdown.value = settings.graphicsQuality;
		fullscreenToggle.isOn = settings.fullscreen;
		masterVolumeSlider.value = settings.masterVolume;
		musicVolumeSlider.value = settings.musicVolume;
		sfxVolumeSlider.value = settings.sfxVolume;
		vsyncToggle.isOn = settings.isVsyncOn;
		showVideosToggle.isOn = !settings.doNotShowVideos;
	}

	public void OnToggleEdgePanning(bool isOn)
	{
		_settings.useEdgePanning = isOn;
		Messenger.Broadcast(SettingsSignals.EDGE_PANNING_TOGGLED, isOn);
	}

	private void OnHoverOverEdgePanning()
	{
		Tooltip.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Edge_Panning_Tooltip"), LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Edge Panning"), autoReplaceText: false);
	}

	private void OnHoverOutEdgePanning()
	{
		Tooltip.Instance.HideSmallInfo();
	}

	private void LoadSettings()
	{
		if (Utilities.DoesFileExist(_settingFileLocation))
		{
			StreamReader streamReader = new StreamReader(_settingFileLocation);
			string text = streamReader.ReadToEnd();
			streamReader.Close();
			text = text.Trim();
			if (!string.IsNullOrEmpty(text))
			{
				_settings = JsonUtility.FromJson<Settings>(text);
				if (_settings.cameraPanSpeed == 0)
				{
					_settings.cameraPanSpeed = defaultSettingsData.defaultSettings.cameraPanSpeed;
				}
				if (!text.Contains("sfxVolume"))
				{
					_settings.sfxVolume = defaultSettingsData.defaultSettings.sfxVolume;
				}
				if (_settings.masterVolume < AudioManager.Minimum_Volume_Level)
				{
					_settings.masterVolume = defaultSettingsData.defaultSettings.masterVolume;
				}
				if (_settings.musicVolume < AudioManager.Minimum_Volume_Level)
				{
					_settings.musicVolume = defaultSettingsData.defaultSettings.musicVolume;
				}
				if (_settings.sfxVolume < AudioManager.Minimum_Volume_Level)
				{
					_settings.sfxVolume = defaultSettingsData.defaultSettings.sfxVolume;
				}
				if (string.IsNullOrEmpty(_settings.language))
				{
					_settings.language = defaultSettingsData.defaultSettings.language;
				}
				if (_settings.logLimit < 1000 || _settings.logLimit > 5000)
				{
					_settings.logLimit = defaultSettingsData.defaultSettings.logLimit;
				}
			}
			else
			{
				_settings = new Settings(defaultSettingsData.defaultSettings);
				Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, settings.fullscreen);
				QualitySettings.SetQualityLevel(settings.graphicsQuality);
			}
		}
		else
		{
			_settings = new Settings(defaultSettingsData.defaultSettings);
			Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, settings.fullscreen);
			QualitySettings.SetQualityLevel(settings.graphicsQuality);
		}
		SetVsync(_settings.isVsyncOn);
		SetConfineCursor(_settings.confineCursor);
	}

	public void ApplySettings()
	{
		SetGraphicsQuality();
		SetResolution();
		SetFullscreen();
		_settings.useEdgePanning = edgePanningToggle.isOn;
		_settings.isVsyncOn = vsyncToggle.isOn;
		_settings.doNotShowVideos = !showVideosToggle.isOn;
		_settings.confineCursor = confineCursorToggle.isOn;
		ApplyConfineCursorSetting();
		Screen.fullScreen = settings.fullscreen;
		string[] array = settings.resolution.Split('x');
		int width = int.Parse(array[0]);
		int height = int.Parse(array[1]);
		Screen.SetResolution(width, height, settings.fullscreen);
		QualitySettings.SetQualityLevel(settings.graphicsQuality);
		SetVsync(_settings.isVsyncOn);
		SaveSettingsFile();
	}

	private void SetGraphicsQuality()
	{
		_settings.graphicsQuality = graphicsDropdown.value;
	}

	private void SetResolution()
	{
		_settings.resolution = resolutionsDropdown.options[resolutionsDropdown.value].text;
	}

	private void SetFullscreen()
	{
		_settings.fullscreen = fullscreenToggle.isOn;
	}

	private void SaveSettingsFile()
	{
		string value = JsonUtility.ToJson(settings, prettyPrint: true);
		StreamWriter streamWriter = new StreamWriter(_settingFileLocation, append: false);
		streamWriter.WriteLine(value);
		streamWriter.Close();
		string value2 = InputManager.Instance.playerInput.actions.SaveBindingOverridesAsJson();
		if (!string.IsNullOrEmpty(value2))
		{
			StreamWriter streamWriter2 = new StreamWriter(_keybindsFileLocation, append: false);
			streamWriter2.WriteLine(value2);
			streamWriter2.Close();
		}
	}

	public void OnCameraPanSpeedChanged(float p_amount)
	{
		_settings.cameraPanSpeed = (int)p_amount;
	}

	public void OnMusicVolumeChanged(float volume)
	{
		if (_settings != null)
		{
			_settings.musicVolume = volume;
		}
		Messenger.Broadcast(SettingsSignals.MUSIC_VOLUME_CHANGED, volume);
	}

	public void OnMasterVolumeChanged(float volume)
	{
		if (_settings != null)
		{
			_settings.masterVolume = volume;
		}
		Messenger.Broadcast(SettingsSignals.MASTER_VOLUME_CHANGED, volume);
	}

	public void OnSFXVolumeChanged(float volume)
	{
		if (_settings != null)
		{
			_settings.sfxVolume = volume;
		}
		Messenger.Broadcast(SettingsSignals.SFX_VOLUME_CHANGED, volume);
	}

	public void SetVsync(bool state)
	{
		if (state)
		{
			QualitySettings.vSyncCount = 1;
		}
		else
		{
			QualitySettings.vSyncCount = 0;
		}
	}

	public void OnToggleConfineCursor(bool state)
	{
		SetConfineCursor(state);
	}

	private void SetConfineCursor(bool state)
	{
		_settings.confineCursor = state;
		ApplyConfineCursorSetting();
	}

	private void ApplyConfineCursorSetting()
	{
		if (_settings.confineCursor)
		{
			Cursor.lockState = CursorLockMode.Confined;
		}
		else
		{
			Cursor.lockState = CursorLockMode.None;
		}
	}

	private void OnHoverOverConfineCursor()
	{
		Tooltip.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Confine_Cursor_Tooltip"), LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Confine Cursor"), autoReplaceText: false);
	}

	private void OnHoverOutConfineCursor()
	{
		Tooltip.Instance.HideSmallInfo();
	}

	public void OnToggleSkipEarlyAccessAnnouncement(bool state)
	{
		_settings.skipEarlyAccessAnnouncement = state;
	}

	public void SetHasShownEarlyAccessAnnouncement(bool state)
	{
		hasShownEarlyAccessAnnouncement = state;
	}

	public void OnToggleCameraShake(bool p_isOn)
	{
		cameraShakeToggle.isOn = p_isOn;
		_settings.disableCameraShake = p_isOn;
	}

	private void OnHoverOverCameraShake()
	{
	}

	private void OnHoverOutCameraShake()
	{
	}

	public void OnToggleRandomizeMonsterNames(bool p_isOn)
	{
		randomizeMonsterNamesToggle.isOn = p_isOn;
		_settings.randomizeMonsterNames = p_isOn;
	}

	private void OnHoverOverRandomizeMonsterNames()
	{
		Tooltip.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Randomize_Monster_Names_Tooltip"), LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Randomize Monster Names"), autoReplaceText: false);
	}

	private void OnHoverOutRandomizeMonsterNames()
	{
		Tooltip.Instance.HideSmallInfo();
	}

	private void ResetAlerts()
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
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Reset_Tutorials");
			string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Reset_Tutorials_Description");
			yesNoConfirmation.ShowYesNoConfirmation(localizedValue, localizedValue2, OnConfirmResetAlerts, null, showCover: true, 50);
		}
	}

	private void OnConfirmResetAlerts()
	{
		SaveManager.Instance.currentSaveDataPlayer.ResetTutorialAlerts();
		Messenger.Broadcast(SettingsSignals.ALERTS_CLEARED);
	}

	public void OnToggleArachnophobia(bool p_isOn)
	{
		arachnophobiaToggle.isOn = p_isOn;
		_settings.arachnophobiaToggle = p_isOn;
		Messenger.Broadcast(SettingsSignals.ARACHNOPHOBIA_TOGGLED, _settings.arachnophobiaToggle);
	}

	public void OnToggleAutosave(bool p_isOn)
	{
		_settings.shouldAutosave = p_isOn;
	}

	private void OnChangeLanguage(int p_index)
	{
		if (LocalizationSettings.AvailableLocales.Locales.IsIndexInList(p_index))
		{
			Locale locale = LocalizationSettings.AvailableLocales.Locales[p_index];
			_settings.language = locale.LocaleName;
			LocalizationSettings.SelectedLocale = locale;
		}
	}

	private void OnHoverOutLanguageDropdown()
	{
		if (!languageDropdown.interactable)
		{
			Tooltip.Instance.HideSmallInfo();
		}
	}

	private void OnHoverOverLanguageDropdown()
	{
		if (!languageDropdown.interactable)
		{
			Tooltip.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Language_Unable_Tooltip"));
		}
	}

	private void OnEndEditLogLimit(string p_value)
	{
		int num;
		int result;
		if (string.IsNullOrEmpty(p_value))
		{
			num = 5000;
			logLimitInputField.SetTextWithoutNotify(5000.ToString());
		}
		else if (int.TryParse(p_value, out result))
		{
			if (result < 1000)
			{
				num = 1000;
				logLimitInputField.SetTextWithoutNotify(1000.ToString());
			}
			else if (result > 5000)
			{
				num = 5000;
				logLimitInputField.SetTextWithoutNotify(5000.ToString());
			}
			else
			{
				num = result;
			}
		}
		else
		{
			num = 5000;
		}
		if (num != _settings.logLimit)
		{
			_settings.logLimit = num;
			Messenger.Broadcast(SettingsSignals.LOG_LIMIT_CHANGED);
		}
	}

	public void OnLogLimitSliderChanged(float value)
	{
		_settings.logLimit = (int)value;
		logLimitValueLbl.text = value.ToString();
		if (value != (float)_settings.logLimit)
		{
			Messenger.Broadcast(SettingsSignals.LOG_LIMIT_CHANGED);
		}
	}

	public void ResetControlsToDefault()
	{
		foreach (InputActionMap actionMap in InputManager.Instance.playerInput.actions.actionMaps)
		{
			actionMap.RemoveAllBindingOverrides();
			foreach (InputAction action in actionMap.actions)
			{
				Messenger.Broadcast(ControlsSignals.UPDATED_KEYBIND, action);
			}
		}
	}

	public void OpenControllerLayoutWindow()
	{
		goControllerLayoutWindow.SetActive(value: true);
	}

	public void CloseControllerLayoutWindow()
	{
		goControllerLayoutWindow.SetActive(value: false);
	}
}

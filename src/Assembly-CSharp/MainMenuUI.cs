using System.IO;
using System.IO.Compression;
using Managers;
using Ruinarch.Custom_UI;
using Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class MainMenuUI : MonoBehaviour
{
	public static MainMenuUI Instance;

	[Header("Developer Settings")]
	[SerializeField]
	private bool allowEarlyAccessAnnouncement;

	[SerializeField]
	private bool allowRoadmap;

	[Space(5f)]
	[SerializeField]
	private EasyTween buttonsTween;

	[SerializeField]
	private EasyTween titleTween;

	[SerializeField]
	private EasyTween glowTween;

	[SerializeField]
	private EasyTween glow2Tween;

	[SerializeField]
	private Image bg;

	[SerializeField]
	private TextMeshProUGUI _headerText;

	[Header("Buttons")]
	[SerializeField]
	private Button continueButton;

	[SerializeField]
	private Button newGameButton;

	[Header("Steam")]
	[SerializeField]
	private TextMeshProUGUI steamName;

	[Header("Version")]
	[SerializeField]
	private TextMeshProUGUI version;

	[Header("Yes/No Confirmation")]
	public YesNoConfirmation yesNoConfirmation;

	[Header("Load Game")]
	[SerializeField]
	private Button loadGameButton;

	[Header("Early Access Announcement")]
	[SerializeField]
	private GameObject earlyAccessAnnouncementGO;

	[SerializeField]
	private GameObject roadmapGO;

	[SerializeField]
	private RuinarchToggle skipEarlyAccessAnnouncementToggle;

	[Header("General Confirmation")]
	public GeneralConfirmation generalConfirmation;

	[Header("Mods UI")]
	[SerializeField]
	private ModParentUI _modParentUI;

	private bool _isLatestSaveFileVersionValid;

	private bool _isLatestSaveFileModCompatible;

	private string _latestSaveFilePath;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		if (!SaveManager.Instance.savePlayerManager.hasSavedDataPlayer)
		{
			SaveManager.Instance.savePlayerManager.CreateNewSaveDataPlayer();
		}
		newGameButton.interactable = true;
		steamName.text = "Logged in as: <b>" + SteamworksManager.Instance.GetSteamName() + "</b>";
		version.text = "Version: " + Application.version;
		SaveManager.Instance.saveCurrentProgressManager.SetCurrentSaveDataPath(string.Empty);
		UpdateButtonStates();
		Messenger.AddListener<string>(UISignals.SAVE_FILE_DELETED, OnSaveFileDeleted);
		if (ExternalFileManager.Instance.hasInitialized)
		{
			InitializeLatestSaveFile();
		}
	}

	public void ShowMenuButtons()
	{
		titleTween.OnValueChangedAnimation(value: true);
		glowTween.OnValueChangedAnimation(value: true);
		buttonsTween.OnValueChangedAnimation(value: true);
	}

	private void HideMenuButtons()
	{
		buttonsTween.OnValueChangedAnimation(value: false);
	}

	public void ShowEarlyAccessAnnouncement()
	{
		if (allowEarlyAccessAnnouncement)
		{
			if (!SettingsManager.Instance.hasShownEarlyAccessAnnouncement)
			{
				SettingsManager.Instance.SetHasShownEarlyAccessAnnouncement(state: true);
				if (SettingsManager.Instance.settings.skipEarlyAccessAnnouncement)
				{
					earlyAccessAnnouncementGO.SetActive(value: false);
					SetRoadmapGOState(p_state: true);
				}
				else
				{
					skipEarlyAccessAnnouncementToggle.isOn = SettingsManager.Instance.settings.skipEarlyAccessAnnouncement;
					earlyAccessAnnouncementGO.SetActive(value: true);
					SetRoadmapGOState(p_state: false);
				}
			}
		}
		else
		{
			SetRoadmapGOState(p_state: true);
		}
	}

	private void SetRoadmapGOState(bool p_state)
	{
		if (!p_state || allowRoadmap)
		{
			roadmapGO.SetActive(p_state);
		}
	}

	public void OnClickOkEarlyAccessAnnouncement()
	{
		earlyAccessAnnouncementGO.SetActive(value: false);
		SetRoadmapGOState(p_state: true);
	}

	public void ExitGame()
	{
		Application.Quit();
	}

	public void OnClickPlayGame()
	{
		WorldSettings.Instance.Open();
	}

	public void OnClickContinue()
	{
		AudioManager.Instance.TryPlayUISFX("Play_Click_Continue");
		if (!string.IsNullOrEmpty(_latestSaveFilePath))
		{
			if (!File.Exists(_latestSaveFilePath))
			{
				InitializeLatestSaveFile();
			}
			if (!_isLatestSaveFileVersionValid)
			{
				string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Incompatible_Save");
				string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Incompatible_Save_Description");
				yesNoConfirmation.ShowYesNoConfirmation(localizedValue, localizedValue2, delegate
				{
					OnConfirmDelete(_latestSaveFilePath);
				}, null, showCover: true, 50);
			}
			else if (!_isLatestSaveFileModCompatible)
			{
				string localizedValue3 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Incompatible_Save");
				string localizedValue4 = LocalizationManager.Instance.GetLocalizedValue("OtherUI_Table", "Saved_Mods_Incompatible");
				generalConfirmation.ShowGeneralConfirmation(localizedValue3, localizedValue4);
			}
			else
			{
				SaveManager.Instance.saveCurrentProgressManager.SetCurrentSaveDataPath(_latestSaveFilePath);
				MainMenuManager.Instance.StartGame();
			}
		}
		else
		{
			OnClickPlayGame();
		}
	}

	public void OnClickSettings()
	{
		SettingsManager.Instance.OpenSettings();
	}

	public void OnClickDiscord()
	{
		Application.OpenURL(GameUtilities.Discord_Link);
	}

	private void UpdateButtonStates()
	{
		bool interactable = SaveManager.Instance.saveCurrentProgressManager.HasAnySaveFiles();
		continueButton.interactable = interactable;
		loadGameButton.interactable = interactable;
	}

	private void OnConfirmDelete(string path)
	{
		File.Delete(path);
		Messenger.Broadcast(UISignals.SAVE_FILE_DELETED, path);
	}

	public void InitializeLatestSaveFile()
	{
		_latestSaveFilePath = SaveManager.Instance.saveCurrentProgressManager.GetLatestSaveFile();
		if (!string.IsNullOrEmpty(_latestSaveFilePath))
		{
			CheckSaveFileValidAndCompatible(_latestSaveFilePath);
		}
	}

	private void CheckSaveFileValidAndCompatible(string p_filePath)
	{
		string empty = string.Empty;
		using ZipArchive zipArchive = ZipFile.Open(p_filePath, ZipArchiveMode.Read);
		foreach (ZipArchiveEntry entry in zipArchive.Entries)
		{
			if (entry.Name == "mainSave.sav")
			{
				empty = string.Empty;
				using (StreamReader streamReader = new StreamReader(entry.Open()))
				{
					empty = streamReader.ReadToEnd();
				}
				if (!string.IsNullOrEmpty(empty))
				{
					string gameVersionOfSaveFile = SaveUtilities.GetGameVersionOfSaveFile(empty);
					_isLatestSaveFileVersionValid = SaveUtilities.IsSaveFileVersionCompatible(gameVersionOfSaveFile);
					if (!_isLatestSaveFileVersionValid)
					{
						break;
					}
				}
			}
			else
			{
				if (!(entry.Name == "quickInfo.json"))
				{
					continue;
				}
				empty = string.Empty;
				using (StreamReader streamReader2 = new StreamReader(entry.Open()))
				{
					empty = streamReader2.ReadToEnd();
				}
				if (!string.IsNullOrEmpty(empty))
				{
					SaveDataQuickInfo loadedInfo = JsonUtility.FromJson<SaveDataQuickInfo>(empty);
					_isLatestSaveFileModCompatible = SaveUtilities.AreAppliedModsCompatible(loadedInfo);
					if (!_isLatestSaveFileModCompatible)
					{
						break;
					}
				}
			}
		}
	}

	public void ShowHeaderText(string p_text)
	{
		_headerText.text = p_text;
		_headerText.gameObject.SetActive(value: true);
	}

	public void HideHeaderText()
	{
		_headerText.gameObject.SetActive(value: false);
	}

	public void OnClickMods()
	{
		_modParentUI.Show();
	}

	private void OnSaveFileDeleted(string saveFileDeleted)
	{
		UpdateButtonStates();
		if (!SaveManager.Instance.saveCurrentProgressManager.HasAnySaveFiles())
		{
			SaveWindowUIController.Instance.HideUI();
		}
	}

	public void OnClickLoadGame()
	{
		SaveWindowUIController.Instance.ShowLoadWindow();
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Maccima_Games.Util;
using Ruinarch.MVCFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UtilityScripts;

public class SaveWindowUIController : MVCUIController, SaveWindowUIView.IListener
{
	public enum Window_Function
	{
		Save,
		Load
	}

	public static SaveWindowUIController Instance;

	[SerializeField]
	private SaveWindowUIModel m_saveWindowUIModel;

	private SaveWindowUIView m_saveWindowUIView;

	public bool isShowing;

	private List<SavedGameItem> _activeItems;

	private SavedGameItem _currentlyShowingSaveInfo;

	private SavedGameItem _fileToRename;

	private Texture2D _texture2D;

	private string _fileToOverwritePath;

	private string _fileToOverwriteFileName;

	private Window_Function _windowFunction;

	public Window_Function windowFunction => _windowFunction;

	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		SaveWindowUIView.Create(_canvas, m_saveWindowUIModel, delegate(SaveWindowUIView p_ui)
		{
			m_saveWindowUIView = p_ui;
			m_saveWindowUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
			p_ui.UIModel.transform.SetSiblingIndex(siblingIndex);
			HideUI();
		});
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			_activeItems = new List<SavedGameItem>();
			_texture2D = new Texture2D(2, 2);
			InstantiateUI();
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void OnDestroy()
	{
		m_saveWindowUIView?.Unsubscribe(this);
	}

	public override void ShowUI()
	{
		base.ShowUI();
		isShowing = true;
	}

	public override void HideUI()
	{
		base.HideUI();
		isShowing = false;
	}

	public void ShowSaveWindow()
	{
		_windowFunction = Window_Function.Save;
		ShowUI();
		m_saveWindowUIView.SetTitle(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Save Game"));
		m_saveWindowUIView.SetSaveLoadBtnText(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Save"));
		_currentlyShowingSaveInfo = null;
		HideInputNewSaveFileName();
		CreateSaveItems();
		m_saveWindowUIView.UIModel.createNewSaveItemRect.gameObject.SetActive(value: true);
	}

	public void ShowLoadWindow()
	{
		_windowFunction = Window_Function.Load;
		ShowUI();
		m_saveWindowUIView.SetTitle(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Load Game"));
		m_saveWindowUIView.SetSaveLoadBtnText(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Load"));
		_currentlyShowingSaveInfo = null;
		HideInputNewSaveFileName();
		CreateSaveItems();
		m_saveWindowUIView.UIModel.createNewSaveItemRect.gameObject.SetActive(value: false);
	}

	private void CreateSaveItems()
	{
		for (int i = 0; i < _activeItems.Count; i++)
		{
			SavedGameItem pooledObject = _activeItems[i];
			ObjectPoolManager.Instance.DestroyObjectWithoutCheckingChildren(pooledObject);
		}
		_activeItems.Clear();
		string[] files = Directory.GetFiles(Utilities.gameSavePath, "*.zip");
		files = files.OrderBy(File.GetLastWriteTime).ToArray();
		for (int j = 0; j < files.Length; j++)
		{
			string p_savePath = files[j];
			SavedGameItem savedGameItem = CreateSaveItem(p_savePath, isFromAutosave: false);
			if (files.IsLastIndex(j))
			{
				savedGameItem.ToggleOn();
				OnSelectSaveItem(savedGameItem);
			}
		}
		string[] files2 = Directory.GetFiles(Utilities.autosavePath, "*.zip");
		if (files2 != null && files2.Length != 0)
		{
			files2 = files2.OrderBy(File.GetLastWriteTime).ToArray();
			for (int k = 0; k < files2.Length; k++)
			{
				string p_savePath2 = files2[k];
				SavedGameItem savedGameItem2 = CreateSaveItem(p_savePath2, isFromAutosave: true);
				if (files2.IsLastIndex(k))
				{
					savedGameItem2.ToggleOn();
					OnSelectSaveItem(savedGameItem2);
				}
			}
		}
		if (_currentlyShowingSaveInfo == null)
		{
			RemoveSaveItemSelection();
		}
		m_saveWindowUIView.UIModel.createNewSaveItemRect.SetAsFirstSibling();
	}

	private SavedGameItem CreateSaveItem(string p_savePath, bool isFromAutosave)
	{
		SavedGameItem component = ObjectPoolManager.Instance.InstantiateObjectFromPool(m_saveWindowUIView.UIModel.saveItemPrefab.name, Vector3.zero, Quaternion.identity, m_saveWindowUIView.UIModel.saveItemsScrollView.content).GetComponent<SavedGameItem>();
		component.Initialize(p_savePath, OnSelectSaveItem, OnClickDeleteSaveItem, OnClickRenameSaveItem, m_saveWindowUIView.UIModel.saveItemsToggleGroup, isFromAutosave);
		_activeItems.Add(component);
		(component.transform as RectTransform)?.SetAsFirstSibling();
		return component;
	}

	private void DeleteSaveItem(SavedGameItem p_item)
	{
		ObjectPoolManager.Instance.DestroyObjectWithoutCheckingChildren(p_item);
		_activeItems.Remove(p_item);
	}

	private void OnSelectSaveItem(SavedGameItem p_item)
	{
		_currentlyShowingSaveInfo = p_item;
		ShowSaveInfo(_currentlyShowingSaveInfo);
	}

	private void OnClickRenameSaveItem(SavedGameItem p_item)
	{
		_fileToRename = p_item;
		m_saveWindowUIView.UIModel.renameSaveGO.gameObject.SetActive(value: true);
		m_saveWindowUIView.UIModel.renameSaveInputField.text = p_item.fileName;
		m_saveWindowUIView.UIModel.renameSaveInputField.Select();
	}

	private void OnClickDeleteSaveItem(SavedGameItem p_item)
	{
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Delete_Save");
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
		dictionary.Add("fileName", p_item.fileName);
		string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Delete_Save_Description", dictionary);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
		ShowYesNoConfirmation(localizedValue, localizedValue2, delegate
		{
			OnConfirmDelete(p_item);
		});
	}

	private void RemoveSaveItemSelection()
	{
		_currentlyShowingSaveInfo = null;
		m_saveWindowUIView.SetSaveInfo(string.Empty);
		m_saveWindowUIView.SetSaveNameWithoutNotify("No Save File Selected");
		m_saveWindowUIView.SetScreenshotState(p_state: false);
		m_saveWindowUIView.SetSaveErrorMessage(string.Empty);
		m_saveWindowUIView.UIModel.saveLoadBtn.interactable = false;
	}

	private void ShowSaveInfo(SavedGameItem p_save)
	{
		SaveDataQuickInfo saveDataQuickInfo = null;
		string text = Utilities.tempZipPath + "screen.png";
		bool flag = false;
		using (ZipArchive zipArchive = ZipFile.Open(p_save.savePath, ZipArchiveMode.Read))
		{
			foreach (ZipArchiveEntry entry in zipArchive.Entries)
			{
				if (entry.Name == "quickInfo.json")
				{
					string json = string.Empty;
					using (StreamReader streamReader = new StreamReader(entry.Open()))
					{
						json = streamReader.ReadToEnd();
					}
					saveDataQuickInfo = JsonUtility.FromJson<SaveDataQuickInfo>(json);
				}
				else if (entry.Name == "screen.png")
				{
					if (File.Exists(text))
					{
						File.Delete(text);
					}
					entry.ExtractToFile(text);
					flag = true;
				}
			}
		}
		m_saveWindowUIView.UIModel.saveLoadBtn.interactable = true;
		if (File.Exists(text) && flag)
		{
			byte[] data = File.ReadAllBytes(text);
			_texture2D.LoadImage(data);
			m_saveWindowUIView.SetSaveScreenshot(_texture2D);
			m_saveWindowUIView.SetScreenshotState(p_state: true);
		}
		else
		{
			m_saveWindowUIView.SetScreenshotState(p_state: false);
		}
		m_saveWindowUIView.SetSaveNameWithoutNotify(p_save.fileName);
		string empty = string.Empty;
		string text2 = string.Empty;
		DateTime lastWriteTime = File.GetLastWriteTime(p_save.savePath);
		if (saveDataQuickInfo != null)
		{
			text2 = saveDataQuickInfo.saveVersion;
			empty = ((!IsSaveFileCompatible(text2)) ? (empty + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Save Version") + " " + Utilities.ColorizeInvalidText(saveDataQuickInfo.saveVersion)) : (empty + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Save Version") + " " + saveDataQuickInfo.saveVersion));
			empty = empty + "\n" + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Timestamp") + " " + lastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
		}
		else
		{
			empty = empty + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Save Version") + " --";
			empty = empty + "\n" + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Timestamp") + " --";
			empty = empty + "\n" + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Archetype") + " --";
		}
		m_saveWindowUIView.SetSaveInfo(empty);
		m_saveWindowUIView.UIModel.saveNameInputField.interactable = false;
		DateTime t = new DateTime(2023, 4, 26, 9, 0, 0);
		if (!IsSaveFileCompatible(text2))
		{
			m_saveWindowUIView.SetSaveErrorMessage(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Save_File_Incompatible") + " " + Application.version);
			if (windowFunction == Window_Function.Load)
			{
				m_saveWindowUIView.UIModel.saveLoadBtn.interactable = false;
			}
		}
		else if (!SaveUtilities.AreAppliedModsCompatible(saveDataQuickInfo))
		{
			m_saveWindowUIView.SetSaveErrorMessage(LocalizationManager.Instance.GetLocalizedValue("OtherUI_Table", "Saved_Mods_Incompatible") ?? "");
			if (windowFunction == Window_Function.Load)
			{
				m_saveWindowUIView.UIModel.saveLoadBtn.interactable = false;
			}
		}
		else if (text2 == "1.0" && DateTime.Compare(lastWriteTime, t) <= 0)
		{
			m_saveWindowUIView.SetSaveErrorMessage(LocalizationManager.Instance.GetLocalizedValue("OtherUI_Table", "Load_1.0_Warning"));
		}
		else
		{
			m_saveWindowUIView.SetSaveErrorMessage(string.Empty);
		}
		if (File.Exists(text))
		{
			File.Delete(text);
		}
	}

	private bool IsSaveFileCompatible(string saveFileVersion)
	{
		return SaveUtilities.IsSaveFileVersionCompatible(saveFileVersion);
	}

	private void ShowYesNoConfirmation(string header, string question, Action onClickYes)
	{
		if (MainMenuUI.Instance != null)
		{
			MainMenuUI.Instance.yesNoConfirmation.ShowYesNoConfirmation(header, question, onClickYes, null, showCover: true, 100);
		}
		else if (UIManager.Instance != null)
		{
			UIManager.Instance.yesNoConfirmation.ShowYesNoConfirmation(header, question, onClickYes, null, showCover: true, 100);
		}
	}

	public void OnClickClose()
	{
		HideUI();
	}

	public void OnClickSaveLoad()
	{
		if (_currentlyShowingSaveInfo != null)
		{
			if (windowFunction == Window_Function.Save)
			{
				_fileToOverwritePath = _currentlyShowingSaveInfo.savePath;
				_fileToOverwriteFileName = _currentlyShowingSaveInfo.fileName;
				string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Overwrite_Save_File");
				Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
				dictionary.Add("fileName", _currentlyShowingSaveInfo.fileName);
				string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Overwrite_Save_File_Description", dictionary);
				MaccimaDictionaryPool<string, string>.Release(dictionary);
				ShowYesNoConfirmation(localizedValue, localizedValue2, OverwriteFile);
			}
			else if (windowFunction == Window_Function.Load)
			{
				AskForLoadConfirmation();
			}
		}
	}

	private void OnConfirmDelete(SavedGameItem itemToDelete)
	{
		string savePath = itemToDelete.savePath;
		File.Delete(itemToDelete.savePath);
		DeleteSaveItem(itemToDelete);
		SavedGameItem savedGameItem = _activeItems.LastOrDefault();
		Messenger.Broadcast(UISignals.SAVE_FILE_DELETED, savePath);
		if (savedGameItem != null)
		{
			savedGameItem.ToggleOn();
		}
		else
		{
			RemoveSaveItemSelection();
		}
	}

	public void OnClickCreateNewSave()
	{
		ShowInputSaveFileName();
	}

	public void OnConfirmNewSaveFileName()
	{
		HideInputNewSaveFileName();
		string text = m_saveWindowUIView.UIModel.createNewSaveInputField.text;
		string[] files = Directory.GetFiles(Utilities.gameSavePath, "*.zip");
		bool flag = false;
		string text2 = string.Empty;
		foreach (string path in files)
		{
			if (string.Equals(text, Path.GetFileNameWithoutExtension(path), StringComparison.InvariantCultureIgnoreCase))
			{
				flag = true;
				text2 = Path.GetFileNameWithoutExtension(path) + ".zip";
				break;
			}
		}
		if (flag)
		{
			_fileToOverwritePath = Utilities.gameSavePath + text2;
			_fileToOverwriteFileName = text;
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Overwrite_Save_File");
			Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
			dictionary.Add("fileName", text);
			string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Overwrite_Save_File_Description", dictionary);
			MaccimaDictionaryPool<string, string>.Release(dictionary);
			ShowYesNoConfirmation(localizedValue, localizedValue2, OverwriteFile);
		}
		else
		{
			SaveGame(text);
			HideUI();
		}
	}

	public void OnNewSaveFileNameValueChanged(string p_value)
	{
		if (string.IsNullOrEmpty(p_value))
		{
			m_saveWindowUIView.UIModel.confirmCreateNewSaveBtn.interactable = false;
		}
		else
		{
			m_saveWindowUIView.UIModel.confirmCreateNewSaveBtn.interactable = true;
		}
	}

	public void OnSubmitNewSaveFileName(string p_value)
	{
		OnConfirmNewSaveFileName();
	}

	public void OnClickConfirmRenameSaveFile()
	{
		if (TryRenameSaveFile())
		{
			_fileToRename = null;
			m_saveWindowUIView.UIModel.renameSaveGO.SetActive(value: false);
		}
	}

	public void OnSubmitRenameSaveFile()
	{
		if (TryRenameSaveFile())
		{
			_fileToRename = null;
			m_saveWindowUIView.UIModel.renameSaveGO.SetActive(value: false);
		}
	}

	private bool TryRenameSaveFile()
	{
		string text = m_saveWindowUIView.UIModel.renameSaveInputField.text;
		if (!string.Equals(_fileToRename.fileName, text, StringComparison.InvariantCultureIgnoreCase))
		{
			if (string.IsNullOrEmpty(text))
			{
				string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "File_Name_Empty_Title");
				string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "File_Name_Empty_Description");
				if (PlayerUI.Instance != null)
				{
					PlayerUI.Instance.ShowGeneralConfirmation(localizedValue, localizedValue2);
				}
				else if (MainMenuUI.Instance != null)
				{
					MainMenuUI.Instance.generalConfirmation.ShowGeneralConfirmation(localizedValue, localizedValue2);
				}
				return false;
			}
			if (File.Exists(Utilities.gameSavePath + text + ".zip"))
			{
				string localizedValue3 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "File_Exists_Title");
				Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
				dictionary.Add("fileName", text);
				string localizedValue4 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "File_Exists_Description", dictionary);
				MaccimaDictionaryPool<string, string>.Release(dictionary);
				if (PlayerUI.Instance != null)
				{
					PlayerUI.Instance.ShowGeneralConfirmation(localizedValue3, localizedValue4);
				}
				else if (MainMenuUI.Instance != null)
				{
					MainMenuUI.Instance.generalConfirmation.ShowGeneralConfirmation(localizedValue3, localizedValue4);
				}
				m_saveWindowUIView.SetSaveNameWithoutNotify(_fileToRename.fileName);
			}
			else
			{
				_fileToRename.Rename(text);
				m_saveWindowUIView.SetSaveNameWithoutNotify(_fileToRename.fileName);
			}
		}
		return true;
	}

	private void ShowInputSaveFileName()
	{
		m_saveWindowUIView.UIModel.createNewSaveGO.gameObject.SetActive(value: true);
		m_saveWindowUIView.UIModel.createNewSaveInputField.text = SaveManager.Instance.saveCurrentProgressManager.GetFileName();
		m_saveWindowUIView.UIModel.createNewSaveInputField.Select();
	}

	private void HideInputNewSaveFileName()
	{
		m_saveWindowUIView.UIModel.createNewSaveGO.gameObject.SetActive(value: false);
	}

	private void SaveGame(string p_fileName)
	{
		if (!SaveManager.Instance.saveCurrentProgressManager.isSaving && !SaveManager.Instance.saveCurrentProgressManager.isWritingToDisk)
		{
			OptionsMenu.SaveCurrentProgress(p_fileName);
		}
	}

	private void OverwriteFile()
	{
		File.Delete(_fileToOverwritePath);
		SaveGame(_fileToOverwriteFileName);
		HideUI();
		_fileToOverwritePath = string.Empty;
		_fileToOverwriteFileName = string.Empty;
	}

	private void AskForLoadConfirmation()
	{
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Load_Game");
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
		dictionary.Add("fileName", _currentlyShowingSaveInfo.fileName);
		string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Load_Game_Description", dictionary);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
		ShowYesNoConfirmation(localizedValue, localizedValue2, OnConfirmLoad);
	}

	private void OnConfirmLoad()
	{
		OnLoadFileChosen(_currentlyShowingSaveInfo.savePath);
	}

	private void OnLoadFileChosen(string path)
	{
		HideUI();
		SaveManager.Instance.saveCurrentProgressManager.SetCurrentSaveDataPath(path);
		Scene activeScene = SceneManager.GetActiveScene();
		if (activeScene.name == "MainMenu")
		{
			MainMenuManager.Instance.StartGame();
		}
		else if (activeScene.name == "Game")
		{
			UIManager.Instance.optionsMenu.LoadSave();
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using Maccima_Games.Util;
using Ruinarch;
using Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class OptionsMenu : PopupMenuBase
{
	[SerializeField]
	private GameObject saveLoadingGO;

	[SerializeField]
	private TextMeshProUGUI saveLbl;

	[SerializeField]
	private Button saveBtn;

	[SerializeField]
	private Button exitBtn;

	[SerializeField]
	private Button saveAndExitBtn;

	[SerializeField]
	private Button loadBtn;

	[SerializeField]
	private GameObject saveBtnSavingIcon;

	[SerializeField]
	private GameObject saveAndExitBtnSavingIcon;

	[SerializeField]
	private GameObject exitBtnSavingIcon;

	[SerializeField]
	private GameObject loadBtnSavingIcon;

	[SerializeField]
	private GameObject inputSaveNameGO;

	[SerializeField]
	private InputField saveNameInput;

	[SerializeField]
	private Button btnSaveFileName;

	private Action m_saveAction;

	private string m_fileNameToDelete;

	public void SubscribeListeners()
	{
		Messenger.AddListener<SHORTCUT_ACTION>(ControlsSignals.PLAYER_INPUT_ACTION, OnReceivePlayerInputAction);
	}

	private void OnReceivePlayerInputAction(SHORTCUT_ACTION p_action)
	{
		if (p_action == SHORTCUT_ACTION.Quick_Save)
		{
			QuickSave();
		}
	}

	public override void Open()
	{
		saveNameInput.onValueChanged.AddListener(OnInputSaveFileNameChanged);
		UIManager.Instance.Pause();
		UIManager.Instance.SetSpeedTogglesState(state: false);
		base.Open();
		UpdateButtonsForSaving();
		InputManager.Instance.SetAllHotkeysEnabledState(p_state: false);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Cancel, p_state: true);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Toggle_Options_Menu, p_state: true);
	}

	public override void Close()
	{
		saveNameInput.onValueChanged.RemoveListener(OnInputSaveFileNameChanged);
		UIManager.Instance.ResumeLastProgressionSpeed();
		base.Close();
		InputManager.Instance.SetAllHotkeysEnabledState(p_state: true);
	}

	public void OpenSettings()
	{
		SettingsManager.Instance.OpenSettings();
	}

	public void SaveGame()
	{
		SaveWindowUIController.Instance.ShowSaveWindow();
	}

	public void SaveAndExit()
	{
		m_saveAction = ActualSaveAndExit;
		ShowInputFileName();
	}

	private void ActualSaveAndExit()
	{
		if (!SaveManager.Instance.saveCurrentProgressManager.isSaving && !SaveManager.Instance.saveCurrentProgressManager.isWritingToDisk)
		{
			InputManager.Instance.SetAllHotkeysEnabledState(p_state: false);
			SaveCurrentProgress(saveNameInput.text);
			saveLoadingGO.SetActive(value: true);
			StartCoroutine(WaitForLoadToFinishThenExit());
		}
	}

	private IEnumerator WaitForLoadToFinishThenExit()
	{
		while (SaveManager.Instance.saveCurrentProgressManager.isWritingToDisk || SaveManager.Instance.saveCurrentProgressManager.isSaving)
		{
			if (!saveLoadingGO.activeSelf)
			{
				saveLoadingGO.SetActive(value: true);
			}
			yield return 0;
		}
		saveLoadingGO.SetActive(value: false);
		Application.Quit();
		InputManager.Instance.SetAllHotkeysEnabledState(p_state: true);
	}

	public void QuickSave()
	{
		if (!SaveManager.Instance.saveCurrentProgressManager.isSaving && !SaveManager.Instance.saveCurrentProgressManager.isWritingToDisk && SaveManager.Instance.saveCurrentProgressManager.CanSaveCurrentProgress())
		{
			UIManager.Instance.Pause();
			UIManager.Instance.SetSpeedTogglesState(state: false);
			SaveManager.Instance.savePlayerManager.SavePlayerData();
			SaveCurrentProgress("", UIManager.Instance.ResumeLastProgressionSpeed);
		}
	}

	public void OnClickLoadGame()
	{
		UIManager.Instance.OpenLoadWindow();
	}

	public void ExitGame()
	{
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Exit Game");
		string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Exit_Game_Confirmation");
		UIManager.Instance.ShowYesNoConfirmation(localizedValue, localizedValue2, Application.Quit, null, showCover: true, 50);
	}

	public void AbandonWorld()
	{
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Abandon World");
		string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Abandon_World_Confirmation");
		UIManager.Instance.ShowYesNoConfirmation(localizedValue, localizedValue2, Abandon, null, showCover: true, 50);
	}

	public void ReportABug()
	{
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Open_Browser");
		string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Report_Bug_Description");
		UIManager.Instance.ShowYesNoConfirmation(localizedValue, localizedValue2, delegate
		{
			Application.OpenURL("https://forms.gle/gcoa8oHxywFLegNx7");
		}, null, showCover: true, 50);
	}

	public void SubmitFeedback()
	{
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Open_Browser");
		string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Submit_Feedback_Confirmation");
		UIManager.Instance.ShowYesNoConfirmation(localizedValue, localizedValue2, delegate
		{
			Application.OpenURL("https://forms.gle/6QYHiSmU8ySVGSXp7");
		}, null, showCover: true, 50);
	}

	public void OnInputSaveFileNameChanged(string p_fileName)
	{
		if (p_fileName.Length <= 0)
		{
			btnSaveFileName.interactable = false;
		}
		else
		{
			btnSaveFileName.interactable = true;
		}
	}

	private void ShowInputFileName()
	{
		saveNameInput.text = SaveManager.Instance.saveCurrentProgressManager.GetFileName();
		inputSaveNameGO.gameObject.SetActive(value: true);
	}

	public void HideInputFileName()
	{
		inputSaveNameGO.gameObject.SetActive(value: false);
	}

	public void SaveNameFromInputName()
	{
		HideInputFileName();
		string[] files = Directory.GetFiles(Utilities.gameSavePath, "*.zip");
		bool flag = false;
		string text = string.Empty;
		for (int i = 0; i < files.Length; i++)
		{
			if (saveNameInput.text == Path.GetFileNameWithoutExtension(files[i]))
			{
				flag = true;
				text = Path.GetFileNameWithoutExtension(files[i]) + ".zip";
				break;
			}
		}
		if (flag)
		{
			m_fileNameToDelete = Utilities.gameSavePath + text;
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Overwrite_Save_File");
			Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
			dictionary.Add("fileName", text);
			string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Overwrite_Save_File_Description", dictionary);
			MaccimaDictionaryPool<string, string>.Release(dictionary);
			UIManager.Instance.ShowYesNoConfirmation(localizedValue, localizedValue2, DeleteExistingFileThenSave, null, showCover: true, 50);
		}
		else
		{
			m_saveAction?.Invoke();
		}
	}

	private void DeleteExistingFileThenSave()
	{
		File.Delete(m_fileNameToDelete);
		m_saveAction?.Invoke();
	}

	private void Abandon()
	{
		DOTween.Clear(destroy: true);
		LevelLoaderManager.Instance.UpdateLoadingInfo(string.Empty);
		LevelLoaderManager.Instance.LoadLevel("MainMenu", updateSceneProgress: true);
	}

	public void LoadSave()
	{
		DOTween.Clear(destroy: true);
		LevelLoaderManager.Instance.SetLoadingState(state: true);
		LevelLoaderManager.Instance.UpdateLoadingInfo("Initializing_Data");
		LevelLoaderManager.Instance.UpdateLoadingBar(0.1f, 3f);
		LevelLoaderManager.Instance.LoadLevel("Game");
	}

	public void ShowSaveLoading()
	{
		saveLoadingGO.SetActive(value: true);
	}

	public void HideSaveLoading()
	{
		saveLoadingGO.SetActive(value: false);
	}

	public void UpdateSaveMessage(string message)
	{
		saveLbl.text = message;
	}

	public void UpdateButtonsForSaving()
	{
		bool flag = SaveManager.Instance.saveCurrentProgressManager.isSaving || SaveManager.Instance.saveCurrentProgressManager.isWritingToDisk;
		saveBtn.interactable = SaveManager.Instance.saveCurrentProgressManager.CanSaveCurrentProgress() && !flag;
		saveBtnSavingIcon.gameObject.SetActive(flag);
		exitBtn.interactable = !flag;
		exitBtnSavingIcon.gameObject.SetActive(flag);
		saveAndExitBtn.interactable = !flag;
		saveAndExitBtnSavingIcon.gameObject.SetActive(flag);
		loadBtn.interactable = !flag;
		loadBtnSavingIcon.gameObject.SetActive(flag);
	}

	public static void SaveCurrentProgress(string fileName = "", Action saveCallback = null)
	{
		if (SaveManager.Instance.saveCurrentProgressManager.CanSaveCurrentProgress())
		{
			SaveManager.Instance.saveCurrentProgressManager.DoManualSave(fileName, saveCallback);
			return;
		}
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Save_Progress");
		string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Save_Seizing");
		PlayerUI.Instance.ShowGeneralConfirmation(localizedValue, localizedValue2);
	}
}

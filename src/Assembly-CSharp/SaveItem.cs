using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Maccima_Games.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveItem : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI saveNameLbl;

	[SerializeField]
	private TextMeshProUGUI timeStampLbl;

	[SerializeField]
	private InputField inpFldNewSaveName;

	[SerializeField]
	private Button btnSave;

	private bool m_isTyping;

	private string path;

	private string json;

	public void SetSaveFile(string path)
	{
		this.path = path;
		DateTime lastWriteTime = File.GetLastWriteTime(path);
		timeStampLbl.text = lastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
		string text = (fileNameWithoutExtension.Contains('(') ? fileNameWithoutExtension.Substring(0, fileNameWithoutExtension.IndexOf('(')).Replace(" ", "").Replace('_', ' ')
			.Replace('-', ':') : fileNameWithoutExtension);
		saveNameLbl.text = text.Replace(' ', '-');
	}

	public void OnClickItem()
	{
		if (string.IsNullOrEmpty(json))
		{
			using ZipArchive zipArchive = ZipFile.Open(path, ZipArchiveMode.Read);
			foreach (ZipArchiveEntry entry in zipArchive.Entries)
			{
				if (entry.Name == "mainSave.sav")
				{
					using (StreamReader streamReader = new StreamReader(entry.Open()))
					{
						json = streamReader.ReadToEnd();
					}
					break;
				}
			}
		}
		string gameVersionOfSaveFile = SaveUtilities.GetGameVersionOfSaveFile(json);
		if (gameVersionOfSaveFile != Application.version && !SaveUtilities.compatibleSaveFileVersions.Contains(gameVersionOfSaveFile))
		{
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Incompatible_Save");
			string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Incompatible_Save_Description");
			if (MainMenuUI.Instance != null)
			{
				MainMenuUI.Instance.yesNoConfirmation.ShowYesNoConfirmation(localizedValue, localizedValue2, OnConfirmDelete, null, showCover: true, 50);
			}
			else if (UIManager.Instance != null)
			{
				UIManager.Instance.yesNoConfirmation.ShowYesNoConfirmation(localizedValue, localizedValue2, OnConfirmDelete, null, showCover: true, 50);
			}
			return;
		}
		string localizedValue3 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Load_Game");
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
		dictionary.Add("fileName", saveNameLbl.text);
		string localizedValue4 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Load_Game_Description", dictionary);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
		if (MainMenuUI.Instance != null)
		{
			MainMenuUI.Instance.yesNoConfirmation.ShowYesNoConfirmation(localizedValue3, localizedValue4, OnConfirmLoad, null, showCover: true, 50);
		}
		else if (UIManager.Instance != null)
		{
			UIManager.Instance.yesNoConfirmation.ShowYesNoConfirmation(localizedValue3, localizedValue4, OnConfirmLoad, null, showCover: true, 50);
		}
	}

	public void OnClickRename()
	{
		btnSave.GetComponentInChildren<Text>().text = "Save";
		inpFldNewSaveName.gameObject.SetActive(value: true);
	}

	private void OnConfirmLoad()
	{
		Messenger.Broadcast(UISignals.LOAD_SAVE_FILE, path);
	}

	public void OnClickDelete()
	{
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Delete_Save");
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
		dictionary.Add("fileName", saveNameLbl.text);
		string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Delete_Save_Description", dictionary);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
		if (MainMenuUI.Instance != null)
		{
			MainMenuUI.Instance.yesNoConfirmation.ShowYesNoConfirmation(localizedValue, localizedValue2, OnConfirmDelete, null, showCover: true, 50);
		}
		else if (UIManager.Instance != null)
		{
			UIManager.Instance.yesNoConfirmation.ShowYesNoConfirmation(localizedValue, localizedValue2, OnConfirmDelete, null, showCover: true, 50);
		}
	}

	private void OnConfirmDelete()
	{
		File.Delete(path);
		UnityEngine.Object.Destroy(base.gameObject);
		Messenger.Broadcast(UISignals.SAVE_FILE_DELETED, path);
	}

	private void OnDestroy()
	{
		path = string.Empty;
		json = string.Empty;
	}
}

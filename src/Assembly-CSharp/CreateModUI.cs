using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using LapinerTools.Steam;
using LapinerTools.Steam.Data;
using LapinerTools.Steam.Data.Internal;
using Maccima_Games.Util;
using Ruinarch.Custom_UI;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class CreateModUI : MonoBehaviour
{
	[SerializeField]
	private GameObject _windowGO;

	[SerializeField]
	private TMP_InputField _modFolderPathField;

	[SerializeField]
	private GameObject _uploadProgressGO;

	[SerializeField]
	private RuinarchText _uploadProgressText;

	[Header("Folder Section")]
	[SerializeField]
	private ModFolderItem _modFolderItemPrefab;

	[SerializeField]
	private ScrollRect _modFoldersPanel;

	[SerializeField]
	private ToggleGroup _modFoldersToggleGroup;

	[Header("Preview Section")]
	[SerializeField]
	private RawImage _modPreviewImage;

	[SerializeField]
	private TMP_InputField _modNameField;

	[SerializeField]
	private TMP_InputField _modDescriptionField;

	[SerializeField]
	private RuinarchButton _uploadButton;

	[SerializeField]
	private RuinarchButton _subscribeButton;

	[SerializeField]
	private TMP_Dropdown _tagDropdown;

	[Header("Pooling")]
	[SerializeField]
	private Transform _poolParent;

	private Queue<ModFolderItem> _pooledItems = new Queue<ModFolderItem>(10);

	private string _currentPreviewImagePath;

	private ModFolderItem _currentlySelectedMFI;

	private List<ModFolderItem> _currentActiveModFolders = new List<ModFolderItem>(10);

	private WorkshopItemUpdate _workshopItem = new WorkshopItemUpdate();

	private bool _isUploading;

	private string _localizedUploadingText => LocalizationManager.Instance.GetLocalizedValue("OtherUI_Table", "Uploading");

	public void OnOpenParentModUI()
	{
	}

	public void ShowHide(bool p_state)
	{
		if (p_state)
		{
			Show();
		}
		else
		{
			Hide();
		}
	}

	private void Show()
	{
		UpdatePreviewSection();
		UpdateButtons();
		_windowGO.SetActive(value: true);
	}

	public void Hide()
	{
		_windowGO.SetActive(value: false);
	}

	private void Start()
	{
		if (SteamWorkshopMain.IsInstanceSet)
		{
			SteamWorkshopMain.Instance.OnUploaded += OnUploadDone;
			SteamWorkshopMain.Instance.OnError += OnErrorUpload;
		}
	}

	public void LoadModFolders()
	{
		if (_currentlySelectedMFI != null)
		{
			_currentlySelectedMFI.toggle.isOn = false;
		}
		ClearAllModFolderItems();
		string text = _modFolderPathField.text;
		if (string.IsNullOrEmpty(text) || !Directory.Exists(text))
		{
			return;
		}
		string[] directories = Directory.GetDirectories(text);
		if (directories != null && directories.Length != 0)
		{
			foreach (string p_fullFolderPath in directories)
			{
				ModFolderItem modFolderItem = CreateNewModFolderItem(p_fullFolderPath);
				modFolderItem.transform.SetParent(_modFoldersPanel.content);
				_currentActiveModFolders.Add(modFolderItem);
			}
		}
	}

	private void OnToggleMFI(ModFolderItem p_mfi, bool p_state)
	{
		if (p_state)
		{
			_currentlySelectedMFI = p_mfi;
		}
		else if (_currentlySelectedMFI == p_mfi)
		{
			_currentlySelectedMFI = null;
		}
		OnSetCurrentMFI();
	}

	private void OnSetCurrentMFI()
	{
		UpdatePreviewSection();
		UpdateButtons();
	}

	private ModFolderItem CreateNewModFolderItem(string p_fullFolderPath)
	{
		ModFolderItem modFolderItem = null;
		modFolderItem = ((_pooledItems.Count <= 0) ? UnityEngine.Object.Instantiate(_modFolderItemPrefab, _poolParent) : _pooledItems.Dequeue());
		modFolderItem.Initialize(_modFoldersToggleGroup);
		modFolderItem.SetFullFilePath(p_fullFolderPath);
		modFolderItem.SetToggleAction(OnToggleMFI);
		return modFolderItem;
	}

	private void ClearAllModFolderItems()
	{
		for (int i = 0; i < _currentActiveModFolders.Count; i++)
		{
			DestroyModFolderItem(_currentActiveModFolders[i]);
		}
		_currentActiveModFolders.Clear();
	}

	private void DestroyModFolderItem(ModFolderItem p_item)
	{
		p_item.transform.SetParent(_poolParent);
		p_item.transform.localPosition = Vector3.zero;
		p_item.ResetItem();
		_pooledItems.Enqueue(p_item);
	}

	private void UpdatePreviewSection()
	{
		DestroyPreviewImage();
		if (_currentlySelectedMFI != null)
		{
			_currentPreviewImagePath = Path.Combine(_currentlySelectedMFI.fullFolderPath, "PreviewImage.png");
			if (File.Exists(_currentPreviewImagePath))
			{
				_modPreviewImage.texture = TextureManager.Instance.LoadTexture2DFromFile(_currentPreviewImagePath);
				_modPreviewImage.gameObject.SetActive(value: true);
			}
			else
			{
				_currentPreviewImagePath = string.Empty;
			}
			_modNameField.text = _currentlySelectedMFI.directoryName;
		}
		else
		{
			_modNameField.text = string.Empty;
			_modPreviewImage.gameObject.SetActive(value: false);
		}
	}

	private void DestroyPreviewImage()
	{
		if (_modPreviewImage.texture != null)
		{
			UnityEngine.Object.Destroy(_modPreviewImage.texture);
			_modPreviewImage.texture = null;
		}
	}

	private void UpdateButtons()
	{
		bool flag = _currentlySelectedMFI != null;
		_uploadButton.interactable = flag && SteamManager.Initialized;
		_subscribeButton.interactable = flag;
	}

	public void UploadMod()
	{
		_workshopItem.Reset();
		_workshopItem.Name = _modNameField.text;
		_workshopItem.Description = _modDescriptionField.text;
		_workshopItem.IconPath = _currentPreviewImagePath;
		_workshopItem.ContentPath = _currentlySelectedMFI.fullFolderPath;
		string path = Path.Combine(_currentlySelectedMFI.fullFolderPath, "WorkshopItemInfo.xml");
		if (File.Exists(path))
		{
			using FileStream stream = new FileStream(path, FileMode.Open);
			WorkshopItemInfo workshopItemInfo = new XmlSerializer(typeof(WorkshopItemInfo)).Deserialize(stream) as WorkshopItemInfo;
			_workshopItem.SteamNative.m_nPublishedFileId = new PublishedFileId_t(workshopItemInfo.PublishedFileId);
			SteamUGC.GetItemState(_workshopItem.SteamNative.m_nPublishedFileId);
			Debug.Log("SDSF");
		}
		if (_tagDropdown.value > 0)
		{
			string text = _tagDropdown.options[_tagDropdown.value].text;
			_workshopItem.Tags.Add(text);
		}
		if (string.IsNullOrEmpty(_workshopItem.Name) || string.IsNullOrEmpty(_workshopItem.Description) || string.IsNullOrEmpty(_workshopItem.IconPath))
		{
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("OtherUI_Table", "Upload_Failed");
			string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("OtherUI_Table", "Upload_Missing_Field_Error");
			MainMenuUI.Instance.generalConfirmation.ShowGeneralConfirmation(localizedValue, localizedValue2);
		}
		else
		{
			_isUploading = true;
			StartCoroutine(ShowUploadProgress());
			SteamWorkshopMain.Instance.Upload(_workshopItem, null);
		}
	}

	public void SubscribeLocalMod()
	{
		if (!(_currentlySelectedMFI != null))
		{
			return;
		}
		InstalledModData installedModData = ExternalFileManager.Instance.GetInstalledModData(_currentlySelectedMFI.fullFolderPath);
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
		string empty = string.Empty;
		if (installedModData != null)
		{
			installedModData.itemInfo.Name = _modNameField.text;
			installedModData.itemInfo.Description = _modDescriptionField.text;
			installedModData.itemInfo.IconFileName = Path.GetFileName(_currentPreviewImagePath);
			installedModData.itemInfo.AuthorName = "Unknown";
			installedModData.itemInfo.OwnerLocalPath = _currentlySelectedMFI.fullFolderPath;
			if (_tagDropdown.value > 0)
			{
				string text = _tagDropdown.options[_tagDropdown.value].text;
				if (installedModData.itemInfo.Tags != null && installedModData.itemInfo.Tags.Length != 0)
				{
					installedModData.itemInfo.Tags[0] = text;
				}
				else
				{
					installedModData.itemInfo.Tags = new string[1] { text };
				}
			}
			ExternalFileManager.Instance.SaveToWorkshopItemInfo(installedModData);
			dictionary.Add("modName", installedModData.itemInfo.Name);
			empty = "Success_Local_Mod_Update";
		}
		else
		{
			installedModData = new InstalledModData();
			installedModData.localID = Guid.NewGuid().ToString();
			installedModData.fullFolderPath = _currentlySelectedMFI.fullFolderPath;
			installedModData.applyMod = false;
			installedModData.itemInfo = new WorkshopItemInfo();
			installedModData.itemInfo.Name = _modNameField.text;
			installedModData.itemInfo.Description = _modDescriptionField.text;
			installedModData.itemInfo.IconFileName = Path.GetFileName(_currentPreviewImagePath);
			installedModData.itemInfo.AuthorName = "Unknown";
			installedModData.itemInfo.OwnerLocalPath = _currentlySelectedMFI.fullFolderPath;
			if (_tagDropdown.value > 0)
			{
				string text2 = _tagDropdown.options[_tagDropdown.value].text;
				installedModData.itemInfo.Tags = new string[1] { text2 };
			}
			ExternalFileManager.Instance.OnLocalModSubscribed(installedModData);
			dictionary.Add("modName", installedModData.itemInfo.Name);
			empty = "Success_Local_Mod_Subscription";
		}
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("OtherUI_Table", "Mod_Subscription");
		string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("OtherUI_Table", empty, dictionary);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
		MainMenuUI.Instance.generalConfirmation.ShowGeneralConfirmation(localizedValue, localizedValue2);
	}

	private IEnumerator ShowUploadProgress()
	{
		while (_workshopItem != null && _isUploading)
		{
			yield return null;
			float uploadProgress = SteamWorkshopMain.Instance.GetUploadProgress(_workshopItem);
			ShowUploadProgressUI(uploadProgress);
			yield return GameUtilities.waitForHalfSecond;
		}
		HideUploadProgressUI();
	}

	private void ShowUploadProgressUI(float p_progress)
	{
		_uploadProgressText.text = $"{_localizedUploadingText} {(int)(p_progress * 100f)}%";
		_uploadProgressGO.SetActive(value: true);
	}

	private void HideUploadProgressUI()
	{
		_uploadProgressText.text = _localizedUploadingText + " 0%";
		_uploadProgressGO.SetActive(value: false);
	}

	private void OnUploadDone(WorkshopItemUpdateEventArgs p_args)
	{
		_isUploading = false;
		HideUploadProgressUI();
		if (!p_args.IsError && p_args.Item != null)
		{
			Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
			dictionary.Add("modName", p_args.Item.Name);
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("OtherUI_Table", "Success_Upload_Mod", dictionary);
			MaccimaDictionaryPool<string, string>.Release(dictionary);
			string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("OtherUI_Table", "Upload_Success");
			MainMenuUI.Instance.generalConfirmation.ShowGeneralConfirmation(localizedValue2, localizedValue);
		}
	}

	private void OnErrorUpload(LapinerTools.Steam.Data.ErrorEventArgs p_errorArgs)
	{
		_isUploading = false;
		HideUploadProgressUI();
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("OtherUI_Table", "Steam_Error");
		MainMenuUI.Instance.generalConfirmation.ShowGeneralConfirmation(localizedValue, p_errorArgs.ErrorMessage);
	}
}

using System.Collections;
using System.Collections.Generic;
using LapinerTools.Steam;
using LapinerTools.Steam.Data;
using Ruinarch.Custom_UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class SubscribedModsUI : MonoBehaviour
{
	[SerializeField]
	private GameObject _windowGO;

	[Header("Folder Section")]
	[SerializeField]
	private SubscribedModItem _smiPrefab;

	[SerializeField]
	private ScrollRect _smiScrollPanel;

	[SerializeField]
	private ToggleGroup _modsToggleGroup;

	[Header("Preview Section")]
	[SerializeField]
	private RawImage _modPreviewImage;

	[SerializeField]
	private TMP_InputField _modNameField;

	[SerializeField]
	private TMP_InputField _modAuthorField;

	[SerializeField]
	private TMP_InputField _modDescriptionField;

	[SerializeField]
	private RuinarchButton _unsubscribeButton;

	[SerializeField]
	private RuinarchButton _updateButton;

	[SerializeField]
	private GameObject _downloadProgressGO;

	[SerializeField]
	private RuinarchText _downloadProgressText;

	[Header("Pooling")]
	[SerializeField]
	private Transform _poolParent;

	private Queue<SubscribedModItem> _pooledItems = new Queue<SubscribedModItem>(10);

	private SubscribedModItem _currentlySelectedSMI;

	private List<SubscribedModItem> _currentActiveSMIs = new List<SubscribedModItem>(10);

	private bool _isDownloading;

	private string _localizedDownloadingText => LocalizationManager.Instance.GetLocalizedValue("OtherUI_Table", "Downloading");

	public void OnOpenParentModUI()
	{
		if (_windowGO.activeSelf)
		{
			UpdateAllExistingSMIs();
		}
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
		LoadSMIs();
		ExternalFileManager.Instance.OnModAddedToCollection += AddNewSMI;
		_windowGO.SetActive(value: true);
		SetPreviewData(null);
	}

	private void Hide()
	{
		if (_currentlySelectedSMI != null)
		{
			_currentlySelectedSMI.ForceToggleSMI(p_state: false);
		}
		ExternalFileManager.Instance.OnModAddedToCollection -= AddNewSMI;
		_windowGO.SetActive(value: false);
	}

	private void LoadSMIs()
	{
		ClearAllSMIs();
		List<InstalledModData> installedModCollection = ExternalFileManager.Instance.GetInstalledModCollection();
		for (int i = 0; i < installedModCollection.Count; i++)
		{
			InstalledModData p_modData = installedModCollection[i];
			AddNewSMI(p_modData);
		}
	}

	private void AddNewSMI(InstalledModData p_modData)
	{
		SubscribedModItem subscribedModItem = CreateNewSMI(p_modData);
		subscribedModItem.SetOnIncreasedPriorityAction(OnIncreasedSMIPriority);
		subscribedModItem.SetOnDecreasedPriorityAction(OnDecreasedSMIPriority);
		subscribedModItem.transform.SetParent(_smiScrollPanel.content);
		_currentActiveSMIs.Add(subscribedModItem);
	}

	private void RemoveSMI(SubscribedModItem p_smi)
	{
		if (_currentActiveSMIs.Remove(p_smi))
		{
			DestroySMI(p_smi);
		}
	}

	private void OnIncreasedSMIPriority(SubscribedModItem p_smi)
	{
		int siblingIndex = p_smi.transform.GetSiblingIndex();
		OnIncreasedSMIPriority(p_smi, siblingIndex);
	}

	private void OnIncreasedSMIPriority(SubscribedModItem p_smi, int p_index)
	{
		if (p_index > 0)
		{
			int num = p_index - 1;
			p_smi.transform.SetSiblingIndex(num);
			_currentActiveSMIs.RemoveAt(p_index);
			_currentActiveSMIs.Insert(num, p_smi);
			ExternalFileManager.Instance.IncreasePrioritySaveModInfo(p_smi.modData);
		}
	}

	private void OnDecreasedSMIPriority(SubscribedModItem p_smi)
	{
		int siblingIndex = p_smi.transform.GetSiblingIndex();
		if (siblingIndex < _currentActiveSMIs.Count - 1)
		{
			int num = siblingIndex + 1;
			p_smi = _currentActiveSMIs[num];
			OnIncreasedSMIPriority(p_smi, num);
		}
	}

	private void UpdateAllExistingSMIs()
	{
		for (int i = 0; i < _currentActiveSMIs.Count; i++)
		{
			_currentActiveSMIs[i].UpdateSMI();
		}
	}

	private SubscribedModItem CreateNewSMI(InstalledModData p_modData)
	{
		SubscribedModItem subscribedModItem = null;
		if (_pooledItems.Count > 0)
		{
			subscribedModItem = _pooledItems.Dequeue();
		}
		else
		{
			subscribedModItem = Object.Instantiate(_smiPrefab, _poolParent);
			subscribedModItem.SetToggleGroup(_modsToggleGroup);
			subscribedModItem.SetOnToggleSMI(OnToggleSMI);
		}
		subscribedModItem.Initialize(p_modData);
		return subscribedModItem;
	}

	private void OnToggleSMI(SubscribedModItem p_smi, bool p_state)
	{
		if (p_state)
		{
			if (_currentlySelectedSMI != p_smi)
			{
				_currentlySelectedSMI = p_smi;
				SetPreviewData(_currentlySelectedSMI);
			}
		}
		else if (_currentlySelectedSMI == p_smi)
		{
			_currentlySelectedSMI = null;
			SetPreviewData(null);
		}
	}

	private void ClearAllSMIs()
	{
		for (int i = 0; i < _currentActiveSMIs.Count; i++)
		{
			DestroySMI(_currentActiveSMIs[i]);
		}
		_currentActiveSMIs.Clear();
	}

	private void DestroySMI(SubscribedModItem p_item)
	{
		p_item.transform.SetParent(_poolParent);
		p_item.transform.localPosition = Vector3.zero;
		p_item.ResetItem();
		OnToggleSMI(_currentlySelectedSMI, p_state: false);
		_pooledItems.Enqueue(p_item);
	}

	private void SetPreviewData(SubscribedModItem p_smi)
	{
		InstalledModData installedModData = null;
		if (p_smi != null)
		{
			installedModData = p_smi.modData;
		}
		DestroyPreviewImage();
		if (installedModData != null)
		{
			_modNameField.text = installedModData.itemInfo.Name;
			_modAuthorField.text = installedModData.itemInfo.AuthorName;
			_modDescriptionField.text = installedModData.itemInfo.Description;
			_modPreviewImage.texture = TextureManager.Instance.LoadTexture2DFromFile(installedModData.fullFolderPath, installedModData.itemInfo.IconFileName);
			_modPreviewImage.gameObject.SetActive(value: true);
			_unsubscribeButton.interactable = true;
			_updateButton.interactable = true;
		}
		else
		{
			_modNameField.text = string.Empty;
			_modAuthorField.text = string.Empty;
			_modDescriptionField.text = string.Empty;
			_modPreviewImage.gameObject.SetActive(value: false);
			_unsubscribeButton.interactable = false;
			_updateButton.interactable = false;
		}
	}

	private void DestroyPreviewImage()
	{
		if (_modPreviewImage.texture != null)
		{
			Object.Destroy(_modPreviewImage.texture);
			_modPreviewImage.texture = null;
		}
	}

	public void ApplyChanges()
	{
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("OtherUI_Table", "Mod_Application");
		string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("OtherUI_Table", "Apply_Mods_Question");
		MainMenuUI.Instance.yesNoConfirmation.ShowYesNoConfirmation(localizedValue, localizedValue2, OnYesApplyChanges, OnNoApplyChanges, showCover: true, 21, "Yes", "No", yesBtnInteractable: true, noBtnInteractable: true, yesBtnActive: true, noBtnActive: true, null, null, OnNoApplyChanges);
	}

	private void OnYesApplyChanges()
	{
		for (int i = 0; i < _currentActiveSMIs.Count; i++)
		{
			SubscribedModItem subscribedModItem = _currentActiveSMIs[i];
			subscribedModItem.modData.applyMod = subscribedModItem.ShouldInstall();
		}
		ExternalFileManager.Instance.SaveAllExistingModInfos();
		Application.Quit();
	}

	private void OnNoApplyChanges()
	{
		MainMenuUI.Instance.yesNoConfirmation.Close();
	}

	public void Unsubscribe()
	{
		if (!(_currentlySelectedSMI != null))
		{
			return;
		}
		InstalledModData modData = _currentlySelectedSMI.modData;
		if (modData != null)
		{
			if (modData.isLocal)
			{
				ExternalFileManager.Instance.OnLocalModUnsubscribed(modData);
			}
			else
			{
				ExternalFileManager.Instance.OnModUnsubscribed(modData.publishedField.m_PublishedFileId);
				SteamWorkshopMain.Instance.Unsubscribe(modData.publishedField, OnItemUpdated);
			}
		}
		RemoveSMI(_currentlySelectedSMI);
	}

	public void UpdateItem()
	{
		if (_currentlySelectedSMI != null && SteamWorkshopMain.Instance.UpdateItem(_currentlySelectedSMI.modData.publishedField))
		{
			StartDownload();
		}
	}

	private void OnItemUpdated(WorkshopItemEventArgs p_itemArgs)
	{
		SWBItem activeItem = SWBParentUI.Instance.GetActiveItem(p_itemArgs.Item.SteamNative.m_nPublishedFileId.m_PublishedFileId);
		if (activeItem != null)
		{
			activeItem.SetData(p_itemArgs.Item);
		}
	}

	private void StartDownload()
	{
		_isDownloading = true;
		StartCoroutine(ShowDownloadProgress());
	}

	private void FinishDownload()
	{
		StopDownload();
		_currentlySelectedSMI.modData.UpdateWorkshopItemInfo();
		_currentlySelectedSMI.UpdateSMI();
		SetPreviewData(_currentlySelectedSMI);
	}

	private void StopDownload()
	{
		_isDownloading = false;
	}

	private IEnumerator ShowDownloadProgress()
	{
		HideDownloadProgressUI();
		while (_currentlySelectedSMI != null && _isDownloading)
		{
			if (SteamManager.Initialized)
			{
				bool p_finishedDownloading = false;
				float downloadProgress = SteamWorkshopMain.Instance.GetDownloadProgress(_currentlySelectedSMI.modData.publishedField, ref p_finishedDownloading);
				ShowDownloadProgressUI(downloadProgress);
				if (p_finishedDownloading)
				{
					yield return GameUtilities.waitForHalfSecond;
					FinishDownload();
					break;
				}
				yield return GameUtilities.waitForHalfSecond;
				continue;
			}
			StopDownload();
			break;
		}
		HideDownloadProgressUI();
	}

	private void ShowDownloadProgressUI(float p_progress)
	{
		_downloadProgressText.text = $"{_localizedDownloadingText} {(int)(p_progress * 100f)}%";
		_downloadProgressGO.SetActive(value: true);
	}

	private void HideDownloadProgressUI()
	{
		_downloadProgressText.text = _localizedDownloadingText + " 0%";
		_downloadProgressGO.SetActive(value: false);
	}
}

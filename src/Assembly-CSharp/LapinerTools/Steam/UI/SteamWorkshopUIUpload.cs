using System;
using System.Collections;
using System.IO;
using LapinerTools.Steam.Data;
using LapinerTools.uMyGUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LapinerTools.Steam.UI;

public class SteamWorkshopUIUpload : MonoBehaviour
{
	protected static SteamWorkshopUIUpload s_instance;

	[SerializeField]
	protected int ICON_WIDTH = 512;

	[SerializeField]
	protected int ICON_HEIGHT = 512;

	[SerializeField]
	protected InputField NAME_INPUT;

	[SerializeField]
	protected InputField DESCRIPTION_INPUT;

	[SerializeField]
	protected RawImage ICON;

	[SerializeField]
	protected Button SCREENSHOT_BUTTON;

	[SerializeField]
	protected Button UPLOAD_BUTTON;

	[SerializeField]
	protected bool m_improveNavigationFocus = true;

	protected bool m_isUploading;

	protected WWW m_pendingImageDownload;

	protected WorkshopItemUpdate m_itemData = new WorkshopItemUpdate();

	public static SteamWorkshopUIUpload Instance
	{
		get
		{
			if (s_instance == null)
			{
				s_instance = UnityEngine.Object.FindObjectOfType<SteamWorkshopUIUpload>();
			}
			return s_instance;
		}
	}

	public event Action<string> OnNameSet;

	public event Action<string> OnDescriptionSet;

	public event Action<string> OnIconFilePathSet;

	public event Action<Texture2D> OnIconTextureSet;

	public event Action<WorkshopItemUpdateEventArgs> OnStartedUpload;

	public event Action<WorkshopItemUpdateEventArgs> OnFinishedUpload;

	public virtual void SetItemData(WorkshopItemUpdate p_itemData)
	{
		m_itemData = ((p_itemData != null) ? p_itemData : new WorkshopItemUpdate());
		if (m_itemData.Name == null)
		{
			m_itemData.Name = "";
		}
		if (m_itemData.Description == null)
		{
			m_itemData.Description = "";
		}
		if (NAME_INPUT != null)
		{
			NAME_INPUT.text = m_itemData.Name;
		}
		else
		{
			Debug.LogError("SteamWorkshopUIUpload: SetItemData: NAME_INPUT is not set in inspector!");
		}
		if (DESCRIPTION_INPUT != null)
		{
			if (!string.IsNullOrEmpty(m_itemData.Description))
			{
				StartCoroutine(SetDescriptionSafe(m_itemData.Description));
			}
			else
			{
				DESCRIPTION_INPUT.text = "";
			}
		}
		else
		{
			Debug.LogError("SteamWorkshopUIUpload: SetItemData: DESCRIPTION_INPUT is not set in inspector!");
		}
		if (ICON != null)
		{
			if (!string.IsNullOrEmpty(m_itemData.IconPath))
			{
				StartCoroutine(LoadIcon(m_itemData.IconPath));
			}
			else
			{
				ICON.texture = null;
			}
		}
		else
		{
			Debug.LogError("SteamWorkshopUIUpload: SetItemData: ICON is not set in inspector!");
		}
	}

	protected virtual void Start()
	{
		SteamWorkshopMain.Instance.OnUploaded += ShowSuccessMessage;
		SteamWorkshopMain.Instance.OnError += ShowErrorMessage;
		if (NAME_INPUT != null)
		{
			NAME_INPUT.onEndEdit.AddListener(OnEditName);
		}
		else
		{
			Debug.LogError("SteamWorkshopUIUpload: NAME_INPUT is not set in inspector!");
		}
		if (DESCRIPTION_INPUT != null)
		{
			DESCRIPTION_INPUT.onEndEdit.AddListener(OnEditDescription);
		}
		else
		{
			Debug.LogError("SteamWorkshopUIUpload: DESCRIPTION_INPUT is not set in inspector!");
		}
		if (SCREENSHOT_BUTTON != null)
		{
			SCREENSHOT_BUTTON.onClick.AddListener(OnScreenshotButtonClick);
		}
		else
		{
			Debug.LogError("SteamWorkshopUIUpload: SCREENSHOT_BUTTON is not set in inspector!");
		}
		if (UPLOAD_BUTTON != null)
		{
			UPLOAD_BUTTON.onClick.AddListener(OnUploadButtonClick);
		}
		else
		{
			Debug.LogError("SteamWorkshopUIUpload: UPLOAD_BUTTON is not set in inspector!");
		}
	}

	protected virtual void LateUpdate()
	{
		if (!m_improveNavigationFocus)
		{
			return;
		}
		EventSystem current = EventSystem.current;
		if (current != null && (current.currentSelectedGameObject == null || !current.currentSelectedGameObject.activeInHierarchy))
		{
			if (current.lastSelectedGameObject != null && current.lastSelectedGameObject.activeInHierarchy)
			{
				current.SetSelectedGameObject(current.lastSelectedGameObject);
			}
			else if (NAME_INPUT != null)
			{
				NAME_INPUT.Select();
			}
		}
	}

	protected virtual void OnDestroy()
	{
		if (ICON != null)
		{
			UnityEngine.Object.Destroy(ICON.texture);
		}
		if (m_pendingImageDownload != null)
		{
			m_pendingImageDownload.Dispose();
			m_pendingImageDownload = null;
		}
		if (SteamWorkshopMain.IsInstanceSet)
		{
			SteamWorkshopMain.Instance.OnUploaded -= ShowSuccessMessage;
			SteamWorkshopMain.Instance.OnError -= ShowErrorMessage;
		}
	}

	protected virtual void OnEditName(string p_name)
	{
		m_itemData.Name = p_name;
		InvokeEventHandlerSafely(this.OnNameSet, p_name);
	}

	protected virtual void OnEditDescription(string p_description)
	{
		m_itemData.Description = p_description;
		InvokeEventHandlerSafely(this.OnDescriptionSet, p_description);
	}

	protected virtual void OnScreenshotButtonClick()
	{
		if (string.IsNullOrEmpty(m_itemData.ContentPath))
		{
			m_itemData.ContentPath = Path.Combine(Application.persistentDataPath, m_itemData.Name);
		}
		string iconFilePath = Path.Combine(m_itemData.ContentPath, m_itemData.Name + ".png");
		SteamWorkshopMain.Instance.RenderIcon(Camera.main, ICON_WIDTH, ICON_HEIGHT, iconFilePath, delegate(Texture2D p_renderedIcon)
		{
			m_itemData.IconPath = iconFilePath;
			InvokeEventHandlerSafely(this.OnIconFilePathSet, iconFilePath);
			if (ICON != null)
			{
				ICON.texture = p_renderedIcon;
				InvokeEventHandlerSafely(this.OnIconTextureSet, p_renderedIcon);
			}
			else
			{
				Debug.LogError("SteamWorkshopUIUpload: OnScreenshotButtonClick: ICON is not set in inspector!");
			}
		});
	}

	protected virtual void OnUploadButtonClick()
	{
		if (string.IsNullOrEmpty(m_itemData.Name))
		{
			((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup("text")).SetText("Invalid Item Name", "Please give your item a non-empty name!").ShowButton("ok");
			return;
		}
		if (string.IsNullOrEmpty(m_itemData.Description))
		{
			((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup("text")).SetText("Invalid Item Name", "Please give your item a non-empty description!").ShowButton("ok");
			return;
		}
		if (string.IsNullOrEmpty(m_itemData.IconPath))
		{
			((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup("text")).SetText("Invalid Item Icon", "Please provide an icon image for your item!").ShowButton("ok");
			return;
		}
		m_isUploading = true;
		StartCoroutine(ShowUploadProgress());
		SteamWorkshopMain.Instance.Upload(m_itemData, null);
		if (this.OnStartedUpload != null)
		{
			this.OnStartedUpload(new WorkshopItemUpdateEventArgs
			{
				Item = m_itemData
			});
		}
	}

	protected virtual void ShowSuccessMessage(WorkshopItemUpdateEventArgs p_successArgs)
	{
		m_isUploading = false;
		if (!p_successArgs.IsError && p_successArgs.Item != null)
		{
			((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup("text")).SetText("Item Uploaded", "Item '" + p_successArgs.Item.Name + "' was successfully uploaded!").ShowButton("ok");
		}
		if (this.OnFinishedUpload != null)
		{
			this.OnFinishedUpload(p_successArgs);
		}
	}

	protected virtual void ShowErrorMessage(LapinerTools.Steam.Data.ErrorEventArgs p_errorArgs)
	{
		m_isUploading = false;
		((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup("text")).SetText("Steam Error", p_errorArgs.ErrorMessage).ShowButton("ok");
	}

	protected virtual void InvokeEventHandlerSafely<T>(Action<T> p_handler, T p_data)
	{
		try
		{
			p_handler?.Invoke(p_data);
		}
		catch (Exception ex)
		{
			Debug.LogError("SteamWorkshopUIUpload: your event handler (" + p_handler.Target?.ToString() + " - System.Action<" + typeof(T)?.ToString() + ">) has thrown an excepotion!\n" + ex);
		}
	}

	protected virtual IEnumerator ShowUploadProgress()
	{
		while (m_itemData != null && m_isUploading)
		{
			float uploadProgress = SteamWorkshopMain.Instance.GetUploadProgress(m_itemData);
			((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup("text")).SetText("Uploading Item", "<size=32>" + (int)(uploadProgress * 100f) + "%</size>");
			yield return new WaitForSeconds(0.4f);
		}
	}

	protected virtual IEnumerator SetDescriptionSafe(string p_description)
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		if (DESCRIPTION_INPUT != null)
		{
			DESCRIPTION_INPUT.text = p_description;
		}
	}

	protected virtual IEnumerator LoadIcon(string p_filePath)
	{
		if (string.IsNullOrEmpty(p_filePath))
		{
			yield break;
		}
		m_pendingImageDownload = new WWW("file:///" + p_filePath);
		yield return m_pendingImageDownload;
		if (m_pendingImageDownload == null)
		{
			yield break;
		}
		if (m_pendingImageDownload.isDone && string.IsNullOrEmpty(m_pendingImageDownload.error))
		{
			if (ICON != null)
			{
				ICON.texture = m_pendingImageDownload.texture;
			}
		}
		else
		{
			Debug.LogError("SteamWorkshopUIUpload: LoadIcon: could not load icon at '" + p_filePath + "'\n" + m_pendingImageDownload.error);
		}
		m_pendingImageDownload = null;
	}
}

using System;
using System.Collections;
using System.Text.RegularExpressions;
using LapinerTools.Steam;
using LapinerTools.Steam.Data;
using Ruinarch.Custom_UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UtilityScripts;

public class SWBItem : MonoBehaviour, IScrollHandler, IEventSystemHandler
{
	public class ItemDataSetEventArgs : EventArgsBase
	{
		public WorkshopItem ItemData { get; set; }

		public SWBItem ItemUI { get; set; }
	}

	public class SendMessageInitData
	{
		public WorkshopItem Item { get; set; }
	}

	[SerializeField]
	protected RuinarchText m_textName;

	[SerializeField]
	protected RuinarchText m_textDescription;

	[SerializeField]
	protected RuinarchText m_textVotesUp;

	[SerializeField]
	protected RuinarchText m_textVotesDown;

	[SerializeField]
	protected RuinarchButton m_btnVotesUp;

	[SerializeField]
	protected RuinarchButton m_btnVotesUpActive;

	[SerializeField]
	protected RuinarchButton m_btnVotesDown;

	[SerializeField]
	protected RuinarchButton m_btnVotesDownActive;

	[SerializeField]
	protected RuinarchText m_textSubscriptions;

	[SerializeField]
	protected RuinarchText m_textDownloadProgress;

	[SerializeField]
	protected RuinarchButton m_btnSubscriptions;

	[SerializeField]
	protected RuinarchButton m_btnSubscriptionsActive;

	[SerializeField]
	protected RawImage m_image;

	[SerializeField]
	protected bool m_useExplicitNavigation;

	[SerializeField]
	protected bool m_improveNavigationFocus;

	[SerializeField]
	protected GameObject _subscriptionSection;

	protected WorkshopItem m_data;

	protected ScrollRect m_parentScroller;

	protected WWW m_pendingImageDownload;

	public RawImage Image => m_image;

	public void SetData(WorkshopItem p_data)
	{
		if (SteamWorkshopMain.IsInstanceSet)
		{
			SteamWorkshopMain.Instance.OnInstalled -= OnItemInstalled;
			SteamWorkshopMain.Instance.OnInstalled += OnItemInstalled;
		}
		m_data = p_data;
		if (m_image != null && m_image.texture == null && m_pendingImageDownload == null)
		{
			StartCoroutine(DownloadPreview(m_data.PreviewImageURL));
		}
		if (m_textName != null)
		{
			m_textName.text = m_data.Name;
		}
		if (m_textDescription != null)
		{
			m_textDescription.text = Regex.Replace(m_data.Description, "[\\r\\n]+", " ");
		}
		if (m_textVotesUp != null)
		{
			m_textVotesUp.text = m_data.VotesUp.ToString();
		}
		if (m_textVotesDown != null)
		{
			m_textVotesDown.text = m_data.VotesDown.ToString();
		}
		if (m_textSubscriptions != null)
		{
			m_textSubscriptions.text = m_data.Subscriptions.ToString();
		}
		if (m_btnSubscriptions != null && m_btnSubscriptionsActive != null)
		{
			m_btnSubscriptions.gameObject.SetActive(!m_data.IsSubscribed);
			m_btnSubscriptionsActive.gameObject.SetActive(m_data.IsSubscribed);
		}
		if (m_btnVotesUp != null && m_btnVotesUpActive != null)
		{
			m_btnVotesUp.gameObject.SetActive(!m_data.IsVotedUp);
			m_btnVotesUpActive.gameObject.SetActive(m_data.IsVotedUp);
		}
		if (m_btnVotesDown != null && m_btnVotesDownActive != null)
		{
			m_btnVotesDown.gameObject.SetActive(!m_data.IsVotedDown);
			m_btnVotesDownActive.gameObject.SetActive(m_data.IsVotedDown);
		}
		if (m_useExplicitNavigation)
		{
			SetNavigationTargetsHorizontal(new Selectable[6] { m_btnVotesUp, m_btnVotesUpActive, m_btnVotesDown, m_btnVotesDownActive, m_btnSubscriptions, m_btnSubscriptionsActive });
			StartCoroutine(SetNavigationTargetsVertical());
		}
		if (m_textDownloadProgress != null)
		{
			m_textDownloadProgress.gameObject.SetActive(m_data.IsDownloading);
		}
		if (m_data.IsDownloading)
		{
			StartCoroutine(ShowDownloadProgress());
			_subscriptionSection.gameObject.SetActive(value: false);
		}
		else
		{
			_subscriptionSection.gameObject.SetActive(value: true);
		}
		SWBParentUI.Instance.InvokeOnItemDataSet(m_data, this);
	}

	public virtual void OnScroll(PointerEventData data)
	{
		if (m_parentScroller == null)
		{
			m_parentScroller = GetComponentInParent<ScrollRect>();
		}
		if (!(m_parentScroller == null))
		{
			m_parentScroller.OnScroll(data);
		}
	}

	public virtual void Select()
	{
	}

	protected virtual void Start()
	{
		if (m_btnSubscriptions != null && m_btnSubscriptionsActive != null)
		{
			m_btnSubscriptions.onClick.AddListener(Subscribe);
			m_btnSubscriptionsActive.onClick.AddListener(Unsubscribe);
		}
		if (m_btnVotesUp != null && m_btnVotesUpActive != null)
		{
			m_btnVotesUp.onClick.AddListener(VoteUp);
		}
		if (m_btnVotesDown != null && m_btnVotesDownActive != null)
		{
			m_btnVotesDown.onClick.AddListener(VoteDown);
		}
	}

	protected void OnDestroy()
	{
		ResetItem();
	}

	public void ResetItem()
	{
		if (m_image != null)
		{
			UnityEngine.Object.Destroy(m_image.texture);
			m_image.texture = null;
		}
		if (m_pendingImageDownload != null)
		{
			m_pendingImageDownload.Dispose();
			m_pendingImageDownload = null;
		}
		if (SteamWorkshopMain.IsInstanceSet)
		{
			SteamWorkshopMain.Instance.OnInstalled -= OnItemInstalled;
		}
	}

	protected virtual void Subscribe()
	{
		if (m_data != null)
		{
			SteamWorkshopMain.Instance.Subscribe(m_data, OnItemUpdated);
			SWBParentUI.Instance.InvokeOnSubscribeButtonClick(m_data);
		}
	}

	protected virtual void Unsubscribe()
	{
		if (m_data != null)
		{
			ExternalFileManager.Instance.OnModUnsubscribed(m_data.SteamNative.m_nPublishedFileId.m_PublishedFileId);
			SteamWorkshopMain.Instance.Unsubscribe(m_data, OnItemUpdated);
			SWBParentUI.Instance.InvokeOnUnsubscribeButtonClick(m_data);
		}
	}

	protected virtual void VoteUp()
	{
		if (m_data != null)
		{
			SteamWorkshopMain.Instance.Vote(m_data, p_isUpVote: true, OnItemUpdated);
			SWBParentUI.Instance.InvokeOnVoteUpButtonClick(m_data);
		}
	}

	protected virtual void VoteDown()
	{
		if (m_data != null)
		{
			SteamWorkshopMain.Instance.Vote(m_data, p_isUpVote: false, OnItemUpdated);
			SWBParentUI.Instance.InvokeOnVoteDownButtonClick(m_data);
		}
	}

	protected virtual void OnItemInstalled(WorkshopItemEventArgs p_itemArgs)
	{
		OnItemUpdated(p_itemArgs);
	}

	protected void OnItemUpdated(WorkshopItemEventArgs p_itemArgs)
	{
		if (m_data.SteamNative.m_nPublishedFileId == p_itemArgs.Item.SteamNative.m_nPublishedFileId)
		{
			SetData(p_itemArgs.Item);
		}
	}

	public bool ContainsPublishedFileID(ulong p_publishedFileId)
	{
		if (m_data != null && m_data.SteamNative != null)
		{
			return m_data.SteamNative.m_nPublishedFileId.m_PublishedFileId == p_publishedFileId;
		}
		return false;
	}

	protected virtual void SetNavigationTargetsHorizontal(Selectable[] p_horizontalNavOrder)
	{
		for (int i = 0; i < p_horizontalNavOrder.Length; i++)
		{
			Selectable selectable = p_horizontalNavOrder[i];
			if (!(selectable != null))
			{
				continue;
			}
			Navigation navigation = selectable.navigation;
			navigation.mode = Navigation.Mode.Explicit;
			for (int num = i - 1; num >= 0; num--)
			{
				Selectable selectable2 = p_horizontalNavOrder[num];
				if (selectable2 != null && selectable2.gameObject.activeSelf)
				{
					navigation.selectOnLeft = selectable2;
					break;
				}
			}
			for (int j = i + 1; j < p_horizontalNavOrder.Length; j++)
			{
				Selectable selectable3 = p_horizontalNavOrder[j];
				if (selectable3 != null && selectable3.gameObject.activeSelf)
				{
					navigation.selectOnRight = selectable3;
					break;
				}
			}
			selectable.navigation = navigation;
		}
	}

	protected virtual void SetNavigationTargetsVertical(Selectable p_current, Selectable[] p_verticalNavOrder)
	{
		if (p_current == null || !p_current.gameObject.activeSelf)
		{
			return;
		}
		for (int i = 0; i < p_verticalNavOrder.Length; i++)
		{
			Selectable selectable = p_verticalNavOrder[i];
			if (!(selectable != null) || i < 0)
			{
				continue;
			}
			Navigation navigation = selectable.navigation;
			navigation.mode = Navigation.Mode.Explicit;
			for (int num = i - 1; num >= 0; num--)
			{
				Selectable selectable2 = p_verticalNavOrder[num];
				if (selectable2 != null && selectable2.gameObject.activeSelf)
				{
					navigation.selectOnUp = selectable2;
					break;
				}
			}
			for (int j = i + 1; j < p_verticalNavOrder.Length; j++)
			{
				Selectable selectable3 = p_verticalNavOrder[j];
				if (selectable3 != null && selectable3.gameObject.activeSelf)
				{
					navigation.selectOnDown = selectable3;
					break;
				}
			}
			selectable.navigation = navigation;
		}
	}

	protected virtual IEnumerator SetNavigationTargetsVertical()
	{
		yield return new WaitForEndOfFrame();
		if (!(base.transform.parent != null))
		{
			yield break;
		}
		SWBItem[] componentsInChildren = base.transform.parent.GetComponentsInChildren<SWBItem>();
		int num = Array.IndexOf(componentsInChildren, this);
		if (num >= 0)
		{
			SWBItem sWBItem = componentsInChildren[num];
			SWBItem sWBItem2 = ((num > 0) ? componentsInChildren[num - 1] : null);
			SWBItem sWBItem3 = ((num < componentsInChildren.Length - 1) ? componentsInChildren[num + 1] : null);
			SetNavigationTargetsVertical(sWBItem.m_btnVotesUp, new Selectable[5]
			{
				sWBItem2 ? sWBItem2.m_btnVotesUp : null,
				sWBItem2 ? sWBItem2.m_btnVotesUpActive : null,
				sWBItem.m_btnVotesUp,
				sWBItem3 ? sWBItem3.m_btnVotesUp : null,
				sWBItem3 ? sWBItem3.m_btnVotesUpActive : null
			});
			SetNavigationTargetsVertical(sWBItem.m_btnVotesUpActive, new Selectable[5]
			{
				sWBItem2 ? sWBItem2.m_btnVotesUp : null,
				sWBItem2 ? sWBItem2.m_btnVotesUpActive : null,
				sWBItem.m_btnVotesUpActive,
				sWBItem3 ? sWBItem3.m_btnVotesUp : null,
				sWBItem3 ? sWBItem3.m_btnVotesUpActive : null
			});
			SetNavigationTargetsVertical(sWBItem.m_btnVotesDown, new Selectable[5]
			{
				sWBItem2 ? sWBItem2.m_btnVotesDown : null,
				sWBItem2 ? sWBItem2.m_btnVotesDownActive : null,
				sWBItem.m_btnVotesDown,
				sWBItem3 ? sWBItem3.m_btnVotesDown : null,
				sWBItem3 ? sWBItem3.m_btnVotesDownActive : null
			});
			SetNavigationTargetsVertical(sWBItem.m_btnVotesDownActive, new Selectable[5]
			{
				sWBItem2 ? sWBItem2.m_btnVotesDown : null,
				sWBItem2 ? sWBItem2.m_btnVotesDownActive : null,
				sWBItem.m_btnVotesDownActive,
				sWBItem3 ? sWBItem3.m_btnVotesDown : null,
				sWBItem3 ? sWBItem3.m_btnVotesDownActive : null
			});
			SetNavigationTargetsVertical(sWBItem.m_btnSubscriptions, new Selectable[5]
			{
				sWBItem2 ? sWBItem2.m_btnSubscriptions : null,
				sWBItem2 ? sWBItem2.m_btnSubscriptionsActive : null,
				sWBItem.m_btnSubscriptions,
				sWBItem3 ? sWBItem3.m_btnSubscriptions : null,
				sWBItem3 ? sWBItem3.m_btnSubscriptionsActive : null
			});
			SetNavigationTargetsVertical(sWBItem.m_btnSubscriptionsActive, new Selectable[5]
			{
				sWBItem2 ? sWBItem2.m_btnSubscriptions : null,
				sWBItem2 ? sWBItem2.m_btnSubscriptionsActive : null,
				sWBItem.m_btnSubscriptionsActive,
				sWBItem3 ? sWBItem3.m_btnSubscriptions : null,
				sWBItem3 ? sWBItem3.m_btnSubscriptionsActive : null
			});
			if (num == 0 || num == componentsInChildren.Length - 1)
			{
				yield return new WaitForEndOfFrame();
				SetAutomaticNavigation(m_btnVotesUp);
				SetAutomaticNavigation(m_btnVotesUpActive);
				SetAutomaticNavigation(m_btnVotesDown);
				SetAutomaticNavigation(m_btnVotesDownActive);
				SetAutomaticNavigation(m_btnSubscriptions);
				SetAutomaticNavigation(m_btnSubscriptionsActive);
			}
		}
	}

	protected virtual void SetAutomaticNavigation(Selectable p_selectable)
	{
		if (p_selectable != null)
		{
			Navigation navigation = p_selectable.navigation;
			navigation.mode = Navigation.Mode.Automatic;
			p_selectable.navigation = navigation;
		}
	}

	protected virtual IEnumerator ShowDownloadProgress()
	{
		while (m_data != null && m_data.IsDownloading)
		{
			if (m_textDownloadProgress != null)
			{
				m_textDownloadProgress.gameObject.SetActive(value: true);
				m_textDownloadProgress.text = string.Format("{0} {1}%", LocalizationManager.Instance.GetLocalizedValue("OtherUI_Table", "Downloading"), (int)(SteamWorkshopMain.Instance.GetDownloadProgress(m_data) * 100f));
			}
			yield return GameUtilities.waitForHalfSecond;
		}
	}

	protected virtual IEnumerator DownloadPreview(string p_URL)
	{
		if (string.IsNullOrEmpty(p_URL))
		{
			yield break;
		}
		m_pendingImageDownload = new WWW(p_URL);
		yield return m_pendingImageDownload;
		if (m_pendingImageDownload == null)
		{
			yield break;
		}
		if (m_pendingImageDownload.isDone && string.IsNullOrEmpty(m_pendingImageDownload.error))
		{
			if (m_image != null)
			{
				m_image.texture = m_pendingImageDownload.texture;
			}
		}
		else
		{
			Debug.LogError("SteamWorkshopItemNode: DownloadPreview: could not load preview image at '" + p_URL + "'\n" + m_pendingImageDownload.error);
		}
		m_pendingImageDownload = null;
	}
}

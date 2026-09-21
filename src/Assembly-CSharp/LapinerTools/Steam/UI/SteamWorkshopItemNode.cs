using System;
using System.Collections;
using LapinerTools.Steam.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LapinerTools.Steam.UI;

public class SteamWorkshopItemNode : MonoBehaviour, IScrollHandler, IEventSystemHandler
{
	public class ItemDataSetEventArgs : EventArgsBase
	{
		public WorkshopItem ItemData { get; set; }

		public SteamWorkshopItemNode ItemUI { get; set; }
	}

	public class SendMessageInitData
	{
		public WorkshopItem Item { get; set; }
	}

	[SerializeField]
	protected Text m_textName;

	[SerializeField]
	protected Text m_textDescription;

	[SerializeField]
	protected Text m_textVotes;

	[SerializeField]
	protected Button m_btnVotesUp;

	[SerializeField]
	protected Button m_btnVotesUpActive;

	[SerializeField]
	protected Button m_btnVotesDown;

	[SerializeField]
	protected Button m_btnVotesDownActive;

	[SerializeField]
	protected Text m_textFavorites;

	[SerializeField]
	protected Button m_btnFavorites;

	[SerializeField]
	protected Button m_btnFavoritesActive;

	[SerializeField]
	protected Text m_textSubscriptions;

	[SerializeField]
	protected Text m_textDownloadProgress;

	[SerializeField]
	protected Button m_btnSubscriptions;

	[SerializeField]
	protected Button m_btnSubscriptionsActive;

	[SerializeField]
	protected RawImage m_image;

	[SerializeField]
	protected Image m_selectionImage;

	[SerializeField]
	protected Button m_btnDownload;

	[SerializeField]
	protected Button m_btnPlay;

	[SerializeField]
	protected Button m_btnDelete;

	[SerializeField]
	protected bool m_useExplicitNavigation = true;

	[SerializeField]
	protected bool m_improveNavigationFocus = true;

	protected SendMessageInitData m_data;

	protected ScrollRect m_parentScroller;

	protected WWW m_pendingImageDownload;

	protected bool isDestroyed;

	public RawImage Image => m_image;

	public virtual void uMyGUI_TreeBrowser_InitNode(object p_data)
	{
		if (p_data is SendMessageInitData)
		{
			SteamWorkshopMain.Instance.OnInstalled -= OnItemInstalled;
			SteamWorkshopMain.Instance.OnInstalled += OnItemInstalled;
			m_data = (SendMessageInitData)p_data;
			if (m_image != null && m_image.texture == null && m_pendingImageDownload == null)
			{
				StartCoroutine(DownloadPreview(m_data.Item.PreviewImageURL));
			}
			if (m_textName != null)
			{
				m_textName.text = m_data.Item.Name;
			}
			if (m_textDescription != null)
			{
				m_textDescription.text = m_data.Item.Description;
			}
			if (m_textVotes != null)
			{
				m_textVotes.text = m_data.Item.VotesUp + " / " + m_data.Item.VotesDown;
			}
			if (m_textFavorites != null)
			{
				m_textFavorites.text = m_data.Item.Favorites.ToString();
			}
			if (m_textSubscriptions != null)
			{
				m_textSubscriptions.text = m_data.Item.Subscriptions.ToString();
			}
			if (m_btnFavorites != null && m_btnFavoritesActive != null)
			{
				m_btnFavorites.gameObject.SetActive(!m_data.Item.IsFavorited);
				m_btnFavoritesActive.gameObject.SetActive(m_data.Item.IsFavorited);
			}
			if (m_btnSubscriptions != null && m_btnSubscriptionsActive != null)
			{
				m_btnSubscriptions.gameObject.SetActive(!m_data.Item.IsSubscribed);
				m_btnSubscriptionsActive.gameObject.SetActive(m_data.Item.IsSubscribed);
			}
			if (m_btnVotesUp != null && m_btnVotesUpActive != null)
			{
				m_btnVotesUp.gameObject.SetActive(!m_data.Item.IsVotedUp);
				m_btnVotesUpActive.gameObject.SetActive(m_data.Item.IsVotedUp);
			}
			if (m_btnVotesDown != null && m_btnVotesDownActive != null)
			{
				m_btnVotesDown.gameObject.SetActive(!m_data.Item.IsVotedDown);
				m_btnVotesDownActive.gameObject.SetActive(m_data.Item.IsVotedDown);
			}
			if (m_btnDownload != null)
			{
				m_btnDownload.gameObject.SetActive(!m_data.Item.IsInstalled && !m_data.Item.IsDownloading);
			}
			if (m_btnPlay != null)
			{
				m_btnPlay.gameObject.SetActive(m_data.Item.IsInstalled && !m_data.Item.IsDownloading);
			}
			if (m_btnDelete != null)
			{
				m_btnDelete.gameObject.SetActive(m_data.Item.IsSubscribed);
			}
			if (m_useExplicitNavigation)
			{
				SetNavigationTargetsHorizontal(new Selectable[11]
				{
					m_btnDelete, m_btnVotesUp, m_btnVotesUpActive, m_btnVotesDown, m_btnVotesDownActive, m_btnFavorites, m_btnFavoritesActive, m_btnSubscriptions, m_btnSubscriptionsActive, m_btnPlay,
					m_btnDownload
				});
				StartCoroutine(SetNavigationTargetsVertical());
			}
			if (m_textDownloadProgress != null)
			{
				m_textDownloadProgress.gameObject.SetActive(m_data.Item.IsDownloading);
			}
			if (m_data.Item.IsDownloading)
			{
				StartCoroutine(ShowDownloadProgress());
			}
			SteamWorkshopUIBrowse.Instance.InvokeOnItemDataSet(m_data.Item, this);
		}
		else
		{
			Debug.LogError("SteamWorkshopItemNode: uMyGUI_TreeBrowser_InitNode: expected p_data to be a SteamWorkshopItemNode.SendMessageInitData! p_data: " + p_data);
		}
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
		if (m_btnDownload != null && m_btnDownload.gameObject.activeSelf)
		{
			m_btnDownload.Select();
		}
		else if (m_btnPlay != null && m_btnPlay.gameObject.activeSelf)
		{
			m_btnPlay.Select();
		}
	}

	protected virtual void Start()
	{
		if (m_btnFavorites != null && m_btnFavoritesActive != null)
		{
			m_btnFavorites.onClick.AddListener(AddFavorite);
			m_btnFavoritesActive.onClick.AddListener(RemovedFavorite);
		}
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
		if (m_btnDownload != null)
		{
			m_btnDownload.onClick.AddListener(Subscribe);
		}
		if (m_btnPlay != null)
		{
			m_btnPlay.onClick.AddListener(OnPlayBtn);
		}
		if (m_btnDelete != null)
		{
			m_btnDelete.onClick.AddListener(Unsubscribe);
		}
	}

	protected virtual void OnDestroy()
	{
		isDestroyed = true;
		if (m_image != null)
		{
			UnityEngine.Object.Destroy(m_image.texture);
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

	protected virtual void OnPlayBtn()
	{
		if (m_data != null)
		{
			SteamWorkshopUIBrowse.Instance.InvokeOnPlayButtonClick(m_data.Item);
		}
	}

	protected virtual void Subscribe()
	{
		if (m_data != null)
		{
			SteamWorkshopMain.Instance.Subscribe(m_data.Item, OnItemUpdated(m_btnSubscriptionsActive));
			SteamWorkshopUIBrowse.Instance.InvokeOnSubscribeButtonClick(m_data.Item);
		}
	}

	protected virtual void Unsubscribe()
	{
		if (m_data != null)
		{
			SteamWorkshopMain.Instance.Unsubscribe(m_data.Item, OnItemUpdated(m_btnSubscriptions));
			SteamWorkshopUIBrowse.Instance.InvokeOnUnsubscribeButtonClick(m_data.Item);
		}
	}

	protected virtual void AddFavorite()
	{
		if (m_data != null)
		{
			SteamWorkshopMain.Instance.AddFavorite(m_data.Item, OnItemUpdated(m_btnFavoritesActive));
			SteamWorkshopUIBrowse.Instance.InvokeOnAddFavoriteButtonClick(m_data.Item);
		}
	}

	protected virtual void RemovedFavorite()
	{
		if (m_data != null)
		{
			SteamWorkshopMain.Instance.RemoveFavorite(m_data.Item, OnItemUpdated(m_btnFavorites));
			SteamWorkshopUIBrowse.Instance.InvokeOnRemoveFavoriteButtonClick(m_data.Item);
		}
	}

	protected virtual void VoteUp()
	{
		if (m_data != null)
		{
			SteamWorkshopMain.Instance.Vote(m_data.Item, p_isUpVote: true, OnItemUpdated(m_btnVotesUpActive));
			SteamWorkshopUIBrowse.Instance.InvokeOnVoteUpButtonClick(m_data.Item);
		}
	}

	protected virtual void VoteDown()
	{
		if (m_data != null)
		{
			SteamWorkshopMain.Instance.Vote(m_data.Item, p_isUpVote: false, OnItemUpdated(m_btnVotesDownActive));
			SteamWorkshopUIBrowse.Instance.InvokeOnVoteDownButtonClick(m_data.Item);
		}
	}

	protected virtual void OnItemInstalled(WorkshopItemEventArgs p_itemArgs)
	{
		OnItemUpdated(m_btnPlay)(p_itemArgs);
	}

	protected virtual Action<WorkshopItemEventArgs> OnItemUpdated(Selectable p_focusWhenDone)
	{
		return delegate(WorkshopItemEventArgs p_itemArgs)
		{
			if (!isDestroyed && m_data != null && !p_itemArgs.IsError && m_data.Item.SteamNative.m_nPublishedFileId == p_itemArgs.Item.SteamNative.m_nPublishedFileId)
			{
				uMyGUI_TreeBrowser_InitNode(new SendMessageInitData
				{
					Item = p_itemArgs.Item
				});
				if (m_improveNavigationFocus && p_focusWhenDone != null)
				{
					p_focusWhenDone.Select();
				}
			}
		};
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
		SteamWorkshopItemNode[] componentsInChildren = base.transform.parent.GetComponentsInChildren<SteamWorkshopItemNode>();
		int num = Array.IndexOf(componentsInChildren, this);
		if (num >= 0)
		{
			SteamWorkshopItemNode steamWorkshopItemNode = componentsInChildren[num];
			SteamWorkshopItemNode steamWorkshopItemNode2 = ((num > 0) ? componentsInChildren[num - 1] : null);
			SteamWorkshopItemNode steamWorkshopItemNode3 = ((num < componentsInChildren.Length - 1) ? componentsInChildren[num + 1] : null);
			SetNavigationTargetsVertical(steamWorkshopItemNode.m_btnDelete, new Selectable[3]
			{
				steamWorkshopItemNode2 ? steamWorkshopItemNode2.m_btnDelete : null,
				steamWorkshopItemNode.m_btnDelete,
				steamWorkshopItemNode3 ? steamWorkshopItemNode3.m_btnDelete : null
			});
			SetNavigationTargetsVertical(steamWorkshopItemNode.m_btnVotesUp, new Selectable[5]
			{
				steamWorkshopItemNode2 ? steamWorkshopItemNode2.m_btnVotesUp : null,
				steamWorkshopItemNode2 ? steamWorkshopItemNode2.m_btnVotesUpActive : null,
				steamWorkshopItemNode.m_btnVotesUp,
				steamWorkshopItemNode3 ? steamWorkshopItemNode3.m_btnVotesUp : null,
				steamWorkshopItemNode3 ? steamWorkshopItemNode3.m_btnVotesUpActive : null
			});
			SetNavigationTargetsVertical(steamWorkshopItemNode.m_btnVotesUpActive, new Selectable[5]
			{
				steamWorkshopItemNode2 ? steamWorkshopItemNode2.m_btnVotesUp : null,
				steamWorkshopItemNode2 ? steamWorkshopItemNode2.m_btnVotesUpActive : null,
				steamWorkshopItemNode.m_btnVotesUpActive,
				steamWorkshopItemNode3 ? steamWorkshopItemNode3.m_btnVotesUp : null,
				steamWorkshopItemNode3 ? steamWorkshopItemNode3.m_btnVotesUpActive : null
			});
			SetNavigationTargetsVertical(steamWorkshopItemNode.m_btnVotesDown, new Selectable[5]
			{
				steamWorkshopItemNode2 ? steamWorkshopItemNode2.m_btnVotesDown : null,
				steamWorkshopItemNode2 ? steamWorkshopItemNode2.m_btnVotesDownActive : null,
				steamWorkshopItemNode.m_btnVotesDown,
				steamWorkshopItemNode3 ? steamWorkshopItemNode3.m_btnVotesDown : null,
				steamWorkshopItemNode3 ? steamWorkshopItemNode3.m_btnVotesDownActive : null
			});
			SetNavigationTargetsVertical(steamWorkshopItemNode.m_btnVotesDownActive, new Selectable[5]
			{
				steamWorkshopItemNode2 ? steamWorkshopItemNode2.m_btnVotesDown : null,
				steamWorkshopItemNode2 ? steamWorkshopItemNode2.m_btnVotesDownActive : null,
				steamWorkshopItemNode.m_btnVotesDownActive,
				steamWorkshopItemNode3 ? steamWorkshopItemNode3.m_btnVotesDown : null,
				steamWorkshopItemNode3 ? steamWorkshopItemNode3.m_btnVotesDownActive : null
			});
			SetNavigationTargetsVertical(steamWorkshopItemNode.m_btnFavorites, new Selectable[5]
			{
				steamWorkshopItemNode2 ? steamWorkshopItemNode2.m_btnFavorites : null,
				steamWorkshopItemNode2 ? steamWorkshopItemNode2.m_btnFavoritesActive : null,
				steamWorkshopItemNode.m_btnFavorites,
				steamWorkshopItemNode3 ? steamWorkshopItemNode3.m_btnFavorites : null,
				steamWorkshopItemNode3 ? steamWorkshopItemNode3.m_btnFavoritesActive : null
			});
			SetNavigationTargetsVertical(steamWorkshopItemNode.m_btnFavoritesActive, new Selectable[5]
			{
				steamWorkshopItemNode2 ? steamWorkshopItemNode2.m_btnFavorites : null,
				steamWorkshopItemNode2 ? steamWorkshopItemNode2.m_btnFavoritesActive : null,
				steamWorkshopItemNode.m_btnFavoritesActive,
				steamWorkshopItemNode3 ? steamWorkshopItemNode3.m_btnFavorites : null,
				steamWorkshopItemNode3 ? steamWorkshopItemNode3.m_btnFavoritesActive : null
			});
			SetNavigationTargetsVertical(steamWorkshopItemNode.m_btnSubscriptions, new Selectable[5]
			{
				steamWorkshopItemNode2 ? steamWorkshopItemNode2.m_btnSubscriptions : null,
				steamWorkshopItemNode2 ? steamWorkshopItemNode2.m_btnSubscriptionsActive : null,
				steamWorkshopItemNode.m_btnSubscriptions,
				steamWorkshopItemNode3 ? steamWorkshopItemNode3.m_btnSubscriptions : null,
				steamWorkshopItemNode3 ? steamWorkshopItemNode3.m_btnSubscriptionsActive : null
			});
			SetNavigationTargetsVertical(steamWorkshopItemNode.m_btnSubscriptionsActive, new Selectable[5]
			{
				steamWorkshopItemNode2 ? steamWorkshopItemNode2.m_btnSubscriptions : null,
				steamWorkshopItemNode2 ? steamWorkshopItemNode2.m_btnSubscriptionsActive : null,
				steamWorkshopItemNode.m_btnSubscriptionsActive,
				steamWorkshopItemNode3 ? steamWorkshopItemNode3.m_btnSubscriptions : null,
				steamWorkshopItemNode3 ? steamWorkshopItemNode3.m_btnSubscriptionsActive : null
			});
			SetNavigationTargetsVertical(steamWorkshopItemNode.m_btnPlay, new Selectable[5]
			{
				steamWorkshopItemNode2 ? steamWorkshopItemNode2.m_btnPlay : null,
				steamWorkshopItemNode2 ? steamWorkshopItemNode2.m_btnDownload : null,
				steamWorkshopItemNode.m_btnPlay,
				steamWorkshopItemNode3 ? steamWorkshopItemNode3.m_btnPlay : null,
				steamWorkshopItemNode3 ? steamWorkshopItemNode3.m_btnDownload : null
			});
			SetNavigationTargetsVertical(steamWorkshopItemNode.m_btnDownload, new Selectable[5]
			{
				steamWorkshopItemNode2 ? steamWorkshopItemNode2.m_btnPlay : null,
				steamWorkshopItemNode2 ? steamWorkshopItemNode2.m_btnDownload : null,
				steamWorkshopItemNode.m_btnDownload,
				steamWorkshopItemNode3 ? steamWorkshopItemNode3.m_btnPlay : null,
				steamWorkshopItemNode3 ? steamWorkshopItemNode3.m_btnDownload : null
			});
			if (num == 0 || num == componentsInChildren.Length - 1)
			{
				yield return new WaitForEndOfFrame();
				SetAutomaticNavigation(m_btnDelete);
				SetAutomaticNavigation(m_btnVotesUp);
				SetAutomaticNavigation(m_btnVotesUpActive);
				SetAutomaticNavigation(m_btnVotesDown);
				SetAutomaticNavigation(m_btnVotesDownActive);
				SetAutomaticNavigation(m_btnFavorites);
				SetAutomaticNavigation(m_btnFavoritesActive);
				SetAutomaticNavigation(m_btnSubscriptions);
				SetAutomaticNavigation(m_btnSubscriptionsActive);
				SetAutomaticNavigation(m_btnPlay);
				SetAutomaticNavigation(m_btnDownload);
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
		while (m_data != null && m_data.Item.IsDownloading)
		{
			if (m_textDownloadProgress != null)
			{
				m_textDownloadProgress.gameObject.SetActive(value: true);
				m_textDownloadProgress.text = (int)(SteamWorkshopMain.Instance.GetDownloadProgress(m_data.Item) * 100f) + "%";
			}
			yield return new WaitForSeconds(0.4f);
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

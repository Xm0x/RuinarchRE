using System;
using System.Collections.Generic;
using LapinerTools.Steam.Data;
using LapinerTools.uMyGUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LapinerTools.Steam.UI;

public class SteamWorkshopUIBrowse : MonoBehaviour
{
	[Serializable]
	public class SortingConfig
	{
		[Serializable]
		public class Option
		{
			[SerializeField]
			public WorkshopSortMode MODE = new WorkshopSortMode();

			[SerializeField]
			public string DISPLAY_TEXT = "Votes";
		}

		[SerializeField]
		public uMyGUI_Dropdown DROPDOWN;

		[SerializeField]
		public int DEFAULT_SORT_MODE;

		[SerializeField]
		public Option[] OPTIONS = new Option[0];
	}

	protected static SteamWorkshopUIBrowse s_instance;

	[SerializeField]
	protected uMyGUI_TreeBrowser ITEM_BROWSER;

	[SerializeField]
	protected uMyGUI_PageBox PAGE_SELCTOR;

	[SerializeField]
	protected SortingConfig SORTING;

	[SerializeField]
	protected InputField SEARCH_INPUT;

	[SerializeField]
	protected Button SEARCH_BUTTON;

	[SerializeField]
	[Tooltip("If true, then the first page will be loaded on MonoBehaviour.OnStart")]
	protected bool m_loadOnStart = true;

	[SerializeField]
	protected bool m_improveNavigationFocus = true;

	protected Dictionary<uMyGUI_TreeBrowser.Node, WorkshopItem> m_uiNodeToSteamItem = new Dictionary<uMyGUI_TreeBrowser.Node, WorkshopItem>();

	public static SteamWorkshopUIBrowse Instance
	{
		get
		{
			if (s_instance == null)
			{
				s_instance = UnityEngine.Object.FindObjectOfType<SteamWorkshopUIBrowse>();
			}
			return s_instance;
		}
	}

	public event Action<WorkshopSortModeEventArgs> OnSortModeChanged;

	public event Action<string> OnSearchButtonClick;

	public event Action<int> OnPageChanged;

	public event Action<WorkshopItemEventArgs> OnPlayButtonClick;

	public event Action<WorkshopItemEventArgs> OnVoteUpButtonClick;

	public event Action<WorkshopItemEventArgs> OnVoteDownButtonClick;

	public event Action<WorkshopItemEventArgs> OnSubscribeButtonClick;

	public event Action<WorkshopItemEventArgs> OnUnsubscribeButtonClick;

	public event Action<WorkshopItemEventArgs> OnAddFavoriteButtonClick;

	public event Action<WorkshopItemEventArgs> OnRemoveFavoriteButtonClick;

	public event Action<SteamWorkshopItemNode.ItemDataSetEventArgs> OnItemDataSet;

	public void InvokeOnPlayButtonClick(WorkshopItem p_clickedItem)
	{
		InvokeEventHandlerSafely(this.OnPlayButtonClick, new WorkshopItemEventArgs(p_clickedItem));
	}

	public void InvokeOnVoteUpButtonClick(WorkshopItem p_clickedItem)
	{
		InvokeEventHandlerSafely(this.OnVoteUpButtonClick, new WorkshopItemEventArgs(p_clickedItem));
	}

	public void InvokeOnVoteDownButtonClick(WorkshopItem p_clickedItem)
	{
		InvokeEventHandlerSafely(this.OnVoteDownButtonClick, new WorkshopItemEventArgs(p_clickedItem));
	}

	public void InvokeOnSubscribeButtonClick(WorkshopItem p_clickedItem)
	{
		InvokeEventHandlerSafely(this.OnSubscribeButtonClick, new WorkshopItemEventArgs(p_clickedItem));
	}

	public void InvokeOnUnsubscribeButtonClick(WorkshopItem p_clickedItem)
	{
		InvokeEventHandlerSafely(this.OnUnsubscribeButtonClick, new WorkshopItemEventArgs(p_clickedItem));
	}

	public void InvokeOnAddFavoriteButtonClick(WorkshopItem p_clickedItem)
	{
		InvokeEventHandlerSafely(this.OnAddFavoriteButtonClick, new WorkshopItemEventArgs(p_clickedItem));
	}

	public void InvokeOnRemoveFavoriteButtonClick(WorkshopItem p_clickedItem)
	{
		InvokeEventHandlerSafely(this.OnRemoveFavoriteButtonClick, new WorkshopItemEventArgs(p_clickedItem));
	}

	public void InvokeOnItemDataSet(WorkshopItem p_itemData, SteamWorkshopItemNode p_itemUI)
	{
		InvokeEventHandlerSafely(this.OnItemDataSet, new SteamWorkshopItemNode.ItemDataSetEventArgs
		{
			ItemData = p_itemData,
			ItemUI = p_itemUI
		});
	}

	public void SetItems(WorkshopItemList p_itemList)
	{
		if (ITEM_BROWSER != null)
		{
			m_uiNodeToSteamItem.Clear();
			ITEM_BROWSER.Clear();
			ITEM_BROWSER.BuildTree(ConvertItemsToNodes(p_itemList.Items.ToArray()));
		}
		else
		{
			Debug.LogError("SteamWorkshopUIBrowse: SetItems: ITEM_BROWSER is not set in inspector!");
		}
		if (PAGE_SELCTOR != null)
		{
			PAGE_SELCTOR.OnPageSelected -= SetPage;
			PAGE_SELCTOR.SetPageCount((int)p_itemList.PagesItems);
			PAGE_SELCTOR.SelectPage((int)p_itemList.Page);
			PAGE_SELCTOR.OnPageSelected += SetPage;
		}
		else
		{
			Debug.LogError("SteamWorkshopUIBrowse: SetItems: PAGE_SELCTOR is not set in inspector!");
		}
		if (m_improveNavigationFocus && ITEM_BROWSER != null && ITEM_BROWSER.transform.childCount > 0 && ITEM_BROWSER.transform.GetChild(0).GetComponent<SteamWorkshopItemNode>() != null)
		{
			ITEM_BROWSER.transform.GetChild(0).GetComponent<SteamWorkshopItemNode>().Select();
		}
	}

	public void LoadItems(int p_page)
	{
		uMyGUI_PopupManager.Instance.ShowPopup("loading");
		SteamWorkshopMain.Instance.GetItemList((uint)p_page, delegate
		{
			uMyGUI_PopupManager.Instance.HidePopup("loading");
		});
	}

	public void Search(string p_searchText)
	{
		bool num = p_searchText != SteamWorkshopMain.Instance.SearchText;
		bool flag = SteamWorkshopMain.Instance.SearchText != null && !string.IsNullOrEmpty(SteamWorkshopMain.Instance.SearchText);
		bool flag2 = p_searchText != null && !string.IsNullOrEmpty(p_searchText.Trim());
		SteamWorkshopMain.Instance.SearchText = p_searchText;
		if (num && (flag2 || (flag && !flag2)))
		{
			InvokeEventHandlerSafely(this.OnSearchButtonClick, p_searchText);
			LoadItems(1);
		}
	}

	protected void SetPage(int p_page)
	{
		InvokeEventHandlerSafely(this.OnPageChanged, p_page);
		LoadItems(p_page);
	}

	protected virtual void Start()
	{
		InitSorting();
		InitSearch();
		SteamWorkshopMain.Instance.OnItemListLoaded += SetItems;
		SteamWorkshopMain.Instance.OnError += ShowErrorMessage;
		if (m_loadOnStart)
		{
			LoadItems(1);
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
			else if (ITEM_BROWSER != null && ITEM_BROWSER.transform.childCount > 0 && ITEM_BROWSER.transform.GetChild(0).GetComponent<SteamWorkshopItemNode>() != null)
			{
				ITEM_BROWSER.transform.GetChild(0).GetComponent<SteamWorkshopItemNode>().Select();
			}
			else if (SEARCH_INPUT != null)
			{
				SEARCH_INPUT.Select();
			}
		}
	}

	protected virtual void OnDestroy()
	{
		if (SteamWorkshopMain.IsInstanceSet)
		{
			SteamWorkshopMain.Instance.OnItemListLoaded -= SetItems;
			SteamWorkshopMain.Instance.OnError -= ShowErrorMessage;
		}
	}

	protected virtual void ShowErrorMessage(ErrorEventArgs p_errorArgs)
	{
		uMyGUI_PopupManager.Instance.HidePopup("loading");
		((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup("text")).SetText("Steam Error", p_errorArgs.ErrorMessage).ShowButton("ok");
	}

	protected virtual void SetItems(WorkshopItemListEventArgs p_itemListArgs)
	{
		if (!p_itemListArgs.IsError)
		{
			SetItems(p_itemListArgs.ItemList);
		}
		else
		{
			Debug.LogError("SteamWorkshopUIBrowse: SetItems: Steam Error: " + p_itemListArgs.ErrorMessage);
		}
	}

	protected virtual uMyGUI_TreeBrowser.Node[] ConvertItemsToNodes(WorkshopItem[] p_items)
	{
		uMyGUI_TreeBrowser.Node[] array = new uMyGUI_TreeBrowser.Node[p_items.Length];
		for (int i = 0; i < p_items.Length; i++)
		{
			if (p_items[i] != null)
			{
				SteamWorkshopItemNode.SendMessageInitData p_sendMessageData = new SteamWorkshopItemNode.SendMessageInitData
				{
					Item = p_items[i]
				};
				uMyGUI_TreeBrowser.Node key = (array[i] = new uMyGUI_TreeBrowser.Node(p_sendMessageData, null));
				m_uiNodeToSteamItem.Add(key, p_items[i]);
			}
			else
			{
				Debug.LogError("SteamWorkshopUIBrowse: ConvertItemsToNodes: item at index '" + i + "' is null!");
			}
		}
		return array;
	}

	protected virtual void InitSorting()
	{
		if (SORTING != null && SORTING.DROPDOWN != null)
		{
			string[] array = new string[SORTING.OPTIONS.Length];
			for (int i = 0; i < array.Length; i++)
			{
				if (SORTING.OPTIONS[i] != null)
				{
					array[i] = SORTING.OPTIONS[i].DISPLAY_TEXT;
				}
				else
				{
					array[i] = "NULL";
				}
			}
			SORTING.DROPDOWN.Entries = array;
			SORTING.DROPDOWN.Select(Mathf.Clamp(SORTING.DEFAULT_SORT_MODE, 0, array.Length - 1));
			SORTING.DROPDOWN.OnSelected += delegate(int p_selectedSortIndex)
			{
				if (p_selectedSortIndex >= 0 && p_selectedSortIndex < SORTING.OPTIONS.Length)
				{
					WorkshopSortMode mODE = SORTING.OPTIONS[p_selectedSortIndex].MODE;
					bool num = SteamWorkshopMain.Instance.Sorting != mODE;
					SteamWorkshopMain.Instance.Sorting = mODE;
					if (num)
					{
						InvokeEventHandlerSafely(this.OnSortModeChanged, new WorkshopSortModeEventArgs(mODE));
						LoadItems(1);
					}
				}
			};
			if (SORTING.DEFAULT_SORT_MODE >= 0 && SORTING.DEFAULT_SORT_MODE < SORTING.OPTIONS.Length)
			{
				SteamWorkshopMain.Instance.Sorting = SORTING.OPTIONS[SORTING.DEFAULT_SORT_MODE].MODE;
			}
		}
		else
		{
			Debug.LogError("SteamWorkshopUIBrowse: SORTING.DROPDOWN is not set in inspector!");
		}
	}

	protected virtual void InitSearch()
	{
		if (SEARCH_INPUT != null)
		{
			SEARCH_INPUT.onEndEdit.AddListener(Search);
			if (SEARCH_BUTTON != null)
			{
				SEARCH_BUTTON.onClick.AddListener(delegate
				{
					if (SEARCH_INPUT != null)
					{
						Search(SEARCH_INPUT.text);
					}
				});
			}
			else
			{
				Debug.LogError("SteamWorkshopUIBrowse: SEARCH_BUTTON is not set in inspector!");
			}
		}
		else
		{
			Debug.LogError("SteamWorkshopUIBrowse: SEARCH_INPUT is not set in inspector!");
		}
	}

	protected virtual void InvokeEventHandlerSafely<T>(Action<T> p_handler, T p_data)
	{
		try
		{
			p_handler?.Invoke(p_data);
		}
		catch (Exception ex)
		{
			Debug.LogError("SteamWorkshopUIBrowse: your event handler (" + p_handler.Target?.ToString() + " - System.Action<" + typeof(T)?.ToString() + ">) has thrown an excepotion!\n" + ex);
		}
	}
}

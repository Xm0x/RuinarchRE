using System;
using LapinerTools.Steam;
using LapinerTools.Steam.Data;
using Ruinarch.Custom_UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class SWBParentUI : MonoBehaviour
{
	[Serializable]
	public class SortingConfig
	{
		[Serializable]
		public class Option
		{
			[SerializeField]
			public WorkshopSortMode sortMode = new WorkshopSortMode();

			[SerializeField]
			private string _displayText = "Votes";

			[SerializeField]
			private bool _useLocalization;

			private string _localizedText;

			public string GetDisplayText()
			{
				if (_useLocalization)
				{
					if (!string.IsNullOrEmpty(_localizedText))
					{
						return _localizedText;
					}
					_localizedText = LocalizationManager.Instance.GetLocalizedValue("OtherUI_Table", _displayText);
					if (!string.IsNullOrEmpty(_localizedText))
					{
						return _localizedText;
					}
				}
				return _displayText;
			}
		}

		[SerializeField]
		public SWBDropdown dropdown;

		[SerializeField]
		public int defaultSortMode;

		[SerializeField]
		public Option[] options = new Option[0];
	}

	protected static SWBParentUI s_instance;

	[SerializeField]
	protected GameObject _windowGO;

	[SerializeField]
	protected SWBContentPanel contentPanel;

	[SerializeField]
	protected SWBPagination pagination;

	[SerializeField]
	protected SortingConfig sortingConfig;

	[SerializeField]
	protected TMP_InputField searchInputField;

	[SerializeField]
	protected RuinarchButton searchButton;

	[SerializeField]
	[Tooltip("If true, then the first page will be loaded on MonoBehaviour.OnStart")]
	protected bool m_loadOnStart = true;

	[SerializeField]
	protected bool m_improveNavigationFocus = true;

	private bool _hasLoaded;

	public static SWBParentUI Instance => s_instance;

	public event Action<WorkshopSortModeEventArgs> onSortModeChanged;

	public event Action<string> onSearchButtonClick;

	public event Action<int> onPageChanged;

	public event Action<WorkshopItemEventArgs> onPlayButtonClick;

	public event Action<WorkshopItemEventArgs> onVoteUpButtonClick;

	public event Action<WorkshopItemEventArgs> onVoteDownButtonClick;

	public event Action<WorkshopItemEventArgs> onSubscribeButtonClick;

	public event Action<WorkshopItemEventArgs> onUnsubscribeButtonClick;

	public event Action<WorkshopItemEventArgs> onAddFavoriteButtonClick;

	public event Action<WorkshopItemEventArgs> onRemoveFavoriteButtonClick;

	public event Action<SWBItem.ItemDataSetEventArgs> onItemDataSet;

	public void InvokeOnPlayButtonClick(WorkshopItem p_clickedItem)
	{
		InvokeEventHandlerSafely(this.onPlayButtonClick, new WorkshopItemEventArgs(p_clickedItem));
	}

	public void InvokeOnVoteUpButtonClick(WorkshopItem p_clickedItem)
	{
		InvokeEventHandlerSafely(this.onVoteUpButtonClick, new WorkshopItemEventArgs(p_clickedItem));
	}

	public void InvokeOnVoteDownButtonClick(WorkshopItem p_clickedItem)
	{
		InvokeEventHandlerSafely(this.onVoteDownButtonClick, new WorkshopItemEventArgs(p_clickedItem));
	}

	public void InvokeOnSubscribeButtonClick(WorkshopItem p_clickedItem)
	{
		InvokeEventHandlerSafely(this.onSubscribeButtonClick, new WorkshopItemEventArgs(p_clickedItem));
	}

	public void InvokeOnUnsubscribeButtonClick(WorkshopItem p_clickedItem)
	{
		InvokeEventHandlerSafely(this.onUnsubscribeButtonClick, new WorkshopItemEventArgs(p_clickedItem));
	}

	public void InvokeOnAddFavoriteButtonClick(WorkshopItem p_clickedItem)
	{
		InvokeEventHandlerSafely(this.onAddFavoriteButtonClick, new WorkshopItemEventArgs(p_clickedItem));
	}

	public void InvokeOnRemoveFavoriteButtonClick(WorkshopItem p_clickedItem)
	{
		InvokeEventHandlerSafely(this.onRemoveFavoriteButtonClick, new WorkshopItemEventArgs(p_clickedItem));
	}

	public void InvokeOnItemDataSet(WorkshopItem p_itemData, SWBItem p_itemUI)
	{
		InvokeEventHandlerSafely(this.onItemDataSet, new SWBItem.ItemDataSetEventArgs
		{
			ItemData = p_itemData,
			ItemUI = p_itemUI
		});
	}

	private void Awake()
	{
		s_instance = this;
	}

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
		if (!_hasLoaded)
		{
			_hasLoaded = true;
			LoadItems(1);
		}
		_windowGO.SetActive(value: true);
	}

	private void Hide()
	{
		_windowGO.SetActive(value: false);
	}

	protected virtual void Start()
	{
		InitSorting();
		InitSearch();
		if (SteamWorkshopMain.IsInstanceSet)
		{
			SteamWorkshopMain.Instance.OnItemListLoaded += SetItems;
			SteamWorkshopMain.Instance.OnError += ShowErrorMessage;
		}
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
			else if (contentPanel != null && contentPanel.transform.childCount > 0 && contentPanel.transform.GetChild(0).GetComponent<SWBItem>() != null)
			{
				contentPanel.transform.GetChild(0).GetComponent<SWBItem>().Select();
			}
			else if (searchInputField != null)
			{
				searchInputField.Select();
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

	public void SetItems(WorkshopItemList p_itemList)
	{
		if (contentPanel != null)
		{
			contentPanel.Clear();
			contentPanel.BuildTree(p_itemList.Items);
		}
		else
		{
			Debug.LogError("SteamWorkshopUIBrowse: SetItems: ITEM_BROWSER is not set in inspector!");
		}
		if (pagination != null)
		{
			pagination.OnPageSelected -= SetPage;
			pagination.SetPageCount((int)p_itemList.PagesItems);
			pagination.SelectPage((int)p_itemList.Page);
			pagination.OnPageSelected += SetPage;
		}
		else
		{
			Debug.LogError("SteamWorkshopUIBrowse: SetItems: PAGE_SELCTOR is not set in inspector!");
		}
		if (m_improveNavigationFocus && contentPanel != null && contentPanel.transform.childCount > 0 && contentPanel.transform.GetChild(0).GetComponent<SWBItem>() != null)
		{
			contentPanel.transform.GetChild(0).GetComponent<SWBItem>().Select();
		}
	}

	public void LoadItems(int p_page)
	{
		SWBLoadingPopup.Instance.Show();
		SteamWorkshopMain.Instance.GetItemList((uint)p_page, delegate
		{
			SWBLoadingPopup.Instance.Hide();
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
			InvokeEventHandlerSafely(this.onSearchButtonClick, p_searchText);
			LoadItems(1);
		}
	}

	protected void SetPage(int p_page)
	{
		InvokeEventHandlerSafely(this.onPageChanged, p_page);
		LoadItems(p_page);
	}

	protected virtual void ShowErrorMessage(ErrorEventArgs p_errorArgs)
	{
		SWBLoadingPopup.Instance.Hide();
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("OtherUI_Table", "Steam_Error");
		MainMenuUI.Instance.generalConfirmation.ShowGeneralConfirmation(localizedValue, p_errorArgs.ErrorMessage);
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

	protected virtual void InitSorting()
	{
		if (sortingConfig != null && sortingConfig.dropdown != null)
		{
			string[] array = new string[sortingConfig.options.Length];
			for (int i = 0; i < array.Length; i++)
			{
				if (sortingConfig.options[i] != null)
				{
					array[i] = sortingConfig.options[i].GetDisplayText();
				}
				else
				{
					array[i] = "NULL";
				}
			}
			sortingConfig.dropdown.Entries = array;
			sortingConfig.dropdown.Select(Mathf.Clamp(sortingConfig.defaultSortMode, 0, array.Length - 1));
			sortingConfig.dropdown.OnSelected += delegate(int p_selectedSortIndex)
			{
				if (p_selectedSortIndex >= 0 && p_selectedSortIndex < sortingConfig.options.Length)
				{
					WorkshopSortMode sortMode = sortingConfig.options[p_selectedSortIndex].sortMode;
					bool num = SteamWorkshopMain.Instance.Sorting != sortMode;
					SteamWorkshopMain.Instance.Sorting = sortMode;
					if (num)
					{
						InvokeEventHandlerSafely(this.onSortModeChanged, new WorkshopSortModeEventArgs(sortMode));
						LoadItems(1);
					}
				}
			};
			if (sortingConfig.defaultSortMode >= 0 && sortingConfig.defaultSortMode < sortingConfig.options.Length)
			{
				SteamWorkshopMain.Instance.Sorting = sortingConfig.options[sortingConfig.defaultSortMode].sortMode;
			}
		}
		else
		{
			Debug.LogError("SteamWorkshopUIBrowse: SORTING.DROPDOWN is not set in inspector!");
		}
	}

	protected virtual void InitSearch()
	{
		if (searchInputField != null)
		{
			searchInputField.onEndEdit.AddListener(Search);
			if (searchButton != null)
			{
				searchButton.onClick.AddListener(delegate
				{
					if (searchInputField != null)
					{
						Search(searchInputField.text);
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

	public void ReloadPages()
	{
		LoadItems(1);
	}

	public SWBItem GetActiveItem(ulong p_publishedFieldId)
	{
		return contentPanel.GetActiveItem(p_publishedFieldId);
	}
}

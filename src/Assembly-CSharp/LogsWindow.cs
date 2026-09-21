using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class LogsWindow : MonoBehaviour
{
	[Space(10f)]
	[Header("Logs")]
	[SerializeField]
	private GameObject logParentGO;

	[SerializeField]
	private GameObject logHistoryPrefab;

	[SerializeField]
	private ScrollRect historyScrollView;

	[SerializeField]
	private UIHoverPosition logHoverPosition;

	[SerializeField]
	private GameObject daySeparatorPrefab;

	[SerializeField]
	private TMP_InputField searchField;

	[SerializeField]
	private Button clearBtn;

	[SerializeField]
	private float fixedLogWidth = -1f;

	private List<LogHistoryItem> logHistoryItems;

	private List<DaySeparator> daySeparators;

	[Space(10f)]
	[Header("Filters")]
	[SerializeField]
	private LogFiltersWindow _logFiltersWindow;

	[SerializeField]
	private Button filtersBtn;

	private string _objPersistentID;

	private static string SharedSearch;

	private List<Log> _logs;

	private int _previousStartRange;

	private int _startRange;

	private int _rangeLength;

	private float _lastScrollbarValue;

	private bool _reachedEndOfLogs;

	private bool _reachedStartOfLogs;

	private bool _dontNotifyScrollbar;

	private const int MAX_LIMIT = 40;

	private const int MAX_THRESHOLD = 20;

	private RuinarchScrollbar _scrollbar;

	public void Initialize()
	{
		_scrollbar = historyScrollView.verticalScrollbar as RuinarchScrollbar;
		_logs = new List<Log>(80);
		logHistoryItems = new List<LogHistoryItem>(80);
		daySeparators = new List<DaySeparator>(5);
		searchField.onValueChanged.AddListener(OnEndSearchEdit);
		clearBtn.onClick.AddListener(OnClickClearSearch);
		filtersBtn.onClick.AddListener(ToggleFiltersWindow);
		historyScrollView.verticalScrollbar.onValueChanged.AddListener(OnScrollbarValueChanged);
		_logFiltersWindow.Initialize();
		_logFiltersWindow.SetShowAllTogglesStateWithoutNotify(p_state: true);
		_logFiltersWindow.AddOnToggleActionOfTypeFilterToggles(OnToggleFilter);
		_logFiltersWindow.AddOnToggleActionOfShowAllToggle(OnToggleAllFilters);
		_logFiltersWindow.SetAllTypeTogglesStateWithoutNotify(p_state: true);
		_logFiltersWindow.DisableVillagesTab();
		_logFiltersWindow.AddOnToggleVillageFilterAction(OnToggleVillageFilter);
		for (int i = 0; i < 40; i++)
		{
			CreateNewLogHistoryItem();
		}
	}

	public void OnParentMenuOpened(string id)
	{
		_objPersistentID = id;
		searchField.SetTextWithoutNotify(SharedSearch);
		clearBtn.gameObject.SetActive(!string.IsNullOrEmpty(SharedSearch));
		ResetRangeLength();
	}

	private void CreateNewLogHistoryItem()
	{
		GameObject obj = ObjectPoolManager.Instance.InstantiateObjectFromPool(logHistoryPrefab.name, Vector3.zero, Quaternion.identity, historyScrollView.content);
		obj.transform.localScale = Vector3.one;
		LogHistoryItem component = obj.GetComponent<LogHistoryItem>();
		component.Hide();
		if (fixedLogWidth > 0f)
		{
			component.SetLogLblWidth(fixedLogWidth);
		}
		logHistoryItems.Add(component);
	}

	public void UpdateAllHistoryInfo()
	{
		UIManager.Instance.StartCoroutine(UpdateAllHistoryInfoLimitedScrollDown());
	}

	private IEnumerator UpdateAllHistoryInfoCoroutine()
	{
		List<Log> logs = DatabaseManager.Instance.mainSQLDatabase.GetLogsThatMatchCriteria(_objPersistentID, SharedSearch, _logFiltersWindow.enabledFilters);
		int historyCount = logs?.Count ?? 0;
		int historyLastIndex = historyCount - 1;
		int missingItems = historyCount - logHistoryItems.Count;
		int batches = 0;
		for (int i = 0; i < missingItems; i++)
		{
			CreateNewLogHistoryItem();
			batches++;
			if (batches > 500)
			{
				batches = 0;
				yield return null;
			}
		}
		for (int j = 0; j < daySeparators.Count; j++)
		{
			ObjectPoolManager.Instance.DestroyObject(daySeparators[j]);
		}
		daySeparators.Clear();
		batches = 0;
		int currentDay = 0;
		for (int i = 0; i < logHistoryItems.Count; i++)
		{
			LogHistoryItem logHistoryItem = logHistoryItems[i];
			logHistoryItem.ManualReset();
			if (logs != null && i < historyCount)
			{
				Log log = logs[historyLastIndex - i];
				logHistoryItem.Show();
				logHistoryItem.SetLog(log);
				logHistoryItem.SetHoverPosition(logHoverPosition);
				if (log.gameDate.day != currentDay)
				{
					int num = logHistoryItem.transform.GetSiblingIndex();
					if (num < 0)
					{
						num = 0;
					}
					CreateDaySeparator(log.gameDate.ConvertToContinuousDays(), num);
					currentDay = log.gameDate.day;
				}
			}
			else
			{
				logHistoryItem.Hide();
			}
			batches++;
			if (batches > 1000)
			{
				batches = 0;
				yield return null;
			}
		}
		logs.ReleaseLogInstancesAndLogList();
	}

	private IEnumerator UpdateAllHistoryInfoLimitedScrollDown()
	{
		_dontNotifyScrollbar = true;
		UpdateLogsList();
		for (int i = 0; i < daySeparators.Count; i++)
		{
			ObjectPoolManager.Instance.DestroyObject(daySeparators[i]);
		}
		daySeparators.Clear();
		int count = _logs.Count;
		if (count > 0)
		{
			if (logHistoryItems.Count < count + 20)
			{
				for (int j = 0; j < 20; j++)
				{
					CreateNewLogHistoryItem();
				}
			}
			int num = 0;
			int startIndex = 0;
			if (_startRange > 0)
			{
				startIndex = 20;
			}
			for (int k = startIndex; k < logHistoryItems.Count; k++)
			{
				int num2 = k;
				if (startIndex == 20)
				{
					num2 = k - 20;
				}
				LogHistoryItem logHistoryItem = logHistoryItems[k];
				logHistoryItem.Reset();
				if (num2 < count)
				{
					Log log = _logs[num2];
					logHistoryItem.Show();
					logHistoryItem.SetLog(log);
					logHistoryItem.SetHoverPosition(logHoverPosition);
					if (log.gameDate.day != num)
					{
						int num3 = logHistoryItem.transform.GetSiblingIndex();
						if (num3 < 0)
						{
							num3 = 0;
						}
						CreateDaySeparator(log.gameDate.ConvertToContinuousDays(), num3);
						num = log.gameDate.day;
					}
				}
				else
				{
					logHistoryItem.Hide();
				}
			}
			yield return null;
			yield return null;
			yield return null;
			float contentHeightBefore = historyScrollView.content.rect.height;
			int num4 = 0;
			bool hasRemoved = false;
			while (num4 < startIndex)
			{
				ObjectPoolManager.Instance.DestroyObject(logHistoryItems[0]);
				logHistoryItems.RemoveAt(0);
				num4++;
				hasRemoved = true;
			}
			yield return null;
			yield return null;
			if (hasRemoved)
			{
				float height = historyScrollView.content.rect.height;
				historyScrollView.verticalScrollbar.value = (height - height * historyScrollView.verticalScrollbar.value) / contentHeightBefore;
			}
		}
		else
		{
			for (int l = 0; l < logHistoryItems.Count; l++)
			{
				logHistoryItems[l].Hide();
			}
		}
		_logs.ReleaseLogInstances();
		_logs.Clear();
		_dontNotifyScrollbar = false;
	}

	private IEnumerator UpdateAllHistoryInfoLimitedScrollUp()
	{
		_dontNotifyScrollbar = true;
		UpdateLogsList();
		for (int i = 0; i < daySeparators.Count; i++)
		{
			ObjectPoolManager.Instance.DestroyObject(daySeparators[i]);
		}
		daySeparators.Clear();
		int count = _logs.Count;
		if (count > 0)
		{
			if (logHistoryItems.Count < count + 20)
			{
				for (int j = 0; j < 20; j++)
				{
					CreateNewLogHistoryItem();
				}
			}
			int num = 0;
			for (int k = 0; k < logHistoryItems.Count; k++)
			{
				LogHistoryItem logHistoryItem = logHistoryItems[k];
				logHistoryItem.Show();
				if (k >= count)
				{
					continue;
				}
				Log log = _logs[k];
				logHistoryItem.SetLog(log);
				logHistoryItem.SetHoverPosition(logHoverPosition);
				if (log.gameDate.day != num)
				{
					int num2 = logHistoryItem.transform.GetSiblingIndex();
					if (num2 < 0)
					{
						num2 = 0;
					}
					CreateDaySeparator(log.gameDate.ConvertToContinuousDays(), num2);
					num = log.gameDate.day;
				}
			}
			yield return null;
			yield return null;
			yield return null;
			float contentHeightBefore = historyScrollView.content.rect.height;
			int num3 = 0;
			bool hasRemoved = false;
			while (num3 < 20)
			{
				ObjectPoolManager.Instance.DestroyObject(logHistoryItems[logHistoryItems.Count - 1]);
				logHistoryItems.RemoveAt(logHistoryItems.Count - 1);
				num3++;
				hasRemoved = true;
			}
			yield return null;
			yield return null;
			if (hasRemoved)
			{
				float height = historyScrollView.content.rect.height;
				float value = 1f - height / contentHeightBefore;
				historyScrollView.verticalScrollbar.value = value;
			}
		}
		_logs.ReleaseLogInstances();
		_logs.Clear();
		_dontNotifyScrollbar = false;
	}

	private void UpdateLogsList()
	{
		int num = _rangeLength - _startRange;
		_logs.Clear();
		DatabaseManager.Instance.mainSQLDatabase.PopulateLogsThatMatchCriteria(_logs, _objPersistentID, SharedSearch, _logFiltersWindow.enabledFilters, _startRange, num);
		if (_startRange > 0)
		{
			_reachedStartOfLogs = false;
			if (_logs.Count < num)
			{
				_startRange = _previousStartRange;
				num = _rangeLength - _startRange;
				_logs.Clear();
				DatabaseManager.Instance.mainSQLDatabase.PopulateLogsThatMatchCriteria(_logs, _objPersistentID, SharedSearch, _logFiltersWindow.enabledFilters, _startRange, num);
				if (_startRange <= 0)
				{
					_reachedStartOfLogs = true;
				}
				_reachedEndOfLogs = true;
			}
			else if (_logs.Count < num)
			{
				_reachedEndOfLogs = true;
			}
			else
			{
				_reachedEndOfLogs = false;
			}
		}
		else
		{
			_reachedStartOfLogs = true;
			if (_logs.Count < num)
			{
				_reachedEndOfLogs = true;
			}
			else
			{
				_reachedEndOfLogs = false;
			}
		}
	}

	private void CreateDaySeparator(int day, int indexInHierarchy)
	{
		GameObject obj = ObjectPoolManager.Instance.InstantiateObjectFromPool(daySeparatorPrefab.name, Vector3.zero, Quaternion.identity, historyScrollView.content);
		DaySeparator component = obj.GetComponent<DaySeparator>();
		component.SetDay(day);
		obj.transform.SetSiblingIndex(indexInHierarchy);
		daySeparators.Add(component);
	}

	public void ResetScrollPosition()
	{
		historyScrollView.verticalNormalizedPosition = 1f;
	}

	public void DoSearch()
	{
		UpdateAllHistoryInfo();
	}

	private void OnEndSearchEdit(string text)
	{
		SharedSearch = text;
		clearBtn.gameObject.SetActive(!string.IsNullOrEmpty(SharedSearch));
		ResetRangeLength();
		UpdateAllHistoryInfo();
	}

	private void OnClickClearSearch()
	{
		searchField.SetTextWithoutNotify(string.Empty);
		ResetRangeLength();
		OnEndSearchEdit(string.Empty);
	}

	private void ToggleFiltersWindow()
	{
		_logFiltersWindow.ToggleFilters();
	}

	private void OnToggleFilter(bool isOn, LOG_TAG tag)
	{
		UpdateAllHistoryInfo();
	}

	private void OnToggleAllFilters(bool state)
	{
		UpdateAllHistoryInfo();
	}

	private void OnToggleVillageFilter(bool isOn, NPCSettlement p_settlement)
	{
		UpdateAllHistoryInfo();
	}

	private void ResetRangeLength()
	{
		_startRange = 0;
		_rangeLength = 20;
		_reachedEndOfLogs = false;
		_reachedStartOfLogs = true;
	}

	private void IncreaseRangeLength()
	{
		_rangeLength += 20;
		if (_rangeLength > 40)
		{
			_previousStartRange = _startRange;
			_startRange = _rangeLength - 40;
		}
	}

	private void DecreaseRangeLength()
	{
		if (_startRange > 0)
		{
			_previousStartRange = _startRange;
			_startRange -= 20;
			if (_startRange < 0)
			{
				_startRange = 0;
			}
			_rangeLength = _startRange + 40;
		}
	}

	private void OnScrollbarValueChanged(float p_value)
	{
		bool flag = IsScrollingDown(p_value);
		_lastScrollbarValue = p_value;
		if (!_dontNotifyScrollbar)
		{
			if (p_value <= 0.05f && !_reachedEndOfLogs && flag)
			{
				IncreaseRangeLength();
				UIManager.Instance.StartCoroutine(UpdateAllHistoryInfoLimitedScrollDown());
			}
			else if (p_value >= 0.95f && !_reachedStartOfLogs && !flag)
			{
				DecreaseRangeLength();
				UIManager.Instance.StartCoroutine(UpdateAllHistoryInfoLimitedScrollUp());
			}
		}
	}

	private void UpdateLogsOnPointerUp(float p_scrollbarValue)
	{
		if (p_scrollbarValue <= 0.05f && !_reachedEndOfLogs)
		{
			IncreaseRangeLength();
			UIManager.Instance.StartCoroutine(UpdateAllHistoryInfoLimitedScrollDown());
		}
		else if (p_scrollbarValue >= 0.95f && !_reachedStartOfLogs)
		{
			DecreaseRangeLength();
			UIManager.Instance.StartCoroutine(UpdateAllHistoryInfoLimitedScrollUp());
		}
	}

	private void OnScrollbarPointerUp()
	{
		UpdateLogsOnPointerUp(historyScrollView.verticalScrollbar.value);
	}

	private bool IsScrollingDown(float p_value)
	{
		return p_value < _lastScrollbarValue;
	}
}

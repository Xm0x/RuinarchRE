using System;
using System.Collections.Generic;
using System.Linq;
using Locations.Settlements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UtilityScripts;

public class LogFiltersWindow : MonoBehaviour
{
	[SerializeField]
	private GameObject _filterGO;

	[SerializeField]
	private Toggle _tglTabTypeFilters;

	[SerializeField]
	private Toggle _tglTabVillageFilters;

	[Header("Type Filters")]
	[SerializeField]
	private GameObject _typeFiltersContainer;

	[SerializeField]
	private LogFilterItem[] _typeFilters;

	[SerializeField]
	private Toggle _showAllTypesToggle;

	[Header("Village Filters")]
	[SerializeField]
	private GameObject _villageFiltersContainer;

	[SerializeField]
	private Toggle _showAllVillagesToggle;

	[SerializeField]
	private GameObject _villageFilterPrefab;

	[SerializeField]
	private ScrollRect _villageFiltersScrollRect;

	private List<LogVillageFilterItem> _villageFilters;

	private Action<bool, NPCSettlement> _onToggleSettlementFilterAction;

	private List<LOG_TAG> _enabledFilters;

	private List<string> _enabledVillageFilters;

	public LogFilterItem[] typeFilters => _typeFilters;

	public Toggle showAllTypesToggle => _showAllTypesToggle;

	public List<string> enabledVillageFilters => _enabledVillageFilters;

	public List<LOG_TAG> enabledFilters => _enabledFilters;

	private void OnDisable()
	{
		_filterGO.gameObject.SetActive(value: false);
	}

	private void OnDestroy()
	{
		_enabledFilters = null;
		_enabledVillageFilters = null;
		Messenger.RemoveListener<BaseSettlement>(SettlementSignals.SETTLEMENT_CREATED, OnSettlementCreated);
	}

	private void OnEnable()
	{
		for (int i = 0; i < typeFilters.Length; i++)
		{
			LogFilterItem logFilterItem = typeFilters[i];
			logFilterItem.SetIsOnWithoutNotify(enabledFilters.Contains(logFilterItem.filterType));
		}
		showAllTypesToggle.SetIsOnWithoutNotify(AreAllTypeFiltersOn());
		for (int j = 0; j < _villageFilters.Count; j++)
		{
			LogVillageFilterItem logVillageFilterItem = _villageFilters[j];
			logVillageFilterItem.SetIsOnWithoutNotify(enabledVillageFilters.Contains(logVillageFilterItem.settlementPersistentID));
		}
		_showAllVillagesToggle.SetIsOnWithoutNotify(AreAllVillageFiltersOn());
	}

	public void Initialize()
	{
		_enabledFilters = CollectionUtilities.GetEnumValues<LOG_TAG>().ToList();
		_enabledVillageFilters = new List<string>();
		_tglTabTypeFilters.onValueChanged.RemoveAllListeners();
		_tglTabTypeFilters.onValueChanged.AddListener(OnToggleTypeTab);
		_tglTabVillageFilters.onValueChanged.RemoveAllListeners();
		_tglTabVillageFilters.onValueChanged.AddListener(OnToggleVillagesTab);
		_villageFilters = new List<LogVillageFilterItem>();
		CreateInitialVillageFilters();
		AddOnToggleActionOfShowAllToggle(OnToggleShowAllFilters);
		_showAllVillagesToggle.onValueChanged.AddListener(OnToggleShowAllVillages);
		AddOnToggleActionOfTypeFilterToggles(OnToggleFilter);
	}

	public void InitializeAfterLoadoutPicked()
	{
		CreateInitialVillageFilters();
		Messenger.AddListener<BaseSettlement>(SettlementSignals.SETTLEMENT_CREATED, OnSettlementCreated);
	}

	public void SetShowAllTogglesStateWithoutNotify(bool p_state)
	{
		_showAllTypesToggle.SetIsOnWithoutNotify(p_state);
	}

	public void AddOnToggleActionOfShowAllToggle(UnityAction<bool> onToggleAction)
	{
		_showAllTypesToggle.onValueChanged.AddListener(onToggleAction);
	}

	private void OnToggleShowAllFilters(bool state)
	{
		enabledFilters.Clear();
		for (int i = 0; i < typeFilters.Length; i++)
		{
			LogFilterItem logFilterItem = typeFilters[i];
			logFilterItem.SetIsOnWithoutNotify(state);
			if (state)
			{
				enabledFilters.Add(logFilterItem.filterType);
			}
		}
	}

	private void OnToggleTypeTab(bool p_isOn)
	{
		_typeFiltersContainer.SetActive(p_isOn);
	}

	public void AddOnToggleActionOfTypeFilterToggles(Action<bool, LOG_TAG> onToggleAction)
	{
		for (int i = 0; i < _typeFilters.Length; i++)
		{
			_typeFilters[i].AddOnToggleAction(onToggleAction);
		}
	}

	public void SetAllTypeTogglesStateWithoutNotify(bool p_state)
	{
		for (int i = 0; i < _typeFilters.Length; i++)
		{
			_typeFilters[i].SetIsOnWithoutNotify(p_state);
		}
	}

	private void OnToggleFilter(bool isOn, LOG_TAG tag)
	{
		if (isOn)
		{
			enabledFilters.Add(tag);
		}
		else
		{
			enabledFilters.Remove(tag);
		}
		showAllTypesToggle.SetIsOnWithoutNotify(AreAllTypeFiltersOn());
	}

	private bool AreAllTypeFiltersOn()
	{
		for (int i = 0; i < typeFilters.Length; i++)
		{
			if (!typeFilters[i].isOn)
			{
				return false;
			}
		}
		return true;
	}

	public void ToggleFilters()
	{
		_filterGO.gameObject.SetActive(!_filterGO.activeInHierarchy);
	}

	public void DisableVillagesTab()
	{
		_tglTabVillageFilters.isOn = false;
		_tglTabVillageFilters.interactable = false;
	}

	public void EnableVillagesTab()
	{
		_tglTabVillageFilters.interactable = true;
	}

	private void OnToggleVillagesTab(bool p_isOn)
	{
		_villageFiltersContainer.SetActive(p_isOn);
	}

	public void AddOnToggleVillageFilterAction(Action<bool, NPCSettlement> p_action)
	{
		_onToggleSettlementFilterAction = (Action<bool, NPCSettlement>)Delegate.Combine(_onToggleSettlementFilterAction, p_action);
	}

	private void CreateInitialVillageFilters()
	{
		for (int i = 0; i < DatabaseManager.Instance.settlementDatabase.allNonPlayerSettlements.Count; i++)
		{
			NPCSettlement nPCSettlement = DatabaseManager.Instance.settlementDatabase.allNonPlayerSettlements[i];
			if (nPCSettlement.locationType == LOCATION_TYPE.VILLAGE)
			{
				CreateNewVillageFilter(nPCSettlement);
			}
		}
	}

	private void CreateNewVillageFilter(NPCSettlement p_settlement)
	{
		LogVillageFilterItem component = ObjectPoolManager.Instance.InstantiateObjectFromPool(_villageFilterPrefab.name, Vector3.zero, Quaternion.identity, _villageFiltersScrollRect.content).GetComponent<LogVillageFilterItem>();
		component.Initialize(p_settlement);
		component.SetOnToggleAction(OnToggleVillageFilter);
		_villageFilters.Add(component);
		component.SetIsOn(state: true);
	}

	private void OnSettlementCreated(BaseSettlement p_settlement)
	{
		if (p_settlement is NPCSettlement { locationType: LOCATION_TYPE.VILLAGE } nPCSettlement)
		{
			CreateNewVillageFilter(nPCSettlement);
		}
	}

	private bool AreAllVillageFiltersOn()
	{
		for (int i = 0; i < _villageFilters.Count; i++)
		{
			if (!_villageFilters[i].isOn)
			{
				return false;
			}
		}
		return true;
	}

	private void OnToggleVillageFilter(bool p_isOn, NPCSettlement p_settlement)
	{
		if (p_isOn)
		{
			if (!enabledVillageFilters.Contains(p_settlement.persistentID))
			{
				enabledVillageFilters.Add(p_settlement.persistentID);
			}
		}
		else
		{
			enabledVillageFilters.Remove(p_settlement.persistentID);
		}
		_showAllVillagesToggle.SetIsOnWithoutNotify(AreAllVillageFiltersOn());
		_onToggleSettlementFilterAction?.Invoke(p_isOn, p_settlement);
	}

	private void OnToggleShowAllVillages(bool state)
	{
		enabledVillageFilters.Clear();
		for (int i = 0; i < _villageFilters.Count; i++)
		{
			LogVillageFilterItem logVillageFilterItem = _villageFilters[i];
			logVillageFilterItem.SetIsOnWithoutNotify(state);
			if (state && !enabledVillageFilters.Contains(logVillageFilterItem.settlementPersistentID))
			{
				enabledVillageFilters.Add(logVillageFilterItem.settlementPersistentID);
			}
		}
	}

	public void AddOnToggleActionOfShowAllVillagesToggle(UnityAction<bool> onToggleAction)
	{
		_showAllVillagesToggle.onValueChanged.AddListener(onToggleAction);
	}

	public bool HasUntoggledVillageFilter()
	{
		for (int i = 0; i < _villageFilters.Count; i++)
		{
			if (!_villageFilters[i].isOn)
			{
				return true;
			}
		}
		return false;
	}
}

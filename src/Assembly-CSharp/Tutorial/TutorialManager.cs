using System;
using System.Collections.Generic;
using System.Linq;
using Quests.Alerts;
using UnityEngine;
using UtilityScripts;

namespace Tutorial;

public class TutorialManager : BaseMonoBehaviour
{
	public enum Tutorial_Type
	{
		Unlocking_Bonus_Powers,
		Upgrading_The_Portal,
		Mana,
		Chaotic_Energy,
		Storing_Targets,
		Maraud,
		Intel,
		Spirit_Energy,
		Migration_Controls,
		Base_Building,
		Abilities,
		Resistances,
		Time_Management,
		Target_Menu
	}

	public static TutorialManager Instance;

	private Dictionary<Tutorial_Type, TutorialScriptableObjectData> _loadedTutorialData;

	private List<GameAlert> _spawnedAlerts;

	private List<GameAlert> _activeAlerts;

	private List<GameAlert> _genericAlertPool;

	private List<GameAlert> _buildingAlertPool;

	public List<GameAlert> genericAlertPool => _genericAlertPool;

	public List<GameAlert> buildingAlertPool => _buildingAlertPool;

	public List<GameAlert> spawnedAlerts => _spawnedAlerts;

	public List<GameAlert> activeAlerts => _activeAlerts;

	private void Awake()
	{
		Instance = this;
		_loadedTutorialData = new Dictionary<Tutorial_Type, TutorialScriptableObjectData>();
		_spawnedAlerts = new List<GameAlert>();
		_activeAlerts = new List<GameAlert>();
		_genericAlertPool = new List<GameAlert>();
		_buildingAlertPool = new List<GameAlert>();
	}

	protected override void OnDestroy()
	{
		if (_spawnedAlerts != null)
		{
			for (int i = 0; i < _spawnedAlerts.Count; i++)
			{
				_spawnedAlerts[i].CleanUp();
			}
			_spawnedAlerts.Clear();
		}
		base.OnDestroy();
		_spawnedAlerts = null;
		Instance = null;
		_loadedTutorialData = null;
	}

	public void Initialize()
	{
		InstantiatePendingTutorialGameAlerts();
		StartGenericAlertPoolLoop();
		StartBuildingAlertPoolLoop();
		Messenger.AddListener(SettingsSignals.ALERTS_CLEARED, OnAlertsCleared);
	}

	public TutorialScriptableObjectData GetTutorialData(Tutorial_Type p_type)
	{
		if (_loadedTutorialData.ContainsKey(p_type))
		{
			return _loadedTutorialData[p_type];
		}
		TutorialScriptableObjectData tutorialScriptableObjectData = Resources.Load<TutorialScriptableObjectData>("Tutorial Data/" + p_type.ToStringEnum());
		_loadedTutorialData.Add(p_type, tutorialScriptableObjectData);
		return tutorialScriptableObjectData;
	}

	public void UnloadTutorialAssets()
	{
		foreach (KeyValuePair<Tutorial_Type, TutorialScriptableObjectData> loadedTutorialDatum in _loadedTutorialData)
		{
			for (int i = 0; i < loadedTutorialDatum.Value.pages.Count; i++)
			{
				_ = loadedTutorialDatum.Value.pages[i];
				Resources.UnloadAsset(loadedTutorialDatum.Value);
			}
		}
	}

	public void UnlockTutorial(Tutorial_Type p_type)
	{
		SaveManager.Instance.currentSaveDataPlayer.UnlockTutorial(p_type);
	}

	private void InstantiatePendingTutorialGameAlerts()
	{
		Game_Alert[] enumValues = CollectionUtilities.GetEnumValues<Game_Alert>();
		foreach (Game_Alert game_Alert in enumValues)
		{
			if ((!SaveManager.Instance.currentSaveDataPlayer.IsTutorialAlertDone(game_Alert) || game_Alert == Game_Alert.Upgrade_Portal) && game_Alert != Game_Alert.Spawn_Defensive_Units && !IsAlertSpawned(game_Alert) && game_Alert.IsTutorialTypeAlert())
			{
				CreateGameAlert<GameAlert>(game_Alert);
			}
		}
	}

	public T CreateGameAlert<T>(Game_Alert p_alert) where T : GameAlert
	{
		string text = "Quests.Alerts." + p_alert.ToStringEnumNoSpace() + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
		Type type = Type.GetType(text);
		if (type != null)
		{
			T val = Activator.CreateInstance(type) as T;
			SetAlertAsSpawned(val);
			return val;
		}
		throw new Exception(text + " is not a valid alert type!");
	}

	public T LoadGameAlert<T>(SaveDataGameAlert p_alert) where T : GameAlert
	{
		string text = "Quests.Alerts." + p_alert.alertType.ToStringEnumNoSpace() + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
		Type type = Type.GetType(text);
		if (type != null)
		{
			T val = Activator.CreateInstance(type, p_alert) as T;
			_spawnedAlerts.Add(val);
			return val;
		}
		throw new Exception(text + " is not a valid alert type!");
	}

	private void OnAlertsCleared()
	{
		InstantiatePendingTutorialGameAlerts();
	}

	private bool IsAlertSpawned(Game_Alert p_alert)
	{
		for (int i = 0; i < _spawnedAlerts.Count; i++)
		{
			if (_spawnedAlerts[i].alertType == p_alert)
			{
				return true;
			}
		}
		return false;
	}

	private void SetAlertAsSpawned(GameAlert p_alert)
	{
		_spawnedAlerts.Add(p_alert);
		p_alert.SetAsSpawned();
	}

	public void OnAlertSetAsActive(GameAlert p_alert)
	{
		_activeAlerts.Add(p_alert);
	}

	public void RemoveAlertFromActiveList(GameAlert p_alert)
	{
		_activeAlerts.Remove(p_alert);
	}

	public void OnAlertCleared(GameAlert p_alert)
	{
		RemoveAlertFromActiveList(p_alert);
		_spawnedAlerts.Remove(p_alert);
		RemoveFromGenericAlertPool(p_alert);
		RemoveFromBuildingAlertPool(p_alert);
		p_alert.CleanUp();
	}

	public void AddAlertToGenericAlertPool(GameAlert p_alert)
	{
		if (!_genericAlertPool.Contains(p_alert))
		{
			_genericAlertPool.Add(p_alert);
		}
	}

	public void RemoveFromGenericAlertPool(GameAlert p_alert)
	{
		_genericAlertPool.Remove(p_alert);
	}

	private void StartGenericAlertPoolLoop()
	{
		ScheduleNextGenericAlertPop();
	}

	private void TryPopGenericAlert()
	{
		int num = _activeAlerts.Count((GameAlert a) => a.alertType.IsTutorialTypeAlert());
		if (_genericAlertPool.Count > 0 && num < 2)
		{
			GameAlert randomElement = CollectionUtilities.GetRandomElement(_genericAlertPool);
			randomElement.SetAsActive();
			RemoveFromGenericAlertPool(randomElement);
		}
		ScheduleNextGenericAlertPop();
	}

	private void ScheduleNextGenericAlertPop()
	{
		GameDate gameDate = GameManager.Instance.Today();
		gameDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(2));
		SchedulingManager.Instance.AddEntry(gameDate, TryPopGenericAlert, this);
	}

	public void AddAlertToBuildingAlertPool(GameAlert p_alert)
	{
		if (!_buildingAlertPool.Contains(p_alert))
		{
			_buildingAlertPool.Add(p_alert);
		}
	}

	public void RemoveFromBuildingAlertPool(GameAlert p_alert)
	{
		_buildingAlertPool.Remove(p_alert);
	}

	private void StartBuildingAlertPoolLoop()
	{
		GameDate gameDate = GameManager.Instance.Today();
		gameDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(3));
		SchedulingManager.Instance.AddEntry(gameDate, TryPopBuildingAlert, this);
	}

	private void TryPopBuildingAlert()
	{
		int num = _activeAlerts.Count((GameAlert a) => a.alertType.IsTutorialTypeAlert());
		if (_buildingAlertPool.Count > 0 && num < 2)
		{
			GameAlert randomElement = CollectionUtilities.GetRandomElement(_buildingAlertPool);
			randomElement.SetAsActive();
			RemoveFromBuildingAlertPool(randomElement);
		}
		ScheduleNextBuildingAlertPop();
	}

	private void ScheduleNextBuildingAlertPop()
	{
		GameDate gameDate = GameManager.Instance.Today();
		gameDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(2));
		SchedulingManager.Instance.AddEntry(gameDate, TryPopBuildingAlert, this);
	}
}

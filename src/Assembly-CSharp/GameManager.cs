using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Inner_Maps;
using Maccima_Games.Util;
using Object_Pools;
using Ruinarch;
using UnityEngine;
using UnityEngine.Serialization;
using UtilityScripts;

public class GameManager : BaseMonoBehaviour
{
	public static GameManager Instance;

	public static string[] daysInWords = new string[7] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

	[FormerlySerializedAs("month")]
	public int startMonth;

	[FormerlySerializedAs("days")]
	public int startDay;

	[FormerlySerializedAs("year")]
	public int startYear;

	[FormerlySerializedAs("tick")]
	public int startTick;

	public int continuousDays;

	public const int daysPerMonth = 30;

	public const int ticksPerDay = 480;

	public const int ticksPerHour = 20;

	private const int minutesPerTick = 3;

	public PROGRESSION_SPEED currProgressionSpeed;

	public float progressionSpeed;

	public bool showFullDebug;

	public static bool showAllTilesTooltip = false;

	[Header("Particle Effects")]
	[SerializeField]
	private GameObject aoeParticlesPrefab;

	[SerializeField]
	private GameObject aoeParticlesAutoDestroyPrefab;

	[SerializeField]
	private ParticleEffectAssetDictionary particleEffectsDictionary;

	[SerializeField]
	private ElementalColorDictionary elementalColorDictionary;

	private const float X1_SPEED = 0.8f;

	private const float X2_SPEED = 0.55f;

	private const float X4_SPEED = 0.3f;

	private float timeElapsed;

	private bool _gameHasStarted;

	public string lastProgressionBeforePausing;

	private static GameDate today;

	private List<BaseParticleEffect> _activeEffects;

	public static Stopwatch stopwatch;

	public bool isPaused { get; private set; }

	public bool gameHasStarted => _gameHasStarted;

	public int currentTick => today.tick;

	private void Awake()
	{
		Instance = this;
		timeElapsed = 0f;
		_gameHasStarted = false;
		InputManager.Instance.SetCursorTo(Cursor_Type.Default);
		_activeEffects = new List<BaseParticleEffect>();
		stopwatch = new Stopwatch();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_activeEffects?.Clear();
		_activeEffects = null;
		Utilities.ResetUsedIDs();
	}

	private void OnReceivePlayerInputAction(SHORTCUT_ACTION p_action)
	{
		switch (p_action)
		{
		case SHORTCUT_ACTION.Toggle_Pause:
			if (!UIManager.Instance.IsConsoleShowing() && UIManager.Instance.pauseBtn.IsInteractable() && !InputManager.Instance.HasSelectedUIObject())
			{
				if (isPaused)
				{
					UIManager.Instance.Unpause();
				}
				else
				{
					UIManager.Instance.PauseByPlayer();
				}
			}
			break;
		case SHORTCUT_ACTION.Decrease_Speed:
			if (UIManager.Instance.IsConsoleShowing() || InputManager.Instance.HasSelectedUIObject())
			{
				break;
			}
			if (!isPaused)
			{
				switch (currProgressionSpeed)
				{
				case PROGRESSION_SPEED.X1:
					if (UIManager.Instance.pauseBtn.IsInteractable())
					{
						UIManager.Instance.PauseByPlayer();
					}
					break;
				case PROGRESSION_SPEED.X2:
					UIManager.Instance.SetProgressionSpeed1X();
					break;
				case PROGRESSION_SPEED.X4:
					UIManager.Instance.SetProgressionSpeed2X();
					break;
				}
			}
			else
			{
				UIManager.Instance.SetProgressionSpeed4X();
			}
			break;
		case SHORTCUT_ACTION.Increase_Speed:
			if (isPaused)
			{
				UIManager.Instance.SetProgressionSpeed1X();
				break;
			}
			switch (currProgressionSpeed)
			{
			case PROGRESSION_SPEED.X1:
				UIManager.Instance.SetProgressionSpeed2X();
				break;
			case PROGRESSION_SPEED.X2:
				UIManager.Instance.SetProgressionSpeed4X();
				break;
			case PROGRESSION_SPEED.X4:
				if (UIManager.Instance.pauseBtn.IsInteractable())
				{
					UIManager.Instance.PauseByPlayer();
				}
				break;
			}
			break;
		}
	}

	private void Update()
	{
		if (_gameHasStarted && !isPaused)
		{
			if (Math.Abs(timeElapsed) <= 0f)
			{
				TickStarted();
				CharacterTickManager.Instance.TickStarted();
			}
			timeElapsed += Time.deltaTime;
			if (timeElapsed >= progressionSpeed)
			{
				timeElapsed = 0f;
				TickEnded();
				CharacterTickManager.Instance.TickEnded();
			}
		}
	}

	public void Initialize()
	{
		today = new GameDate(startMonth, startDay, startYear, startTick);
		BaseParticleEffect.particleEffectActivated = AddActiveEffect;
		BaseParticleEffect.particleEffectDeactivated = RemoveActiveEffect;
	}

	public void StartProgression()
	{
		WorldConfigManager.Instance.mapGenerationData?.CleanUpAfterMapGeneration();
		_gameHasStarted = true;
		UIManager.Instance.Pause();
		lastProgressionBeforePausing = "paused";
		Messenger.Broadcast(Signals.GAME_STARTED);
		SchedulingManager.Instance.StartScheduleCalls();
		Messenger.Broadcast(Signals.DAY_STARTED);
		Messenger.Broadcast(Signals.MONTH_START);
		Messenger.AddListener<SHORTCUT_ACTION>(ControlsSignals.PLAYER_INPUT_ACTION, OnReceivePlayerInputAction);
		if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Custom && !SaveManager.Instance.useSaveData)
		{
			string localizedValue;
			if (WorldSettings.Instance.worldSettingsData.victoryCondition == VICTORY_CONDITION.Eradication)
			{
				Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
				dictionary.Add("villagerCount", DatabaseManager.Instance.characterDatabase.aliveVillagersList.Count((Character c) => !c.traitContainer.HasTrait("Demon Cultist")).ToString());
				localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Game_Intro_Eradication", dictionary);
				MaccimaDictionaryPool<string, string>.Release(dictionary);
			}
			else
			{
				localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Game_Intro_" + WorldSettings.Instance.worldSettingsData.victoryCondition);
			}
			UIManager.Instance.ShowStartScenario(localizedValue);
		}
		Canvas.ForceUpdateCanvases();
		CharacterTickManager.Instance.StartUp();
	}

	public void LoadProgression()
	{
		_gameHasStarted = true;
		UIManager.Instance.Pause();
		lastProgressionBeforePausing = "paused";
		SchedulingManager.Instance.StartScheduleCalls();
		Messenger.AddListener<SHORTCUT_ACTION>(ControlsSignals.PLAYER_INPUT_ACTION, OnReceivePlayerInputAction);
		Canvas.ForceUpdateCanvases();
		Messenger.Broadcast(Signals.PROGRESSION_LOADED);
	}

	public GameDate Today()
	{
		return new GameDate(today.month, today.day, today.year, today.tick);
	}

	public void SetToday(GameDate date)
	{
		today = date;
	}

	public string TodayLogString()
	{
		return "[" + continuousDays + " - " + ConvertTickToTime(today.tick) + "] ";
	}

	public void SetPausedState(bool isPaused)
	{
		if (isPaused)
		{
			StoreLastProgressionBeforePausing();
		}
		if (this.isPaused != isPaused)
		{
			this.isPaused = isPaused;
			UpdateActiveEffectsOnPauseChanged(isPaused);
			Messenger.Broadcast(UISignals.PAUSED, isPaused);
		}
	}

	private void StoreLastProgressionBeforePausing()
	{
		if (isPaused)
		{
			lastProgressionBeforePausing = "paused";
			return;
		}
		switch (currProgressionSpeed)
		{
		case PROGRESSION_SPEED.X1:
			lastProgressionBeforePausing = "1";
			break;
		case PROGRESSION_SPEED.X2:
			lastProgressionBeforePausing = "2";
			break;
		case PROGRESSION_SPEED.X4:
			lastProgressionBeforePausing = "4";
			break;
		}
	}

	public void SetProgressionSpeed(PROGRESSION_SPEED progressionSpeed)
	{
		currProgressionSpeed = progressionSpeed;
		float num = 0.8f;
		switch (progressionSpeed)
		{
		case PROGRESSION_SPEED.X2:
			num = 0.55f;
			break;
		case PROGRESSION_SPEED.X4:
			num = 0.3f;
			break;
		}
		this.progressionSpeed = num;
		Messenger.Broadcast(UISignals.PROGRESSION_SPEED_CHANGED, progressionSpeed);
	}

	public float GetTickSpeed(PROGRESSION_SPEED progressionSpeed)
	{
		return progressionSpeed switch
		{
			PROGRESSION_SPEED.X1 => 0.8f, 
			PROGRESSION_SPEED.X2 => 0.55f, 
			PROGRESSION_SPEED.X4 => 0.3f, 
			_ => throw new Exception("Could not get tick speed from " + currProgressionSpeed), 
		};
	}

	private void TickStarted()
	{
		if (today.tick % 20 == 0 && !IsStartOfGame())
		{
			Messenger.Broadcast(Signals.HOUR_STARTED);
		}
		Messenger.Broadcast(Signals.TICK_STARTED);
		Messenger.Broadcast(UISignals.UPDATE_UI);
	}

	private void TickEnded()
	{
		Messenger.Broadcast(Signals.CHECK_SCHEDULES);
		Messenger.Broadcast(Signals.TICK_ENDED);
		Messenger.Broadcast(Signals.AFTER_TICK_ENDED);
		today.tick++;
		if (today.tick > 480)
		{
			today.tick = 1;
			DayStarted(broadcastUI: false);
		}
		Messenger.Broadcast(UISignals.UPDATE_UI);
	}

	private void DayStarted(bool broadcastUI = true)
	{
		today.day++;
		continuousDays++;
		Messenger.Broadcast(Signals.DAY_STARTED);
		if (today.day > 30)
		{
			today.day = 1;
			today.month++;
			if (today.month > 12)
			{
				today.month = 1;
				today.year++;
			}
			Messenger.Broadcast(Signals.MONTH_START);
		}
		if (broadcastUI)
		{
			Messenger.Broadcast(UISignals.UPDATE_UI);
		}
		SaveManager.Instance.saveCurrentProgressManager.Autosave();
	}

	public string ConvertTickToTime(int tick, string timeSeparator = ":")
	{
		float num = (float)tick / 20f;
		int num2 = (int)num;
		int num3 = Mathf.RoundToInt((num - (float)num2) * 20f * 3f);
		string text = LocalizationManager.Time_AM;
		if (num2 >= 12)
		{
			if (num2 < 24)
			{
				text = LocalizationManager.Time_PM;
			}
			num2 -= 12;
		}
		if (num2 == 0)
		{
			num2 = 12;
		}
		return $"{num2}{timeSeparator}{num3:D2} {text}";
	}

	public TIME_IN_WORDS GetTimeInWordsOfTick(int tick)
	{
		float hoursBasedOnTicksInFloat = GetHoursBasedOnTicksInFloat(tick);
		if ((hoursBasedOnTicksInFloat > 22f && hoursBasedOnTicksInFloat <= 24f) || (hoursBasedOnTicksInFloat >= 0f && hoursBasedOnTicksInFloat <= 5f))
		{
			return TIME_IN_WORDS.AFTER_MIDNIGHT;
		}
		if (hoursBasedOnTicksInFloat > 5f && hoursBasedOnTicksInFloat <= 11f)
		{
			return TIME_IN_WORDS.MORNING;
		}
		if (hoursBasedOnTicksInFloat > 11f && hoursBasedOnTicksInFloat <= 13f)
		{
			return TIME_IN_WORDS.LUNCH_TIME;
		}
		if (hoursBasedOnTicksInFloat > 13f && hoursBasedOnTicksInFloat <= 17f)
		{
			return TIME_IN_WORDS.AFTERNOON;
		}
		if (hoursBasedOnTicksInFloat > 17f && hoursBasedOnTicksInFloat <= 20f)
		{
			return TIME_IN_WORDS.EARLY_NIGHT;
		}
		if (hoursBasedOnTicksInFloat > 20f && hoursBasedOnTicksInFloat <= 22f)
		{
			return TIME_IN_WORDS.LATE_NIGHT;
		}
		return TIME_IN_WORDS.NONE;
	}

	public TIME_IN_WORDS GetCurrentTimeInWordsOfTick(Character relativeTo = null)
	{
		int tick = today.tick;
		TIME_IN_WORDS tIME_IN_WORDS = GetTimeInWordsOfTick(tick);
		if (relativeTo != null && relativeTo.traitContainer.HasTrait("Nocturnal"))
		{
			tIME_IN_WORDS = ConvertTimeInWordsWhenNocturnal(tIME_IN_WORDS);
		}
		return tIME_IN_WORDS;
	}

	public TIME_IN_WORDS ConvertTimeInWordsWhenNocturnal(TIME_IN_WORDS currentTimeInWords)
	{
		return currentTimeInWords switch
		{
			TIME_IN_WORDS.MORNING => TIME_IN_WORDS.LATE_NIGHT, 
			TIME_IN_WORDS.LUNCH_TIME => TIME_IN_WORDS.AFTER_MIDNIGHT, 
			TIME_IN_WORDS.AFTERNOON => TIME_IN_WORDS.AFTER_MIDNIGHT, 
			TIME_IN_WORDS.EARLY_NIGHT => TIME_IN_WORDS.MORNING, 
			TIME_IN_WORDS.LATE_NIGHT => TIME_IN_WORDS.MORNING, 
			TIME_IN_WORDS.AFTER_MIDNIGHT => TIME_IN_WORDS.AFTERNOON, 
			_ => TIME_IN_WORDS.NONE, 
		};
	}

	public int GetTicksBasedOnHour(int hours)
	{
		return 20 * hours;
	}

	public int GetTicksBasedOnMinutes(int minutes)
	{
		float num = (float)minutes / 60f;
		return Mathf.FloorToInt(20f * num);
	}

	public int GetHoursBasedOnTicks(int ticks)
	{
		return ticks / 20;
	}

	private float GetHoursBasedOnTicksInFloat(int ticks)
	{
		return (float)ticks / 20f;
	}

	public int GetMinutesBasedOnTicks(int ticks)
	{
		return ticks * 3;
	}

	public int GetCeilingHoursBasedOnTicks(int ticks)
	{
		return Mathf.CeilToInt((float)ticks / 20f);
	}

	public static string ConvertTicksToWholeTime(int ticks)
	{
		string empty = string.Empty;
		List<string> list = RuinarchListPool<string>.Claim();
		int num = ticks;
		if (num >= 480)
		{
			int num2 = Mathf.FloorToInt((float)num / 480f);
			string item = ((num2 == 1) ? (num2 + " " + LocalizationManager.Day) : (num2 + " " + LocalizationManager.Days));
			list.Add(item);
			num -= num2 * 480;
		}
		if (num >= 20)
		{
			int num3 = Mathf.FloorToInt((float)num / 20f);
			string item2 = ((num3 == 1) ? (num3 + " " + LocalizationManager.Hour) : (num3 + " " + LocalizationManager.Hours));
			list.Add(item2);
			num -= num3 * 20;
		}
		if (num > 0)
		{
			int num4 = num * 3;
			string item3 = ((num4 == 1) ? (num4 + " " + LocalizationManager.Minute) : (num4 + " " + LocalizationManager.Minutes));
			list.Add(item3);
		}
		empty = list.ComafyList();
		RuinarchListPool<string>.Release(list);
		return empty;
	}

	public static int GetTimeAsWholeDuration(int ticks)
	{
		if (ticks >= 480)
		{
			return Mathf.CeilToInt((float)ticks / 480f);
		}
		if (ticks >= 20)
		{
			return Mathf.CeilToInt((float)ticks / 20f);
		}
		return ticks * 3;
	}

	public static string GetTimeIdentifierAsWholeDuration(int ticks)
	{
		if (ticks > 480)
		{
			return LocalizationManager.Days;
		}
		if (ticks == 480)
		{
			return LocalizationManager.Day;
		}
		if (ticks > 20)
		{
			return LocalizationManager.Hours;
		}
		if (ticks == 20)
		{
			return LocalizationManager.Hour;
		}
		return LocalizationManager.Minutes;
	}

	public GameObject CreateParticleEffectAt(Vector3 worldLocation, InnerTileMap innerTileMap, PARTICLE_EFFECT particle, int sortingOrder = -1)
	{
		GameObject gameObject = null;
		if (particleEffectsDictionary.ContainsKey(particle))
		{
			gameObject = particleEffectsDictionary[particle];
			GameObject obj = ObjectPoolManager.Instance.InstantiateObjectFromPool(gameObject.name, Vector3.zero, Quaternion.identity, innerTileMap.objectsParent);
			obj.transform.position = worldLocation;
			obj.SetActive(value: true);
			BaseParticleEffect component = obj.GetComponent<BaseParticleEffect>();
			if ((bool)component)
			{
				if (sortingOrder != -1)
				{
					component.SetSortingOrder(sortingOrder);
				}
				component.PlayParticleEffect();
			}
			return obj;
		}
		return null;
	}

	public GameObject CreateElementalExplosionEffectAt(Vector3 worldLocation, InnerTileMap innerTileMap, ELEMENTAL_TYPE p_element)
	{
		GameObject gameObject = null;
		if (particleEffectsDictionary.ContainsKey(PARTICLE_EFFECT.Elemental_Explosion))
		{
			gameObject = particleEffectsDictionary[PARTICLE_EFFECT.Elemental_Explosion];
			GameObject obj = ObjectPoolManager.Instance.InstantiateObjectFromPool(gameObject.name, Vector3.zero, Quaternion.identity, innerTileMap.objectsParent);
			obj.transform.position = worldLocation;
			obj.SetActive(value: true);
			ElementalExplosionParticle component = obj.GetComponent<ElementalExplosionParticle>();
			if ((bool)component)
			{
				component.SetColor(elementalColorDictionary[p_element]);
			}
			return obj;
		}
		return null;
	}

	public GameObject CreateParticleEffectAt(LocationGridTile tile, PARTICLE_EFFECT particle, int sortingOrder = -1)
	{
		GameObject gameObject = null;
		if (particleEffectsDictionary.ContainsKey(particle))
		{
			gameObject = particleEffectsDictionary[particle];
			GameObject obj = ObjectPoolManager.Instance.InstantiateObjectFromPool(gameObject.name, Vector3.zero, Quaternion.identity, tile.parentMap.objectsParent);
			obj.transform.localPosition = tile.centeredLocalLocation;
			obj.SetActive(value: true);
			BaseParticleEffect component = obj.GetComponent<BaseParticleEffect>();
			if ((bool)component)
			{
				if (sortingOrder != -1)
				{
					component.SetSortingOrder(sortingOrder);
				}
				component.SetTargetTile(tile);
				component.PlayParticleEffect();
			}
			return obj;
		}
		return null;
	}

	public GameObject CreateParticleEffectAt(ThinWall wallObject, PARTICLE_EFFECT particle)
	{
		GameObject gameObject = null;
		if (particleEffectsDictionary.ContainsKey(particle))
		{
			gameObject = particleEffectsDictionary[particle];
			GameObject obj = ObjectPoolManager.Instance.InstantiateObjectFromPool(gameObject.name, Vector3.zero, Quaternion.identity, wallObject.mapVisual.particleEffectParent.transform);
			obj.transform.localPosition = Vector3.zero;
			obj.SetActive(value: true);
			return obj;
		}
		return null;
	}

	public GameObject CreateParticleEffectAt(IPointOfInterest poi, PARTICLE_EFFECT particle, bool allowRotation = true)
	{
		GameObject gameObject = null;
		GameObject gameObject2 = null;
		if (particleEffectsDictionary.ContainsKey(particle))
		{
			gameObject = particleEffectsDictionary[particle];
			if (poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER)
			{
				Character character = poi as Character;
				if (!character.hasMarker)
				{
					return null;
				}
				Transform parent = character.marker.particleEffectParentAllowRotation;
				if (!allowRotation)
				{
					parent = character.marker.particleEffectParent;
				}
				gameObject2 = ObjectPoolManager.Instance.InstantiateObjectFromPool(gameObject.name, Vector3.zero, Quaternion.identity, parent);
				gameObject2.transform.localPosition = Vector3.zero;
				gameObject2.SetActive(value: true);
			}
			else
			{
				if ((bool)poi.mapObjectVisual)
				{
					gameObject2 = ObjectPoolManager.Instance.InstantiateObjectFromPool(gameObject.name, Vector3.zero, Quaternion.identity, poi.mapObjectVisual.particleEffectParent.transform);
					gameObject2.transform.localPosition = Vector3.zero;
				}
				else
				{
					if (poi.gridTileLocation == null)
					{
						return null;
					}
					gameObject2 = ObjectPoolManager.Instance.InstantiateObjectFromPool(gameObject.name, Vector3.zero, Quaternion.identity, poi.gridTileLocation.parentMap.objectsParent);
					gameObject2.transform.localPosition = poi.gridTileLocation.centeredLocalLocation;
				}
				gameObject2.SetActive(value: true);
			}
			return gameObject2;
		}
		return null;
	}

	public GameObject CreateParticleEffectAtWithScale(LocationGridTile tile, PARTICLE_EFFECT particle, float p_scaleFactor = 1f, int sortingOrder = -1)
	{
		GameObject gameObject = null;
		if (particleEffectsDictionary.ContainsKey(particle))
		{
			gameObject = particleEffectsDictionary[particle];
			GameObject obj = ObjectPoolManager.Instance.InstantiateObjectFromPool(gameObject.name, Vector3.zero, Quaternion.identity, tile.parentMap.objectsParent);
			obj.transform.localPosition = tile.centeredLocalLocation;
			Transform child = obj.transform.GetChild(0);
			child.localScale = new Vector3(child.localScale.x * p_scaleFactor, child.localScale.y * p_scaleFactor, child.localScale.z);
			obj.SetActive(value: true);
			BaseParticleEffect component = obj.GetComponent<BaseParticleEffect>();
			if ((bool)component)
			{
				if (sortingOrder != -1)
				{
					component.SetSortingOrder(sortingOrder);
				}
				component.SetTargetTile(tile);
				component.PlayParticleEffect();
			}
			return obj;
		}
		return null;
	}

	private void AddActiveEffect(BaseParticleEffect p_effect)
	{
		_activeEffects?.Add(p_effect);
	}

	private void RemoveActiveEffect(BaseParticleEffect p_effect)
	{
		_activeEffects?.Remove(p_effect);
	}

	private void UpdateActiveEffectsOnPauseChanged(bool state)
	{
		for (int i = 0; i < _activeEffects.Count; i++)
		{
			BaseParticleEffect baseParticleEffect = _activeEffects[i];
			if (baseParticleEffect.pauseOnGamePaused)
			{
				baseParticleEffect.OnGamePaused(state);
			}
		}
	}

	[ContextMenu("Print Event Table")]
	public void PrintEventTable()
	{
		Messenger.PrintEventTable();
	}

	private bool IsStartOfGame()
	{
		if (today.year == startYear && today.month == startMonth && today.day == startDay && today.tick == startTick)
		{
			return true;
		}
		return false;
	}

	public static Log CreateNewLog()
	{
		return LogPool.Claim();
	}

	public static Log CreateNewLogUsingNewLocalization(GameDate date, string category, string table, string key)
	{
		Log log = CreateNewLog();
		log.SetPersistentID(Utilities.GetNewUniqueID());
		log.SetDate(date);
		log.SetCategory(category);
		log.SetFile(table);
		log.SetKey(key);
		log.DetermineInitialLogText();
		return log;
	}

	public static Log CreateNewLogUsingNewLocalization(GameDate date, string category, string table, string key, LOG_TAG providedTags = LOG_TAG.Work, ActualGoapNode node = null)
	{
		Log log = CreateNewLogUsingNewLocalization(date, category, table, key);
		log.SetConnectedAction(node);
		log.AddTag(providedTags);
		return log;
	}

	public static Log CreateNewLogUsingNewLocalization(GameDate date, string category, string table, string key, LOG_TAG providedTag1, LOG_TAG providedTag2)
	{
		Log log = CreateNewLogUsingNewLocalization(date, category, table, key);
		log.AddTag(providedTag1);
		log.AddTag(providedTag2);
		return log;
	}

	public static Log CreateNewLogUsingNewLocalization(GameDate date, string category, string table, string key, LOG_TAG providedTag1, LOG_TAG providedTag2, LOG_TAG providedTag3)
	{
		Log log = CreateNewLogUsingNewLocalization(date, category, table, key);
		log.AddTag(providedTag1);
		log.AddTag(providedTag2);
		log.AddTag(providedTag3);
		return log;
	}

	public static Log CreateNewLogUsingNewLocalization(GameDate date, string category, string table, string key, LOG_TAG[] providedTags)
	{
		Log log = CreateNewLogUsingNewLocalization(date, category, table, key);
		if (providedTags != null)
		{
			for (int i = 0; i < providedTags.Length; i++)
			{
				log.AddTag(providedTags[i]);
			}
		}
		return log;
	}

	public static Log CreateNewLogUsingNewLocalization(GameDate date, string category, string table, string key, LOG_TAG[] providedTags, ActualGoapNode node)
	{
		Log log = CreateNewLogUsingNewLocalization(date, category, table, key);
		if (providedTags != null)
		{
			for (int i = 0; i < providedTags.Length; i++)
			{
				log.AddTag(providedTags[i]);
			}
		}
		log.SetConnectedAction(node);
		return log;
	}

	public static Log CreateNewLogUsingNewLocalization(GameDate date, string category, string table, string key, List<LOG_TAG> providedTags)
	{
		Log log = CreateNewLogUsingNewLocalization(date, category, table, key);
		if (providedTags != null)
		{
			for (int i = 0; i < providedTags.Count; i++)
			{
				log.AddTag(providedTags[i]);
			}
		}
		return log;
	}

	public static Log CreateNewLogUsingNewLocalization(GameDate date, string category, string table, string key, LOG_TAG providedTag1, LOG_TAG providedTag2, LOG_TAG providedTag3, ActualGoapNode node)
	{
		Log log = CreateNewLogUsingNewLocalization(date, category, table, key);
		log.AddTag(providedTag1);
		log.AddTag(providedTag2);
		log.AddTag(providedTag3);
		log.SetConnectedAction(node);
		return log;
	}

	public static Log CreateNewLog(GameDate date, string category, string file, string key, ActualGoapNode node = null, LOG_TAG providedTags = LOG_TAG.Work)
	{
		Log log = CreateNewLog();
		log.SetPersistentID(Utilities.GetNewUniqueID());
		log.SetDate(date);
		log.SetCategory(category);
		log.SetFile(file);
		log.SetKey(key);
		log.SetConnectedAction(node);
		log.AddTag(providedTags);
		log.DetermineInitialLogText();
		return log;
	}

	public static Log CreateNewLog(string id, GameDate date, string logText, string category, string key, string file, string involvedObjects, List<LOG_TAG> providedTags, string rawText)
	{
		Log log = CreateNewLog();
		log.SetPersistentID(id);
		log.SetDate(date);
		log.SetLogText(logText);
		log.SetCategory(category);
		log.SetFile(file);
		log.SetKey(key);
		log.SetInvolvedObjects(involvedObjects);
		log.AddTag(providedTags);
		log.SetRawText(rawText);
		return log;
	}
}

using System.Collections.Generic;
using Events.World_Events;
using Inner_Maps;
using UtilityScripts;

namespace Managers;

public class WorldEventManager : BaseMonoBehaviour
{
	public static WorldEventManager Instance;

	private List<WorldEvent> _activeEvents;

	public List<WorldEvent> activeEvents => _activeEvents;

	private void Awake()
	{
		Instance = this;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Instance = null;
	}

	public void Initialize()
	{
		_activeEvents = new List<WorldEvent>();
		Messenger.AddListener(Signals.GAME_LOADED, OnGameLoaded);
	}

	private void OnGameLoaded()
	{
		Messenger.RemoveListener(Signals.GAME_LOADED, OnGameLoaded);
		ActivateEvents();
		InitialScheduleWispSpawning();
	}

	public void AddActiveEvent(WorldEvent worldEvent)
	{
		_activeEvents.Add(worldEvent);
	}

	private void ActivateEvents()
	{
		for (int i = 0; i < _activeEvents.Count; i++)
		{
			_activeEvents[i].InitializeEvent();
		}
	}

	public void InitializeAfterLoadoutPicked()
	{
		for (int i = 0; i < _activeEvents.Count; i++)
		{
			_activeEvents[i].InitializeAfterLoadoutPicked();
		}
	}

	public void LoadEvent(WorldEvent worldEvent)
	{
		_activeEvents.Add(worldEvent);
	}

	private void InitialScheduleWispSpawning()
	{
		int ticks = GameUtilities.RandomBetweenTwoNumbers(1, 481);
		GameDate gameDate = GameManager.Instance.Today().AddDays(1);
		gameDate.SetTicks(ticks);
		SchedulingManager.Instance.AddEntry(gameDate, SpawnWisps, null);
	}

	private void SpawnWisps()
	{
		string log = string.Empty;
		if (GameUtilities.RollChance(50, ref log) && CharacterManager.Instance.IsThereALivingWhispererInTheWorld() && CharacterManager.Instance.GetNumberOfAliveMonsterByType<Wisp>() < 3)
		{
			for (int i = 0; i < 3; i++)
			{
				LocationGridTile randomPassableTileThatIsNotPartOfSettlement = GridMap.Instance.mainRegion.wilderness.GetRandomPassableTileThatIsNotPartOfSettlement();
				if (randomPassableTileThatIsNotPartOfSettlement != null)
				{
					SUMMON_TYPE randomWispType = CharacterManager.Instance.GetRandomWispType();
					Summon summon = CharacterManager.Instance.CreateNewSummon(randomWispType, FactionManager.Instance.GetDefaultFactionForMonster(randomWispType), null, null, null, "", bypassIdeologyChecking: true);
					CharacterManager.Instance.PlaceSummonInitially(summon, randomPassableTileThatIsNotPartOfSettlement);
					if (!summon.HasHome())
					{
						summon.SetTerritory(randomPassableTileThatIsNotPartOfSettlement.area);
					}
				}
			}
		}
		GameDate gameDate = GameManager.Instance.Today().AddDays(1);
		SchedulingManager.Instance.AddEntry(gameDate, SpawnWisps, null);
	}
}

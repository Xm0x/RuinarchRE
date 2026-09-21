using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class MooncrawlerHole : AnimalBurrow
{
	public MooncrawlerHole()
		: base(SUMMON_TYPE.Moonwalker)
	{
		Initialize(TILE_OBJECT_TYPE.MOONCRAWLER_HOLE);
	}

	public MooncrawlerHole(SaveDataTileObject data)
		: base(data, SUMMON_TYPE.Moonwalker)
	{
	}

	protected override void SubscribeListeners(bool shouldLock)
	{
		if (!base.hasSubscribedToSignals)
		{
			base.SubscribeListeners(shouldLock);
			Messenger.AddListener(Signals.HOUR_STARTED, OnHourStarted, shouldLock);
		}
	}

	protected override void UnsubscribeListeners()
	{
		if (base.hasSubscribedToSignals)
		{
			base.hasSubscribedToSignals = false;
			base.UnsubscribeListeners();
			Messenger.RemoveListener(Signals.HOUR_STARTED, OnHourStarted);
		}
	}

	private void OnHourStarted()
	{
		if (gridTileLocation == null)
		{
			UnsubscribeListeners();
		}
		else if (GameManager.Instance.currentTick == GameManager.Instance.GetTicksBasedOnHour(24))
		{
			TrySpawnAnimalsOnMidnight();
		}
	}

	public override void LoadSecondWave(SaveDataTileObject data)
	{
		base.LoadSecondWave(data);
		if (base.isDead)
		{
			UnsubscribeListeners();
		}
	}

	private void TrySpawnAnimalsOnMidnight()
	{
		if (base.spawnedMonsters.Count > 2)
		{
			return;
		}
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		Area area = gridTileLocation.area;
		for (int i = 0; i < area.gridTileComponent.passableTiles.Count; i++)
		{
			LocationGridTile locationGridTile = area.gridTileComponent.passableTiles[i];
			if (locationGridTile.structure is Wilderness)
			{
				list.Add(locationGridTile);
			}
		}
		CreateNewMonster(list);
		RuinarchListPool<LocationGridTile>.Release(list);
	}

	protected override void OnGameLoaded()
	{
		base.OnGameLoaded();
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		Area area = gridTileLocation.area;
		for (int i = 0; i < area.gridTileComponent.passableTiles.Count; i++)
		{
			LocationGridTile locationGridTile = area.gridTileComponent.passableTiles[i];
			if (locationGridTile.structure is Wilderness)
			{
				list.Add(locationGridTile);
			}
		}
		for (int j = 0; j < 3; j++)
		{
			CreateNewMonster(list);
		}
		RuinarchListPool<LocationGridTile>.Release(list);
	}
}

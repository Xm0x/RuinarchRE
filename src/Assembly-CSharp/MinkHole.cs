using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class MinkHole : AnimalBurrow
{
	public MinkHole()
		: base(SUMMON_TYPE.Mink)
	{
		Initialize(TILE_OBJECT_TYPE.MINK_HOLE);
	}

	public MinkHole(SaveDataTileObject data)
		: base(data, SUMMON_TYPE.Mink)
	{
	}

	protected override void SubscribeListeners(bool shouldLock)
	{
		if (!base.hasSubscribedToSignals)
		{
			base.SubscribeListeners(shouldLock);
			Messenger.AddListener(Signals.DAY_STARTED, OnDayStarted, shouldLock);
		}
	}

	protected override void UnsubscribeListeners()
	{
		if (base.hasSubscribedToSignals)
		{
			base.hasSubscribedToSignals = false;
			base.UnsubscribeListeners();
			Messenger.RemoveListener(Signals.DAY_STARTED, OnDayStarted);
		}
	}

	private void OnDayStarted()
	{
		if (gridTileLocation == null)
		{
			UnsubscribeListeners();
		}
		else
		{
			TrySpawnAnimalsOnDayStarted();
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

	private void TrySpawnAnimalsOnDayStarted()
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
		for (int j = 0; j < 2; j++)
		{
			CreateNewMonster(list);
		}
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

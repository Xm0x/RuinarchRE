using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class BoarDen : AnimalBurrow
{
	private const int MaxBoars = 4;

	public BoarDen()
		: base(SUMMON_TYPE.Boar)
	{
		Initialize(TILE_OBJECT_TYPE.BOAR_DEN);
	}

	public BoarDen(SaveDataTileObject data)
		: base(data, SUMMON_TYPE.Boar)
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
		if (!GameUtilities.RollChance(35) || base.spawnedMonsters.Count > 0)
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
		for (int j = 0; j < 4; j++)
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
		for (int j = 0; j < 4; j++)
		{
			CreateNewMonster(list);
		}
		RuinarchListPool<LocationGridTile>.Release(list);
	}
}

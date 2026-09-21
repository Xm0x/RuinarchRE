using System;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;

public class PoisonVent : TileObject
{
	private readonly int _activityCycle;

	private string _currentActivitySchedule;

	public int activityCycle => _activityCycle;

	public override Type serializedData => typeof(SaveDataPoisonVent);

	public PoisonVent()
	{
		Initialize(TILE_OBJECT_TYPE.POISON_VENT);
		_activityCycle = UnityEngine.Random.Range(12, 61);
	}

	public PoisonVent(SaveDataPoisonVent data)
		: base(data)
	{
		_activityCycle = data.activityCycle;
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		ScheduleActivity();
	}

	public override void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom, bool removeTraits = true, bool destroyTileSlots = true)
	{
		base.OnRemoveTileObject(removedBy, removedFrom, removeTraits, destroyTileSlots);
		if (!string.IsNullOrEmpty(_currentActivitySchedule))
		{
			SchedulingManager.Instance.RemoveSpecificEntry(_currentActivitySchedule);
			_currentActivitySchedule = string.Empty;
		}
	}

	public override void OnDestroyPOI(Character p_destroyer = null)
	{
		base.OnDestroyPOI(p_destroyer);
		if (!string.IsNullOrEmpty(_currentActivitySchedule))
		{
			SchedulingManager.Instance.RemoveSpecificEntry(_currentActivitySchedule);
			_currentActivitySchedule = string.Empty;
		}
	}

	public override void ActivateTileObject()
	{
		ProducePoisonCloud();
	}

	private void ScheduleActivity()
	{
		GameDate gameDate = GameManager.Instance.Today().AddTicks(_activityCycle);
		_currentActivitySchedule = SchedulingManager.Instance.AddEntry(gameDate, ProduceScheduledPoisonCloud, this);
	}

	private void ProduceScheduledPoisonCloud()
	{
		ProducePoisonCloud();
		ScheduleActivity();
	}

	private void ProducePoisonCloud()
	{
		InnerMapManager.Instance.SpawnPoisonCloud(gridTileLocation, GameUtilities.RandomBetweenTwoNumbers(2, 8));
	}
}

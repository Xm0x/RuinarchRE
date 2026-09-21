using System;
using UnityEngine;

public class VaporVent : TileObject
{
	private readonly int _activityCycle;

	private string _currentActivitySchedule;

	public int activityCycle => _activityCycle;

	public override Type serializedData => typeof(SaveDataVaporVent);

	public VaporVent()
	{
		Initialize(TILE_OBJECT_TYPE.VAPOR_VENT);
		_activityCycle = UnityEngine.Random.Range(12, 61);
	}

	public VaporVent(SaveDataVaporVent data)
		: base(data)
	{
		_activityCycle = data.activityCycle;
	}

	public override void ConstructDefaultPlayerActions(bool broadcastSignal = true)
	{
		base.ConstructDefaultPlayerActions(broadcastSignal);
		RemovePlayerAction(PLAYER_SKILL_TYPE.SEIZE_OBJECT, broadcastSignal);
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		ScheduleActivity();
	}

	public override void OnDestroyPOI(Character p_destroyer = null)
	{
		base.OnDestroyPOI(p_destroyer);
		SchedulingManager.Instance.RemoveSpecificEntry(_currentActivitySchedule);
	}

	public override void ActivateTileObject()
	{
		ProduceVapor();
	}

	private void ScheduleActivity()
	{
		GameDate gameDate = GameManager.Instance.Today().AddTicks(_activityCycle);
		_currentActivitySchedule = SchedulingManager.Instance.AddEntry(gameDate, ProduceScheduledVapor, this);
	}

	private void ProduceScheduledVapor()
	{
		ProduceVapor();
		ScheduleActivity();
	}

	private void ProduceVapor()
	{
		Vapor vapor = new Vapor();
		vapor.SetGridTileLocation(gridTileLocation);
		vapor.OnPlacePOI();
		vapor.SetStacks(UnityEngine.Random.Range(2, 9));
	}
}

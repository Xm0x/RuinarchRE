using System;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;

namespace Locations.Area_Features;

public class PoisonBloomFeature : AreaFeature
{
	private string expirationKey;

	private Area owner;

	public int expiryInTicks { get; private set; }

	public GameDate expiryDate { get; private set; }

	public bool isPlayerSource { get; private set; }

	public override Type serializedData => typeof(SaveDataPoisonBloomFeature);

	public PoisonBloomFeature()
	{
		base.name = "Poison Emitting";
		base.description = "This location is naturally emitting poison clouds.";
		expiryInTicks = 20;
	}

	public override void OnAddFeature(Area p_area)
	{
		base.OnAddFeature(p_area);
		owner = p_area;
		Messenger.AddListener(Signals.TICK_ENDED, EmitPoisonCloudsPerTick);
		ScheduleExpiry();
	}

	public override void OnRemoveFeature(Area p_area)
	{
		base.OnRemoveFeature(p_area);
		Messenger.RemoveListener(Signals.TICK_ENDED, EmitPoisonCloudsPerTick);
		owner = null;
	}

	public void ResetDuration()
	{
		if (owner != null)
		{
			SchedulingManager.Instance.RemoveSpecificEntry(expirationKey);
			ScheduleExpiry();
		}
	}

	private void ScheduleExpiry()
	{
		expiryDate = GameManager.Instance.Today().AddTicks(expiryInTicks);
		expirationKey = SchedulingManager.Instance.AddEntry(expiryDate, delegate
		{
			owner.featureComponent.RemoveFeature(this, owner);
		}, this);
	}

	private void EmitPoisonCloudsPerTick()
	{
		if (UnityEngine.Random.Range(0, 100) < 60 && owner != null)
		{
			LocationGridTile randomElement = CollectionUtilities.GetRandomElement(owner.gridTileComponent.gridTiles);
			InnerMapManager.Instance.SpawnPoisonCloud(randomElement, 3).SetIsPlayerSource(isPlayerSource);
		}
	}

	public void SetIsPlayerSource(bool p_state)
	{
		isPlayerSource = p_state;
	}

	public void SetExpiryInTicks(int ticks)
	{
		expiryInTicks = ticks;
	}
}

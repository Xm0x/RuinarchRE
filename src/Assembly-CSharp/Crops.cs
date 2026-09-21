using System;
using Inner_Maps;
using UnityEngine;

public abstract class Crops : TileObject
{
	public enum Growth_State
	{
		Growing,
		Ripe
	}

	private int _remainingRipeningTicks;

	private int _growthRate;

	private bool hasStartedGrowth;

	public Growth_State currentGrowthState { get; private set; }

	public int count { get; set; }

	public int remainingRipeningTicks => _remainingRipeningTicks;

	public override Type serializedData => typeof(SaveDataCrops);

	public int growthRate => _growthRate;

	public abstract TILE_OBJECT_TYPE producedObjectOnHarvest { get; }

	public abstract bool isFarmCrop { get; }

	protected Crops()
	{
		count = 90;
	}

	protected Crops(SaveDataCrops data)
		: base(data)
	{
	}

	protected override void Initialize(TILE_OBJECT_TYPE tileObjectType, bool shouldAddCommonAdvertisements = true)
	{
		base.Initialize(tileObjectType, shouldAddCommonAdvertisements);
		SetGrowthRate(1);
		SetGrowthState(Growth_State.Growing);
	}

	public override void LoadSecondWave(SaveDataTileObject data)
	{
		base.LoadSecondWave(data);
		SaveDataCrops saveDataCrops = data as SaveDataCrops;
		currentGrowthState = saveDataCrops.growthState;
		SetRemainingRipeningTicks(saveDataCrops.remainingRipeningTicks);
		SetGrowthRate(saveDataCrops.growthRate);
	}

	public override void LoadAdditionalInfo(SaveDataTileObject data)
	{
		base.LoadAdditionalInfo(data);
		if (mapVisual != null)
		{
			mapVisual.UpdateTileObjectVisual(this);
		}
	}

	public virtual void SetGrowthState(Growth_State growthState)
	{
		currentGrowthState = growthState;
		switch (growthState)
		{
		case Growth_State.Growing:
			_remainingRipeningTicks = -1;
			StartPerTickGrowth();
			RemoveAdvertisedAction(INTERACTION_TYPE.HARVEST_PLANT);
			RemoveAdvertisedAction(INTERACTION_TYPE.HARVEST_CROPS);
			base.traitContainer.RemoveTrait(this, "Edible");
			break;
		case Growth_State.Ripe:
			_remainingRipeningTicks = 0;
			StopPerTickGrowth();
			AddAdvertisedAction(INTERACTION_TYPE.HARVEST_PLANT);
			AddAdvertisedAction(INTERACTION_TYPE.HARVEST_CROPS);
			if (!base.traitContainer.HasTrait("Edible"))
			{
				base.traitContainer.AddTrait(this, "Edible");
			}
			break;
		}
		if (mapVisual != null)
		{
			mapVisual.UpdateTileObjectVisual(this);
		}
	}

	private void StartPerTickGrowth()
	{
		if (!hasStartedGrowth)
		{
			hasStartedGrowth = true;
			Messenger.AddListener(Signals.TICK_ENDED, PerTickGrowth);
		}
	}

	private void StopPerTickGrowth()
	{
		hasStartedGrowth = false;
		Messenger.RemoveListener(Signals.TICK_ENDED, PerTickGrowth);
	}

	public abstract int GetRipeningTicks();

	private void PerTickGrowth()
	{
		if (_remainingRipeningTicks == -1)
		{
			_remainingRipeningTicks = GetRipeningTicks();
		}
		if (_remainingRipeningTicks == 0)
		{
			SetGrowthState(Growth_State.Ripe);
		}
		_remainingRipeningTicks -= growthRate;
		_remainingRipeningTicks = Mathf.Max(0, _remainingRipeningTicks);
		if (mapVisual != null)
		{
			mapVisual.UpdateTileObjectVisual(this);
		}
	}

	public void SetGrowthRate(int growthRate)
	{
		_growthRate = growthRate;
	}

	public void AdjustGrowthRate(int p_adjustment)
	{
		_growthRate += p_adjustment;
	}

	private void SetRemainingRipeningTicks(int value)
	{
		_remainingRipeningTicks = value;
	}

	protected int DefaultRipeningTicksBasedOnLocation()
	{
		switch (gridTileLocation.mainBiomeType)
		{
		case BIOMES.GRASSLAND:
		case BIOMES.FOREST:
			return 120;
		case BIOMES.SNOW:
			return 180;
		case BIOMES.DESERT:
			return 240;
		default:
			return 120;
		}
	}

	public override void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom, bool removeTraits = true, bool destroyTileSlots = true)
	{
		base.OnRemoveTileObject(removedBy, removedFrom, removeTraits, destroyTileSlots);
		StopPerTickGrowth();
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		StartPerTickGrowth();
	}

	public override void OnDestroyPOI(Character p_destroyer = null)
	{
		base.OnDestroyPOI(p_destroyer);
		StopPerTickGrowth();
	}

	public override string GetAdditionalTestingData()
	{
		return string.Concat(string.Concat(string.Concat(string.Concat(base.GetAdditionalTestingData() + "\n<b>Count:</b> " + count, "\n\tGrowth State ", currentGrowthState.ToStringEnum()), "\n\tGrowth Rate ", growthRate.ToString()), "\n\tRipening ticks: ", GetRipeningTicks().ToString()), "\n\tRemaining ticks until ripe: ", remainingRipeningTicks.ToString());
	}
}

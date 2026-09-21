using System;
using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UtilityScripts;

public sealed class PoisonCloud : MovingTileObject
{
	private PoisonCloudMapObjectVisual _poisonCloudVisual;

	public GameDate expiryDate { get; private set; }

	public int size { get; private set; }

	public int stacks { get; private set; }

	public int maxSize => 6;

	public bool doExpireEffect { get; private set; }

	public override string neutralizer => "Poison Expert";

	protected override int affectedRange => size;

	public override Type serializedData => typeof(SaveDataPoisonCloud);

	public PoisonCloud()
	{
		Initialize(TILE_OBJECT_TYPE.POISON_CLOUD, shouldAddCommonAdvertisements: false);
		base.traitContainer.RemoveTrait(this, "Flammable");
		AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
		AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
		SetExpiryDate(GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(2)));
		SetDoExpireEffect(state: true);
	}

	public PoisonCloud(SaveDataPoisonCloud data)
		: base(data)
	{
		expiryDate = data.expiryDate;
		base.hasExpired = data.hasExpired;
	}

	public void SetStacks(int stacks)
	{
		this.stacks = stacks;
		if (stacks >= 10)
		{
			base.traitContainer.AddTrait(this, "Dangerous");
		}
		else
		{
			base.traitContainer.RemoveTrait(this, "Dangerous");
		}
		UpdateSizeBasedOnPoisonedStacks();
	}

	public void Explode()
	{
		_poisonCloudVisual.Explode();
	}

	public void SetDoExpireEffect(bool state)
	{
		doExpireEffect = state;
	}

	protected override void CreateMapObjectVisual()
	{
		base.CreateMapObjectVisual();
		_poisonCloudVisual = mapVisual as PoisonCloudMapObjectVisual;
	}

	protected override bool TryGetGridTileLocation(out LocationGridTile tile)
	{
		if (_poisonCloudVisual != null && !base.hasExpired && _poisonCloudVisual.isSpawned)
		{
			tile = _poisonCloudVisual.gridTileLocation;
			return true;
		}
		tile = null;
		return false;
	}

	public override void Neutralize()
	{
		_poisonCloudVisual.Expire();
	}

	public void OnExpire()
	{
		if (doExpireEffect)
		{
			ExpireEffect();
		}
		if (PlayerManager.Instance.player.playerSkillComponent.latestCastMovingTileObject == this)
		{
			PlayerManager.Instance.player.playerSkillComponent.ClearLatestCastMovingObject();
		}
	}

	public override bool CanBeAffectedByElementalStatus(string traitName)
	{
		return false;
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		base.traitContainer.RemoveTrait(this, "Flammable");
	}

	public override void OnNoLongerLatestCast()
	{
		base.OnNoLongerLatestCast();
		SetMovementType(Moving_Object_Movement_Type.Default);
		if (_poisonCloudVisual != null)
		{
			_poisonCloudVisual.OnNoLongerLatestCastMovingObject();
		}
	}

	public void SetExpiryDate(GameDate expiryDate)
	{
		this.expiryDate = expiryDate;
	}

	private void SetSize(int size)
	{
		this.size = size;
		if (_poisonCloudVisual != null)
		{
			_poisonCloudVisual.SetSize(size);
		}
	}

	private void UpdateSizeBasedOnPoisonedStacks()
	{
		if (Utilities.IsInRange(stacks, 1, 3))
		{
			SetSize(1);
		}
		else if (Utilities.IsInRange(stacks, 3, 5))
		{
			SetSize(2);
		}
		else if (Utilities.IsInRange(stacks, 5, 10))
		{
			SetSize(3);
		}
		else if (Utilities.IsInRange(stacks, 10, 17))
		{
			SetSize(4);
		}
		else if (Utilities.IsInRange(stacks, 17, 26))
		{
			SetSize(5);
		}
		else
		{
			SetSize(6);
		}
	}

	private void ExpireEffect()
	{
		if (gridTileLocation == null)
		{
			return;
		}
		int num = 0;
		num = ((!Utilities.IsEven(size)) ? (size - 2) : (size - 3));
		if (num <= 0)
		{
			gridTileLocation.tileObjectComponent.genericTileObject.traitContainer.AddTrait(gridTileLocation.tileObjectComponent.genericTileObject, "Poisoned", null, bypassElementalChance: true, -1, 0f, ELEMENTAL_TYPE.Poison);
			gridTileLocation.tileObjectComponent.genericTileObject.traitContainer.GetTraitOrStatus<Poisoned>("Poisoned")?.SetIsPlayerSource(base.isPlayerSource);
			return;
		}
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		gridTileLocation.PopulateTilesInRadius(list, num, 0, includeCenterTile: true, includeTilesInDifferentStructure: true);
		for (int i = 0; i < list.Count; i++)
		{
			list[i].tileObjectComponent.genericTileObject.traitContainer.AddTrait(list[i].tileObjectComponent.genericTileObject, "Poisoned", null, bypassElementalChance: true, -1, 0f, ELEMENTAL_TYPE.Poison);
			list[i].tileObjectComponent.genericTileObject.traitContainer.GetTraitOrStatus<Poisoned>("Poisoned")?.SetIsPlayerSource(base.isPlayerSource);
		}
		RuinarchListPool<LocationGridTile>.Release(list);
	}
}

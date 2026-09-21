using System;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;

public class FrostyFog : MovingTileObject
{
	private FrostyFogMapObjectVisual _frostyFogMapVisual;

	public int size { get; private set; }

	public int stacks { get; private set; }

	public GameDate expiryDate { get; }

	public int maxSize => 6;

	public override Type serializedData => typeof(SaveDataFrostyFog);

	public FrostyFog()
	{
		Initialize(TILE_OBJECT_TYPE.FROSTY_FOG, shouldAddCommonAdvertisements: false);
		AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
		AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
		base.traitContainer.RemoveTrait(this, "Flammable");
		expiryDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(2));
	}

	public FrostyFog(SaveDataFrostyFog data)
		: base(data)
	{
		expiryDate = data.expiryDate;
		SetStacks(data.stacks);
		base.hasExpired = data.hasExpired;
	}

	protected override void CreateMapObjectVisual()
	{
		base.CreateMapObjectVisual();
		_frostyFogMapVisual = mapVisual as FrostyFogMapObjectVisual;
	}

	public override void Neutralize()
	{
		_frostyFogMapVisual.Expire();
	}

	public void OnExpire()
	{
		Messenger.Broadcast<TileObject, Character, LocationGridTile>(GridTileSignals.TILE_OBJECT_REMOVED, this, null, base.gridTileLocation);
	}

	public override string ToString()
	{
		return "Frosty Fog";
	}

	public override void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false, object source = null, CombatManager.ElementalTraitProcessor elementalTraitProcessor = null, bool showHPBar = false, float piercingPower = 0f, bool isPlayerSource = false, bool isTrueDamage = false)
	{
		if (base.currentHP == 0 && amount < 0)
		{
			return;
		}
		LocationGridTile locationGridTile = gridTileLocation;
		if (!isTrueDamage)
		{
			CombatManager.Instance.ModifyDamage(ref amount, elementalDamageType, piercingPower, this);
		}
		base.currentHP += amount;
		base.currentHP = Mathf.Clamp(base.currentHP, 0, base.maxHP);
		if (amount < 0)
		{
			Character characterResponsible = null;
			if (source is Character character)
			{
				characterResponsible = character;
			}
			CombatManager.Instance.ApplyElementalDamage(amount, elementalDamageType, this, characterResponsible, elementalTraitProcessor, createHitEffect: true, isPlayerSource, piercingPower);
		}
		switch (elementalDamageType)
		{
		case ELEMENTAL_TYPE.Fire:
		{
			List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
			locationGridTile.PopulateTilesInRadius(list, 1, 0, includeCenterTile: true, includeTilesInDifferentStructure: true);
			for (int i = 0; i < list.Count; i++)
			{
				list[i].AddTraitToAllPOIsOnTile("Wet", ELEMENTAL_TYPE.Water);
			}
			RuinarchListPool<LocationGridTile>.Release(list);
			_frostyFogMapVisual.Expire();
			break;
		}
		case ELEMENTAL_TYPE.Electric:
		{
			int num = 1 + stacks / 4;
			if (num > 4)
			{
				num = 4;
			}
			for (int j = 0; j < num; j++)
			{
				SpawnBallLightningToDirection(isPlayerSource);
			}
			_frostyFogMapVisual.Expire();
			break;
		}
		default:
			if (base.currentHP == 0)
			{
				_frostyFogMapVisual.Expire();
			}
			break;
		}
	}

	private void SpawnBallLightningToDirection(bool p_isPlayerSource)
	{
		BallLightning ballLightning = new BallLightning();
		ballLightning.targetDirection = Vector3.zero;
		ballLightning.SetGridTileLocation(gridTileLocation);
		ballLightning.OnPlacePOI();
		if (p_isPlayerSource)
		{
			PlayerManager.Instance?.player?.goalComponent.CompleteSubGoal(SUB_GOAL.GOAL_BALL_LIGHTNING);
		}
	}

	protected override bool TryGetGridTileLocation(out LocationGridTile tile)
	{
		if (_frostyFogMapVisual != null && _frostyFogMapVisual.isSpawned)
		{
			tile = _frostyFogMapVisual.gridTileLocation;
			return true;
		}
		tile = null;
		return false;
	}

	public void SetStacks(int stacks)
	{
		this.stacks = stacks;
		UpdateSizeBasedOnStacks();
	}

	private void SetSize(int size)
	{
		this.size = size;
		if (_frostyFogMapVisual != null)
		{
			_frostyFogMapVisual.SetSize(size);
		}
	}

	private void UpdateSizeBasedOnStacks()
	{
		if (stacks >= 1 && stacks <= 2)
		{
			SetSize(1);
		}
		else if (stacks >= 3 && stacks <= 4)
		{
			SetSize(2);
		}
		else if (stacks >= 5 && stacks <= 9)
		{
			SetSize(3);
		}
		else if (stacks >= 10 && stacks <= 16)
		{
			SetSize(4);
		}
		else if (stacks >= 17 && stacks <= 25)
		{
			SetSize(5);
		}
		else if (stacks >= 26)
		{
			SetSize(6);
		}
	}
}

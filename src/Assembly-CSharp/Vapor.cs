using System;
using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UnityEngine;
using UtilityScripts;

public class Vapor : MovingTileObject
{
	private VaporMapObjectVisual _vaporMapVisualObject;

	public int size { get; private set; }

	public int stacks { get; private set; }

	public GameDate expiryDate { get; }

	public bool doExpireEffect { get; private set; }

	protected override int affectedRange => size;

	public int maxSize => 6;

	public override Type serializedData => typeof(SaveDataVapor);

	public Vapor()
	{
		Initialize(TILE_OBJECT_TYPE.VAPOR, shouldAddCommonAdvertisements: false);
		base.traitContainer.RemoveTrait(this, "Flammable");
		SetDoExpireEffect(state: true);
		expiryDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(2));
	}

	public Vapor(SaveDataVapor data)
		: base(data)
	{
		expiryDate = data.expiryDate;
		SetStacks(data.stacks);
		base.hasExpired = data.hasExpired;
	}

	protected override void CreateMapObjectVisual()
	{
		base.CreateMapObjectVisual();
		_vaporMapVisualObject = mapVisual as VaporMapObjectVisual;
	}

	public void SetDoExpireEffect(bool state)
	{
		doExpireEffect = state;
	}

	public void SetStacks(int stacks)
	{
		this.stacks = stacks;
		UpdateSizeBasedOnWetStacks();
	}

	public override void Neutralize()
	{
		_vaporMapVisualObject.Expire();
	}

	public void OnExpire()
	{
		if (doExpireEffect)
		{
			ExpireEffect();
		}
	}

	public override string ToString()
	{
		return "Vapor";
	}

	public override void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false, object source = null, CombatManager.ElementalTraitProcessor elementalTraitProcessor = null, bool showHPBar = false, float piercingPower = 0f, bool isPlayerSource = false, bool isTrueDamage = false)
	{
		if (base.currentHP == 0 && amount < 0)
		{
			return;
		}
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
			_vaporMapVisualObject.Expire();
			break;
		case ELEMENTAL_TYPE.Ice:
		{
			LocationGridTile locationGridTile2 = gridTileLocation;
			SetDoExpireEffect(state: false);
			_vaporMapVisualObject.Expire();
			FrostyFog frostyFog = new FrostyFog();
			frostyFog.SetGridTileLocation(locationGridTile2);
			frostyFog.OnPlacePOI();
			frostyFog.SetStacks(stacks);
			if (isPlayerSource)
			{
				PlayerManager.Instance?.player?.goalComponent.CompleteSubGoal(SUB_GOAL.GOAL_FROSTY_FOG);
			}
			break;
		}
		case ELEMENTAL_TYPE.Poison:
		{
			LocationGridTile locationGridTile = gridTileLocation;
			SetDoExpireEffect(state: false);
			_vaporMapVisualObject.Expire();
			InnerMapManager.Instance.SpawnPoisonCloud(locationGridTile, stacks, GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(GameUtilities.RandomBetweenTwoNumbers(2, 5))));
			if (isPlayerSource)
			{
				PlayerManager.Instance?.player?.goalComponent.CompleteSubGoal(SUB_GOAL.GOAL_POISON_CLOUD);
			}
			break;
		}
		}
		if (!base.hasExpired && base.currentHP == 0)
		{
			_vaporMapVisualObject.Expire();
		}
	}

	protected override bool TryGetGridTileLocation(out LocationGridTile tile)
	{
		if (_vaporMapVisualObject != null && _vaporMapVisualObject.isSpawned)
		{
			tile = _vaporMapVisualObject.gridTileLocation;
			return true;
		}
		tile = null;
		return false;
	}

	private void SetSize(int size)
	{
		this.size = size;
		if (_vaporMapVisualObject != null)
		{
			_vaporMapVisualObject.SetSize(size);
		}
	}

	private void UpdateSizeBasedOnWetStacks()
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
			gridTileLocation.tileObjectComponent.genericTileObject.traitContainer.AddTrait(gridTileLocation.tileObjectComponent.genericTileObject, "Wet", null, bypassElementalChance: false, -1, 0f, ELEMENTAL_TYPE.Water);
			gridTileLocation.tileObjectComponent.genericTileObject.traitContainer.GetTraitOrStatus<Wet>("Wet")?.SetIsPlayerSource(base.isPlayerSource);
			return;
		}
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		gridTileLocation.PopulateTilesInRadius(list, num, 0, includeCenterTile: true, includeTilesInDifferentStructure: true);
		for (int i = 0; i < list.Count; i++)
		{
			list[i].tileObjectComponent.genericTileObject.traitContainer.AddTrait(list[i].tileObjectComponent.genericTileObject, "Wet", null, bypassElementalChance: false, -1, 0f, ELEMENTAL_TYPE.Water);
			list[i].tileObjectComponent.genericTileObject.traitContainer.GetTraitOrStatus<Wet>("Wet")?.SetIsPlayerSource(base.isPlayerSource);
		}
		RuinarchListPool<LocationGridTile>.Release(list);
	}
}

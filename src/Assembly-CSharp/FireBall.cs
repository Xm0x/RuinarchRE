using System;
using Inner_Maps;
using UnityEngine;

public class FireBall : MovingTileObject
{
	private FireBallMapObjectVisual _fireBallMapVisual;

	public override string neutralizer => "Fire Master";

	public GameDate expiryDate { get; set; }

	public Vector3 targetDirection { get; set; }

	public override Type serializedData => typeof(SaveDataFireBall);

	public FireBall()
	{
		Initialize(TILE_OBJECT_TYPE.FIRE_BALL, shouldAddCommonAdvertisements: false);
		base.traitContainer.RemoveTrait(this, "Flammable");
		AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
		AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
		expiryDate = GameManager.Instance.Today().AddTicks(PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.FIRE_BALL));
	}

	public FireBall(SaveDataFireBall data)
		: base(data)
	{
		expiryDate = data.expiryDate;
		base.hasExpired = data.hasExpired;
	}

	protected override void CreateMapObjectVisual()
	{
		base.CreateMapObjectVisual();
		_fireBallMapVisual = mapVisual as FireBallMapObjectVisual;
	}

	public override void Neutralize()
	{
		_fireBallMapVisual.Expire();
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		base.traitContainer.AddTrait(this, "Dangerous");
		base.traitContainer.RemoveTrait(this, "Flammable");
	}

	public void OnExpire()
	{
		Messenger.Broadcast<TileObject, Character, LocationGridTile>(GridTileSignals.TILE_OBJECT_REMOVED, this, null, base.gridTileLocation);
	}

	public override string ToString()
	{
		return "Fire Ball";
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
		if (amount < 0 && elementalDamageType == ELEMENTAL_TYPE.Water)
		{
			for (int i = 0; i < 2; i++)
			{
				Vapor vapor = new Vapor();
				vapor.SetStacks(2);
				vapor.SetGridTileLocation(locationGridTile);
				vapor.OnPlacePOI();
			}
		}
		else if (base.currentHP == 0 || (amount < 0 && elementalDamageType == ELEMENTAL_TYPE.Ice))
		{
			_fireBallMapVisual.Expire();
		}
	}

	protected override bool TryGetGridTileLocation(out LocationGridTile tile)
	{
		if (_fireBallMapVisual != null && _fireBallMapVisual.isSpawned)
		{
			tile = _fireBallMapVisual.gridTileLocation;
			return true;
		}
		tile = null;
		return false;
	}
}

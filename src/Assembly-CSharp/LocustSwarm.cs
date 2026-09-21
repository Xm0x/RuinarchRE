using System;
using UnityEngine;

public class LocustSwarm : MovingTileObject
{
	private LocustSwarmMapObjectVisual _locustSwarmMapObjectVisual;

	public override string neutralizer => "Beastmaster";

	public GameDate expiryDate { get; }

	public override Type serializedData => typeof(SaveDataLocustSwarm);

	public LocustSwarm()
	{
		int durationBonusPerLevel = PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.LOCUST_SWARM);
		Initialize(TILE_OBJECT_TYPE.LOCUST_SWARM, shouldAddCommonAdvertisements: false);
		base.traitContainer.RemoveTrait(this, "Flammable");
		AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
		AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
		expiryDate = GameManager.Instance.Today().AddTicks(durationBonusPerLevel);
	}

	public LocustSwarm(SaveDataLocustSwarm data)
		: base(data)
	{
		expiryDate = data.expiryDate;
		base.hasExpired = data.hasExpired;
	}

	protected override void CreateMapObjectVisual()
	{
		base.CreateMapObjectVisual();
		_locustSwarmMapObjectVisual = mapVisual as LocustSwarmMapObjectVisual;
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		base.traitContainer.AddTrait(this, "Dangerous");
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
		if (amount < 0 && source != null)
		{
			Character characterResponsible = null;
			if (source is Character character)
			{
				characterResponsible = character;
			}
			CombatManager.Instance.ApplyElementalDamage(amount, elementalDamageType, this, characterResponsible, elementalTraitProcessor, createHitEffect: true, isPlayerSource, piercingPower);
		}
		if (base.currentHP == 0)
		{
			_locustSwarmMapObjectVisual.Expire();
		}
		if (amount < 0)
		{
			if (source is Character arg)
			{
				Messenger.Broadcast(TileObjectSignals.TILE_OBJECT_DAMAGED_BY, (TileObject)this, amount, arg, isPlayerSource);
			}
			else
			{
				Messenger.Broadcast(TileObjectSignals.TILE_OBJECT_DAMAGED, (TileObject)this, amount, isPlayerSource);
			}
		}
		else if (amount > 0)
		{
			if (base.currentHP == base.maxHP)
			{
				Messenger.Broadcast(TileObjectSignals.TILE_OBJECT_FULLY_REPAIRED, (TileObject)this);
			}
			else
			{
				Messenger.Broadcast(TileObjectSignals.TILE_OBJECT_REPAIRED, (TileObject)this, amount);
			}
		}
	}

	public override void Neutralize()
	{
		_locustSwarmMapObjectVisual.Expire();
	}

	public override void OnNoLongerLatestCast()
	{
		base.OnNoLongerLatestCast();
		SetMovementType(Moving_Object_Movement_Type.Default);
		if (_locustSwarmMapObjectVisual != null)
		{
			_locustSwarmMapObjectVisual.OnNoLongerLatestCastMovingObject();
		}
	}
}

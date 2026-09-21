using System;
using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UnityEngine;
using UtilityScripts;

public abstract class MovingTileObject : TileObject
{
	private GameObject _followParticleEffect;

	public sealed override LocationGridTile gridTileLocation
	{
		get
		{
			if (!TryGetGridTileLocation(out var tile))
			{
				return base.gridTileLocation;
			}
			return tile;
		}
		protected set
		{
			base.gridTileLocation = value;
		}
	}

	public override MapObjectVisual<TileObject> mapVisual => movingMapVisual;

	public MovingMapObjectVisual movingMapVisual { get; private set; }

	public bool hasExpired { get; protected set; }

	public bool isPlayerSource { get; private set; }

	public Moving_Object_Movement_Type currentMovementType { get; private set; }

	protected virtual int affectedRange => 1;

	public override Type serializedData => typeof(SaveDataMovingTileObject);

	public MovingTileObject()
	{
	}

	public MovingTileObject(SaveDataMovingTileObject data)
		: base(data)
	{
		isPlayerSource = data.isPlayerSource;
	}

	protected virtual bool TryGetGridTileLocation(out LocationGridTile tile)
	{
		if (movingMapVisual != null && movingMapVisual.isSpawned)
		{
			tile = movingMapVisual.gridTileLocation;
			return true;
		}
		tile = null;
		return false;
	}

	public virtual void OnNoLongerLatestCast()
	{
	}

	protected override void CreateMapObjectVisual()
	{
		GameObject gameObject = InnerMapManager.Instance.mapObjectFactory.CreateNewTileObjectMapVisual(base.tileObjectType);
		movingMapVisual = gameObject.GetComponent<MovingMapObjectVisual>();
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		Messenger.AddListener<LocationGridTile, List<ITraitable>>(GridTileSignals.POPULATE_TRAITABLES_ON_TILE, OnPopulateTraitablesOnTile);
		Messenger.AddListener<LocationGridTile, List<ITraitable>>(GridTileSignals.POPULATE_ALIVE_TRAITABLES_ON_TILE, OnPopulateTraitablesOnTile);
	}

	public virtual void Expire()
	{
		if (!hasExpired)
		{
			hasExpired = true;
			Messenger.RemoveListener<LocationGridTile, List<ITraitable>>(GridTileSignals.POPULATE_TRAITABLES_ON_TILE, OnPopulateTraitablesOnTile);
			Messenger.RemoveListener<LocationGridTile, List<ITraitable>>(GridTileSignals.POPULATE_ALIVE_TRAITABLES_ON_TILE, OnPopulateTraitablesOnTile);
			DatabaseManager.Instance.tileObjectDatabase.UnRegisterTileObject(this);
			Messenger.Broadcast(TileObjectSignals.MOVING_TILE_OBJECT_EXPIRED, this);
			Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, (IPointOfInterest)this, "");
		}
	}

	public override void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false, object source = null, CombatManager.ElementalTraitProcessor elementalTraitProcessor = null, bool showHPBar = false, float piercingPower = 0f, bool isPlayerSource = false, bool isTrueDamage = false)
	{
		if (base.currentHP == 0 && amount < 0)
		{
			return;
		}
		Character character = source as Character;
		if (character != null && character.faction != null && character.faction.isPlayerFaction)
		{
			isPlayerSource = true;
		}
		if (!isTrueDamage)
		{
			CombatManager.Instance.ModifyDamage(ref amount, elementalDamageType, piercingPower, this);
		}
		if ((amount < 0 && CanBeDamaged()) || amount > 0)
		{
			if (amount < 0 && Mathf.Abs(amount) > base.currentHP)
			{
				amount = -base.currentHP;
			}
			base.currentHP += amount;
			base.currentHP = Mathf.Clamp(base.currentHP, 0, base.maxHP);
		}
		if (amount < 0)
		{
			CombatManager.Instance.ApplyElementalDamage(amount, elementalDamageType, this, character, elementalTraitProcessor, createHitEffect: true, isPlayerSource, piercingPower);
		}
		LocationGridTile locationGridTile = gridTileLocation;
		if (base.currentHP <= 0)
		{
			if (base.isBeingSeized)
			{
				PlayerManager.Instance.player.seizeComponent.UnseizeTileObjectThenDestroyIt();
			}
			else if (locationGridTile != null && locationGridTile.structure != null)
			{
				locationGridTile.structure.RemovePOI(this, character, isPlayerSource);
			}
			else if (base.isBeingCarriedBy != null)
			{
				base.isBeingCarriedBy.UncarryPOI(this, bringBackToInventory: false, addToLocation: false);
			}
		}
		if (amount < 0)
		{
			if (character != null)
			{
				Messenger.Broadcast(TileObjectSignals.TILE_OBJECT_DAMAGED_BY, this, amount, character, isPlayerSource);
			}
			else
			{
				Messenger.Broadcast(TileObjectSignals.TILE_OBJECT_DAMAGED, this, amount, isPlayerSource);
			}
		}
		else if (amount > 0)
		{
			Messenger.Broadcast(TileObjectSignals.TILE_OBJECT_REPAIRED, this, amount);
		}
		if (base.currentHP == base.maxHP)
		{
			Messenger.Broadcast(TileObjectSignals.TILE_OBJECT_FULLY_REPAIRED, this);
		}
		if (amount < 0)
		{
			base.currentHP = base.maxHP;
		}
	}

	private void OnPopulateTraitablesOnTile(LocationGridTile tile, List<ITraitable> traitables)
	{
		if (traitables.Contains(this))
		{
			return;
		}
		LocationGridTile locationGridTile = gridTileLocation;
		if (locationGridTile == null)
		{
			return;
		}
		if (affectedRange == 0)
		{
			if (tile == locationGridTile)
			{
				traitables.Add(this);
			}
			return;
		}
		if (affectedRange == 1)
		{
			if (tile == locationGridTile)
			{
				traitables.Add(this);
			}
			else if (locationGridTile.neighbourList.Contains(tile))
			{
				traitables.Add(this);
			}
			return;
		}
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		locationGridTile.PopulateTilesInRadius(list, affectedRange, 0, includeCenterTile: true, includeTilesInDifferentStructure: true);
		if (list.Contains(tile))
		{
			traitables.Add(this);
		}
		RuinarchListPool<LocationGridTile>.Release(list);
	}

	public void SetIsPlayerSource(bool p_state)
	{
		isPlayerSource = p_state;
	}

	public bool SetMovementType(Moving_Object_Movement_Type p_type)
	{
		if (currentMovementType == p_type)
		{
			return false;
		}
		currentMovementType = p_type;
		if (p_type == Moving_Object_Movement_Type.Follow_Cursor)
		{
			_followParticleEffect = GameManager.Instance.CreateParticleEffectAt(this, PARTICLE_EFFECT.Follow_Cursor);
		}
		else if (_followParticleEffect != null)
		{
			ObjectPoolManager.Instance.DestroyObject(_followParticleEffect);
		}
		return true;
	}
}

using System;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Map_Objects.Map_Object_Visuals;
using Traits;
using UnityEngine;
using UtilityScripts;

public class IceBlockWall : TileObject
{
	private string _expiryScheduleKey;

	private Action<ITraitable> _traitableCallback;

	public GameDate expiryDate { get; private set; }

	public bool leftBotImpassable { get; private set; }

	public bool leftTopImpassable { get; private set; }

	public bool topLeftImpassable { get; private set; }

	public bool topRightImpassable { get; private set; }

	public bool rightTopImpassable { get; private set; }

	public bool rightBotImpassable { get; private set; }

	public bool botRightImpassable { get; private set; }

	public bool botLeftImpassable { get; private set; }

	public string expiryScheduleKey => _expiryScheduleKey;

	public override Type serializedData => typeof(SaveDataIceBlockWall);

	public IceBlockWall()
	{
		Initialize(TILE_OBJECT_TYPE.ICE_BLOCK_WALL, shouldAddCommonAdvertisements: false);
		AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
		AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
		base.traitContainer.AddTrait(this, "Immovable");
		base.traitContainer.RemoveTrait(this, "Flammable");
		_traitableCallback = ApplyExplosionEffect;
	}

	public IceBlockWall(SaveDataIceBlockWall data)
		: base(data)
	{
		leftBotImpassable = data.leftBotImpassable;
		leftTopImpassable = data.leftTopImpassable;
		topLeftImpassable = data.topLeftImpassable;
		topRightImpassable = data.topRightImpassable;
		rightTopImpassable = data.rightTopImpassable;
		rightBotImpassable = data.rightBotImpassable;
		botRightImpassable = data.botRightImpassable;
		botLeftImpassable = data.botLeftImpassable;
		_traitableCallback = ApplyExplosionEffect;
	}

	protected override void SubscribeListeners(bool shouldLock)
	{
		if (!base.hasSubscribedToSignals)
		{
			base.hasSubscribedToSignals = true;
			base.SubscribeListeners(shouldLock);
			Messenger.AddListener(Signals.TICK_STARTED, OnTickStarted, shouldLock);
		}
	}

	protected override void UnsubscribeListeners()
	{
		if (base.hasSubscribedToSignals)
		{
			base.hasSubscribedToSignals = false;
			base.UnsubscribeListeners();
			Messenger.RemoveListener(Signals.TICK_STARTED, OnTickStarted);
		}
	}

	private void OnTickStarted()
	{
		if (!GameUtilities.RollChance(25))
		{
			return;
		}
		LocationGridTile locationGridTile = gridTileLocation;
		if (locationGridTile == null)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < locationGridTile.neighbourList.Count; i++)
		{
			LocationGridTile locationGridTile2 = locationGridTile.neighbourList[i];
			if (locationGridTile2.tileObjectComponent.genericTileObject.traitContainer.HasTrait("Burning"))
			{
				flag = true;
				break;
			}
			TileObject objHere = locationGridTile2.tileObjectComponent.objHere;
			if (objHere != null && objHere.traitContainer.HasTrait("Burning"))
			{
				flag = true;
				break;
			}
			objHere = locationGridTile2.tileObjectComponent.hiddenObjHere;
			if (objHere != null && objHere.traitContainer.HasTrait("Burning"))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			locationGridTile.structure.RemovePOI(this);
		}
	}

	public override bool IsUnpassable()
	{
		return true;
	}

	public override bool IsValidCombatTargetFor(IPointOfInterest source)
	{
		if (gridTileLocation == null)
		{
			return false;
		}
		if (source.gridTileLocation == null)
		{
			return false;
		}
		return true;
	}

	public override void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom, bool removeTraits = true, bool destroyTileSlots = true)
	{
		removedFrom.SetTileType(LocationGridTile.Tile_Type.Empty);
		mapVisual.DestroyExistingGUS();
		if (!string.IsNullOrEmpty(_expiryScheduleKey))
		{
			SchedulingManager.Instance.RemoveSpecificEntry(_expiryScheduleKey);
		}
		base.OnRemoveTileObject(removedBy, removedFrom, removeTraits, destroyTileSlots);
		EvaluateAllOreVeinOrBlockWallNeighboursForDiagonalImpassables(removedFrom, p_includeCenterTile: false);
		removedFrom.CreateSeamlessEdgesForSelfAndNeighbours();
		removedFrom.tileObjectComponent.genericTileObject.traitContainer.AddTrait(removedFrom.tileObjectComponent.genericTileObject, "Wet");
	}

	protected override void OnPlaceTileObjectAtTile(LocationGridTile tile)
	{
		tile.SetTileType(LocationGridTile.Tile_Type.Wall);
		Vector2 size = new Vector2(0.5f, 0.5f);
		mapVisual.InitializeGUS(Vector2.zero, size, tile);
		base.OnPlaceTileObjectAtTile(tile);
		EvaluateAllOreVeinOrBlockWallNeighboursForDiagonalImpassables(tile, p_includeCenterTile: true);
		tile.CreateSeamlessEdgesForSelfAndNeighbours();
	}

	public override void OnLoadPlacePOI()
	{
		base.OnLoadPlacePOI();
		if (mapVisual is IceBlockWallGameObject iceBlockWallGameObject)
		{
			iceBlockWallGameObject.leftBotImpassable.SetActive(leftBotImpassable);
			iceBlockWallGameObject.leftTopImpassable.SetActive(leftTopImpassable);
			iceBlockWallGameObject.topLeftImpassable.SetActive(topLeftImpassable);
			iceBlockWallGameObject.topRightImpassable.SetActive(topRightImpassable);
			iceBlockWallGameObject.rightTopImpassable.SetActive(rightTopImpassable);
			iceBlockWallGameObject.rightBotImpassable.SetActive(rightBotImpassable);
			iceBlockWallGameObject.botRightImpassable.SetActive(botRightImpassable);
			iceBlockWallGameObject.botLeftImpassable.SetActive(botLeftImpassable);
		}
	}

	public override void ConstructDefaultPlayerActions(bool broadcastSignal = true)
	{
		base.ConstructDefaultPlayerActions(broadcastSignal);
		RemovePlayerAction(PLAYER_SKILL_TYPE.SEIZE_OBJECT, broadcastSignal);
		RemovePlayerAction(PLAYER_SKILL_TYPE.POISON, broadcastSignal);
		RemovePlayerAction(PLAYER_SKILL_TYPE.IGNITE, broadcastSignal);
	}

	public override bool CanBeSelected()
	{
		return true;
	}

	public void SetExpiry(GameDate expiry)
	{
		expiryDate = expiry;
		_expiryScheduleKey = SchedulingManager.Instance.AddEntry(expiryDate, Expire, this);
	}

	private void Expire()
	{
		if (gridTileLocation != null)
		{
			gridTileLocation.structure.RemovePOI(this);
		}
	}

	private void EvaluateAllOreVeinOrBlockWallNeighboursForDiagonalImpassables(LocationGridTile p_centerTile, bool p_includeCenterTile)
	{
		if (GameManager.Instance.gameHasStarted)
		{
			if (p_includeCenterTile)
			{
				(mapVisual as IceBlockWallGameObject).EvaluateImpassables(this);
			}
			for (int i = 0; i < p_centerTile.neighbourList.Count; i++)
			{
				LocationGridTile p_neighbourTile = p_centerTile.neighbourList[i];
				EvaluateOreVeinOrBlockWallNeighbour(p_neighbourTile);
			}
		}
	}

	private void EvaluateOreVeinOrBlockWallNeighbour(LocationGridTile p_neighbourTile)
	{
		if (p_neighbourTile.tileObjectComponent.objHere is OreVein oreVein)
		{
			(oreVein.mapVisual as OreVeinGameObject).EvaluateImpassables(oreVein);
		}
		else if (p_neighbourTile.tileObjectComponent.objHere is BlockWall blockWall)
		{
			(blockWall.mapVisual as BlockWallGameObject).EvaluateImpassables(blockWall);
		}
		else if (p_neighbourTile.tileObjectComponent.objHere is IceBlockWall iceBlockWall)
		{
			(iceBlockWall.mapVisual as IceBlockWallGameObject).EvaluateImpassables(iceBlockWall);
		}
	}

	public void SetImpassables(bool leftBotImpassable, bool leftTopImpassable, bool topLeftImpassable, bool topRightImpassable, bool rightTopImpassable, bool rightBotImpassable, bool botRightImpassable, bool botLeftImpassable)
	{
		this.leftBotImpassable = leftBotImpassable;
		this.leftTopImpassable = leftTopImpassable;
		this.topLeftImpassable = topLeftImpassable;
		this.topRightImpassable = topRightImpassable;
		this.rightTopImpassable = rightTopImpassable;
		this.rightBotImpassable = rightBotImpassable;
		this.botRightImpassable = botRightImpassable;
		this.botLeftImpassable = botLeftImpassable;
	}

	public void OnHitByFire()
	{
		LocationGridTile locationGridTile = gridTileLocation;
		if (locationGridTile != null)
		{
			SkillData spellData = PlayerSkillManager.Instance.GetSpellData(PLAYER_SKILL_TYPE.ICETEROIDS);
			if (spellData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Ice_Block_Explode))
			{
				Explode();
			}
			else if (!spellData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Ice_Block_Lightning_Fire_No_Melt))
			{
				locationGridTile.structure.RemovePOI(this);
			}
		}
	}

	public void OnHitByLightning()
	{
		LocationGridTile locationGridTile = gridTileLocation;
		if (locationGridTile != null)
		{
			SkillData spellData = PlayerSkillManager.Instance.GetSpellData(PLAYER_SKILL_TYPE.ICETEROIDS);
			if (spellData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Ice_Block_Explode))
			{
				Explode();
			}
			else if (!spellData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Ice_Block_Lightning_No_Melt) && !spellData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Ice_Block_Lightning_Fire_No_Melt))
			{
				locationGridTile.structure.RemovePOI(this);
			}
		}
	}

	public void Explode()
	{
		LocationGridTile locationGridTile = gridTileLocation;
		if (locationGridTile != null)
		{
			locationGridTile.structure.RemovePOI(this);
			List<ITraitable> list = RuinarchListPool<ITraitable>.Claim();
			locationGridTile.PopulateAliveTraitablesOnTile(list);
			for (int i = 0; i < locationGridTile.neighbourList.Count; i++)
			{
				locationGridTile.neighbourList[i].PopulateAliveTraitablesOnTile(list);
			}
			TraitManager.Instance.PerformActionOnTraitables(list, _traitableCallback);
			RuinarchListPool<ITraitable>.Release(list);
		}
	}

	private void ApplyExplosionEffect(ITraitable traitable)
	{
		traitable.AdjustHP(-100, ELEMENTAL_TYPE.Ice, triggerDeath: true, null, null, showHPBar: true);
	}
}

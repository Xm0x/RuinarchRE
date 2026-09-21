using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UnityEngine;
using UtilityScripts;

public class ThinWall : TileObject
{
	private ThinWallGameObject _visual;

	public WALL_RESOURCE madeOf { get; private set; }

	public override MapObjectVisual<TileObject> mapVisual => _visual;

	public ThinWall()
	{
	}

	public ThinWall(SaveDataTileObject data)
		: base(data)
	{
	}

	public void InitializeThinWall()
	{
		Initialize(TILE_OBJECT_TYPE.THIN_WALL, shouldAddCommonAdvertisements: false);
	}

	protected override void Initialize(TILE_OBJECT_TYPE tileObjectType, bool shouldAddCommonAdvertisements = true)
	{
		base.Initialize(tileObjectType, shouldAddCommonAdvertisements);
		AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
		AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
		AddAdvertisedAction(INTERACTION_TYPE.DIG);
		UpdateMaxHPBasedOnResource();
		base.currentHP = base.maxHP;
		InitializeMapObject(this);
	}

	public void SetVisualGO(ThinWallGameObject p_visualGO)
	{
		_visual = p_visualGO;
	}

	public void SetResourceMadeOf(WALL_RESOURCE p_resource)
	{
		madeOf = p_resource;
	}

	private void OnWallDestroyed()
	{
		gridTileLocation.SetTileType(LocationGridTile.Tile_Type.Empty);
		gridTileLocation.CreateSeamlessEdgesForSelfAndNeighbours();
		base.traitContainer.RemoveAllTraits(this);
		LocationAwarenessUtility.RemoveFromAwarenessList(this);
	}

	private void OnWallRepaired()
	{
		if (gridTileLocation != null)
		{
			LocationAwarenessUtility.AddToAwarenessList(this, gridTileLocation);
		}
	}

	private void UpdateMaxHPBasedOnResource()
	{
		switch (madeOf.GetResourceForWall())
		{
		case RESOURCE.WOOD:
			base.maxHP = 250;
			break;
		case RESOURCE.STONE:
			base.maxHP = 500;
			break;
		case RESOURCE.METAL:
			base.maxHP = 800;
			break;
		}
	}

	public override void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false, object source = null, CombatManager.ElementalTraitProcessor elementalTraitProcessor = null, bool showHPBar = false, float piercingPower = 0f, bool isPlayerSource = false, bool isTrueDamage = false)
	{
		bool flag = base.currentHP <= 0;
		if (flag && amount < 0)
		{
			return;
		}
		if (!isTrueDamage)
		{
			CombatManager.Instance.ModifyDamage(ref amount, elementalDamageType, piercingPower, this);
		}
		if (amount < 0 && Mathf.Abs(amount) > base.currentHP)
		{
			amount = -base.currentHP;
		}
		base.currentHP += amount;
		base.currentHP = Mathf.Clamp(base.currentHP, 0, base.maxHP);
		if (amount <= 0 && base.currentHP > 0)
		{
			Character characterResponsible = null;
			if (source != null && source is Character)
			{
				characterResponsible = source as Character;
			}
			CombatManager.Instance.ApplyElementalDamage(amount, elementalDamageType, this, characterResponsible, elementalTraitProcessor, createHitEffect: true, isPlayerSource, piercingPower);
		}
		if (amount < 0)
		{
			if (source is Character arg)
			{
				Messenger.Broadcast(StructureSignals.WALL_DAMAGED_BY, this, amount, arg, isPlayerSource);
			}
			else
			{
				Messenger.Broadcast(StructureSignals.WALL_DAMAGED, this, amount, isPlayerSource);
			}
		}
		else if (amount > 0)
		{
			Messenger.Broadcast(StructureSignals.WALL_REPAIRED, this, amount);
			if (flag)
			{
				OnWallRepaired();
			}
		}
		_visual.UpdateWallAssets(this);
		_visual.UpdateWallState(this);
		if (base.currentHP <= 0)
		{
			OnWallDestroyed();
		}
		else if (base.currentHP >= base.maxHP)
		{
			gridTileLocation.SetTileType(LocationGridTile.Tile_Type.Wall);
			gridTileLocation.CreateSeamlessEdgesForSelfAndNeighbours();
		}
	}

	protected override void OnMapObjectStateChanged()
	{
	}

	public override void InitializeMapObject(TileObject obj)
	{
		base.visionTrigger.Initialize(obj);
		base.visionTrigger.gameObject.SetActive(value: true);
		_visual.UpdateWallAssets(obj as ThinWall);
		if (!(mapVisual.selectable is TileObject tileObject))
		{
			return;
		}
		List<Trait> traitOverrideFunctions = tileObject.traitContainer.GetTraitOverrideFunctions("Initiate_Map_Visual_Trait");
		if (traitOverrideFunctions != null)
		{
			for (int i = 0; i < traitOverrideFunctions.Count; i++)
			{
				traitOverrideFunctions[i].OnInitiateMapObjectVisual(tileObject);
			}
		}
	}

	public void LoadDataFromSave(SaveDataTileObject saveDataStructureWallObject)
	{
		base.currentHP = saveDataStructureWallObject.currentHP;
		_visual.UpdateWallAssets(this);
		_visual.UpdateWallState(this);
	}
}

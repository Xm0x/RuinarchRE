using System;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Inner_Maps.Map_Objects.Map_Object_Visuals;
using UnityEngine;

public class BlockWall : TileObject
{
	private string _expiryScheduleKey;

	public WALL_TYPE wallType { get; private set; }

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

	public override Type serializedData => typeof(SaveDataBlockWall);

	public BlockWall()
	{
		Initialize(TILE_OBJECT_TYPE.BLOCK_WALL, shouldAddCommonAdvertisements: false);
		AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
		AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
		AddAdvertisedAction(INTERACTION_TYPE.DIG);
		base.traitContainer.RemoveTrait(this, "Flammable");
		base.traitContainer.AddTrait(this, "Immovable");
	}

	public BlockWall(SaveDataBlockWall data)
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
	}

	public void SetWallType(WALL_TYPE _wallType)
	{
		wallType = _wallType;
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

	public override bool CanBeAffectedByElementalStatus(string traitName)
	{
		return false;
	}

	public override void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom, bool removeTraits = true, bool destroyTileSlots = true)
	{
		removedFrom.SetElevationTilemapTileAsset(null);
		removedFrom.SetTileType(LocationGridTile.Tile_Type.Empty);
		mapVisual.DestroyExistingGUS();
		if (!string.IsNullOrEmpty(_expiryScheduleKey))
		{
			SchedulingManager.Instance.RemoveSpecificEntry(_expiryScheduleKey);
		}
		base.OnRemoveTileObject(removedBy, removedFrom, removeTraits, destroyTileSlots);
		EvaluateAllOreVeinOrBlockWallNeighboursForDiagonalImpassables(removedFrom, p_includeCenterTile: false);
		removedFrom.CreateSeamlessEdgesForSelfAndNeighbours();
		if (wallType == WALL_TYPE.Stone)
		{
			removedFrom.UpdateMinimapVisual(removedFrom.structure);
		}
	}

	protected override void OnPlaceTileObjectAtTile(LocationGridTile tile)
	{
		tile.SetElevationTilemapTileAsset(InnerMapManager.Instance.assetManager.GetWallAssetBasedOnWallType(wallType));
		tile.SetTileType(LocationGridTile.Tile_Type.Wall);
		Vector2 size = new Vector2(0.8f, 0.8f);
		mapVisual.InitializeGUS(Vector2.zero, size, tile);
		base.OnPlaceTileObjectAtTile(tile);
		EvaluateAllOreVeinOrBlockWallNeighboursForDiagonalImpassables(tile, p_includeCenterTile: true);
		tile.CreateSeamlessEdgesForSelfAndNeighbours();
		if (wallType == WALL_TYPE.Stone)
		{
			LocationStructure structure = tile.structure;
			if (structure != null && structure.structureType == STRUCTURE_TYPE.CAVE)
			{
				tile.UpdateMinimapVisual(tile.structure);
			}
		}
	}

	public override void OnLoadPlacePOI()
	{
		base.OnLoadPlacePOI();
		if (mapVisual is BlockWallGameObject blockWallGameObject)
		{
			blockWallGameObject.leftBotImpassable.SetActive(leftBotImpassable);
			blockWallGameObject.leftTopImpassable.SetActive(leftTopImpassable);
			blockWallGameObject.topLeftImpassable.SetActive(topLeftImpassable);
			blockWallGameObject.topRightImpassable.SetActive(topRightImpassable);
			blockWallGameObject.rightTopImpassable.SetActive(rightTopImpassable);
			blockWallGameObject.rightBotImpassable.SetActive(rightBotImpassable);
			blockWallGameObject.botRightImpassable.SetActive(botRightImpassable);
			blockWallGameObject.botLeftImpassable.SetActive(botLeftImpassable);
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
		if (wallType == WALL_TYPE.Demon_Stone && gridTileLocation?.structure is DemonicStructure && !expiryDate.hasValue)
		{
			return false;
		}
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

	public void UpdateVisual(LocationGridTile tile)
	{
		tile.SetElevationTilemapTileAsset(InnerMapManager.Instance.assetManager.GetWallAssetBasedOnWallType(wallType));
	}

	private void EvaluateAllOreVeinOrBlockWallNeighboursForDiagonalImpassables(LocationGridTile p_centerTile, bool p_includeCenterTile)
	{
		if (GameManager.Instance.gameHasStarted)
		{
			if (p_includeCenterTile)
			{
				(mapVisual as BlockWallGameObject).EvaluateImpassables(this);
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

	public override string GetAdditionalTestingData()
	{
		return string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(base.GetAdditionalTestingData() + "\nleftBotImpassable - " + leftBotImpassable, "\nleftTopImpassable - ", leftTopImpassable.ToString()), "\ntopLeftImpassable - ", topLeftImpassable.ToString()), "\ntopRightImpassable - ", topRightImpassable.ToString()), "\nrightTopImpassable - ", rightTopImpassable.ToString()), "\nrightBotImpassable - ", rightBotImpassable.ToString()), "\nbotRightImpassable - ", botRightImpassable.ToString()), "\nbotLeftImpassable - ", botLeftImpassable.ToString());
	}
}

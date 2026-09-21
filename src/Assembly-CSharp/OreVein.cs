using System;
using Inner_Maps;
using Inner_Maps.Map_Objects.Map_Object_Visuals;
using UnityEngine;

public class OreVein : TileObject
{
	public bool leftBotImpassable { get; private set; }

	public bool leftTopImpassable { get; private set; }

	public bool topLeftImpassable { get; private set; }

	public bool topRightImpassable { get; private set; }

	public bool rightTopImpassable { get; private set; }

	public bool rightBotImpassable { get; private set; }

	public bool botRightImpassable { get; private set; }

	public bool botLeftImpassable { get; private set; }

	public override Type serializedData => typeof(SaveDataOreVein);

	public OreVein()
	{
		Initialize(TILE_OBJECT_TYPE.ORE_VEIN);
		AddAdvertisedAction(INTERACTION_TYPE.DIG);
		base.traitContainer.RemoveTrait(this, "Flammable");
	}

	public OreVein(SaveDataOreVein data)
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

	public override bool CanBeAffectedByElementalStatus(string traitName)
	{
		return false;
	}

	public override void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom, bool removeTraits = true, bool destroyTileSlots = true)
	{
		removedFrom.SetElevationTilemapTileAsset(null);
		removedFrom.SetTileType(LocationGridTile.Tile_Type.Empty);
		mapVisual.DestroyExistingGUS();
		base.OnRemoveTileObject(removedBy, removedFrom, removeTraits, destroyTileSlots);
		EvaluateAllOreVeinBlockWallNeighboursForDiagonalImpassables(removedFrom, p_includeCenterTile: false);
		removedFrom.UpdateMinimapVisual(removedFrom.structure);
	}

	protected override void OnPlaceTileObjectAtTile(LocationGridTile tile)
	{
		tile.SetElevationTilemapTileAsset(InnerMapManager.Instance.assetManager.GetWallAssetBasedOnWallType(WALL_TYPE.Stone));
		tile.SetTileType(LocationGridTile.Tile_Type.Wall);
		Vector2 size = new Vector2(0.5f, 0.5f);
		mapVisual.InitializeGUS(Vector2.zero, size, tile);
		mapVisual.UpdateTileObjectVisual(this);
		base.OnPlaceTileObjectAtTile(tile);
		EvaluateAllOreVeinBlockWallNeighboursForDiagonalImpassables(tile, p_includeCenterTile: true);
		tile.UpdateMinimapVisual(tile.structure);
	}

	public override void OnLoadPlacePOI()
	{
		base.OnLoadPlacePOI();
		if (mapVisual is OreVeinGameObject oreVeinGameObject)
		{
			oreVeinGameObject.leftBotImpassable.SetActive(leftBotImpassable);
			oreVeinGameObject.leftTopImpassable.SetActive(leftTopImpassable);
			oreVeinGameObject.topLeftImpassable.SetActive(topLeftImpassable);
			oreVeinGameObject.topRightImpassable.SetActive(topRightImpassable);
			oreVeinGameObject.rightTopImpassable.SetActive(rightTopImpassable);
			oreVeinGameObject.rightBotImpassable.SetActive(rightBotImpassable);
			oreVeinGameObject.botRightImpassable.SetActive(botRightImpassable);
			oreVeinGameObject.botLeftImpassable.SetActive(botLeftImpassable);
		}
	}

	public override void ConstructDefaultPlayerActions(bool broadcastSignal = true)
	{
		base.ConstructDefaultPlayerActions(broadcastSignal);
		RemovePlayerAction(PLAYER_SKILL_TYPE.SEIZE_OBJECT, broadcastSignal);
		RemovePlayerAction(PLAYER_SKILL_TYPE.POISON, broadcastSignal);
		RemovePlayerAction(PLAYER_SKILL_TYPE.IGNITE, broadcastSignal);
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

	public override void LoadAdditionalInfo(SaveDataTileObject data)
	{
		if (mapVisual != null)
		{
			mapVisual.UpdateTileObjectVisual(this);
		}
	}

	private void EvaluateAllOreVeinBlockWallNeighboursForDiagonalImpassables(LocationGridTile p_centerTile, bool p_includeCenterTile)
	{
		if (GameManager.Instance.gameHasStarted)
		{
			if (p_includeCenterTile)
			{
				(mapVisual as OreVeinGameObject).EvaluateImpassables(this);
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
}

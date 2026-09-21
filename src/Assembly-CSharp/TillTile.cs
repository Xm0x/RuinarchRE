using System;
using Inner_Maps;

public class TillTile : GoapAction
{
	public int m_amountProducedPerTick = 1;

	public TillTile()
		: base(INTERACTION_TYPE.TILL_TILE)
	{
		base.actionIconString = GoapActionStateDB.Harvest_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Till Tile Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		return base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
	}

	public void AfterTillTileSuccess(ActualGoapNode goapNode)
	{
		LocationGridTile gridTileLocation = goapNode.target.gridTileLocation;
		if (gridTileLocation.tileObjectComponent.objHere != null)
		{
			gridTileLocation.structure.RemovePOI(gridTileLocation.tileObjectComponent.objHere);
		}
		TILE_OBJECT_TYPE cropToCreate = GetCropToCreate(gridTileLocation);
		gridTileLocation.structure.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(cropToCreate), gridTileLocation);
	}

	private TILE_OBJECT_TYPE GetCropToCreate(LocationGridTile p_tile)
	{
		if (p_tile.specificBiomeTileType == Biome_Tile_Type.Grassland)
		{
			return TILE_OBJECT_TYPE.CORN_CROP;
		}
		if (p_tile.specificBiomeTileType == Biome_Tile_Type.Jungle)
		{
			return TILE_OBJECT_TYPE.POTATO_CROP;
		}
		if (p_tile.specificBiomeTileType == Biome_Tile_Type.Desert || p_tile.specificBiomeTileType == Biome_Tile_Type.Oasis)
		{
			return TILE_OBJECT_TYPE.PINEAPPLE_CROP;
		}
		if (p_tile.specificBiomeTileType == Biome_Tile_Type.Snow || p_tile.specificBiomeTileType == Biome_Tile_Type.Taiga || p_tile.specificBiomeTileType == Biome_Tile_Type.Tundra)
		{
			return TILE_OBJECT_TYPE.ICEBERRY_CROP;
		}
		throw new Exception("No crop production case for " + p_tile.specificBiomeTileType);
	}
}

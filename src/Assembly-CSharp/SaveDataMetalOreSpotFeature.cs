using Inner_Maps;
using Inner_Maps.Grid_Tile_Features;

public class SaveDataMetalOreSpotFeature : SaveDataGridTileFeature
{
	public TileLocationSave[] unoccupiedTiles;

	public override void Save(GridTileFeature p_data)
	{
		base.Save(p_data);
		MetalOreSpotFeature metalOreSpotFeature = p_data as MetalOreSpotFeature;
		unoccupiedTiles = new TileLocationSave[metalOreSpotFeature.unoccupiedSpots.Count];
		for (int i = 0; i < metalOreSpotFeature.unoccupiedSpots.Count; i++)
		{
			LocationGridTile locationGridTile = metalOreSpotFeature.unoccupiedSpots[i];
			unoccupiedTiles[i] = new TileLocationSave(locationGridTile);
		}
	}

	public override GridTileFeature Load()
	{
		return new MetalOreSpotFeature(this);
	}
}

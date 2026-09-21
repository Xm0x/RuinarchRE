using Inner_Maps;
using Inner_Maps.Grid_Tile_Features;

public class SaveDataBigTreeSpotFeature : SaveDataGridTileFeature
{
	public TileLocationSave[] unoccupiedTiles;

	public override void Save(GridTileFeature p_data)
	{
		base.Save(p_data);
		BigTreeSpotFeature bigTreeSpotFeature = p_data as BigTreeSpotFeature;
		unoccupiedTiles = new TileLocationSave[bigTreeSpotFeature.unoccupiedSpots.Count];
		for (int i = 0; i < bigTreeSpotFeature.unoccupiedSpots.Count; i++)
		{
			LocationGridTile locationGridTile = bigTreeSpotFeature.unoccupiedSpots[i];
			unoccupiedTiles[i] = new TileLocationSave(locationGridTile);
		}
	}

	public override GridTileFeature Load()
	{
		return new BigTreeSpotFeature(this);
	}
}

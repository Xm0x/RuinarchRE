using Inner_Maps;
using Inner_Maps.Grid_Tile_Features;

public class SaveDataStoneSpotFeature : SaveDataGridTileFeature
{
	public TileLocationSave[] unoccupiedTiles;

	public override void Save(GridTileFeature p_data)
	{
		base.Save(p_data);
		StoneSpotFeature stoneSpotFeature = p_data as StoneSpotFeature;
		unoccupiedTiles = new TileLocationSave[stoneSpotFeature.unoccupiedSpots.Count];
		for (int i = 0; i < stoneSpotFeature.unoccupiedSpots.Count; i++)
		{
			LocationGridTile locationGridTile = stoneSpotFeature.unoccupiedSpots[i];
			unoccupiedTiles[i] = new TileLocationSave(locationGridTile);
		}
	}

	public override GridTileFeature Load()
	{
		return new StoneSpotFeature(this);
	}
}

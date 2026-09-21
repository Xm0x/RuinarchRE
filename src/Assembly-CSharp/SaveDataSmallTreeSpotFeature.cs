using Inner_Maps;
using Inner_Maps.Grid_Tile_Features;

public class SaveDataSmallTreeSpotFeature : SaveDataGridTileFeature
{
	public TileLocationSave[] unoccupiedTiles;

	public override void Save(GridTileFeature p_data)
	{
		base.Save(p_data);
		SmallTreeSpotFeature smallTreeSpotFeature = p_data as SmallTreeSpotFeature;
		unoccupiedTiles = new TileLocationSave[smallTreeSpotFeature.unoccupiedSpots.Count];
		for (int i = 0; i < smallTreeSpotFeature.unoccupiedSpots.Count; i++)
		{
			LocationGridTile locationGridTile = smallTreeSpotFeature.unoccupiedSpots[i];
			unoccupiedTiles[i] = new TileLocationSave(locationGridTile);
		}
	}

	public override GridTileFeature Load()
	{
		return new SmallTreeSpotFeature(this);
	}
}

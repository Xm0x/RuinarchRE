using Inner_Maps;
using Inner_Maps.Grid_Tile_Features;

public class SaveDataMushroomSpotFeature : SaveDataGridTileFeature
{
	public TileLocationSave[] unoccupiedTiles;

	public override void Save(GridTileFeature p_data)
	{
		base.Save(p_data);
		MushroomSpotFeature mushroomSpotFeature = p_data as MushroomSpotFeature;
		unoccupiedTiles = new TileLocationSave[mushroomSpotFeature.unoccupiedSpots.Count];
		for (int i = 0; i < mushroomSpotFeature.unoccupiedSpots.Count; i++)
		{
			LocationGridTile locationGridTile = mushroomSpotFeature.unoccupiedSpots[i];
			unoccupiedTiles[i] = new TileLocationSave(locationGridTile);
		}
	}

	public override GridTileFeature Load()
	{
		return new MushroomSpotFeature(this);
	}
}

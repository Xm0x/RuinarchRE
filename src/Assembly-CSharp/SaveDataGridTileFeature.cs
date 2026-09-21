using Inner_Maps;
using Inner_Maps.Grid_Tile_Features;

public abstract class SaveDataGridTileFeature : SaveData<GridTileFeature>
{
	public TileLocationSave[] tiles;

	public override void Save(GridTileFeature data)
	{
		base.Save(data);
		tiles = new TileLocationSave[data.tilesWithFeature.Count];
		for (int i = 0; i < data.tilesWithFeature.Count; i++)
		{
			LocationGridTile locationGridTile = data.tilesWithFeature[i];
			tiles[i] = new TileLocationSave(locationGridTile);
		}
	}

	public override void CleanUp()
	{
		tiles = null;
	}
}

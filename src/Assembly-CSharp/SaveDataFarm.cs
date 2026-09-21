using Inner_Maps;
using Inner_Maps.Location_Structures;

public class SaveDataFarm : SaveDataManMadeStructure
{
	public TileLocationSave[] farmTiles;

	public override void Save(LocationStructure locationStructure)
	{
		base.Save(locationStructure);
		Farm farm = locationStructure as Farm;
		farmTiles = new TileLocationSave[farm.farmTiles.Count];
		for (int i = 0; i < farm.farmTiles.Count; i++)
		{
			LocationGridTile locationGridTile = farm.farmTiles[i];
			farmTiles[i] = new TileLocationSave(locationGridTile);
		}
	}
}

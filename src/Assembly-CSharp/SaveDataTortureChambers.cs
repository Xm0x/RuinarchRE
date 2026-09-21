using Inner_Maps;
using Inner_Maps.Location_Structures;

public class SaveDataTortureChambers : SaveDataDemonicStructure
{
	public TileLocationSave[] borderTiles;

	public override void Save(LocationStructure locationStructure)
	{
		base.Save(locationStructure);
		TortureChambers tortureChambers = locationStructure as TortureChambers;
		borderTiles = new TileLocationSave[tortureChambers.borderTiles.Count];
		for (int i = 0; i < tortureChambers.borderTiles.Count; i++)
		{
			LocationGridTile locationGridTile = tortureChambers.borderTiles[i];
			borderTiles[i] = new TileLocationSave(locationGridTile);
		}
	}
}

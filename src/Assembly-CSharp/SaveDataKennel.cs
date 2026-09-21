using Inner_Maps;
using Inner_Maps.Location_Structures;

public class SaveDataKennel : SaveDataDemonicStructure
{
	public string occupyingSummonID;

	public TileLocationSave[] borderTiles;

	public override void Save(LocationStructure structure)
	{
		base.Save(structure);
		Kennel kennel = structure as Kennel;
		if (kennel.occupyingSummon != null)
		{
			occupyingSummonID = kennel.occupyingSummon.persistentID;
		}
		borderTiles = new TileLocationSave[kennel.borderTiles.Count];
		for (int i = 0; i < kennel.borderTiles.Count; i++)
		{
			LocationGridTile locationGridTile = kennel.borderTiles[i];
			borderTiles[i] = new TileLocationSave(locationGridTile);
		}
	}
}

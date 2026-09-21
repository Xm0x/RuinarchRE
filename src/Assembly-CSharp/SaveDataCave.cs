using Inner_Maps;
using Inner_Maps.Location_Structures;

public class SaveDataCave : SaveDataNaturalStructure
{
	public CONCRETE_RESOURCES producedResource;

	public TileLocationSave[] stoneSpots;

	public TileLocationSave[] oreSpots;

	public TileLocationSave[] mushroomSpots;

	public string[] connectedMines;

	public override void Save(LocationStructure structure)
	{
		base.Save(structure);
		Cave cave = structure as Cave;
		producedResource = cave.producedResource;
		if (cave.oreSpots.Count > 0)
		{
			oreSpots = new TileLocationSave[cave.oreSpots.Count];
			for (int i = 0; i < oreSpots.Length; i++)
			{
				LocationGridTile locationGridTile = cave.oreSpots[i];
				oreSpots[i] = new TileLocationSave(locationGridTile);
			}
		}
		if (cave.stoneSpots.Count > 0)
		{
			stoneSpots = new TileLocationSave[cave.stoneSpots.Count];
			for (int j = 0; j < stoneSpots.Length; j++)
			{
				LocationGridTile locationGridTile2 = cave.stoneSpots[j];
				stoneSpots[j] = new TileLocationSave(locationGridTile2);
			}
		}
		if (cave.mushroomSpots.Count > 0)
		{
			mushroomSpots = new TileLocationSave[cave.mushroomSpots.Count];
			for (int k = 0; k < mushroomSpots.Length; k++)
			{
				LocationGridTile locationGridTile3 = cave.mushroomSpots[k];
				mushroomSpots[k] = new TileLocationSave(locationGridTile3);
			}
		}
		if (cave.connectedMines.Count > 0)
		{
			connectedMines = new string[cave.connectedMines.Count];
			for (int l = 0; l < cave.connectedMines.Count; l++)
			{
				LocationStructure locationStructure = cave.connectedMines[l];
				connectedMines[l] = locationStructure.persistentID;
			}
		}
	}
}

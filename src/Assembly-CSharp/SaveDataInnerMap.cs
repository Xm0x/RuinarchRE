using System.Collections.Generic;
using Inner_Maps;
using Perlin_Noise;

public class SaveDataInnerMap : SaveData<InnerTileMap>
{
	public float xSeed;

	public float ySeed;

	public PerlinNoiseSettings elevationPerlinNoiseSettings;

	public Dictionary<Point, SaveDataLocationGridTile> tileSaves;

	public float warpWeight;

	public float temperatureSeed;

	public override void Save(InnerTileMap innerTileMap)
	{
		xSeed = innerTileMap.xSeed;
		ySeed = innerTileMap.ySeed;
		elevationPerlinNoiseSettings = innerTileMap.elevationPerlinSettings;
		tileSaves = new Dictionary<Point, SaveDataLocationGridTile>();
		warpWeight = innerTileMap.warpWeight;
		temperatureSeed = innerTileMap.temperatureSeed;
		for (int i = 0; i < innerTileMap.width; i++)
		{
			for (int j = 0; j < innerTileMap.height; j++)
			{
				LocationGridTile locationGridTile = innerTileMap.map[i, j];
				if (!locationGridTile.isDefault)
				{
					SaveDataLocationGridTile saveDataLocationGridTile = new SaveDataLocationGridTile();
					saveDataLocationGridTile.Save(locationGridTile);
					tileSaves.Add(new Point(i, j), saveDataLocationGridTile);
				}
			}
		}
	}

	public SaveDataLocationGridTile GetSaveDataForTile(Point point)
	{
		if (tileSaves.ContainsKey(point))
		{
			return tileSaves[point];
		}
		return null;
	}

	public override void CleanUp()
	{
		if (tileSaves == null)
		{
			return;
		}
		foreach (KeyValuePair<Point, SaveDataLocationGridTile> tileSafe in tileSaves)
		{
			tileSafe.Value.CleanUp();
		}
		tileSaves.Clear();
		tileSaves = null;
	}
}

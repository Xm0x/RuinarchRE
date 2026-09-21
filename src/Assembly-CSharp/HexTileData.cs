using System;
using UnityEngine;

[Serializable]
public class HexTileData
{
	[Header("General Tile Details")]
	public int id;

	public string persistentID;

	public int xCoordinate;

	public int yCoordinate;

	public string tileName;

	[Space(10f)]
	[Header("Biome Settings")]
	public float elevationNoise;

	public float moistureNoise;

	public float temperature;

	public BIOMES biomeType;

	public ELEVATION elevationType;
}

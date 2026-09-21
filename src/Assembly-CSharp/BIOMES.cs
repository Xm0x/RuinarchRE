using UnityEngine;

public enum BIOMES
{
	GRASSLAND,
	SNOW,
	TUNDRA,
	DESERT,
	FOREST,
	BARE,
	NONE,
	ANCIENT_RUIN
}
public class Biomes : MonoBehaviour
{
	public static Biomes Instance;

	private void Awake()
	{
		Instance = this;
	}

	private ELEVATION GetElevationType(float elevationNoise)
	{
		if (elevationNoise <= 0.2f)
		{
			return ELEVATION.WATER;
		}
		if (elevationNoise > 0.2f && elevationNoise <= 0.39f)
		{
			return ELEVATION.TREES;
		}
		if (elevationNoise > 0.39f && elevationNoise <= 0.7f)
		{
			return ELEVATION.PLAIN;
		}
		return ELEVATION.MOUNTAIN;
	}
}

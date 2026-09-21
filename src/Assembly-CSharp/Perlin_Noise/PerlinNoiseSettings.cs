using System;
using UnityEngine;

namespace Perlin_Noise;

[Serializable]
public struct PerlinNoiseSettings
{
	public float noiseScale;

	public int octaves;

	[Range(0f, 1f)]
	public float persistance;

	public float lacunarity;

	public int seed;

	public Vector2 offset;

	public PerlinNoiseRegion[] regions;

	public PerlinNoiseRegion GetPerlinNoiseRegion(float height)
	{
		for (int i = 0; i < regions.Length; i++)
		{
			PerlinNoiseRegion result = regions[i];
			if (height <= result.height)
			{
				return result;
			}
		}
		throw new Exception("Could not find region for height " + height);
	}
}

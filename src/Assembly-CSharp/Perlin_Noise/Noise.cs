using System;
using UnityEngine;

namespace Perlin_Noise;

public static class Noise
{
	public static float[,] GenerateNoiseMap(PerlinNoiseSettings p_settings, int p_width, int p_height)
	{
		float[,] array = new float[p_width, p_height];
		System.Random random = new System.Random(p_settings.seed);
		Vector2[] array2 = new Vector2[p_settings.octaves];
		for (int i = 0; i < p_settings.octaves; i++)
		{
			float x = (float)random.Next(-100000, 100000) + p_settings.offset.x;
			float y = (float)random.Next(-100000, 100000) + p_settings.offset.y;
			array2[i] = new Vector2(x, y);
		}
		if (p_settings.noiseScale <= 0f)
		{
			p_settings.noiseScale = 0.0001f;
		}
		float num = float.MinValue;
		float num2 = float.MaxValue;
		float num3 = (float)p_width / 2f;
		float num4 = (float)p_height / 2f;
		for (int j = 0; j < p_height; j++)
		{
			for (int k = 0; k < p_width; k++)
			{
				float num5 = 1f;
				float num6 = 1f;
				float num7 = 0f;
				for (int l = 0; l < p_settings.octaves; l++)
				{
					float x2 = ((float)k - num3) / p_settings.noiseScale * num6 + array2[l].x;
					float y2 = ((float)j - num4) / p_settings.noiseScale * num6 + array2[l].y;
					num7 += (array[k, j] = Mathf.PerlinNoise(x2, y2) * 2f - 1f) * num5;
					num5 *= p_settings.persistance;
					num6 *= p_settings.lacunarity;
				}
				if (num7 > num)
				{
					num = num7;
				}
				else if (num7 < num2)
				{
					num2 = num7;
				}
				array[k, j] = num7;
			}
		}
		for (int m = 0; m < p_height; m++)
		{
			for (int n = 0; n < p_width; n++)
			{
				array[n, m] = Mathf.InverseLerp(num2, num, array[n, m]);
			}
		}
		return array;
	}

	public static float[,] GenerateTemperatureGradient(int p_width, int p_height, Gradient_Direction gradientDirection, float warpNoiseScale, float warpSeed, float warpStrength, float warpWeight, float seed)
	{
		float[,] array = new float[p_width, p_height];
		for (int i = 0; i < p_width; i++)
		{
			for (int j = 0; j < p_height; j++)
			{
				float graidentValue = GetGraidentValue(new Vector2(i, j), p_width, p_height, gradientDirection, seed);
				graidentValue += DistortedNoise((float)i / (float)p_width * warpNoiseScale + warpSeed, (float)j / (float)p_height * warpNoiseScale + warpSeed, warpStrength) * warpWeight;
				array[i, j] = graidentValue;
			}
		}
		return array;
	}

	private static float GetGraidentValue(Vector2 p, int p_width, int p_height, Gradient_Direction gradientDirection, float random)
	{
		float num;
		float num2;
		switch (gradientDirection)
		{
		case Gradient_Direction.Top:
			num = (float)p_height - p.y;
			num2 = p_height;
			break;
		case Gradient_Direction.Bottom:
			num = p.y;
			num2 = p_height;
			break;
		case Gradient_Direction.Left:
			num = p.x;
			num2 = p_width;
			break;
		case Gradient_Direction.Right:
			num = (float)p_width - p.x;
			num2 = p_width;
			break;
		default:
			throw new ArgumentOutOfRangeException("gradientDirection", gradientDirection, null);
		}
		return num / num2 * 0.75f + random;
	}

	private static float DistortedNoise(float x, float y, float distortionStrength)
	{
		float num = distortionStrength * Distort(x + 2.3f, y + 2.9f);
		float num2 = distortionStrength * Distort(x - 2.1f, y - 2.3f);
		return Mathf.PerlinNoise(x + num, y + num2);
	}

	private static float Distort(float x, float y)
	{
		float num = 3.7f;
		return Mathf.PerlinNoise(x * num, y * num);
	}
}

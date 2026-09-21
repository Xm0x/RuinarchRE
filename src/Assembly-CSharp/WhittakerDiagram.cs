using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WhittakerDiagram
{
	[Header("Precipitation")]
	[Tooltip("The scale at which to take all precipitation values. Default is 0 - 1")]
	public Range totalPrecipitationRange = new Range(0f, 1f);

	public PrecipitationTypeDictionary precipitationTypeTable;

	[Header("Temperature")]
	[Tooltip("The scale at which to take all temperature values. Default is 0 - 1")]
	public Range totalTemperatureRange = new Range(0f, 1f);

	public TemperatureTypeDictionary temperatureTypeTable;

	private Biome_Tile_Type[,] tileTypeTable = new Biome_Tile_Type[6, 6]
	{
		{
			Biome_Tile_Type.Taiga,
			Biome_Tile_Type.Grassland,
			Biome_Tile_Type.Grassland,
			Biome_Tile_Type.Grassland,
			Biome_Tile_Type.Desert,
			Biome_Tile_Type.Desert
		},
		{
			Biome_Tile_Type.Snow,
			Biome_Tile_Type.Tundra,
			Biome_Tile_Type.Grassland,
			Biome_Tile_Type.Grassland,
			Biome_Tile_Type.Oasis,
			Biome_Tile_Type.Desert
		},
		{
			Biome_Tile_Type.Snow,
			Biome_Tile_Type.Tundra,
			Biome_Tile_Type.Grassland,
			Biome_Tile_Type.Grassland,
			Biome_Tile_Type.Grassland,
			Biome_Tile_Type.Oasis
		},
		{
			Biome_Tile_Type.Snow,
			Biome_Tile_Type.Tundra,
			Biome_Tile_Type.Jungle,
			Biome_Tile_Type.Grassland,
			Biome_Tile_Type.Grassland,
			Biome_Tile_Type.Oasis
		},
		{
			Biome_Tile_Type.Snow,
			Biome_Tile_Type.Snow,
			Biome_Tile_Type.Tundra,
			Biome_Tile_Type.Jungle,
			Biome_Tile_Type.Grassland,
			Biome_Tile_Type.Grassland
		},
		{
			Biome_Tile_Type.Snow,
			Biome_Tile_Type.Snow,
			Biome_Tile_Type.Taiga,
			Biome_Tile_Type.Jungle,
			Biome_Tile_Type.Jungle,
			Biome_Tile_Type.Grassland
		}
	};

	public Biome_Tile_Type GetTileType(float precipitation, float temperature)
	{
		Temperature_Type temperatureType = GetTemperatureType(temperature);
		Precipitation_Type precipitationType = GetPrecipitationType(precipitation);
		return tileTypeTable[(int)precipitationType, (int)temperatureType];
	}

	public Temperature_Type GetTemperatureType(float temperature)
	{
		float value = Mathf.Lerp(totalTemperatureRange.minimum, totalTemperatureRange.maximum, temperature);
		foreach (KeyValuePair<Temperature_Type, Range> item in temperatureTypeTable)
		{
			if (item.Value.IsInRange(value))
			{
				return item.Key;
			}
		}
		throw new Exception("Could not find Temperature type for " + value);
	}

	public Precipitation_Type GetPrecipitationType(float precipitation)
	{
		float value = Mathf.Lerp(totalPrecipitationRange.minimum, totalPrecipitationRange.maximum, precipitation);
		foreach (KeyValuePair<Precipitation_Type, Range> item in precipitationTypeTable)
		{
			if (item.Value.IsInRange(value))
			{
				return item.Key;
			}
		}
		throw new Exception("Could not find Precipitation type for " + value);
	}
}

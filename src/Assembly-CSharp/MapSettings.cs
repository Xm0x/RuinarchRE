using System;
using UnityEngine;
using UtilityScripts;

[Serializable]
public class MapSettings
{
	public MAP_SIZE mapSize;

	public MapSettings()
	{
		mapSize = MAP_SIZE.Small;
	}

	public void SetMapSize(MAP_SIZE p_mapSize)
	{
		mapSize = p_mapSize;
	}

	public Vector2 GetMapSize()
	{
		return mapSize switch
		{
			MAP_SIZE.Small => new Vector2(10f, 10f), 
			MAP_SIZE.Medium => new Vector2(16f, 10f), 
			MAP_SIZE.Large => new Vector2(20f, 12f), 
			MAP_SIZE.Extra_Large => new Vector2(24f, 14f), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public int GetMaxStartingFactions()
	{
		return mapSize switch
		{
			MAP_SIZE.Small => 1, 
			MAP_SIZE.Medium => 2, 
			MAP_SIZE.Large => 2, 
			MAP_SIZE.Extra_Large => 2, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public int GetMaxStartingVillages()
	{
		return mapSize switch
		{
			MAP_SIZE.Small => 1, 
			MAP_SIZE.Medium => 2, 
			MAP_SIZE.Large => 3, 
			MAP_SIZE.Extra_Large => 4, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public int GetMaxVillagesForMapSize()
	{
		return mapSize switch
		{
			MAP_SIZE.Small => 6, 
			MAP_SIZE.Medium => 8, 
			MAP_SIZE.Large => 12, 
			MAP_SIZE.Extra_Large => 14, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public int GetMaxActiveFactionsForMapSize()
	{
		return mapSize switch
		{
			MAP_SIZE.Small => 2, 
			MAP_SIZE.Medium => 3, 
			MAP_SIZE.Large => 4, 
			MAP_SIZE.Extra_Large => 6, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public int GetSpecialStructuresToCreate()
	{
		return mapSize switch
		{
			MAP_SIZE.Small => GameUtilities.RandomBetweenTwoNumbers(2, 4), 
			MAP_SIZE.Medium => GameUtilities.RandomBetweenTwoNumbers(4, 6), 
			MAP_SIZE.Large => GameUtilities.RandomBetweenTwoNumbers(5, 7), 
			MAP_SIZE.Extra_Large => GameUtilities.RandomBetweenTwoNumbers(6, 8), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}

using System;
using UtilityScripts;

[Serializable]
public struct VillageSetting
{
	public string villageName;

	public VILLAGE_SIZE villageSize;

	public static VillageSetting Default => new VillageSetting
	{
		villageName = RandomNameGenerator.GenerateSettlementName(RACE.HUMANS),
		villageSize = VILLAGE_SIZE.Small
	};

	public int GetTileCountReservedForVillage(MAP_SIZE p_mapSize)
	{
		switch (villageSize)
		{
		case VILLAGE_SIZE.Small:
			return 2;
		case VILLAGE_SIZE.Medium:
			if (p_mapSize == MAP_SIZE.Small)
			{
				return 2;
			}
			return 3;
		case VILLAGE_SIZE.Large:
			if (p_mapSize == MAP_SIZE.Small)
			{
				return 2;
			}
			return 3;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public int GetRandomDwellingCount()
	{
		return villageSize switch
		{
			VILLAGE_SIZE.Small => Utilities.Rng.Next(5, 7), 
			VILLAGE_SIZE.Medium => Utilities.Rng.Next(9, 11), 
			VILLAGE_SIZE.Large => Utilities.Rng.Next(13, 15), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public int GetFacilityCount()
	{
		return villageSize switch
		{
			VILLAGE_SIZE.Small => 2, 
			VILLAGE_SIZE.Medium => 4, 
			VILLAGE_SIZE.Large => 6, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public int GetFoodProducingStructureCount()
	{
		return villageSize switch
		{
			VILLAGE_SIZE.Small => 1, 
			VILLAGE_SIZE.Medium => 2, 
			VILLAGE_SIZE.Large => 2, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public int GetBasicResourceProducingStructureCount()
	{
		return villageSize switch
		{
			VILLAGE_SIZE.Small => 1, 
			VILLAGE_SIZE.Medium => 1, 
			VILLAGE_SIZE.Large => 1, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public int GetSpecialStructureCount()
	{
		return villageSize switch
		{
			VILLAGE_SIZE.Small => 0, 
			VILLAGE_SIZE.Medium => 1, 
			VILLAGE_SIZE.Large => 2, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}

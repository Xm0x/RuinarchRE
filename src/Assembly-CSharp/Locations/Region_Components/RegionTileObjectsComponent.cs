using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UtilityScripts;

namespace Locations.Region_Components;

public class RegionTileObjectsComponent
{
	public List<STRUCTURE_TYPE> possibleStructureScrollChoices { get; }

	public Dictionary<TILE_OBJECT_TYPE, int> objectsInRegionCount { get; private set; }

	public RegionTileObjectsComponent()
	{
		possibleStructureScrollChoices = new List<STRUCTURE_TYPE>
		{
			STRUCTURE_TYPE.LIGHTNING_TOWER,
			STRUCTURE_TYPE.ARROW_TOWER,
			STRUCTURE_TYPE.WYVERN_COOP,
			STRUCTURE_TYPE.BEAST_PEN
		};
		objectsInRegionCount = new Dictionary<TILE_OBJECT_TYPE, int>();
	}

	public RegionTileObjectsComponent(SaveDataRegionTileObjectsComponent p_data)
	{
		possibleStructureScrollChoices = new List<STRUCTURE_TYPE>(p_data.possibleStructureScrollChoices);
		objectsInRegionCount = new Dictionary<TILE_OBJECT_TYPE, int>();
	}

	public STRUCTURE_TYPE GetRandomStructureTypeForStructureScroll()
	{
		return CollectionUtilities.GetRandomElement(possibleStructureScrollChoices);
	}

	public void RemoveStructureFromStructureScrollChoices(STRUCTURE_TYPE p_structure)
	{
		possibleStructureScrollChoices.Remove(p_structure);
	}

	public bool HasStructureThatCanBeLearned()
	{
		return possibleStructureScrollChoices.Count > 0;
	}

	public void AddTileObjectInRegion(TileObject tileObject)
	{
		if (!objectsInRegionCount.ContainsKey(tileObject.tileObjectType))
		{
			objectsInRegionCount.Add(tileObject.tileObjectType, 0);
		}
		objectsInRegionCount[tileObject.tileObjectType]++;
	}

	public void RemoveTileObjectInRegion(TileObject tileObject)
	{
		if (objectsInRegionCount.ContainsKey(tileObject.tileObjectType))
		{
			objectsInRegionCount[tileObject.tileObjectType]--;
			if (objectsInRegionCount[tileObject.tileObjectType] <= 0)
			{
				objectsInRegionCount.Remove(tileObject.tileObjectType);
			}
		}
	}

	public int GetTileObjectInRegionCount(TILE_OBJECT_TYPE tileObjectType)
	{
		if (objectsInRegionCount.ContainsKey(tileObjectType))
		{
			return objectsInRegionCount[tileObjectType];
		}
		return 0;
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}
}

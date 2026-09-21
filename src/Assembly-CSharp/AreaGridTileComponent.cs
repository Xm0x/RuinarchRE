using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

public class AreaGridTileComponent : AreaComponent
{
	public LocationGridTile centerGridTile { get; private set; }

	public List<LocationGridTile> gridTiles { get; private set; }

	public List<LocationGridTile> borderTiles { get; private set; }

	public List<LocationGridTile> passableTiles { get; }

	public AreaGridTileComponent()
	{
		gridTiles = new List<LocationGridTile>();
		borderTiles = new List<LocationGridTile>();
		passableTiles = new List<LocationGridTile>();
	}

	public void SetCenterGridTile(LocationGridTile p_gridTile)
	{
		centerGridTile = p_gridTile;
	}

	public void EvaluatePassabilityOfTile(LocationGridTile p_tile)
	{
		if (p_tile.IsPassable())
		{
			AddPassableTile(p_tile);
		}
		else
		{
			RemovePassableTile(p_tile);
		}
	}

	public void AddGridTile(LocationGridTile p_gridTile)
	{
		gridTiles.Add(p_gridTile);
		if (p_gridTile.IsPassable())
		{
			AddPassableTile(p_gridTile);
		}
	}

	public void PopulateBorderTiles(Area p_area)
	{
		borderTiles.Clear();
		InnerTileMap innerMap = p_area.region.innerMap;
		int x = InnerMapManager.AreaLocationGridTileSize.x;
		int y = InnerMapManager.AreaLocationGridTileSize.y;
		int num = gridTiles.Min((LocationGridTile t) => t.localPlace.x);
		int num2 = gridTiles.Min((LocationGridTile t) => t.localPlace.y);
		int num3 = num2 + (y - 1);
		int num4 = num + (x - 1);
		for (int num5 = num2; num5 < num3; num5++)
		{
			borderTiles.Add(innerMap.map[num, num5]);
		}
		for (int num6 = num; num6 < num4; num6++)
		{
			borderTiles.Add(innerMap.map[num6, num3]);
		}
		for (int num7 = num2; num7 <= num3; num7++)
		{
			borderTiles.Add(innerMap.map[num4, num7]);
		}
		for (int num8 = num + 1; num8 < num4; num8++)
		{
			borderTiles.Add(innerMap.map[num8, num2]);
		}
	}

	public void AddPassableTile(LocationGridTile p_tile)
	{
		if (!passableTiles.Contains(p_tile))
		{
			passableTiles.Add(p_tile);
		}
	}

	public void RemovePassableTile(LocationGridTile p_tile)
	{
		passableTiles.Remove(p_tile);
	}

	public LocationGridTile GetRandomTile()
	{
		return gridTiles[Random.Range(0, gridTiles.Count)];
	}

	public LocationGridTile GetRandomTileThatIsPassableAndOpenSpace()
	{
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		for (int i = 0; i < passableTiles.Count; i++)
		{
			LocationGridTile locationGridTile = passableTiles[i];
			if (locationGridTile.structure.structureType.IsOpenSpace())
			{
				list.Add(locationGridTile);
			}
		}
		LocationGridTile result = null;
		if (list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<LocationGridTile>.Release(list);
		return result;
	}

	public LocationGridTile GetRandomTileThatIsPassableAndHasNoObjectAndIsNotInStructure(LocationStructure p_structure)
	{
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		for (int i = 0; i < passableTiles.Count; i++)
		{
			LocationGridTile locationGridTile = passableTiles[i];
			if (locationGridTile.tileObjectComponent.objHere == null && locationGridTile.structure != p_structure)
			{
				list.Add(locationGridTile);
			}
		}
		LocationGridTile result = null;
		if (list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<LocationGridTile>.Release(list);
		return result;
	}

	public LocationGridTile GetRandomTileThatIsPassableAndHasNoObjectAndIsInWilderness()
	{
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		for (int i = 0; i < passableTiles.Count; i++)
		{
			LocationGridTile locationGridTile = passableTiles[i];
			if (locationGridTile.tileObjectComponent.objHere == null && locationGridTile.structure is Wilderness)
			{
				list.Add(locationGridTile);
			}
		}
		LocationGridTile result = null;
		if (list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<LocationGridTile>.Release(list);
		return result;
	}

	public LocationGridTile GetRandomTileThatIsPassableAndIsNotInVillageStructure()
	{
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		for (int i = 0; i < passableTiles.Count; i++)
		{
			LocationGridTile locationGridTile = passableTiles[i];
			if (!locationGridTile.structure.structureType.IsVillageStructure())
			{
				list.Add(locationGridTile);
			}
		}
		LocationGridTile result = null;
		if (list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<LocationGridTile>.Release(list);
		return result;
	}

	public LocationGridTile GetRandomTileThatIsPassableAndIsInWildernessAndHasPathTo(Character p_character)
	{
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		for (int i = 0; i < passableTiles.Count; i++)
		{
			LocationGridTile locationGridTile = passableTiles[i];
			if (locationGridTile.structure is Wilderness && p_character.movementComponent.HasPathToEvenIfDiffRegion(locationGridTile))
			{
				list.Add(locationGridTile);
			}
		}
		LocationGridTile result = null;
		if (list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<LocationGridTile>.Release(list);
		return result;
	}

	public LocationGridTile GetRandomPassableTile()
	{
		return CollectionUtilities.GetRandomElement(passableTiles);
	}

	public LocationGridTile GetFirstPassableWithWalkableNodeTile()
	{
		for (int i = 0; i < gridTiles.Count; i++)
		{
			LocationGridTile locationGridTile = gridTiles[i];
			if (locationGridTile.IsPassable() && locationGridTile.HasWalkableNode())
			{
				return locationGridTile;
			}
		}
		return null;
	}

	public LocationGridTile GetRandomPassableTileThatIsNotOccupied()
	{
		if (passableTiles.Count <= 0)
		{
			return null;
		}
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		for (int i = 0; i < passableTiles.Count; i++)
		{
			LocationGridTile locationGridTile = passableTiles[i];
			if (!locationGridTile.isOccupied && locationGridTile.tileObjectComponent.objHere == null)
			{
				list.Add(locationGridTile);
			}
		}
		LocationGridTile result = null;
		if (list.Count > 0)
		{
			result = list[Utilities.Rng.Next(0, list.Count)];
		}
		RuinarchListPool<LocationGridTile>.Release(list);
		return result;
	}

	public LocationGridTile GetRandomPassableTileThatIsNotPartOfAStructure()
	{
		LocationGridTile result = null;
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		for (int i = 0; i < passableTiles.Count; i++)
		{
			LocationGridTile locationGridTile = passableTiles[i];
			if (locationGridTile.structure.structureType == STRUCTURE_TYPE.WILDERNESS)
			{
				list.Add(locationGridTile);
			}
		}
		if (list != null && list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<LocationGridTile>.Release(list);
		return result;
	}

	public LocationGridTile GetRandomPassableTileThatIsCorruptedWithPathTo(Character character)
	{
		LocationGridTile result = null;
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		for (int i = 0; i < passableTiles.Count; i++)
		{
			LocationGridTile locationGridTile = passableTiles[i];
			if (locationGridTile.corruptionComponent.isCorrupted && character.movementComponent.HasPathToEvenIfDiffRegion(locationGridTile))
			{
				list.Add(locationGridTile);
			}
		}
		if (list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<LocationGridTile>.Release(list);
		return result;
	}

	public LocationGridTile GetRandomCorruptedTile()
	{
		LocationGridTile result = null;
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		for (int i = 0; i < gridTiles.Count; i++)
		{
			LocationGridTile locationGridTile = gridTiles[i];
			if (locationGridTile.corruptionComponent.isCorrupted)
			{
				list.Add(locationGridTile);
			}
		}
		if (list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<LocationGridTile>.Release(list);
		return result;
	}

	public LocationGridTile GetRandomUnoccupiedNoFreezingTrap()
	{
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		for (int i = 0; i < gridTiles.Count; i++)
		{
			LocationGridTile locationGridTile = gridTiles[i];
			if (!locationGridTile.tileObjectComponent.hasFreezingTrap && !locationGridTile.isOccupied)
			{
				list.Add(locationGridTile);
			}
		}
		LocationGridTile result = null;
		if (list != null && list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<LocationGridTile>.Release(list);
		return result;
	}

	public LocationGridTile GetRandomPassableUnoccupiedNonWaterTile()
	{
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		for (int i = 0; i < passableTiles.Count; i++)
		{
			LocationGridTile locationGridTile = passableTiles[i];
			if (locationGridTile.tileObjectComponent.objHere == null && locationGridTile.elevationType != ELEVATION.WATER)
			{
				list.Add(locationGridTile);
			}
		}
		LocationGridTile result = null;
		if (list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<LocationGridTile>.Release(list);
		return result;
	}

	public LocationGridTile GetRandomTileThatCharacterCanReach(Character p_character)
	{
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		LocationGridTile result = null;
		for (int i = 0; i < gridTiles.Count; i++)
		{
			LocationGridTile locationGridTile = gridTiles[i];
			if (p_character.movementComponent.HasPathToEvenIfDiffRegion(locationGridTile))
			{
				list.Add(locationGridTile);
			}
		}
		if (list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<LocationGridTile>.Release(list);
		return result;
	}

	public void PopulateUnoccupiedTiles(List<LocationGridTile> tiles)
	{
		for (int i = 0; i < gridTiles.Count; i++)
		{
			LocationGridTile locationGridTile = gridTiles[i];
			if (locationGridTile.tileObjectComponent.objHere == null)
			{
				tiles.Add(locationGridTile);
			}
		}
	}

	public bool HasCorruption()
	{
		for (int i = 0; i < gridTiles.Count; i++)
		{
			if (gridTiles[i].corruptionComponent.isCorrupted)
			{
				return true;
			}
		}
		return false;
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using PathFind;
using UnityEngine;

public class PathGenerator : MonoBehaviour
{
	public static PathGenerator Instance;

	private void Awake()
	{
		Instance = this;
	}

	public List<LocationGridTile> GetPath(LocationGridTile startingTile, LocationGridTile destinationTile, GRID_PATHFINDING_MODE pathMode = GRID_PATHFINDING_MODE.NORMAL, bool includeFirstTile = false)
	{
		List<LocationGridTile> list = null;
		Func<LocationGridTile, LocationGridTile, double> distance = (LocationGridTile node1, LocationGridTile node2) => 1.0;
		Func<LocationGridTile, double> estimate = (LocationGridTile t) => Math.Sqrt(Math.Pow(t.localPlace.x - destinationTile.localPlace.x, 2.0) + Math.Pow(t.localPlace.y - destinationTile.localPlace.y, 2.0));
		Path<LocationGridTile> path = global::PathFind.PathFind.FindPath(startingTile, destinationTile, distance, estimate, pathMode, null);
		if (path != null)
		{
			list = path.ToList();
		}
		if (list != null)
		{
			list.Reverse();
			if (!includeFirstTile)
			{
				list.RemoveAt(0);
			}
			return list;
		}
		return null;
	}

	private List<LocationGridTile> SameStructureTiles(LocationGridTile tile, params object[] args)
	{
		_ = args[0];
		_ = args[1];
		_ = args[2];
		List<LocationGridTile> result = new List<LocationGridTile>();
		List<LocationGridTile> list = tile.FourNeighbours();
		for (int i = 0; i < list.Count; i++)
		{
			_ = list[i];
		}
		return result;
	}

	private List<LocationGridTile> AllowedStructureTiles(LocationGridTile tile, params object[] args)
	{
		_ = args[0];
		_ = args[1];
		_ = args[2];
		List<LocationGridTile> result = new List<LocationGridTile>();
		List<LocationGridTile> list = tile.FourNeighbours();
		for (int i = 0; i < list.Count; i++)
		{
			_ = list[i];
		}
		return result;
	}
}

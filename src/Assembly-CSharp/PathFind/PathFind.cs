using System;
using System.Collections.Generic;
using Inner_Maps;
using UtilityScripts;

namespace PathFind;

public static class PathFind
{
	public static Path<Node> FindPath<Node>(Node start, Node destination, Func<Node, Node, double> distance, Func<Node, double> estimate, GRID_PATHFINDING_MODE pathMode, Func<Node, object[], List<Node>> tileGetFunction = null, params object[] args) where Node : LocationGridTile, IHasNeighbours<Node>
	{
		HashSet<Node> hashSet = new HashSet<Node>();
		PriorityQueue<double, Path<Node>> priorityQueue = new PriorityQueue<double, Path<Node>>();
		priorityQueue.Enqueue(0.0, new Path<Node>(start));
		while (!priorityQueue.IsEmpty)
		{
			Path<Node> path = priorityQueue.Dequeue();
			if (hashSet.Contains(path.LastStep))
			{
				continue;
			}
			if (path.LastStep.Equals(destination))
			{
				return path;
			}
			hashSet.Add(path.LastStep);
			_ = path.LastStep;
			if (tileGetFunction != null)
			{
				foreach (Node item in tileGetFunction(path.LastStep, args))
				{
					double stepCost = distance(path.LastStep, item);
					Path<Node> path2 = path.AddStep(item, stepCost);
					priorityQueue.Enqueue(path2.TotalCost + estimate(item), path2);
				}
				continue;
			}
			switch (pathMode)
			{
			case GRID_PATHFINDING_MODE.NORMAL:
			{
				List<LocationGridTile> list2 = RuinarchListPool<LocationGridTile>.Claim();
				path.LastStep.PopulateFourNeighboursValidTiles(list2);
				foreach (Node item2 in list2)
				{
					double stepCost = distance(path.LastStep, item2);
					Path<Node> path2 = path.AddStep(item2, stepCost);
					priorityQueue.Enqueue(path2.TotalCost + estimate(item2), path2);
				}
				RuinarchListPool<LocationGridTile>.Release(list2);
				continue;
			}
			case GRID_PATHFINDING_MODE.UNCONSTRAINED:
				foreach (Node item3 in path.LastStep.FourNeighbours())
				{
					double stepCost = distance(path.LastStep, item3);
					Path<Node> path2 = path.AddStep(item3, stepCost);
					priorityQueue.Enqueue(path2.TotalCost + estimate(item3), path2);
				}
				continue;
			case GRID_PATHFINDING_MODE.CAVE_INTERCONNECTION:
			{
				List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
				path.LastStep.PopulateNeighboursInSameStructureThatHasNoNeighbourInDifferentStructure(list);
				foreach (Node item4 in list)
				{
					double stepCost = distance(path.LastStep, item4);
					Path<Node> path2 = path.AddStep(item4, stepCost);
					priorityQueue.Enqueue(path2.TotalCost + estimate(item4), path2);
				}
				RuinarchListPool<LocationGridTile>.Release(list);
				continue;
			}
			}
			List<LocationGridTile> list3 = RuinarchListPool<LocationGridTile>.Claim();
			path.LastStep.PopulateFourNeighboursValidTiles(list3);
			foreach (Node item5 in list3)
			{
				double stepCost = distance(path.LastStep, item5);
				Path<Node> path2 = path.AddStep(item5, stepCost);
				priorityQueue.Enqueue(path2.TotalCost + estimate(item5), path2);
			}
			RuinarchListPool<LocationGridTile>.Release(list3);
		}
		return null;
	}
}

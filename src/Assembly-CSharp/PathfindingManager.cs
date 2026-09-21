using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using PathFinding;
using Pathfinding;
using UnityEngine;

public class PathfindingManager : BaseMonoBehaviour
{
	public static PathfindingManager Instance;

	private const float nodeSize = 0.5f;

	[SerializeField]
	private AstarPath aStarPath;

	private GridGraph mainGraph;

	private List<CharacterAIPath> _allAgents;

	public List<CharacterAIPath> allAgents => _allAgents;

	private void Awake()
	{
		Instance = this;
		_allAgents = new List<CharacterAIPath>(150);
	}

	private void Start()
	{
		Messenger.AddListener<bool>(UISignals.PAUSED, OnGamePaused);
	}

	private void FixedUpdate()
	{
		for (int i = 0; i < _allAgents.Count; i++)
		{
			_allAgents[i].marker.ManualUpdate();
		}
	}

	protected override void OnDestroy()
	{
		_allAgents.Clear();
		_allAgents = null;
		aStarPath = null;
		base.OnDestroy();
		Instance = null;
	}

	public void RescanGrid(GridGraph graph)
	{
		AstarPath.active.Scan(graph);
	}

	public void AddAgent(CharacterAIPath agent)
	{
		_allAgents.Add(agent);
	}

	public void RemoveAgent(CharacterAIPath agent)
	{
		_allAgents.Remove(agent);
	}

	public void UpdatePathfindingGraphPartialCoroutine(Bounds bounds)
	{
		StartCoroutine(UpdatePathfindingGraphPartial(bounds));
	}

	private IEnumerator UpdatePathfindingGraphPartial(Bounds bounds)
	{
		yield return null;
		AstarPath.active.UpdateGraphs(bounds);
	}

	public void UpdatePathfindingGraphPartialCoroutine(GraphUpdateObject guo)
	{
		StartCoroutine(UpdatePathfindingGraphPartial(guo));
	}

	private IEnumerator UpdatePathfindingGraphPartial(GraphUpdateObject guo)
	{
		yield return null;
		AstarPath.active.UpdateGraphs(guo);
	}

	public bool HasPath(LocationGridTile fromTile, LocationGridTile toTile)
	{
		if (fromTile == null || toTile == null)
		{
			return false;
		}
		if (fromTile == toTile)
		{
			return true;
		}
		Vector3 positionWithinTileThatIsOnAWalkableNode = fromTile.GetPositionWithinTileThatIsOnAWalkableNode();
		Vector3 positionWithinTileThatIsOnAWalkableNode2 = toTile.GetPositionWithinTileThatIsOnAWalkableNode();
		return HasPath(positionWithinTileThatIsOnAWalkableNode, positionWithinTileThatIsOnAWalkableNode2);
	}

	public bool HasPath(Vector3 fromPos, Vector3 toPos)
	{
		return HasPath(fromPos, toPos, GridMap.Instance.mainRegion.innerMap.onlyUnwalkableGraph);
	}

	public bool HasPath(Vector3 fromPos, Vector3 toPos, NNConstraint constraint)
	{
		if (fromPos.Equals(Vector3.positiveInfinity) || fromPos.Equals(Vector3.negativeInfinity) || toPos.Equals(Vector3.positiveInfinity) || toPos.Equals(Vector3.negativeInfinity))
		{
			return false;
		}
		if (fromPos.Equals(toPos))
		{
			return true;
		}
		return PathUtilities.IsPathPossible(AstarPath.active.GetNearest(fromPos, constraint).node, AstarPath.active.GetNearest(toPos, constraint).node);
	}

	public bool HasPathEvenDiffRegion(LocationGridTile fromTile, LocationGridTile toTile)
	{
		if (fromTile == null || toTile == null)
		{
			return false;
		}
		if (fromTile == toTile)
		{
			return true;
		}
		if (fromTile.structure == null)
		{
			return false;
		}
		if (toTile.structure == null)
		{
			return false;
		}
		Vector3 positionWithinTileThatIsOnAWalkableNode = fromTile.GetPositionWithinTileThatIsOnAWalkableNode();
		Vector3 positionWithinTileThatIsOnAWalkableNode2 = toTile.GetPositionWithinTileThatIsOnAWalkableNode();
		return HasPath(positionWithinTileThatIsOnAWalkableNode, positionWithinTileThatIsOnAWalkableNode2);
	}

	public bool HasPathEvenDiffRegion(LocationGridTile fromTile, LocationGridTile toTile, NNConstraint constraint)
	{
		if (fromTile == null || toTile == null)
		{
			return false;
		}
		if (fromTile == toTile)
		{
			return true;
		}
		Vector3 positionWithinTileThatIsOnAWalkableNode = fromTile.GetPositionWithinTileThatIsOnAWalkableNode();
		Vector3 positionWithinTileThatIsOnAWalkableNode2 = toTile.GetPositionWithinTileThatIsOnAWalkableNode();
		return HasPath(positionWithinTileThatIsOnAWalkableNode, positionWithinTileThatIsOnAWalkableNode2, constraint);
	}

	public void CreatePathfindingGraphForLocation(InnerTileMap newMap)
	{
		GridGraph pathfindingGraph = CreatePathfindingGraph(newMap, typeof(RuinarchGridGraph), newMap.region.name + " Main Map");
		newMap.pathfindingGraph = pathfindingGraph;
		GridGraph unwalkableGraph = CreatePathfindingGraph(newMap, typeof(GridGraph), newMap.region.name + " UnWalkable Map");
		newMap.unwalkableGraph = unwalkableGraph;
	}

	private GridGraph CreatePathfindingGraph(InnerTileMap newMap, Type graphType, string name)
	{
		GridGraph gridGraph = aStarPath.data.AddGraph(graphType) as GridGraph;
		gridGraph.name = name;
		gridGraph.cutCorners = false;
		gridGraph.rotation = new Vector3(-90f, 0f, 0f);
		gridGraph.nodeSize = 0.5f;
		int width = newMap.width;
		int height = newMap.height;
		gridGraph.SetDimensions(Mathf.FloorToInt((float)width / gridGraph.nodeSize), Mathf.FloorToInt((float)height / gridGraph.nodeSize), 0.5f);
		Vector3 position = InnerMapManager.Instance.transform.position;
		position.x += (float)newMap.width / 2f;
		position.y += (float)newMap.height / 2f + newMap.transform.localPosition.y;
		gridGraph.center = position;
		gridGraph.collision.use2D = true;
		gridGraph.collision.type = ColliderType.Sphere;
		gridGraph.collision.diameter = 0.9f;
		gridGraph.collision.mask = LayerMask.GetMask("Unpassable");
		return gridGraph;
	}

	private void OnGamePaused(bool state)
	{
		if (state)
		{
			for (int i = 0; i < _allAgents.Count; i++)
			{
				_allAgents[i].marker.PauseAnimation();
			}
		}
		else
		{
			for (int j = 0; j < _allAgents.Count; j++)
			{
				_allAgents[j].marker.UnpauseAnimation();
			}
		}
	}

	public void ApplyGraphUpdateSceneCoroutine(GraphUpdateScene gus)
	{
		StartCoroutine(UpdateGraph(gus));
	}

	private IEnumerator UpdateGraph(GraphUpdateScene gus)
	{
		yield return null;
		gus.Apply();
	}
}

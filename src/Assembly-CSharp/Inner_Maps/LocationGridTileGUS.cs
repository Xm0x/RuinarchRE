using System.Collections;
using EZObjectPools;
using Pathfinding;
using UnityEngine;
using UtilityScripts;

namespace Inner_Maps;

public class LocationGridTileGUS : PooledObject
{
	[SerializeField]
	private GraphUpdateScene gus;

	[SerializeField]
	private BoxCollider2D boxCollider;

	private InnerTileMap _innerTileMap;

	public void Initialize(Vector2 offset, Vector2 size, InnerTileMap innerTileMap)
	{
		boxCollider.offset = offset;
		boxCollider.size = size;
		_innerTileMap = innerTileMap;
		base.transform.localPosition = Vector3.zero;
		base.gameObject.SetActive(value: true);
		DelayedApply();
	}

	public void Destroy()
	{
		InstantApply();
		ObjectPoolManager.Instance.DestroyObject(this);
	}

	[ContextMenu("Apply")]
	public void InstantApply()
	{
		Apply();
	}

	[ContextMenu("Apply Coroutine")]
	private void DelayedApply()
	{
		StartCoroutine(UpdateGraph());
	}

	private IEnumerator UpdateGraph()
	{
		yield return GameUtilities.waitForHalfSecond;
		Apply();
	}

	private void Apply()
	{
		GraphUpdateObject guo = new GraphUpdateObject(boxCollider.bounds)
		{
			nnConstraint = _innerTileMap.onlyUnwalkableGraph
		};
		PathfindingManager.Instance.UpdatePathfindingGraphPartialCoroutine(guo);
		guo = new TagGraphUpdateObject(boxCollider.bounds)
		{
			nnConstraint = _innerTileMap.onlyPathfindingGraph,
			updatePhysics = true,
			modifyWalkability = false
		};
		PathfindingManager.Instance.UpdatePathfindingGraphPartialCoroutine(guo);
	}
}

using Pathfinding;
using UnityEngine;

namespace Inner_Maps;

public class TagGraphUpdateObject : GraphUpdateObject
{
	public TagGraphUpdateObject(Bounds bounds)
		: base(bounds)
	{
	}

	public override void Apply(GraphNode node)
	{
		base.Apply(node);
		if (!node.Walkable)
		{
			node.Tag = 1u;
			node.Walkable = true;
			return;
		}
		bool flag = false;
		uint tag = 0u;
		for (int i = 0; i < InnerMapManager.Instance.innerMaps.Count; i++)
		{
			InnerTileMap innerTileMap = InnerMapManager.Instance.innerMaps[i];
			Vector3 worldPosition = (Vector3)node.position;
			LocationGridTile tileFromWorldPosition = innerTileMap.GetTileFromWorldPosition(worldPosition);
			if (tileFromWorldPosition != null)
			{
				if (tileFromWorldPosition.area.HasSettlementVillageTypeWithFactionOwner(out var p_settlement))
				{
					flag = true;
					tag = p_settlement.owner.pathfindingTag;
				}
				break;
			}
		}
		if (flag)
		{
			node.Tag = tag;
		}
		else
		{
			node.Tag = 0u;
		}
	}
}

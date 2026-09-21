using Inner_Maps;
using Inner_Maps.Location_Structures;
using Pathfinding;
using Pathfinding.Serialization;
using Pathfinding.Util;
using UnityEngine;

namespace PathFinding;

[JsonOptIn]
[Preserve]
public class RuinarchGridGraph : GridGraph
{
	public override void RecalculateCell(int x, int z, bool resetPenalties = true, bool resetTags = true)
	{
		base.RecalculateCell(x, z, resetPenalties, resetTags);
		GridNode gridNode = nodes[z * width + x];
		gridNode.Walkable = true;
		if (collision.Check((Vector3)gridNode.position))
		{
			bool flag = false;
			uint tag = 0u;
			for (int i = 0; i < InnerMapManager.Instance.innerMaps.Count; i++)
			{
				InnerTileMap innerTileMap = InnerMapManager.Instance.innerMaps[i];
				Vector3 worldPosition = (Vector3)gridNode.position;
				LocationGridTile tileFromWorldPosition = innerTileMap.GetTileFromWorldPosition(worldPosition);
				if (tileFromWorldPosition != null)
				{
					if (tileFromWorldPosition.corruptionComponent.isCorrupted || tileFromWorldPosition.structure is DemonicStructure)
					{
						flag = true;
						tag = 3u;
						break;
					}
					if (tileFromWorldPosition.area.HasSettlementVillageTypeWithFactionOwner(out var p_settlement))
					{
						flag = true;
						tag = p_settlement.owner.pathfindingTag;
						break;
					}
				}
			}
			if (flag)
			{
				gridNode.Tag = tag;
			}
			else
			{
				gridNode.Tag = 0u;
			}
		}
		else
		{
			gridNode.Tag = 1u;
		}
	}
}

using Inner_Maps;
using Pathfinding;
using UnityEngine;
using UnityEngine.EventSystems;

public class AreaItem : BaseMonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private Collider2D boundsCollider;

	private InnerTileMap _innerTileMap;

	private int xCoordinate;

	private int yCoordinate;

	public void Initialize(InnerTileMap innerTileMap, int x_coord, int y_coord)
	{
		_innerTileMap = innerTileMap;
		xCoordinate = x_coord;
		yCoordinate = y_coord;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(base.transform.position, new Vector3((float)InnerMapManager.AreaLocationGridTileSize.x - 0.5f, (float)InnerMapManager.AreaLocationGridTileSize.y - 0.5f, 0f));
	}

	[ContextMenu("Update Pathfinding Graphs")]
	public void UpdatePathfindingGraph()
	{
		GraphUpdateObject guo = new TagGraphUpdateObject(boundsCollider.bounds)
		{
			nnConstraint = _innerTileMap.onlyPathfindingGraph,
			updatePhysics = true,
			modifyWalkability = false
		};
		PathfindingManager.Instance.UpdatePathfindingGraphPartialCoroutine(guo);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_innerTileMap = null;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (GridMap.Instance?.mainRegion == null || !GridMap.Instance.mainRegion.innerMap.perlinTilemap.gameObject.activeSelf)
		{
			return;
		}
		Area p_area = _innerTileMap.region.areaMap[xCoordinate, yCoordinate];
		VillageSpot coreVillageSpotOnArea = _innerTileMap.region.GetCoreVillageSpotOnArea(p_area);
		if (coreVillageSpotOnArea != null)
		{
			coreVillageSpotOnArea?.ColorWholeVillageSpot(Color.blue);
			string text = $"is Disabled: {coreVillageSpotOnArea.isDisabled}";
			text += "\nDisable votes:";
			for (int i = 0; i < coreVillageSpotOnArea.reservedAreas.Count; i++)
			{
				Area area = coreVillageSpotOnArea.reservedAreas[i];
				int num = coreVillageSpotOnArea.areaDisableVotes[i];
				text = text + "\n\t- " + area.locationName + ": " + num;
			}
			UIManager.Instance.ShowSmallInfo(text);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (GridMap.Instance?.mainRegion != null && GridMap.Instance.mainRegion.innerMap.perlinTilemap.gameObject.activeSelf)
		{
			Area p_area = _innerTileMap.region.areaMap[xCoordinate, yCoordinate];
			_innerTileMap.region.GetCoreVillageSpotOnArea(p_area)?.ResetColorOnWholeVillageSpot();
			UIManager.Instance.HideSmallInfo();
			for (int i = 0; i < GridMap.Instance.mainRegion.villageSpots.Count; i++)
			{
				GridMap.Instance.mainRegion.villageSpots[i].ColorCoreSpot();
			}
		}
	}
}

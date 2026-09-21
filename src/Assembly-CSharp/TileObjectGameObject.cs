using EZObjectPools;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;

public class TileObjectGameObject : MapObjectVisual<TileObject>
{
	[Header("Construction")]
	public GameObject constructionProgressPrefabGO;

	private ConstructionProgress _constructionProgress;

	public override void Initialize(TileObject tileObject)
	{
		base.Initialize(tileObject);
		base.name = tileObject.ToString();
		if (tileObject.gridTileLocation != null)
		{
			UpdateTileObjectVisual(tileObject);
		}
		else
		{
			SetVisual(InnerMapManager.Instance.GetTileObjectAsset(tileObject, tileObject.state));
		}
		UpdateSortingOrders(tileObject);
	}

	private void SetSortingOrder(int sortingOrder, string layerName = "Area Maps")
	{
		if (objectVisual != null)
		{
			objectVisual.sortingLayerName = layerName;
			objectVisual.sortingOrder = sortingOrder;
		}
	}

	public override void UpdateSortingOrders(TileObject obj)
	{
		if (obj.IsCurrentlySelected())
		{
			SetSortingOrder(900);
		}
		else if (obj.isBeingCarriedBy != null)
		{
			SetSortingOrder(obj.isBeingCarriedBy.marker.sortingOrder);
		}
		else if (obj.tileObjectType == TILE_OBJECT_TYPE.SMALL_TREE_OBJECT)
		{
			SetSortingOrder(45);
		}
		else if (obj.tileObjectType == TILE_OBJECT_TYPE.BIG_TREE_OBJECT)
		{
			SetSortingOrder(50);
		}
		else if (obj.tileObjectType == TILE_OBJECT_TYPE.MAGIC_CIRCLE || obj.tileObjectType == TILE_OBJECT_TYPE.RUG || obj.tileObjectType == TILE_OBJECT_TYPE.CARPET)
		{
			SetSortingOrder(39);
		}
		else if (obj.tileObjectType == TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT)
		{
			SetSortingOrder(12);
		}
		else if (obj is EquipmentItem)
		{
			SetSortingOrder(61);
		}
		else if (obj.tileObjectType == TILE_OBJECT_TYPE.POWER_CRYSTAL)
		{
			SetSortingOrder(62);
		}
		else
		{
			base.UpdateSortingOrders(obj);
		}
	}

	public override void UpdateTileObjectVisual(TileObject tileObject)
	{
		SetVisual(InnerMapManager.Instance.GetTileObjectAsset(tileObject, tileObject.state, tileObject.gridTileLocation.mainBiomeType, tileObject.gridTileLocation?.corruptionComponent.isCorrupted ?? false));
		tileObject.hiddenComponent.OnSetHiddenState(tileObject);
	}

	protected override void OnPointerLeftClick(TileObject poi)
	{
		base.OnPointerLeftClick(poi);
		UIManager.Instance.ShowTileObjectInfo(poi);
	}

	protected override void OnPointerRightClick(TileObject poi)
	{
		base.OnPointerRightClick(poi);
		_ = poi.gridTileLocation;
		UIManager.Instance.ShowPlayerActionContextMenu(poi, poi.worldPosition, p_isScreenPosition: false);
	}

	protected override void OnPointerMiddleClick(TileObject poi)
	{
		base.OnPointerMiddleClick(poi);
		_ = UIManager.Instance.characterInfoUI.activeCharacter ?? UIManager.Instance.monsterInfoUI.activeMonster;
	}

	protected override void OnPointerEnter(TileObject to)
	{
		if (to.mapObjectState != MAP_OBJECT_STATE.UNBUILT && to.CanBeSelected())
		{
			base.OnPointerEnter(to);
			if (objectVisual != null)
			{
				SetHoverObjectState(state: true);
			}
			InnerMapManager.Instance.SetCurrentlyHoveredPOI(to);
			InnerMapManager.Instance.ShowTileData(to.gridTileLocation);
			if (to is Tombstone { character: not null } tombstone && tombstone.character.hasMarker && (bool)tombstone.character.marker.nameplate)
			{
				tombstone.character.marker.nameplate.UpdateNameActiveState();
			}
		}
	}

	protected override void OnPointerExit(TileObject to)
	{
		if (to.mapObjectState != MAP_OBJECT_STATE.UNBUILT && to.CanBeSelected())
		{
			base.OnPointerExit(to);
			if (objectVisual != null)
			{
				SetHoverObjectState(state: false);
			}
			if (InnerMapManager.Instance.currentlyHoveredPoi == to)
			{
				InnerMapManager.Instance.SetCurrentlyHoveredPOI(null);
			}
			UIManager.Instance?.HideSmallInfo();
			if (to is Tombstone { character: not null } tombstone && tombstone.character.hasMarker && (bool)tombstone.character.marker.nameplate)
			{
				tombstone.character.marker.nameplate.UpdateNameActiveState();
			}
		}
	}

	public void ShowConstructionVisual()
	{
		GameObject gameObject = ObjectPoolManager.Instance.InstantiateObjectFromPool(constructionProgressPrefabGO.name, Vector3.zero, Quaternion.identity, base.transform);
		_constructionProgress = gameObject.GetComponent<ConstructionProgress>();
		gameObject.SetActive(value: true);
	}

	public void HideConstructionVisual()
	{
		if (_constructionProgress != null)
		{
			ObjectPoolManager.Instance.DestroyObject(_constructionProgress);
			_constructionProgress = null;
		}
	}

	public void SetConstructionProgress(float p_progress)
	{
		if (_constructionProgress != null)
		{
			_constructionProgress.SetConstructionProgress(p_progress);
		}
	}

	public override void Reset()
	{
		base.Reset();
		PooledObject[] componentsInDirectChildren = GameUtilities.GetComponentsInDirectChildren<PooledObject>(base.gameObject);
		foreach (PooledObject pooledObject in componentsInDirectChildren)
		{
			ObjectPoolManager.Instance.DestroyObject(pooledObject);
		}
		if (_constructionProgress != null)
		{
			ObjectPoolManager.Instance.DestroyObject(_constructionProgress);
			_constructionProgress = null;
		}
		base.obj = null;
	}
}

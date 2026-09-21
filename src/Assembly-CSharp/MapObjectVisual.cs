using UnityEngine;

public abstract class MapObjectVisual<T> : BaseMapObjectVisual where T : IDamageable
{
	protected T obj { get; set; }

	public virtual void Initialize(T obj)
	{
		Initialize(obj as ISelectable);
		this.obj = obj;
		onHoverOverAction = delegate
		{
			OnPointerEnter(obj);
		};
		onHoverExitAction = delegate
		{
			OnPointerExit(obj);
		};
		onLeftClickAction = delegate
		{
			OnPointerLeftClick(obj);
		};
		onRightClickAction = delegate
		{
			OnPointerRightClick(obj);
		};
		onMiddleClickAction = delegate
		{
			OnPointerMiddleClick(obj);
		};
	}

	public abstract void UpdateTileObjectVisual(T obj);

	public virtual void UpdateSortingOrders(T obj)
	{
		if (objectVisual != null)
		{
			objectVisual.sortingLayerName = "Area Maps";
			objectVisual.sortingOrder = 40;
		}
	}

	protected virtual void OnPointerEnter(T character)
	{
	}

	protected virtual void OnPointerExit(T poi)
	{
	}

	protected virtual void OnPointerLeftClick(T poi)
	{
	}

	protected virtual void OnPointerRightClick(T poi)
	{
	}

	protected virtual void OnPointerMiddleClick(T poi)
	{
	}

	public bool IsNear(Vector3 pos)
	{
		return Vector3.Distance(base.transform.position, pos) <= 0.75f;
	}

	public override void Reset()
	{
		base.Reset();
		obj = default(T);
		onHoverOverAction = null;
		onHoverExitAction = null;
		onLeftClickAction = null;
		onRightClickAction = null;
		onMiddleClickAction = null;
	}
}

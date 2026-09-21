using System.Collections.Generic;
using Ruinarch;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IDragHandler, IEventSystemHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField]
	protected bool _isDraggable;

	[SerializeField]
	protected bool _isDragging;

	protected Vector2 _draggingObjectOriginalSize;

	protected RectTransform _draggingObject;

	protected object associatedObj;

	public virtual bool isDraggable => _isDraggable;

	private void Awake()
	{
		SetDraggable(state: true);
	}

	public virtual void SetAssociatedObject(object obj)
	{
		associatedObj = obj;
	}

	public virtual void OnBeginDrag(PointerEventData eventData)
	{
		if (isDraggable)
		{
			InputManager.Instance.SetCursorTo(Cursor_Type.Drag_Clicked);
		}
	}

	public virtual void OnDrag(PointerEventData eventData)
	{
		if (_isDragging)
		{
			Canvas componentInParent = _draggingObject.GetComponentInParent<Canvas>();
			RectTransformUtility.ScreenPointToWorldPointInRectangle(componentInParent.GetComponent<RectTransform>(), eventData.position, componentInParent.worldCamera, out var worldPoint);
			_draggingObject.position = worldPoint;
		}
	}

	public virtual void OnEndDrag(PointerEventData eventData)
	{
		_isDragging = false;
		InputManager.Instance.SetCursorTo(Cursor_Type.Drag_Hover);
		if (!(_draggingObject != null))
		{
			return;
		}
		List<RaycastResult> list = new List<RaycastResult>();
		CustomDropZone customDropZone = null;
		EventSystem.current.RaycastAll(eventData, list);
		for (int i = 0; i < list.Count; i++)
		{
			customDropZone = list[i].gameObject.GetComponent<CustomDropZone>();
			if (customDropZone != null)
			{
				break;
			}
		}
		if (customDropZone != null)
		{
			customDropZone.OnDrop(_draggingObject.gameObject);
			Object.Destroy(_draggingObject.gameObject);
		}
		else
		{
			CancelDrag();
		}
	}

	public virtual void CancelDrag()
	{
		_isDragging = false;
		Object.Destroy(_draggingObject.gameObject);
	}

	public virtual void SetDraggable(bool state)
	{
		_isDraggable = state;
	}

	public virtual void OnPointerEnter(PointerEventData eventData)
	{
		if (isDraggable && !InputManager.Instance.isDraggingItem)
		{
			InputManager.Instance.SetCursorTo(Cursor_Type.Drag_Hover);
		}
	}

	public virtual void OnPointerExit(PointerEventData eventData)
	{
		if (isDraggable && !InputManager.Instance.isDraggingItem)
		{
			InputManager.Instance.SetCursorTo(Cursor_Type.Default);
		}
	}
}

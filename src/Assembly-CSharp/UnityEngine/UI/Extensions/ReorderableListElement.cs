using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions;

[RequireComponent(typeof(RectTransform))]
public class ReorderableListElement : MonoBehaviour, IDragHandler, IEventSystemHandler, IBeginDragHandler, IEndDragHandler
{
	[Tooltip("Can this element be dragged?")]
	public bool IsGrabbable = true;

	[Tooltip("Can this element be transfered to another list")]
	public bool IsTransferable = true;

	[Tooltip("Can this element be dropped in space?")]
	public bool isDroppableInSpace;

	public bool isDraggable = true;

	private readonly List<RaycastResult> _raycastResults = new List<RaycastResult>();

	private ReorderableList _currentReorderableListRaycasted;

	private RectTransform _draggingObject;

	private LayoutElement _draggingObjectLE;

	private Vector2 _draggingObjectOriginalSize;

	private RectTransform _fakeElement;

	private LayoutElement _fakeElementLE;

	private int _fromIndex;

	private bool _isDragging;

	private RectTransform _rect;

	private ReorderableList _reorderableList;

	internal bool isValid;

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (!isDraggable)
		{
			return;
		}
		isValid = true;
		if (_reorderableList == null)
		{
			return;
		}
		if (!_reorderableList.IsDraggable || !IsGrabbable)
		{
			_draggingObject = null;
			return;
		}
		if (!_reorderableList.CloneDraggedObject)
		{
			_draggingObject = _rect;
			_fromIndex = _rect.GetSiblingIndex();
			if (_reorderableList.OnElementRemoved != null)
			{
				_reorderableList.OnElementRemoved.Invoke(new ReorderableList.ReorderableListEventStruct
				{
					DroppedObject = _draggingObject.gameObject,
					IsAClone = _reorderableList.CloneDraggedObject,
					SourceObject = (_reorderableList.CloneDraggedObject ? base.gameObject : _draggingObject.gameObject),
					FromList = _reorderableList,
					FromIndex = _fromIndex
				});
			}
			if (!isValid)
			{
				_draggingObject = null;
				return;
			}
		}
		else
		{
			GameObject gameObject = Object.Instantiate(base.gameObject);
			_draggingObject = gameObject.GetComponent<RectTransform>();
		}
		_draggingObjectOriginalSize = base.gameObject.GetComponent<RectTransform>().rect.size;
		_draggingObjectLE = _draggingObject.GetComponent<LayoutElement>();
		_draggingObject.SetParent(_reorderableList.DraggableArea, worldPositionStays: true);
		_draggingObject.SetAsLastSibling();
		_fakeElement = new GameObject("Fake").AddComponent<RectTransform>();
		_fakeElementLE = _fakeElement.gameObject.AddComponent<LayoutElement>();
		RefreshSizes();
		if (_reorderableList.OnElementGrabbed != null)
		{
			_reorderableList.OnElementGrabbed.Invoke(new ReorderableList.ReorderableListEventStruct
			{
				DroppedObject = _draggingObject.gameObject,
				IsAClone = _reorderableList.CloneDraggedObject,
				SourceObject = (_reorderableList.CloneDraggedObject ? base.gameObject : _draggingObject.gameObject),
				FromList = _reorderableList,
				FromIndex = _fromIndex
			});
			if (!isValid)
			{
				CancelDrag();
				return;
			}
		}
		_isDragging = true;
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (!_isDragging)
		{
			return;
		}
		if (!isValid)
		{
			CancelDrag();
			return;
		}
		Canvas componentInParent = _draggingObject.GetComponentInParent<Canvas>();
		RectTransformUtility.ScreenPointToWorldPointInRectangle(componentInParent.GetComponent<RectTransform>(), eventData.position, componentInParent.worldCamera, out var worldPoint);
		_draggingObject.position = worldPoint;
		EventSystem.current.RaycastAll(eventData, _raycastResults);
		for (int i = 0; i < _raycastResults.Count; i++)
		{
			_currentReorderableListRaycasted = _raycastResults[i].gameObject.GetComponent<ReorderableList>();
			if (_currentReorderableListRaycasted != null)
			{
				break;
			}
		}
		if (_currentReorderableListRaycasted == null || !_currentReorderableListRaycasted.IsDropable)
		{
			RefreshSizes();
			_fakeElement.transform.SetParent(_reorderableList.DraggableArea, worldPositionStays: false);
			return;
		}
		if (_fakeElement.parent != _currentReorderableListRaycasted)
		{
			_fakeElement.SetParent(_currentReorderableListRaycasted.Content, worldPositionStays: false);
		}
		float num = float.PositiveInfinity;
		int siblingIndex = 0;
		float num2 = 0f;
		for (int j = 0; j < _currentReorderableListRaycasted.Content.childCount; j++)
		{
			RectTransform component = _currentReorderableListRaycasted.Content.GetChild(j).GetComponent<RectTransform>();
			if (_currentReorderableListRaycasted.ContentLayout is VerticalLayoutGroup)
			{
				num2 = Mathf.Abs(component.position.y - worldPoint.y);
			}
			else if (_currentReorderableListRaycasted.ContentLayout is HorizontalLayoutGroup)
			{
				num2 = Mathf.Abs(component.position.x - worldPoint.x);
			}
			else if (_currentReorderableListRaycasted.ContentLayout is GridLayoutGroup)
			{
				num2 = Mathf.Abs(component.position.x - worldPoint.x) + Mathf.Abs(component.position.y - worldPoint.y);
			}
			if (num2 < num)
			{
				num = num2;
				siblingIndex = j;
			}
		}
		RefreshSizes();
		_fakeElement.SetSiblingIndex(siblingIndex);
		_fakeElement.gameObject.SetActive(value: true);
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		_isDragging = false;
		if (_draggingObject != null)
		{
			if (_currentReorderableListRaycasted != null && _currentReorderableListRaycasted.IsDropable && (IsTransferable || _currentReorderableListRaycasted == _reorderableList))
			{
				ReorderableList.ReorderableListEventStruct arg = new ReorderableList.ReorderableListEventStruct
				{
					DroppedObject = _draggingObject.gameObject,
					IsAClone = _reorderableList.CloneDraggedObject,
					SourceObject = (_reorderableList.CloneDraggedObject ? base.gameObject : _draggingObject.gameObject),
					FromList = _reorderableList,
					FromIndex = _fromIndex,
					ToList = _currentReorderableListRaycasted,
					ToIndex = _fakeElement.GetSiblingIndex()
				};
				if ((bool)_reorderableList && _reorderableList.OnElementDropped != null)
				{
					_reorderableList.OnElementDropped.Invoke(arg);
				}
				if (!isValid)
				{
					CancelDrag();
					return;
				}
				RefreshSizes();
				_draggingObject.SetParent(_currentReorderableListRaycasted.Content, worldPositionStays: false);
				_draggingObject.rotation = _currentReorderableListRaycasted.transform.rotation;
				_draggingObject.SetSiblingIndex(_fakeElement.GetSiblingIndex());
				_reorderableList.OnElementAdded.Invoke(arg);
				if (!isValid)
				{
					throw new Exception("It's too late to cancel the Transfer! Do so in OnElementDropped!");
				}
			}
			else if (isDroppableInSpace)
			{
				_reorderableList.OnElementDropped.Invoke(new ReorderableList.ReorderableListEventStruct
				{
					DroppedObject = _draggingObject.gameObject,
					IsAClone = _reorderableList.CloneDraggedObject,
					SourceObject = (_reorderableList.CloneDraggedObject ? base.gameObject : _draggingObject.gameObject),
					FromList = _reorderableList,
					FromIndex = _fromIndex
				});
			}
			else
			{
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
					if (_reorderableList.CloneDraggedObject)
					{
						Object.Destroy(_draggingObject.gameObject);
					}
					ReorderableList.ReorderableListEventStruct arg2 = new ReorderableList.ReorderableListEventStruct
					{
						DroppedObject = _draggingObject.gameObject,
						IsAClone = _reorderableList.CloneDraggedObject,
						SourceObject = (_reorderableList.CloneDraggedObject ? base.gameObject : _draggingObject.gameObject),
						FromList = _reorderableList,
						FromIndex = _fromIndex,
						ToList = _currentReorderableListRaycasted,
						ToIndex = _fakeElement.GetSiblingIndex()
					};
					if ((bool)_reorderableList && _reorderableList.OnElementDropped != null)
					{
						_reorderableList.OnElementDropped.Invoke(arg2);
					}
				}
				else
				{
					CancelDrag();
				}
			}
		}
		if (_fakeElement != null)
		{
			Object.Destroy(_fakeElement.gameObject);
		}
	}

	private void CancelDrag()
	{
		_isDragging = false;
		if (_reorderableList.CloneDraggedObject)
		{
			Object.Destroy(_draggingObject.gameObject);
		}
		else
		{
			RefreshSizes();
			_draggingObject.SetParent(_reorderableList.Content, worldPositionStays: false);
			_draggingObject.rotation = _reorderableList.Content.transform.rotation;
			_draggingObject.SetSiblingIndex(_fromIndex);
			ReorderableList.ReorderableListEventStruct arg = new ReorderableList.ReorderableListEventStruct
			{
				DroppedObject = _draggingObject.gameObject,
				IsAClone = _reorderableList.CloneDraggedObject,
				SourceObject = (_reorderableList.CloneDraggedObject ? base.gameObject : _draggingObject.gameObject),
				FromList = _reorderableList,
				FromIndex = _fromIndex,
				ToList = _reorderableList,
				ToIndex = _fromIndex
			};
			_reorderableList.OnElementAdded.Invoke(arg);
			if (!isValid)
			{
				throw new Exception("Transfer is already Cancelled.");
			}
		}
		if (_fakeElement != null)
		{
			Object.Destroy(_fakeElement.gameObject);
		}
		if (_reorderableList.OnCancelDrag != null)
		{
			_reorderableList.OnCancelDrag.Invoke(new ReorderableList.ReorderableListEventStruct
			{
				DroppedObject = _draggingObject.gameObject,
				IsAClone = _reorderableList.CloneDraggedObject,
				SourceObject = (_reorderableList.CloneDraggedObject ? base.gameObject : _draggingObject.gameObject),
				FromList = _reorderableList,
				FromIndex = _fromIndex
			});
		}
	}

	private void RefreshSizes()
	{
		Vector2 sizeDelta = _draggingObjectOriginalSize;
		if (_currentReorderableListRaycasted != null && _currentReorderableListRaycasted.IsDropable && _currentReorderableListRaycasted.Content.childCount > 0)
		{
			Transform child = _currentReorderableListRaycasted.Content.GetChild(0);
			if (child != null)
			{
				sizeDelta = child.GetComponent<RectTransform>().rect.size;
			}
		}
		_draggingObject.sizeDelta = sizeDelta;
		LayoutElement fakeElementLE = _fakeElementLE;
		float preferredHeight = (_draggingObjectLE.preferredHeight = sizeDelta.y);
		fakeElementLE.preferredHeight = preferredHeight;
		LayoutElement fakeElementLE2 = _fakeElementLE;
		preferredHeight = (_draggingObjectLE.preferredWidth = sizeDelta.x);
		fakeElementLE2.preferredWidth = preferredHeight;
	}

	public void Init(ReorderableList reorderableList)
	{
		_reorderableList = reorderableList;
		_rect = GetComponent<RectTransform>();
	}
}

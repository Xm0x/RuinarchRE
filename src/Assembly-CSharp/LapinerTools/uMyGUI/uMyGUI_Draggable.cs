using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LapinerTools.uMyGUI;

public class uMyGUI_Draggable : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler
{
	public class DragEvent : EventArgs
	{
		public readonly PointerEventData m_event;

		public DragEvent(PointerEventData p_event)
		{
			m_event = p_event;
		}
	}

	[SerializeField]
	private bool m_isResetRotationWhenDragged;

	[SerializeField]
	private bool m_isSnapBackOnEndDrag;

	[SerializeField]
	private bool m_isTopInHierarchyWhenDragged = true;

	[SerializeField]
	private CanvasGroup m_disableBlocksRaycastsOnDrag;

	private bool m_isDragged;

	public EventHandler<DragEvent> m_onBeginDrag;

	public EventHandler<DragEvent> m_onDrag;

	public EventHandler<DragEvent> m_onEndDrag;

	private RectTransform m_initialParentTransform;

	private RectTransform m_canvasTransform;

	private RectTransform m_transform;

	private int m_initialSiblingIndex;

	private Vector3 m_initialPosition;

	private Quaternion m_initialRotation;

	private Vector3 m_dragOffset = Vector3.zero;

	public bool IsResetRotationWhenDragged
	{
		get
		{
			return m_isResetRotationWhenDragged;
		}
		set
		{
			m_isResetRotationWhenDragged = value;
		}
	}

	public bool IsSnapBackOnEndDrag
	{
		get
		{
			return m_isSnapBackOnEndDrag;
		}
		set
		{
			m_isSnapBackOnEndDrag = value;
		}
	}

	public bool IsTopInHierarchyWhenDragged
	{
		get
		{
			return m_isTopInHierarchyWhenDragged;
		}
		set
		{
			m_isTopInHierarchyWhenDragged = value;
		}
	}

	public CanvasGroup DisableBlocksRaycastsOnDrag
	{
		get
		{
			return m_disableBlocksRaycastsOnDrag;
		}
		set
		{
			m_disableBlocksRaycastsOnDrag = value;
		}
	}

	public bool IsDragged => m_isDragged;

	public void OnBeginDrag(PointerEventData p_event)
	{
		m_isDragged = true;
		m_initialParentTransform = m_transform.parent as RectTransform;
		m_initialPosition = m_transform.position;
		m_initialRotation = m_transform.rotation;
		if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_canvasTransform, p_event.position, p_event.pressEventCamera, out var worldPoint))
		{
			m_dragOffset = m_transform.position - worldPoint;
		}
		else
		{
			m_dragOffset = Vector3.zero;
		}
		if (m_isResetRotationWhenDragged)
		{
			m_transform.rotation = Quaternion.identity;
		}
		if (m_isTopInHierarchyWhenDragged)
		{
			m_initialSiblingIndex = m_transform.GetSiblingIndex();
			m_transform.SetParent(m_canvasTransform, worldPositionStays: true);
			m_transform.SetAsLastSibling();
		}
		if (m_disableBlocksRaycastsOnDrag != null)
		{
			m_disableBlocksRaycastsOnDrag.blocksRaycasts = false;
		}
		if (m_onBeginDrag != null)
		{
			m_onBeginDrag(this, new DragEvent(p_event));
		}
	}

	public void OnDrag(PointerEventData p_event)
	{
		if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_canvasTransform, p_event.position, p_event.pressEventCamera, out var worldPoint))
		{
			m_transform.position = worldPoint + m_dragOffset;
		}
		if (m_onDrag != null)
		{
			m_onDrag(this, new DragEvent(p_event));
		}
	}

	public void OnEndDrag(PointerEventData p_event)
	{
		if (m_isDragged)
		{
			m_isDragged = false;
			if (m_isSnapBackOnEndDrag)
			{
				m_transform.position = m_initialPosition;
				m_transform.rotation = m_initialRotation;
			}
			if (m_isTopInHierarchyWhenDragged)
			{
				m_transform.SetParent(m_initialParentTransform, worldPositionStays: true);
				m_transform.SetSiblingIndex(m_initialSiblingIndex);
			}
			if (m_disableBlocksRaycastsOnDrag != null)
			{
				m_disableBlocksRaycastsOnDrag.blocksRaycasts = true;
			}
			if (m_onEndDrag != null)
			{
				m_onEndDrag(this, new DragEvent(p_event));
			}
		}
	}

	private void Start()
	{
		Canvas componentInParent = GetComponentInParent<Canvas>();
		if (componentInParent != null)
		{
			m_canvasTransform = componentInParent.GetComponent<RectTransform>();
		}
		else
		{
			Debug.LogError("uMyGUI_Draggable: no Canvas component was found in parent!");
			base.enabled = false;
		}
		m_transform = GetComponent<RectTransform>();
	}
}

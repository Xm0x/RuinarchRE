using System.Collections.Generic;
using Ruinarch;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class PanelDragBehaviour : MonoBehaviour, IDragHandler, IEventSystemHandler, IBeginDragHandler
{
	private RectTransform rect;

	[SerializeField]
	private RectTransform canvasRectTransform;

	private bool allowDrag;

	private Vector3 difference;

	public void Awake()
	{
		rect = GetComponent<RectTransform>();
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		List<RaycastResult> list = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventData, list);
		allowDrag = false;
		if (list.Count <= 0)
		{
			return;
		}
		foreach (RaycastResult item in list)
		{
			if (item.gameObject.GetComponent<PanelDragBehaviour>() != null)
			{
				allowDrag = true;
				difference = rect.position - new Vector3(eventData.position.x, eventData.position.y, 0f);
				break;
			}
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (allowDrag)
		{
			base.transform.position = InputManager.Instance.mousePosition + difference;
		}
	}
}

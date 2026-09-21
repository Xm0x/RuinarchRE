using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
{
	public Transform dropTransform;

	public DropEvent onItemDropped;

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!(eventData.pointerDrag == null))
		{
			Draggable component = eventData.pointerDrag.GetComponent<Draggable>();
			if (component != null)
			{
				component.placeHolderParent = base.transform;
			}
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (!(eventData.pointerDrag == null))
		{
			Draggable component = eventData.pointerDrag.GetComponent<Draggable>();
			if (component != null && component.placeHolderParent == base.transform)
			{
				component.placeHolderParent = component.parentToReturnTo;
			}
		}
	}

	public void OnDrop(PointerEventData eventData)
	{
		Draggable component = eventData.pointerDrag.GetComponent<Draggable>();
		if (component != null)
		{
			if (dropTransform == null)
			{
				component.parentToReturnTo = base.transform;
			}
			else
			{
				component.parentToReturnTo = dropTransform;
			}
		}
	}
}

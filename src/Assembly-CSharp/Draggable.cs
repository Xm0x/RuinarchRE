using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Draggable : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler
{
	[HideInInspector]
	public Transform parentToReturnTo;

	[HideInInspector]
	public Transform placeHolderParent;

	public Transform parentWhileDragging;

	private GameObject placeHolder;

	public void OnBeginDrag(PointerEventData eventData)
	{
		placeHolder = new GameObject();
		placeHolder.transform.SetParent(base.transform.parent);
		LayoutElement layoutElement = placeHolder.AddComponent<LayoutElement>();
		layoutElement.preferredWidth = GetComponent<LayoutElement>().preferredWidth;
		layoutElement.preferredHeight = GetComponent<LayoutElement>().preferredHeight;
		layoutElement.flexibleWidth = 0f;
		layoutElement.flexibleHeight = 0f;
		placeHolder.transform.SetSiblingIndex(base.transform.GetSiblingIndex());
		parentToReturnTo = base.transform.parent;
		placeHolderParent = parentToReturnTo;
		base.transform.SetParent(parentWhileDragging);
		GetComponent<CanvasGroup>().blocksRaycasts = false;
	}

	public void OnDrag(PointerEventData eventData)
	{
		base.transform.position = eventData.position;
		if (placeHolder.transform.parent != placeHolderParent)
		{
			placeHolder.transform.SetParent(placeHolderParent);
		}
		int num = placeHolderParent.childCount;
		for (int i = 0; i < placeHolderParent.childCount; i++)
		{
			if (base.transform.position.x < placeHolderParent.GetChild(i).position.x)
			{
				num = i;
				if (placeHolder.transform.GetSiblingIndex() < num)
				{
					num--;
				}
				break;
			}
		}
		placeHolder.transform.SetSiblingIndex(num);
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		base.transform.SetParent(parentToReturnTo);
		base.transform.SetSiblingIndex(placeHolder.transform.GetSiblingIndex());
		GetComponent<CanvasGroup>().blocksRaycasts = true;
		if (parentToReturnTo.GetComponent<LayoutGroup>() == null)
		{
			base.transform.localPosition = Vector3.zero;
		}
		DropZone component = parentToReturnTo.GetComponent<DropZone>();
		if (component != null)
		{
			component.onItemDropped.Invoke(base.transform);
		}
		Object.Destroy(placeHolder);
	}
}

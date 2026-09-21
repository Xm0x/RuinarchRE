using UnityEngine.EventSystems;

public class ActionOptionHoverHandler : HoverHandler
{
	public override void OnPointerEnter(PointerEventData eventData)
	{
		if (selectable != null)
		{
			isHovering = true;
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		if (selectable != null)
		{
			isHovering = false;
			if (onHoverExitAction != null)
			{
				onHoverExitAction.Invoke();
			}
		}
	}
}

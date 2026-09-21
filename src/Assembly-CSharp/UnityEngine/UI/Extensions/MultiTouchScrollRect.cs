using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions;

[AddComponentMenu("UI/Extensions/MultiTouchScrollRect")]
public class MultiTouchScrollRect : ScrollRect
{
	private int pid = -100;

	public override void OnBeginDrag(PointerEventData eventData)
	{
		pid = eventData.pointerId;
		base.OnBeginDrag(eventData);
	}

	public override void OnDrag(PointerEventData eventData)
	{
		if (pid == eventData.pointerId)
		{
			base.OnDrag(eventData);
		}
	}

	public override void OnEndDrag(PointerEventData eventData)
	{
		pid = -100;
		base.OnEndDrag(eventData);
	}
}

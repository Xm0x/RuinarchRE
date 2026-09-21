using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions.Examples;

public class PaginationScript : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public HorizontalScrollSnap hss;

	public int Page;

	public void OnPointerClick(PointerEventData eventData)
	{
		if (hss != null)
		{
			hss.GoToScreen(Page);
		}
	}
}

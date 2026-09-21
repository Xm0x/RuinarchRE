using UnityEngine;
using UnityEngine.EventSystems;

public class NameplateButton : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public INameplateItem nameplateItem;

	public void SetNameplateItem(INameplateItem nameplateItem)
	{
		this.nameplateItem = nameplateItem;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		nameplateItem.OnPointerClick(eventData);
	}
}

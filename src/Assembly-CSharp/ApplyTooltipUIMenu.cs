using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class ApplyTooltipUIMenu : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[FormerlySerializedAs("uiParent")]
	public InfoUIBase infoUiParent;

	public GameObject objectToCheck;

	private bool isHovering;

	public void OnPointerEnter(PointerEventData eventData)
	{
		isHovering = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		isHovering = false;
		UIManager.Instance.HideSmallInfo();
	}

	private void Update()
	{
		if (isHovering)
		{
			infoUiParent.ShowTooltip(objectToCheck);
		}
	}
}

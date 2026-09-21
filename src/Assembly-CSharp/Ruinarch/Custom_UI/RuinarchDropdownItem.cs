using TMPro;
using UnityEngine;

namespace Ruinarch.Custom_UI;

[RequireComponent(typeof(HoverHandler))]
public class RuinarchDropdownItem : MonoBehaviour
{
	[SerializeField]
	private HoverHandler _hoverHandler;

	[SerializeField]
	private TMP_Dropdown _parentDropdown;

	private void Awake()
	{
		_hoverHandler.AddOnHoverOverAction(OnHoverOver);
		_hoverHandler.AddOnHoverOutAction(OnHoverOut);
	}

	private void OnDestroy()
	{
		_hoverHandler.RemoveOnHoverOverAction(OnHoverOver);
		_hoverHandler.RemoveOnHoverOutAction(OnHoverOut);
	}

	private void OnHoverOver()
	{
		int num = base.transform.GetSiblingIndex() - 1;
		if (num != -1)
		{
			Messenger.Broadcast(UISignals.DROPDOWN_ITEM_HOVERED_OVER, _parentDropdown, num);
		}
	}

	private void OnHoverOut()
	{
		int num = base.transform.GetSiblingIndex() - 1;
		if (num != -1)
		{
			Messenger.Broadcast(UISignals.DROPDOWN_ITEM_HOVERED_OUT, _parentDropdown, num);
		}
	}
}

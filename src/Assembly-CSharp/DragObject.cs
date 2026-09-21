using UnityEngine;

public class DragObject : MonoBehaviour
{
	private IDragParentItem _parentItem;

	public IDragParentItem parentItem
	{
		get
		{
			return _parentItem;
		}
		set
		{
			SetParentItem(value);
		}
	}

	private void SetParentItem(IDragParentItem parentItem)
	{
		_parentItem = parentItem;
		Messenger.Broadcast(UISignals.DRAG_OBJECT_CREATED, this);
	}

	public void OnDestroy()
	{
		Messenger.Broadcast(UISignals.DRAG_OBJECT_DESTROYED, this);
	}
}

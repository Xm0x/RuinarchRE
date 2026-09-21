using UnityEngine;
using UnityEngine.Events;

public class CustomDropZone : MonoBehaviour
{
	public CustomDropEvent onDropItem;

	public bool isEnabled = true;

	[Space(10f)]
	[Header("On Enable/Disable")]
	[SerializeField]
	private UnityEvent onEnableSlotAction;

	[SerializeField]
	private UnityEvent onDisableSlotAction;

	public GameObject droppedItem { get; private set; }

	public void OnDrop(GameObject go)
	{
		if (isEnabled && go != null)
		{
			droppedItem = go;
			if (onDropItem != null)
			{
				onDropItem.Invoke(go);
			}
		}
	}

	public void SetEnabledState(bool state)
	{
		isEnabled = state;
		if (isEnabled)
		{
			if (onEnableSlotAction != null)
			{
				onEnableSlotAction.Invoke();
			}
		}
		else if (onDisableSlotAction != null)
		{
			onDisableSlotAction.Invoke();
		}
	}
}

using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class RuinarchScrollbar : Scrollbar
{
	private bool _isPressing;

	private UnityEvent _pointerUpEvent = new UnityEvent();

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_pointerUpEvent.RemoveAllListeners();
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		base.OnPointerDown(eventData);
		_isPressing = true;
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		base.OnPointerUp(eventData);
		_isPressing = false;
		_pointerUpEvent.Invoke();
	}

	public bool IsPressing()
	{
		return _isPressing;
	}

	public void AddPointerUpEvent(UnityAction p_action)
	{
		_pointerUpEvent.AddListener(p_action);
	}
}

using UnityEngine;

public abstract class PopupMenuBase : MonoBehaviour
{
	[SerializeField]
	private GameObject goWindow;

	public bool isShowing { get; private set; }

	protected virtual void OnGameObjectEnabled()
	{
		isShowing = true;
		Messenger.Broadcast(UISignals.POPUP_MENU_OPENED, this);
	}

	protected virtual void OnGameObjectDisabled()
	{
		isShowing = false;
		Messenger.Broadcast(UISignals.POPUP_MENU_CLOSED, this);
	}

	public virtual void Open()
	{
		OnGameObjectEnabled();
		goWindow.SetActive(value: true);
	}

	public virtual void Close()
	{
		OnGameObjectDisabled();
		goWindow.SetActive(value: false);
	}
}

using UnityEngine;

public class SWBLoadingPopup : MonoBehaviour
{
	public static SWBLoadingPopup Instance;

	[SerializeField]
	private GameObject _windowGO;

	private void Awake()
	{
		Instance = this;
	}

	public void Show()
	{
		_windowGO.SetActive(value: true);
	}

	public void Hide()
	{
		_windowGO.SetActive(value: false);
	}
}

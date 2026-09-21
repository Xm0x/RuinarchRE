using UnityEngine;

public class ModParentUI : MonoBehaviour
{
	[SerializeField]
	private GameObject _windowGO;

	[SerializeField]
	private GameObject _coverGO;

	[Header("Sub UI")]
	[SerializeField]
	private SubscribedModsUI _subscribeModsUI;

	[SerializeField]
	private SWBParentUI _browseModUI;

	[SerializeField]
	private CreateModUI _createModUI;

	public void Show()
	{
		_windowGO.SetActive(value: true);
		_coverGO.SetActive(value: true);
		_subscribeModsUI.OnOpenParentModUI();
		_browseModUI.OnOpenParentModUI();
		_createModUI.OnOpenParentModUI();
	}

	public void Hide()
	{
		_windowGO.SetActive(value: false);
		_coverGO.SetActive(value: false);
	}
}

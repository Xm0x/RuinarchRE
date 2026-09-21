using Ruinarch.MVCFramework;
using UnityEngine;

public class SidebarUIController : MVCUIController, SidebarUIView.IListener
{
	[SerializeField]
	private SidebarUIModel m_sidebarUIModel;

	private SidebarUIView m_sidebarUIView;

	public BookmarkUIController bookmarkUIController;

	public MinimapUIController minimapUIController;

	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		SidebarUIView.Create(_canvas, m_sidebarUIModel, delegate(SidebarUIView p_ui)
		{
			m_sidebarUIView = p_ui;
			m_sidebarUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
			p_ui.UIModel.transform.SetSiblingIndex(siblingIndex);
			bookmarkUIController.InstantiateUI();
			minimapUIController.InstantiateUI();
			bookmarkUIController.OnClickHide();
			minimapUIController.HideUI();
		});
	}

	private void Awake()
	{
		InstantiateUI();
		Messenger.AddListener(Signals.GAME_STARTED, OnGameLoaded);
		Messenger.AddListener(Signals.PROGRESSION_LOADED, OnGameLoaded);
	}

	private void OnDestroy()
	{
		m_sidebarUIView?.Unsubscribe(this);
	}

	private void OnGameLoaded()
	{
		Messenger.RemoveListener(Signals.PROGRESSION_LOADED, OnGameLoaded);
		Messenger.RemoveListener(Signals.GAME_STARTED, OnGameLoaded);
		m_sidebarUIView.UIModel.tglBookmarks.isOn = true;
	}

	public void OnToggleBookmark(bool p_isOn)
	{
		if (p_isOn)
		{
			bookmarkUIController.OnClickShow();
			AkSoundEngine.PostEvent("Play_Turn_Page", AudioManager.Instance.gameObject);
		}
		else
		{
			bookmarkUIController.OnClickHide();
		}
	}

	public void OnToggleMinimap(bool p_isOn)
	{
		if (p_isOn)
		{
			minimapUIController.ShowUI();
			AkSoundEngine.PostEvent("Play_Turn_Page", AudioManager.Instance.gameObject);
		}
		else
		{
			minimapUIController.HideUI();
		}
	}

	public void OnHoverOverBookmark()
	{
		UIManager.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Bookmarks"), m_sidebarUIView.UIModel.tooltipHoverPos);
	}

	public void OnHoverOutBookmark()
	{
		UIManager.Instance.HideSmallInfo();
	}

	public void OnHoverOverMinimap()
	{
		UIManager.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Minimap"), m_sidebarUIView.UIModel.tooltipHoverPos);
	}

	public void OnHoverOutMinimap()
	{
		UIManager.Instance.HideSmallInfo();
	}

	public override void HideUI()
	{
		base.HideUI();
		minimapUIController.HideUI();
		bookmarkUIController.OnClickHide();
	}

	public override void ShowUI()
	{
		base.ShowUI();
		if (m_sidebarUIView.UIModel.tglBookmarks.isOn)
		{
			bookmarkUIController.OnClickShow();
		}
		if (m_sidebarUIView.UIModel.tglMinimap.isOn)
		{
			minimapUIController.ShowUI();
		}
	}
}
